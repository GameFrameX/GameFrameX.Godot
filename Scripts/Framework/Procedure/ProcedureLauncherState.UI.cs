using System;
using System.Threading.Tasks;
using GameFrameX.Runtime;
using GameFrameX.UI.Runtime;
using Godot;

namespace Godot.Startup.Procedure;

public sealed partial class ProcedureLauncherState
{
    private static readonly object s_LauncherUiFlowGate = new object();
    private static bool s_IsLauncherUiFlowRunning;
    private static bool s_LauncherUiFlowStarted;

#if FAIRY_GUI
    private const string LauncherScenePath = "res://Assets/Resources/UI/FGUI/UILauncher/UILauncher.tscn";
#else
    private const string LauncherScenePath = "res://Assets/Resources/UI/GGUI/UILauncher/UILauncher.tscn";
#endif

    internal static void EnsureLauncherUiFlowStarted(string callerTag)
    {
        lock (s_LauncherUiFlowGate)
        {
            if (s_LauncherUiFlowStarted || s_IsLauncherUiFlowRunning)
            {
                return;
            }

            s_IsLauncherUiFlowRunning = true;
        }

        _ = StartLauncherUiFlowAsync(callerTag);
    }

    internal static bool IsLauncherUiReady()
    {
        lock (s_LauncherUiFlowGate)
        {
            return s_LauncherUiFlowStarted;
        }
    }

    private static async Task StartLauncherUiFlowAsync(string callerTag)
    {
        try
        {
#if FAIRY_GUI
            Log.Info("[LauncherUI] 编译模式：FAIRY_GUI caller={0}", callerTag);
#else
            Log.Info("[LauncherUI] 编译模式：GDGUI caller={0}", callerTag);
#endif

            var uiComp = GameEntry.GetComponent<UIComponent>();
            if (uiComp == null)
            {
                Log.Warning("[LauncherUI] UIComponent not found. caller={0}", callerTag);
                return;
            }

            if (TryPrepareLauncherResources(out var launcherPrepareError) == false)
            {
                Log.Warning("[LauncherUI] launcher resources prepare failed: {0}", launcherPrepareError);
                return;
            }

            var launcher = await uiComp.OpenUI(LauncherScenePath);
            if (launcher == null)
            {
                Log.Warning("[LauncherUI] open UILauncher failed.");
                return;
            }

            lock (s_LauncherUiFlowGate)
            {
                s_LauncherUiFlowStarted = true;
            }

            Log.Info("[LauncherUI] UILauncher shown by {0}.", callerTag);
        }
        catch (Exception exception)
        {
            Log.Error("[LauncherUI] flow exception: {0}", exception);
        }
        finally
        {
            lock (s_LauncherUiFlowGate)
            {
                s_IsLauncherUiFlowRunning = false;
            }
        }
    }

    private static bool TryPrepareLauncherResources(out string error)
    {
        error = string.Empty;
        if (FileAccess.FileExists(LauncherScenePath) == false)
        {
            error = $"launcher scene missing: {LauncherScenePath}";
            return false;
        }

        var launcherScene = ResourceLoader.Load<PackedScene>(LauncherScenePath);
        if (launcherScene == null)
        {
            error = $"launcher scene load failed: {LauncherScenePath}";
            return false;
        }

        Log.Info("[LauncherUI] launcher scene prepared. scene={0}", LauncherScenePath);
        return true;
    }
}
