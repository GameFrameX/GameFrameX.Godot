using System;
using FairyGUI;
using Godot;
using Godot.Startup.Account;

namespace Godot.Hotfix.FairyGUI
{
    public partial class UIPlayerCreateForm
    {
        /// <summary>角色创建成功后触发。</summary>
        public event Action RoleCreated;

        private GComponent _view;
        private GObject _createTrigger;
        private GTextInput _roleNameInput;
        private GTextField _errorText;

        public override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            GD.Print("[UIPlayerCreate-FGUI] OnOpen");

            DisposeView();
            _view = FairyGuiRuntimeBridge.CreateFullScreenView("UILogin", "UIPlayerCreate", UIGroup?.Name);
            if (_view == null)
            {
                GD.PushError("[UIPlayerCreate-FGUI] create fullscreen view failed.");
                return;
            }

            _roleNameInput = _view.GetChild("UserName")?.asTextInput;
            _errorText = _view.GetChild("ErrorText")?.asTextField;
            _createTrigger = _view.GetChild("enter");
            if (_createTrigger == null)
            {
                GD.PushWarning("[UIPlayerCreate-FGUI] enter button not found.");
                return;
            }

            if (_roleNameInput != null)
            {
                _roleNameInput.text = string.Empty;
            }

            SetErrorText(string.Empty);
            _createTrigger.onClick.Add(OnCreateClicked);
            GD.Print("[UIPlayerCreate-FGUI] create trigger bound.");
        }

        public override void OnClose(bool isShutdown, object userData)
        {
            DisposeView();
            base.OnClose(isShutdown, userData);
        }

        public override void _ExitTree()
        {
            DisposeView();
            base._ExitTree();
        }

        /// <summary>
        /// 自动化入口：等价于用户填入角色名并点击“创建”按钮。
        /// </summary>
        public void AutoSubmitCreate(string roleName)
        {
            if (_roleNameInput != null)
            {
                _roleNameInput.text = roleName;
            }

            OnCreateClicked();
        }

        private void OnCreateClicked()
        {
            GD.Print("[UIPlayerCreate-FGUI] create trigger clicked.");
            var roleName = _roleNameInput?.text?.Trim() ?? string.Empty;
            if (!LocalAccountStore.TryCreateRole(roleName, out var role, out var error))
            {
                SetErrorText(error);
                return;
            }

            SetErrorText(string.Empty);
            GD.Print($"[UIPlayerCreate-FGUI] 角色创建成功并进入游戏：{role.Name}");
            RoleCreated?.Invoke();
        }

        private void SetErrorText(string message)
        {
            if (_errorText != null)
            {
                _errorText.text = message;
            }
        }

        private void DisposeView()
        {
            if (_createTrigger != null)
            {
                _createTrigger.onClick.Remove(OnCreateClicked);
                _createTrigger = null;
            }

            _roleNameInput = null;
            _errorText = null;
            FairyGuiRuntimeBridge.DisposeView(ref _view);
        }
    }
}
