using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Godot;

namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    public sealed class GodotHttpTransport : IHttpTransport
    {
        private static readonly System.Net.Http.HttpClient s_HttpClient = new System.Net.Http.HttpClient(new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
        });

        [AssetSystemPreserve]
        public async Task<HttpResponse> GetTextAsync(string requestURL, int timeout, bool appendTimeTicks, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(requestURL))
            {
                return new HttpResponse
                {
                    Success = false,
                    Error = "request url is null or empty.",
                };
            }

            try
            {
                if (TryResolveLocalFilePath(requestURL, out var localFilePath))
                {
                    return await ReadLocalTextAsync(localFilePath, cancellationToken);
                }

                using var request = new HttpRequestMessage(HttpMethod.Get, BuildRequestURL(requestURL, appendTimeTicks));
                using var linkedCts = CreateTimeoutToken(timeout, cancellationToken);
                using var response = await s_HttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead, linkedCts.Token);
                var body = await response.Content.ReadAsStringAsync(linkedCts.Token);
                return new HttpResponse
                {
                    Success = response.IsSuccessStatusCode,
                    StatusCode = (long)response.StatusCode,
                    Text = body,
                    Error = response.IsSuccessStatusCode ? null : response.ReasonPhrase,
                };
            }
            catch (OperationCanceledException)
            {
                return new HttpResponse
                {
                    Success = false,
                    Error = "request canceled or timeout.",
                };
            }
            catch (Exception exception)
            {
                return new HttpResponse
                {
                    Success = false,
                    Error = exception.Message,
                };
            }
        }

        [AssetSystemPreserve]
        public async Task<HttpResponse> GetDataAsync(string requestURL, int timeout, bool appendTimeTicks, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(requestURL))
            {
                return new HttpResponse
                {
                    Success = false,
                    Error = "request url is null or empty.",
                };
            }

            try
            {
                if (TryResolveLocalFilePath(requestURL, out var localFilePath))
                {
                    return await ReadLocalDataAsync(localFilePath, cancellationToken);
                }

                using var request = new HttpRequestMessage(HttpMethod.Get, BuildRequestURL(requestURL, appendTimeTicks));
                using var linkedCts = CreateTimeoutToken(timeout, cancellationToken);
                using var response = await s_HttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead, linkedCts.Token);
                var data = await response.Content.ReadAsByteArrayAsync(linkedCts.Token);
                return new HttpResponse
                {
                    Success = response.IsSuccessStatusCode,
                    StatusCode = (long)response.StatusCode,
                    Data = data,
                    Error = response.IsSuccessStatusCode ? null : response.ReasonPhrase,
                };
            }
            catch (OperationCanceledException)
            {
                return new HttpResponse
                {
                    Success = false,
                    Error = "request canceled or timeout.",
                };
            }
            catch (Exception exception)
            {
                return new HttpResponse
                {
                    Success = false,
                    Error = exception.Message,
                };
            }
        }

        private static string BuildRequestURL(string requestURL, bool appendTimeTicks)
        {
            if (!appendTimeTicks)
            {
                return requestURL;
            }

            var separator = requestURL.Contains("?") ? "&" : "?";
            return $"{requestURL}{separator}time_ticks={DateTime.Now.Ticks}";
        }

        private static bool TryResolveLocalFilePath(string requestURL, out string localFilePath)
        {
            localFilePath = string.Empty;
            if (string.IsNullOrWhiteSpace(requestURL))
            {
                return false;
            }

            if (requestURL.StartsWith("res://", StringComparison.OrdinalIgnoreCase) ||
                requestURL.StartsWith("user://", StringComparison.OrdinalIgnoreCase))
            {
                localFilePath = ProjectSettings.GlobalizePath(requestURL).Replace('\\', '/');
                return true;
            }

            if (Uri.TryCreate(requestURL, UriKind.Absolute, out var uri) && uri.IsFile)
            {
                localFilePath = uri.LocalPath.Replace('\\', '/');
                return true;
            }

            if (Path.IsPathRooted(requestURL))
            {
                localFilePath = requestURL.Replace('\\', '/');
                return true;
            }

            return false;
        }

        private static async Task<HttpResponse> ReadLocalTextAsync(string localFilePath, CancellationToken cancellationToken)
        {
            if (!File.Exists(localFilePath))
            {
                return CreateLocalFileMissingResponse(localFilePath);
            }

            var text = await File.ReadAllTextAsync(localFilePath, Encoding.UTF8, cancellationToken);
            return new HttpResponse
            {
                Success = true,
                StatusCode = 200,
                Text = text,
            };
        }

        private static async Task<HttpResponse> ReadLocalDataAsync(string localFilePath, CancellationToken cancellationToken)
        {
            if (!File.Exists(localFilePath))
            {
                return CreateLocalFileMissingResponse(localFilePath);
            }

            var data = await File.ReadAllBytesAsync(localFilePath, cancellationToken);
            return new HttpResponse
            {
                Success = true,
                StatusCode = 200,
                Data = data,
            };
        }

        private static HttpResponse CreateLocalFileMissingResponse(string localFilePath)
        {
            return new HttpResponse
            {
                Success = false,
                StatusCode = 404,
                Error = $"local file not found: {localFilePath}",
            };
        }

        private static CancellationTokenSource CreateTimeoutToken(int timeout, CancellationToken cancellationToken)
        {
            var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            if (timeout > 0)
            {
                linkedCts.CancelAfter(TimeSpan.FromSeconds(timeout));
            }

            return linkedCts;
        }
    }
}
