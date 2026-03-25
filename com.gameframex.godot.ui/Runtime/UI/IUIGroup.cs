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
//  Any legal disputes and liabilities arising from secondary development based on this project
//  本项目组织与贡献者概不承担。
//  shall be borne solely by the developer; the project organization and contributors assume no responsibility.
//
//  GitHub 仓库：https://github.com/GameFrameX
//  GitHub Repository: https://github.com/GameFrameX
//  Gitee  仓库：https://gitee.com/GameFrameX
//  Gitee Repository:  https://gitee.com/GameFrameX
//  官方文档：https://gameframex.doc.alianblank.com/
//  Official Documentation: https://gameframex.doc.alianblank.com/
// ==========================================================================================

using System.Collections.Generic;

namespace GameFrameX.UI.Runtime
{
    /// <summary>
    /// 界面组接口。
    /// </summary>
    public interface IUIGroup
    {
        /// <summary>
        /// 获取界面组名称。
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 获取或设置界面组深度。
        /// </summary>
        int Depth { get; set; }

        /// <summary>
        /// 获取或设置界面组是否暂停。
        /// </summary>
        bool Pause { get; set; }

        /// <summary>
        /// 获取界面组中界面数量。
        /// </summary>
        int UIFormCount { get; }

        /// <summary>
        /// 获取当前界面。
        /// </summary>
        IUIForm CurrentUIForm { get; }

        /// <summary>
        /// 获取界面组辅助器。
        /// </summary>
        IUIGroupHelper Helper { get; }

        /// <summary>
        /// 界面组中是否存在界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>界面组中是否存在界面。</returns>
        bool HasUIForm(int serialId);

        /// <summary>
        /// 界面组中是否存在界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>界面组中是否存在界面。</returns>
        bool HasUIForm(string uiFormAssetName);

        /// <summary>
        /// 从界面组中获取界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>要获取的界面。</returns>
        IUIForm GetUIForm(int serialId);

        /// <summary>
        /// 从界面组中获取界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>要获取的界面。</returns>
        IUIForm GetUIForm(string uiFormAssetName);

        /// <summary>
        /// 从界面组中获取界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>要获取的界面。</returns>
        IUIForm[] GetUIForms(string uiFormAssetName);

        /// <summary>
        /// 从界面组中获取界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="results">要获取的界面。</param>
        void GetUIForms(string uiFormAssetName, List<IUIForm> results);

        /// <summary>
        /// 从界面组中获取所有界面。
        /// </summary>
        /// <returns>界面组中的所有界面。</returns>
        IUIForm[] GetAllUIForms();

        /// <summary>
        /// 从界面组中获取所有界面。
        /// </summary>
        /// <param name="results">界面组中的所有界面。</param>
        void GetAllUIForms(List<IUIForm> results);

        /// <summary>
        /// 检查界面组中是否存在指定界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiForm">要检查的界面。</param>
        /// <returns>是否存在指定界面。</returns>
        bool InternalHasInstanceUIForm(string uiFormAssetName, IUIForm uiForm);

        /// <summary>
        /// 往界面组增加界面。
        /// </summary>
        /// <param name="uiForm">要增加的界面。</param>
        void AddUIForm(IUIForm uiForm);

        /// <summary>
        /// 刷新界面组。
        /// </summary>
        void Refresh();
    }
}
