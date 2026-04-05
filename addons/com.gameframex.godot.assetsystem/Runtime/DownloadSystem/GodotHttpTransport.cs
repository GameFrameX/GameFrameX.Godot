using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace YooAsset
{
    [UnityEngine.Scripting.Preserve]
    public sealed class GodotHttpTransport : IHttpTransport
    {
        private static readonly HttpClient s_HttpClient = new HttpClient(new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
        });

        [UnityEngine.Scripting.Preserve]
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

        [UnityEngine.Scripting.Preserve]
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
