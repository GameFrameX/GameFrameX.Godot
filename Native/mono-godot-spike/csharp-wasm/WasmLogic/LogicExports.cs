using System;
using System.Globalization;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace WasmLogic
{
    // C1 Spike-W3 + 场景化热更：C# 承载场景内容、UI 布局与行为状态，Godot 侧仅做规格驱动宿主。
    // 微信容器缺 WebAssembly.Function，[JSExport] 启动期绑定全部静默失败；
    // 全部方法保持无参 void 签名，由 JS 桥经 mono_wasm_invoke_jsexport 句柄链驱动，数据走 MEMFS 文件：
    //   WriteSceneSpec → /scene-spec.txt   WriteUiSpec → /ui-spec.txt
    //   Tick          → /spike-tick.txt（格式固定，取证判定依赖）+ /ui-state.txt，并消费 /ui-events.txt
    [SupportedOSPlatform("browser")]
    public static partial class LogicExports
    {
        private static int tickCount = 0;
        private static int rotationDirection = 1;
        private static double speedMultiplier = 1.0;
        private static int paletteIndex = 0;
        private static double pulseAmount = 0.0;
        private static int consumedEventCount = 0;
        private static bool stateDirty = true;

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
            var builder = new System.Text.StringBuilder();
            builder.AppendLine("sky 0.22 0.44 0.86 0.55 0.76 0.95 0.16 0.15 0.22");
            builder.AppendLine("ground 0.30 0.40 0.34");
            builder.AppendLine("camera 0.0 1.8 5.4 0.35");
            builder.AppendLine("light -45 -30 1.2");
            builder.AppendLine("object mainCube box 0.0 0.42 0.0 1.30 1.00 0.55 0.15 0.35 1.30 0.55");
            builder.AppendLine("object orbitSphere sphere 1.85 0.55 0.35 0.52 0.20 0.62 0.95 0.15 0.90 -0.35");
            builder.AppendLine("object cornerColumn cylinder -2.05 0.65 -1.25 0.28 1.10 0.34 0.95 0.15 0.40 0.40");
            System.IO.File.WriteAllText("/scene-spec.txt", builder.ToString());
        }

        // UI 规格：按钮与标签（文本内空格以 ~ 占位，宿主还原）
        [JSExport]
        public static void WriteUiSpec()
        {
            var builder = new System.Text.StringBuilder();
            builder.AppendLine("button direction 36 60 40 切换旋转方向");
            builder.AppendLine("button speedup 36 130 40 加速自转");
            builder.AppendLine("button palette 36 200 40 切换配色");
            builder.AppendLine("label status 36 270 36 C#~热更新逻辑驱动中");
            builder.AppendLine("label counter 36 322 30 C#~ticks:~booting");
            System.IO.File.WriteAllText("/ui-spec.txt", builder.ToString());
        }

        // 帧驱动入口：计数落盘（取证格式固定）+ 事件消费 + 状态落盘
        [JSExport]
        public static void Tick()
        {
            tickCount += 1;
            System.IO.File.WriteAllText("/spike-tick.txt", "ticks=" + tickCount.ToString(CultureInfo.InvariantCulture)
                + " at " + DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture));
            ConsumeUiEvents();
            WriteUiState();
            if (pulseAmount > 0.0)
            {
                pulseAmount = Math.Max(0.0, pulseAmount - 0.12);
            }
            stateDirty = false;
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
                var buttonIdentifier = line.Substring("button:".Length);
                ApplyButtonEvent(buttonIdentifier);
            }
            try
            {
                System.IO.File.Delete("/ui-events.txt");
            }
            catch (Exception)
            {
                // 删除失败仅意味着下轮重复消费，事件幂等，可安全忽略
            }
        }

        private static void ApplyButtonEvent(string buttonIdentifier)
        {
            consumedEventCount += 1;
            stateDirty = true;
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

        // 状态落盘：全局运动参数 + 每对象颜色与脉冲 + 标签文本（空格以 ~ 占位）
        private static void WriteUiState()
        {
            var palette = ObjectPalettes[paletteIndex];
            var builder = new System.Text.StringBuilder();
            builder.AppendLine("motion " + FormatDouble(rotationDirection) + " " + FormatDouble(speedMultiplier));
            builder.AppendLine("objectState mainCube "
                + FormatDouble(palette[0]) + " " + FormatDouble(palette[1]) + " " + FormatDouble(palette[2]) + " "
                + FormatDouble(pulseAmount));
            builder.AppendLine("objectState orbitSphere "
                + FormatDouble(1.0 - palette[0] * 0.5) + " " + FormatDouble(palette[1]) + " " + FormatDouble(palette[2]) + " "
                + FormatDouble(pulseAmount));
            builder.AppendLine("objectState cornerColumn "
                + FormatDouble(palette[1]) + " " + FormatDouble(palette[2]) + " " + FormatDouble(palette[0]) + " "
                + FormatDouble(pulseAmount));
            builder.AppendLine("labelText counter C#~ticks:~" + tickCount.ToString(CultureInfo.InvariantCulture) + "~(v1)");
            builder.AppendLine("labelText status 自转方向~" + (rotationDirection > 0 ? "正向" : "反向")
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
