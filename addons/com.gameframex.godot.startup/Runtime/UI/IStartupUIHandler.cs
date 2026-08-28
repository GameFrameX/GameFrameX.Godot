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

namespace GameFrameX.Startup.Runtime
{
    /// <summary>
    /// 启动 UI 处理接口，由应用层实现。包内不依赖具体 UI 后端（FairyGUI / GodotGUI / 自定义）。
    /// </summary>
    public interface IStartupUIHandler
    {
        /// <summary>
        /// 打开启动 UI（全屏），完成 UI 资源加载。
        /// </summary>
        /// <param name="uiResName">UI 资源路径（来自 StartupOptions.LauncherUIResName）。</param>
        Task StartAsync(string uiResName);

        /// <summary>
        /// 更新启动 UI 上的提示文本。
        /// </summary>
        /// <param name="text">提示文本。</param>
        void SetTipText(string text);

        /// <summary>
        /// 更新下载进度条和文本。
        /// </summary>
        /// <param name="progress">0-1 范围的进度值。</param>
        /// <param name="sizeInfo">如 "5.2MB / 100MB"。</param>
        void SetProgress(float progress, string sizeInfo);

        /// <summary>
        /// 标记下载完成状态。
        /// </summary>
        void SetProgressUpdateFinish();

        /// <summary>
        /// 显示应用版本升级弹窗。
        /// </summary>
        /// <returns>true 表示继续启动流程；false 表示流程停留在升级弹窗或外部下载页。</returns>
        Task<bool> ShowUpgradeAsync(StartupUpgradeInfo upgradeInfo);

        /// <summary>
        /// 关闭启动 UI、释放订阅。
        /// </summary>
        void Dispose();
    }
}
