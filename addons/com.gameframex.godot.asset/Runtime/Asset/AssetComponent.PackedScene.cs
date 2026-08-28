using System.Threading.Tasks;
using GameFrameX.AssetSystem;
using Godot;

namespace GameFrameX.Asset.Runtime
{
    /// <summary>
    /// 资源组件扩展（PackedScene，对应 Unity prefab GameObject）。
    /// </summary>
    public static class AssetComponentPackedSceneExtensions
    {
        /// <summary>
        /// 异步加载 PackedScene 资源（Unity prefab → Godot PackedScene）
        /// </summary>
        /// <param name="assetComponent">资源组件</param>
        /// <param name="path">资源路径</param>
        /// <returns>资源句柄</returns>
        public static Task<AssetHandle> LoadPackedSceneAsync(this AssetComponent assetComponent, string path)
        {
            return assetComponent.LoadAssetAsync<PackedScene>(path);
        }

        /// <summary>
        /// 同步加载 PackedScene 资源（Unity prefab → Godot PackedScene）
        /// </summary>
        /// <param name="assetComponent">资源组件</param>
        /// <param name="path">资源路径</param>
        /// <returns>资源句柄</returns>
        public static AssetHandle LoadPackedSceneSync(this AssetComponent assetComponent, string path)
        {
            return assetComponent.LoadAssetSync<PackedScene>(path);
        }
    }
}
