using System.IO;
using GameFrameX.AssetSystem;

namespace GameFrameX.EngineTests
{
    /// <summary>
    /// 本地目录 IRemoteServices：把远端 URL 映射到 fixture 包根目录，
    /// HostPlayMode 的版本/清单/资源包"下载"全部退化为本地文件读取。
    /// </summary>
    public sealed class LocalDirectoryRemoteServices : IRemoteServices
    {
        private readonly string _mainDirectory;

        public LocalDirectoryRemoteServices(string mainDirectory)
        {
            _mainDirectory = mainDirectory ?? string.Empty;
        }

        public string GetRemoteMainURL(string fileName, string packageVersion)
        {
            return Path.Combine(_mainDirectory, fileName).Replace('\\', '/');
        }

        public string GetRemoteFallbackURL(string fileName, string packageVersion)
        {
            return GetRemoteMainURL(fileName, packageVersion);
        }
    }
}
