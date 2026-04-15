using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GameFrameX.Network.Runtime;
using GameFrameX.Web.ProtoBuff.Runtime;
using Godot;

namespace Godot.Startup.Verification
{
	/// <summary>
	/// Proto 消息发送/回包回环验证器：
	/// 1) 本地启动一个 HTTP 回包服务（可选）
	/// 2) 使用 WebProtoBuffManager.Post 发送请求
	/// 3) 校验回包类型与内容
	/// </summary>
	public partial class ProtoMessageRuntimeVerifier : Node
	{
		[Export] public bool AutoRunOnReady { get; set; } = true;
		[Export] public bool AutoQuitOnFinish { get; set; } = true;
		[Export] public bool UseLocalLoopbackServer { get; set; } = true;
		[Export] public string EndpointUrl { get; set; } = "http://127.0.0.1:29180/proto/";
		[Export(PropertyHint.Range, "1,30,1")] public int TimeoutSeconds { get; set; } = 8;
		[Export] public string RequestText { get; set; } = "hello-proto";
		[Export] public int RequestNumber { get; set; } = 2026;

		private HttpListener _listener;
		private CancellationTokenSource _listenerCts;
		private Task _listenerTask;

		public override async void _Ready()
		{
			if (!AutoRunOnReady)
			{
				GD.Print("[ProtoRuntimeVerifier] AutoRunOnReady=false, skip.");
				return;
			}

			await RunVerificationAsync();
		}

		private async Task RunVerificationAsync()
		{
			var stopwatch = Stopwatch.StartNew();
			var failed = false;
			WebProtoBuffManager manager = null;
			try
			{
				ProtoMessageIdHandler.Init(typeof(ProtoMessageRuntimeVerifier).Assembly);
				GD.Print("[ProtoRuntimeVerifier] ProtoMessageIdHandler initialized.");

				if (UseLocalLoopbackServer)
				{
					StartLoopbackServer(EndpointUrl);
				}

				manager = new WebProtoBuffManager
				{
					Timeout = Math.Max(1, TimeoutSeconds)
				};

				var request = new ProtoEchoRequest
				{
					Text = RequestText ?? string.Empty,
					Number = RequestNumber
				};
				GD.Print($"[ProtoRuntimeVerifier] SEND reqId={request.UniqueId} text={request.Text} number={request.Number} url={EndpointUrl}");

				var callTask = manager.Post<ProtoEchoResponse>(EndpointUrl, request);
				await PumpManagerUntilDoneAsync(manager, callTask, TimeSpan.FromSeconds(Math.Max(1, TimeoutSeconds)));
				var response = await callTask;
				if (response == null)
				{
					throw new InvalidOperationException("Response is null.");
				}

				if (response.ErrorCode != 0)
				{
					throw new InvalidOperationException($"Response ErrorCode={response.ErrorCode}");
				}

				if (!string.Equals(response.EchoText, request.Text, StringComparison.Ordinal))
				{
					throw new InvalidOperationException($"EchoText mismatch. req={request.Text}, resp={response.EchoText}");
				}

				if (response.EchoNumber != request.Number)
				{
					throw new InvalidOperationException($"EchoNumber mismatch. req={request.Number}, resp={response.EchoNumber}");
				}

				GD.Print($"[ProtoRuntimeVerifier] PASS respId={response.UniqueId} echoText={response.EchoText} echoNumber={response.EchoNumber}");
			}
			catch (Exception exception)
			{
				failed = true;
				GD.PrintErr($"[ProtoRuntimeVerifier] FAIL: {exception.Message}");
			}
			finally
			{
				stopwatch.Stop();
				try
				{
					manager?.Shutdown();
				}
				catch
				{
					// ignore
				}

				await StopLoopbackServerAsync();
				GD.Print($"[ProtoRuntimeVerifier] elapsed={stopwatch.ElapsedMilliseconds}ms");
				if (AutoQuitOnFinish)
				{
					GetTree().Quit(failed ? 1 : 0);
				}
			}
		}

		private async Task PumpManagerUntilDoneAsync(WebProtoBuffManager manager, Task task, TimeSpan timeout)
		{
			if (manager == null)
			{
				throw new ArgumentNullException(nameof(manager));
			}

			if (task == null)
			{
				throw new ArgumentNullException(nameof(task));
			}

			var begin = Stopwatch.GetTimestamp();
			while (!task.IsCompleted)
			{
				manager.Update(0f, 0f);
				await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
				var elapsedSeconds = (Stopwatch.GetTimestamp() - begin) / (double)Stopwatch.Frequency;
				if (elapsedSeconds > timeout.TotalSeconds)
				{
					throw new TimeoutException($"Call timeout after {timeout.TotalSeconds:F1}s.");
				}
			}
		}

