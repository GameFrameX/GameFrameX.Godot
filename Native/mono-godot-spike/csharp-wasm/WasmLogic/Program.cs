using System;
using System.Runtime.Versioning;

namespace WasmLogic
{
    // C1 Spike-W3：Main 只做启动标记（上轮已验证 Timer 回调不会被驱动，移除死代码）
    [SupportedOSPlatform("browser")]
    public static partial class Program
    {
        public static int Main(string[] arguments)
        {
            try
            {
                System.IO.File.WriteAllText("/spike-boot.txt", "main entered at " + DateTime.UtcNow.ToString("O"));
                return 0;
            }
            catch (Exception exception)
            {
                System.IO.File.WriteAllText("/spike-error.txt", exception.ToString());
                return 1;
            }
        }
    }
}
