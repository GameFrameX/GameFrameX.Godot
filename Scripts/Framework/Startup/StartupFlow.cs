using System;
using System.Threading.Tasks;
using GameFrameX.AssetSystem;
using GameFrameX.Runtime;
using GameFrameX.Startup.Runtime;
using Godot;

namespace GameFrameX.Startup.Application
{
    /// <summary>
    /// StartupRunner 启动编排：构造演示环境默认 <see cref="StartupOptions"/> 并驱动完整启动管线
    /// （版本请求 → 清单 → 下载 → 热补丁启动）。对应 Unity 侧 GameBootstrap 的启动接线。
    /// </summary>
    /// <remarks>
    /// 组件由自动 Host（GameFrameXRuntimeBootstrap autoload）注入，本编排只负责流程驱动；
    /// 调用时机必须在自动 Host 启动之后（场景脚本经 CallDeferred 调用即可满足）。
    /// 演示默认 <c>SkipRemoteStartupRequests = true</c>：无后端部署时跳过全部远程启动请求。
    /// </remarks>
    public static class StartupFlow
    {
        private static bool s_Running;

        /// <summary>
        /// 运行启动管线（幂等：进程内只跑一次）。
        /// </summary>
        /// <returns>启动结果。</returns>
        public static async Task<StartupResult> RunAsync()
        {
            if (s_Running)
            {
                Log.Warning("[StartupFlow] startup flow already requested, skipped.");
                return StartupResult.Succeed();
            }

            s_Running = true;
            Log.Info("[StartupFlow] startup pipeline begin.");
            var options = CreateDefaultOptions();
            var uiHandler = new GameStartupUIHandler();
            var hotfixLauncher = new HotfixAssemblyLauncher();

            StartupResult result;
            try
            {
                result = await StartupRunner.Run(options, uiHandler, hotfixLauncher);
            }
            catch (Exception exception)
            {
                result = StartupResult.Fail(nameof(StartupFlow), string.Empty, exception.Message);
            }

            if (result.Success)
            {
                Log.Info("[StartupFlow] startup completed.");
            }
            else
            {
                Log.Error("[StartupFlow] startup failed at {0}: {1}", result.FailedProcedureName, result.ErrorMessage);
            }

            return result;
        }

        private static StartupOptions CreateDefaultOptions()
        {
            var options = new StartupOptions
            {
                GamePlayMode = EPlayMode.EditorSimulateMode,
                // 演示环境无后端：跳过全部远程启动请求（全局信息 / App 版本 / 资源包版本）。
                SkipRemoteStartupRequests = true,
                PackageName = "DefaultPackage",
                Channel = "Default",
                SubChannel = "Default",
            };
            options.GlobalInfoUrls = new[] { "http://127.0.0.1/gameframex/global-info" };
            return options;
        }
    }
}
