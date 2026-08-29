using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GameFrameX.AssetSystem;

namespace GameFrameX.EngineTests
{
    /// <summary>
    /// 本地文件 IHttpTransport：把非 http 的绝对路径请求当作本地文件读取，
    /// 使 HostPlayMode 的版本/清单/资源包下载在 headless 环境完全离线可跑。
    /// 与 Scripts/Verification/AssetSystemRuntimeVerifier 的同名实现同模式。
    /// </summary>
    public sealed class LocalFileHttpTransport : IHttpTransport
    {
        private readonly GodotHttpTransport _fallback = new GodotHttpTransport();

        public Task<HttpResponse> GetTextAsync(string requestURL, int timeout, bool appendTimeTicks, CancellationToken cancellationToken)
        {
            if (TryResolveLocalFile(requestURL, out var localPath))
            {
                if (File.Exists(localPath) == false)
                {
                    return Task.FromResult(new HttpResponse { Success = false, StatusCode = 404, Error = "Local file not found: " + localPath });
                }

                return Task.FromResult(new HttpResponse { Success = true, StatusCode = 200, Text = File.ReadAllText(localPath, Encoding.UTF8) });
            }

            return _fallback.GetTextAsync(requestURL, timeout, appendTimeTicks, cancellationToken);
        }

        public Task<HttpResponse> GetDataAsync(string requestURL, int timeout, bool appendTimeTicks, CancellationToken cancellationToken)
        {
            if (TryResolveLocalFile(requestURL, out var localPath))
            {
                if (File.Exists(localPath) == false)
                {
                    return Task.FromResult(new HttpResponse { Success = false, StatusCode = 404, Error = "Local file not found: " + localPath });
                }

                return Task.FromResult(new HttpResponse { Success = true, StatusCode = 200, Data = File.ReadAllBytes(localPath) });
            }

            return _fallback.GetDataAsync(requestURL, timeout, appendTimeTicks, cancellationToken);
        }

        private static bool TryResolveLocalFile(string requestURL, out string localPath)
        {
            localPath = string.Empty;
            if (string.IsNullOrWhiteSpace(requestURL))
            {
                return false;
            }

            if (requestURL.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
            {
                var uri = new Uri(requestURL);
                localPath = uri.LocalPath;
                return true;
            }

            if (Path.IsPathRooted(requestURL))
            {
                localPath = requestURL;
                return true;
            }

            return false;
        }
    }
}
