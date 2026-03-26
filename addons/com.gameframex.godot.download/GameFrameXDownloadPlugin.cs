#if TOOLS
using Godot;

namespace GameFrameX.Download.Editor
{
    [Tool]
    public partial class GameFrameXDownloadPlugin : EditorPlugin
    {
        private DownloadComponentInspectorPlugin m_InspectorPlugin;

        public override void _EnterTree()
        {
            m_InspectorPlugin = new DownloadComponentInspectorPlugin();
            AddInspectorPlugin(m_InspectorPlugin);
        }

        public override void _ExitTree()
        {
            m_InspectorPlugin = null;
        }
    }
}
#endif
