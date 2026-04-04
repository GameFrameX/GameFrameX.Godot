using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.Networking;

namespace YooAsset
{
    /// <summary>
    /// 自定义下载器的请求委托
    /// </summary>
    public delegate UnityWebRequest UnityWebRequestDelegate(string url);

    [UnityEngine.Scripting.Preserve]
    public class DownloadSystemHelper
    {
        public static UnityWebRequestDelegate UnityWebRequestCreater = null;
        public static IHttpTransport HttpTransport = new GodotHttpTransport();

        [UnityEngine.Scripting.Preserve]
        public static UnityWebRequest NewUnityWebRequestGet(string requestURL)
        {
            UnityWebRequest webRequest;
            if (UnityWebRequestCreater != null)
            {
                webRequest = UnityWebRequestCreater.Invoke(requestURL);
            }
            else
            {
                webRequest = new UnityWebRequest(requestURL, UnityWebRequest.kHttpVerbGET);
            }

            return webRequest;
        }

        /// <summary>
        /// 创建GET请求并设置超时时间
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public static UnityWebRequest NewUnityWebRequestGet(string requestURL, int timeout)
        {
            var webRequest = NewUnityWebRequestGet(requestURL);
            SetRequestTimeout(webRequest, timeout);
            return webRequest;
        }

        /// <summary>
        /// 设置请求超时时间
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public static void SetRequestTimeout(UnityWebRequest webRequest, int timeout)
        {
            if (webRequest == null)
            {
                return;
            }

            if (timeout > 0)
            {
                webRequest.timeout = timeout;
            }
        }

        /// <summary>
        /// 发送请求
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public static UnityWebRequestAsyncOperation SendRequest(UnityWebRequest webRequest)
        {
            return webRequest.SendWebRequest();
        }

        /// <summary>
        /// 取消请求
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public static void AbortRequest(UnityWebRequest webRequest)
        {
            if (webRequest != null)
            {
                webRequest.Abort();
            }
        }

        [UnityEngine.Scripting.Preserve]
        public static bool IsRetryableRequest(UnityWebRequest webRequest, bool isAbort)
        {
            if (webRequest == null)
            {
                return false;
            }

            if (isAbort)
            {
                return true;
            }

            var responseCode = webRequest.responseCode;
            if (responseCode == 0)
            {
                return true;
            }

            if (responseCode == 408 || responseCode == 429)
            {
                return true;
            }

            return responseCode >= 500;
        }

        [UnityEngine.Scripting.Preserve]
        public static string GetRequestErrorCode(UnityWebRequest webRequest, bool isAbort)
        {
            if (webRequest == null)
            {
                return "REQUEST_NULL";
            }

            if (isAbort)
            {
                return "TIMEOUT";
            }

            var responseCode = webRequest.responseCode;
            if (responseCode > 0)
            {
                return $"HTTP_{responseCode}";
            }

            return webRequest.result switch
            {
                UnityWebRequest.Result.ConnectionError => "CONNECTION_ERROR",
                UnityWebRequest.Result.ProtocolError => "PROTOCOL_ERROR",
                UnityWebRequest.Result.DataProcessingError => "DATA_PROCESSING_ERROR",
                _ => "UNKNOWN",
            };
        }

        [UnityEngine.Scripting.Preserve]
        public static string FormatRequestError(UnityWebRequest webRequest, string requestURL, bool isAbort)
        {
            var errorCode = GetRequestErrorCode(webRequest, isAbort);
            var requestError = webRequest != null ? webRequest.error : "request is null";
            var responseCode = webRequest != null ? webRequest.responseCode : 0;
            return $"[{errorCode}] URL : {requestURL} HTTP : {responseCode} Error : {requestError}";
        }

        [UnityEngine.Scripting.Preserve]
        public static Task<HttpResponse> RequestTextAsync(string requestURL, int timeout, bool appendTimeTicks, CancellationToken cancellationToken)
        {
            if (HttpTransport == null)
            {
                return Task.FromResult<HttpResponse>(new HttpResponse
                {
                    Success = false,
                    Error = "http transport is null.",
                });
            }

            return HttpTransport.GetTextAsync(requestURL, timeout, appendTimeTicks, cancellationToken);
        }

        [UnityEngine.Scripting.Preserve]
        public static Task<HttpResponse> RequestDataAsync(string requestURL, int timeout, bool appendTimeTicks, CancellationToken cancellationToken)
        {
            if (HttpTransport == null)
            {
                return Task.FromResult<HttpResponse>(new HttpResponse
                {
                    Success = false,
                    Error = "http transport is null.",
                });
            }

            return HttpTransport.GetDataAsync(requestURL, timeout, appendTimeTicks, cancellationToken);
        }

        /// <summary>
        /// 获取WWW加载本地资源的路径
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public static string ConvertToWWWPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("jar:file://", StringComparison.OrdinalIgnoreCase))
            {
                return path;
            }

            if (path.StartsWith("res://", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("user://", StringComparison.OrdinalIgnoreCase))
            {
                return path;
            }

            if (Uri.TryCreate(path, UriKind.Absolute, out var absoluteUri))
            {
                return absoluteUri.ToString();
            }

            if (Path.IsPathRooted(path))
            {
                return new Uri(path).AbsoluteUri;
            }

            return path;
        }
    }
}
