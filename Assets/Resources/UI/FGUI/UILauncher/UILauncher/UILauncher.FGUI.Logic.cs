using System;
using System.Threading.Tasks;
using FairyGUI;
using GameFrameX.Runtime;
using GameFrameX.UI.Runtime;
using Godot;
using Godot.Startup.Hotfix;
using Godot.Startup.Procedure;

namespace Godot.Hotfix.FairyGUI
{
	public partial class UILauncher
	{
		private const string LoginScenePath = "res://Assets/Bundles/UI/FGUI/UILogin/UILogin.tscn";
		private const string MainScenePath = "res://Assets/Bundles/UI/FGUI/UIMain/UIMain.tscn";
		private const string PlayerListScenePath = "res://Assets/Bundles/UI/FGUI/UILogin/UIPlayerListForm.tscn";
		private const string PlayerCreateScenePath = "res://Assets/Bundles/UI/FGUI/UILogin/UIPlayerCreateForm.tscn";
		private const string LoginTypeFullName = "Godot.Hotfix.FairyGUI.UILogin";
		private const string MainTypeFullName = "Godot.Hotfix.FairyGUI.UIMain";
		private const string PlayerListTypeFullName = "Godot.Hotfix.FairyGUI.UIPlayerListForm";
		private const string PlayerCreateTypeFullName = "Godot.Hotfix.FairyGUI.UIPlayerCreateForm";
#if NOT_EDITOR
		private const string LoginPckPackageName = "fgui_uilogin";
#endif

		private GComponent _view;
		private GProgressBar _progressBar;
		private GTextField _versionText;
		private bool _flowStarted;
		private bool _loginClicked;
		private bool _handoffToLogin;
		private int _flowGeneration;
		private IUIForm _loginForm;
		private int _waitLoginClickFrameCount;
		private const int OpenLoginTimeoutFrames = 1200;

		public override void OnOpen(object userData)
		{
			base.OnOpen(userData);

			FairyGuiRuntimeBridge.DisposeView(ref _view);
			_progressBar = null;
			_versionText = null;

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
			_versionText = _view.GetChild("txtVersion")?.asTextField;
			if (_versionText != null)
			{
				_versionText.text = global::GameFrameX.Runtime.Version.GameVersion;
			}
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
			_progressBar = null;
			_versionText = null;
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

		private async Task RunFlowAsync(int flowGeneration)
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

				// 避免在 UILauncher.OnOpen 调用栈内重入打开下一个 UI，先让出一帧。
				await sceneTree.ToSignal(sceneTree, SceneTree.SignalName.ProcessFrame);

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
					GD.PushError($"[UILauncher-FGUI] NOT_EDITOR login prepare failed. pck package mount failed. package={LoginPckPackageName} pck={loginPckPath}");
					return;
				}

				if (!FairyGuiRuntimeBridge.TryEnsurePackageReady("UILogin", out var packageError))
				{
					GD.PushError($"[UILauncher-FGUI] NOT_EDITOR UILogin AddPackage failed. {packageError}");
					return;
				}

				GD.Print($"[UILauncher-FGUI] NOT_EDITOR login prepared. package={LoginPckPackageName} resource=UILogin");
				GD.Print($"[UILauncher-FGUI] opening login scene by type. path={LoginScenePath} type={LoginTypeFullName}");
				var loginForm = await OpenUiByKnownTypeAsync(uiComponent, LoginScenePath, LoginTypeFullName);
#else
				GD.Print($"[UILauncher-FGUI] opening login scene by type. path={LoginScenePath} type={LoginTypeFullName}");
				var loginForm = await OpenUiByKnownTypeAsync(uiComponent, LoginScenePath, LoginTypeFullName);
#endif
				_loginForm = loginForm;
				if (_loginForm == null)
				{
					GD.PushError("[UILauncher-FGUI] Open UILogin failed.");
					return;
				}

				GD.Print($"[UILauncher-FGUI] Open UILogin success. type={_loginForm.GetType().FullName}");

				if (!HotfixTypeResolver.TrySubscribeEvent(_loginForm, "LoginClicked", (Action)OnLoginClicked))
				{
					GD.PushError("[UILauncher-FGUI] subscribe LoginClicked failed.");
					return;
				}

				_loginClicked = false;
				_handoffToLogin = true;
				_waitLoginClickFrameCount = 0;
				uiComponent.CloseUIForm(this, true);
				// 兜底：某些复用/热重载场景下实例引用关闭可能未命中，再按类型补一次。
				if (uiComponent.HasUIForm(nameof(UILauncher)))
				{
					uiComponent.CloseUIForm<UILauncher>(isNowRecycle: true);
				}

				if (_loginForm is UIForm loginUiForm)
				{
					uiComponent.RefocusUIForm(loginUiForm);
				}

				GD.Print("[UILauncher-FGUI] launcher closed, waiting login click.");
				while (!_loginClicked)
				{
					if (flowGeneration != _flowGeneration)
					{
						return;
					}

					_waitLoginClickFrameCount++;
					if (_waitLoginClickFrameCount == 300 || _waitLoginClickFrameCount % 900 == 0)
					{
						GD.PushWarning($"[UILauncher-FGUI] still waiting login click. frames={_waitLoginClickFrameCount}");
					}

					await sceneTree.ToSignal(sceneTree, SceneTree.SignalName.ProcessFrame);
				}

