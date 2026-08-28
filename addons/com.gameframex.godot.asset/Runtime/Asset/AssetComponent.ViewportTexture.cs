using System.Threading.Tasks;
using GameFrameX.AssetSystem;
using Godot;

namespace GameFrameX.Asset.Runtime
{
    /// <summary>
    /// 资源组件扩展（ViewportTexture，对应 Unity RenderTexture）。
    /// </summary>
    public static class AssetComponentViewportTextureExtensions
    {
        /// <summary>
        /// 异步加载 ViewportTexture 资源（Unity RenderTexture → Godot ViewportTexture）
        /// </summary>
        /// <param name="assetComponent">资源组件</param>
        /// <param name="path">资源路径</param>
        /// <returns>资源句柄</returns>
        public static Task<AssetHandle> LoadViewportTextureAsync(this AssetComponent assetComponent, string path)
        {
            return assetComponent.LoadAssetAsync<ViewportTexture>(path);
        }

        /// <summary>
        /// 同步加载 ViewportTexture 资源（Unity RenderTexture → Godot ViewportTexture）
        /// </summary>
        /// <param name="assetComponent">资源组件</param>
        /// <param name="path">资源路径</param>
        /// <returns>资源句柄</returns>
        public static AssetHandle LoadViewportTextureSync(this AssetComponent assetComponent, string path)
        {
            return assetComponent.LoadAssetSync<ViewportTexture>(path);
        }
    }
}
