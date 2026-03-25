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

using System;
using GameFrameX.Runtime;

namespace GameFrameX.UI.Runtime
{
    /// <summary>
    /// 界面管理器。
    /// </summary>
    public partial class BaseUIManager
    {
        protected EventHandler<CloseUIFormCompleteEventArgs> m_CloseUIFormCompleteEventHandler;

        /// <summary>
        /// 关闭界面完成事件。
        /// </summary>
        public event EventHandler<CloseUIFormCompleteEventArgs> CloseUIFormComplete
        {
            add { m_CloseUIFormCompleteEventHandler += value; }
            remove { m_CloseUIFormCompleteEventHandler -= value; }
        }

        /// <summary>
        /// 回收界面实例对象。
        /// </summary>
        /// <param name="uiForm"></param>
        /// <param name="isDispose">是否销毁释放</param>
        protected abstract void RecycleUIForm(IUIForm uiForm, bool isDispose = false);

        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="serialId">要关闭界面的序列编号。</param>
        /// <param name="isNowRecycle">是否立即回收界面,默认是否</param>
        public void CloseUIForm(int serialId, bool isNowRecycle = false)
        {
            CloseUIForm(serialId, null, isNowRecycle);
        }

        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="serialId">要关闭界面的序列编号。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <param name="isNowRecycle">是否立即回收界面,默认是否</param>
        public void CloseUIForm(int serialId, object userData, bool isNowRecycle = false)
        {
            var uiForm = GetUIForm(serialId);
            if (uiForm == null)
            {
                Log.Error(Utility.Text.Format("Can not find UI form '{0}'.", serialId));
                return;
            }

            CloseUIForm(uiForm, userData, isNowRecycle);
        }

        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="uiForm">要关闭的界面。</param>
        /// <param name="isNowRecycle">是否立即回收界面,默认是否</param>
        public void CloseUIForm(IUIForm uiForm, bool isNowRecycle = false)
        {
            CloseUIForm(uiForm, null, isNowRecycle);
        }

        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        /// <param name="isNowRecycle">是否立即回收界面,默认是否</param>
        /// <typeparam name="T"></typeparam>
        public void CloseUIForm<T>(object userData, bool isNowRecycle = false) where T : IUIForm
        {
            var fullName = typeof(T).FullName;
            var uiForms = GetAllLoadedUIForms();
            foreach (var uiForm in uiForms)
            {
                if (uiForm.FullName != fullName)
                {
                    continue;
                }

                if (!HasUIFormFullName(uiForm.FullName))
                {
                    continue;
                }

                CloseUIForm(uiForm, userData, isNowRecycle);
                break;
            }
        }

        /// <summary>
        /// 关闭界面。
        /// </summary>
        /// <param name="uiForm">要关闭的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <param name="isNowRecycle">是否立即回收界面,默认是否</param>
        public void CloseUIForm(IUIForm uiForm, object userData, bool isNowRecycle = false)
        {
            GameFrameworkGuard.NotNull(uiForm, nameof(uiForm));
            var serialId = uiForm.SerialId;
            if (IsLoadingUIForm(serialId))
            {
                m_UIFormsToReleaseOnLoad.Add(serialId);
                m_UIFormsBeingLoaded.Remove(serialId);
                return;
            }

            if (uiForm.IsDisableClosing)
            {
                return;
            }

            GameFrameworkGuard.NotNull(uiForm.UIGroup, nameof(uiForm.UIGroup));
            var uiGroup = (UIGroup)uiForm.UIGroup;

            if (uiForm.EnableHideAnimation)
            {
                uiGroup.RemoveUIForm(uiForm, true);
                uiForm.Hide(m_UIFormHideHandler, () =>
                {
                    uiForm.OnClose(m_IsShutdown, userData);
                    uiGroup.Refresh();
                    if (isNowRecycle)
                    {
                        RecycleUIForm(uiForm, true);
                    }
                });
            }
            else
            {
                uiGroup.RemoveUIForm(uiForm);
                uiForm.OnClose(m_IsShutdown, userData);
                uiGroup.Refresh();
                if (isNowRecycle)
                {
                    RecycleUIForm(uiForm, true);
                }
            }

            if (m_CloseUIFormCompleteEventHandler != null)
            {
                var closeUIFormCompleteEventArgs = CloseUIFormCompleteEventArgs.Create(uiForm.SerialId, uiForm.UIFormAssetName, uiGroup, userData);
                m_CloseUIFormCompleteEventHandler(this, closeUIFormCompleteEventArgs);
            }

            // 判断是否禁用了界面的回收
            if (uiForm.IsDisableRecycling)
            {
                return;
            }

            // 判断是否立即回收界面
            if (isNowRecycle)
            {
                return;
            }

            m_RecycleQueue.Enqueue(uiForm);
        }

        /// <summary>
        /// 关闭所有已加载的界面。
        /// </summary>
        /// <param name="isNowRecycle">是否立即回收界面,默认是否</param>
        public void CloseAllLoadedUIForms(bool isNowRecycle = false)
        {
            CloseAllLoadedUIForms(null, isNowRecycle);
        }

        /// <summary>
        /// 关闭所有已加载的界面。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        /// <param name="isNowRecycle">是否立即回收界面,默认是否</param>
        public void CloseAllLoadedUIForms(object userData, bool isNowRecycle = false)
        {
            var uiForms = GetAllLoadedUIForms();
            foreach (var uiForm in uiForms)
            {
                if (!HasUIForm(uiForm.SerialId))
                {
                    continue;
                }

                CloseUIForm(uiForm, userData, isNowRecycle);
            }
        }

        /// <summary>
        /// 关闭所有正在加载的界面。
        /// </summary>
        public void CloseAllLoadingUIForms()
        {
            foreach (var uiFormBeingLoaded in m_UIFormsBeingLoaded)
            {
                m_UIFormsToReleaseOnLoad.Add(uiFormBeingLoaded.Key);
            }

            m_UIFormsBeingLoaded.Clear();
        }

        /// <summary>
        /// 释放所有已加载的界面。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        /// <param name="isNowRecycle">是否立即回收界面,默认是否</param>
        public void ReleaseAllLoadedUIForms(bool isNowRecycle = false, object userData = null)
        {
            foreach (var id in m_UIFormsToReleaseOnLoad)
            {
                var uiForm = GetUIForm(id);
                if (uiForm != null)
                {
                    RecycleUIForm(uiForm, isNowRecycle);
                }
            }

            m_UIFormsToReleaseOnLoad.Clear();
        }
    }
}
