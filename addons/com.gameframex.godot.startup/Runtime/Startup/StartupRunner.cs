// ==========================================================================================
//  GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//  GameFrameX organization and its derivative projects' copyrights, trademarks, patents, and related rights
//  均受中华人民共和国及相关国际法律法规保护。
//  are protected by the laws of the People's Republic of China and relevant international regulations.
//
//  使用本项目须严格遵守相应法律法规及开源许可证之规定。
//  Usage of this project must strictly comply with applicable laws, regulations, and open-source licenses.
//
//  本项目采用 MIT 许可证与 Apache License 2.0 双许可证分发，
//  This project is dual-licensed under the MIT License and Apache License 2.0,
//  完整许可证文本请参见源代码根目录下的 LICENSE 文件。
//  please refer to the LICENSE file in the root directory of the source code for the full license text.
//
//  禁止利用本项目实施任何危害国家安全、破坏社会秩序、
//  It is prohibited to use this project to engage in any activities that endanger national security, disrupt social order,
//  侵犯他人合法权益等法律法规所禁止的行为！
//  or infringe upon the legitimate rights and interests of others, as prohibited by laws and regulations!
//  因基于本项目二次开发所产生的一切法律纠纷与责任，
//  Any legal disputes or liabilities arising from secondary development based on this project
//  本项目组织与贡献者概不承担。
//  shall be borne solely by the developer; the project organization and contributors assume no responsibility.
//
//  GitHub 仓库：https://github.com/GameFrameX
//  GitHub Repository: https://github.com/GameFrameX
//  Gitee  仓库：https://gitee.com/GameFrameX
//  Gitee Repository:  https://gitee.com/GameFrameX
//  CNB  仓库：https://cnb.cool/GameFrameX
//  CNB Repository: https://cnb.cool/GameFrameX
//  官方文档：https://gameframex.doc.alianblank.com/
//  Official Documentation: https://gameframex.doc.alianblank.com/
// ==========================================================================================

using System;
using System.Threading.Tasks;
using GameFrameX.Asset.Runtime;
using GameFrameX.AssetSystem;
using GameFrameX.Fsm.Runtime;
using GameFrameX.Procedure.Runtime;
using GameFrameX.Runtime;
using Godot.Startup.Procedure.Patch;
using Godot.Startup.Procedure.Startup;

