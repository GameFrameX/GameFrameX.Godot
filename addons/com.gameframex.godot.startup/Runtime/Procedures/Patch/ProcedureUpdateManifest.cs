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
    /// 更新资源清单流程。根据版本信息更新资源包清单。
    /// </summary>
    /// <remarks>
    /// Update manifest procedure. Updates the package manifest based on version information.
    /// </remarks>
    internal sealed class ProcedureUpdateManifest : ProcedureBase
    {
        /// <inheritdoc />
        protected internal override async void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            await UpdateManifestAsync(procedureOwner);
        }

        /// <summary>
        /// 异步执行清单更新。
        /// </summary>
        /// <remarks>
        /// Asynchronously updates the package manifest. In offline mode, proceeds directly to patch done.
        /// </remarks>
        /// <param name="procedureOwner">流程所有者 / Procedure owner</param>
        /// <returns>更新完成的任务 / Update completion task</returns>
        private async Task UpdateManifestAsync(IFsm<IProcedureManager> procedureOwner)
        {
            var assetComponent = GameEntry.GetComponent<AssetComponent>();
            if (assetComponent.GamePlayMode == EPlayMode.OfflinePlayMode)
            {
                var versionValue = procedureOwner.GetData<VarString>(AssetComponent.BuildInPackageName + "Version");
                var package = assetComponent.GetAssetsPackage(AssetComponent.BuildInPackageName);
                var operation = package.UpdatePackageManifestAsync(versionValue.Value);
                await operation.Task;
                ChangeState<ProcedurePatchDone>(procedureOwner);
                return;
            }

            GameEntry.GetComponent<EventComponent>().Fire(this, AssetPatchStatesChangeEventArgs.Create(AssetComponent.BuildInPackageName, EPatchStates.UpdateManifest));
            await UpdateManifestCoreAsync(procedureOwner);
        }

        /// <summary>
        /// 执行资源清单更新。
        /// </summary>
        /// <remarks>
        /// Executes the package manifest update with retry on failure.
        /// </remarks>
        /// <param name="procedureOwner">流程所有者 / Procedure owner</param>
        /// <returns>清单更新任务 / Manifest update task</returns>
        private async Task UpdateManifestCoreAsync(IFsm<IProcedureManager> procedureOwner)
        {
            // 说明：对应 Unity 版 WaitForSecondsRealtime(0.1f) 的重试间隔。
            await Task.Delay(100);

            var package = GameEntry.GetComponent<AssetComponent>().GetAssetsPackage(AssetComponent.BuildInPackageName);
            UpdatePackageManifestOperation operation;
            var assetComponent = GameEntry.GetComponent<AssetComponent>();
            if (assetComponent.GamePlayMode == EPlayMode.EditorSimulateMode)
            {
                operation = package.UpdatePackageManifestAsync("Simulate");
            }
            else
            {
                var versionValue = procedureOwner.GetData<VarString>(AssetComponent.BuildInPackageName + "Version");
                operation = package.UpdatePackageManifestAsync(versionValue.Value);
            }

            await operation.Task;

            if (operation.Status == EOperationStatus.Succeed)
            {
                procedureOwner.RemoveData(AssetComponent.BuildInPackageName + "Version");
                ChangeState<ProcedureCreateDownloader>(procedureOwner);
                return;
            }

            Log.Error(operation.Error);
            GameEntry.GetComponent<EventComponent>().Fire(this, AssetPatchManifestUpdateFailedEventArgs.Create(AssetComponent.BuildInPackageName, operation.Error));
            ChangeState<ProcedureUpdateManifest>(procedureOwner);
        }
    }
}
