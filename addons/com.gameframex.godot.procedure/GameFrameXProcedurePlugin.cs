#if TOOLS
using Godot;

namespace GameFrameX.Procedure.Editor
{
    [Tool]
    public partial class GameFrameXProcedurePlugin : EditorPlugin
    {
        private ProcedureComponentInspectorPlugin m_InspectorPlugin;

        public override void _EnterTree()
        {
            m_InspectorPlugin = new ProcedureComponentInspectorPlugin();
            AddInspectorPlugin(m_InspectorPlugin);
        }

        public override void _ExitTree()
        {
            m_InspectorPlugin = null;
        }
    }
}
#endif
