using System.Threading.Tasks;
using GameFrameX.AssetSystem;
using Godot;

namespace GameFrameX.Asset.Runtime
{
    /// <summary>
    /// 资源组件扩展（Animation，对应 Unity AnimationClip）。
    /// </summary>
    public static class AssetComponentAnimationExtensions
    {
        /// <summary>
        /// 异步加载 Animation 资源（Unity AnimationClip → Godot Animation）
        /// </summary>
        /// <param name="assetComponent">资源组件</param>
        /// <param name="path">资源路径</param>
        /// <returns>资源句柄</returns>
        public static Task<AssetHandle> LoadAnimationAsync(this AssetComponent assetComponent, string path)
        {
            return assetComponent.LoadAssetAsync<Animation>(path);
        }

        /// <summary>
        /// 同步加载 Animation 资源（Unity AnimationClip → Godot Animation）
        /// </summary>
        /// <param name="assetComponent">资源组件</param>
        /// <param name="path">资源路径</param>
        /// <returns>资源句柄</returns>
        public static AssetHandle LoadAnimationSync(this AssetComponent assetComponent, string path)
        {
            return assetComponent.LoadAssetSync<Animation>(path);
        }
    }
}
