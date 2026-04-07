using System;
using System.Threading.Tasks;
using FairyGUI;
using GameFrameX.Runtime;
using GameFrameX.UI.Runtime;
using Godot;

namespace Godot.Hotfix.FairyGUI
{
	public partial class UILauncher
	{
		private const int LauncherDurationMs = 3000;

		private GComponent _view;
		private GProgressBar _progressBar;
		private bool _flowStarted;
		private bool _loginClicked;
		private UILogin _loginForm;

		public override void OnOpen(object userData)
		{
			base.OnOpen(userData);

			FairyGuiRuntimeBridge.DisposeView(ref _view);
			_progressBar = null;

			_view = FairyGuiRuntimeBridge.CreateFullScreenView("UILauncher", "UILauncher", UIGroup?.Name);
			if (_view == null)
			{
				return;
			}

			var isDownload = _view.GetController("IsDownload");
			if (isDownload != null)
			{
				isDownload.selectedPage = "Yes";
			}

			_progressBar = _view.GetChild("ProgressBar")?.asProgress;
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
			_progressBar = null;
			FairyGuiRuntimeBridge.DisposeView(ref _view);
			base.OnClose(isShutdown, userData);
		}

		public void SetProgress(float value)
		{
			if (_progressBar == null)
			{
				return;
			}

			_progressBar.max = 100;
			_progressBar.value = Mathf.Clamp(value, 0f, 100f);
		}

		private async Task RunFlowAsync()
		{
			try
			{
				var sceneTree = Engine.GetMainLoop() as SceneTree;
				if (sceneTree == null)
				{
					GD.PushWarning("[UILauncher-FGUI] SceneTree not found.");
					return;
				}

				var uiComponent = GameEntry.GetComponent<UIComponent>();
				if (uiComponent == null)
				{
					GD.PushError("[UILauncher-FGUI] UIComponent not found.");
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
					GD.PushError("[UILauncher-FGUI] Open UILogin failed.");
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
					GD.PushError("[UILauncher-FGUI] Open UIMain failed.");
					return;
				}

				mainForm.SetPlayerInfo("GameFrameX", "Lv.1");
			}
			catch (Exception exception)
			{
				GD.PushError($"[UILauncher-FGUI] RunFlowAsync exception: {exception}");
			}
		}

		private void OnLoginClicked()
		{
			_loginClicked = true;
		}
	}
}
