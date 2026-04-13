using GameFrameX.Fsm.Runtime;
using GameFrameX.Procedure.Runtime;
using GameFrameX.Runtime;

namespace Godot.Startup.Procedure;

/// <summary>
/// 游戏启动阶段流程。
/// </summary>
public sealed class ProcedureGameLauncherState : ProcedureBase
{
	/// <summary>
	/// 进入流程时执行。
	/// </summary>
	/// <param name="procedureOwner">流程持有者。</param>
	protected internal override void OnEnter(IFsm<IProcedureManager> procedureOwner)
	{
		base.OnEnter(procedureOwner);
		Log.Info("进入流程：ProcedureGameLauncherState");
		LauncherFlowProgressReporter.Report(100f, nameof(ProcedureGameLauncherState));
		ProcedureLauncherState.EnsureLauncherUiFlowStarted(nameof(ProcedureGameLauncherState));
	}
}
