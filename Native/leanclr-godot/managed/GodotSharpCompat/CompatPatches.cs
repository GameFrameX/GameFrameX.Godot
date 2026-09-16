using System;
using System.Runtime.CompilerServices;

namespace Godot
{
    /// <summary>
    /// 引擎内建单例静态指针通道（0=Engine, 1=OS, 2=ProjectSettings）。
    /// </summary>
    internal static partial class NativeCalls
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern IntPtr GodotStaticGetSingletonPtr(int which);
    }

    /// <summary>
    /// Engine 静态外观：对齐官方 GodotSharp 的静态 API 形态（Godot 侧 Engine 为单例静态类）。
    /// </summary>
    public partial class Engine
    {
        private static Engine Singleton
        {
            get { return CreateFromNative<Engine>(NativeCalls.GodotStaticGetSingletonPtr(0)); }
        }

        public static float TimeScale
        {
            get { return NativeCalls.GodotEngineGetTimeScale(Singleton.NativePtr); }
            set { NativeCalls.GodotEngineSetTimeScale(Singleton.NativePtr, value); }
        }

        public static int MaxFps
        {
            get { return NativeCalls.GodotEngineGetMaxFps(Singleton.NativePtr); }
            set { NativeCalls.GodotEngineSetMaxFps(Singleton.NativePtr, value); }
        }

        public static MainLoop GetMainLoop()
        {
            return Singleton.GetMainLoopInstance();
        }

        public static Dictionary GetVersionInfo()
        {
            return Singleton.GetVersionInfoInstance();
        }
    }

    /// <summary>
    /// OS 静态外观。
    /// </summary>
    public partial class OS
    {
        private static OS Singleton
        {
            get { return CreateFromNative<OS>(NativeCalls.GodotStaticGetSingletonPtr(1)); }
        }

        public static bool HasFeature(string tagName)
        {
            return NativeCalls.GodotOSHasFeature(Singleton.NativePtr, tagName);
        }
    }

    /// <summary>
    /// ProjectSettings 静态外观。
    /// </summary>
    public partial class ProjectSettings
    {
        private static ProjectSettings Singleton
        {
            get { return CreateFromNative<ProjectSettings>(NativeCalls.GodotStaticGetSingletonPtr(2)); }
        }

        public static bool HasSetting(string name)
        {
            return NativeCalls.GodotProjectSettingsHasSetting(Singleton.NativePtr, name);
        }

        public static Variant GetSetting(string name)
        {
            return NativeCalls.GodotProjectSettingsGetSetting(Singleton.NativePtr, name);
        }

        public static string GlobalizePath(string path)
        {
            return NativeCalls.GodotProjectSettingsGlobalizePath(Singleton.NativePtr, path);
        }
    }

    /// <summary>
    /// Node3D 变换属性（生成器以 Get/Set 方法对生成，此处包装为属性以对齐官方 API）。
    /// </summary>
    public partial class Node3D
    {
        public Vector3 Position
        {
            get { return GetPosition(); }
            set { SetPosition(value); }
        }

        public Vector3 GlobalPosition
        {
            get { return GetGlobalPosition(); }
            set { SetGlobalPosition(value); }
        }

        public Vector3 Scale
        {
            get { return GetScale(); }
            set { SetScale(value); }
        }

        public Vector3 Rotation
        {
            get { return GetRotation(); }
            set { SetRotation(value); }
        }

        public Vector3 RotationDegrees
        {
            get { return GetRotationDegrees(); }
            set { SetRotationDegrees(value); }
        }
    }

    /// <summary>
    /// SceneTree 补充属性。
    /// </summary>
    public partial class SceneTree
    {
        public Window Root
        {
            get { return CreateFromNative<Window>(NativeCalls.GodotSceneTreeGetRoot(NativePtr)); }
        }
    }

    /// <summary>
    /// GD 补充静态输出 API。
    /// </summary>
    public static partial class GD
    {
        public static void PrintErr(string message)
        {
            Print(message);
        }
    }

    /// <summary>
    /// 单精度数学库：对齐官方 GodotSharp 的 Mathf（netfx BCL 无 MathF，由 double Math 降精度实现）。
    /// </summary>
    public static partial class Mathf
    {
        public const float PI = 3.14159274f;
        public const float Tau = 6.28318548f;
        public const float Epsilon = 1.40129846E-45f;

        public static float Abs(float value) { return Math.Abs(value); }
        public static int Abs(int value) { return Math.Abs(value); }
        public static float Sign(float value) { return value == 0f ? 0f : (value > 0f ? 1f : -1f); }
        public static int Sign(int value) { return value == 0 ? 0 : (value > 0 ? 1 : -1); }
        public static float Min(float a, float b) { return a < b ? a : b; }
        public static float Max(float a, float b) { return a > b ? a : b; }
        public static int Min(int a, int b) { return a < b ? a : b; }
        public static int Max(int a, int b) { return a > b ? a : b; }
        public static float Clamp(float value, float min, float max) { return value < min ? min : (value > max ? max : value); }
        public static int Clamp(int value, int min, int max) { return value < min ? min : (value > max ? max : value); }
        public static float Lerp(float from, float to, float weight) { return from + (to - from) * weight; }
        public static float InverseLerp(float from, float to, float value) { return (from != to) ? (value - from) / (to - from) : 0f; }
        public static float Remap(float value, float inFrom, float inTo, float outFrom, float outTo) { return Lerp(outFrom, outTo, InverseLerp(inFrom, inTo, value)); }
        public static float Sin(float value) { return (float)Math.Sin(value); }
        public static float Cos(float value) { return (float)Math.Cos(value); }
        public static float Tan(float value) { return (float)Math.Tan(value); }
        public static float Asin(float value) { return (float)Math.Asin(value); }
        public static float Acos(float value) { return (float)Math.Acos(value); }
        public static float Atan(float value) { return (float)Math.Atan(value); }
        public static float Atan2(float y, float x) { return (float)Math.Atan2(y, x); }
        public static float Sqrt(float value) { return (float)Math.Sqrt(value); }
        public static float Pow(float x, float y) { return (float)Math.Pow(x, y); }
        public static float Exp(float value) { return (float)Math.Exp(value); }
        public static float Log(float value) { return (float)Math.Log(value); }
        public static float Log10(float value) { return (float)Math.Log10(value); }
        public static float Floor(float value) { return (float)Math.Floor(value); }
        public static float Ceil(float value) { return (float)Math.Ceiling(value); }
        public static float Round(float value) { return (float)Math.Round(value); }
        public static int FloorToInt(float value) { return (int)Math.Floor(value); }
        public static int CeilToInt(float value) { return (int)Math.Ceiling(value); }
        public static int RoundToInt(float value) { return (int)Math.Round(value); }
        public static float DegToRad(float deg) { return deg * ((float)Math.PI / 180f); }
        public static float RadToDeg(float rad) { return rad * (180f / (float)Math.PI); }
        public static bool IsEqualApprox(float a, float b) { return Math.Abs(a - b) < 1E-05f * Max(Math.Abs(a), Math.Abs(b)); }
        public static bool IsZeroApprox(float value) { return Math.Abs(value) < 1E-05f; }
        public static float Snapped(float value, float step) { return step != 0f ? (float)Math.Round(value / step) * step : value; }
    }
}
