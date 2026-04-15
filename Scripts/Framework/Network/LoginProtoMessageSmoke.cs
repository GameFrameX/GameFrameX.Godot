using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GameFrameX.Network.Runtime;
using GameFrameX.Runtime;
using Godot;

namespace Godot.Startup.Network
{
	/// <summary>
	/// UILogin 打开时的 Proto 发送/回包测试工具（本地回环服务）。
	/// </summary>
	public static class LoginProtoMessageSmoke
	{
		private const int RequestMessageId = 910001;
		private const int ResponseMessageId = 910002;
		private const int ListenPort = 29183;
		private const string TcpEndpoint = "tcp://127.0.0.1:29183";
		private const int MaxFrameSize = 4 * 1024 * 1024;

		private static readonly object Gate = new();
		private static TcpListener _listener;
		private static CancellationTokenSource _listenerCts;
		private static Task _listenerTask;

		public static async Task SendOnLoginOpenAsync(string callerTag)
		{
			try
			{
				LogInfo($"[ProtoSmoke][Client] begin caller={callerTag}");
				// WebSocket smoke 已停用，当前改为 TCP 回环测试。
				var started = await EnsureLoopbackServerStartedAsync();
				if (!started)
				{
					LogError($"[ProtoSmoke][Client] loopback tcp server start failed. endpoint={TcpEndpoint}");
					return;
				}

				var request = new LoginOpenProtoRequest
				{
					CallerTag = callerTag ?? string.Empty,
					ClientUtcMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
				};

				var requestWrapper = new MessageHttpObject
				{
					Id = RequestMessageId,
					UniqueId = request.UniqueId,
					Body = SerializerHelper.Serialize(request)
				};
				var requestBytes = SerializeMessageEnvelope(requestWrapper);

				using var client = new TcpClient();
				await client.ConnectAsync(IPAddress.Loopback, ListenPort);
				LogInfo($"[ProtoSmoke][Client] tcp connected endpoint={TcpEndpoint}");

				using var stream = client.GetStream();
				await WriteFrameAsync(stream, requestBytes, CancellationToken.None);
				LogInfo($"[ProtoSmoke][Client->Server] send id={RequestMessageId} unique={request.UniqueId} caller={request.CallerTag}");

				var responseBytes = await ReadFrameAsync(stream, CancellationToken.None);
				LogInfo($"[ProtoSmoke][Client] tcp recv bytes={responseBytes.Length}");
				if (responseBytes.Length == 0)
				{
					LogError("[ProtoSmoke][Client] tcp closed without payload.");
					return;
				}

				var responseWrapper = TryDeserializeMessageHttpObject(responseBytes);
				if (responseWrapper == null)
				{
					LogError("[ProtoSmoke][Client] response wrapper deserialize failed.");
					return;
				}

				if (responseWrapper.Id != ResponseMessageId)
				{
					LogError($"[ProtoSmoke][Client] invalid response id={responseWrapper.Id}, expected={ResponseMessageId}");
					return;
				}

				var responseMessage = SerializerHelper.Deserialize(responseWrapper.Body, typeof(LoginOpenProtoResponse)) as LoginOpenProtoResponse;
				if (responseMessage == null)
				{
					LogError("[ProtoSmoke][Client] response body deserialize failed.");
					return;
				}

				LogInfo(
					$"[ProtoSmoke][Server->Client] recv id={responseWrapper.Id} unique={responseWrapper.UniqueId} error={responseMessage.ErrorCode} message={responseMessage.Message} serverUtcMs={responseMessage.ServerUtcMs}");
			}
			catch (Exception exception)
			{
				LogError($"[ProtoSmoke][Client] send failed: {exception}");
			}
		}

		private static Task<bool> EnsureLoopbackServerStartedAsync()
		{
			lock (Gate)
			{
				if (_listenerTask != null)
				{
					return Task.FromResult(true);
				}

				try
				{
					_listener = new TcpListener(IPAddress.Loopback, ListenPort);
					_listener.Start();
					_listenerCts = new CancellationTokenSource();
					_listenerTask = Task.Run(() => ServerLoopAsync(_listener, _listenerCts.Token));
					LogInfo($"[ProtoSmoke][Server] started tcp={TcpEndpoint}");
					return Task.FromResult(true);
				}
				catch (Exception exception)
				{
					_listener?.Stop();
					_listener = null;
					_listenerCts = null;
					_listenerTask = null;
					LogError($"[ProtoSmoke][Server] start failed on :{ListenPort}. {exception.Message}");
					return Task.FromResult(false);
				}
			}
		}

