using System.Threading.Tasks;
using GameFrameX.AssetSystem;
using Godot;

namespace GameFrameX.Asset.Runtime
{
    /// <summary>
    /// 资源组件扩展（Texture2D，对应 Unity Texture2D）。
    /// </summary>
    public static class AssetComponentTexture2DExtensions
    {
        /// <summary>
        /// 异步加载 Texture2D 资源（Unity Texture2D → Godot Texture2D）
        /// </summary>
        /// <param name="assetComponent">资源组件</param>
        /// <param name="path">资源路径</param>
        /// <returns>资源句柄</returns>
        public static Task<AssetHandle> LoadTexture2DAsync(this AssetComponent assetComponent, string path)
        {
            return assetComponent.LoadAssetAsync<Texture2D>(path);
        }

        /// <summary>
        /// 同步加载 Texture2D 资源（Unity Texture2D → Godot Texture2D）
        /// </summary>
        /// <param name="assetComponent">资源组件</param>
        /// <param name="path">资源路径</param>
        /// <returns>资源句柄</returns>
        public static AssetHandle LoadTexture2DSync(this AssetComponent assetComponent, string path)
        {
            return assetComponent.LoadAssetSync<Texture2D>(path);
        }
    }
}
