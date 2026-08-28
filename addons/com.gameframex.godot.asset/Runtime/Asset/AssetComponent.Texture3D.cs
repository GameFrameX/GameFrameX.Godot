using System.Threading.Tasks;
using GameFrameX.AssetSystem;
using Godot;

namespace GameFrameX.Asset.Runtime
{
    /// <summary>
    /// 资源组件扩展（Texture3D，对应 Unity Texture3D）。
    /// </summary>
    public static class AssetComponentTexture3DExtensions
    {
        /// <summary>
        /// 异步加载 Texture3D 资源（Unity Texture3D → Godot Texture3D）
        /// </summary>
        /// <param name="assetComponent">资源组件</param>
        /// <param name="path">资源路径</param>
        /// <returns>资源句柄</returns>
        public static Task<AssetHandle> LoadTexture3DAsync(this AssetComponent assetComponent, string path)
        {
            return assetComponent.LoadAssetAsync<Texture3D>(path);
        }

        /// <summary>
        /// 同步加载 Texture3D 资源（Unity Texture3D → Godot Texture3D）
        /// </summary>
        /// <param name="assetComponent">资源组件</param>
        /// <param name="path">资源路径</param>
        /// <returns>资源句柄</returns>
        public static AssetHandle LoadTexture3DSync(this AssetComponent assetComponent, string path)
        {
            return assetComponent.LoadAssetSync<Texture3D>(path);
        }
    }
}