		private static async Task ServerLoopAsync(TcpListener listener, CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				TcpClient client;
				try
				{
					client = await listener.AcceptTcpClientAsync(cancellationToken);
				}
				catch (OperationCanceledException)
				{
					break;
				}
				catch (ObjectDisposedException)
				{
					break;
				}
				catch (Exception exception)
				{
					if (cancellationToken.IsCancellationRequested)
					{
						break;
					}

					LogWarning($"[ProtoSmoke][Server] accept failed: {exception.Message}");
					continue;
				}

				_ = Task.Run(async () =>
				{
					using (client)
					{
						try
						{
							await HandleClientAsync(client, cancellationToken);
						}
						catch (Exception exception)
						{
							LogError($"[ProtoSmoke][Server] handle failed: {exception}");
						}
					}
				}, cancellationToken);
			}
		}

		private static async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
		{
			using var stream = client.GetStream();
			var requestBody = await ReadFrameAsync(stream, cancellationToken);
			if (requestBody == null || requestBody.Length == 0)
			{
				LogWarning("[ProtoSmoke][Server] request payload empty.");
				return;
			}

			var requestWrapper = TryDeserializeMessageHttpObject(requestBody);
			if (requestWrapper == null)
			{
				throw new InvalidOperationException("request wrapper deserialize failed.");
			}

			if (requestWrapper.Id != RequestMessageId)
			{
				throw new InvalidOperationException($"invalid request id={requestWrapper.Id}, expected={RequestMessageId}");
			}

			if (requestWrapper.Body == null || requestWrapper.Body.Length == 0)
			{
				throw new InvalidOperationException("request body is empty. message envelope missing Body payload.");
			}

			var requestMessage = SerializerHelper.Deserialize(requestWrapper.Body, typeof(LoginOpenProtoRequest)) as LoginOpenProtoRequest;
			if (requestMessage == null)
			{
				throw new InvalidOperationException("request body deserialize failed.");
			}

			LogInfo(
				$"[ProtoSmoke][Client->Server] recv id={requestWrapper.Id} unique={requestWrapper.UniqueId} caller={requestMessage.CallerTag} clientUtcMs={requestMessage.ClientUtcMs}");

			var responseMessage = new LoginOpenProtoResponse
			{
				ErrorCode = 0,
				Message = "OK",
				ServerUtcMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
			};
			responseMessage.SetUpdateUniqueId(requestWrapper.UniqueId);

			var responseWrapper = new MessageHttpObject
			{
				Id = ResponseMessageId,
				UniqueId = requestWrapper.UniqueId,
				Body = SerializerHelper.Serialize(responseMessage)
			};

			var responseBody = SerializeMessageEnvelope(responseWrapper);
			await WriteFrameAsync(stream, responseBody, cancellationToken);

			LogInfo(
				$"[ProtoSmoke][Server->Client] send id={responseWrapper.Id} unique={responseWrapper.UniqueId} error={responseMessage.ErrorCode} message={responseMessage.Message}");
		}

		private static async Task WriteFrameAsync(Stream stream, byte[] payload, CancellationToken cancellationToken)
		{
			payload ??= Array.Empty<byte>();
			if (payload.Length > MaxFrameSize)
			{
				throw new InvalidOperationException($"payload too large: {payload.Length}");
			}

			var lengthPrefix = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(payload.Length));
			await stream.WriteAsync(lengthPrefix, 0, lengthPrefix.Length, cancellationToken);
			if (payload.Length > 0)
			{
				await stream.WriteAsync(payload, 0, payload.Length, cancellationToken);
			}

