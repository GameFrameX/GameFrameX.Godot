using System.Threading.Tasks;
using GameFrameX.AssetSystem;
using Godot;

namespace GameFrameX.Asset.Runtime
{
    /// <summary>
    /// 资源组件扩展（Material，对应 Unity Material）。
    /// </summary>
    public static class AssetComponentMaterialExtensions
    {
        /// <summary>
        /// 异步加载 Material 资源（Unity Material → Godot Material）
        /// </summary>
        /// <param name="assetComponent">资源组件</param>
        /// <param name="path">资源路径</param>
        /// <returns>资源句柄</returns>
        public static Task<AssetHandle> LoadMaterialAsync(this AssetComponent assetComponent, string path)
        {
            return assetComponent.LoadAssetAsync<Material>(path);
        }

        /// <summary>
        /// 同步加载 Material 资源（Unity Material → Godot Material）
        /// </summary>
        /// <param name="assetComponent">资源组件</param>
        /// <param name="path">资源路径</param>
        /// <returns>资源句柄</returns>
        public static AssetHandle LoadMaterialSync(this AssetComponent assetComponent, string path)
        {
            return assetComponent.LoadAssetSync<Material>(path);
        }
    }
}
