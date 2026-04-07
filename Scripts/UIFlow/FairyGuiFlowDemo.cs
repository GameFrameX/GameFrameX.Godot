using System;
using System.Threading.Tasks;
using GameFrameX.Runtime;
using GameFrameX.UI.Runtime;
using Godot;
using UILauncher = Godot.Hotfix.FairyGUI.UILauncher;
using UILogin = Godot.Hotfix.FairyGUI.UILogin;
using UIMain = Godot.Hotfix.FairyGUI.UIMain;

namespace Godot.Startup.UIFlow
{
    public partial class FairyGuiFlowDemo : UiFlowDemoBase
    {
        protected override string FlowLogTag => "FairyGuiFlowDemo";
    protected override string UiAssetRootPath => "res://Assets/Bundles/Prefabs/UI/FGUI";

        protected override Task<IUIForm> OpenLauncherFormAsync(UIComponent uiComponent, string rootPath)
        {
            return OpenFormAsync<UILauncher>(uiComponent, rootPath);
        }

        protected override void SetLauncherProgress(IUIForm launcherForm, float progressPercent)
        {
            if (launcherForm is UILauncher launcher)
            {
                launcher.SetProgress(progressPercent);
            }
        }

        protected override Task<IUIForm> OpenLoginFormAsync(UIComponent uiComponent, string rootPath)
        {
            return OpenFormAsync<UILogin>(uiComponent, rootPath);
        }

        protected override void BindLoginClicked(IUIForm loginForm, Action onClicked)
        {
            if (loginForm is UILogin login)
            {
                login.LoginClicked += onClicked;
            }
        }

        protected override void UnbindLoginClicked(IUIForm loginForm, Action onClicked)
        {
            if (loginForm is UILogin login)
            {
                login.LoginClicked -= onClicked;
            }
        }

        protected override Task<IUIForm> OpenMainFormAsync(UIComponent uiComponent, string rootPath)
        {
            return OpenFormAsync<UIMain>(uiComponent, rootPath);
        }

        protected override void SetMainPlayerInfo(IUIForm mainForm, string playerName, string playerLevel)
        {
            if (mainForm is UIMain main)
            {
                main.SetPlayerInfo(playerName, playerLevel);
            }
        }

        private static async Task<IUIForm> OpenFormAsync<TForm>(UIComponent uiComponent, string rootPath)
            where TForm : class, IUIForm
        {
            return await uiComponent.OpenFullScreenAsync<TForm>(rootPath);
        }
    }
}

