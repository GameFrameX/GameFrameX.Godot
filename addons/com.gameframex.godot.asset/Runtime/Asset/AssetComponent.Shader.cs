using System.Threading.Tasks;
using GameFrameX.AssetSystem;
using Godot;

namespace GameFrameX.Asset.Runtime
{
    /// <summary>
    /// 资源组件扩展（Shader，对应 Unity Shader / ShaderVariantCollection）。
    /// </summary>
    public static class AssetComponentShaderExtensions
    {
        /// <summary>
        /// 异步加载 Shader 资源（Unity Shader → Godot Shader）
        /// </summary>
        /// <param name="assetComponent">资源组件</param>
        /// <param name="path">资源路径</param>
        /// <returns>资源句柄</returns>
        public static Task<AssetHandle> LoadShaderAsync(this AssetComponent assetComponent, string path)
        {
            return assetComponent.LoadAssetAsync<Shader>(path);
        }

        /// <summary>
        /// 同步加载 Shader 资源（Unity Shader → Godot Shader）
        /// </summary>
        /// <param name="assetComponent">资源组件</param>
        /// <param name="path">资源路径</param>
        /// <returns>资源句柄</returns>
        public static AssetHandle LoadShaderSync(this AssetComponent assetComponent, string path)
        {
            return assetComponent.LoadAssetSync<Shader>(path);
        }
    }
}
