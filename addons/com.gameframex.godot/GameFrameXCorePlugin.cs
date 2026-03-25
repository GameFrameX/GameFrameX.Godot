#if TOOLS
using Godot;

namespace GameFrameX.Editor
{
    /// <summary>
    /// GameFrameX 核心编辑器插件。
    /// 只负责注册核心模块的 InspectorPlugin。
    /// </summary>
    /// <remarks>
    /// GameFrameX core editor plugin.
    /// Only responsible for registering InspectorPlugins for core modules.
    /// </remarks>
    [Tool]
    public partial class GameFrameXCorePlugin : EditorPlugin
    {
        /// <summary>
        /// BaseComponent 的 Inspector 插件实例。
        /// </summary>
        /// <remarks>
        /// The Inspector plugin instance for BaseComponent.
        /// </remarks>
        private BaseComponentInspector m_BaseComponentInspector;

        /// <summary>
        /// 当插件进入场景树时调用，注册 Inspector 插件。
        /// </summary>
        /// <remarks>
        /// Called when the plugin enters the scene tree, registers Inspector plugins.
        /// </remarks>
        public override void _EnterTree()
        {
            // 只注册核心的 BaseComponent Inspector / Only register the core BaseComponent Inspector
            m_BaseComponentInspector = new BaseComponentInspector();
            AddInspectorPlugin(m_BaseComponentInspector);
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
            m_BaseComponentInspector = null;
        }
    }
}
#endif