namespace GameFrameX.Startup.Runtime
{
    /// <summary>
    /// 启动流程入口。提供静态 Run 方法，封装完整的启动流程。
    /// </summary>
    /// <remarks>
    /// - 同步配置校验
    /// - 在任何资源加载前同步资源运行模式到 AssetComponent
    /// - 通过 FSM BlackBoard 注入 options/uiHandler/hotfixLauncher/completionSource/httpParamsProvider
    /// - 启动 ProcedureLauncherState
    /// - 返回 Task&lt;StartupResult&gt;（成功/失败的 SetResult 由 Procedure 内部调用，不抛异常）
    /// </remarks>
    public static class StartupRunner
    {
        /// <summary>
        /// 启动完整启动流程。
        /// </summary>
        /// <param name="options">配置资产，提供资源模式、URL 列表、热更入口、HTTP 参数等。</param>
        /// <param name="uiHandler">UI 处理实现（应用层注入）。</param>
        /// <param name="hotfixLauncher">热更启动实现（应用层注入）。</param>
        /// <param name="httpParamsProvider">HTTP 公共参数提供器。</param>
        /// <returns>
        /// Task&lt;StartupResult&gt; — 永远会 complete（成功/失败都返回，不抛异常）。
        /// 调用方可 await 此 Task；同时流程结束时会通过 EventComponent.Fire 触发 StartupCompleted/FailedEventArgs。
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// options/uiHandler/hotfixLauncher 任一为 null 时同步抛出。
        /// </exception>
        /// <exception cref="ArgumentException">
        /// options.GlobalInfoUrls 为 null 或空数组时同步抛出（消息含 "GlobalInfoUrls"）。
        /// 此异常在 await 之前抛出，不进入 async 状态机。
        /// </exception>
        public static Task<StartupResult> Run(
            StartupOptions options,
            IStartupUIHandler uiHandler,
            IHotfixLauncher hotfixLauncher,
            IStartupHttpParamsProvider httpParamsProvider = null)
        {
            // 步骤 1: 同步配置校验（不进入 async 状态机）
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (uiHandler == null)
            {
                throw new ArgumentNullException(nameof(uiHandler));
            }

            if (hotfixLauncher == null)
            {
                throw new ArgumentNullException(nameof(hotfixLauncher));
            }

            if (options.GlobalInfoUrls == null || options.GlobalInfoUrls.Length == 0)
            {
                throw new ArgumentException(
                    "StartupOptions.GlobalInfoUrls must contain at least one URL.",
                    nameof(StartupOptions.GlobalInfoUrls));
            }

            // 步骤 2: 在任何资源加载前同步资源运行模式。
            ApplyAssetPlayMode(options);

            // 步骤 3: 创建 TaskCompletionSource（成功/失败的 SetResult 由 Procedure 在流程结束时调用）
            var tcs = new TaskCompletionSource<StartupResult>();

            // 步骤 4: 获取框架模块
            var fsmManager = GameFrameworkEntry.GetModule<IFsmManager>();
            var procedureManager = GameFrameworkEntry.GetModule<IProcedureManager>();

            // 步骤 5: Initialize 内部由 procedureManager 创建 procedure FSM
            procedureManager.Initialize(fsmManager, new ProcedureBase[]
            {
                new ProcedureLauncherState(),
                new ProcedureGetGlobalInfoState(),
                new ProcedureGetAppVersionInfoState(),
                new ProcedureGetGameAssetPackageVersionInfoByDefaultPackageState(),
                new ProcedurePatchInit(),
                new ProcedureUpdateStaticVersion(),
                new ProcedureUpdateManifest(),
                new ProcedureCreateDownloader(),
                new ProcedureDownloadWebFiles(),
                new ProcedurePatchDone(),
                new ProcedureGameLauncherState(),
            });

            // 步骤 6: 通过 IFsmManager.GetFsm<T>() 取回刚创建的 procedure FSM
            var procedureFsm = fsmManager.GetFsm<IProcedureManager>();

            // 步骤 7: 注入 BlackBoard key（VarObject 无隐式转换，显式 Acquire + 赋 Value）
            var optionsBox = ReferencePool.Acquire<VarObject>();
            optionsBox.Value = options;
            procedureFsm.SetData(BlackBoardKeys.StartupOptions, optionsBox);

            var uiHandlerBox = ReferencePool.Acquire<VarObject>();
            uiHandlerBox.Value = uiHandler;
            procedureFsm.SetData(BlackBoardKeys.StartupUIHandler, uiHandlerBox);

            var hotfixLauncherBox = ReferencePool.Acquire<VarObject>();
            hotfixLauncherBox.Value = hotfixLauncher;
            procedureFsm.SetData(BlackBoardKeys.StartupHotfixLauncher, hotfixLauncherBox);

            var completionSourceBox = ReferencePool.Acquire<VarObject>();
            completionSourceBox.Value = tcs;
            procedureFsm.SetData(BlackBoardKeys.StartupCompletionSource, completionSourceBox);

            var httpParamsProviderBox = ReferencePool.Acquire<VarObject>();
            httpParamsProviderBox.Value = httpParamsProvider ?? new DefaultStartupHttpParamsProvider();
            procedureFsm.SetData(BlackBoardKeys.StartupHttpParamsProvider, httpParamsProviderBox);

            // 步骤 8: 启动第一个 Procedure 状态
            procedureManager.StartProcedure<ProcedureLauncherState>();

            // 步骤 9: 返回 tcs.Task（fire-and-forget 启动后立即返回；tcs 由 Procedure 在流程结束时完成）
            return tcs.Task;
        }

        private static void ApplyAssetPlayMode(StartupOptions options)
        {
            GameEntry.GetComponent<AssetComponent>().GamePlayMode = NormalizeAssetPlayMode(options.GamePlayMode);
        }

        private static EPlayMode NormalizeAssetPlayMode(EPlayMode playMode)
        {
            // 说明：Godot 运行时无编辑器模拟上下文，与 AssetComponent._Ready 的回退规则一致：
            // 运行时 EditorSimulateMode → HostPlayMode；Web 平台 → WebPlayMode（对应 Unity 的 UNITY_WEBGL 分支）。
            if (!ApplicationHelper.IsEditor)
            {
                if (playMode == EPlayMode.EditorSimulateMode)
                {
                    playMode = EPlayMode.HostPlayMode;
                }

                if (ApplicationHelper.IsWebGL)
                {
                    playMode = EPlayMode.WebPlayMode;
                }
            }

            return playMode;
        }
    }
}
