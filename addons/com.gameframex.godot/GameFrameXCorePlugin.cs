#if TOOLS
using System;
using Godot;

namespace GameFrameX.Editor
{
    /// <summary>
    /// GameFrameX 核心编辑器插件。
    /// 只负责注册核心模块的 InspectorPlugin。
    /// </summary>
    /// <remarks>
    /// GameFrameX core editor plugin.
    /// Only responsible for registering InspectorPlugins for core modules.
    /// </remarks>
    [Tool]
    public partial class GameFrameXCorePlugin : EditorPlugin
    {
        /// <summary>
        /// 顶部菜单项：日志宏定义子菜单。
        /// </summary>
        private const string TopMenuScriptDefineSubmenuName = "ScriptingDefineSymbolsSubmenu";

        /// <summary>
        /// 日志宏定义菜单项：禁用所有日志。
        /// </summary>
        private const int LogDefineDisableAllLogsId = 100;

        /// <summary>
        /// 日志宏定义菜单项：开启所有日志。
        /// </summary>
        private const int LogDefineEnableAllLogsId = 101;

        /// <summary>
        /// 日志宏定义菜单项：开启调试及以上日志。
        /// </summary>
        private const int LogDefineEnableDebugAndAboveLogsId = 102;

        /// <summary>
        /// 日志宏定义菜单项：开启信息及以上日志。
        /// </summary>
        private const int LogDefineEnableInfoAndAboveLogsId = 103;

        /// <summary>
        /// 日志宏定义菜单项：开启警告及以上日志。
        /// </summary>
        private const int LogDefineEnableWarningAndAboveLogsId = 104;

        /// <summary>
        /// 日志宏定义菜单项：开启错误及以上日志。
        /// </summary>
        private const int LogDefineEnableErrorAndAboveLogsId = 105;

        /// <summary>
        /// 日志宏定义菜单项：开启严重错误及以上日志。
        /// </summary>
        private const int LogDefineEnableFatalAndAboveLogsId = 106;

        /// <summary>
        /// BaseComponent 的 Inspector 插件实例。
        /// </summary>
        /// <remarks>
        /// The Inspector plugin instance for BaseComponent.
        /// </remarks>
        private BaseComponentInspector m_BaseComponentInspector;

        /// <summary>
        /// ObjectPoolComponent 的 Inspector 插件实例。
        /// </summary>
        /// <remarks>
        /// The Inspector plugin instance for ObjectPoolComponent.
        /// </remarks>
        private ObjectPoolComponentInspector m_ObjectPoolComponentInspector;

        /// <summary>
        /// ReferencePoolComponent 的 Inspector 插件实例。
        /// </summary>
        /// <remarks>
        /// The Inspector plugin instance for ReferencePoolComponent.
        /// </remarks>
        private ReferencePoolComponentInspector m_ReferencePoolComponentInspector;

        /// <summary>
        /// 顶部工具栏菜单按钮。
        /// </summary>
        private MenuButton m_TopMenuButton;

        /// <summary>
        /// 顶部菜单弹出菜单实例。
        /// </summary>
        private PopupMenu m_TopPopupMenu;

        /// <summary>
        /// 日志宏定义二级菜单实例。
        /// </summary>
        private PopupMenu m_LogDefinePopupMenu;

        /// <summary>
        /// 当前语言代码缓存。
        /// </summary>
        private string m_CurrentLocale;

        /// <summary>
        /// 当插件进入场景树时调用，注册 Inspector 插件。
        /// </summary>
        /// <remarks>
        /// Called when the plugin enters the scene tree, registers Inspector plugins.
        /// </remarks>
        public override void _EnterTree()
        {
            m_CurrentLocale = TranslationServer.GetLocale();

            // 注册核心的 BaseComponent Inspector / Register the core BaseComponent Inspector
            m_BaseComponentInspector = new BaseComponentInspector();
            AddInspectorPlugin(m_BaseComponentInspector);

            // 注册 ObjectPoolComponent Inspector / Register the ObjectPoolComponent Inspector
            m_ObjectPoolComponentInspector = new ObjectPoolComponentInspector();
            AddInspectorPlugin(m_ObjectPoolComponentInspector);

            // 注册 ReferencePoolComponent Inspector / Register the ReferencePoolComponent Inspector
            m_ReferencePoolComponentInspector = new ReferencePoolComponentInspector();
            AddInspectorPlugin(m_ReferencePoolComponentInspector);

            // 初始化顶部菜单
            RegisterTopToolbarMenu();
            SetProcess(true);
        }

