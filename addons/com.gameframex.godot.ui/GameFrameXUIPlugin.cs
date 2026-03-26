#if TOOLS
using Godot;

namespace GameFrameX.UI.Editor
{
    [Tool]
    public partial class GameFrameXUIPlugin : EditorPlugin
    {
        private UIComponentInspectorPlugin m_UIComponentInspectorPlugin;
        private UIFormInspectorPlugin m_UIFormInspectorPlugin;

        public override void _EnterTree()
        {
            m_UIComponentInspectorPlugin = new UIComponentInspectorPlugin();
            AddInspectorPlugin(m_UIComponentInspectorPlugin);

            m_UIFormInspectorPlugin = new UIFormInspectorPlugin();
            AddInspectorPlugin(m_UIFormInspectorPlugin);
        }

        public override void _ExitTree()
        {
            m_UIComponentInspectorPlugin = null;
            m_UIFormInspectorPlugin = null;
        }
    }
}
#endif
