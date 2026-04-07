using System;
using GameFrameX.UI.GDGUI.Runtime;
using GameFrameX.UI.Runtime;
using Godot;

namespace Godot.Hotfix.GodotGUI
{
	[OptionUIGroup(UIGroupNameConstants.Normal)]
	[OptionUIConfig(path: "res://Assets/Bundles/Prefabs/UI/GodotUI")]
	public partial class UILogin : GDGUI
	{
		public event Action LoginClicked;

		private Button _loginButton;
		private bool _isLoginButtonBound;

		public override void OnOpen(object userData)
		{
			base.OnOpen(userData);
			GD.Print("[UILogin] OnOpen");
			BindLoginButton();
		}

		public override void OnClose(bool isShutdown, object userData)
		{
			UnbindLoginButton();
			base.OnClose(isShutdown, userData);
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
			LoginClicked?.Invoke();
		}
	}
}
