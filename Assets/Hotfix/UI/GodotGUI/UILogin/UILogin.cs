using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameFrameX.Runtime;
using GameFrameX.UI.GDGUI.Runtime;
using GameFrameX.UI.Runtime;
using GameFrameX.Web.Runtime;
using Godot;
using Godot.Startup.Network;
#if HOTFIX_RUNTIME
using Godot.Hotfix.Config;
#endif

namespace Godot.Hotfix.GodotGUI
{
	[OptionUIGroup(UIGroupNameConstants.Normal)]
	public partial class UILogin : GDGUI
	{
		public event Action LoginClicked;
		private const string SmokeChannelName = "smoke";
		private const string SmokeTcpServerUri = "tcp://127.0.0.1:29100";
		private const string SmokeWebSocketServerUri = "ws://127.0.0.1:29110";
		private const string SmokeHttpUrl = "http://127.0.0.1:8080/game/api/test";

		private Button _loginButton;
		private bool _isLoginButtonBound;

		public override void OnOpen(object userData)
		{
			base.OnOpen(userData);
			GD.Print("[UILogin] OnOpen");
			GD.Print("[ProtoSmoke] trigger from GGUI.UILogin.OnOpen");
#pragma warning disable CS4014
			LoginProtoMessageSmoke.SendOnLoginOpenAsync("GGUI.UILogin.OnOpen");
#pragma warning restore CS4014
#if HOTFIX_RUNTIME
			_ = ConfigRuntimeDispatcher.EnsureLoadedAndLogDemoAsync("GodotGUI.UILogin");
#endif
			BindLoginButton();
		}

		public override void OnClose(bool isShutdown, object userData)
		{
			UnbindLoginButton();
			base.OnClose(isShutdown, userData);
		}

		public override void _ExitTree()
		{
			UnbindLoginButton();
			base._ExitTree();
		}

		private void BindLoginButton()
		{
			if (_loginButton == null)
			{
				_loginButton = FindChild("btnLogin", true, false) as Button;
				if (_loginButton == null)
				{
					_loginButton = FindChild("LoginButton", true, false) as Button;
				}
			}

			if (_loginButton == null)
			{
				GD.PushWarning("[UILogin] LoginButton not found.");
				return;
			}

			if (_isLoginButtonBound)
			{
				return;
			}

			_loginButton.Pressed += OnLoginButtonPressed;
			_isLoginButtonBound = true;
			GD.Print($"[UILogin] Login button bound: {_loginButton.Name}");
		}

		private void UnbindLoginButton()
		{
			if (_loginButton == null || !_isLoginButtonBound)
			{
				return;
			}

			_loginButton.Pressed -= OnLoginButtonPressed;
			_isLoginButtonBound = false;
		}

		private void OnLoginButtonPressed()
		{
			GD.Print("[UILogin] Login button pressed");
			_ = TryRequestHttpSmokeAsync();
			LoginClicked?.Invoke();
		}

		private static async Task TryRequestHttpSmokeAsync()
		{
			try
			{
				// WebSocket smoke test (disabled):
				// var network = GameEntry.GetComponent<NetworkComponent>();
				// if (network == null)
				// {
				// 	GD.PushWarning("[UILogin] NetworkComponent not found, skip smoke connect.");
				// 	return;
				// }
				//
				// var channel = network.GetNetworkChannel(SmokeChannelName) ??
				// 			  network.CreateNetworkChannel(SmokeChannelName, new DefaultNetworkChannelHelper());
				// if (channel.Connected)
				// {
				// 	GD.Print($"[UILogin] smoke channel already connected: {SmokeWebSocketServerUri}");
				// 	return;
				// }
				//
				// channel.Connect(new Uri(SmokeWebSocketServerUri));
				// GD.Print($"[UILogin] smoke connecting (WebSocket): {SmokeWebSocketServerUri}");

				var webComponent = GameEntry.GetComponent<WebComponent>();
				if (webComponent == null)
				{
					GD.PushWarning("[UILogin] WebComponent not found, skip HTTP smoke test.");
					return;
				}

				var body = new Dictionary<string, object>(StringComparer.Ordinal)
				{
					["ping"] = "1"
				};
				var response = await webComponent.PostToString(SmokeHttpUrl, body).ConfigureAwait(false);
				var responseText = response?.Result ?? string.Empty;
				GD.Print($"[UILogin] smoke HTTP success: {SmokeHttpUrl}, result: {responseText}");
			}
			catch (Exception exception)
			{
				GD.PushError($"[UILogin] smoke HTTP failed: {exception.Message}");
			}
		}
	}
}
