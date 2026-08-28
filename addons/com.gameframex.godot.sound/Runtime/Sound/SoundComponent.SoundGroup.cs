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
//  Any disputes or liabilities arising from secondary development based on this project
//  本项目组织与贡献者概不承担。
//  shall be borne solely by the developer; the project organization and contributors assume no responsibility.
//
//  GitHub 仓库：https://github.com/GameFrameX
//  GitHub Repository: https://github.com/GameFrameX
//  Gitee  仓库：https://gitee.com/GameFrameX
//  Gitee Repository: https://gitee.com/GameFrameX
//  官方文档：https://gameframex.doc.alianblank.com/
//  Official Documentation: https://gameframex.doc.alianblank.com/
// ==========================================================================================
using System;

namespace GameFrameX.Sound.Runtime
{
    public sealed partial class SoundComponent
    {
        /// <summary>
        /// 声音组配置。
        /// </summary>
        /// <remarks>
        /// Godot 迁移说明（对应 Unity 版 [Serializable] 嵌套配置类）：
        /// Godot 4.5 的检查器无法直接导出普通嵌套类数组（需要 Resource 化并加 [GlobalClass] 全局注册，
        /// 嵌套类不适用该机制），因此检查器配置改由 <see cref="SoundComponent"/> 上的平行数组导出承载，
        /// 初始化时聚合为本配置对象，保持与 Unity 版一致的使用结构。
        /// </remarks>
        private sealed class SoundGroup
        {
            private readonly string m_Name;
            private readonly bool m_AvoidBeingReplacedBySamePriority;
            private readonly bool m_Mute;
            private readonly float m_Volume;
            private readonly int m_AgentHelperCount;

            /// <summary>
            /// 初始化声音组配置的新实例。
            /// </summary>
            /// <param name="name">声音组名称。</param>
            /// <param name="avoidBeingReplacedBySamePriority">是否禁止同优先级声音替换。</param>
            /// <param name="mute">是否静音。</param>
            /// <param name="volume">音量。</param>
            /// <param name="agentHelperCount">声音代理辅助器数量。</param>
            public SoundGroup(string name, bool avoidBeingReplacedBySamePriority, bool mute, float volume, int agentHelperCount)
            {
                m_Name = name;
                m_AvoidBeingReplacedBySamePriority = avoidBeingReplacedBySamePriority;
                m_Mute = mute;
                m_Volume = volume;
                m_AgentHelperCount = agentHelperCount;
            }

            /// <summary>
            /// 获取声音组名称。
            /// </summary>
            public string Name
            {
                get { return m_Name; }
            }

            /// <summary>
            /// 获取是否禁止同优先级声音替换。
            /// </summary>
            public bool AvoidBeingReplacedBySamePriority
            {
                get { return m_AvoidBeingReplacedBySamePriority; }
            }

            /// <summary>
            /// 获取是否静音。
            /// </summary>
            public bool Mute
            {
                get { return m_Mute; }
            }

            /// <summary>
            /// 获取音量。
            /// </summary>
            public float Volume
            {
                get { return m_Volume; }
            }

            /// <summary>
            /// 获取声音代理辅助器数量。
            /// </summary>
            public int AgentHelperCount
            {
                get { return m_AgentHelperCount; }
            }
        }
    }
}
