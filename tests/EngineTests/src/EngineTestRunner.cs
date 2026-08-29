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

            // ponytail: 退出路径演进史（两次崩溃均在全部用例 PASS 之后的退出期）：
            // 1) GetTree().Quit：引擎 teardown 与 GameSceneManager.Shutdown 发起的异步 Unload 链竞态，已在 556294c
            //    同步化修复；
            // 2) Environment.Exit：绕过引擎正常 teardown，libc exit() 在主线程直接跑 Godot C++ 静态析构器，与未停机的
            //    WorkerThreadPool/StringName 全局态交叠，std::mutex::lock 抛 system_error(EINVAL) → SIGABRT
            //    （2026-08-29 实测 10/10 必现；崩溃报告栈：Environment_Exit→exit→__cxa_finalize_ranges
            //    →Godot 静态析构→mutex::lock，伴随 Unreferenced static string / PagedAllocator 报错）；
            // 故回到 Quit() 正常 teardown 路径（editor 模式同一退出路径实测干净）；
            // 本节点可能已被 C2 Single 模式场景切换释放，故经 Engine.GetMainLoop() 静态获取 SceneTree，
            // 不依赖本节点存活；仅在拿不到 SceneTree 的极端情况下兜底 Environment.Exit。
            var tree = Engine.GetMainLoop() as SceneTree;
            if (tree != null)
            {
                tree.Quit(exitCode);
            }
            else
            {
                System.Environment.Exit(exitCode);
            }
        }

        public override void _Process(double delta)
        {
            // TryCreateGodotDriver 已改为 CallDeferred("add_child") 挂载（_Ready 传播期间 Root 处于
            // "busy setting up children"，直接 AddChild 会被引擎拒绝），驱动节点进树后由其 _Process 自我驱动。
            // 此处手动 Tick 保留为双保险：兜底 deferred 挂载生效前（首帧）的推进窗口，以及本节点被 C2 Single
            // 模式场景切换释放后、驱动节点接管前的窗口；Update 幂等，同帧多次推进无副作用。
            global::GameFrameX.AssetSystem.AssetSystem.Tick();
        }
    }
}
