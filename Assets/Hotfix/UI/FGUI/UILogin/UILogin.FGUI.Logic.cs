using System;
using FairyGUI;
using Godot;
using Godot.Hotfix.Config;

namespace Godot.Hotfix.FairyGUI
{
    public partial class UILogin
    {
        public event Action LoginClicked;

        private GComponent _view;
        private GObject _loginTrigger;

        public override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            GD.Print("[UILogin-FGUI] OnOpen");
            _ = ConfigRuntimeDispatcher.EnsureLoadedAndLogDemoAsync("FairyGUI.UILogin");

            UnbindLoginTrigger();
            FairyGuiRuntimeBridge.DisposeView(ref _view);

            _view = FairyGuiRuntimeBridge.CreateFullScreenView("UILogin", "UILogin", UIGroup?.Name);
            if (_view == null)
            {
                GD.PushError("[UILogin-FGUI] create fullscreen view failed.");
                return;
            }

            _loginTrigger = _view.GetChild("enter");
            if (_loginTrigger == null)
            {
                GD.PushWarning("[UILogin-FGUI] enter button not found.");
                return;
            }

            _loginTrigger.onClick.Add(OnLoginClicked);
            GD.Print("[UILogin-FGUI] login trigger bound.");
        }

        public override void OnClose(bool isShutdown, object userData)
        {
            UnbindLoginTrigger();
            FairyGuiRuntimeBridge.DisposeView(ref _view);
            base.OnClose(isShutdown, userData);
        }

        private void OnLoginClicked()
        {
            GD.Print("[UILogin-FGUI] login trigger clicked.");
            LoginClicked?.Invoke();
        }

        private void UnbindLoginTrigger()
        {
            if (_loginTrigger == null)
            {
                return;
            }

            _loginTrigger.onClick.Remove(OnLoginClicked);
            _loginTrigger = null;
        }
    }
}
