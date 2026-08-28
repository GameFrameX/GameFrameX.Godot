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

using System.Threading.Tasks;
using GameFrameX.Fsm.Runtime;
using GameFrameX.Procedure.Runtime;
using GameFrameX.Runtime;
using GameFrameX.Startup.Runtime;
using GameFrameX.Web.Runtime;

namespace Godot.Startup.Procedure.Startup
{
    /// <summary>
    /// 启动入口状态。负责拉起启动 UI，然后切换到下一个状态。
    /// </summary>
    /// <remarks>
    /// Launcher entry state. Responsible for launching the startup UI, then transitions to the next state.
    /// </remarks>
    public sealed class ProcedureLauncherState : ProcedureBase
    {
        /// <inheritdoc />
        protected internal override async void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);

            await StartLauncherUIAsync(procedureOwner);
        }

        private async Task StartLauncherUIAsync(IFsm<IProcedureManager> procedureOwner)
        {
            var optionsBox = procedureOwner.GetData<VarObject>(BlackBoardKeys.StartupOptions);
            var uiHandlerBox = procedureOwner.GetData<VarObject>(BlackBoardKeys.StartupUIHandler);

            var options = optionsBox.Value as StartupOptions;
            var uiHandler = uiHandlerBox.Value as IStartupUIHandler;

            if (options == null || uiHandler == null)
            {
                return;
            }

            var webComponent = GameEntry.GetComponent<WebComponent>();
            webComponent.RemoveBaseHeader(StartupProcedureUtility.GameFrameXApiKeyHeader);
            webComponent.RemoveBaseHeader(StartupProcedureUtility.GameFrameXAppIdHeader);
            webComponent.RemoveBaseHeader(StartupProcedureUtility.GameFrameXAppSecretHeader);
            webComponent.RemoveBaseHeader(StartupProcedureUtility.GameFrameXTenantSecretHeader);

            foreach (var header in StartupProcedureUtility.CreateGameFrameXHeaders(options))
            {
                webComponent.AddBaseHeader(header.Key, header.Value);
            }

            await uiHandler.StartAsync(options.LauncherUIResName);

            await StartupProcedureUtility.WaitForNextFrameAsync();
            ChangeState<ProcedureGetGlobalInfoState>(procedureOwner);
        }
    }
}
