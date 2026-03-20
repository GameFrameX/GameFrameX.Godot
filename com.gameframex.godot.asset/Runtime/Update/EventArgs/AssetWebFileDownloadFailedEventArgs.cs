using GameFrameX.Event.Runtime;
using GameFrameX.Runtime;

namespace GameFrameX.Asset.Runtime
{
    /// <summary>
    /// 网络文件下载失败
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public sealed class AssetWebFileDownloadFailedEventArgs : GameEventArgs
    {
        /// <summary>
        /// 文件名
        /// </summary>
        public string FileName { get; private set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string Error { get; private set; }

        /// <summary>
        /// 包名称
        /// </summary>
        public string PackageName { get; set; }

        public override void Clear()
        {
            FileName = null;
            Error = null;
            PackageName = null;
        }

        /// <summary>
        /// 网络文件下载失败事件编号
        /// </summary>
        public static readonly string EventId = typeof(AssetWebFileDownloadFailedEventArgs).FullName;

        public override string Id
        {
            get { return EventId; }
        }

        /// <summary>
        /// 创建网络文件下载失败
        /// </summary>
        /// <param name="packageName">包名称</param>
        /// <param name="fileName">文件名</param>
        /// <param name="error">错误信息</param>
        /// <returns></returns>
        public static AssetWebFileDownloadFailedEventArgs Create(string packageName, string fileName, string error)
        {
            var assetWebFileDownloadFailed = ReferencePool.Acquire<AssetWebFileDownloadFailedEventArgs>();
            assetWebFileDownloadFailed.FileName = fileName;
            assetWebFileDownloadFailed.Error = error;
            assetWebFileDownloadFailed.PackageName = packageName;
            return assetWebFileDownloadFailed;
        }
    }
}