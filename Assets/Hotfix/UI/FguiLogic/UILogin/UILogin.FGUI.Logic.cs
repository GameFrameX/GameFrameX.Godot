using System;
using FairyGUI;
using Godot;

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

            UnbindLoginTrigger();
            FairyGuiRuntimeBridge.DisposeView(ref _view);

            _view = FairyGuiRuntimeBridge.CreateFullScreenView("UILogin", "UILogin");
            if (_view == null)
            {
                return;
            }

            _loginTrigger = _view.GetChild("enter");
            if (_loginTrigger == null)
            {
                GD.PushWarning("[UILogin-FGUI] enter button not found.");
                return;
            }

            _loginTrigger.onClick.Add(OnLoginClicked);
        }

        public override void OnClose(bool isShutdown, object userData)
        {
            UnbindLoginTrigger();
            FairyGuiRuntimeBridge.DisposeView(ref _view);
            base.OnClose(isShutdown, userData);
        }

        private void OnLoginClicked()
        {
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
