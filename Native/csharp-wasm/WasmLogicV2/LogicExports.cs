using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace WasmLogicV2
{
    // 场景化热更逻辑层（V2）：黄昏场景 + 减速按钮 + 每 12 tick 自动轮换配色。
    // 与 V1 同包共存（均经 CDN 分发）；激活哪个版本由 HotupdateController 依据 CDN manifest 决策。
    // 文件格式（EncodeSpecLines 产出）：单行、条目以 ; 分隔、空格以 ~ 占位；JS 桥只透传不解析。
    [SupportedOSPlatform("browser")]
    public static partial class LogicExports
    {
        private static int tickCount = 0;
        private static int rotationDirection = -1;
        private static double speedMultiplier = 1.6;
        private static int paletteIndex = 2;
        private static double pulseAmount = 0.0;
        private static int consumedEventCount = 0;
        private static string activeAssemblyName = "WasmLogicV2";

        private static readonly double[][] ObjectPalettes = new double[][]
        {
            new double[] { 0.95, 0.45, 0.18 },
            new double[] { 0.72, 0.30, 0.85 },
            new double[] { 0.15, 0.85, 0.45 },
            new double[] { 0.95, 0.78, 0.25 },
        };

        [JSExport]
        public static string Hello()
        {
            return "version two";
        }

        [JSExport]
        public static void WriteSceneSpec()
        {
            var entries = new string[]
            {
                "sky 0.36 0.18 0.42 0.98 0.55 0.28 0.12 0.08 0.16",
                "ground 0.24 0.18 0.26",
                "camera 0.0 2.1 5.8 0.55",
                "light -60 25 1.4",
                "object mainCube box 0.0 0.46 0.0 1.45 0.15 0.85 0.45 0.55 1.80 0.85",
                "object ringTorus torus 0.0 0.50 0.0 0.62 0.95 0.78 0.25 0.60 0.55 0.00",
                "object sideTower cylinder 2.15 0.85 -0.85 0.34 0.72 0.30 0.85 0.25 0.65 0.30",
                "object smallSphere sphere -1.95 0.40 1.35 0.42 0.36 0.18 0.42 0.20 1.10 -0.50",
            };
            WriteEncodedFile("/scene-spec.txt", entries);
        }

        [JSExport]
        public static void WriteUiSpec()
        {
            var entries = new string[]
            {
                "button direction 36 60 40 反向自转 V2",
                "button slowdown 36 130 40 减速自转 V2",
                "button palette 36 200 40 切换配色 V2",
                "label status 36 270 36 V2 热更新已生效 黄昏场景",
                "label counter 36 322 30 C# ticks: booting",
            };
            WriteEncodedFile("/ui-spec.txt", entries);
        }

        [JSExport]
        public static void Tick()
        {
            tickCount += 1;
            System.IO.File.WriteAllText("/spike-tick.txt", "ver=2 ticks=" + tickCount.ToString(CultureInfo.InvariantCulture)
                + " at " + DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture));
            ConsumeUiEvents();
            RunHotupdateController();
            if (tickCount % 12 == 0)
            {
                paletteIndex = (paletteIndex + 1) % ObjectPalettes.Length;
                pulseAmount = 0.18;
            }
            WriteUiState();
            if (pulseAmount > 0.0)
            {
                pulseAmount = Math.Max(0.0, pulseAmount - 0.12);
            }
        }

        [JSExport]
        public static void TickPure()
        {
            tickCount += 1;
        }

        // 热更决策（逻辑在 C#）：读 JS 投递的 CDN manifest，目标版本 ≠ 当前激活 → 写切换决策；
        // JS 侧只照办执行（mono_wasm_assembly_load 只能命中 boot 预置表，字节搬运必须经运行时加载器）。
        private static void RunHotupdateController()
        {
            string manifestText;
            try
            {
                manifestText = System.IO.File.ReadAllText("/cdn-manifest.txt");
            }
            catch (Exception)
            {
                return; // manifest 未就绪：维持现状
            }
            var targetAssembly = ParseManifestField(manifestText, "target");
            if (targetAssembly.Length == 0 || targetAssembly == activeAssemblyName)
            {
                return;
            }
            var entries = new string[]
            {
                "switch " + targetAssembly,
                "from " + activeAssemblyName,
                "at " + DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture),
            };
            WriteEncodedFile("/hotupdate-decision.txt", entries);
        }

        private static string ParseManifestField(string manifestText, string fieldName)
        {
            // 兼容压缩单行与多行两种 JSON 形态：统一按逗号分段，再剥掉花括号/方括号后取 key:value
            foreach (var rawSegment in manifestText.Split(','))
            {
                var segment = rawSegment.Trim().TrimStart('{').TrimEnd('}').Trim();
                if (segment.Length == 0)
                {
                    continue;
                }
                var separator = segment.IndexOf(':');
                if (separator <= 0)
                {
                    continue;
                }
                var key = segment.Substring(0, separator).Trim().Trim('"');
                if (key != fieldName)
                {
                    continue;
                }
                return segment.Substring(separator + 1).Trim().Trim('"').TrimEnd(']', '}').Trim();
            }
            return "";
        }

        private static void ConsumeUiEvents()
        {
            if (!System.IO.File.Exists("/ui-events.txt"))
            {
                return;
            }
            string events;
            try
            {
                events = System.IO.File.ReadAllText("/ui-events.txt");
            }
            catch (Exception)
            {
                return;
            }
            foreach (var rawLine in events.Split('\n'))
            {
                var line = rawLine.Trim();
                if (line.Length == 0 || !line.StartsWith("button:", StringComparison.Ordinal))
                {
                    continue;
                }
                ApplyButtonEvent(line.Substring("button:".Length));
            }
            try
            {
                System.IO.File.Delete("/ui-events.txt");
            }
            catch (Exception)
            {
                // 事件幂等，重复消费无害
            }
        }

        private static void ApplyButtonEvent(string buttonIdentifier)
        {
            consumedEventCount += 1;
            pulseAmount = 0.25;
            if (buttonIdentifier == "direction")
            {
                rotationDirection = -rotationDirection;
            }
            else if (buttonIdentifier == "slowdown")
            {
                speedMultiplier = Math.Max(0.2, speedMultiplier / 1.5);
            }
            else if (buttonIdentifier == "palette")
            {
                paletteIndex = (paletteIndex + 1) % ObjectPalettes.Length;
            }
        }

        private static void WriteUiState()
        {
            var palette = ObjectPalettes[paletteIndex];
            var entries = new string[]
            {
                "motion " + FormatDouble(rotationDirection) + " " + FormatDouble(speedMultiplier),
                "objectState mainCube " + FormatTriple(palette) + " " + FormatDouble(pulseAmount),
                "objectState ringTorus " + FormatTriple(1.0 - palette[0] * 0.4, palette[1], palette[2]) + " " + FormatDouble(pulseAmount),
                "objectState sideTower " + FormatTriple(palette[2], palette[0], palette[1]) + " " + FormatDouble(pulseAmount),
                "objectState smallSphere " + FormatTriple(palette[1], palette[2], palette[0]) + " " + FormatDouble(pulseAmount),
                "labelText counter C# ticks: " + tickCount.ToString(CultureInfo.InvariantCulture) + " (v2)",
                "labelText status 方向 " + (rotationDirection > 0 ? "正向" : "反向")
                    + " 速度 x" + speedMultiplier.ToString("0.0", CultureInfo.InvariantCulture)
                    + " 配色 " + (paletteIndex + 1).ToString(CultureInfo.InvariantCulture)
                    + " 事件 " + consumedEventCount.ToString(CultureInfo.InvariantCulture),
            };
            WriteEncodedFile("/ui-state.txt", entries);
        }

        private static void WriteEncodedFile(string path, string[] entries)
        {
            // 仅用数组与 string.Join（CoreLib/System.Runtime 即可满足，避免引入 System.Collections）
            var encoded = new string[entries.Length];
            for (var index = 0; index < entries.Length; index++)
            {
                encoded[index] = entries[index].Replace(" ", "~");
            }
            System.IO.File.WriteAllText(path, string.Join(";", encoded));
        }

        private static string FormatTriple(params double[] values)
        {
            var formatted = new string[values.Length];
            for (var index = 0; index < values.Length; index++)
            {
                formatted[index] = FormatDouble(values[index]);
            }
            return string.Join(" ", formatted);
        }

        private static string FormatDouble(double value)
        {
            return value.ToString("0.####", CultureInfo.InvariantCulture);
        }
    }
}
