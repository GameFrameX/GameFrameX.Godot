using Godot;

namespace GameFrameX.EngineTests
{
    /// <summary>
    /// headless 引擎测试入口节点：
    /// 编辑器模式: godot --headless -e --path . --script res://tests/EngineTests/EngineTestMain.gd
    /// 游戏模式:   godot --headless --path . res://tests/EngineTests/EngineTestRunner.tscn
    /// 退出码 0=全部通过, 1=存在失败。
    /// </summary>
    public partial class EngineTestRunner : Node
    {
        public override async void _Ready()
        {
            EngineTestContext.Root = this;
            var program = new EngineTestProgram();
            var exitCode = 1;
            try
            {
                exitCode = await program.RunAllAsync();
            }
            catch (System.Exception exception)
            {
                GD.PrintErr("引擎测试框架致命错误: " + exception.Message);
                exitCode = 1;
            }

            // ponytail: 游戏模式下 GetTree().Quit 后引擎 teardown 与 C# 挂起续体存在竞态，
            // 曾在全部用例 PASS 后触发 mutex lock failed SIGABRT（exit 134 关停崩溃，栈关联
            // GameSceneManager.Shutdown → UnloadScene 的关停期清理，已列入框架问题清单）；
            // headless 测试进程只需退出码与日志，直接跳过引擎正常收尾（若本节点已被 Single 模式
            // 场景切换释放，Quit 同样无法执行，Environment.Exit 两者兼治）。
            System.Environment.Exit(exitCode);
        }

        public override void _Process(double delta)
        {
            // 框架 Initialize 的 TryCreateGodotDriver 在 _Ready 期间 add_child 会被引擎拒绝
            //（"Parent node is busy setting up children"），驱动节点实际未进树、无人驱动 OperationSystem；
            // 这里每帧手动 Tick，保证异步操作持续前进（驱动节点创建成功时仅是每帧多推进一次，无副作用）。
            global::GameFrameX.AssetSystem.AssetSystem.Tick();
        }
    }
}
