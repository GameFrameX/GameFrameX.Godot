#if TOOLS
using Godot;

namespace GameFrameX.Editor
{
    /// <summary>
    /// GameFrameX 核心编辑器插件。
    /// 只负责注册核心模块的 InspectorPlugin。
    /// </summary>
    [Tool]
    public partial class GameFrameXCorePlugin : EditorPlugin
    {
        private BaseComponentInspectorPlugin m_BaseComponentInspectorPlugin;

        public override void _EnterTree()
        {
            // 只注册核心的 BaseComponent Inspector
            m_BaseComponentInspectorPlugin = new BaseComponentInspectorPlugin();
            AddInspectorPlugin(m_BaseComponentInspectorPlugin);
        }

        public override void _ExitTree()
        {
            // Godot 4 会自动清理 InspectorPlugin，无需手动移除
            m_BaseComponentInspectorPlugin = null;
        }
    }
}
#endif
