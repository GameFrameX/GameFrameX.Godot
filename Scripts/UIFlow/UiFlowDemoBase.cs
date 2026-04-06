using System;
using System.Threading.Tasks;
using GameFrameX.Runtime;
using GameFrameX.UI.Runtime;
using Godot;

namespace Godot.Startup.UIFlow
{
    public abstract partial class UiFlowDemoBase : Node
    {
        private const int MaxUiComponentRetryFrames = 300;

        [Export] public bool AutoRunOnReady { get; set; } = true;
        [Export] public float LauncherDurationSeconds { get; set; } = 3f;

        private UIComponent _uiComponent;
        private IUIForm _launcherForm;
        private IUIForm _loginForm;
        private IUIForm _mainForm;
        private double _launcherElapsedSeconds;
        private bool _launcherRunning;
        private bool _switchingToLogin;
        private bool _switchingToMain;
        private bool _isInitializing;

        protected abstract string FlowLogTag { get; }
        protected abstract string UiAssetRootPath { get; }

        protected abstract Task<IUIForm> OpenLauncherFormAsync(UIComponent uiComponent, string rootPath);
        protected abstract void SetLauncherProgress(IUIForm launcherForm, float progressPercent);
        protected abstract Task<IUIForm> OpenLoginFormAsync(UIComponent uiComponent, string rootPath);
        protected abstract void BindLoginClicked(IUIForm loginForm, Action onClicked);
        protected abstract void UnbindLoginClicked(IUIForm loginForm, Action onClicked);
        protected abstract Task<IUIForm> OpenMainFormAsync(UIComponent uiComponent, string rootPath);
        protected abstract void SetMainPlayerInfo(IUIForm mainForm, string playerName, string playerLevel);

        public override void _Ready()
        {
            GD.Print($"[{FlowLogTag}] _Ready AutoRunOnReady={AutoRunOnReady} Node={Name}");
            if (!AutoRunOnReady)
            {
                return;
            }

            ForceRestartFlow("ready");
        }

        public void ForceRestartFlow(string reason)
        {
            GD.Print($"[{FlowLogTag}] ForceRestartFlow reason={reason}");
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
            SetLauncherProgress(_launcherForm, progress * 100f);
            if (progress >= 1f && !_switchingToLogin)
            {
                _switchingToLogin = true;
                _launcherRunning = false;
                GD.Print($"[{FlowLogTag}] launcher complete -> switch to login");
                _ = ShowLoginViewAsync();
            }
        }

        public override void _ExitTree()
        {
            GD.Print($"[{FlowLogTag}] _ExitTree");
            CloseAllDemoForms();
            base._ExitTree();
        }

        private async Task StartDemoFlowAsync(string reason)
        {
            if (_isInitializing)
            {
                GD.PushWarning($"[{FlowLogTag}] initialization already running, ignore reason={reason}");
                return;
            }

            _isInitializing = true;
            GD.Print($"[{FlowLogTag}] StartDemoFlowAsync begin reason={reason}");

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
                        GD.Print($"[{FlowLogTag}] UIComponent initialized at frameRetry={i} backend={_uiComponent.RuntimeBackendTypeName}");
                        break;
                    }

                    if (i == 0 || i % 60 == 0)
                    {
                        var backend = _uiComponent?.RuntimeBackendTypeName ?? "<null>";
                        GD.PushWarning($"[{FlowLogTag}] waiting UIComponent initialization... retry={i}/{MaxUiComponentRetryFrames} backend={backend}");
                    }

                    await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                }

                if (_uiComponent == null || !_uiComponent.IsInitialized)
                {
                    GD.PushError($"[{FlowLogTag}] UIComponent not initialized after retry={MaxUiComponentRetryFrames}, flow aborted.");
                    return;
                }

                await ShowLauncherViewAsync();
                GD.Print($"[{FlowLogTag}] StartDemoFlowAsync end");
            }
            catch (Exception exception)
            {
                GD.PushError($"[{FlowLogTag}] StartDemoFlowAsync exception: {exception}");
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
            GD.Print($"[{FlowLogTag}] open launcher pathRoot={UiAssetRootPath}");

            try
            {
                _launcherForm = await OpenLauncherFormAsync(_uiComponent, UiAssetRootPath);
            }
            catch (Exception exception)
            {
                GD.PushError($"[{FlowLogTag}] Open UILauncher exception: {exception}");
                return;
            }

            if (_launcherForm == null)
            {
                GD.PushError($"[{FlowLogTag}] Open UILauncher failed. path={UiAssetRootPath}/UILauncher(.tscn)");
                return;
            }

            SetLauncherProgress(_launcherForm, 0f);
            _launcherRunning = true;
            GD.Print($"[{FlowLogTag}] UILauncher shown via UIManager.");
        }

        private async Task ShowLoginViewAsync()
        {
            if (_launcherForm != null)
            {
                _uiComponent.CloseUIForm(_launcherForm, true);
                _launcherForm = null;
                GD.Print($"[{FlowLogTag}] launcher closed");
            }

            GD.Print($"[{FlowLogTag}] open login pathRoot={UiAssetRootPath}");
            _loginForm = await OpenLoginFormAsync(_uiComponent, UiAssetRootPath);
            if (_loginForm == null)
            {
                GD.PushError($"[{FlowLogTag}] Open UILogin failed. path={UiAssetRootPath}/UILogin(.tscn)");
                return;
            }

            BindLoginClicked(_loginForm, OnLoginClicked);
            GD.Print($"[{FlowLogTag}] UILogin shown via UIManager.");
        }

        private void OnLoginClicked()
        {
            if (_switchingToMain)
            {
                return;
            }

            _switchingToMain = true;
            GD.Print($"[{FlowLogTag}] login clicked -> switch to main");
            _ = ShowMainViewAsync();
        }

        private async Task ShowMainViewAsync()
        {
            if (_loginForm != null)
            {
                UnbindLoginClicked(_loginForm, OnLoginClicked);
                _uiComponent.CloseUIForm(_loginForm, true);
                _loginForm = null;
                GD.Print($"[{FlowLogTag}] login closed");
            }

            GD.Print($"[{FlowLogTag}] open main pathRoot={UiAssetRootPath}");
            _mainForm = await OpenMainFormAsync(_uiComponent, UiAssetRootPath);
            if (_mainForm == null)
            {
                GD.PushError($"[{FlowLogTag}] Open UIMain failed. path={UiAssetRootPath}/UIMain(.tscn)");
                return;
            }

            SetMainPlayerInfo(_mainForm, "GameFrameX", "Lv.1");
            GD.Print($"[{FlowLogTag}] UIMain shown via UIManager.");
        }

        private void CloseAllDemoForms()
        {
            if (_loginForm != null)
            {
                UnbindLoginClicked(_loginForm, OnLoginClicked);
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

