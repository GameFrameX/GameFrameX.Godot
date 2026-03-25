#if TOOLS
using GameFrameX.Config.Editor;
using GameFrameX.Download.Editor;
using GameFrameX.Entity.Editor;
using GameFrameX.Event.Editor;
using GameFrameX.Fsm.Editor;
using GameFrameX.Localization.Editor;
using GameFrameX.Network.Editor;
using GameFrameX.Procedure.Editor;
using GameFrameX.Setting.Editor;
using GameFrameX.Timer.Editor;
using GameFrameX.UI.Editor;
using Godot;

[Tool]
public partial class godot : EditorPlugin
{
    private BaseComponentInspectorPlugin m_BaseComponentInspectorPlugin;

    // UI Inspectors
    private UIComponentInspectorPlugin m_UIComponentInspectorPlugin;
    private UIFormInspectorPlugin m_UIFormInspectorPlugin;

    // Core Inspectors
    private FsmComponentInspectorPlugin m_FsmComponentInspectorPlugin;
    private ProcedureComponentInspectorPlugin m_ProcedureComponentInspectorPlugin;
    private EventComponentInspectorPlugin m_EventComponentInspectorPlugin;
    private ConfigComponentInspectorPlugin m_ConfigComponentInspectorPlugin;
    private SettingComponentInspectorPlugin m_SettingComponentInspectorPlugin;
    private LocalizationComponentInspectorPlugin m_LocalizationComponentInspectorPlugin;
    private NetworkComponentInspectorPlugin m_NetworkComponentInspectorPlugin;
    private TimerComponentInspectorPlugin m_TimerComponentInspectorPlugin;
    private DownloadComponentInspectorPlugin m_DownloadComponentInspectorPlugin;
    private EntityComponentInspectorPlugin m_EntityComponentInspectorPlugin;

    public override void _EnterTree()
    {
        // Base component inspector
        m_BaseComponentInspectorPlugin = new BaseComponentInspectorPlugin();
        AddInspectorPlugin(m_BaseComponentInspectorPlugin);

        // UI Inspectors
        m_UIComponentInspectorPlugin = new UIComponentInspectorPlugin();
        AddInspectorPlugin(m_UIComponentInspectorPlugin);

        m_UIFormInspectorPlugin = new UIFormInspectorPlugin();
        AddInspectorPlugin(m_UIFormInspectorPlugin);

        // Core Inspectors
        m_FsmComponentInspectorPlugin = new FsmComponentInspectorPlugin();
        AddInspectorPlugin(m_FsmComponentInspectorPlugin);

        m_ProcedureComponentInspectorPlugin = new ProcedureComponentInspectorPlugin();
        AddInspectorPlugin(m_ProcedureComponentInspectorPlugin);

        m_EventComponentInspectorPlugin = new EventComponentInspectorPlugin();
        AddInspectorPlugin(m_EventComponentInspectorPlugin);

        m_ConfigComponentInspectorPlugin = new ConfigComponentInspectorPlugin();
        AddInspectorPlugin(m_ConfigComponentInspectorPlugin);

        m_SettingComponentInspectorPlugin = new SettingComponentInspectorPlugin();
        AddInspectorPlugin(m_SettingComponentInspectorPlugin);

        m_LocalizationComponentInspectorPlugin = new LocalizationComponentInspectorPlugin();
        AddInspectorPlugin(m_LocalizationComponentInspectorPlugin);

        m_NetworkComponentInspectorPlugin = new NetworkComponentInspectorPlugin();
        AddInspectorPlugin(m_NetworkComponentInspectorPlugin);

        m_TimerComponentInspectorPlugin = new TimerComponentInspectorPlugin();
        AddInspectorPlugin(m_TimerComponentInspectorPlugin);

        m_DownloadComponentInspectorPlugin = new DownloadComponentInspectorPlugin();
        AddInspectorPlugin(m_DownloadComponentInspectorPlugin);

        m_EntityComponentInspectorPlugin = new EntityComponentInspectorPlugin();
        AddInspectorPlugin(m_EntityComponentInspectorPlugin);
    }

    public override void _ExitTree()
    {
        // Base component inspector
        if (m_BaseComponentInspectorPlugin != null)
        {
            RemoveInspectorPlugin(m_BaseComponentInspectorPlugin);
            m_BaseComponentInspectorPlugin = null;
        }

        // UI Inspectors
        if (m_UIComponentInspectorPlugin != null)
        {
            RemoveInspectorPlugin(m_UIComponentInspectorPlugin);
            m_UIComponentInspectorPlugin = null;
        }

        if (m_UIFormInspectorPlugin != null)
        {
            RemoveInspectorPlugin(m_UIFormInspectorPlugin);
            m_UIFormInspectorPlugin = null;
        }

        // Core Inspectors
        if (m_FsmComponentInspectorPlugin != null)
        {
            RemoveInspectorPlugin(m_FsmComponentInspectorPlugin);
            m_FsmComponentInspectorPlugin = null;
        }

        if (m_ProcedureComponentInspectorPlugin != null)
        {
            RemoveInspectorPlugin(m_ProcedureComponentInspectorPlugin);
            m_ProcedureComponentInspectorPlugin = null;
        }

        if (m_EventComponentInspectorPlugin != null)
        {
            RemoveInspectorPlugin(m_EventComponentInspectorPlugin);
            m_EventComponentInspectorPlugin = null;
        }

        if (m_ConfigComponentInspectorPlugin != null)
        {
            RemoveInspectorPlugin(m_ConfigComponentInspectorPlugin);
            m_ConfigComponentInspectorPlugin = null;
        }

        if (m_SettingComponentInspectorPlugin != null)
        {
            RemoveInspectorPlugin(m_SettingComponentInspectorPlugin);
            m_SettingComponentInspectorPlugin = null;
        }

        if (m_LocalizationComponentInspectorPlugin != null)
        {
            RemoveInspectorPlugin(m_LocalizationComponentInspectorPlugin);
            m_LocalizationComponentInspectorPlugin = null;
        }

        if (m_NetworkComponentInspectorPlugin != null)
        {
            RemoveInspectorPlugin(m_NetworkComponentInspectorPlugin);
            m_NetworkComponentInspectorPlugin = null;
        }

        if (m_TimerComponentInspectorPlugin != null)
        {
            RemoveInspectorPlugin(m_TimerComponentInspectorPlugin);
            m_TimerComponentInspectorPlugin = null;
        }

        if (m_DownloadComponentInspectorPlugin != null)
        {
            RemoveInspectorPlugin(m_DownloadComponentInspectorPlugin);
            m_DownloadComponentInspectorPlugin = null;
        }

        if (m_EntityComponentInspectorPlugin != null)
        {
            RemoveInspectorPlugin(m_EntityComponentInspectorPlugin);
            m_EntityComponentInspectorPlugin = null;
        }
    }
}
#endif
