// ==========================================================================================
//  GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//  GameFrameX organization and its derivative projects' copyrights, trademarks, patents and other related rights
//  均受中华人民共和国及相关国际法律法规保护。
//  are protected by the laws of the People's Republic of China and international regulations.
//
//  使用本项目须严格遵守相应法律法规与开源许可证之规定。
//  Usage of this project must strictly comply with applicable laws, regulations, and open-source licenses.
//
//  本项目采用 MIT 许可证与 Apache License 2.0 双许可证分发，
//  This project is dual-licensed under the MIT License and Apache License 2.0,
//  完整许可证文本请参见源代码根目录下的 LICENSE 文件。
//  please refer to the LICENSE file in the source code root directory for the full license text.
//
//  禁止利用本项目实施任何危害国家安全、破坏社会秩序、
//  It is prohibited to use this project to engage in any activities that endanger national security, disrupt social order,
//  侵犯他人合法权益等法律法规所禁止的行为！
//  or infringe upon the legal rights and interests of others, as prohibited by laws and regulations!
//  因基于本项目二次开发所产生的一切法律纠纷与责任，
//  Any legal disputes or liabilities arising from secondary development based on this project
//  本组织与贡献者概不承担。
//  shall be borne solely by the developer; the project organization and contributors assume no responsibility.
//
//  GitHub 仓库：https://github.com/GameFrameX
//  GitHub Repository: https://github.com/GameFrameX
//  Gitee  仓库：https://gitee.com/GameFrameX
//  Gitee Repository: https://gitee.com/GameFrameX
//  CNB 仓库：https://cnb.cool/GameFrameX
//  CNB Repository: https://cnb.cool/GameFrameX
//  官方文档：https://gameframex.doc.alianblank.com/
//  Official Documentation: https://gameframex.doc.alianblank.com/
// ==========================================================================================

using System;
using System.Collections.Generic;

namespace GameFrameX.Runtime
{
    /// <summary>
    /// 运行时扫描结果：组件类型、Manager 描述符、Options 贡献者、OverrideProvider 与错误列表。
    /// </summary>
    public sealed class GameFrameXRuntimeScanResult
    {
        private readonly List<Type> m_ComponentTypes = new List<Type>();

        private readonly List<GameFrameXRuntimeManagerDescriptor> m_ManagerDescriptors = new List<GameFrameXRuntimeManagerDescriptor>();

        private readonly List<System.Type> m_OptionsContributorTypes = new List<System.Type>();

        private readonly List<System.Type> m_OverrideProviderTypes = new List<System.Type>();

        private readonly List<string> m_Errors = new List<string>();

        /// <summary>
        /// 获取扫描到的组件类型列表。
        /// </summary>
        public IList<Type> ComponentTypes
        {
            get { return m_ComponentTypes; }
        }

        /// <summary>
        /// 获取扫描到的 Manager 描述符列表。
        /// </summary>
        public IList<GameFrameXRuntimeManagerDescriptor> ManagerDescriptors
        {
            get { return m_ManagerDescriptors; }
        }

        /// <summary>
        /// 获取扫描到的 Options 贡献者类型列表。
        /// </summary>
        public IList<System.Type> OptionsContributorTypes
        {
            get { return m_OptionsContributorTypes; }
        }

        /// <summary>
        /// 获取扫描到的运行时覆盖 Provider 类型列表。
        /// </summary>
        public IList<System.Type> OverrideProviderTypes
        {
            get { return m_OverrideProviderTypes; }
        }

        /// <summary>
        /// 获取扫描错误列表。
        /// </summary>
        public IList<string> Errors
        {
            get { return m_Errors; }
        }
    }
}
