#if TOOLS
using System;
using Godot;

namespace GameFrameX.Network.Editor
{
    [Tool]
    public partial class GameFrameXNetworkPlugin : EditorPlugin
    {
        /// <summary>
        /// 顶部菜单项：网络宏定义子菜单名称。
        /// </summary>
        private const string TopMenuNetworkDefineSubmenuName = "NetworkScriptingDefineSymbolsSubmenu";

        /// <summary>
        /// 网络宏定义菜单项：关闭强制使用 WebSocket。
        /// </summary>
        private const int NetworkDefineDisableForceWebSocketId = 200;

        /// <summary>
        /// 网络宏定义菜单项：开启强制使用 WebSocket。
        /// </summary>
        private const int NetworkDefineEnableForceWebSocketId = 201;

        /// <summary>
        /// 网络宏定义菜单项：关闭网络接收日志。
        /// </summary>
        private const int NetworkDefineDisableReceiveLogsId = 202;

        /// <summary>
        /// 网络宏定义菜单项：开启网络接收日志。
        /// </summary>
        private const int NetworkDefineEnableReceiveLogsId = 203;

        /// <summary>
        /// 网络宏定义菜单项：关闭网络发送日志。
        /// </summary>
        private const int NetworkDefineDisableSendLogsId = 204;

        /// <summary>
        /// 网络宏定义菜单项：开启网络发送日志。
        /// </summary>
        private const int NetworkDefineEnableSendLogsId = 205;

        private NetworkComponentInspectorPlugin m_InspectorPlugin;
        private PopupMenu m_TopPopupMenu;
        private PopupMenu m_NetworkDefinePopupMenu;
        private string m_CurrentLocale;

        /// <summary>
        /// 功能：注册网络插件的检查器与宏定义菜单。
        /// </summary>
        public override void _EnterTree()
        {
            m_CurrentLocale = TranslationServer.GetLocale();
            m_InspectorPlugin = new NetworkComponentInspectorPlugin();
            AddInspectorPlugin(m_InspectorPlugin);
            CleanupLegacyNetworkToolbarMenu();
            TryRegisterTopToolbarMenu();
            SetProcess(true);
        }

        /// <summary>
        /// 功能：卸载网络插件的检查器与宏定义菜单。
        /// </summary>
        public override void _ExitTree()
        {
            SetProcess(false);
            UnregisterTopToolbarMenu();
            m_InspectorPlugin = null;
            m_CurrentLocale = null;
        }

        /// <summary>
        /// 功能：监听语言变化并刷新菜单文案。
        /// </summary>
        /// <param name="delta">帧间隔时间。</param>
        public override void _Process(double delta)
        {
            CleanupLegacyNetworkToolbarMenu();
            TryRegisterTopToolbarMenu();

            string locale = TranslationServer.GetLocale();
            if (string.Equals(locale, m_CurrentLocale, StringComparison.Ordinal))
            {
                return;
            }

            m_CurrentLocale = locale;
            RebuildTopToolbarMenu();
        }

        /// <summary>
        /// 功能：尝试注册到 GameFrameX 顶部工具栏菜单。
        /// </summary>
        private void TryRegisterTopToolbarMenu()
        {
            if (m_NetworkDefinePopupMenu != null && GodotObject.IsInstanceValid(m_NetworkDefinePopupMenu))
            {
                return;
            }

            Control editorBaseControl = EditorInterface.Singleton?.GetBaseControl();
            if (editorBaseControl == null)
            {
                return;
            }

            MenuButton gameFrameXMenuButton = FindGameFrameXMenuButton(editorBaseControl);
            if (gameFrameXMenuButton == null)
            {
                return;
            }

            m_TopPopupMenu = gameFrameXMenuButton.GetPopup();
            if (m_TopPopupMenu == null)
            {
                return;
            }

            m_NetworkDefinePopupMenu = new PopupMenu();
            m_NetworkDefinePopupMenu.Name = TopMenuNetworkDefineSubmenuName;
            m_NetworkDefinePopupMenu.AddItem(L("关闭强制使用 WebSocket", "Disable Force WebSocket"), NetworkDefineDisableForceWebSocketId);
            m_NetworkDefinePopupMenu.AddItem(L("开启强制使用 WebSocket", "Enable Force WebSocket"), NetworkDefineEnableForceWebSocketId);
            m_NetworkDefinePopupMenu.AddSeparator();
            m_NetworkDefinePopupMenu.AddItem(L("关闭网络接收日志", "Disable Network Receive Logs"), NetworkDefineDisableReceiveLogsId);
            m_NetworkDefinePopupMenu.AddItem(L("开启网络接收日志", "Enable Network Receive Logs"), NetworkDefineEnableReceiveLogsId);
            m_NetworkDefinePopupMenu.AddSeparator();
            m_NetworkDefinePopupMenu.AddItem(L("关闭网络发送日志", "Disable Network Send Logs"), NetworkDefineDisableSendLogsId);
            m_NetworkDefinePopupMenu.AddItem(L("开启网络发送日志", "Enable Network Send Logs"), NetworkDefineEnableSendLogsId);
            m_NetworkDefinePopupMenu.IdPressed += OnNetworkDefineMenuIdPressed;
            m_TopPopupMenu.AddChild(m_NetworkDefinePopupMenu);
            m_TopPopupMenu.AddSubmenuNodeItem(L("网络宏定义", "Network Define Symbols"), m_NetworkDefinePopupMenu);
        }

