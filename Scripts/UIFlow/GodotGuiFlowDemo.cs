using System;
using System.Threading.Tasks;
using GameFrameX.Runtime;
using GameFrameX.UI.Runtime;
using Godot;
using Godot.Startup.Hotfix;

namespace Godot.Startup.UIFlow
{
    public partial class GodotGuiFlowDemo : UiFlowDemoBase
    {
        private const string LauncherTypeFullName = "Godot.Hotfix.GodotGUI.UILauncher";
        private const string LoginTypeFullName = "Godot.Hotfix.GodotGUI.UILogin";
        private const string MainTypeFullName = "Godot.Hotfix.GodotGUI.UIMain";

        protected override string FlowLogTag => "GodotGuiFlowDemo";
        protected override string UiAssetRootPath => "res://Assets/Bundles/Prefabs/UI/GodotUI";

        protected override Task<IUIForm> OpenLauncherFormAsync(UIComponent uiComponent, string rootPath)
        {
            return OpenFormAsync(uiComponent, rootPath, LauncherTypeFullName);
        }

        protected override void SetLauncherProgress(IUIForm launcherForm, float progressPercent)
        {
            HotfixTypeResolver.TryInvokeMethod(launcherForm, "SetProgress", progressPercent);
        }

        protected override Task<IUIForm> OpenLoginFormAsync(UIComponent uiComponent, string rootPath)
        {
            return OpenFormAsync(uiComponent, rootPath, LoginTypeFullName);
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
            return OpenFormAsync(uiComponent, rootPath, MainTypeFullName);
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
                GD.PushError($"[GodotGuiFlowDemo] type not found: {typeFullName}");
                return null;
            }

            return await uiComponent.OpenUIAsync(rootPath, formType, true, null, true);
        }
    }
}

