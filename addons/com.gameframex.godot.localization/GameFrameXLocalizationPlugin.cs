#if TOOLS
using Godot;

namespace GameFrameX.Localization.Editor
{
    [Tool]
    public partial class GameFrameXLocalizationPlugin : EditorPlugin
    {
        private LocalizationComponentInspectorPlugin m_InspectorPlugin;

        public override void _EnterTree()
        {
            m_InspectorPlugin = new LocalizationComponentInspectorPlugin();
            AddInspectorPlugin(m_InspectorPlugin);
        }

        public override void _ExitTree()
        {
            m_InspectorPlugin = null;
        }
    }
}
#endif
