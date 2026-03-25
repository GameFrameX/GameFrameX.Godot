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
using GameFrameX.Runtime;

namespace GameFrameX.UI.Runtime
{
    /// <summary>
    /// 界面组。
    /// </summary>
    public sealed partial class UIGroup : IUIGroup
    {
        private readonly string m_Name;
        private int m_Depth;
        private bool m_Pause;
        private readonly IUIGroupHelper m_UIGroupHelper;
        private readonly GameFrameworkLinkedList<UIFormInfo> m_UIFormInfos;
        private LinkedListNode<UIFormInfo> m_CachedNode;

        /// <summary>
        /// 初始化界面组的新实例。
        /// </summary>
        /// <param name="name">界面组名称。</param>
        /// <param name="depth">界面组深度。</param>
        /// <param name="uiGroupHelper">界面组辅助器。</param>
        public UIGroup(string name, int depth, IUIGroupHelper uiGroupHelper)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("UI group name is invalid.");
            }

            if (uiGroupHelper == null)
            {
                throw new GameFrameworkException("UI group helper is invalid.");
            }

            m_Name = name;
            m_Pause = false;
            m_UIGroupHelper = uiGroupHelper;
            m_UIFormInfos = new GameFrameworkLinkedList<UIFormInfo>();
            m_CachedNode = null;
            Depth = depth;
        }

        /// <summary>
        /// 获取界面组名称。
        /// </summary>
        public string Name
        {
            get { return m_Name; }
        }

        /// <summary>
        /// 获取或设置界面组深度。
        /// </summary>
        public int Depth
        {
            get { return m_Depth; }
            set
            {
                if (m_Depth == value)
                {
                    return;
                }

                m_Depth = value;
                m_UIGroupHelper.SetDepth(m_Depth);
                Refresh();
            }
        }

        /// <summary>
        /// 获取或设置界面组是否暂停。
        /// </summary>
        public bool Pause
        {
            get { return m_Pause; }
            set
            {
                if (m_Pause == value)
                {
                    return;
                }

                m_Pause = value;
                Refresh();
            }
        }

        /// <summary>
        /// 获取界面组中界面数量。
        /// </summary>
        public int UIFormCount
        {
            get { return m_UIFormInfos.Count; }
        }

        /// <summary>
        /// 获取当前界面。
        /// </summary>
        public IUIForm CurrentUIForm
        {
            get { return m_UIFormInfos.First?.Value.UIForm; }
        }

        /// <summary>
        /// 获取界面组辅助器。
        /// </summary>
        public IUIGroupHelper Helper
        {
            get { return m_UIGroupHelper; }
        }

        /// <summary>
        /// 界面组轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public void Update(float elapseSeconds, float realElapseSeconds)
        {
            var current = m_UIFormInfos.First;
            while (current != null)
            {
                if (current.Value.Paused)
                {
                    break;
                }

                m_CachedNode = current.Next;
                current.Value.UIForm.OnUpdate(elapseSeconds, realElapseSeconds);
                current = m_CachedNode;
                m_CachedNode = null;
            }
        }

        /// <summary>
        /// 界面组中是否存在界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>界面组中是否存在界面。</returns>
        public bool HasUIForm(int serialId)
        {
            foreach (var uiFormInfo in m_UIFormInfos)
            {
                if (uiFormInfo.UIForm.SerialId == serialId)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 界面组中是否存在界面。
        /// </summary>
        /// <param name="fullName">界面资源完整名称。</param>
        /// <returns>界面组中是否存在界面。</returns>
        public bool HasUIFormFullName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
            {
                throw new GameFrameworkException("UI form asset name is invalid.");
            }

            foreach (var uiFormInfo in m_UIFormInfos)
            {
                if (uiFormInfo.UIForm.FullName == fullName)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 界面组中是否存在界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>界面组中是否存在界面。</returns>
        public bool HasUIForm(string uiFormAssetName)
        {
            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                throw new GameFrameworkException("UI form asset name is invalid.");
            }

            foreach (var uiFormInfo in m_UIFormInfos)
            {
                if (uiFormInfo.UIForm.UIFormAssetName == uiFormAssetName)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 从界面组中获取界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>要获取的界面。</returns>
        public IUIForm GetUIForm(int serialId)
        {
            foreach (var uiFormInfo in m_UIFormInfos)
            {
                if (uiFormInfo.UIForm.SerialId == serialId)
                {
                    return uiFormInfo.UIForm;
                }
            }

            return null;
        }

        /// <summary>
        /// 从界面组中获取界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>要获取的界面。</returns>
        public IUIForm GetUIForm(string uiFormAssetName)
        {
            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                throw new GameFrameworkException("UI form asset name is invalid.");
            }

            foreach (var uiFormInfo in m_UIFormInfos)
            {
                if (uiFormInfo.UIForm.UIFormAssetName == uiFormAssetName)
                {
                    return uiFormInfo.UIForm;
                }
            }

            return null;
        }

        /// <summary>
        /// 从界面组中获取界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>要获取的界面。</returns>
        public IUIForm[] GetUIForms(string uiFormAssetName)
        {
            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                throw new GameFrameworkException("UI form asset name is invalid.");
            }

            var results = new List<IUIForm>();
            foreach (var uiFormInfo in m_UIFormInfos)
            {
                if (uiFormInfo.UIForm.UIFormAssetName == uiFormAssetName)
                {
                    results.Add(uiFormInfo.UIForm);
                }
            }

            return results.ToArray();
        }

        /// <summary>
        /// 从界面组中获取界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="results">要获取的界面。</param>
        public void GetUIForms(string uiFormAssetName, List<IUIForm> results)
        {
            if (string.IsNullOrEmpty(uiFormAssetName))
            {
                throw new GameFrameworkException("UI form asset name is invalid.");
            }

            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (var uiFormInfo in m_UIFormInfos)
            {
                if (uiFormInfo.UIForm.UIFormAssetName == uiFormAssetName)
                {
                    results.Add(uiFormInfo.UIForm);
                }
            }
        }

        /// <summary>
        /// 从界面组中获取所有界面。
        /// </summary>
        /// <returns>界面组中的所有界面。</returns>
        public IUIForm[] GetAllUIForms()
        {
            var results = new List<IUIForm>();
            foreach (var uiFormInfo in m_UIFormInfos)
            {
                results.Add(uiFormInfo.UIForm);
            }

            return results.ToArray();
        }

        /// <summary>
        /// 从界面组中获取所有界面。
        /// </summary>
        /// <param name="results">界面组中的所有界面。</param>
        public void GetAllUIForms(List<IUIForm> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (var uiFormInfo in m_UIFormInfos)
            {
                results.Add(uiFormInfo.UIForm);
            }
        }

        /// <summary>
        /// 往界面组增加界面。
        /// </summary>
        /// <param name="uiForm">要增加的界面。</param>
        public void AddUIForm(IUIForm uiForm)
        {
            m_UIFormInfos.AddFirst(UIFormInfo.Create(uiForm));
        }

        /// <summary>
        /// 从界面组移除界面。
        /// </summary>
        /// <param name="uiForm">要移除的界面。</param>
        /// <param name="isSkipPause">是否跳过暂停。</param>
        public void RemoveUIForm(IUIForm uiForm, bool isSkipPause = false)
        {
            UIFormInfo uiFormInfo = GetUIFormInfo(uiForm);
            if (uiFormInfo == null)
            {
                Log.Error(Utility.Text.Format("Can not find UI form info for serial id '{0}', UI form asset name is '{1}'.", uiForm.SerialId, uiForm.UIFormAssetName));
                return;
            }

            if (!uiFormInfo.Covered)
            {
                uiFormInfo.Covered = true;
                uiForm.OnCover();
            }

            if (!uiFormInfo.Paused)
            {
                uiFormInfo.Paused = true;
                if (!isSkipPause)
                {
                    uiForm.OnPause();
                }
            }

            if (m_CachedNode != null && m_CachedNode.Value.UIForm == uiForm)
            {
                m_CachedNode = m_CachedNode.Next;
            }

            if (!m_UIFormInfos.Remove(uiFormInfo))
            {
                Log.Error(Utility.Text.Format("UI group '{0}' not exists specified UI form '[{1}]{2}'.", m_Name, uiForm.SerialId, uiForm.UIFormAssetName));
            }

            ReferencePool.Release(uiFormInfo);
        }

        /// <summary>
        /// 激活界面。
        /// </summary>
        /// <param name="uiForm">要激活的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void RefocusUIForm(IUIForm uiForm, object userData)
        {
            UIFormInfo uiFormInfo = GetUIFormInfo(uiForm);
            if (uiFormInfo == null)
            {
                Log.Error(Utility.Text.Format("Can not find UI form info for serial id '{0}', UI form asset name is '{1}'.", uiForm.SerialId, uiForm.UIFormAssetName));
                return;
            }

            m_UIFormInfos.Remove(uiFormInfo);
            m_UIFormInfos.AddFirst(uiFormInfo);
            uiFormInfo.UIForm.OnRefocus(userData);
        }

        /// <summary>
        /// 刷新界面组。
        /// </summary>
        public void Refresh()
        {
            LinkedListNode<UIFormInfo> current = m_UIFormInfos.First;
            bool pause = m_Pause;
            bool cover = false;
            int depth = UIFormCount;
            while (current != null && current.Value != null)
            {
                LinkedListNode<UIFormInfo> next = current.Next;
                current.Value.UIForm.OnDepthChanged(Depth, depth--);
                if (current.Value == null)
                {
                    return;
                }

                if (pause)
                {
                    if (!current.Value.Covered)
                    {
                        current.Value.Covered = true;
                        current.Value.UIForm.OnCover();
                        if (current.Value == null)
                        {
                            return;
                        }
                    }

                    if (!current.Value.Paused)
                    {
                        current.Value.Paused = true;
                        current.Value.UIForm.OnPause();
                        if (current.Value == null)
                        {
                            return;
                        }
                    }
                }
                else
                {
                    if (current.Value.Paused)
                    {
                        current.Value.Paused = false;
                        current.Value.UIForm.OnResume();
                        if (current.Value == null)
                        {
                            return;
                        }
                    }

                    if (current.Value.UIForm.PauseCoveredUIForm)
                    {
                        pause = true;
                    }

                    if (cover)
                    {
                        if (!current.Value.Covered)
                        {
                            current.Value.Covered = true;
                            current.Value.UIForm.OnCover();
                            if (current.Value == null)
                            {
                                return;
                            }
                        }
                    }
                    else
                    {
                        if (current.Value.Covered)
                        {
                            current.Value.Covered = false;
                            current.Value.UIForm.OnReveal();
                            if (current.Value == null)
                            {
                                return;
                            }
                        }

                        cover = true;
                    }
                }

                current = next;
            }
        }

        /// <summary>
        /// 获取界面组中指定资源名称的所有界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="results">要获取的界面列表。</param>
        public void InternalGetUIForms(string uiFormAssetName, List<IUIForm> results)
        {
            foreach (var uiFormInfo in m_UIFormInfos)
            {
                if (uiFormInfo.UIForm.UIFormAssetName == uiFormAssetName)
                {
                    results.Add(uiFormInfo.UIForm);
                }
            }
        }

        /// <summary>
        /// 检查界面组中是否存在指定界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiForm">要检查的界面。</param>
        /// <returns>是否存在指定界面。</returns>
        public bool InternalHasInstanceUIForm(string uiFormAssetName, IUIForm uiForm)
        {
            foreach (var uiFormInfo in m_UIFormInfos)
            {
                if (uiFormInfo.UIForm.UIFormAssetName == uiFormAssetName && uiFormInfo.UIForm == uiForm)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 获取界面组中的所有界面。
        /// </summary>
        /// <param name="results">要获取的界面列表。</param>
        public void InternalGetAllUIForms(List<IUIForm> results)
        {
            foreach (var uiFormInfo in m_UIFormInfos)
            {
                results.Add(uiFormInfo.UIForm);
            }
        }

        private UIFormInfo GetUIFormInfo(IUIForm uiForm)
        {
            if (uiForm == null)
            {
                Log.Warning("UI form is invalid.");
                return null;
            }

            foreach (var uiFormInfo in m_UIFormInfos)
            {
                if (uiFormInfo.UIForm == uiForm)
                {
                    return uiFormInfo;
                }
            }

            return null;
        }
    }
}
