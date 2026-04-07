using System;
using System.Threading.Tasks;
using GameFrameX.Runtime;
using GameFrameX.UI.GDGUI.Runtime;
using GameFrameX.UI.Runtime;
using Godot;

namespace Godot.Hotfix.GodotGUI
{
	[OptionUIGroup(UIGroupNameConstants.Normal)]
	[OptionUIConfig(path: "res://Assets/Bundles/Prefabs/UI/GodotUI")]
	public partial class UILauncher : GDGUI
	{
		private const int LauncherDurationMs = 3000;

		private ProgressBar _progressBar;
		private bool _flowStarted;
		private bool _loginClicked;
		private UILogin _loginForm;

		public override void OnOpen(object userData)
		{
			base.OnOpen(userData);
			EnsureNodes();
			SetProgress(0f);
			if (_flowStarted)
			{
				return;
			}

			_flowStarted = true;
			_ = RunFlowAsync();
		}

		public override void OnClose(bool isShutdown, object userData)
		{
			if (_loginForm != null)
			{
				_loginForm.LoginClicked -= OnLoginClicked;
				_loginForm = null;
			}

			_loginClicked = false;
			_flowStarted = false;
			base.OnClose(isShutdown, userData);
		}

		public void SetProgress(float value)
		{
			EnsureNodes();
			if (_progressBar == null)
			{
				return;
			}

			_progressBar.MinValue = 0;
			_progressBar.MaxValue = 100;
			_progressBar.Value = Mathf.Clamp(value, 0f, 100f);
		}

		private void EnsureNodes()
		{
			if (_progressBar != null)
			{
				return;
			}

			_progressBar = FindChild("ProgressBar", true, false) as ProgressBar;
		}

		private async Task RunFlowAsync()
		{
			try
			{
				var sceneTree = Engine.GetMainLoop() as SceneTree;
				if (sceneTree == null)
				{
					GD.PushWarning("[UILauncher] SceneTree not found.");
					return;
				}

				var uiComponent = GameEntry.GetComponent<UIComponent>();
				if (uiComponent == null)
				{
					GD.PushError("[UILauncher] UIComponent not found.");
					return;
				}

				var startTick = Time.GetTicksMsec();
				while (true)
				{
					var elapsedMs = Time.GetTicksMsec() - startTick;
					var progress = Mathf.Clamp((float)elapsedMs / LauncherDurationMs, 0f, 1f);
					SetProgress(progress * 100f);
					if (progress >= 1f)
					{
						break;
					}

					await sceneTree.ToSignal(sceneTree, SceneTree.SignalName.ProcessFrame);
				}

				uiComponent.CloseUIForm(this, true);

				_loginForm = await uiComponent.OpenRequiredAsync<UILogin>();
				if (_loginForm == null)
				{
					GD.PushError("[UILauncher] Open UILogin failed.");
					return;
				}

				_loginClicked = false;
				_loginForm.LoginClicked += OnLoginClicked;
				while (!_loginClicked)
				{
					await sceneTree.ToSignal(sceneTree, SceneTree.SignalName.ProcessFrame);
				}

				_loginForm.LoginClicked -= OnLoginClicked;
				uiComponent.CloseUIForm(_loginForm, true);
				_loginForm = null;

				var mainForm = await uiComponent.OpenRequiredAsync<UIMain>();
				if (mainForm == null)
				{
					GD.PushError("[UILauncher] Open UIMain failed.");
					return;
				}

				mainForm.SetPlayerInfo("GameFrameX", "Lv.1");
			}
			catch (Exception exception)
			{
				GD.PushError($"[UILauncher] RunFlowAsync exception: {exception}");
			}
		}

		private void OnLoginClicked()
		{
			_loginClicked = true;
		}
	}
}
