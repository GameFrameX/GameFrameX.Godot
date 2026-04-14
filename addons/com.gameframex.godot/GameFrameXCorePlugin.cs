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
        private const string TopMenuButtonNodeName = "GameFrameXTopMenuButton";

        /// <summary>
        /// 顶部菜单项：日志宏定义子菜单。
        /// </summary>
        private const string TopMenuScriptDefineSubmenuName = "ScriptingDefineSymbolsSubmenu";

        /// <summary>
        /// 日志宏定义菜单项：打开宏定义窗口。
        /// </summary>
        private const int LogDefineOpenWindowId = 90;

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
        /// 顶部菜单项：资源打包器（兼容旧入口）。
        /// </summary>
        private const int TopMenuAssetBuilderId = 20;
        
        /// <summary>
        /// 顶部菜单项：生成客户端配置。
        /// </summary>
        private const int TopMenuGenerateClientConfigId = 21;

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
        private RuntimeLogBridge m_RuntimeLogBridge;
        private AssetSystemBuilderDialog m_AssetSystemBuilderDialog;
        private ScriptingDefineSymbolsWindow m_ScriptingDefineSymbolsWindow;

        /// <summary>
        /// 当插件进入场景树时调用，注册 Inspector 插件。
        /// </summary>
        /// <remarks>
        /// Called when the plugin enters the scene tree, registers Inspector plugins.
        /// </remarks>
        public override void _EnterTree()
        {
            m_CurrentLocale = TranslationServer.GetLocale();
            ScriptingDefineSymbols.DefineSymbolsChanged -= OnDefineSymbolsChanged;
            ScriptingDefineSymbols.DefineSymbolsChanged += OnDefineSymbolsChanged;
            try
            {
                bool changed = ScriptingDefineSymbols.AlignHotfixDefineConstantsWithGodot();
                string symbols = string.Join(";", ScriptingDefineSymbols.GetScriptingDefineSymbols());
                GD.Print($"[ScriptingDefineSymbols] plugin enter align checked. changed={changed} symbols={symbols}");
            }
            catch (Exception exception)
            {
                GD.PushWarning($"[ScriptingDefineSymbols] align Hotfix.csproj failed on plugin enter: {exception.Message}");
            }

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
            m_RuntimeLogBridge = new RuntimeLogBridge();
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
            ScriptingDefineSymbols.DefineSymbolsChanged -= OnDefineSymbolsChanged;
            SetProcess(false);
            UnregisterTopToolbarMenu();
            if (m_AssetSystemBuilderDialog != null)
            {
                m_AssetSystemBuilderDialog.QueueFree();
                m_AssetSystemBuilderDialog = null;
            }

            if (m_ScriptingDefineSymbolsWindow != null)
            {
                m_ScriptingDefineSymbolsWindow.QueueFree();
                m_ScriptingDefineSymbolsWindow = null;
            }

            // Godot 4 会自动清理 InspectorPlugin，无需手动移除 / Godot 4 automatically cleans up InspectorPlugins, no need to remove manually
            m_BaseComponentInspector = null;
            m_ObjectPoolComponentInspector = null;
            m_ReferencePoolComponentInspector = null;
            m_TopMenuButton = null;
            m_TopPopupMenu = null;
            m_LogDefinePopupMenu = null;
            m_CurrentLocale = null;
            m_RuntimeLogBridge = null;
        }

        private void OnDefineSymbolsChanged()
        {
            GD.Print("[ScriptingDefineSymbols] define constants changed, cleaning editor windows for hot-reload.");
            CloseTransientEditorWindowsForDefineSwitch();
        }

        private void CloseTransientEditorWindowsForDefineSwitch()
        {
            SafeCloseWindow(ref m_AssetSystemBuilderDialog);
        }

        private static void SafeCloseWindow<TWindow>(ref TWindow window) where TWindow : Window
        {
            if (window == null)
            {
                return;
            }

            if (GodotObject.IsInstanceValid(window))
            {
                window.Hide();
                window.QueueFree();
            }

            window = null;
        }

        /// <summary>
        /// 功能：监听语言变化并自动刷新菜单文本。
        /// </summary>
        /// <param name="delta">帧间隔时间。</param>
        public override void _Process(double delta)
        {
            m_RuntimeLogBridge?.Tick(delta);
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

            CleanupStaleTopMenuButtons();

            m_TopMenuButton = new MenuButton();
            m_TopMenuButton.Name = TopMenuButtonNodeName;
            m_TopMenuButton.Text = "GameFrameX";

            m_TopPopupMenu = m_TopMenuButton.GetPopup();
            m_TopPopupMenu.AddItem(L("资源打包器", "Asset Builder"), TopMenuAssetBuilderId);
            m_TopPopupMenu.AddItem(L("生成客户端配置", "Generate Client Config"), TopMenuGenerateClientConfigId);
            m_TopPopupMenu.AddSeparator();
            ReconnectPopupMenuIdPressed(m_TopPopupMenu, nameof(OnTopMenuIdPressed));
            BuildLogDefineSubmenu();
            if (m_LogDefinePopupMenu != null)
            {
                m_TopPopupMenu.AddSubmenuNodeItem(L("脚本宏定义", "Scripting Define Symbols"), m_LogDefinePopupMenu);
            }

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
            m_LogDefinePopupMenu.AddItem(L("打开宏窗口", "Open Define Window"), LogDefineOpenWindowId);
            m_LogDefinePopupMenu.AddSeparator();
            m_LogDefinePopupMenu.AddItem(L("禁用所有日志", "Disable All Logs"), LogDefineDisableAllLogsId);
            m_LogDefinePopupMenu.AddItem(L("开启所有日志", "Enable All Logs"), LogDefineEnableAllLogsId);
            m_LogDefinePopupMenu.AddSeparator();
            m_LogDefinePopupMenu.AddItem(L("开启调试及以上日志", "Enable Debug+ Logs"), LogDefineEnableDebugAndAboveLogsId);
            m_LogDefinePopupMenu.AddItem(L("开启信息及以上日志", "Enable Info+ Logs"), LogDefineEnableInfoAndAboveLogsId);
            m_LogDefinePopupMenu.AddItem(L("开启警告及以上日志", "Enable Warning+ Logs"), LogDefineEnableWarningAndAboveLogsId);
            m_LogDefinePopupMenu.AddItem(L("开启错误及以上日志", "Enable Error+ Logs"), LogDefineEnableErrorAndAboveLogsId);
            m_LogDefinePopupMenu.AddItem(L("开启严重错误及以上日志", "Enable Fatal+ Logs"), LogDefineEnableFatalAndAboveLogsId);
            ReconnectPopupMenuIdPressed(m_LogDefinePopupMenu, nameof(OnLogDefineMenuIdPressed));
            m_TopPopupMenu.AddChild(m_LogDefinePopupMenu);
        }

        /// <summary>
        /// 功能：注销编辑器顶部工具栏下拉菜单。
        /// </summary>
        private void UnregisterTopToolbarMenu()
        {
            if (m_LogDefinePopupMenu != null)
            {
                DisconnectPopupMenuIdPressed(m_LogDefinePopupMenu, nameof(OnLogDefineMenuIdPressed));

                m_LogDefinePopupMenu.QueueFree();
                m_LogDefinePopupMenu = null;
            }

            m_TopPopupMenu = null;

            if (m_TopMenuButton != null)
            {
                PopupMenu popupMenu = m_TopMenuButton.GetPopup();
                if (popupMenu != null)
                {
                    DisconnectPopupMenuIdPressed(popupMenu, nameof(OnTopMenuIdPressed));
                }

                RemoveControlFromContainer(CustomControlContainer.Toolbar, m_TopMenuButton);
                m_TopMenuButton.QueueFree();
                m_TopMenuButton = null;
            }

        }

        private void CleanupStaleTopMenuButtons()
        {
            var root = EditorInterface.Singleton?.GetBaseControl();
            if (root == null)
            {
                return;
            }

            CleanupStaleTopMenuButtonsRecursive(root);
        }

        private void CleanupStaleTopMenuButtonsRecursive(Node node)
        {
            foreach (var childObj in node.GetChildren())
            {
                if (childObj is not Node childNode)
                {
                    continue;
                }

                CleanupStaleTopMenuButtonsRecursive(childNode);
                if (childNode is not MenuButton staleButton)
                {
                    continue;
                }

                if (!string.Equals(staleButton.Name, TopMenuButtonNodeName, StringComparison.Ordinal))
                {
                    continue;
                }

                if (staleButton == m_TopMenuButton)
                {
                    continue;
                }

                var stalePopup = staleButton.GetPopup();
                if (stalePopup != null)
                {
                    DisconnectPopupMenuIdPressed(stalePopup, nameof(OnTopMenuIdPressed));
                }

                staleButton.QueueFree();
            }
        }

        private void ReconnectPopupMenuIdPressed(PopupMenu popupMenu, string methodName)
        {
            if (popupMenu == null || string.IsNullOrWhiteSpace(methodName))
            {
                return;
            }

            var callable = new Callable(this, methodName);
            if (popupMenu.IsConnected(PopupMenu.SignalName.IdPressed, callable))
            {
                popupMenu.Disconnect(PopupMenu.SignalName.IdPressed, callable);
            }

            popupMenu.Connect(PopupMenu.SignalName.IdPressed, callable);
        }

        private void DisconnectPopupMenuIdPressed(PopupMenu popupMenu, string methodName)
        {
            if (popupMenu == null || string.IsNullOrWhiteSpace(methodName))
            {
                return;
            }

            var callable = new Callable(this, methodName);
            if (popupMenu.IsConnected(PopupMenu.SignalName.IdPressed, callable))
            {
                popupMenu.Disconnect(PopupMenu.SignalName.IdPressed, callable);
            }
        }

        /// <summary>
        /// 功能：处理 GameFrameX 顶层菜单点击。
        /// </summary>
        /// <param name="id">菜单项标识。</param>
        private void OnTopMenuIdPressed(long id)
        {
            if (id == TopMenuAssetBuilderId)
            {
                if (!ShowUnifiedAssetBuilderDialog() && global::AssetSystemEditorPlugin.RequestOpenBuilderFromCompatibilityEntry() == false)
                {
                    GD.PrintErr("无法打开资源打包器：统一窗口与 AssetSystem 入口均不可用。");
                }

                return;
            }

            if (id == TopMenuGenerateClientConfigId)
            {
                RunGenerateClientConfig();
            }
        }

        private void RunGenerateClientConfig()
        {
            bool success = ConfigGenerationHelper.GenerateClientJson(out string summary);
            if (success)
            {
                GD.Print(summary);
                return;
            }

            GD.PrintErr(summary);
        }

        private bool ShowUnifiedAssetBuilderDialog()
        {
            try
            {
                if (m_AssetSystemBuilderDialog != null && GodotObject.IsInstanceValid(m_AssetSystemBuilderDialog))
                {
                    m_AssetSystemBuilderDialog.QueueFree();
                    m_AssetSystemBuilderDialog = null;
                }

                if (m_AssetSystemBuilderDialog == null || !GodotObject.IsInstanceValid(m_AssetSystemBuilderDialog))
                {
                    m_AssetSystemBuilderDialog = new AssetSystemBuilderDialog();
                    var parent = EditorInterface.Singleton?.GetBaseControl();
                    if (parent == null)
                    {
                        return false;
                    }

                    parent.AddChild(m_AssetSystemBuilderDialog);
                }

                var popupSize = m_AssetSystemBuilderDialog.Size;
                if (popupSize.X <= 0 || popupSize.Y <= 0)
                {
                    popupSize = new Vector2I(1700, 860);
                }

                m_AssetSystemBuilderDialog.PopupCentered(popupSize);
                m_AssetSystemBuilderDialog.Show();
                return true;
            }
            catch (Exception exception)
            {
                GD.PrintErr($"打开统一资源打包窗口失败: {exception.Message}");
                return false;
            }
        }

        /// <summary>
        /// 功能：处理日志宏定义二级菜单点击事件。
        /// </summary>
        /// <param name="id">菜单项标识。</param>
        private void OnLogDefineMenuIdPressed(long id)
        {
            if (id == LogDefineOpenWindowId)
            {
                ShowScriptingDefineSymbolsWindow();
                return;
            }

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

        private bool ShowScriptingDefineSymbolsWindow()
        {
            try
            {
                var parent = EditorInterface.Singleton?.GetBaseControl();
                if (parent == null)
                {
                    return false;
                }

                if (m_ScriptingDefineSymbolsWindow != null && GodotObject.IsInstanceValid(m_ScriptingDefineSymbolsWindow))
                {
                    m_ScriptingDefineSymbolsWindow.QueueFree();
                    m_ScriptingDefineSymbolsWindow = null;
                }

                if (m_ScriptingDefineSymbolsWindow == null || !GodotObject.IsInstanceValid(m_ScriptingDefineSymbolsWindow))
                {
                    m_ScriptingDefineSymbolsWindow = new ScriptingDefineSymbolsWindow();
                }

                if (m_ScriptingDefineSymbolsWindow.GetParent() is Node currentParent)
                {
                    if (currentParent != parent)
                    {
                        m_ScriptingDefineSymbolsWindow.Reparent(parent);
                    }
                }
                else
                {
                    parent.AddChild(m_ScriptingDefineSymbolsWindow);
                }

                m_ScriptingDefineSymbolsWindow.PrepareForDisplay();
                m_ScriptingDefineSymbolsWindow.MinSize = new Vector2I(980, 700);
                m_ScriptingDefineSymbolsWindow.Size = new Vector2I(1120, 700);
                m_ScriptingDefineSymbolsWindow.PopupCentered(new Vector2I(1120, 700));
                return true;
            }
            catch (Exception exception)
            {
                GD.PrintErr($"打开脚本宏窗口失败: {exception.Message}");
                return false;
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
