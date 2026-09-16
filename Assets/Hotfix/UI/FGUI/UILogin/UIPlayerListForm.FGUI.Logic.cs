using System;
using System.Collections.Generic;
using FairyGUI;
using Godot;
using Godot.Startup.Account;

namespace Godot.Hotfix.FairyGUI
{
    public partial class UIPlayerListForm
    {
        /// <summary>角色被选中并点击进入游戏后触发。</summary>
        public event Action RoleSelected;

        /// <summary>UIPlayerListItem 组件资源地址（来源：UIPlayerListItem.cs 生成的 URL 常量）。</summary>
        private const string PlayerListItemUrl = "ui://f011l0h9i3dbs9n";

        private GComponent _view;
        private GList _playerList;
        private GButton _loginButton;
        private GTextField _selectedName;
        private GTextField _selectedLevel;
        private Controller _isSelectedController;
        private List<LocalAccountStore.RoleRecord> _roles = new();
        private int _selectedIndex = -1;

        public override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            GD.Print("[UIPlayerList-FGUI] OnOpen");

            DisposeView();
            _view = FairyGuiRuntimeBridge.CreateFullScreenView("UILogin", "UIPlayerList", UIGroup?.Name);
            if (_view == null)
            {
                GD.PushError("[UIPlayerList-FGUI] create fullscreen view failed.");
                return;
            }

            _playerList = _view.GetChild("player_list")?.asList;
            _loginButton = _view.GetChild("login_button")?.asButton;
            _selectedName = _view.GetChild("selected_name")?.asTextField;
            _selectedLevel = _view.GetChild("selected_level")?.asTextField;
            _isSelectedController = _view.GetController("IsSelected");

            if (_loginButton != null)
            {
                _loginButton.onClick.Add(OnLoginButtonClicked);
            }

            ReloadRoles();
        }

        public override void OnClose(bool isShutdown, object userData)
        {
            DisposeView();
            _roles.Clear();
            _selectedIndex = -1;
            base.OnClose(isShutdown, userData);
        }

        public override void _ExitTree()
        {
            DisposeView();
            base._ExitTree();
        }

        /// <summary>
        /// 自动化入口：等价于用户点选第 index 个角色并点击“进入游戏”按钮。
        /// </summary>
        public void AutoSelectRoleAndEnter(int index)
        {
            SelectRole(index);
            OnLoginButtonClicked();
        }

        private void ReloadRoles()
        {
            _roles = LocalAccountStore.GetCurrentRoles();
            _selectedIndex = -1;
            ApplySelection();
            if (_playerList == null)
            {
                GD.PushWarning("[UIPlayerList-FGUI] player_list not found.");
                return;
            }

            _playerList.RemoveChildrenToPool();
            for (var i = 0; i < _roles.Count; i++)
            {
                AddRoleItem(i);
            }

            GD.Print($"[UIPlayerList-FGUI] roles loaded. count={_roles.Count} account={LocalAccountStore.CurrentAccountName}");
        }

        private void AddRoleItem(int index)
        {
            var role = _roles[index];
            var item = _playerList.AddItemFromPool(PlayerListItemUrl);
            if (item == null)
            {
                GD.PushError($"[UIPlayerList-FGUI] create list item failed. url={PlayerListItemUrl}");
                return;
            }

            var itemCom = item.asCom;
            var nameText = itemCom?.GetChild("name_text")?.asTextField;
            var levelText = itemCom?.GetChild("level_text")?.asTextField;
            if (nameText != null)
            {
                nameText.text = role.Name;
            }

            if (levelText != null)
            {
                levelText.text = $"Lv.{role.Level}";
            }

            item.onClick.Add(() => SelectRole(index));
        }

        private void SelectRole(int index)
        {
            if (index < 0 || index >= _roles.Count)
            {
                return;
            }

            _selectedIndex = index;
            ApplySelection();
            GD.Print($"[UIPlayerList-FGUI] role selected. index={index} name={_roles[index].Name}");
        }

        private void ApplySelection()
        {
            var hasSelection = _selectedIndex >= 0 && _selectedIndex < _roles.Count;
            if (_selectedName != null)
            {
                _selectedName.text = hasSelection ? _roles[_selectedIndex].Name : string.Empty;
            }

            if (_selectedLevel != null)
            {
                _selectedLevel.text = hasSelection ? $"Lv.{_roles[_selectedIndex].Level}" : string.Empty;
            }

            if (_isSelectedController != null)
            {
                try
                {
                    _isSelectedController.SetSelectedPage(hasSelection ? "Yes" : "No");
                }
                catch (Exception exception)
                {
                    GD.PushWarning($"[UIPlayerList-FGUI] switch IsSelected controller failed. {exception.Message}");
                }
            }
        }

        private void OnLoginButtonClicked()
        {
            if (_selectedIndex < 0 || _selectedIndex >= _roles.Count)
            {
                GD.PushWarning("[UIPlayerList-FGUI] login button clicked without role selection.");
                return;
            }

            LocalAccountStore.SelectRole(_roles[_selectedIndex]);
            RoleSelected?.Invoke();
        }

        private void DisposeView()
        {
            if (_loginButton != null)
            {
                _loginButton.onClick.Remove(OnLoginButtonClicked);
                _loginButton = null;
            }

            _playerList = null;
            _selectedName = null;
            _selectedLevel = null;
            _isSelectedController = null;
            FairyGuiRuntimeBridge.DisposeView(ref _view);
        }
    }
}
