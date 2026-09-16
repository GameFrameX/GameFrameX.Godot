using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameFrameX.Runtime;
using GameFrameX.Web.Runtime;
using FairyGUI;
using Godot;
using Godot.Startup.Account;
using Godot.Startup.Network;
#if HOTFIX_RUNTIME
using Godot.Hotfix.Config;
#endif

namespace Godot.Hotfix.FairyGUI
{
    public partial class UILogin
    {
        public event Action LoginClicked;

        private const string SmokeHttpUrl = "http://127.0.0.1:8080/game/api/test";

        private GComponent _view;
        private GObject _loginTrigger;
        private GTextInput _userNameInput;
        private GTextInput _passwordInput;
        private GTextField _errorText;

        public override void OnOpen(object userData)
        {
            base.OnOpen(userData);
            GD.Print("[UILogin-FGUI] OnOpen");
            GD.Print("[ProtoSmoke] trigger from FGUI.UILogin.OnOpen");
#pragma warning disable CS4014
            LoginProtoMessageSmoke.SendOnLoginOpenAsync("FGUI.UILogin.OnOpen");
#pragma warning restore CS4014
#if HOTFIX_RUNTIME
            _ = ConfigRuntimeDispatcher.EnsureLoadedAndLogDemoAsync("FairyGUI.UILogin");
#endif

            UnbindLoginTrigger();
            FairyGuiRuntimeBridge.DisposeView(ref _view);

            _view = FairyGuiRuntimeBridge.CreateFullScreenView("UILogin", "UILogin", UIGroup?.Name);
            if (_view == null)
            {
                GD.PushError("[UILogin-FGUI] create fullscreen view failed.");
                return;
            }

            _userNameInput = _view.GetChild("UserName")?.asTextInput;
            _passwordInput = _view.GetChild("Password")?.asTextInput;
            _errorText = _view.GetChild("ErrorText")?.asTextField;
            _loginTrigger = _view.GetChild("enter");
            if (_loginTrigger == null)
            {
                GD.PushWarning("[UILogin-FGUI] enter button not found.");
                return;
            }

            if (_userNameInput != null)
            {
                _userNameInput.text = string.Empty;
            }

            if (_passwordInput != null)
            {
                _passwordInput.text = string.Empty;
            }

            SetErrorText(string.Empty);
            _loginTrigger.onClick.Add(OnLoginClicked);
            GD.Print("[UILogin-FGUI] login trigger bound.");
        }

        public override void OnClose(bool isShutdown, object userData)
        {
            UnbindLoginTrigger();
            _userNameInput = null;
            _passwordInput = null;
            _errorText = null;
            FairyGuiRuntimeBridge.DisposeView(ref _view);
            base.OnClose(isShutdown, userData);
        }

        public override void _ExitTree()
        {
            UnbindLoginTrigger();
            _userNameInput = null;
            _passwordInput = null;
            _errorText = null;
            FairyGuiRuntimeBridge.DisposeView(ref _view);
            base._ExitTree();
        }

        /// <summary>
        /// 自动化入口：等价于用户填入账号密码并点击“进入游戏”按钮。
        /// </summary>
        public void AutoSubmitLogin(string account, string password)
        {
            if (_userNameInput != null)
            {
                _userNameInput.text = account;
            }

            if (_passwordInput != null)
            {
                _passwordInput.text = password;
            }

            OnLoginClicked();
        }

        private void OnLoginClicked()
        {
            GD.Print("[UILogin-FGUI] login trigger clicked.");
            var account = _userNameInput?.text?.Trim() ?? string.Empty;
            var password = _passwordInput?.text ?? string.Empty;
            if (account.Length == 0)
            {
                SetErrorText("请输入账号");
                return;
            }

            if (password.Length == 0)
            {
                SetErrorText("请输入密码");
                return;
            }

            if (!LocalAccountStore.TryLoginOrCreate(account, password, out var created, out var error))
            {
                SetErrorText(error);
                return;
            }

            SetErrorText(string.Empty);
            GD.Print(created
                ? $"[UILogin-FGUI] 账号创建成功并登录：{account}"
                : $"[UILogin-FGUI] 登录成功：{account}");
            _ = TryRequestHttpSmokeAsync();
            LoginClicked?.Invoke();
        }

        private void SetErrorText(string message)
        {
            if (_errorText != null)
            {
                _errorText.text = message;
            }
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

        private static async Task TryRequestHttpSmokeAsync()
        {
            try
            {
                var webComponent = GameEntry.GetComponent<WebComponent>();
                if (webComponent == null)
                {
                    GD.PushWarning("[UILogin-FGUI] WebComponent not found, skip HTTP smoke test.");
                    return;
                }

                var body = new Dictionary<string, object>(StringComparer.Ordinal)
                {
                    ["ping"] = "1"
                };
                var response = await webComponent.PostToString(SmokeHttpUrl, body).ConfigureAwait(false);
                var responseText = response?.Result ?? string.Empty;
                GD.Print($"[UILogin-FGUI] smoke HTTP success: {SmokeHttpUrl}, result: {responseText}");
            }
            catch (Exception exception)
            {
                GD.PushError($"[UILogin-FGUI] smoke HTTP failed: {exception.Message}");
            }
        }
    }
}
