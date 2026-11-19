using System;
using System.Runtime.Versioning;

namespace WasmLogicV2
{
    // 阶段4 热更程序集入口（编译需要）：实际运行时仅 WasmLogic(V1) 的 Main 会被 runMain 调用，
    // 本 Main 不执行；V2 行为差异全部经 LogicExports.Tick 的 ver=2 落盘标记取证。
    [SupportedOSPlatform("browser")]
    public static partial class Program
    {
        public static int Main(string[] arguments)
        {
            System.IO.File.WriteAllText("/spike-boot-v2.txt", "v2 main entered at " + DateTime.UtcNow.ToString("O"));
            return 0;
        }
    }
}
