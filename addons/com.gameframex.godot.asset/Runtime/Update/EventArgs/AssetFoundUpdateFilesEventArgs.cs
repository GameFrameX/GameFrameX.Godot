using GameFrameX.Event.Runtime;
using GameFrameX.Runtime;

namespace GameFrameX.Asset.Runtime
{
    /// <summary>
    /// 发现更新文件
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public sealed class AssetFoundUpdateFilesEventArgs : GameEventArgs
    {
        /// <summary>
        /// 总数量
        /// </summary>
        public int TotalCount { get; private set; }

        /// <summary>
        /// 总大小
        /// </summary>
        public long TotalSizeBytes { get; private set; }

        /// <summary>
        /// 包名称
        /// </summary>
        public string PackageName { get; set; }

        public override void Clear()
        {
            PackageName = null;
            TotalCount = 0;
            TotalSizeBytes = 0;
        }

        /// <summary>
        /// 发现更新文件事件编号
        /// </summary>
        public static readonly string EventId = typeof(AssetFoundUpdateFilesEventArgs).FullName;

        public override string Id
        {
            get { return EventId; }
        }

        /// <summary>
        /// 创建发现更新文件
        /// </summary>
        /// <param name="packageName">包名称</param>
        /// <param name="totalCount">总数量</param>
        /// <param name="totalSizeBytes">总大小</param>
        /// <returns></returns>
        public static AssetFoundUpdateFilesEventArgs Create(string packageName, int totalCount, long totalSizeBytes)
        {
            var foundUpdateFiles = ReferencePool.Acquire<AssetFoundUpdateFilesEventArgs>();
            foundUpdateFiles.TotalCount = totalCount;
            foundUpdateFiles.TotalSizeBytes = totalSizeBytes;
            foundUpdateFiles.PackageName = packageName;
            return foundUpdateFiles;
        }
    }
}