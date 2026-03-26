#if TOOLS
using Godot;

namespace GameFrameX.Entity.Editor
{
    [Tool]
    public partial class GameFrameXEntityPlugin : EditorPlugin
    {
        private EntityComponentInspectorPlugin m_InspectorPlugin;

        public override void _EnterTree()
        {
            m_InspectorPlugin = new EntityComponentInspectorPlugin();
            AddInspectorPlugin(m_InspectorPlugin);
        }

        public override void _ExitTree()
        {
            m_InspectorPlugin = null;
        }
    }
}
#endif
