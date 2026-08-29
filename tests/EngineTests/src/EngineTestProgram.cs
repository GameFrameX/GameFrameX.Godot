using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Godot;

namespace GameFrameX.EngineTests
{
    /// <summary>
    /// 引擎测试编排器：注册全部用例组，按运行上下文过滤套件，逐个执行并输出 PASS/FAIL 明细，返回进程退出码。
    /// </summary>
    public sealed class EngineTestProgram
    {
        public async Task<int> RunAllAsync()
        {
            GD.Print("=== GameFrameX Godot 引擎测试开始 ===");
            GD.Print("环境: TOOLS 编译=" + EngineTestEnvironment.HasToolsDefine + ", IsEditorHint=" + Engine.IsEditorHint() + ", headless=" + DisplayServer.GetName());
            var cases = new List<EngineTestCase>();
            CollectCases(cases);
            var nameFilter = OS.GetEnvironment("ENGINE_TEST_FILTER");
            if (string.IsNullOrEmpty(nameFilter) == false)
            {
                for (var i = cases.Count - 1; i >= 0; i--)
                {
                    if (cases[i].Name.Contains(nameFilter) == false)
                    {
                        cases.RemoveAt(i);
                    }
                }
            }
            var passed = 0;
            var failed = 0;
            var skipped = 0;
            var stopwatch = new Stopwatch();
            foreach (var testCase in cases)
            {
                stopwatch.Restart();
                try
                {
                    await testCase.Body();
                    passed++;
                    GD.Print("PASS [" + testCase.Group + "] " + testCase.Name + " (" + stopwatch.ElapsedMilliseconds + "ms)");
                }
                catch (EngineTestSkippedException skip)
                {
                    skipped++;
                    GD.Print("SKIP [" + testCase.Group + "] " + testCase.Name + " : " + skip.Message);
                }
                catch (Exception exception)
                {
                    failed++;
                    GD.Print("FAIL [" + testCase.Group + "] " + testCase.Name + " : " + exception.GetType().Name + ": " + exception.Message);
                    if (exception.InnerException != null)
                    {
                        GD.Print("     inner: " + exception.InnerException.GetType().Name + ": " + exception.InnerException.Message);
                    }
                    PrintStackTraceHead(exception.StackTrace);
                }
            }
            GD.Print("=== RESULT: total=" + cases.Count + " passed=" + passed + " failed=" + failed + " skipped=" + skipped + " ===");
            if (failed > 0)
            {
                return 1;
            }
            return 0;
        }

        private static void PrintStackTraceHead(string stackTrace)
        {
            if (string.IsNullOrEmpty(stackTrace))
            {
                return;
            }
            var lines = stackTrace.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var limit = lines.Length < 3 ? lines.Length : 3;
            for (var i = 0; i < limit; i++)
            {
                GD.Print("     " + lines[i].Trim());
            }
        }

        private static void CollectCases(List<EngineTestCase> cases)
        {
            AssetSystemEditorSimulateTests.Register(cases);
            BundledPckTests.Register(cases);
            SoundModuleTests.Register(cases);
            // Sound 组需先于 Scene 组执行：C2 的 Single 模式会释放游戏模式的 runner 主场景，
            // 之后节点不可再 AddChild（ObjectDisposedException），故含释放副作用的用例排最后。
            UIDesignResolutionTests.Register(cases);
            SceneModuleTests.Register(cases);
            FilterByRunMode(cases);
        }

        private static void FilterByRunMode(List<EngineTestCase> cases)
        {
            var editorContext = Engine.IsEditorHint();
            // 编辑器上下文：A 组 + B6 + C/D（B1-B5 依赖 ProjectSettings.LoadResourcePack，编辑器模式不支持挂载 PCK）
            // 游戏上下文：B 组全部 + C/D（A 组依赖 TOOLS 构建 + 编辑器上下文；C/D 改走 HostPlayMode 真实下载链路）
            for (var i = cases.Count - 1; i >= 0; i--)
            {
                var testCase = cases[i];
                var keep = true;
                if (editorContext)
                {
                    keep = testCase.Group != "BundledPck" || testCase.Name == "B6_PackageLoadMissingAssetMustFail";
                }
                else
                {
                    keep = testCase.Group != "AssetSystem";
                }

                if (keep == false)
                {
                    cases.RemoveAt(i);
                }
            }
        }
    }
}
