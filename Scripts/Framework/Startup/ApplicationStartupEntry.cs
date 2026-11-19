using System;
using Godot;

namespace GameFrameX.Startup.Application
{
    /// <summary>
    /// 代码驱动的启动入口（LauncherAuto 场景根脚本）。
    /// </summary>
    /// <remarks>
    /// 框架组件由自动 Host（GameFrameXRuntimeBootstrap autoload）注入，本入口不再手工构建组件链；
    /// 仅在自动 Host 就绪后驱动 StartupRunner 启动管线（版本请求 → 清单 → 下载 → 热补丁启动）。
    /// 对应 Unity 侧 GameBootstrap：await StartupRunner.Run(options, uiHandler, hotfixLauncher)。
    /// </remarks>
    public sealed partial class ApplicationStartupEntry : Node
    {
        public override void _Ready()
        {
            base._Ready();
            // 延迟到空闲帧：自动 Host 在同一批延迟回调中创建（autoload 先于场景排队），
            // 本回调执行时全部组件已注册完毕。
            CallDeferred(nameof(RunStartupFlow));
        }

        private async void RunStartupFlow()
        {
            try
            {
                await StartupFlow.RunAsync();
            }
            catch (Exception exception)
            {
                GD.PushError($"[ApplicationStartupEntry] startup flow crashed: {exception}");
            }
        }
    }
}
