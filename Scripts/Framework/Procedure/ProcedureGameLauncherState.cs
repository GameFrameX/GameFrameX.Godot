using GameFrameX.Fsm.Runtime;
using GameFrameX.Procedure.Runtime;
using GameFrameX.Runtime;
using Godot;
using Godot.Startup.Demo;

namespace Godot.Startup.Procedure
{
    /// <summary>
    /// 启动热更新后游戏流程。
    /// </summary>
    public sealed class ProcedureGameLauncherState : ProcedureBase
    {
        private const string FairyGuiDemoNodeName = "FairyGuiFlowDemo";

        /// <summary>
        /// 进入流程时执行。
        /// </summary>
        /// <param name="procedureOwner">流程持有者。</param>
        protected internal override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            Log.Info("进入流程：ProcedureGameLauncherState");
            StartFairyGuiFlow();
        }

        private static void StartFairyGuiFlow()
        {
            var sceneTree = Engine.GetMainLoop() as SceneTree;
            var rootNode = sceneTree?.CurrentScene ?? sceneTree?.Root;
            if (rootNode == null)
            {
                Log.Warning("启动 FairyGUI 流程失败：未找到可用场景根节点。");
                return;
            }

            if (rootNode.GetNodeOrNull<FairyGuiFlowDemo>(FairyGuiDemoNodeName) != null)
            {
                Log.Info("FairyGUI 演示流程已存在，跳过重复挂载。");
                return;
            }

            var demoNode = new FairyGuiFlowDemo
            {
                Name = FairyGuiDemoNodeName,
                AutoRunOnReady = true
            };
            rootNode.AddChild(demoNode);
            Log.Info("FairyGUI 演示流程已挂载。");
        }
    }
}
