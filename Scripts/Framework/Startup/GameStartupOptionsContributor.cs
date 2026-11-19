using GameFrameX.Procedure.Runtime;
using GameFrameX.Runtime;

namespace GameFrameX.Startup.Application
{
    /// <summary>
    /// 演示工程的框架 Options 贡献者：被运行时扫描静默发现并在任何模块实例化前应用。
    /// </summary>
    /// <remarks>
    /// 流程组件声明由 StartupRunner 接管（组件自启动路径让位）；正式项目在此集中注入各模块差异值
    /// （对应 Unity 版 GameFrameXOptions.Configure 的 BeforeSceneLoad 注入时机）。
    /// </remarks>
    public sealed class GameStartupOptionsContributor : IGameFrameXOptionsContributor
    {
        /// <summary>
        /// 向上下文写入本工程需要的 Options 默认值。
        /// </summary>
        /// <param name="context">Options 贡献者上下文。</param>
        public void ConfigureOptions(GameFrameXOptionsContributorContext context)
        {
            context.Configure(new ProcedureOptions { UseStartupRunner = true });
        }
    }
}
