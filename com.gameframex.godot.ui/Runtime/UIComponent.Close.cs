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

namespace GameFrameX.UI.Runtime
{
    public partial class UIComponent
    {
        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="serialId">要关闭界面的序列编号。</param>
        /// <param name="isNowRecycle">是否立即回收界面,默认是否</param>
        public void CloseUIForm(int serialId, bool isNowRecycle = false)
        {
            m_UIManager.CloseUIForm(serialId, isNowRecycle);
        }

        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="serialId">要关闭界面的序列编号。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <param name="isNowRecycle">是否立即回收界面,默认是否</param>
        public void CloseUIForm(int serialId, object userData, bool isNowRecycle = false)
        {
            m_UIManager.CloseUIForm(serialId, userData, isNowRecycle);
        }

        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="uiForm">要关闭的界面。</param>
        /// <param name="isNowRecycle">是否立即回收界面,默认是否</param>
        public void CloseUIForm(IUIForm uiForm, bool isNowRecycle = false)
        {
            m_UIManager.CloseUIForm(uiForm, isNowRecycle);
        }

        /// <summary>
        /// 关闭界面。
        /// 该函数只适用于界面只有一个的情况.因为当找到一个目标对象之后就会立即终止
        /// </summary>
        /// <typeparam name="T">关闭界面的类型</typeparam>
        /// <param name="userData">用户自定义数据。</param>
        /// <param name="isNowRecycle">是否立即回收界面,默认是否</param>
        public void CloseUIForm<T>(object userData = null, bool isNowRecycle = false) where T : IUIForm
        {
            m_UIManager.CloseUIForm<T>(userData, isNowRecycle);
        }

        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="uiForm">要关闭的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <param name="isNowRecycle">是否立即回收界面,默认是否</param>
        public void CloseUIForm(IUIForm uiForm, object userData, bool isNowRecycle = false)
        {
            m_UIManager.CloseUIForm(uiForm, userData, isNowRecycle);
        }

        /// <summary>
        /// 关闭界面组。
        /// </summary>
        /// <param name="uiGroupName">界面组名称。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void CloseUIGroup(string uiGroupName, object userData = null)
        {
            m_UIManager.CloseUIGroup(uiGroupName, userData);
        }

        /// <summary>
        /// 关闭所有已加载的界面。
        /// </summary>
        /// <param name="isNowRecycle">是否立即回收界面,默认是否</param>
        public void CloseAllLoadedUIForms(bool isNowRecycle = false)
        {
            m_UIManager.CloseAllLoadedUIForms(isNowRecycle);
        }

        /// <summary>
        /// 释放所有已加载的界面。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        /// <param name="isNowRecycle">是否立即回收界面,默认是否</param>
        public void ReleaseAllLoadedUIForms(bool isNowRecycle = false, object userData = null)
        {
            m_UIManager.ReleaseAllLoadedUIForms(isNowRecycle, userData);
        }

        /// <summary>
        /// 关闭所有已加载的界面。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        /// <param name="isNowRecycle">是否立即回收界面,默认是否</param>
        public void CloseAllLoadedUIForms(object userData, bool isNowRecycle = false)
        {
            m_UIManager.CloseAllLoadedUIForms(userData, isNowRecycle);
        }

        /// <summary>
        /// 关闭所有正在加载的界面。
        /// </summary>
        public void CloseAllLoadingUIForms()
        {
            m_UIManager.CloseAllLoadingUIForms();
        }
    }
}
