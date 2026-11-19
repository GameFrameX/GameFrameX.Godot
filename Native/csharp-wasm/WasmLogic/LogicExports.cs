using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace WasmLogic
{
    // 场景化热更逻辑层（V1）：场景内容、UI 布局、行为状态全部由 C# 决定，经 CDN 分发。
    // 通道约束（探针实证）：微信容器缺 WebAssembly.Function，[JSExport] 绑定失败；
    // 全部入口保持无参 void，JS 桥仅"invoke → 读文件 → 原样透传"，不做任何解析/编码。
    // 文件格式（由 EncodeSpecLines 产出）：单行、条目以 ; 分隔、空格以 ~ 占位。
    //   WriteSceneSpec → /scene-spec.txt   WriteUiSpec → /ui-spec.txt
    //   Tick          → /spike-tick.txt（计数取证，格式固定）+ /ui-state.txt，消费 /ui-events.txt
    [SupportedOSPlatform("browser")]
    public static partial class LogicExports
    {
        private static int tickCount = 0;
        private static int rotationDirection = 1;
        private static double speedMultiplier = 1.0;
        private static int paletteIndex = 0;
        private static double pulseAmount = 0.0;
        private static int consumedEventCount = 0;
        private static string activeAssemblyName = "WasmLogic";

        private static readonly double[][] ObjectPalettes = new double[][]
        {
            new double[] { 1.00, 0.55, 0.15 },
            new double[] { 0.20, 0.62, 0.95 },
            new double[] { 0.85, 0.25, 0.45 },
            new double[] { 0.95, 0.80, 0.20 },
        };

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

        // 场景规格：天空/地面/相机/灯光 + 对象清单（类型 位置 尺寸 颜色 材质 自转速度）
        [JSExport]
        public static void WriteSceneSpec()
        {
            var entries = new string[]
            {
                "sky 0.22 0.44 0.86 0.55 0.76 0.95 0.16 0.15 0.22",
                "ground 0.30 0.40 0.34",
                "camera 0.0 1.8 5.4 0.35",
                "light -45 -30 1.2",
                "object mainCube box 0.0 0.42 0.0 1.30 1.00 0.55 0.15 0.35 1.30 0.55",
                "object orbitSphere sphere 1.85 0.55 0.35 0.52 0.20 0.62 0.95 0.15 0.90 -0.35",
                "object cornerColumn cylinder -2.05 0.65 -1.25 0.28 1.10 0.34 0.95 0.15 0.40 0.40",
            };
            WriteEncodedFile("/scene-spec.txt", entries);
        }

        // UI 规格：按钮与标签（位置、字号、文本）
        [JSExport]
        public static void WriteUiSpec()
        {
            var entries = new string[]
            {
                "button direction 36 60 40 切换旋转方向",
                "button speedup 36 130 40 加速自转",
                "button palette 36 200 40 切换配色",
                "label status 36 270 36 C# 热更新逻辑驱动中",
                "label counter 36 322 30 C# ticks: booting",
            };
            WriteEncodedFile("/ui-spec.txt", entries);
        }

        // 帧驱动入口：计数落盘（取证格式固定）+ 事件消费 + 状态落盘
        [JSExport]
        public static void Tick()
        {
            tickCount += 1;
            System.IO.File.WriteAllText("/spike-tick.txt", "ticks=" + tickCount.ToString(CultureInfo.InvariantCulture)
                + " at " + DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture));
            ConsumeUiEvents();
            RunHotupdateController();
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

        [JSExport]
        public static string TickCountText()
        {
            return "count=" + tickCount.ToString(CultureInfo.InvariantCulture);
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

        // 事件消费：/ui-events.txt 每行 "button:<id>"，处理后删除文件
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
            else if (buttonIdentifier == "speedup")
            {
                speedMultiplier = Math.Min(6.0, speedMultiplier * 1.5);
            }
            else if (buttonIdentifier == "palette")
            {
                paletteIndex = (paletteIndex + 1) % ObjectPalettes.Length;
            }
        }

        // 状态落盘：全局运动参数 + 每对象颜色与脉冲 + 标签文本
        private static void WriteUiState()
        {
            var palette = ObjectPalettes[paletteIndex];
            var entries = new string[]
            {
                "motion " + FormatDouble(rotationDirection) + " " + FormatDouble(speedMultiplier),
                "objectState mainCube " + FormatTriple(palette) + " " + FormatDouble(pulseAmount),
                "objectState orbitSphere " + FormatTriple(1.0 - palette[0] * 0.5, palette[1], palette[2]) + " " + FormatDouble(pulseAmount),
                "objectState cornerColumn " + FormatTriple(palette[1], palette[2], palette[0]) + " " + FormatDouble(pulseAmount),
                "labelText counter C# ticks: " + tickCount.ToString(CultureInfo.InvariantCulture) + " (v1)",
                "labelText status 自转方向 " + (rotationDirection > 0 ? "正向" : "反向")
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
