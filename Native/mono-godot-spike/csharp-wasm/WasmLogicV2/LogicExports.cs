using System;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace WasmLogicV2
{
    // 阶段4 热更验证程序集：与 WasmLogic(V1) 同包共存，boot 时预置进 config assembly 列表，
    // Godot 中途经 SWITCH 命令触发 mono_wasm_assembly_load("WasmLogicV2") 句柄链切换。
    // 行为差异取证：Tick 落盘带 ver=2 标记且计数从 0 重新累计（外层证据文件可见版本翻转与计数重置）。
    // Hello 返回 "version two"（低层 invoke 不回填返回值，此标记供浏览器端验证；微信侧以 ver=2 落盘为准）。
    [SupportedOSPlatform("browser")]
    public static partial class LogicExports
    {
        private static int tickCount = 0;

        [JSExport]
        public static string Hello()
        {
            return "version two";
        }

        [JSExport]
        public static void Tick()
        {
            tickCount += 1;
            System.IO.File.WriteAllText("/spike-tick.txt", "ver=2 ticks=" + tickCount.ToString() + " at " + DateTime.UtcNow.ToString("O"));
        }

        [JSExport]
        public static void TickPure()
        {
            tickCount += 1;
        }
    }
}