        /// <summary>
        /// 当插件退出场景树时调用，清理 Inspector 插件引用。
        /// </summary>
        /// <remarks>
        /// Called when the plugin exits the scene tree, cleans up Inspector plugin references.
        /// </remarks>
        public override void _ExitTree()
        {
            SetProcess(false);
            UnregisterTopToolbarMenu();

            // Godot 4 会自动清理 InspectorPlugin，无需手动移除 / Godot 4 automatically cleans up InspectorPlugins, no need to remove manually
            m_BaseComponentInspector = null;
            m_ObjectPoolComponentInspector = null;
            m_ReferencePoolComponentInspector = null;
            m_TopMenuButton = null;
            m_TopPopupMenu = null;
            m_LogDefinePopupMenu = null;
            m_CurrentLocale = null;
        }

        /// <summary>
        /// 功能：监听语言变化并自动刷新菜单文本。
        /// </summary>
        /// <param name="delta">帧间隔时间。</param>
        public override void _Process(double delta)
        {
            string locale = TranslationServer.GetLocale();
            if (string.Equals(locale, m_CurrentLocale, StringComparison.Ordinal))
            {
                return;
            }

            m_CurrentLocale = locale;
            RebuildTopToolbarMenu();
        }

        /// <summary>
        /// 功能：注册编辑器顶部工具栏下拉菜单。
        /// </summary>
        private void RegisterTopToolbarMenu()
        {
            if (m_TopMenuButton != null)
            {
                return;
            }

            m_TopMenuButton = new MenuButton();
            m_TopMenuButton.Text = "GameFrameX";

            m_TopPopupMenu = m_TopMenuButton.GetPopup();
            BuildLogDefineSubmenu();
            m_TopPopupMenu.AddSubmenuItem(L("脚本宏定义", "Scripting Define Symbols"), TopMenuScriptDefineSubmenuName);
            m_TopPopupMenu.AddSubmenuNodeItem(L("脚本宏定义", "Scripting Define Symbols"), new PopupMenu(), 1);

            AddControlToContainer(CustomControlContainer.Toolbar, m_TopMenuButton);
        }

        /// <summary>
        /// 功能：重建顶部菜单，使菜单文案跟随语言变化。
        /// </summary>
        private void RebuildTopToolbarMenu()
        {
            UnregisterTopToolbarMenu();
            RegisterTopToolbarMenu();
        }

        /// <summary>
        /// 功能：构建日志宏定义二级菜单。
        /// </summary>
        private void BuildLogDefineSubmenu()
        {
            if (m_TopPopupMenu == null || m_LogDefinePopupMenu != null)
            {
                return;
            }

            m_LogDefinePopupMenu = new PopupMenu();
            m_LogDefinePopupMenu.Name = TopMenuScriptDefineSubmenuName;
            m_LogDefinePopupMenu.AddItem(L("禁用所有日志", "Disable All Logs"), LogDefineDisableAllLogsId);
            m_LogDefinePopupMenu.AddItem(L("开启所有日志", "Enable All Logs"), LogDefineEnableAllLogsId);
            m_LogDefinePopupMenu.AddSeparator();
            m_LogDefinePopupMenu.AddItem(L("开启调试及以上日志", "Enable Debug+ Logs"), LogDefineEnableDebugAndAboveLogsId);
            m_LogDefinePopupMenu.AddItem(L("开启信息及以上日志", "Enable Info+ Logs"), LogDefineEnableInfoAndAboveLogsId);
            m_LogDefinePopupMenu.AddItem(L("开启警告及以上日志", "Enable Warning+ Logs"), LogDefineEnableWarningAndAboveLogsId);
            m_LogDefinePopupMenu.AddItem(L("开启错误及以上日志", "Enable Error+ Logs"), LogDefineEnableErrorAndAboveLogsId);
            m_LogDefinePopupMenu.AddItem(L("开启严重错误及以上日志", "Enable Fatal+ Logs"), LogDefineEnableFatalAndAboveLogsId);
            m_LogDefinePopupMenu.IdPressed += OnLogDefineMenuIdPressed;
            m_TopPopupMenu.AddChild(m_LogDefinePopupMenu);
        }

