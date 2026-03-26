#if TOOLS
using Godot;

namespace GameFrameX.Fsm.Editor
{
    [Tool]
    public partial class GameFrameXFsmPlugin : EditorPlugin
    {
        private FsmComponentInspectorPlugin m_InspectorPlugin;

        public override void _EnterTree()
        {
            m_InspectorPlugin = new FsmComponentInspectorPlugin();
            AddInspectorPlugin(m_InspectorPlugin);
        }

        public override void _ExitTree()
        {
            m_InspectorPlugin = null;
        }
    }
}
#endif
