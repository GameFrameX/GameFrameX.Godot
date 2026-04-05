using System.Threading.Tasks;
using GameFrameX.Runtime;
using GameFrameX.UI.Runtime;
using Godot;
using Godot.Startup.Demo.GodotUI;

namespace Godot.Startup.Demo
{
	public partial class GodotGuiFlowDemo : Node
	{
		[Export] public bool AutoRunOnReady { get; set; } = true;
		[Export] public float LauncherDurationSeconds { get; set; } = 3f;

		private const string UiAssetRootPath = "res://Scenes/Demo/GodotUI";
		private const int MaxUiComponentRetryFrames = 300;

		private UIComponent _uiComponent;
		private UILauncher _launcherForm;
		private UILogin _loginForm;
		private UIMain _mainForm;
		private double _launcherElapsedSeconds;
		private bool _launcherRunning;
		private bool _switchingToLogin;
		private bool _switchingToMain;
		private bool _isInitializing;

		public override void _Ready()
		{
			GD.Print($"[GodotGuiFlowDemo] _Ready AutoRunOnReady={AutoRunOnReady} Node={Name}");
			if (!AutoRunOnReady)
			{
				return;
			}

			ForceRestartFlow("ready");
		}

		public void ForceRestartFlow(string reason)
		{
			GD.Print($"[GodotGuiFlowDemo] ForceRestartFlow reason={reason}");
			_ = StartDemoFlowAsync(reason);
		}

		public override void _Process(double delta)
		{
			if (!_launcherRunning || _launcherForm == null)
			{
				return;
			}

			_launcherElapsedSeconds += delta;
			var duration = Mathf.Max(0.01f, LauncherDurationSeconds);
			var progress = Mathf.Clamp((float)(_launcherElapsedSeconds / duration), 0f, 1f);
			_launcherForm.SetProgress(progress * 100f);
			if (progress >= 1f && !_switchingToLogin)
			{
				_switchingToLogin = true;
				_launcherRunning = false;
				GD.Print("[GodotGuiFlowDemo] launcher complete -> switch to login");
				_ = ShowLoginViewAsync();
			}
		}

		public override void _ExitTree()
		{
			GD.Print("[GodotGuiFlowDemo] _ExitTree");
			CloseAllDemoForms();
			base._ExitTree();
		}

		private async Task StartDemoFlowAsync(string reason)
		{
			if (_isInitializing)
			{
				GD.PushWarning($"[GodotGuiFlowDemo] initialization already running, ignore reason={reason}");
				return;
			}

			_isInitializing = true;
			GD.Print($"[GodotGuiFlowDemo] StartDemoFlowAsync begin reason={reason}");

			try
			{
				_launcherRunning = false;
				_switchingToLogin = false;
				_switchingToMain = false;
				_launcherElapsedSeconds = 0;
				CloseAllDemoForms();

				_uiComponent = null;
				for (var i = 0; i < MaxUiComponentRetryFrames; i++)
				{
					_uiComponent = GameEntry.GetComponent<UIComponent>();
					if (_uiComponent != null && _uiComponent.IsInitialized)
					{
						GD.Print($"[GodotGuiFlowDemo] UIComponent initialized at frameRetry={i} backend={_uiComponent.RuntimeBackendTypeName}");
						break;
					}

					if (i == 0 || i % 60 == 0)
					{
						var backend = _uiComponent?.RuntimeBackendTypeName ?? "<null>";
						GD.PushWarning($"[GodotGuiFlowDemo] waiting UIComponent initialization... retry={i}/{MaxUiComponentRetryFrames} backend={backend}");
					}

					await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
				}

				if (_uiComponent == null || !_uiComponent.IsInitialized)
				{
					GD.PushError($"[GodotGuiFlowDemo] UIComponent not initialized after retry={MaxUiComponentRetryFrames}, flow aborted.");
					return;
				}

				await ShowLauncherViewAsync();
				GD.Print("[GodotGuiFlowDemo] StartDemoFlowAsync end");
			}
			catch (System.Exception exception)
			{
				GD.PushError($"[GodotGuiFlowDemo] StartDemoFlowAsync exception: {exception}");
			}
			finally
			{
				_isInitializing = false;
			}
		}

