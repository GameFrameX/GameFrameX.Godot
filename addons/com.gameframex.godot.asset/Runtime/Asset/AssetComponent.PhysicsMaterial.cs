using System.Threading.Tasks;
using GameFrameX.AssetSystem;
using Godot;

namespace GameFrameX.Asset.Runtime
{
    /// <summary>
    /// 资源组件扩展（PhysicsMaterial，对应 Unity PhysicsMaterial / PhysicsMaterial2D）。
    /// </summary>
    public static class AssetComponentPhysicsMaterialExtensions
    {
        /// <summary>
        /// 异步加载 PhysicsMaterial 资源（Unity PhysicsMaterial / PhysicsMaterial2D → Godot PhysicsMaterial，2D/3D 合一）
        /// </summary>
        /// <param name="assetComponent">资源组件</param>
        /// <param name="path">资源路径</param>
        /// <returns>资源句柄</returns>
        public static Task<AssetHandle> LoadPhysicsMaterialAsync(this AssetComponent assetComponent, string path)
        {
            return assetComponent.LoadAssetAsync<PhysicsMaterial>(path);
        }

        /// <summary>
        /// 同步加载 PhysicsMaterial 资源（Unity PhysicsMaterial / PhysicsMaterial2D → Godot PhysicsMaterial，2D/3D 合一）
        /// </summary>
        /// <param name="assetComponent">资源组件</param>
        /// <param name="path">资源路径</param>
        /// <returns>资源句柄</returns>
        public static AssetHandle LoadPhysicsMaterialSync(this AssetComponent assetComponent, string path)
        {
            return assetComponent.LoadAssetSync<PhysicsMaterial>(path);
        }
    }
}
