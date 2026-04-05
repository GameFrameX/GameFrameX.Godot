using System;
using Godot;
using FairyGUI;
using FileAccess = Godot.FileAccess;

namespace Godot.Startup.Demo
{
	public partial class FairyGuiFlowDemo : Node
	{
		[Export] public bool AutoRunOnReady { get; set; } = true;
		[Export] public float LauncherDurationSeconds { get; set; } = 3f;
		[Export] public string BundleRootPath { get; set; } = "res://Assets/Bundles/UI";

		private static readonly string[] RequiredPackages =
		{
			"UICommon",
			"UICommonAvatar",
			"UILauncher",
			"UILogin",
			"UIMain"
		};

		private GComponent _activeView;
		private GProgressBar _launcherProgressBar;
		private GObject _loginTrigger;
		private double _launcherElapsedSeconds;
		private bool _launcherRunning;

		public override void _Ready()
		{
			if (!AutoRunOnReady)
			{
				return;
			}

			StartDemoFlow();
		}

		public override void _Process(double delta)
		{
			if (!_launcherRunning)
			{
				return;
			}

			_launcherElapsedSeconds += delta;
			var duration = Mathf.Max(0.01f, LauncherDurationSeconds);
			var progress = Mathf.Clamp((float)(_launcherElapsedSeconds / duration), 0f, 1f);
			if (_launcherProgressBar != null)
			{
				_launcherProgressBar.max = 100;
				_launcherProgressBar.value = progress * 100f;
			}

			if (progress >= 1f)
			{
				_launcherRunning = false;
				ShowLoginView();
			}
		}

		public override void _ExitTree()
		{
			UnbindLoginTrigger();
			DisposeActiveView();
			base._ExitTree();
		}

		private void StartDemoFlow()
		{
			_ = Stage.inst;
			LoadRequiredPackages();
			ShowLauncherView();
		}

		private void LoadRequiredPackages()
		{
			for (var i = 0; i < RequiredPackages.Length; i++)
			{
				var packageName = RequiredPackages[i];
				if (UIPackage.GetByName(packageName) != null)
				{
					continue;
				}

				var packagePath = $"{BundleRootPath}/{packageName}/{packageName}_fui.bytes";
				if (!FileAccess.FileExists(packagePath))
				{
					GD.PushWarning($"[FairyGuiFlowDemo] package file missing: {packagePath}");
					continue;
				}

				var package = UIPackage.AddPackage(packagePath, LoadResourceWithFallback);
				if (package == null)
				{
					GD.PushWarning($"[FairyGuiFlowDemo] add package failed: {packagePath}");
				}
			}
		}

		private static object LoadResourceWithFallback(string path, Type type, out DestroyMethod destroyMethod)
		{
			destroyMethod = DestroyMethod.Unload;
			if (type == typeof(byte[]))
			{
				if (!FileAccess.FileExists(path))
				{
					return null;
				}

				using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
				return file.GetBuffer((long)file.GetLength());
			}

			var resource = TryLoad(path);
			if (resource == null && !string.IsNullOrEmpty(path))
			{
				resource = TryLoad(path + ".png")
					?? TryLoad(path + ".jpg")
					?? TryLoad(path + ".jpeg")
					?? TryLoad(path + ".webp")
					?? TryLoad(path + ".bmp")
					?? TryLoad(path + ".tga")
					?? TryLoad(path + ".wav")
					?? TryLoad(path + ".ogg")
					?? TryLoad(path + ".mp3");
			}

			if (resource == null)
			{
				return null;
			}

			if (type != null && !type.IsInstanceOfType(resource))
			{
				return null;
			}

			return resource;
		}

		private static Resource TryLoad(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				return null;
			}

			return ResourceLoader.Load(path);
		}

		private void ShowLauncherView()
		{
			var view = CreateFullScreenView("UILauncher", "UILauncher");
			if (view == null)
			{
				return;
			}

			var isDownload = view.GetController("IsDownload");
			if (isDownload != null)
			{
				isDownload.selectedPage = "Yes";
			}

			var progressObject = view.GetChild("ProgressBar");
			_launcherProgressBar = progressObject?.asProgress;
			if (_launcherProgressBar != null)
			{
				_launcherProgressBar.max = 100;
				_launcherProgressBar.value = 0;
			}

			_launcherElapsedSeconds = 0;
			_launcherRunning = true;
			GD.Print("[FairyGuiFlowDemo] UILauncher shown.");
		}

		private void ShowLoginView()
		{
			var view = CreateFullScreenView("UILogin", "UILogin");
			if (view == null)
			{
				return;
			}

			_launcherProgressBar = null;
			_launcherElapsedSeconds = 0;
			var enterButton = view.GetChild("enter");
			if (enterButton == null)
			{
				GD.PushWarning("[FairyGuiFlowDemo] UILogin.enter not found.");
				return;
			}

			_loginTrigger = enterButton;
			_loginTrigger.onClick.Add(OnLoginClicked);
			GD.Print("[FairyGuiFlowDemo] UILogin shown. Waiting for login click.");
		}

		private void OnLoginClicked()
		{
			ShowMainView();
		}

		private void ShowMainView()
		{
			var view = CreateFullScreenView("UIMain", "UIMain");
			if (view == null)
			{
				return;
			}

			var playerName = view.GetChild("player_name")?.asTextField;
			if (playerName != null)
			{
				playerName.text = "GameFrameX";
			}

			var playerLevel = view.GetChild("player_level")?.asTextField;
			if (playerLevel != null)
			{
				playerLevel.text = "Lv.1";
			}

			GD.Print("[FairyGuiFlowDemo] UIMain shown.");
		}

		private GComponent CreateFullScreenView(string packageName, string componentName)
		{
			UnbindLoginTrigger();
			DisposeActiveView();

			var gObject = UIPackage.CreateObject(packageName, componentName);
			if (gObject == null)
			{
				GD.PushError($"[FairyGuiFlowDemo] CreateObject failed: {packageName}/{componentName}");
				return null;
			}

			var component = gObject.asCom;
			if (component == null)
			{
				gObject.Dispose();
				GD.PushError($"[FairyGuiFlowDemo] Object is not GComponent: {packageName}/{componentName}");
				return null;
			}

			component.MakeFullScreen(true);
			GRoot.inst.AddChild(component);
			_activeView = component;
			return component;
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

		private void DisposeActiveView()
		{
			if (_activeView == null)
			{
				return;
			}

			_activeView.Dispose();
			_activeView = null;
		}
	}
}
