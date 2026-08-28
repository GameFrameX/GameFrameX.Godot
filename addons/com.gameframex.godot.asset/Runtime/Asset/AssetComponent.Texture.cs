using System.Threading.Tasks;
using GameFrameX.AssetSystem;
using Godot;

namespace GameFrameX.Asset.Runtime
{
    /// <summary>
    /// 资源组件扩展（Texture，对应 Unity Texture）。
    /// </summary>
    public static class AssetComponentTextureExtensions
    {
        /// <summary>
        /// 异步加载 Texture 资源（Unity Texture → Godot Texture 基类，具体子类型由加载器解析）
        /// </summary>
        /// <param name="assetComponent">资源组件</param>
        /// <param name="path">资源路径</param>
        /// <returns>资源句柄</returns>
        public static Task<AssetHandle> LoadTextureAsync(this AssetComponent assetComponent, string path)
        {
            return assetComponent.LoadAssetAsync<Texture>(path);
        }

        /// <summary>
        /// 同步加载 Texture 资源（Unity Texture → Godot Texture 基类，具体子类型由加载器解析）
        /// </summary>
        /// <param name="assetComponent">资源组件</param>
        /// <param name="path">资源路径</param>
        /// <returns>资源句柄</returns>
        public static AssetHandle LoadTextureSync(this AssetComponent assetComponent, string path)
        {
            return assetComponent.LoadAssetSync<Texture>(path);
        }
    }
}
