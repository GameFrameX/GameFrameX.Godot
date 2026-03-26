#if TOOLS
using Godot;

namespace GameFrameX.Timer.Editor
{
    [Tool]
    public partial class GameFrameXTimerPlugin : EditorPlugin
    {
        private TimerComponentInspectorPlugin m_InspectorPlugin;

        public override void _EnterTree()
        {
            m_InspectorPlugin = new TimerComponentInspectorPlugin();
            AddInspectorPlugin(m_InspectorPlugin);
        }

        public override void _ExitTree()
        {
            m_InspectorPlugin = null;
        }
    }
}
#endif
