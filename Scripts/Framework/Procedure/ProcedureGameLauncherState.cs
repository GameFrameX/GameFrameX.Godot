using GameFrameX.Fsm.Runtime;
using GameFrameX.Procedure.Runtime;
using GameFrameX.Runtime;
using Godot;
using Godot.Startup.UIFlow;

namespace Godot.Startup.Procedure
{
    /// <summary>
    /// 启动热更新后游戏流程。
    /// </summary>
    public sealed class ProcedureGameLauncherState : ProcedureBase
    {
        // 编译开关：定义 FAIRY_GUI 时启用 FairyGUI 流程；未定义时启用 Godot GDGUI 流程。
        private const string FairyGuiDemoNodeName = "FairyGuiFlowDemo";
        private const string GodotGuiDemoNodeName = "GodotGuiFlowDemo";

        /// <summary>
        /// 进入流程时执行。
        /// </summary>
        /// <param name="procedureOwner">流程持有者。</param>
        protected internal override void OnEnter(IFsm<IProcedureManager> procedureOwner)
        {
            base.OnEnter(procedureOwner);
            Log.Info("进入流程：ProcedureGameLauncherState");
            StartUiFlow();
        }

        private static void StartUiFlow()
        {
            var sceneTree = Engine.GetMainLoop() as SceneTree;
            var rootNode = sceneTree?.CurrentScene ?? sceneTree?.Root;
            Log.Info("[UIFlow] StartUiFlow sceneTree={0} currentScene={1} root={2}",
                sceneTree != null,
                sceneTree?.CurrentScene?.Name ?? "<null>",
                sceneTree?.Root?.Name ?? "<null>");

            if (rootNode == null)
            {
                Log.Warning("[UIFlow] 启动 UI 流程失败：未找到可用场景根节点。 currentScene={0} root={1}",
                    sceneTree?.CurrentScene?.Name ?? "<null>",
                    sceneTree?.Root?.Name ?? "<null>");
                return;
            }

#if FAIRY_GUI
            Log.Info("[UIFlow] 编译模式：FAIRY_GUI（走 FairyGUI）");
            StartFairyGuiFlow(rootNode);
#else
            Log.Info("[UIFlow] 编译模式：GDGUI（未定义 FAIRY_GUI，走 Godot GUI）");
            StartGodotGuiFlow(rootNode);
#endif
        }

        private static void StartGodotGuiFlow(Node rootNode)
        {
            var existingGodotDemo = rootNode.GetNodeOrNull<GodotGuiFlowDemo>(GodotGuiDemoNodeName);
            if (existingGodotDemo != null)
            {
                Log.Warning("[UIFlow] GodotGuiFlowDemo 已存在，触发重启而非跳过。 node={0}", existingGodotDemo.Name);
                existingGodotDemo.ForceRestartFlow("ProcedureGameLauncherState re-enter");
                return;
            }

            var fairyDemo = rootNode.GetNodeOrNull<FairyGuiFlowDemo>(FairyGuiDemoNodeName);
            if (fairyDemo != null)
            {
                fairyDemo.QueueFree();
                Log.Info("[UIFlow] 已移除 FairyGUI 演示流程节点。 node={0}", fairyDemo.Name);
            }

            var godotDemo = new GodotGuiFlowDemo
            {
                Name = GodotGuiDemoNodeName,
                AutoRunOnReady = true
            };
            rootNode.AddChild(godotDemo);
            Log.Info("[UIFlow] Godot GUI 演示流程已挂载。 node={0} parent={1}", godotDemo.Name, rootNode.Name);
        }

        private static void StartFairyGuiFlow(Node rootNode)
        {
            var existingFairyDemo = rootNode.GetNodeOrNull<FairyGuiFlowDemo>(FairyGuiDemoNodeName);
            if (existingFairyDemo != null)
            {
                Log.Warning("[UIFlow] FairyGuiFlowDemo 已存在，触发重启而非跳过。 node={0}", existingFairyDemo.Name);
                existingFairyDemo.ForceRestartFlow("ProcedureGameLauncherState re-enter");
                return;
            }

            var godotDemo = rootNode.GetNodeOrNull<GodotGuiFlowDemo>(GodotGuiDemoNodeName);
            if (godotDemo != null)
            {
                godotDemo.QueueFree();
                Log.Info("[UIFlow] 已移除 Godot GUI 演示流程节点。 node={0}", godotDemo.Name);
            }

            var fairyDemo = new FairyGuiFlowDemo
            {
                Name = FairyGuiDemoNodeName,
                AutoRunOnReady = true
            };
            rootNode.AddChild(fairyDemo);
            Log.Info("[UIFlow] FairyGUI 演示流程已挂载。 node={0} parent={1}", fairyDemo.Name, rootNode.Name);
        }
    }
}

