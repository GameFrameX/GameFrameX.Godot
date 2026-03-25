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
    /// 界面管理器。
    /// </summary>
    public partial class BaseUIManager
    {
        /// <summary>
        /// 是否存在界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>是否存在界面。</returns>
        public bool HasUIForm(int serialId)
        {
            foreach (var uiGroup in m_UIGroups)
            {
                if (uiGroup.Value.HasUIForm(serialId))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 是否存在界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>是否存在界面。</returns>
        public bool HasUIForm(string uiFormAssetName)
        {
            GameFrameworkGuard.NotNullOrEmpty(uiFormAssetName, nameof(uiFormAssetName));

            foreach (KeyValuePair<string, UIGroup> uiGroup in m_UIGroups)
            {
                if (uiGroup.Value.HasUIForm(uiFormAssetName))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 获取界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>要获取的界面。</returns>
        public IUIForm GetUIForm(int serialId)
        {
            foreach (var uiGroup in m_UIGroups)
            {
                IUIForm uiForm = uiGroup.Value.GetUIForm(serialId);
                if (uiForm != null)
                {
                    return uiForm;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>要获取的界面。</returns>
        public IUIForm GetUIForm(string uiFormAssetName)
        {
            GameFrameworkGuard.NotNullOrEmpty(uiFormAssetName, nameof(uiFormAssetName));

            foreach (KeyValuePair<string, UIGroup> uiGroup in m_UIGroups)
            {
                IUIForm uiForm = uiGroup.Value.GetUIForm(uiFormAssetName);
                if (uiForm != null)
                {
                    return uiForm;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>要获取的界面。</returns>
        public IUIForm[] GetUIForms(string uiFormAssetName)
        {
            GameFrameworkGuard.NotNullOrEmpty(uiFormAssetName, nameof(uiFormAssetName));

            List<IUIForm> results = new List<IUIForm>();
            foreach (KeyValuePair<string, UIGroup> uiGroup in m_UIGroups)
            {
                results.AddRange(uiGroup.Value.GetUIForms(uiFormAssetName));
            }

            return results.ToArray();
        }

        /// <summary>
        /// 获取界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="results">要获取的界面。</param>
        public void GetUIForms(string uiFormAssetName, List<IUIForm> results)
        {
            GameFrameworkGuard.NotNullOrEmpty(uiFormAssetName, nameof(uiFormAssetName));
            GameFrameworkGuard.NotNull(results, nameof(results));
            results.Clear();
            foreach (KeyValuePair<string, UIGroup> uiGroup in m_UIGroups)
            {
                uiGroup.Value.InternalGetUIForms(uiFormAssetName, results);
            }
        }

        /// <summary>
        /// 获取所有已加载的界面。
        /// </summary>
        /// <returns>所有已加载的界面。</returns>
        public IUIForm[] GetAllLoadedUIForms()
        {
            List<IUIForm> results = new List<IUIForm>();
            foreach (KeyValuePair<string, UIGroup> uiGroup in m_UIGroups)
            {
                results.AddRange(uiGroup.Value.GetAllUIForms());
            }

            return results.ToArray();
        }

        /// <summary>
        /// 获取所有已加载的界面。
        /// </summary>
        /// <param name="results">所有已加载的界面。</param>
        public void GetAllLoadedUIForms(List<IUIForm> results)
        {
            GameFrameworkGuard.NotNull(results, nameof(results));

            results.Clear();
            foreach (KeyValuePair<string, UIGroup> uiGroup in m_UIGroups)
            {
                uiGroup.Value.InternalGetAllUIForms(results);
            }
        }

        /// <summary>
        /// 获取所有正在加载界面的序列编号。
        /// </summary>
        /// <returns>所有正在加载界面的序列编号。</returns>
        public int[] GetAllLoadingUIFormSerialIds()
        {
            int index = 0;
            int[] results = new int[m_UIFormsBeingLoaded.Count];
            foreach (KeyValuePair<int, string> uiFormBeingLoaded in m_UIFormsBeingLoaded)
            {
                results[index++] = uiFormBeingLoaded.Key;
            }

            return results;
        }

        /// <summary>
        /// 获取所有正在加载界面的序列编号。
        /// </summary>
        /// <param name="results">所有正在加载界面的序列编号。</param>
        public void GetAllLoadingUIFormSerialIds(List<int> results)
        {
            GameFrameworkGuard.NotNull(results, nameof(results));

            results.Clear();
            foreach (KeyValuePair<int, string> uiFormBeingLoaded in m_UIFormsBeingLoaded)
            {
                results.Add(uiFormBeingLoaded.Key);
            }
        }

        /// <summary>
        /// 是否正在加载界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>是否正在加载界面。</returns>
        public bool IsLoadingUIForm(int serialId)
        {
            return m_UIFormsBeingLoaded.ContainsKey(serialId);
        }

        /// <summary>
        /// 是否正在加载界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>是否正在加载界面。</returns>
        public bool IsLoadingUIForm(string uiFormAssetName)
        {
            GameFrameworkGuard.NotNullOrEmpty(uiFormAssetName, nameof(uiFormAssetName));

            return m_UIFormsBeingLoaded.ContainsValue(uiFormAssetName);
        }

        /// <summary>
        /// 是否是合法的界面。
        /// </summary>
        /// <param name="uiForm">界面。</param>
        /// <returns>界面是否合法。</returns>
        public bool IsValidUIForm(IUIForm uiForm)
        {
            if (uiForm == null)
            {
                return false;
            }

            return HasUIForm(uiForm.SerialId);
        }

        /// <summary>
        /// 是否存在界面。
        /// </summary>
        /// <param name="fullName">完整界面名称</param>
        /// <returns></returns>
        public bool HasUIFormFullName(string fullName)
        {
            GameFrameworkGuard.NotNullOrEmpty(fullName, nameof(fullName));

            foreach (KeyValuePair<string, UIGroup> uiGroup in m_UIGroups)
            {
                if (uiGroup.Value.HasUIFormFullName(fullName))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
