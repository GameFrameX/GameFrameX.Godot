using System;
using System.Globalization;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace WasmLogicV2
{
    // 阶段4 热更验证程序集（场景化版本）：与 V1 同包共存，SWITCH 触发句柄链切换后，
    // 宿主重新拉取规格 → 场景与 UI 整体换脸（黄昏天空 + 不同对象组合 + 不同按钮行为）。
    // 与 V1 的可见差异：黄昏天空/环+柱对象、减速按钮（V1 为加速）、每 12 tick 自动渐变配色。
    [SupportedOSPlatform("browser")]
    public static partial class LogicExports
    {
        private static int tickCount = 0;
        private static int rotationDirection = -1;
        private static double speedMultiplier = 1.6;
        private static int paletteIndex = 2;
        private static double pulseAmount = 0.0;
        private static int consumedEventCount = 0;

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
            var builder = new System.Text.StringBuilder();
            builder.AppendLine("sky 0.36 0.18 0.42 0.98 0.55 0.28 0.12 0.08 0.16");
            builder.AppendLine("ground 0.24 0.18 0.26");
            builder.AppendLine("camera 0.0 2.1 5.8 0.55");
            builder.AppendLine("light -60 25 1.4");
            builder.AppendLine("object mainCube box 0.0 0.46 0.0 1.45 0.15 0.85 0.45 0.55 1.80 0.85");
            builder.AppendLine("object ringTorus torus 0.0 0.50 0.0 0.62 0.95 0.78 0.25 0.60 0.55 0.00");
            builder.AppendLine("object sideTower cylinder 2.15 0.85 -0.85 0.34 0.72 0.30 0.85 0.25 0.65 0.30");
            builder.AppendLine("object smallSphere sphere -1.95 0.40 1.35 0.42 0.36 0.18 0.42 0.20 1.10 -0.50");
            System.IO.File.WriteAllText("/scene-spec.txt", builder.ToString());
        }

        [JSExport]
        public static void WriteUiSpec()
        {
            var builder = new System.Text.StringBuilder();
            builder.AppendLine("button direction 36 60 40 反向自转(V2)");
            builder.AppendLine("button slowdown 36 130 40 减速自转(V2)");
            builder.AppendLine("button palette 36 200 40 切换配色(V2)");
            builder.AppendLine("label status 36 270 36 V2~热更新已生效:~黄昏场景");
            builder.AppendLine("label counter 36 322 30 C#~ticks:~booting");
            System.IO.File.WriteAllText("/ui-spec.txt", builder.ToString());
        }

        [JSExport]
        public static void Tick()
        {
            tickCount += 1;
            System.IO.File.WriteAllText("/spike-tick.txt", "ver=2 ticks=" + tickCount.ToString(CultureInfo.InvariantCulture)
                + " at " + DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture));
            ConsumeUiEvents();
            // V2 行为差异：每 12 tick 自动轮换配色
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
            var builder = new System.Text.StringBuilder();
            builder.AppendLine("motion " + FormatDouble(rotationDirection) + " " + FormatDouble(speedMultiplier));
            builder.AppendLine("objectState mainCube "
                + FormatDouble(palette[0]) + " " + FormatDouble(palette[1]) + " " + FormatDouble(palette[2]) + " "
                + FormatDouble(pulseAmount));
            builder.AppendLine("objectState ringTorus "
                + FormatDouble(1.0 - palette[0] * 0.4) + " " + FormatDouble(palette[1]) + " " + FormatDouble(palette[2]) + " "
                + FormatDouble(pulseAmount));
            builder.AppendLine("objectState sideTower "
                + FormatDouble(palette[2]) + " " + FormatDouble(palette[0]) + " " + FormatDouble(palette[1]) + " "
                + FormatDouble(pulseAmount));
            builder.AppendLine("objectState smallSphere "
                + FormatDouble(palette[1]) + " " + FormatDouble(palette[2]) + " " + FormatDouble(palette[0]) + " "
                + FormatDouble(pulseAmount));
            builder.AppendLine("labelText counter C#~ticks:~" + tickCount.ToString(CultureInfo.InvariantCulture) + "~(v2)");
            builder.AppendLine("labelText status 方向~" + (rotationDirection > 0 ? "正向" : "反向")
                + "~速度~x" + speedMultiplier.ToString("0.0", CultureInfo.InvariantCulture)
                + "~配色~" + (paletteIndex + 1).ToString(CultureInfo.InvariantCulture)
                + "~事件~" + consumedEventCount.ToString(CultureInfo.InvariantCulture));
            System.IO.File.WriteAllText("/ui-state.txt", builder.ToString());
        }

        private static string FormatDouble(double value)
        {
            return value.ToString("0.####", CultureInfo.InvariantCulture);
        }
    }
}
