using GameFrameX.Runtime;

namespace GameFrameX.Asset.Runtime
{
    /// <summary>
    /// 资源系统最小入口。
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public sealed class GfAssetSystem
    {
        private OperationScheduler _operationScheduler;

        /// <summary>
        /// 获取是否已初始化。
        /// </summary>
        public bool IsInitialized { get; private set; }

        /// <summary>
        /// 初始化资源系统。
        /// </summary>
        /// <param name="operationScheduler">可选调度器实例。</param>
        [UnityEngine.Scripting.Preserve]
        public void Initialize(OperationScheduler operationScheduler = null)
        {
            _operationScheduler = operationScheduler ?? new OperationScheduler();
            IsInitialized = true;
        }

        /// <summary>
        /// 驱动资源系统执行一帧。
        /// </summary>
        /// <param name="deltaSeconds">逻辑帧间隔秒数。</param>
        [UnityEngine.Scripting.Preserve]
        public void Tick(double deltaSeconds)
        {
            if (!IsInitialized)
            {
                Log.Warning("GfAssetSystem is not initialized.");
                return;
            }

            _operationScheduler.Tick(deltaSeconds);
        }
    }
}