			GD.Print("[UILauncher-FGUI] login clicked, continue to player selection.");
			HotfixTypeResolver.TryUnsubscribeEvent(_loginForm, "LoginClicked", (Action)OnLoginClicked);
			uiComponent.CloseUIForm(_loginForm, true);
			_loginForm = null;

			var role = await RunRoleSelectionAsync(uiComponent, flowGeneration);
			if (flowGeneration != _flowGeneration)
			{
				return;
			}

			if (role == null)
			{
				GD.PushError("[UILauncher-FGUI] role selection finished without role.");
				return;
			}

			var mainForm = await OpenUiByKnownTypeAsync(uiComponent, MainScenePath, MainTypeFullName);
			if (mainForm == null)
			{
				GD.PushError($"[UILauncher-FGUI] Open UIMain failed. path={MainScenePath}");
				return;
			}

			GD.Print($"[UILauncher-FGUI] 进入游戏。role={role.Name} level={role.Level}");
			if (!HotfixTypeResolver.TryInvokeMethod(mainForm, "SetPlayerInfo", role.Name, $"当前等级:{role.Level}", "1"))
			{
				GD.PushWarning("[UILauncher-FGUI] SetPlayerInfo invoke failed.");
			}
		}
			catch (Exception exception)
			{
				GD.PushError($"[UILauncher-FGUI] RunFlowAsync exception: {exception}");
			}
		}

		/// <summary>
		/// 登录成功后的角色选择/创建环节：已有角色进入列表选择，无角色进入创建。
		/// 返回最终确定进入游戏的角色；null 表示失败。
		/// </summary>
		private async Task<Godot.Startup.Account.LocalAccountStore.RoleRecord> RunRoleSelectionAsync(UIComponent uiComponent, int flowGeneration)
		{
			var roles = Godot.Startup.Account.LocalAccountStore.GetCurrentRoles();
			var useCreate = roles.Count == 0;
			var scenePath = useCreate ? PlayerCreateScenePath : PlayerListScenePath;
			var typeFullName = useCreate ? PlayerCreateTypeFullName : PlayerListTypeFullName;
			GD.Print($"[UILauncher-FGUI] open player selection. mode={(useCreate ? "create" : "list")} path={scenePath}");

			var roleForm = await OpenUiByKnownTypeAsync(uiComponent, scenePath, typeFullName);
			if (roleForm == null)
			{
				GD.PushError($"[UILauncher-FGUI] Open role form failed. path={scenePath}");
				return null;
			}

			var roleChosen = false;
			var waitFrames = 0;
			if (roleForm is UIPlayerListForm listForm)
			{
				void OnRoleSelected()
				{
					roleChosen = true;
				}

				listForm.RoleSelected += OnRoleSelected;
				while (!roleChosen)
				{
					if (flowGeneration != _flowGeneration)
					{
						listForm.RoleSelected -= OnRoleSelected;
						return null;
					}

					waitFrames++;
					if (waitFrames == 300 || waitFrames % 900 == 0)
					{
						GD.PushWarning($"[UILauncher-FGUI] still waiting role selection. frames={waitFrames}");
					}

					await (Engine.GetMainLoop() as SceneTree).ToSignal(Engine.GetMainLoop() as SceneTree, SceneTree.SignalName.ProcessFrame);
				}

				listForm.RoleSelected -= OnRoleSelected;
			}
			else if (roleForm is UIPlayerCreateForm createForm)
			{
				void OnRoleCreated()
				{
					roleChosen = true;
				}

				createForm.RoleCreated += OnRoleCreated;
				while (!roleChosen)
				{
					if (flowGeneration != _flowGeneration)
					{
						createForm.RoleCreated -= OnRoleCreated;
						return null;
					}

					waitFrames++;
					if (waitFrames == 300 || waitFrames % 900 == 0)
					{
						GD.PushWarning($"[UILauncher-FGUI] still waiting role creation. frames={waitFrames}");
					}

					await (Engine.GetMainLoop() as SceneTree).ToSignal(Engine.GetMainLoop() as SceneTree, SceneTree.SignalName.ProcessFrame);
				}

				createForm.RoleCreated -= OnRoleCreated;
			}
			else
			{
				GD.PushError($"[UILauncher-FGUI] unexpected role form type. type={roleForm.GetType().FullName}");
				return null;
			}

			uiComponent.CloseUIForm(roleForm, true);
			return Godot.Startup.Account.LocalAccountStore.SelectedRole;
		}

		private static async Task<IUIForm> OpenUiByKnownTypeAsync(UIComponent uiComponent, string scenePath, string uiTypeFullName)
		{
			var uiType = HotfixTypeResolver.ResolveOrNull(uiTypeFullName);
			if (uiType == null)
			{
				GD.PushError($"[UILauncher-FGUI] resolve ui type failed. type={uiTypeFullName}");
				return null;
			}

			if (!typeof(IUIForm).IsAssignableFrom(uiType))
			{
				GD.PushError($"[UILauncher-FGUI] resolved type is not IUIForm. type={uiType.FullName}");
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

		private void OnLoginClicked()
		{
			_loginClicked = true;
		}

	}
}
