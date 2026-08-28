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
using Godot.Startup.Procedure.Patch;

namespace Godot.Startup.Procedure.Startup
{
    /// <summary>
    /// 获取全局配置信息流程。使用 URL 故障转移机制从服务器获取全局配置信息。
    /// </summary>
    /// <remarks>
    /// Get global info state procedure. Retrieves global configuration from server using URL failover mechanism.
    /// </remarks>
    public sealed class ProcedureGetGlobalInfoState : ProcedureBase
    {
        /// <inheritdoc />
        protected internal override async void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            var assetComponent = GameEntry.GetComponent<AssetComponent>();
            if (assetComponent.GamePlayMode == EPlayMode.EditorSimulateMode)
            {
                Log.Info("Editor simulate mode, skip global info request.");
                ChangeState<ProcedureGetAppVersionInfoState>(procedureOwner);
                return;
            }

            if (assetComponent.GamePlayMode == EPlayMode.OfflinePlayMode)
            {
                Log.Info("Offline play mode, skip remote startup requests.");
                ChangeState<ProcedurePatchInit>(procedureOwner);
                return;
            }

            var options = StartupProcedureUtility.GetOptions(procedureOwner);
            if (options != null && options.SkipRemoteStartupRequests)
            {
                Log.Info("Skip remote startup requests option enabled, skip remote startup requests.");
                ChangeState<ProcedurePatchInit>(procedureOwner);
                return;
            }

            await GetGlobalInfoAsync(procedureOwner);
        }

        /// <summary>
        /// 异步获取全局配置信息。
        /// </summary>
        /// <remarks>
        /// Asynchronously retrieves global configuration using URL failover runner. Updates global config on success.
        /// </remarks>
        /// <param name="procedureOwner">流程所有者 / Procedure owner</param>
        /// <returns>获取完成的任务 / Retrieval completion task</returns>
        private async Task GetGlobalInfoAsync(IFsm<IProcedureManager> procedureOwner)
        {
            var options = StartupProcedureUtility.GetOptions(procedureOwner);
            var uiHandler = StartupProcedureUtility.GetUIHandler(procedureOwner);
            var httpParamsProvider = StartupProcedureUtility.GetHttpParamsProvider(procedureOwner);
            var jsonParams = StartupProcedureUtility.CreateHttpParams(options, httpParamsProvider);
            var result = await UrlFailoverRunner.ExecuteAsync(
                             options.GlobalInfoUrls,
                             options.MaxAttemptsPerUrl,
                             options.RetryDelayMs,
                             async url =>
                             {
                                 try
                                 {
                                     var json = await GameEntry.GetComponent<WebComponent>().PostToString(url, jsonParams);
                                     var responseGlobalInfo = json.Result.ToHttpJsonResultData<ResponseGlobalInfo>();
                                     if (!responseGlobalInfo.IsSuccess)
                                     {
                                         return UrlAttemptResult.Fail("Global info server returned code " + responseGlobalInfo.Code);
                                     }

                                     StartupNetworkCacheUtility.ApplyGlobalInfo(json.Result, responseGlobalInfo.Data);
                                     StartupNetworkCacheUtility.SaveGlobalInfo(json.Result);
                                     return UrlAttemptResult.Succeed();
                                 }
                                 catch (Exception exception)
                                 {
                                     Log.Error(exception);
                                     return UrlAttemptResult.Fail(exception.Message);
                                 }
                             },
                             (url, attempt, total) =>
                             {
                                 if (uiHandler != null)
                                 {
                                     uiHandler.SetTipText("Loading... (" + attempt + "/" + total + ")");
                                 }
                             });

            if (!result.Success)
            {
                if (StartupNetworkCacheUtility.TryApplyCachedGlobalInfo())
                {
                    if (uiHandler != null)
                    {
                        uiHandler.SetTipText("Using cached startup config...");
                    }

                    ChangeState<ProcedureGetAppVersionInfoState>(procedureOwner);
                    return;
                }

                StartupProcedureUtility.CompleteFailure(
                    procedureOwner,
                    nameof(ProcedureGetGlobalInfoState),
                    result.FailedUrl,
                    result.ErrorMessage);
                return;
            }


            if (uiHandler != null)
            {
                uiHandler.SetTipText("Loading...");
            }

            ChangeState<ProcedureGetAppVersionInfoState>(procedureOwner);
        }
    }
}
