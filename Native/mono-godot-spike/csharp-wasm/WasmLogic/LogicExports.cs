using System;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace WasmLogic
{
    // C1 Spike-W3：JS↔C# 底层 invoke 桥的目标方法面。
    // 微信容器缺 WebAssembly.Function，[JSExport] 启动期绑定全部静默失败；
    // 因此 Tick 采用无参无返回签名，由 JS 侧经 runtime.Module 的
    // mono_wasm_assembly_load / find_class / find_method / mono_wasm_invoke_jsexport
    // 句柄链直接驱动，绕过绑定层。[JSExport] 属性保留仅为浏览器 PoC 兼容。
    [SupportedOSPlatform("browser")]
    public static partial class LogicExports
    {
        private static int tickCount = 0;

        [JSExport]
        public static string Hello()
        {
            return "hello from csharp wasm logic";
        }

        [JSExport]
        public static int Add(int leftValue, int rightValue)
        {
            return leftValue + rightValue;
        }

        // 帧驱动入口：JS 桥循环调用，累计计数并落盘 MEMFS 供 JS 读回校验
        [JSExport]
        public static void Tick()
        {
            tickCount += 1;
            System.IO.File.WriteAllText("/spike-tick.txt", "ticks=" + tickCount.ToString() + " at " + DateTime.UtcNow.ToString("O"));
        }

        // 性能测量入口：只累计不落盘，供 JS 桥测纯 invoke 开销（避免文件 I/O 干扰）
        [JSExport]
        public static void TickPure()
        {
            tickCount += 1;
        }

        // 计数读回入口：验证带 string 返回值的低层 invoke 结果槽读取
        [JSExport]
        public static string TickCountText()
        {
            return "count=" + tickCount.ToString();
        }
    }
}
