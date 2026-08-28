using System.Threading.Tasks;
using GameFrameX.AssetSystem;
using Godot;

namespace GameFrameX.Asset.Runtime
{
    /// <summary>
    /// 资源组件扩展（Mesh，对应 Unity Mesh）。
    /// </summary>
    public static class AssetComponentMeshExtensions
    {
        /// <summary>
        /// 异步加载 Mesh 资源（Unity Mesh → Godot Mesh）
        /// </summary>
        /// <param name="assetComponent">资源组件</param>
        /// <param name="path">资源路径</param>
        /// <returns>资源句柄</returns>
        public static Task<AssetHandle> LoadMeshAsync(this AssetComponent assetComponent, string path)
        {
            return assetComponent.LoadAssetAsync<Mesh>(path);
        }

        /// <summary>
        /// 同步加载 Mesh 资源（Unity Mesh → Godot Mesh）
        /// </summary>
        /// <param name="assetComponent">资源组件</param>
        /// <param name="path">资源路径</param>
        /// <returns>资源句柄</returns>
        public static AssetHandle LoadMeshSync(this AssetComponent assetComponent, string path)
        {
            return assetComponent.LoadAssetSync<Mesh>(path);
        }
    }
}
