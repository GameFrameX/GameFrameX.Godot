#if (ENABLE_GAME_FRAME_X_WEB_SOCKET && UNITY_WEBGL) || FORCE_ENABLE_GAME_FRAME_X_WEB_SOCKET
using System;
using System.Net;
using System.Threading.Tasks;
using GameFrameX.Runtime;
using Godot;

namespace GameFrameX.Network.Runtime
{
    public partial class NetworkManager
    {
        
        private sealed class WebSocketNetSocket : INetworkSocket
        {
            private readonly WebSocketPeer _client;
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
            private readonly Action<string, ushort> _onCloseAction;
            private readonly Action<NetworkErrorCode, string> _onErrorAction;

            /// <summary>
            /// 初始化 WebSocket 套接字。
            /// </summary>
            /// <param name="url">连接地址。</param>
            /// <param name="onReceiveAction">收到二进制消息时的回调。</param>
            /// <param name="onCloseAction">连接关闭时的回调。</param>
            /// <param name="onErrorAction">发生错误时的回调。</param>
            public WebSocketNetSocket(string url, Action<byte[]> onReceiveAction, Action<string, ushort> onCloseAction, Action<NetworkErrorCode, string> onErrorAction)
            {
                _url = url;
                _client = new WebSocketPeer();
                _client.InboundBufferSize = _receiveBufferSize;
                _client.OutboundBufferSize = _sendBufferSize;
                _onReceiveAction = onReceiveAction;
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
                if (error != Godot.Error.Ok)
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
                var state = _client.GetReadyState();
                if (state == WebSocketPeer.State.Open)
                {
                    while (_client.GetAvailablePacketCount() > 0)
                    {
                        var packet = _client.GetPacket();
                        if (!_client.WasStringPacket())
                        {
                            _onReceiveAction?.Invoke(packet);
                        }
                        else
                        {
                            _onErrorAction?.Invoke(NetworkErrorCode.DeserializePacketError, "WebSocket received text frame, but current channel only supports binary frames.");
                            _client.Close(1003, "Binary only protocol");
                            return;
                        }
                    }

                    if (_isConnecting)
                    {
                        _isConnecting = false;
                        _connectTask.TrySetResult(true);
                    }
                }
                else if (state == WebSocketPeer.State.Closed)
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
            /// 发送二进制数据。
            /// </summary>
            /// <param name="buffer">要发送的数据。</param>
            public void Send(byte[] buffer)
            {
                if (buffer == null || buffer.Length <= 0)
                {
                    return;
                }

                var error = _client.Send(buffer);
                if (error != Godot.Error.Ok)
                {
                    _onErrorAction?.Invoke(NetworkErrorCode.SendError, $"WebSocket send error: {error}");
                }
            }

            public bool IsConnected
            {
                get { return _client.GetReadyState() == WebSocketPeer.State.Open; }
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

                _client.Close();
            }

            public void Close()
            {
                if (IsClosed)
                {
                    return;
                }

                _client.Close();
                IsClosed = true;
            }
        }
    }
}

#endif