        /// <summary>
        /// 功能：注销编辑器顶部工具栏下拉菜单。
        /// </summary>
        private void UnregisterTopToolbarMenu()
        {
            if (m_LogDefinePopupMenu != null)
            {
                m_LogDefinePopupMenu.IdPressed -= OnLogDefineMenuIdPressed;
                m_LogDefinePopupMenu.QueueFree();
                m_LogDefinePopupMenu = null;
            }

            if (m_TopPopupMenu != null)
            {
                m_TopPopupMenu = null;
            }

            if (m_TopMenuButton != null)
            {
                RemoveControlFromContainer(CustomControlContainer.Toolbar, m_TopMenuButton);
                m_TopMenuButton.QueueFree();
                m_TopMenuButton = null;
            }
        }

        /// <summary>
        /// 功能：处理日志宏定义二级菜单点击事件。
        /// </summary>
        /// <param name="id">菜单项标识。</param>
        private void OnLogDefineMenuIdPressed(long id)
        {
            if (id == LogDefineDisableAllLogsId)
            {
                ApplyLogDefineAction(LogScriptingDefineSymbols.DisableAllLogs, "已禁用所有日志宏定义。");
                return;
            }

            if (id == LogDefineEnableAllLogsId)
            {
                ApplyLogDefineAction(LogScriptingDefineSymbols.EnableAllLogs, "已开启所有日志宏定义。");
                return;
            }

            if (id == LogDefineEnableDebugAndAboveLogsId)
            {
                ApplyLogDefineAction(LogScriptingDefineSymbols.EnableDebugAndAboveLogs, "已开启调试及以上日志宏定义。");
                return;
            }

            if (id == LogDefineEnableInfoAndAboveLogsId)
            {
                ApplyLogDefineAction(LogScriptingDefineSymbols.EnableInfoAndAboveLogs, "已开启信息及以上日志宏定义。");
                return;
            }

            if (id == LogDefineEnableWarningAndAboveLogsId)
            {
                ApplyLogDefineAction(LogScriptingDefineSymbols.EnableWarningAndAboveLogs, "已开启警告及以上日志宏定义。");
                return;
            }

            if (id == LogDefineEnableErrorAndAboveLogsId)
            {
                ApplyLogDefineAction(LogScriptingDefineSymbols.EnableErrorAndAboveLogs, "已开启错误及以上日志宏定义。");
                return;
            }

            if (id == LogDefineEnableFatalAndAboveLogsId)
            {
                ApplyLogDefineAction(LogScriptingDefineSymbols.EnableFatalAndAboveLogs, "已开启严重错误及以上日志宏定义。");
            }
        }

        /// <summary>
        /// 功能：执行日志宏定义菜单动作并刷新界面状态。
        /// </summary>
        /// <param name="action">要执行的动作。</param>
        /// <param name="status">执行后的状态文本。</param>
        private void ApplyLogDefineAction(Action action, string status)
        {
            action?.Invoke();
            GD.Print(status);
        }

        /// <summary>
        /// 功能：根据当前编辑器语言返回本地化文本。
        /// </summary>
        /// <param name="zh">中文文本。</param>
        /// <param name="en">英文文本。</param>
        /// <returns>本地化结果文本。</returns>
        private string L(string zh, string en)
        {
            string locale = string.IsNullOrEmpty(m_CurrentLocale) ? TranslationServer.GetLocale() : m_CurrentLocale;
            if (!string.IsNullOrEmpty(locale) && locale.StartsWith("zh", StringComparison.OrdinalIgnoreCase))
            {
                return zh;
            }

            return en;
        }
    }
}
#endif