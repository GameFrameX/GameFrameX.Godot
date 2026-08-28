namespace GameFrameX.Timer.Runtime
{
    /// <summary>
    /// 定时器时间缩放模式。
    /// </summary>
    public enum TimerTimeScale : byte
    {
        /// <summary>不受 <see cref="Godot.Engine.TimeScale"/> 影响（默认，与现有行为兼容）。</summary>
        Unscaled = 0,

        /// <summary>受 <see cref="Godot.Engine.TimeScale"/> 影响。</summary>
        Scaled = 1,
    }
}
