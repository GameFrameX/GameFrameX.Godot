using System.Threading.Tasks;
using GameFrameX.AssetSystem;
using Godot;

namespace GameFrameX.Asset.Runtime
{
    /// <summary>
    /// 资源组件扩展（Cubemap，对应 Unity Cubemap）。
    /// </summary>
    public static class AssetComponentCubemapExtensions
    {
        /// <summary>
        /// 异步加载 Cubemap 资源（Unity Cubemap → Godot Cubemap）
        /// </summary>
        /// <param name="assetComponent">资源组件</param>
        /// <param name="path">资源路径</param>
        /// <returns>资源句柄</returns>
        public static Task<AssetHandle> LoadCubemapAsync(this AssetComponent assetComponent, string path)
        {
            return assetComponent.LoadAssetAsync<Cubemap>(path);
        }

        /// <summary>
        /// 同步加载 Cubemap 资源（Unity Cubemap → Godot Cubemap）
        /// </summary>
        /// <param name="assetComponent">资源组件</param>
        /// <param name="path">资源路径</param>
        /// <returns>资源句柄</returns>
        public static AssetHandle LoadCubemapSync(this AssetComponent assetComponent, string path)
        {
            return assetComponent.LoadAssetSync<Cubemap>(path);
        }
    }
}
