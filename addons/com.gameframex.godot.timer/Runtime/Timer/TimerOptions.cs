namespace GameFrameX.Timer.Runtime
{
    /// <summary>
    /// 定时器模块启动配置（模块级 Options 样板，C9 配置终态）。
    /// </summary>
    /// <remarks>
    /// 默认值即开箱值（与原组件/静态字段默认值同源）；游戏在主场景加载前（自动启动引导）经 <c>GameFrameXOptions.Configure(new TimerOptions{...})</c> 注入差异值。
    /// <see cref="TimerManager"/> 首次实例化时一次性消费；运行期修改走 <see cref="TimerManager.CatchCallbackExceptions"/> 静态开关（实时型）。
    /// </remarks>
    public sealed class TimerOptions
    {
        /// <summary>
        /// 定时器回调异常捕获开关（默认关闭=异常向上抛出；开启后回调异常被捕获并输出 Warning）。
        /// </summary>
        /// <remarks>实时型：运行期可直接修改 <see cref="TimerManager.CatchCallbackExceptions"/>，立即生效。</remarks>
        public bool CatchCallbackExceptions = false;
    }
}
