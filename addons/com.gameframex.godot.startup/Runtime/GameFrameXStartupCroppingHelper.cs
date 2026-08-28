using Godot;

namespace GameFrameX.Startup.Runtime
{
    /// <summary>
    /// 裁剪保护帮助类。通过引用全部运行时类型，防止 IL 裁剪器误删未被静态引用的类型。
    /// </summary>
    public partial class GameFrameXStartupCroppingHelper : Node
    {
        public override void _Ready()
        {
            _ = typeof(BlackBoardKeys);
            _ = typeof(DefaultStartupHttpParamsProvider);
            _ = typeof(HotfixLaunchResult);
            _ = typeof(IHotfixLauncher);
            _ = typeof(IStartupHttpParams);
            _ = typeof(IStartupHttpParamsProvider);
            _ = typeof(IStartupUIHandler);
            _ = typeof(StartupCompletedEventArgs);
            _ = typeof(StartupFailedEventArgs);
            _ = typeof(StartupHttpParams);
            _ = typeof(StartupNetworkCacheUtility);
            _ = typeof(StartupOptions);
            _ = typeof(StartupProcedureUtility);
            _ = typeof(StartupResult);
            _ = typeof(StartupRunner);
            _ = typeof(StartupUpgradeInfo);
            _ = typeof(UrlAttemptResult);
            _ = typeof(UrlFailoverResult);
            _ = typeof(UrlFailoverRunner);
        }
    }
}
