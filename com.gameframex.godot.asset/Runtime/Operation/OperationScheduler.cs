namespace GameFrameX.Asset.Runtime
{
    /// <summary>
    /// 操作调度器最小实现。
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public sealed class OperationScheduler
    {
        /// <summary>
        /// 获取累计调度帧数。
        /// </summary>
        public long TickCount { get; private set; }

        /// <summary>
        /// 获取最后一次调度增量时间。
        /// </summary>
        public double LastDeltaSeconds { get; private set; }

        /// <summary>
        /// 执行一次调度。
        /// </summary>
        /// <param name="deltaSeconds">逻辑帧间隔秒数。</param>
        [UnityEngine.Scripting.Preserve]
        public void Tick(double deltaSeconds)
        {
            LastDeltaSeconds = deltaSeconds;
            TickCount++;
        }
    }
}
