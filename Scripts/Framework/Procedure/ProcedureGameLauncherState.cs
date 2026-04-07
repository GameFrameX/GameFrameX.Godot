using System.Threading.Tasks;
using GameFrameX.Fsm.Runtime;
using GameFrameX.Procedure.Runtime;
using GameFrameX.Runtime;
using GameFrameX.UI.Runtime;
using Godot;

#if FAIRY_GUI
using UILauncher = Godot.Hotfix.FairyGUI.UILauncher;
#else
using UILauncher = Godot.Hotfix.GodotGUI.UILauncher;
#endif

namespace Godot.Startup.Procedure
{
	/// <summary>
	/// 启动热更新后游戏流程（框架驱动：UIComponent/UIManager）。
	/// </summary>
	public sealed class ProcedureGameLauncherState : ProcedureBase
	{
		private static bool s_IsFlowRunning;

		/// <summary>
		/// 进入流程时执行。
		/// </summary>
		/// <param name="procedureOwner">流程持有者。</param>
		protected internal override void OnEnter(IFsm<IProcedureManager> procedureOwner)
		{
			base.OnEnter(procedureOwner);
			Log.Info("进入流程：ProcedureGameLauncherState");
			_ = StartUiFlowAsync();
		}

		private static async Task StartUiFlowAsync()
		{
			if (s_IsFlowRunning)
			{
				Log.Warning("[UIFlow] 已有流程在运行，忽略重复启动。");
				return;
			}

			s_IsFlowRunning = true;
			try
			{
#if FAIRY_GUI
				Log.Info("[UIFlow] 编译模式：FAIRY_GUI（走框架 UIManager + FairyGUI UIForm）");
#else
				Log.Info("[UIFlow] 编译模式：GDGUI（走框架 UIManager + Godot GUI UIForm）");
#endif

				var UIComp = GameEntry.GetComponent<UIComponent>();
				if (UIComp == null)
				{
					Log.Error("[UIFlow] UIComponent not found.");
					return;
				}

				var launcher = await UIComp.OpenRequiredAsync<UILauncher>();
				if (launcher == null)
				{
					Log.Error("[UIFlow] 打开 UILauncher 失败。");
					return;
				}

				Log.Info("[UIFlow] UILauncher 已显示，后续切换由 UILauncher 内部逻辑处理。");
			}
			catch (System.Exception exception)
			{
				Log.Error("[UIFlow] 流程异常：{0}", exception);
			}
			finally
			{
				s_IsFlowRunning = false;
			}
		}

	}
}
