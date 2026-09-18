#if TOOLS
using System;
using Godot;

namespace GameFrameX.UI.Editor
{
    [Tool]
    public partial class GameFrameXUIPlugin : EditorPlugin
    {
        /// <summary>
        /// 顶部菜单项：UI 宏定义子菜单。
        /// </summary>
        private const string TopMenuUiDefineSubmenuName = "UIScriptingDefineSymbolsSubmenu";

        /// <summary>
        /// UI 宏定义菜单项：切换到 FairyGUI。
        /// </summary>
        private const int UiDefineUseFairyGuiId = 300;

        /// <summary>
        /// UI 宏定义菜单项：切换到 Godot GUI（GDGUI）。
        /// </summary>
        private const int UiDefineUseGodotGuiId = 301;

        private UIComponentInspectorPlugin m_UIComponentInspectorPlugin;
        private UIFormInspectorPlugin m_UIFormInspectorPlugin;
        private PopupMenu m_UiDefinePopupMenu;
        private PopupMenu m_TopPopupMenu;
        private string m_CurrentLocale;

        public override void _EnterTree()
        {
            m_CurrentLocale = TranslationServer.GetLocale();

            m_UIComponentInspectorPlugin = new UIComponentInspectorPlugin();
            AddInspectorPlugin(m_UIComponentInspectorPlugin);

            m_UIFormInspectorPlugin = new UIFormInspectorPlugin();
            AddInspectorPlugin(m_UIFormInspectorPlugin);

            SetProcess(true);
        }

        public override void _ExitTree()
        {
            SetProcess(false);
            UnregisterTopToolbarMenu();
            m_UIComponentInspectorPlugin = null;
            m_UIFormInspectorPlugin = null;
            m_CurrentLocale = null;
        }

        public override void _Process(double delta)
        {
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
            if (m_UiDefinePopupMenu != null && GodotObject.IsInstanceValid(m_UiDefinePopupMenu))
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

            m_UiDefinePopupMenu = new PopupMenu();
            m_UiDefinePopupMenu.Name = TopMenuUiDefineSubmenuName;
            m_UiDefinePopupMenu.AddItem(L("切换到 FairyGUI", "Switch to FairyGUI"), UiDefineUseFairyGuiId);
            m_UiDefinePopupMenu.AddItem(L("切换到 Godot GUI (GDGUI)", "Switch to Godot GUI (GDGUI)"), UiDefineUseGodotGuiId);
            m_UiDefinePopupMenu.IdPressed += OnUiDefineMenuIdPressed;
            m_TopPopupMenu.AddChild(m_UiDefinePopupMenu);
            m_TopPopupMenu.AddSubmenuNodeItem(L("UI 宏定义", "UI Define Symbols"), m_UiDefinePopupMenu);
        }

        /// <summary>
        /// 功能：注销顶部工具栏菜单。
        /// </summary>
        private void UnregisterTopToolbarMenu()
        {
            if (m_UiDefinePopupMenu != null)
            {
                m_UiDefinePopupMenu.IdPressed -= OnUiDefineMenuIdPressed;
                RemoveUiDefineSubmenuItem();
                m_UiDefinePopupMenu.QueueFree();
                m_UiDefinePopupMenu = null;
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
        /// 功能：从顶部菜单中移除 UI 宏定义子菜单入口。
        /// </summary>
        private void RemoveUiDefineSubmenuItem()
        {
            if (m_TopPopupMenu == null || m_UiDefinePopupMenu == null)
            {
                return;
            }

            string submenuName = m_UiDefinePopupMenu.Name;
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
        /// 功能：处理 UI 宏定义菜单点击（互斥切换，一次写回全部工程并触发重编译）。
        /// </summary>
        /// <param name="id">菜单项标识。</param>
        private void OnUiDefineMenuIdPressed(long id)
        {
            if (id == UiDefineUseFairyGuiId)
            {
                ApplyUiDefineAction(() => UIScriptingDefineSymbols.UseFairyGui(), "已切换 UI 后端编译宏：ENABLE_UI_FAIRYGUI（FairyGUI），即将重编译。");
                return;
            }

            if (id == UiDefineUseGodotGuiId)
            {
                ApplyUiDefineAction(() => UIScriptingDefineSymbols.UseGodotGui(), "已切换 UI 后端编译宏：ENABLE_UI_GDGUI（Godot GUI），即将重编译。");
            }
        }

        /// <summary>
        /// 功能：执行 UI 宏定义菜单动作。
        /// </summary>
        /// <param name="action">动作方法。</param>
        /// <param name="status">状态输出。</param>
        private void ApplyUiDefineAction(Action action, string status)
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
