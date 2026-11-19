using System;
using System.Threading.Tasks;
using GameFrameX.Runtime;
using GameFrameX.Startup.Runtime;
using GameFrameX.UI.Runtime;
using Godot;
using Godot.Startup.Procedure;

namespace GameFrameX.Startup.Application
{
    /// <summary>
    /// 启动 UI 处理器：打开 UILauncher 界面并把提示/进度转发到 <see cref="LauncherFlowProgressReporter"/>
    /// （UILauncher 脚本自轮询该进度源，双后端 FGUI/GDGUI 行为一致）。
    /// </summary>
    public sealed class GameStartupUIHandler : IStartupUIHandler
    {
#if ENABLE_UI_FAIRYGUI
        private const string LauncherScenePath = "res://Assets/Resources/UI/FGUI/UILauncher/UILauncher.tscn";
#else
        private const string LauncherScenePath = "res://Assets/Resources/UI/GGUI/UILauncher/UILauncher.tscn";
#endif

        private IUIForm m_LauncherForm;

        /// <summary>
        /// 打开启动 UI（UILauncher）；界面资源缺失时记录警告并跳过（启动流程不因 UI 缺失中断）。
        /// </summary>
        /// <param name="uiResName">启动 UI 资源路径（当前实现固定使用 UILauncher，参数仅作日志）。</param>
        /// <returns>完成的任务。</returns>
        public async Task StartAsync(string uiResName)
        {
            var uiComponent = GameEntry.GetComponent<UIComponent>();
            if (uiComponent == null)
            {
                Log.Warning("[StartupUI] UIComponent not found, launcher UI skipped.");
                return;
            }

            if (FileAccess.FileExists(LauncherScenePath) == false)
            {
                Log.Warning("[StartupUI] launcher scene missing: {0}", LauncherScenePath);
                return;
            }

            m_LauncherForm = await uiComponent.OpenUI(LauncherScenePath);
            if (m_LauncherForm == null)
            {
                Log.Warning("[StartupUI] open UILauncher failed.");
            }
        }

        /// <summary>
        /// 更新启动提示文本。
        /// </summary>
        /// <param name="text">提示文本。</param>
        public void SetTipText(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                Log.Info("[StartupUI] {0}", text);
            }
        }

        /// <summary>
        /// 更新启动进度。
        /// </summary>
        /// <param name="progress">进度值（0-100）。</param>
        /// <param name="sizeInfo">附加大小信息。</param>
        public void SetProgress(float progress, string sizeInfo)
        {
            LauncherFlowProgressReporter.Report(progress, "startup");
        }

        /// <summary>
        /// 标记进度更新完成。
        /// </summary>
        public void SetProgressUpdateFinish()
        {
            LauncherFlowProgressReporter.Report(100f, "startup-finish");
        }

        /// <summary>
        /// 显示升级提示（当前演示实现恒返回确认升级）。
        /// </summary>
        /// <param name="upgradeInfo">升级信息。</param>
        /// <returns>是否确认升级。</returns>
        public Task<bool> ShowUpgradeAsync(StartupUpgradeInfo upgradeInfo)
        {
            Log.Info("[StartupUI] upgrade prompt accepted by default. title={0}", upgradeInfo?.UpdateTitle ?? string.Empty);
            return Task.FromResult(true);
        }

        /// <summary>
        /// 关闭启动 UI 处理器；UILauncher 交由界面自身流转（UILogin 转场由 UILauncher 驱动），此处仅释放引用。
        /// </summary>
        public void Dispose()
        {
            m_LauncherForm = null;
        }
    }
}
