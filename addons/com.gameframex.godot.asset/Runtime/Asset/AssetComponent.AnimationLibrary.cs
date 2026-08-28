using System.Threading.Tasks;
using GameFrameX.AssetSystem;
using Godot;

namespace GameFrameX.Asset.Runtime
{
    /// <summary>
    /// 资源组件扩展（AnimationLibrary，对应 Unity AnimatorController）。
    /// </summary>
    public static class AssetComponentAnimationLibraryExtensions
    {
        /// <summary>
        /// 异步加载 AnimationLibrary 资源（Unity AnimatorController → Godot AnimationLibrary）
        /// </summary>
        /// <param name="assetComponent">资源组件</param>
        /// <param name="path">资源路径</param>
        /// <returns>资源句柄</returns>
        public static Task<AssetHandle> LoadAnimationLibraryAsync(this AssetComponent assetComponent, string path)
        {
            return assetComponent.LoadAssetAsync<AnimationLibrary>(path);
        }

        /// <summary>
        /// 同步加载 AnimationLibrary 资源（Unity AnimatorController → Godot AnimationLibrary）
        /// </summary>
        /// <param name="assetComponent">资源组件</param>
        /// <param name="path">资源路径</param>
        /// <returns>资源句柄</returns>
        public static AssetHandle LoadAnimationLibrarySync(this AssetComponent assetComponent, string path)
        {
            return assetComponent.LoadAssetSync<AnimationLibrary>(path);
        }
    }
}