		private void StartLoopbackServer(string endpointUrl)
		{
			if (!Uri.TryCreate(endpointUrl, UriKind.Absolute, out var endpoint))
			{
				throw new InvalidOperationException($"Invalid EndpointUrl: {endpointUrl}");
			}

			var prefix = endpoint.GetLeftPart(UriPartial.Authority) + endpoint.AbsolutePath;
			if (!prefix.EndsWith("/", StringComparison.Ordinal))
			{
				prefix += "/";
			}

			_listener = new HttpListener();
			_listener.Prefixes.Add(prefix);
			_listener.Start();
			_listenerCts = new CancellationTokenSource();
			_listenerTask = Task.Run(() => LoopbackServerMainAsync(_listener, _listenerCts.Token));
			GD.Print($"[ProtoRuntimeVerifier] loopback server started. prefix={prefix}");
		}

		private async Task StopLoopbackServerAsync()
		{
			if (_listenerCts == null || _listener == null)
			{
				return;
			}

			try
			{
				_listenerCts.Cancel();
				_listener.Stop();
			}
			catch
			{
				// ignore
			}

			try
			{
				if (_listenerTask != null)
				{
					await _listenerTask;
				}
			}
			catch
			{
				// ignore
			}
			finally
			{
				_listener.Close();
				_listener = null;
				_listenerTask = null;
				_listenerCts.Dispose();
				_listenerCts = null;
				GD.Print("[ProtoRuntimeVerifier] loopback server stopped.");
			}
		}

		private async Task LoopbackServerMainAsync(HttpListener listener, CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				HttpListenerContext context;
				try
				{
					context = await listener.GetContextAsync().WaitAsync(cancellationToken);
				}
				catch (OperationCanceledException)
				{
					break;
				}
				catch (HttpListenerException)
				{
					if (cancellationToken.IsCancellationRequested)
					{
						break;
					}

					continue;
				}

				_ = Task.Run(async () =>
				{
					try
					{
						await HandleLoopbackRequestAsync(context);
					}
					catch (Exception exception)
					{
						try
						{
							context.Response.StatusCode = 500;
							var errorBytes = Encoding.UTF8.GetBytes(exception.Message);
							context.Response.ContentType = "text/plain; charset=utf-8";
							context.Response.ContentLength64 = errorBytes.LongLength;
							await context.Response.OutputStream.WriteAsync(errorBytes, 0, errorBytes.Length);
						}
						catch
						{
							// ignore
						}
						finally
						{
							context.Response.Close();
						}
					}
				}, cancellationToken);
			}
		}

		private static async Task HandleLoopbackRequestAsync(HttpListenerContext context)
		{
			byte[] requestBytes;
			using (var memory = new MemoryStream())
			{
				await context.Request.InputStream.CopyToAsync(memory);
				requestBytes = memory.ToArray();
			}

			var wrapper = SerializerHelper.Deserialize(requestBytes, typeof(MessageHttpObject)) as MessageHttpObject;
			if (wrapper == null || wrapper.Id == 0 || wrapper.Body == null || wrapper.Body.Length == 0)
			{
				throw new InvalidOperationException("Invalid request wrapper.");
			}

			var request = SerializerHelper.Deserialize(wrapper.Body, typeof(ProtoEchoRequest)) as ProtoEchoRequest;
			if (request == null)
			{
				throw new InvalidOperationException("Invalid request body.");
			}

			var response = new ProtoEchoResponse
			{
				ErrorCode = 0,
				EchoText = request.Text,
				EchoNumber = request.Number
			};
			response.SetUpdateUniqueId(request.UniqueId);

			var responseWrapper = new MessageHttpObject
			{
				Id = ProtoMessageIdHandler.GetRespMessageIdByType(typeof(ProtoEchoResponse)),
				UniqueId = wrapper.UniqueId,
				Body = SerializerHelper.Serialize(response)
			};
			if (responseWrapper.Id == 0)
			{
				throw new InvalidOperationException("Response message id is not registered.");
			}

			var responseBytes = SerializerHelper.Serialize(responseWrapper);
			context.Response.StatusCode = 200;
			context.Response.ContentType = "application/x-protobuf";
			context.Response.ContentLength64 = responseBytes.LongLength;
			await context.Response.OutputStream.WriteAsync(responseBytes, 0, responseBytes.Length);
			context.Response.Close();
		}

		[MessageTypeHandler(900101)]
		private sealed class ProtoEchoRequest : MessageObject, IRequestMessage
		{
			public string Text { get; set; } = string.Empty;
			public int Number { get; set; }
		}

		[MessageTypeHandler(900102)]
		private sealed class ProtoEchoResponse : MessageObject, IResponseMessage
		{
			public int ErrorCode { get; set; }
			public string EchoText { get; set; } = string.Empty;
			public int EchoNumber { get; set; }
		}
	}
}
