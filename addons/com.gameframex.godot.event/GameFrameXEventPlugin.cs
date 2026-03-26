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
//  禁止利用本项目实施任何危害国家安全、破坏社会秩序、侵犯他人合法权益等法律法规所禁止的行为！
//  It is prohibited to use this project to engage in any activities that endanger national security, disrupt social order,
//  or infringe upon the legitimate rights and interests of others, as prohibited by laws and regulations!
//  因基于本项目二次开发所产生的一切法律纠纷与责任，本项目组织与贡献者概不承担。
//  Any legal disputes and liabilities arising from secondary development based on this project
//  shall be borne solely by the developer; the project organization and contributors assume no responsibility.
//
//  GitHub 仓库：https://github.com/GameFrameX
//  GitHub Repository: https://github.com/GameFrameX
//  Gitee  仓库：https://gitee.com/GameFrameX
//  Gitee Repository:  https://gitee.com/GameFrameX
//  官方文档：https://gameframex.doc.alianblank.com/
//  Official Documentation: https://gameframex.doc.alianblank.com/
// ==========================================================================================

#if TOOLS
using Godot;

namespace GameFrameX.Event.Editor
{
    /// <summary>
    /// GameFrameX 事件模块编辑器插件。
    /// 负责注册 EventComponent 的 InspectorPlugin。
    /// </summary>
    /// <remarks>
    /// GameFrameX Event module editor plugin.
    /// Responsible for registering InspectorPlugins for EventComponent.
    /// </remarks>
    [Tool]
    public partial class GameFrameXEventPlugin : EditorPlugin
    {
        /// <summary>
        /// EventComponent 的 Inspector 插件实例。
        /// </summary>
        /// <remarks>
        /// The Inspector plugin instance for EventComponent.
        /// </remarks>
        private EventComponentInspectorPlugin m_EventComponentInspectorPlugin;

        /// <summary>
        /// 当插件进入场景树时调用，注册 Inspector 插件。
        /// </summary>
        /// <remarks>
        /// Called when the plugin enters the scene tree, registers Inspector plugins.
        /// </remarks>
        public override void _EnterTree()
        {
            m_EventComponentInspectorPlugin = new EventComponentInspectorPlugin();
            AddInspectorPlugin(m_EventComponentInspectorPlugin);
        }

        /// <summary>
        /// 当插件退出场景树时调用，清理 Inspector 插件引用。
        /// </summary>
        /// <remarks>
        /// Called when the plugin exits the scene tree, cleans up Inspector plugin references.
        /// </remarks>
        public override void _ExitTree()
        {
            // Godot 4 会自动清理 InspectorPlugin，无需手动移除 / Godot 4 automatically cleans up InspectorPlugins, no need to remove manually
            m_EventComponentInspectorPlugin = null;
        }
    }
}
#endif
