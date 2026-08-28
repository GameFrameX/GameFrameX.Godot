using System.Threading.Tasks;
using GameFrameX.AssetSystem;
using Godot;

namespace GameFrameX.Asset.Runtime
{
    /// <summary>
    /// 资源组件扩展（Sprite，对应 Unity Sprite）。
    /// </summary>
    public static class AssetComponentSpriteExtensions
    {
        /// <summary>
        /// 异步加载 Sprite 资源（Unity Sprite → Godot Texture2D；图集区域子资源请改用 AtlasTexture 泛型重载）
        /// </summary>
        /// <param name="assetComponent">资源组件</param>
        /// <param name="path">资源路径</param>
        /// <returns>资源句柄</returns>
        public static Task<AssetHandle> LoadSpriteAsync(this AssetComponent assetComponent, string path)
        {
            return assetComponent.LoadAssetAsync<Texture2D>(path);
        }

        /// <summary>
        /// 同步加载 Sprite 资源（Unity Sprite → Godot Texture2D；图集区域子资源请改用 AtlasTexture 泛型重载）
        /// </summary>
        /// <param name="assetComponent">资源组件</param>
        /// <param name="path">资源路径</param>
        /// <returns>资源句柄</returns>
        public static AssetHandle LoadSpriteSync(this AssetComponent assetComponent, string path)
        {
            return assetComponent.LoadAssetSync<Texture2D>(path);
        }
    }
}
