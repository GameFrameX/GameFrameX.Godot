using GameFrameX.Event.Runtime;
using GameFrameX.Runtime;

namespace GameFrameX.Asset.Runtime
{
    /// <summary>
    /// 补丁清单更新失败
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public sealed class AssetPatchManifestUpdateFailedEventArgs : GameEventArgs
    {
        public override void Clear()
        {
            PackageName = null;
            Error = null;
        }

        /// <summary>
        /// 补丁清单更新失败事件编号
        /// </summary>
        public static readonly string EventId = typeof(AssetPatchManifestUpdateFailedEventArgs).FullName;

        public override string Id
        {
            get { return EventId; }
        }

        /// <summary>
        /// 包名称
        /// </summary>
        public string PackageName { get; private set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string Error { get; private set; }

        /// <summary>
        /// 创建补丁清单更新失败
        /// </summary>
        /// <param name="packageName">包名称</param>
        /// <param name="error">错误信息</param>
        /// <returns></returns>
        public static AssetPatchManifestUpdateFailedEventArgs Create(string packageName, string error)
        {
            var assetPatchManifestUpdateFailed = ReferencePool.Acquire<AssetPatchManifestUpdateFailedEventArgs>();
            assetPatchManifestUpdateFailed.PackageName = packageName;
            assetPatchManifestUpdateFailed.Error = error;
            return assetPatchManifestUpdateFailed;
        }
    }
}