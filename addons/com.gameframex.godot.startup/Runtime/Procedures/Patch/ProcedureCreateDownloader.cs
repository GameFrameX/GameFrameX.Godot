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
    /// 创建资源下载器流程。创建下载器并检查是否有资源需要下载。
    /// </summary>
    /// <remarks>
    /// Create resource downloader procedure. Creates a downloader and checks if any resources need to be downloaded.
    /// </remarks>
    internal sealed class ProcedureCreateDownloader : ProcedureBase
    {
        /// <inheritdoc />
        protected internal override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            GameEntry.GetComponent<EventComponent>().Fire(this, AssetPatchStatesChangeEventArgs.Create(AssetComponent.BuildInPackageName, EPatchStates.CreateDownloader));
            CreateDownloader(procedureOwner);
        }

        /// <summary>
        /// 创建资源下载器实例。
        /// </summary>
        /// <remarks>
        /// Creates a resource downloader instance and stores it in procedure owner data.
        /// </remarks>
        /// <param name="procedureOwner">流程所有者 / Procedure owner</param>
        private void CreateDownloader(IFsm<IProcedureManager> procedureOwner)
        {
            // 说明：Unity 版为默认包静态入口 CreateResourceDownloader(10, 3)；
            // Godot 侧为 ResourcePackage 实例方法 CreateResourceDownloader(downloadingMax, failedTryAgain)。
            var package = GameEntry.GetComponent<AssetComponent>().GetAssetsPackage(AssetComponent.BuildInPackageName);
            var downloader = package.CreateResourceDownloader(10, 3);
            var downloaderValue = ReferencePool.Acquire<VarObject>();
            downloaderValue.SetValue(downloader);
            procedureOwner.SetData("Downloader", downloaderValue);

            if (downloader.TotalDownloadCount == 0)
            {
                Log.Info("No resources need to be downloaded.");
                ChangeState<ProcedurePatchDone>(procedureOwner);
                return;
            }

            GameEntry.GetComponent<EventComponent>().Fire(this, AssetFoundUpdateFilesEventArgs.Create(downloader.GetPackageName(), downloader.TotalDownloadCount, downloader.TotalDownloadBytes));
            ChangeState<ProcedureDownloadWebFiles>(procedureOwner);
        }
    }
}
