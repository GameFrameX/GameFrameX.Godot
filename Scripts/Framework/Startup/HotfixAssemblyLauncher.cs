using System;
using System.Threading.Tasks;
using GameFrameX.Runtime;
using GameFrameX.Startup.Runtime;
using Godot.Startup.Hotfix;
using Godot;

namespace GameFrameX.Startup.Application
{
    /// <summary>
    /// 热更程序集启动器：按 <see cref="StartupOptions"/> 约定经 <c>HotfixTypeResolver</c> 解析并调用热更入口。
    /// </summary>
    /// <remarks>
    /// 热更程序集或入口缺失时降级：直接打开 UILogin 演示流并视为启动成功——与旧启动链行为对齐，
    /// 保证无热更 DLL 的开发/演示环境可完整走通启动管线。
    /// </remarks>
    public sealed class HotfixAssemblyLauncher : IHotfixLauncher
    {
#if ENABLE_UI_FAIRYGUI
        private const string LoginScenePath = "res://Assets/Hotfix/UI/FGUI/UILogin/UILogin.tscn";
#else
        private const string LoginScenePath = "res://Assets/Hotfix/UI/GodotGUI/UILogin/UILogin.tscn";
#endif

        /// <summary>
        /// 启动热更入口。
        /// </summary>
        /// <param name="options">启动配置。</param>
        /// <returns>热更启动结果。</returns>
        public async Task<HotfixLaunchResult> StartAsync(StartupOptions options)
        {
            var entryType = HotfixTypeResolver.ResolveOrNull(options.HotfixEntryTypeName);
            if (entryType != null)
            {
                try
                {
                    var entryInstance = Activator.CreateInstance(entryType);
                    if (HotfixTypeResolver.TryInvokeMethod(entryInstance, options.HotfixEntryMethodName))
                    {
                        return HotfixLaunchResult.Succeed();
                    }
                }
                catch (Exception exception)
                {
                    return HotfixLaunchResult.Fail(Utility.Text.Format(
                        "Hotfix entry '{0}.{1}' threw: {2}",
                        options.HotfixEntryTypeName,
                        options.HotfixEntryMethodName,
                        exception.Message));
                }
            }

            Log.Warning("[HotfixLaunch] hotfix entry '{0}.{1}' not found, fallback to UILogin demo flow.",
                options.HotfixEntryTypeName,
                options.HotfixEntryMethodName);
            await OpenLoginFallbackAsync();
            return HotfixLaunchResult.Succeed();
        }

        private static async Task OpenLoginFallbackAsync()
        {
            var uiComponent = GameEntry.GetComponent<GameFrameX.UI.Runtime.UIComponent>();
            if (uiComponent == null)
            {
                Log.Warning("[HotfixLaunch] UIComponent not found, UILogin fallback skipped.");
                return;
            }

            if (FileAccess.FileExists(LoginScenePath) == false)
            {
                Log.Warning("[HotfixLaunch] login scene missing: {0}", LoginScenePath);
                return;
            }

            var loginForm = await uiComponent.OpenUI(LoginScenePath);
            if (loginForm == null)
            {
                Log.Warning("[HotfixLaunch] open UILogin fallback failed.");
            }
        }
    }
}
