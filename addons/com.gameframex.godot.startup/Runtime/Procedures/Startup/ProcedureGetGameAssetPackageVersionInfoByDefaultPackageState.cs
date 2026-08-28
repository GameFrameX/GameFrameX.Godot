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
using GameFrameX.Fsm.Runtime;
using GameFrameX.GlobalConfig.Runtime;
using GameFrameX.Procedure.Runtime;
using GameFrameX.Runtime;
using GameFrameX.Startup.Runtime;
using GameFrameX.Web.Runtime;

namespace Godot.Startup.Procedure.Startup
{
    /// <summary>
    /// 获取游戏资源包版本信息流程（使用默认包）。向服务器请求默认游戏资源包的版本信息。
    /// </summary>
    /// <remarks>
    /// Get game asset package version info by default package state procedure. Requests version info of the default game asset package from server.
    /// </remarks>
    public sealed class ProcedureGetGameAssetPackageVersionInfoByDefaultPackageState : ProcedureBase
    {
        /// <inheritdoc />
        protected internal override async void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            await GetGameAssetPackageVersionInfoAsync(procedureOwner);
        }

        /// <summary>
        /// 异步获取游戏资源包版本信息。
        /// </summary>
        /// <remarks>
        /// Asynchronously retrieves game asset package version info with retry support. Constructs package URL from response and stores it in procedure owner.
        /// </remarks>
        /// <param name="procedureOwner">流程所有者 / Procedure owner</param>
        /// <returns>获取完成的任务 / Retrieval completion task</returns>
        private async Task GetGameAssetPackageVersionInfoAsync(IFsm<IProcedureManager> procedureOwner)
        {
            var options = StartupProcedureUtility.GetOptions(procedureOwner);
            var uiHandler = StartupProcedureUtility.GetUIHandler(procedureOwner);
            var httpParamsProvider = StartupProcedureUtility.GetHttpParamsProvider(procedureOwner);
            var jsonParams = StartupProcedureUtility.CreateHttpParams(options, httpParamsProvider);
            jsonParams["AssetPackageName"] = AssetComponent.BuildInPackageName;

            for (var retryIndex = 1; retryIndex <= options.MaxAttemptsPerUrl; retryIndex++)
            {
                try
                {
                    var json = await GameEntry.GetComponent<WebComponent>().PostToString(GameEntry.GetComponent<GlobalConfigComponent>().CheckResourceVersionUrl, jsonParams);
                    var httpJsonResult = Utility.Json.ToObject<HttpJsonResult>(json.Result);
                    if (httpJsonResult.Code <= 0)
                    {
                        var packageVersion = Utility.Json.ToObject<ResponseGameAssetPackageVersion>(httpJsonResult.Data);
                        StartupNetworkCacheUtility.ApplyAssetPackageVersionInfo(procedureOwner, packageVersion);
                        StartupNetworkCacheUtility.SaveAssetPackageVersionInfo(httpJsonResult.Data);
                        ChangeState<Godot.Startup.Procedure.Patch.ProcedurePatchInit>(procedureOwner);
                        return;
                    }

                    Log.Error("Get asset package version returned code " + httpJsonResult.Code);
                }
                catch (Exception exception)
                {
                    Log.Error(exception);
                }

                if (retryIndex >= options.MaxAttemptsPerUrl)
                {
                    if (StartupNetworkCacheUtility.TryApplyCachedAssetPackageVersionInfo(procedureOwner))
                    {
                        if (uiHandler != null)
                        {
                            uiHandler.SetTipText("Using cached asset version...");
                        }

                        ChangeState<Godot.Startup.Procedure.Patch.ProcedurePatchInit>(procedureOwner);
                        return;
                    }

                    StartupProcedureUtility.CompleteFailure(procedureOwner, nameof(ProcedureGetGameAssetPackageVersionInfoByDefaultPackageState), GameEntry.GetComponent<GlobalConfigComponent>().CheckResourceVersionUrl, "Failed to get asset package version info.");
                    return;
                }

                if (uiHandler != null)
                {
                    uiHandler.SetTipText("Getting asset version failed, retrying... (" + retryIndex + "/" + options.MaxAttemptsPerUrl + ")");
                }

                await Task.Delay(options.RetryDelayMs);
            }
        }
    }
}