        /// <summary>
        /// 功能：注销顶部工具栏菜单。
        /// </summary>
        private void UnregisterTopToolbarMenu()
        {
            if (m_NetworkDefinePopupMenu != null)
            {
                m_NetworkDefinePopupMenu.IdPressed -= OnNetworkDefineMenuIdPressed;
                RemoveNetworkDefineSubmenuItem();
                m_NetworkDefinePopupMenu.QueueFree();
                m_NetworkDefinePopupMenu = null;
            }

            m_TopPopupMenu = null;
        }

        /// <summary>
        /// 功能：重建顶部工具栏菜单。
        /// </summary>
        private void RebuildTopToolbarMenu()
        {
            UnregisterTopToolbarMenu();
            TryRegisterTopToolbarMenu();
        }

        /// <summary>
        /// 功能：从顶部菜单中移除网络宏定义子菜单入口。
        /// </summary>
        private void RemoveNetworkDefineSubmenuItem()
        {
            if (m_TopPopupMenu == null || m_NetworkDefinePopupMenu == null)
            {
                return;
            }

            string submenuName = m_NetworkDefinePopupMenu.Name;
            int itemCount = m_TopPopupMenu.ItemCount;
            for (int i = itemCount - 1; i >= 0; i--)
            {
                PopupMenu submenuNode = m_TopPopupMenu.GetItemSubmenuNode(i);
                if (submenuNode == null || !string.Equals(submenuNode.Name, submenuName, StringComparison.Ordinal))
                {
                    continue;
                }

                m_TopPopupMenu.RemoveItem(i);
            }
        }

        /// <summary>
        /// 功能：处理网络宏定义菜单点击。
        /// </summary>
        /// <param name="id">菜单项标识。</param>
        private void OnNetworkDefineMenuIdPressed(long id)
        {
            if (id == NetworkDefineDisableForceWebSocketId)
            {
                ApplyNetworkDefineAction(NetworkScriptingDefineSymbols.DisableForceWebSocketNetwork, "已关闭强制使用 WebSocket 网络宏定义。");
                return;
            }

            if (id == NetworkDefineEnableForceWebSocketId)
            {
                ApplyNetworkDefineAction(NetworkScriptingDefineSymbols.EnableForceWebSocketNetwork, "已开启强制使用 WebSocket 网络宏定义。");
                return;
            }

            if (id == NetworkDefineDisableReceiveLogsId)
            {
                ApplyNetworkDefineAction(NetworkScriptingDefineSymbols.DisableNetworkReceiveLogs, "已关闭网络接收日志宏定义。");
                return;
            }

            if (id == NetworkDefineEnableReceiveLogsId)
            {
                ApplyNetworkDefineAction(NetworkScriptingDefineSymbols.EnableNetworkReceiveLogs, "已开启网络接收日志宏定义。");
                return;
            }

            if (id == NetworkDefineDisableSendLogsId)
            {
                ApplyNetworkDefineAction(NetworkScriptingDefineSymbols.DisableNetworkSendLogs, "已关闭网络发送日志宏定义。");
                return;
            }

            if (id == NetworkDefineEnableSendLogsId)
            {
                ApplyNetworkDefineAction(NetworkScriptingDefineSymbols.EnableNetworkSendLogs, "已开启网络发送日志宏定义。");
            }
        }

        /// <summary>
        /// 功能：执行网络宏定义菜单动作。
        /// </summary>
        /// <param name="action">动作方法。</param>
        /// <param name="status">状态输出。</param>
        private void ApplyNetworkDefineAction(Action action, string status)
        {
            action?.Invoke();
            GD.Print(status);
        }

        /// <summary>
        /// 功能：查找顶部工具栏中的 GameFrameX 菜单按钮。
        /// </summary>
        /// <param name="root">查找起始节点。</param>
        /// <returns>菜单按钮实例。</returns>
        private MenuButton FindGameFrameXMenuButton(Node root)
        {
            if (root is MenuButton menuButton && string.Equals(menuButton.Text, "GameFrameX", StringComparison.Ordinal))
            {
                return menuButton;
            }

            foreach (Node child in root.GetChildren())
            {
                MenuButton found = FindGameFrameXMenuButton(child);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        /// <summary>
        /// 功能：清理旧版本遗留的 GameFrameX Network 顶部按钮。
        /// </summary>
        private void CleanupLegacyNetworkToolbarMenu()
        {
            Control editorBaseControl = EditorInterface.Singleton?.GetBaseControl();
            if (editorBaseControl == null)
            {
                return;
            }

            MenuButton legacyMenuButton = FindMenuButtonByText(editorBaseControl, "GameFrameX Network");
            if (legacyMenuButton == null)
            {
                return;
            }

            Node parent = legacyMenuButton.GetParent();
            if (parent is Container container)
            {
                container.RemoveChild(legacyMenuButton);
            }

            legacyMenuButton.QueueFree();
        }

        /// <summary>
        /// 功能：按按钮文本查找菜单按钮。
        /// </summary>
        /// <param name="root">查找起始节点。</param>
        /// <param name="text">目标按钮文本。</param>
        /// <returns>菜单按钮实例。</returns>
        private MenuButton FindMenuButtonByText(Node root, string text)
        {
            if (root is MenuButton menuButton && string.Equals(menuButton.Text, text, StringComparison.Ordinal))
            {
                return menuButton;
            }

            foreach (Node child in root.GetChildren())
            {
                MenuButton found = FindMenuButtonByText(child, text);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }

        /// <summary>
        /// 功能：根据当前语言返回本地化文案。
        /// </summary>
        /// <param name="zh">中文文案。</param>
        /// <param name="en">英文文案。</param>
        /// <returns>本地化后的文案。</returns>
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
