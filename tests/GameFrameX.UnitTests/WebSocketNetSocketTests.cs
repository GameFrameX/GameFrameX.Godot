#define FORCE_ENABLE_GAME_FRAME_X_WEB_SOCKET
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GameFrameX.Network.Runtime;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// WebSocket 套接字测试：通过 IWebSocket 桩驱动连接状态机与帧分发，不触碰 Godot native（真实 Peer 引擎外构造会段错误）。
    /// </summary>
    public sealed class WebSocketNetSocketTests
    {
        private sealed class StubWebSocket : IWebSocket
        {
            private readonly Queue<byte[]> _packets = new Queue<byte[]>();
            private readonly Queue<bool> _packetIsText = new Queue<bool>();

            public WebSocketReadyState ReadyState { get; set; }
            public int InboundBufferSize { get; set; }
            public int OutboundBufferSize { get; set; }
            public int ConnectToUrlResult { get; set; }
            public int SendResult { get; set; }
            public int SendTextResult { get; set; }
            public int CloseCode { get; set; }
            public string CloseReason { get; set; }
            public string ConnectUrlArg { get; private set; }
            public byte[] LastSentData { get; private set; }
            public string LastSentText { get; private set; }
            public int PollCount { get; private set; }
            public int? LastCloseCodeArg { get; private set; }

            private bool _lastWasText;

            public void EnqueuePacket(byte[] packet, bool isText)
            {
                _packets.Enqueue(packet);
                _packetIsText.Enqueue(isText);
            }

            public int ConnectToUrl(string url)
            {
                ConnectUrlArg = url;
                return ConnectToUrlResult;
            }

            public void Poll()
            {
                PollCount++;
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
                LastSentData = data;
                return SendResult;
            }

            public int SendText(string text)
            {
                LastSentText = text;
                return SendTextResult;
            }

            public void Close(int code, string reason)
            {
                LastCloseCodeArg = code;
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

        private sealed class SocketHooks
        {
            public List<byte[]> Received { get; } = new List<byte[]>();
            public List<string> ReceivedText { get; } = new List<string>();
            public List<Tuple<string, ushort>> Closed { get; } = new List<Tuple<string, ushort>>();
            public List<Tuple<NetworkErrorCode, string>> Errors { get; } = new List<Tuple<NetworkErrorCode, string>>();
        }

        private static NetworkManager.WebSocketNetSocket CreateSocket(StubWebSocket stub, SocketHooks hooks)
        {
            return new NetworkManager.WebSocketNetSocket(
                stub,
                "ws://127.0.0.1:10000/test",
                buffer => hooks.Received.Add(buffer),
                text => hooks.ReceivedText.Add(text),
                (reason, code) => hooks.Closed.Add(Tuple.Create(reason, code)),
                (errorCode, message) => hooks.Errors.Add(Tuple.Create(errorCode, message)));
        }

        [Fact]
        public void Constructor_NullClient_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => CreateSocket(null, new SocketHooks()));
        }

        [Fact]
        public async Task ConnectAsync_WhenPeerOpens_CompletesWithTrue()
        {
            var stub = new StubWebSocket { ReadyState = WebSocketReadyState.Connecting };
            var hooks = new SocketHooks();
            var socket = CreateSocket(stub, hooks);

            var task = socket.ConnectAsync();
            Assert.False(task.IsCompleted);
            Assert.Equal("ws://127.0.0.1:10000/test", stub.ConnectUrlArg);

            stub.ReadyState = WebSocketReadyState.Open;
            socket.Poll();
            await task;
            Assert.True(socket.IsConnected);
            Assert.Empty(hooks.Errors);
        }

        [Fact]
        public async Task ConnectAsync_WhenConnectRequestRejected_CompletesWithFalseAndError()
        {
            var stub = new StubWebSocket { ReadyState = WebSocketReadyState.Connecting, ConnectToUrlResult = 1 };
            var hooks = new SocketHooks();
            var socket = CreateSocket(stub, hooks);

            await socket.ConnectAsync();
            Assert.Single(hooks.Errors);
            Assert.Equal(NetworkErrorCode.ConnectError, hooks.Errors[0].Item1);
        }

        [Fact]
        public async Task ConnectAsync_WhenPeerClosesDuringHandshake_CompletesWithFalseAndNotifiesClose()
        {
            var stub = new StubWebSocket { ReadyState = WebSocketReadyState.Connecting };
            var hooks = new SocketHooks();
            var socket = CreateSocket(stub, hooks);

            var task = socket.ConnectAsync();
            stub.ReadyState = WebSocketReadyState.Closed;
            stub.CloseCode = 1006;
            stub.CloseReason = "abnormal";
            socket.Poll();

            await task;
            Assert.Single(hooks.Closed);
            Assert.Equal("abnormal", hooks.Closed[0].Item1);
            Assert.Equal((ushort)1006, hooks.Closed[0].Item2);
        }

        [Fact]
        public void Poll_BinaryPacket_InvokesReceiveCallback()
        {
            var stub = new StubWebSocket { ReadyState = WebSocketReadyState.Open };
            var hooks = new SocketHooks();
            var socket = CreateSocket(stub, hooks);

            stub.EnqueuePacket(new byte[] { 1, 2, 3 }, false);
            socket.Poll();

            Assert.Single(hooks.Received);
            Assert.Equal(new byte[] { 1, 2, 3 }, hooks.Received[0]);
            Assert.Empty(hooks.ReceivedText);
        }

        [Fact]
        public void Poll_TextPacket_InvokesTextCallbackWithoutErrorOrClose()
        {
            var stub = new StubWebSocket { ReadyState = WebSocketReadyState.Open };
            var hooks = new SocketHooks();
            var socket = CreateSocket(stub, hooks);

            stub.EnqueuePacket(Encoding.UTF8.GetBytes("hello"), true);
            socket.Poll();

            Assert.Single(hooks.ReceivedText);
            Assert.Equal("hello", hooks.ReceivedText[0]);
            Assert.Empty(hooks.Received);
            Assert.Empty(hooks.Errors);
            Assert.Empty(hooks.Closed);
            Assert.False(socket.IsClosed);
        }

        [Fact]
        public void Poll_MixedPackets_DispatchesByFrameType()
        {
            var stub = new StubWebSocket { ReadyState = WebSocketReadyState.Open };
            var hooks = new SocketHooks();
            var socket = CreateSocket(stub, hooks);

            stub.EnqueuePacket(Encoding.UTF8.GetBytes("text-frame"), true);
            stub.EnqueuePacket(new byte[] { 9 }, false);
            socket.Poll();

            Assert.Single(hooks.ReceivedText);
            Assert.Single(hooks.Received);
        }

        [Fact]
        public void Poll_AfterClosed_SkipsPolling()
        {
            var stub = new StubWebSocket { ReadyState = WebSocketReadyState.Closed };
            var hooks = new SocketHooks();
            var socket = CreateSocket(stub, hooks);
            socket.Poll();
            var pollsAfterClose = stub.PollCount;

            socket.Poll();
            Assert.Equal(pollsAfterClose, stub.PollCount);
        }

        [Fact]
        public void Poll_CloseNotification_IsRaisedOnlyOnce()
        {
            var stub = new StubWebSocket { ReadyState = WebSocketReadyState.Closed };
            var hooks = new SocketHooks();
            var socket = CreateSocket(stub, hooks);

            socket.Poll();
            socket.Poll();

            Assert.Single(hooks.Closed);
            Assert.True(socket.IsClosed);
        }

        [Fact]
        public void Poll_NegativeCloseCode_MapsToZero()
        {
            var stub = new StubWebSocket { ReadyState = WebSocketReadyState.Closed, CloseCode = -1 };
            var hooks = new SocketHooks();
            var socket = CreateSocket(stub, hooks);

            socket.Poll();

            Assert.Single(hooks.Closed);
            Assert.Equal((ushort)0, hooks.Closed[0].Item2);
        }

        [Fact]
        public void Send_ForwardsBufferToClient()
        {
            var stub = new StubWebSocket { ReadyState = WebSocketReadyState.Open };
            var hooks = new SocketHooks();
            var socket = CreateSocket(stub, hooks);

            socket.Send(new byte[] { 4, 5 });
            Assert.Equal(new byte[] { 4, 5 }, stub.LastSentData);
            Assert.Empty(hooks.Errors);
        }

        [Fact]
        public void Send_ClientError_RaisesSendError()
        {
            var stub = new StubWebSocket { SendResult = 7 };
            var hooks = new SocketHooks();
            var socket = CreateSocket(stub, hooks);

            socket.Send(new byte[] { 1 });
            Assert.Single(hooks.Errors);
            Assert.Equal(NetworkErrorCode.SendError, hooks.Errors[0].Item1);
        }

        [Fact]
        public void Send_EmptyBuffer_SkipsClient()
        {
            var stub = new StubWebSocket();
            var hooks = new SocketHooks();
            var socket = CreateSocket(stub, hooks);

            socket.Send(new byte[0]);
            socket.Send(null);
            Assert.Null(stub.LastSentData);
        }

        [Fact]
        public void SendText_ForwardsTextToClient()
        {
            var stub = new StubWebSocket { ReadyState = WebSocketReadyState.Open };
            var hooks = new SocketHooks();
            var socket = CreateSocket(stub, hooks);

            socket.SendText("ping");
            Assert.Equal("ping", stub.LastSentText);
            Assert.Empty(hooks.Errors);
        }

        [Fact]
        public void SendText_ClientError_RaisesSendError()
        {
            var stub = new StubWebSocket { SendTextResult = 3 };
            var hooks = new SocketHooks();
            var socket = CreateSocket(stub, hooks);

            socket.SendText("ping");
            Assert.Single(hooks.Errors);
            Assert.Equal(NetworkErrorCode.SendError, hooks.Errors[0].Item1);
        }

        [Fact]
        public void SendText_EmptyText_SkipsClient()
        {
            var stub = new StubWebSocket();
            var hooks = new SocketHooks();
            var socket = CreateSocket(stub, hooks);

            socket.SendText(string.Empty);
            Assert.Null(stub.LastSentText);
        }

        [Fact]
        public void Shutdown_ClosesClientWithNormalCode()
        {
            var stub = new StubWebSocket { ReadyState = WebSocketReadyState.Open };
            var hooks = new SocketHooks();
            var socket = CreateSocket(stub, hooks);

            socket.Shutdown();
            Assert.Equal(1000, stub.LastCloseCodeArg);
            Assert.False(socket.IsClosed);
        }

        [Fact]
        public void Close_MarksSocketClosed()
        {
            var stub = new StubWebSocket { ReadyState = WebSocketReadyState.Open };
            var hooks = new SocketHooks();
            var socket = CreateSocket(stub, hooks);

            socket.Close();
            Assert.True(socket.IsClosed);
            Assert.Equal(1000, stub.LastCloseCodeArg);
        }

        [Fact]
        public void BufferSizes_PropagateToClient()
        {
            var stub = new StubWebSocket();
            var hooks = new SocketHooks();
            var socket = CreateSocket(stub, hooks);

            socket.ReceiveBufferSize = 1024;
            socket.SendBufferSize = 2048;
            Assert.Equal(1024, stub.InboundBufferSize);
            Assert.Equal(2048, stub.OutboundBufferSize);
        }

        [Fact]
        public void BufferSizes_InvalidValue_Throws()
        {
            var stub = new StubWebSocket();
            var hooks = new SocketHooks();
            var socket = CreateSocket(stub, hooks);

            Assert.Throws<ArgumentException>(() => socket.ReceiveBufferSize = 0);
            Assert.Throws<ArgumentException>(() => socket.SendBufferSize = -1);
        }
    }
}
