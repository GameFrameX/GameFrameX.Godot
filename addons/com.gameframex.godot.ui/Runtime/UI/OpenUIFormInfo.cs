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
    /// 打开界面的信息。
    /// </summary>
    public sealed class OpenUIFormInfo : IReference
    {
        private int m_SerialId = 0;
        private bool m_PauseCoveredUIForm = false;
        private object m_UserData = null;
        private Type m_FormType;
        private object m_AssetHandle;
        private bool m_IsFullScreen = false;
        private string m_AssetPath;
        private string m_AssetName;

        /// <summary>
        /// 获取界面是否全屏。
        /// </summary>
        public bool IsFullScreen
        {
            get { return m_IsFullScreen; }
        }

        /// <summary>
        /// 获取界面资源路径。
        /// </summary>
        public string AssetPath
        {
            get { return m_AssetPath; }
        }

        /// <summary>
        /// 获取界面资源名称。
        /// </summary>
        public string AssetName
        {
            get { return m_AssetName; }
        }

        /// <summary>
        /// 获取界面类型。
        /// </summary>
        public Type FormType
        {
            get { return m_FormType; }
        }

        /// <summary>
        /// 获取界面序列编号。
        /// </summary>
        public int SerialId
        {
            get { return m_SerialId; }
        }

        /// <summary>
        /// 获取是否暂停被覆盖的界面。
        /// </summary>
        public bool PauseCoveredUIForm
        {
            get { return m_PauseCoveredUIForm; }
        }

        /// <summary>
        /// 获取用户自定义数据。
        /// </summary>
        public object UserData
        {
            get { return m_UserData; }
        }

        /// <summary>
        /// 获取用户自定义数据。
        /// </summary>
        public object AssetHandle
        {
            get { return m_AssetHandle; }
        }

        /// <summary>
        /// 设置界面资源句柄。
        /// </summary>
        /// <param name="assetHandle">界面资源句柄。</param>
        public void SetAssetHandle(object assetHandle)
        {
            m_AssetHandle = assetHandle;
        }

        /// <summary>
        /// 创建打开界面的信息。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <param name="assetPath">界面资源路径。</param>
        /// <param name="assetName">界面资源名称。</param>
        /// <param name="uiFormType">界面类型。</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <param name="isFullScreen">界面是否全屏。</param>
        /// <returns>创建的打开界面的信息。</returns>
        public static OpenUIFormInfo Create(int serialId, string assetPath, string assetName, Type uiFormType, bool pauseCoveredUIForm, object userData, bool isFullScreen)
        {
            OpenUIFormInfo openUIFormInfo = ReferencePool.Acquire<OpenUIFormInfo>();
            openUIFormInfo.m_SerialId = serialId;
            openUIFormInfo.m_PauseCoveredUIForm = pauseCoveredUIForm;
            openUIFormInfo.m_UserData = userData;
            openUIFormInfo.m_AssetPath = assetPath;
            openUIFormInfo.m_AssetName = assetName;
            openUIFormInfo.m_FormType = uiFormType;
            openUIFormInfo.m_IsFullScreen = isFullScreen;
            return openUIFormInfo;
        }

        /// <summary>
        /// 创建打开界面的信息。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <param name="uiFormType">界面类型。</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <param name="isFullScreen">界面是否全屏。</param>
        /// <returns>创建的打开界面的信息。</returns>
        public static OpenUIFormInfo Create(int serialId, Type uiFormType, bool pauseCoveredUIForm, object userData, bool isFullScreen)
        {
            return Create(serialId, null, null, uiFormType, pauseCoveredUIForm, userData, isFullScreen);
        }

        /// <summary>
        /// 清理打开界面的信息。
        /// </summary>
        public void Clear()
        {
            m_SerialId = default;
            m_PauseCoveredUIForm = default;
            m_UserData = default;
            m_FormType = default;
            m_AssetHandle = default;
            m_IsFullScreen = default;
            m_AssetPath = default;
            m_AssetName = default;
        }
    }
}
