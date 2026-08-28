using System.Threading.Tasks;
using GameFrameX.AssetSystem;
using Godot;

namespace GameFrameX.Asset.Runtime
{
    /// <summary>
    /// 资源组件扩展（AudioStream，对应 Unity AudioClip）。
    /// </summary>
    public static class AssetComponentAudioStreamExtensions
    {
        /// <summary>
        /// 异步加载 AudioStream 资源（Unity AudioClip → Godot AudioStream）
        /// </summary>
        /// <param name="assetComponent">资源组件</param>
        /// <param name="path">资源路径</param>
        /// <returns>资源句柄</returns>
        public static Task<AssetHandle> LoadAudioStreamAsync(this AssetComponent assetComponent, string path)
        {
            return assetComponent.LoadAssetAsync<AudioStream>(path);
        }

        /// <summary>
        /// 同步加载 AudioStream 资源（Unity AudioClip → Godot AudioStream）
        /// </summary>
        /// <param name="assetComponent">资源组件</param>
        /// <param name="path">资源路径</param>
        /// <returns>资源句柄</returns>
        public static AssetHandle LoadAudioStreamSync(this AssetComponent assetComponent, string path)
        {
            return assetComponent.LoadAssetSync<AudioStream>(path);
        }
    }
}
