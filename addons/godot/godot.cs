#if TOOLS
using Godot;

[Tool]
public partial class godot : EditorPlugin
{
    private BaseComponentInspectorPlugin m_BaseComponentInspectorPlugin;

    public override void _EnterTree()
    {
        m_BaseComponentInspectorPlugin = new BaseComponentInspectorPlugin();
        AddInspectorPlugin(m_BaseComponentInspectorPlugin);
    }

    public override void _ExitTree()
    {
        if (m_BaseComponentInspectorPlugin != null)
        {
            RemoveInspectorPlugin(m_BaseComponentInspectorPlugin);
            m_BaseComponentInspectorPlugin = null;
        }
    }
}
#endif