		private async Task ShowLauncherViewAsync()
		{
			_launcherElapsedSeconds = 0;
			_switchingToLogin = false;
			_switchingToMain = false;
			GD.Print($"[GodotGuiFlowDemo] open launcher pathRoot={UiAssetRootPath}");

			try
			{
				_launcherForm = await _uiComponent.OpenFullScreenAsync<UILauncher>(UiAssetRootPath);
			}
			catch (System.Exception exception)
			{
				GD.PushError($"[GodotGuiFlowDemo] Open UILauncher exception: {exception}");
				return;
			}

			if (_launcherForm == null)
			{
				GD.PushError($"[GodotGuiFlowDemo] Open UILauncher failed. path={UiAssetRootPath}/UILauncher(.tscn)");
				return;
			}

			_launcherForm.SetProgress(0f);
			_launcherRunning = true;
			GD.Print("[GodotGuiFlowDemo] UILauncher shown via UIManager.");
		}

		private async Task ShowLoginViewAsync()
		{
			if (_launcherForm != null)
			{
				_uiComponent.CloseUIForm(_launcherForm, true);
				_launcherForm = null;
				GD.Print("[GodotGuiFlowDemo] launcher closed");
			}

			GD.Print($"[GodotGuiFlowDemo] open login pathRoot={UiAssetRootPath}");
			_loginForm = await _uiComponent.OpenFullScreenAsync<UILogin>(UiAssetRootPath);
			if (_loginForm == null)
			{
				GD.PushError($"[GodotGuiFlowDemo] Open UILogin failed. path={UiAssetRootPath}/UILogin(.tscn)");
				return;
			}

			_loginForm.LoginClicked += OnLoginClicked;
			GD.Print("[GodotGuiFlowDemo] UILogin shown via UIManager.");
		}

		private void OnLoginClicked()
		{
			if (_switchingToMain)
			{
				return;
			}

			_switchingToMain = true;
			GD.Print("[GodotGuiFlowDemo] login clicked -> switch to main");
			_ = ShowMainViewAsync();
		}

		private async Task ShowMainViewAsync()
		{
			if (_loginForm != null)
			{
				_loginForm.LoginClicked -= OnLoginClicked;
				_uiComponent.CloseUIForm(_loginForm, true);
				_loginForm = null;
				GD.Print("[GodotGuiFlowDemo] login closed");
			}

			GD.Print($"[GodotGuiFlowDemo] open main pathRoot={UiAssetRootPath}");
			_mainForm = await _uiComponent.OpenFullScreenAsync<UIMain>(UiAssetRootPath);
			if (_mainForm == null)
			{
				GD.PushError($"[GodotGuiFlowDemo] Open UIMain failed. path={UiAssetRootPath}/UIMain(.tscn)");
				return;
			}

			_mainForm.SetPlayerInfo("GameFrameX", "Lv.1");
			GD.Print("[GodotGuiFlowDemo] UIMain shown via UIManager.");
		}

		private void CloseAllDemoForms()
		{
			if (_loginForm != null)
			{
				_loginForm.LoginClicked -= OnLoginClicked;
			}

			if (_uiComponent != null)
			{
				if (_launcherForm != null)
				{
					_uiComponent.CloseUIForm(_launcherForm, true);
				}

				if (_loginForm != null)
				{
					_uiComponent.CloseUIForm(_loginForm, true);
				}

				if (_mainForm != null)
				{
					_uiComponent.CloseUIForm(_mainForm, true);
				}
			}

			_launcherForm = null;
			_loginForm = null;
			_mainForm = null;
		}
	}
}
