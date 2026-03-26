#if TOOLS
using Godot;

namespace GameFrameX.Setting.Editor
{
    [Tool]
    public partial class GameFrameXSettingPlugin : EditorPlugin
    {
        private SettingComponentInspectorPlugin m_InspectorPlugin;

        public override void _EnterTree()
        {
            m_InspectorPlugin = new SettingComponentInspectorPlugin();
            AddInspectorPlugin(m_InspectorPlugin);
        }

        public override void _ExitTree()
        {
            m_InspectorPlugin = null;
        }
    }
}
#endif
