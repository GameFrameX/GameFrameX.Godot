#define FORCE_ENABLE_GAME_FRAME_X_WEB_SOCKET
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using GameFrameX.Network.Runtime;
using GameFrameX.Runtime;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// WebSocket 网络频道测试：通道注册逻辑与文本帧收发路径，全部经由 IWebSocket 桩驱动（真实 Peer 引擎外构造会段错误）。
    /// </summary>
    public sealed class WebSocketNetworkChannelTests
    {
        private sealed class StubChannelHelper : INetworkChannelHelper
        {
            public INetworkChannel InitializedChannel { get; private set; }
            public bool ShutdownCalled { get; private set; }

            public void Initialize(INetworkChannel networkChannel)
            {
                InitializedChannel = networkChannel;
            }

            public void Shutdown()
            {
                ShutdownCalled = true;
            }

            public void PrepareForConnecting()
            {
            }

            public bool SendHeartBeat()
            {
                return false;
            }

            public bool SerializePacketHeader<T>(T messageObject, MemoryStream destination, out byte[] messageBodyBuffer) where T : MessageObject
            {
                messageBodyBuffer = new byte[0];
                return true;
            }

            public bool SerializePacketBody(byte[] messageBodyBuffer, MemoryStream destination)
            {
                return true;
            }

            public bool DeserializePacketHeader(byte[] source)
            {
                return false;
            }

            public bool DeserializePacketBody(byte[] source, int messageId, out MessageObject messageObject)
            {
                messageObject = null;
                return false;
            }
        }

        private sealed class StubWebSocket : IWebSocket
        {
            private readonly Queue<byte[]> _packets = new Queue<byte[]>();
            private readonly Queue<bool> _packetIsText = new Queue<bool>();

            public WebSocketReadyState ReadyState { get; set; }
            public int InboundBufferSize { get; set; }
            public int OutboundBufferSize { get; set; }
            public string LastSentText { get; private set; }
            public int CloseCode { get; set; }
            public string CloseReason { get; set; }

            private bool _lastWasText;

            public void EnqueuePacket(byte[] packet, bool isText)
            {
                _packets.Enqueue(packet);
                _packetIsText.Enqueue(isText);
            }

            public int ConnectToUrl(string url)
            {
                return 0;
            }

            public void Poll()
            {
            }

            public int GetAvailablePacketCount()
            {
                return _packets.Count;
            }

            public byte[] GetPacket()
            {
                _lastWasText = _packetIsText.Dequeue();
                return _packets.Dequeue();
            }

            public bool WasStringPacket()
            {
                return _lastWasText;
            }

            public int Send(byte[] data)
            {
                return 0;
            }

            public int SendText(string text)
            {
                LastSentText = text;
                return 0;
            }

            public void Close(int code, string reason)
            {
            }

            public int GetCloseCode()
            {
                return CloseCode;
            }

            public string GetCloseReason()
            {
                return CloseReason;
            }
        }

        private static NetworkManager.WebSocketNetworkChannel CreateChannel(StubChannelHelper helper)
        {
            return new NetworkManager.WebSocketNetworkChannel("ws-channel", helper, 3000);
        }

        private static void InjectSocket(NetworkManager.WebSocketNetworkChannel channel, NetworkManager.WebSocketNetSocket socket)
        {
            var field = typeof(NetworkManager.NetworkChannelBase).GetField("PSocket", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            field.SetValue(channel, socket);
        }

        private static NetworkManager.WebSocketNetSocket CreateSocket(StubWebSocket stub, Action<string> onText)
        {
            return new NetworkManager.WebSocketNetSocket(
                stub,
                "ws://127.0.0.1:10000/test",
                buffer => { },
                onText,
                (reason, code) => { },
                (errorCode, message) => { });
        }

        [Fact]
        public void CreateNetworkChannel_RegistersWebSocketChannel()
        {
            var manager = new NetworkManager();
            var channel = manager.CreateNetworkChannel("ws", new StubChannelHelper(), 3000);

            Assert.IsType<NetworkManager.WebSocketNetworkChannel>(channel);
            Assert.True(manager.HasNetworkChannel("ws"));
            Assert.Equal(1, manager.NetworkChannelCount);
            Assert.Same(channel, manager.GetNetworkChannel("ws"));
        }

        [Fact]
        public void CreateNetworkChannel_DuplicateName_Throws()
        {
            var manager = new NetworkManager();
            manager.CreateNetworkChannel("ws", new StubChannelHelper(), 3000);
            Assert.Throws<GameFrameworkException>(() => manager.CreateNetworkChannel("ws", new StubChannelHelper(), 3000));
        }

        [Fact]
        public void DestroyNetworkChannel_RemovesChannelAndShutsDownHelper()
        {
            var manager = new NetworkManager();
            var helper = new StubChannelHelper();
            manager.CreateNetworkChannel("ws", helper, 3000);

            Assert.True(manager.DestroyNetworkChannel("ws"));
            Assert.False(manager.HasNetworkChannel("ws"));
            Assert.True(helper.ShutdownCalled);
        }

        [Fact]
        public void SendText_WithoutSocket_RaisesNetworkChannelError()
        {
            var helper = new StubChannelHelper();
            var channel = CreateChannel(helper);
            var errors = new List<string>();
            channel.NetworkChannelError += (c, errorCode, socketError, message) => errors.Add($"{errorCode}:{message}");

            channel.SendText("hello");

            Assert.Single(errors);
            Assert.StartsWith($"{NetworkErrorCode.SendError}:", errors[0]);
        }

        [Fact]
        public void SendText_EmptyText_RaisesNetworkChannelError()
        {
            var helper = new StubChannelHelper();
            var channel = CreateChannel(helper);
            var errors = new List<string>();
            channel.NetworkChannelError += (c, errorCode, socketError, message) => errors.Add($"{errorCode}:{message}");

            channel.SendText(string.Empty);

            Assert.Single(errors);
            Assert.StartsWith($"{NetworkErrorCode.SendError}:", errors[0]);
        }

        [Fact]
        public void SendText_WithoutErrorSubscriber_Throws()
        {
            var helper = new StubChannelHelper();
            var channel = CreateChannel(helper);

            Assert.Throws<GameFrameworkException>(() => channel.SendText("hello"));
        }

        [Fact]
        public void SendText_WhenSocketOpen_SendsTextFrameAndCountsPacket()
        {
            var helper = new StubChannelHelper();
            var channel = CreateChannel(helper);
            var stub = new StubWebSocket { ReadyState = WebSocketReadyState.Open };
            InjectSocket(channel, CreateSocket(stub, text => { }));

            channel.SendText("hello");

            Assert.Equal("hello", stub.LastSentText);
            Assert.Equal(1, channel.SentPacketCount);
        }

        [Fact]
        public void SendText_WhenSocketNotOpen_RaisesNetworkChannelError()
        {
            var helper = new StubChannelHelper();
            var channel = CreateChannel(helper);
            var stub = new StubWebSocket { ReadyState = WebSocketReadyState.Closed };
            InjectSocket(channel, CreateSocket(stub, text => { }));
            var errors = new List<string>();
            channel.NetworkChannelError += (c, errorCode, socketError, message) => errors.Add(message);

            channel.SendText("hello");

            Assert.Single(errors);
            Assert.Null(stub.LastSentText);
        }

        [Fact]
        public void Update_TextFrame_DeliversTextViaSocketCallback()
        {
            var helper = new StubChannelHelper();
            var channel = CreateChannel(helper);
            var received = new List<string>();
            var stub = new StubWebSocket { ReadyState = WebSocketReadyState.Open };
            stub.EnqueuePacket(System.Text.Encoding.UTF8.GetBytes("hello"), true);
            InjectSocket(channel, CreateSocket(stub, text => received.Add(text)));

            channel.Update(0f, 0f);

            // ponytail: 引擎内 Connect 才会把通道的 ReceiveTextCallback 装到 socket 上，
            // 引擎外仅能验证 Update -> Poll -> 文本回调链；ReceiveTextCallback 触发事件的部分由代码审查保证。
            Assert.Single(received);
            Assert.Equal("hello", received[0]);
        }

        [Fact]
        public void Constructor_InitializesHelperWithChannel()
        {
            var helper = new StubChannelHelper();
            var channel = CreateChannel(helper);
            Assert.Same(channel, helper.InitializedChannel);
        }
    }
}
