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
using GameFrameX.GlobalConfig.Runtime;
using GameFrameX.Procedure.Runtime;
using GameFrameX.Runtime;
using GameFrameX.Startup.Runtime;
using GameFrameX.Web.Runtime;

namespace Godot.Startup.Procedure.Startup
{
    /// <summary>
    /// 获取 App 版本信息流程。向服务器请求 App 版本信息并检查是否需要升级。
    /// </summary>
    /// <remarks>
    /// Get app version info state procedure. Requests app version info from server and checks if upgrade is required.
    /// </remarks>
    public sealed class ProcedureGetAppVersionInfoState : ProcedureBase
    {
        /// <inheritdoc />
        protected internal override async void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            var assetComponent = GameEntry.GetComponent<AssetComponent>();
            if (assetComponent.GamePlayMode == EPlayMode.EditorSimulateMode)
            {
                Log.Info("Editor simulate mode, skip app version request.");
                ChangeState<Godot.Startup.Procedure.Patch.ProcedurePatchInit>(procedureOwner);
                return;
            }

            await GetAppVersionInfoAsync(procedureOwner);
        }

        /// <summary>
        /// 异步获取 App 版本信息。
        /// </summary>
        /// <remarks>
        /// Asynchronously retrieves app version info from server with retry support. Shows upgrade dialog if update is available.
        /// </remarks>
        /// <param name="procedureOwner">流程所有者 / Procedure owner</param>
        /// <returns>获取完成的任务 / Retrieval completion task</returns>
        private async Task GetAppVersionInfoAsync(IFsm<IProcedureManager> procedureOwner)
        {
            var options = StartupProcedureUtility.GetOptions(procedureOwner);
            var uiHandler = StartupProcedureUtility.GetUIHandler(procedureOwner);
            var httpParamsProvider = StartupProcedureUtility.GetHttpParamsProvider(procedureOwner);
            var jsonParams = StartupProcedureUtility.CreateHttpParams(options, httpParamsProvider);

            for (var retryIndex = 1; retryIndex <= options.MaxAttemptsPerUrl; retryIndex++)
            {
                try
                {
                    var json = await GameEntry.GetComponent<WebComponent>().PostToString(GameEntry.GetComponent<GlobalConfigComponent>().CheckAppVersionUrl, jsonParams);
                    var httpJsonResult = Utility.Json.ToObject<HttpJsonResult>(json.Result);
                    if (httpJsonResult.Code <= 0)
                    {
                        var gameAppVersion = Utility.Json.ToObject<ResponseGameAppVersion>(httpJsonResult.Data);
                        StartupNetworkCacheUtility.SaveAppVersionInfo(httpJsonResult.Data);
                        await ApplyAppVersionInfoAsync(procedureOwner, uiHandler, gameAppVersion);
                        return;
                    }

                    Log.Error("Get app version returned code " + httpJsonResult.Code);
                }
                catch (Exception exception)
                {
                    Log.Error(exception);
                }

                if (retryIndex >= options.MaxAttemptsPerUrl)
                {
                    if (StartupNetworkCacheUtility.TryGetCachedAppVersionInfo(out var cachedGameAppVersion))
                    {
                        if (uiHandler != null)
                        {
                            uiHandler.SetTipText("Using cached app version...");
                        }

                        await ApplyAppVersionInfoAsync(procedureOwner, uiHandler, cachedGameAppVersion);
                        return;
                    }

                    StartupProcedureUtility.CompleteFailure(procedureOwner, nameof(ProcedureGetAppVersionInfoState), GameEntry.GetComponent<GlobalConfigComponent>().CheckAppVersionUrl, "Failed to get app version info.");
                    return;
                }

                if (uiHandler != null)
                {
                    uiHandler.SetTipText("Server error, retrying... (" + retryIndex + "/" + options.MaxAttemptsPerUrl + ")");
                }

                await Task.Delay(options.RetryDelayMs);
            }
        }

        private async Task ApplyAppVersionInfoAsync(
            IFsm<IProcedureManager> procedureOwner,
            IStartupUIHandler uiHandler,
            ResponseGameAppVersion gameAppVersion)
        {
            if (gameAppVersion.IsUpgrade)
            {
                var shouldContinue = await uiHandler.ShowUpgradeAsync(new StartupUpgradeInfo(
                                                                          gameAppVersion.IsForce,
                                                                          gameAppVersion.AppDownloadUrl,
                                                                          gameAppVersion.UpdateTitle,
                                                                          gameAppVersion.UpdateAnnouncement));

                if (!shouldContinue)
                {
                    return;
                }
            }

            ChangeState<ProcedureGetGameAssetPackageVersionInfoByDefaultPackageState>(procedureOwner);
        }
    }
}
