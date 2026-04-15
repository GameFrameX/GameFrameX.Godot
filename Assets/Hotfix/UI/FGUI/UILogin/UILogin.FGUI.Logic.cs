using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameFrameX.Runtime;
using GameFrameX.Web.Runtime;
using FairyGUI;
using Godot;
using Godot.Startup.Network;
#if HOTFIX_RUNTIME
using Godot.Hotfix.Config;
#endif

namespace Godot.Hotfix.FairyGUI
{
    public partial class UILogin
    {
        public event Action LoginClicked;

        private const string SmokeChannelName = "smoke";
        private const string SmokeTcpServerUri = "tcp://127.0.0.1:29100";
        private const string SmokeWebSocketServerUri = "ws://127.0.0.1:29110";
        private const string SmokeHttpUrl = "http://127.0.0.1:8080/game/api/test";

        private GComponent _view;
        private GObject _loginTrigger;

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

        public override void _ExitTree()
        {
            UnbindLoginTrigger();
            FairyGuiRuntimeBridge.DisposeView(ref _view);
            base._ExitTree();
        }

        private void OnLoginClicked()
        {
            GD.Print("[UILogin-FGUI] login trigger clicked.");
            _ = TryRequestHttpSmokeAsync();
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

        private static async Task TryRequestHttpSmokeAsync()
        {
            try
            {
                // WebSocket smoke test (disabled):
                // var network = GameEntry.GetComponent<NetworkComponent>();
                // if (network == null)
                // {
                //     GD.PushWarning("[UILogin-FGUI] NetworkComponent not found, skip smoke connect.");
                //     return;
                // }
                //
                // var channel = network.GetNetworkChannel(SmokeChannelName) ??
                //               network.CreateNetworkChannel(SmokeChannelName, new DefaultNetworkChannelHelper());
                // if (channel.Connected)
                // {
                //     GD.Print($"[UILogin-FGUI] smoke channel already connected: {SmokeWebSocketServerUri}");
                //     return;
                // }
                //
                // channel.Connect(new Uri(SmokeWebSocketServerUri));
                // GD.Print($"[UILogin-FGUI] smoke connecting (WebSocket): {SmokeWebSocketServerUri}");

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
