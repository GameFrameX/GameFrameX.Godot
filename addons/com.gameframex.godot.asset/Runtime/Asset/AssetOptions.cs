using GameFrameX.AssetSystem;
using GameFrameX.Runtime;

namespace GameFrameX.Asset.Runtime
{
    /// <summary>
    /// 资源模块启动配置（模块级 Options）。
    /// </summary>
    /// <remarks>
    /// 默认值即开箱值（与原 AssetComponent 序列化字段默认值同源）；游戏在首场景加载前经 <c>GameFrameXOptions.Configure(new AssetOptions{...})</c> 注入差异值，
    /// 由 <see cref="AssetComponent"/> 在初始化时一次性消费。
    /// </remarks>
    public sealed class AssetOptions
    {
        /// <summary>
        /// 资源的运行模式（默认编辑器模拟模式，与原组件字段默认值同源）。
        /// </summary>
        /// <remarks>启动期型：仅在组件初始化资源管理器时消费，运行期不可修改。</remarks>
        [GameFrameXStartupOnly]
        public EPlayMode GamePlayMode = EPlayMode.EditorSimulateMode;
    }
}
