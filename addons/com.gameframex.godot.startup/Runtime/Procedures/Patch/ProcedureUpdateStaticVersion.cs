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
using GameFrameX.Asset.Runtime;
using GameFrameX.AssetSystem;
using GameFrameX.Event.Runtime;
using GameFrameX.Fsm.Runtime;
using GameFrameX.Procedure.Runtime;
using GameFrameX.Runtime;
using GameFrameX.Startup.Runtime;

namespace Godot.Startup.Procedure.Patch
{
    /// <summary>
    /// 更新静态版本流程。从资源系统获取资源包的最新版本信息。
    /// </summary>
    /// <remarks>
    /// Update static version procedure. Requests the latest version information of the asset package.
    /// </remarks>
    internal sealed class ProcedureUpdateStaticVersion : ProcedureBase
    {
        /// <inheritdoc />
        protected internal override async void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            GameEntry.GetComponent<EventComponent>().Fire(this, AssetPatchStatesChangeEventArgs.Create(AssetComponent.BuildInPackageName, EPatchStates.UpdateStaticVersion));
            await GetStaticVersionAsync(procedureOwner);
        }

        /// <summary>
        /// 获取静态版本信息。
        /// </summary>
        /// <remarks>
        /// Requests the static package version and stores it in offline mode.
        /// </remarks>
        /// <param name="procedureOwner">流程所有者 / Procedure owner</param>
        /// <returns>版本获取任务 / Version request task</returns>
        private async Task GetStaticVersionAsync(IFsm<IProcedureManager> procedureOwner)
        {
            // 说明：Unity 版通过 YooAssets.GetPackage 获取默认包；Godot 侧统一走 AssetComponent.GetAssetsPackage 组件层转发。
            var package = GameEntry.GetComponent<AssetComponent>().GetAssetsPackage(AssetComponent.BuildInPackageName);
            var operation = package.RequestPackageVersionAsync();
            await operation.Task;

            if (operation.Status == EOperationStatus.Succeed)
            {
                var assetComponent = GameEntry.GetComponent<AssetComponent>();
                if (assetComponent.GamePlayMode == EPlayMode.OfflinePlayMode)
                {
                    var versionValue = ReferencePool.Acquire<VarString>();
                    versionValue.SetValue(operation.PackageVersion);
                    procedureOwner.SetData(AssetComponent.BuildInPackageName + "Version", versionValue);
                }

                Log.Info("Updated package Version : " + operation.PackageVersion);
                ChangeState<ProcedureUpdateManifest>(procedureOwner);
                return;
            }

            Log.Error(operation.Error);
            GameEntry.GetComponent<EventComponent>().Fire(this, AssetStaticVersionUpdateFailedEventArgs.Create(AssetComponent.BuildInPackageName, operation.Error));
            ChangeState<ProcedureUpdateStaticVersion>(procedureOwner);
        }
    }
}
