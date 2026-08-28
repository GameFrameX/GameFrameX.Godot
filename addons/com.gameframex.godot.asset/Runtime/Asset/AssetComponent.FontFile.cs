using System.Threading.Tasks;
using GameFrameX.AssetSystem;
using Godot;

namespace GameFrameX.Asset.Runtime
{
    /// <summary>
    /// 资源组件扩展（FontFile，对应 Unity Font / TMP_FontAsset）。
    /// </summary>
    public static class AssetComponentFontFileExtensions
    {
        /// <summary>
        /// 异步加载 FontFile 资源（Unity Font → Godot Font 基类下选 FontFile；FontVariation 资源请使用泛型重载自行指定）
        /// </summary>
        /// <param name="assetComponent">资源组件</param>
        /// <param name="path">资源路径</param>
        /// <returns>资源句柄</returns>
        public static Task<AssetHandle> LoadFontFileAsync(this AssetComponent assetComponent, string path)
        {
            return assetComponent.LoadAssetAsync<FontFile>(path);
        }

        /// <summary>
        /// 同步加载 FontFile 资源（Unity Font → Godot Font 基类下选 FontFile；FontVariation 资源请使用泛型重载自行指定）
        /// </summary>
        /// <param name="assetComponent">资源组件</param>
        /// <param name="path">资源路径</param>
        /// <returns>资源句柄</returns>
        public static AssetHandle LoadFontFileSync(this AssetComponent assetComponent, string path)
        {
            return assetComponent.LoadAssetSync<FontFile>(path);
        }
    }
}
