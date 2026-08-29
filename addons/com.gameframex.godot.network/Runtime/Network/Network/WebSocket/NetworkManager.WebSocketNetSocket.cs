#if ENABLE_GAME_FRAME_X_WEB_SOCKET || FORCE_ENABLE_GAME_FRAME_X_WEB_SOCKET
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using GameFrameX.Runtime;

namespace GameFrameX.Network.Runtime
{
    public partial class NetworkManager
    {

        /// <summary>
        /// 基于 <see cref="IWebSocket"/> 抽象的 WebSocket 套接字，具体实现由构造函数注入，便于单元测试替换为桩。
        /// </summary>
        public sealed class WebSocketNetSocket : INetworkSocket
        {
            private readonly IWebSocket _client;
            private readonly string _url;
            private int _receiveBufferSize = 65535;
            private int _sendBufferSize = 65535;

            /// <summary>
            /// 是否正在连接
            /// </summary>
            private bool _isConnecting = false;
            private bool _hasCloseNotified = false;

            private TaskCompletionSource<bool> _connectTask = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            private readonly Action<byte[]> _onReceiveAction;
            private readonly Action<string> _onReceiveTextAction;
            private readonly Action<string, ushort> _onCloseAction;
            private readonly Action<NetworkErrorCode, string> _onErrorAction;

            /// <summary>
            /// 初始化 WebSocket 套接字。
            /// </summary>
            /// <param name="client">WebSocket 抽象实现（引擎内为 NativeWebSocketPeer，测试可注入桩）。</param>
            /// <param name="url">连接地址。</param>
            /// <param name="onReceiveAction">收到二进制帧时的回调。</param>
            /// <param name="onReceiveTextAction">收到文本帧时的回调。</param>
            /// <param name="onCloseAction">连接关闭时的回调。</param>
            /// <param name="onErrorAction">发生错误时的回调。</param>
            public WebSocketNetSocket(IWebSocket client, string url, Action<byte[]> onReceiveAction, Action<string> onReceiveTextAction, Action<string, ushort> onCloseAction, Action<NetworkErrorCode, string> onErrorAction)
            {
                _client = client ?? throw new ArgumentNullException(nameof(client));
                _url = url;
                _client.InboundBufferSize = _receiveBufferSize;
                _client.OutboundBufferSize = _sendBufferSize;
                _onReceiveAction = onReceiveAction;
                _onReceiveTextAction = onReceiveTextAction;
                _onCloseAction = onCloseAction;
                _onErrorAction = onErrorAction;
            }


            public async Task ConnectAsync()
            {
                _isConnecting = true;
                _connectTask = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                _hasCloseNotified = false;
                IsClosed = false;
                var error = _client.ConnectToUrl(_url);
                if (error != 0)
                {
                    _isConnecting = false;
                    _onErrorAction?.Invoke(NetworkErrorCode.ConnectError, $"WebSocket connect error: {error}");
                    _connectTask.TrySetResult(false);
                }

                await _connectTask.Task;
            }

            /// <summary>
            /// 轮询 WebSocket 状态并处理消息。
            /// </summary>
            public void Poll()
            {
                if (IsClosed)
                {
                    return;
                }

                _client.Poll();
                var state = _client.ReadyState;
                if (state == WebSocketReadyState.Open)
                {
                    while (_client.GetAvailablePacketCount() > 0)
                    {
                        var packet = _client.GetPacket();
                        if (_client.WasStringPacket())
                        {
                            // ponytail: 文本帧为 Godot 侧新增能力（Unity 基准二进制 only，文本帧被静默忽略）。
                            // 帧级按 RFC 6455 以 UTF-8 解码后整帧上抛，不进入二进制协议（包头+包体）管线；
                            // 对象级编解码由上层在回调里按 MessageSerializerRegistry 既有约定自行处理。
                            _onReceiveTextAction?.Invoke(Encoding.UTF8.GetString(packet));
                        }
                        else
                        {
                            _onReceiveAction?.Invoke(packet);
                        }
                    }

                    if (_isConnecting)
                    {
                        _isConnecting = false;
                        _connectTask.TrySetResult(true);
                    }
                }
                else if (state == WebSocketReadyState.Closed)
                {
                    IsClosed = true;
                    if (_isConnecting)
                    {
                        _isConnecting = false;
                        _connectTask.TrySetResult(false);
                    }

                    if (!_hasCloseNotified)
                    {
                        _hasCloseNotified = true;
                        var closeCode = _client.GetCloseCode();
                        var closeReason = _client.GetCloseReason();
                        ushort code = closeCode >= 0 ? (ushort)closeCode : (ushort)0;
                        _onCloseAction?.Invoke(closeReason, code);
                    }
                }
            }

            /// <summary>
            /// 发送二进制帧数据。
            /// </summary>
            /// <param name="buffer">要发送的数据。</param>
            public void Send(byte[] buffer)
            {
                if (buffer == null || buffer.Length <= 0)
                {
                    return;
                }

                var error = _client.Send(buffer);
                if (error != 0)
                {
                    _onErrorAction?.Invoke(NetworkErrorCode.SendError, $"WebSocket send error: {error}");
                }
            }

            /// <summary>
            /// 发送文本帧数据。
            /// </summary>
            /// <param name="text">要发送的文本。</param>
            public void SendText(string text)
            {
                if (string.IsNullOrEmpty(text))
                {
                    return;
                }

                var error = _client.SendText(text);
                if (error != 0)
                {
                    _onErrorAction?.Invoke(NetworkErrorCode.SendError, $"WebSocket send text error: {error}");
                }
            }

            public bool IsConnected
            {
                get { return _client.ReadyState == WebSocketReadyState.Open; }
            }

            public bool IsClosed { get; private set; }

            public EndPoint LocalEndPoint
            {
                get { return null; }
            }

            public EndPoint RemoteEndPoint
            {
                get { return null; }
            }

            public int ReceiveBufferSize
            {
                get { return _receiveBufferSize; }
                set
                {
                    if (value <= 0)
                    {
                        throw new ArgumentException("Receive buffer size is invalid.", nameof(value));
                    }

                    _receiveBufferSize = value;
                    _client.InboundBufferSize = value;
                }
            }

            public int SendBufferSize
            {
                get { return _sendBufferSize; }
                set
                {
                    if (value <= 0)
                    {
                        throw new ArgumentException("Send buffer size is invalid.", nameof(value));
                    }

                    _sendBufferSize = value;
                    _client.OutboundBufferSize = value;
                }
            }

            public void Shutdown()
            {
                if (IsClosed)
                {
                    return;
                }

                _client.Close(1000, string.Empty);
            }

            public void Close()
            {
                if (IsClosed)
                {
                    return;
                }

                _client.Close(1000, string.Empty);
                IsClosed = true;
            }
        }
    }
}

#endif
