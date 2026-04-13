using System;
using System.Threading.Tasks;
using GameFrameX.Runtime;
using GameFrameX.UI.GDGUI.Runtime;
using GameFrameX.UI.Runtime;
using Godot;
using Godot.Startup.Hotfix;
using Godot.Startup.Procedure;

namespace Godot.Hotfix.GodotGUI
{
	[OptionUIGroup(UIGroupNameConstants.Normal)]
	public partial class UILauncher : GDGUI
	{
		private const string LoginScenePath = "res://Assets/Bundles/UI/GGUI/UILogin/UILogin.tscn";
		private const string MainScenePath = "res://Assets/Bundles/UI/GGUI/UIMain/UIMain.tscn";
		private const string LoginTypeFullName = "Godot.Hotfix.GodotGUI.UILogin";
		private const string MainTypeFullName = "Godot.Hotfix.GodotGUI.UIMain";
#if NOT_EDITOR
		private const string LoginPckPackageName = "ggui_uilogin";
#endif

		private ProgressBar _progressBar;
		private Label _versionLabel;
		private bool _flowStarted;
		private bool _loginClicked;
		private bool _handoffToLogin;
		private int _flowGeneration;
		private IUIForm _loginForm;

		public override void OnOpen(object userData)
		{
			base.OnOpen(userData);
			EnsureNodes();
			SetVersionText();
			SetProgress(LauncherFlowProgressReporter.GetSnapshot().Progress);
			if (_flowStarted)
			{
				return;
			}

			_flowStarted = true;
			var flowGeneration = ++_flowGeneration;
			_ = RunFlowAsync(flowGeneration);
		}

		public override void OnClose(bool isShutdown, object userData)
		{
			var shouldForceRelease = isShutdown;
			if ((shouldForceRelease || !_handoffToLogin) && _loginForm != null)
			{
				HotfixTypeResolver.TryUnsubscribeEvent(_loginForm, "LoginClicked", (Action)OnLoginClicked);
				_loginForm = null;
			}

			if (shouldForceRelease || !_handoffToLogin)
			{
				_loginClicked = false;
				_flowGeneration++;
			}

			_handoffToLogin = false;
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
			_versionLabel = FindChild("VersionLabel", true, false) as Label;
		}

		private void SetVersionText()
		{
			if (_versionLabel == null)
			{
				return;
			}

			_versionLabel.Text = $"Version: {GameFrameX.Runtime.Version.GameVersion}";
		}

		private async Task RunFlowAsync(int flowGeneration)
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

				while (true)
				{
					if (flowGeneration != _flowGeneration)
					{
						return;
					}

					var snapshot = LauncherFlowProgressReporter.GetSnapshot();
					SetProgress(snapshot.Progress);
					if (snapshot.IsCompleted)
					{
						break;
					}

					await sceneTree.ToSignal(sceneTree, SceneTree.SignalName.ProcessFrame);
				}

				if (flowGeneration != _flowGeneration)
				{
					return;
				}

#if NOT_EDITOR
				var loginPckPath = global::GameFrameX.AssetSystem.GodotAssetPath.GetHotfixPckFileVirtual(LoginPckPackageName);
				if (!global::GameFrameX.AssetSystem.AssetSystem.MountGodotResourcePackByPath(loginPckPath, replaceFiles: false))
				{
					GD.PushError($"[UILauncher] NOT_EDITOR login prepare failed. pck package mount failed. package={LoginPckPackageName} pck={loginPckPath}");
					return;
				}

				GD.Print($"[UILauncher] NOT_EDITOR login prepared. package={LoginPckPackageName} resource=UILogin");
				var loginForm = await OpenUiByKnownTypeAsync(uiComponent, LoginScenePath, LoginTypeFullName);
#else
				var loginForm = await OpenUiByKnownTypeAsync(uiComponent, LoginScenePath, LoginTypeFullName);
#endif
				_loginForm = loginForm;
				if (_loginForm == null)
				{
					GD.PushError("[UILauncher] Open UILogin failed.");
					return;
				}

				if (!HotfixTypeResolver.TrySubscribeEvent(_loginForm, "LoginClicked", (Action)OnLoginClicked))
				{
					GD.PushError("[UILauncher] subscribe LoginClicked failed.");
					return;
				}

				_loginClicked = false;
				_handoffToLogin = true;
				uiComponent.CloseUIForm(this, true);
				// 兜底：某些复用/热重载场景下实例引用关闭可能未命中，再按类型补一次。
				if (uiComponent.HasUIForm(nameof(UILauncher)))
				{
					uiComponent.CloseUIForm<UILauncher>(isNowRecycle: true);
				}
				while (!_loginClicked)
				{
					if (flowGeneration != _flowGeneration)
					{
						return;
					}

					await sceneTree.ToSignal(sceneTree, SceneTree.SignalName.ProcessFrame);
				}

				HotfixTypeResolver.TryUnsubscribeEvent(_loginForm, "LoginClicked", (Action)OnLoginClicked);
				uiComponent.CloseUIForm(_loginForm, true);
				_loginForm = null;

				var mainForm = await OpenUiByKnownTypeAsync(uiComponent, MainScenePath, MainTypeFullName);
				if (mainForm == null)
				{
					GD.PushError($"[UILauncher] Open UIMain failed. path={MainScenePath}");
					return;
				}

				if (!HotfixTypeResolver.TryInvokeMethod(mainForm, "SetPlayerInfo", "GameFrameX", "Lv.1"))
				{
					GD.PushWarning("[UILauncher] SetPlayerInfo invoke failed.");
				}
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

		private static async Task<IUIForm> OpenUiByKnownTypeAsync(UIComponent uiComponent, string scenePath, string uiTypeFullName)
		{
			var uiType = HotfixTypeResolver.ResolveOrNull(uiTypeFullName);
			if (uiType == null)
			{
				GD.PushError($"[UILauncher] resolve ui type failed. type={uiTypeFullName}");
				return null;
			}

			if (!typeof(IUIForm).IsAssignableFrom(uiType))
			{
				GD.PushError($"[UILauncher] resolved type is not IUIForm. type={uiType.FullName}");
				return null;
			}

			var directory = ResolveSceneDirectory(scenePath);
			return await uiComponent.OpenUIAsync(directory, uiType, true, null, true);
		}

		private static string ResolveSceneDirectory(string scenePath)
		{
			if (string.IsNullOrWhiteSpace(scenePath))
			{
				return scenePath;
			}

			var normalized = scenePath.Replace('\\', '/').Trim();
			var slash = normalized.LastIndexOf('/');
			if (slash <= 0)
			{
				return normalized;
			}

			return normalized.Substring(0, slash);
		}

	}
}
