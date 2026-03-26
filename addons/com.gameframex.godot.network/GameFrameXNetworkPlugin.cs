#if TOOLS
using Godot;

namespace GameFrameX.Network.Editor
{
    [Tool]
    public partial class GameFrameXNetworkPlugin : EditorPlugin
    {
        private NetworkComponentInspectorPlugin m_InspectorPlugin;

        public override void _EnterTree()
        {
            m_InspectorPlugin = new NetworkComponentInspectorPlugin();
            AddInspectorPlugin(m_InspectorPlugin);
        }

        public override void _ExitTree()
        {
            m_InspectorPlugin = null;
        }
    }
}
#endif
