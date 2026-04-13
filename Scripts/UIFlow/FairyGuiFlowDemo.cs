using System;
using System.Threading.Tasks;
using GameFrameX.Runtime;
using GameFrameX.UI.Runtime;
using Godot;
using Godot.Startup.Hotfix;

namespace Godot.Startup.UIFlow
{
    public partial class FairyGuiFlowDemo : UiFlowDemoBase
    {
        private const string LauncherTypeFullName = "Godot.Hotfix.FairyGUI.UILauncher";
        private const string LoginTypeFullName = "Godot.Hotfix.FairyGUI.UILogin";
        private const string MainTypeFullName = "Godot.Hotfix.FairyGUI.UIMain";
        private const string LauncherAssetRootPath = "res://Assets/Resources/UI/FGUI/UILauncher";
        private const string LoginAssetRootPath = "res://Assets/Bundles/UI/FGUI/UILogin";
        private const string MainAssetRootPath = "res://Assets/Bundles/UI/FGUI/UIMain";

        protected override string FlowLogTag => "FairyGuiFlowDemo";
        protected override string UiAssetRootPath => "res://Assets/Bundles/UI/FGUI";

        protected override Task<IUIForm> OpenLauncherFormAsync(UIComponent uiComponent, string rootPath)
        {
            return OpenFormAsync(uiComponent, LauncherAssetRootPath, LauncherTypeFullName);
        }

        protected override void SetLauncherProgress(IUIForm launcherForm, float progressPercent)
        {
            HotfixTypeResolver.TryInvokeMethod(launcherForm, "SetProgress", progressPercent);
        }

        protected override Task<IUIForm> OpenLoginFormAsync(UIComponent uiComponent, string rootPath)
        {
            return OpenFormAsync(uiComponent, LoginAssetRootPath, LoginTypeFullName);
        }

        protected override void BindLoginClicked(IUIForm loginForm, Action onClicked)
        {
            HotfixTypeResolver.TrySubscribeEvent(loginForm, "LoginClicked", onClicked);
        }

        protected override void UnbindLoginClicked(IUIForm loginForm, Action onClicked)
        {
            HotfixTypeResolver.TryUnsubscribeEvent(loginForm, "LoginClicked", onClicked);
        }

        protected override Task<IUIForm> OpenMainFormAsync(UIComponent uiComponent, string rootPath)
        {
            return OpenFormAsync(uiComponent, MainAssetRootPath, MainTypeFullName);
        }

        protected override void SetMainPlayerInfo(IUIForm mainForm, string playerName, string playerLevel)
        {
            HotfixTypeResolver.TryInvokeMethod(mainForm, "SetPlayerInfo", playerName, playerLevel);
        }

        private static async Task<IUIForm> OpenFormAsync(UIComponent uiComponent, string rootPath, string typeFullName)
        {
            var formType = HotfixTypeResolver.ResolveOrNull(typeFullName);
            if (formType == null)
            {
                GD.PushError($"[FairyGuiFlowDemo] type not found: {typeFullName}");
                return null;
            }

            return await uiComponent.OpenUIAsync(rootPath, formType, true, null, true);
        }
    }
}