			await stream.FlushAsync(cancellationToken);
		}

		private static async Task<byte[]> ReadFrameAsync(Stream stream, CancellationToken cancellationToken)
		{
			var lengthPrefix = await ReadExactAsync(stream, 4, cancellationToken);
			if (lengthPrefix.Length == 0)
			{
				return Array.Empty<byte>();
			}

			var length = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(lengthPrefix, 0));
			if (length <= 0)
			{
				return Array.Empty<byte>();
			}

			if (length > MaxFrameSize)
			{
				throw new InvalidOperationException($"incoming payload too large: {length}");
			}

			return await ReadExactAsync(stream, length, cancellationToken);
		}

		private static async Task<byte[]> ReadExactAsync(Stream stream, int byteCount, CancellationToken cancellationToken)
		{
			if (byteCount <= 0)
			{
				return Array.Empty<byte>();
			}

			var buffer = new byte[byteCount];
			var offset = 0;
			while (offset < byteCount)
			{
				var read = await stream.ReadAsync(buffer, offset, byteCount - offset, cancellationToken);
				if (read <= 0)
				{
					if (offset == 0)
					{
						return Array.Empty<byte>();
					}

					throw new EndOfStreamException($"stream ended before reading full frame. expected={byteCount} actual={offset}");
				}

				offset += read;
			}

			return buffer;
		}

		private static MessageHttpObject TryDeserializeMessageHttpObject(byte[] bytes)
		{
			if (bytes == null || bytes.Length == 0)
			{
				return null;
			}

			var message = SerializerHelper.Deserialize(bytes, typeof(MessageHttpObject)) as MessageHttpObject;
			if (message?.Body != null && message.Body.Length > 0)
			{
				return message;
			}

			try
			{
				var raw = Encoding.UTF8.GetString(bytes);
				using var document = JsonDocument.Parse(raw);
				var root = document.RootElement;
				var fallback = message ?? new MessageHttpObject();

				if (root.TryGetProperty(nameof(MessageHttpObject.Id), out var idNode))
				{
					fallback.Id = idNode.GetInt32();
				}

				if (root.TryGetProperty(nameof(MessageHttpObject.UniqueId), out var uniqueNode))
				{
					fallback.UniqueId = uniqueNode.GetInt32();
				}

				if (root.TryGetProperty(nameof(MessageHttpObject.Body), out var bodyNode))
				{
					if (bodyNode.ValueKind == JsonValueKind.String)
					{
						var text = bodyNode.GetString();
						fallback.Body = string.IsNullOrWhiteSpace(text) ? Array.Empty<byte>() : Convert.FromBase64String(text);
					}
					else if (bodyNode.ValueKind == JsonValueKind.Array)
					{
						using var bodyStream = new MemoryStream();
						foreach (var item in bodyNode.EnumerateArray())
						{
							bodyStream.WriteByte((byte)item.GetInt32());
						}

						fallback.Body = bodyStream.ToArray();
					}
				}

				if (fallback.Body == null)
				{
					fallback.Body = Array.Empty<byte>();
				}

				return fallback;
			}
			catch
			{
				return message;
			}
		}

		private static byte[] SerializeMessageEnvelope(MessageHttpObject message)
		{
			var payload = new MessageEnvelopePayload
			{
				Id = message?.Id ?? 0,
				UniqueId = message?.UniqueId ?? 0,
				Body = message?.Body == null || message.Body.Length == 0 ? string.Empty : Convert.ToBase64String(message.Body)
			};

			var json = JsonSerializer.Serialize(payload);
			return Encoding.UTF8.GetBytes(json);
		}

		private sealed class LoginOpenProtoRequest : MessageObject
		{
			public string CallerTag { get; set; } = string.Empty;
			public long ClientUtcMs { get; set; }
		}

		private sealed class LoginOpenProtoResponse : MessageObject, IResponseMessage
		{
			public int ErrorCode { get; set; }
			public string Message { get; set; } = string.Empty;
			public long ServerUtcMs { get; set; }
		}

		private sealed class MessageEnvelopePayload
		{
			public int Id { get; set; }
			public int UniqueId { get; set; }
			public string Body { get; set; } = string.Empty;
		}

		private static void LogInfo(string message)
		{
			Log.Info(message);
			Console.WriteLine(message);
		}

		private static void LogWarning(string message)
		{
			Log.Warning(message);
			Console.WriteLine(message);
		}

		private static void LogError(string message)
		{
			Log.Error(message);
			Console.WriteLine(message);
		}
	}
}
