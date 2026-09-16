using System;

namespace Godot
{
    /// <summary>
    /// Engine 静态外观补充（第二波）。
    /// </summary>
    public partial class Engine
    {
        public static bool IsEditorHint()
        {
            return NativeCalls.GodotEngineIsEditorHint(Singleton.NativePtr);
        }
    }

    /// <summary>
    /// OS 静态外观补充（第二波）。
    /// </summary>
    public partial class OS
    {
        public static Error ShellOpen(string uri)
        {
            return (Error)NativeCalls.GodotOSShellOpen(Singleton.NativePtr, uri);
        }

        public static string GetName()
        {
            return NativeCalls.GodotOSGetName(Singleton.NativePtr);
        }

        public static string GetExecutablePath()
        {
            return NativeCalls.GodotOSGetExecutablePath(Singleton.NativePtr);
        }

        public static bool IsDebugBuild()
        {
            return NativeCalls.GodotOSIsDebugBuild(Singleton.NativePtr);
        }
    }

    /// <summary>
    /// Node 补充：带递归/所有权参数的 FindChild 重载（对齐官方 API；生成器只生成了单参数版本）。
    /// 手写递归匹配，忽略 owned 差异（运行时查找语义等价于 owned:false）。
    /// </summary>
    public partial class Node
    {
        public Node FindChild(string pattern, bool recursive, bool owned)
        {
            return FindChildRecursive(this, pattern, recursive);
        }

        private static Node FindChildRecursive(Node current, string pattern, bool recursive)
        {
            GodotArray children = current.GetChildren();
            for (int i = 0; i < children.Count; ++i)
            {
                Node child = children[i].AsObject<Node>();
                if (child == null)
                {
                    continue;
                }

                if (MatchesGlob(child.Name, pattern))
                {
                    return child;
                }

                if (recursive)
                {
                    Node found = FindChildRecursive(child, pattern, recursive);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }

            return null;
        }

        private static bool MatchesGlob(string value, string pattern)
        {
            if (value == null || pattern == null)
            {
                return false;
            }

            return MatchesGlobCore(value, 0, pattern, 0);
        }

        private static bool MatchesGlobCore(string value, int valueIndex, string pattern, int patternIndex)
        {
            while (patternIndex < pattern.Length)
            {
                char p = pattern[patternIndex];
                if (p == '*')
                {
                    for (int next = valueIndex; next <= value.Length; ++next)
                    {
                        if (MatchesGlobCore(value, next, pattern, patternIndex + 1))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                if (p == '?')
                {
                    if (valueIndex >= value.Length)
                    {
                        return false;
                    }

                    valueIndex++;
                    patternIndex++;
                    continue;
                }

                if (valueIndex >= value.Length || value[valueIndex] != p)
                {
                    return false;
                }

                valueIndex++;
                patternIndex++;
            }

            return valueIndex == value.Length;
        }
    }

    /// <summary>
    /// CanvasItem / Node2D 属性补充。
    /// </summary>
    public partial class CanvasItem
    {
        public int ZIndex
        {
            get { return NativeCalls.GodotCanvasItemGetZIndex(NativePtr); }
            set { NativeCalls.GodotCanvasItemSetZIndex(NativePtr, value); }
        }
    }

    public partial class Node2D
    {
        public float RotationDegrees
        {
            get { return NativeCalls.GodotNode2DGetRotationDegrees(NativePtr); }
            set { NativeCalls.GodotNode2DSetRotationDegrees(NativePtr, value); }
        }
    }

    /// <summary>
    /// Vector2 方法补充（纯 C# 数值运算）。
    /// </summary>
    public partial struct Vector2
    {
        public float Length()
        {
            return (float)Math.Sqrt(X * X + Y * Y);
        }

        public float LengthSquared()
        {
            return X * X + Y * Y;
        }

        public Vector2 Normalized()
        {
            float length = Length();
            if (length == 0f)
            {
                return Zero;
            }

            return new Vector2(X / length, Y / length);
        }

        public float Dot(Vector2 with)
        {
            return X * with.X + Y * with.Y;
        }

        public float DistanceTo(Vector2 to)
        {
            return (this - to).Length();
        }

        public float DistanceSquaredTo(Vector2 to)
        {
            return (this - to).LengthSquared();
        }

        public float AngleTo(Vector2 to)
        {
            return (float)Math.Atan2(Cross(to), Dot(to));
        }

        public float Cross(Vector2 with)
        {
            return X * with.Y - Y * with.X;
        }

        public Vector2 Lerp(Vector2 to, float weight)
        {
            return this + (to - this) * weight;
        }
    }

    /// <summary>
    /// Vector3 方法补充（纯 C# 数值运算）。
    /// </summary>
    public partial struct Vector3
    {
        public float Length()
        {
            return (float)Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        public float LengthSquared()
        {
            return X * X + Y * Y + Z * Z;
        }

        public Vector3 Normalized()
        {
            float length = Length();
            if (length == 0f)
            {
                return Zero;
            }

            return new Vector3(X / length, Y / length, Z / length);
        }

        public float Dot(Vector3 with)
        {
            return X * with.X + Y * with.Y + Z * with.Z;
        }

        public Vector3 Cross(Vector3 with)
        {
            return new Vector3(Y * with.Z - Z * with.Y, Z * with.X - X * with.Z, X * with.Y - Y * with.X);
        }

        public float DistanceTo(Vector3 to)
        {
            return (this - to).Length();
        }

        public float AngleTo(Vector3 to)
        {
            return (float)Math.Acos(Math.Min(1.0, Dot(to) / (Length() * to.Length())));
        }

        public Vector3 Lerp(Vector3 to, float weight)
        {
            return this + (to - this) * weight;
        }
    }

    /// <summary>
    /// Rect2I 方法补充。
    /// </summary>
    public partial struct Rect2I
    {
        public bool Intersects(Rect2I b)
        {
            return Position.X < b.Position.X + b.Size.X &&
                   b.Position.X < Position.X + Size.X &&
                   Position.Y < b.Position.Y + b.Size.Y &&
                   b.Position.Y < Position.Y + Size.Y;
        }
    }
}

namespace Godot
{
    /// <summary>
    /// Vector 运算符补充（生成 struct 未包含运算符重载）。
    /// </summary>
    public partial struct Vector2
    {
        public static Vector2 operator +(Vector2 a, Vector2 b) { return new Vector2(a.X + b.X, a.Y + b.Y); }
        public static Vector2 operator -(Vector2 a, Vector2 b) { return new Vector2(a.X - b.X, a.Y - b.Y); }
        public static Vector2 operator -(Vector2 a) { return new Vector2(-a.X, -a.Y); }
        public static Vector2 operator *(Vector2 a, float d) { return new Vector2(a.X * d, a.Y * d); }
        public static Vector2 operator *(float d, Vector2 a) { return new Vector2(a.X * d, a.Y * d); }
        public static Vector2 operator /(Vector2 a, float d) { return new Vector2(a.X / d, a.Y / d); }
        public static bool operator ==(Vector2 a, Vector2 b) { return a.X == b.X && a.Y == b.Y; }
        public static bool operator !=(Vector2 a, Vector2 b) { return !(a == b); }
    }

    public partial struct Vector3
    {
        public static Vector3 operator +(Vector3 a, Vector3 b) { return new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z); }
        public static Vector3 operator -(Vector3 a, Vector3 b) { return new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z); }
        public static Vector3 operator -(Vector3 a) { return new Vector3(-a.X, -a.Y, -a.Z); }
        public static Vector3 operator *(Vector3 a, float d) { return new Vector3(a.X * d, a.Y * d, a.Z * d); }
        public static Vector3 operator *(float d, Vector3 a) { return new Vector3(a.X * d, a.Y * d, a.Z * d); }
        public static Vector3 operator /(Vector3 a, float d) { return new Vector3(a.X / d, a.Y / d, a.Z / d); }
        public static bool operator ==(Vector3 a, Vector3 b) { return a.X == b.X && a.Y == b.Y && a.Z == b.Z; }
        public static bool operator !=(Vector3 a, Vector3 b) { return !(a == b); }
    }
}

namespace Godot
{
    /// <summary>
    /// 通知常量补充（生成器未输出 constants；值取自 extension_api.json Godot 4.5）。
    /// </summary>
    public partial class GodotObject
    {
        public const int NotificationPredelete = 1;
    }

    public partial class Node
    {
        public const int NotificationEnterTree = 10;
        public const int NotificationExitTree = 11;
        public const int NotificationReady = 13;
        public const int NotificationPaused = 14;
        public const int NotificationUnpaused = 15;
        public const int NotificationWMCloseRequest = 1006;
        public const int NotificationWMGoBackRequest = 1007;
        public const int NotificationApplicationResumed = 2014;
        public const int NotificationApplicationPaused = 2015;
        public const int NotificationApplicationFocusIn = 2016;
        public const int NotificationApplicationFocusOut = 2017;
    }

    public static partial class GD
    {
        public static void PushWarning(string message)
        {
            Print("[WARNING] " + message);
        }
    }

    public partial struct Variant
    {
        public static implicit operator Node(Variant variant)
        {
            return variant.AsObject<Node>();
        }
    }
}

namespace Godot
{
    public partial class Mathf
    {
        public const float Pi = PI;
        public const float Tau2 = Tau;
    }

    public partial class CanvasItemMaterial
    {
        /// <summary>官方 GodotSharp 命名别名（生成器输出 BlendMode）。</summary>
        public enum BlendModeEnum
        {
            Mix = BlendMode.Mix,
            Add = BlendMode.Add,
            Sub = BlendMode.Sub,
            Mul = BlendMode.Mul,
            PremultAlpha = BlendMode.PremultAlpha,
        }
    }

    public partial class Window
    {
        /// <summary>官方 GodotSharp 命名别名（生成器输出 ContentScaleAspect）。</summary>
        public enum ContentScaleAspectEnum
        {
            Ignore = ContentScaleAspect.Ignore,
            Keep = ContentScaleAspect.Keep,
            KeepWidth = ContentScaleAspect.KeepWidth,
            KeepHeight = ContentScaleAspect.KeepHeight,
            Expand = ContentScaleAspect.Expand,
        }
    }
}

namespace Godot.Collections
{
    /// <summary>
    /// 强类型泛型数组（对齐官方 Godot.Collections.Array&lt;T&gt;；底层包装非泛型 GodotArray）。
    /// </summary>
    public class Array<T> : global::Godot.Array
    {
        public new T this[int index]
        {
            get { return ConvertTo(base[index]); }
            set { base[index] = ConvertFrom(value); }
        }

        public void Add(T item)
        {
            base.Add(ConvertFrom(item));
        }

        private static T ConvertTo(Variant variant)
        {
            if (typeof(T) == typeof(bool)) { return (T)(object)variant.AsBool(); }
            if (typeof(T) == typeof(int)) { return (T)(object)(int)variant.AsInt64(); }
            if (typeof(T) == typeof(long)) { return (T)(object)variant.AsInt64(); }
            if (typeof(T) == typeof(float)) { return (T)(object)(float)variant.AsDouble(); }
            if (typeof(T) == typeof(double)) { return (T)(object)variant.AsDouble(); }
            if (typeof(T) == typeof(string)) { return (T)(object)variant.AsString(); }
            if (typeof(T).IsSubclassOf(typeof(GodotObject))) { return (T)(object)variant.AsObject<GodotObject>(); }
            return default(T);
        }

        private static Variant ConvertFrom(T item)
        {
            if (item is GodotObject godotObject) { return new Variant(godotObject); }
            if (item is string text) { return new Variant(text); }
            if (item is bool boolean) { return new Variant(boolean); }
            if (item is int integer) { return new Variant(integer); }
            if (item is long int64) { return new Variant(int64); }
            if (item is float single) { return new Variant(single); }
            if (item is double doublePrecision) { return new Variant(doublePrecision); }
            return new Variant();
        }
    }
}

namespace Godot
{
    /// <summary>Time 静态外观。</summary>
    public partial class Time
    {
        private static Time Singleton
        {
            get { return CreateFromNative<Time>(NativeCalls.GodotStaticGetSingletonPtr(3)); }
        }

        public static long GetTicksMsec()
        {
            return NativeCalls.GodotTimeGetTicksMsec(Singleton.NativePtr);
        }
    }

    /// <summary>ResourceLoader 静态外观。</summary>
    public partial class ResourceLoader
    {
        private static ResourceLoader Singleton
        {
            get { return CreateFromNative<ResourceLoader>(NativeCalls.GodotStaticGetSingletonPtr(6)); }
        }

        public static bool Exists(string path)
        {
            return NativeCalls.GodotResourceLoaderExists(Singleton.NativePtr, path);
        }

        public static Resource Load(string path)
        {
            return CreateFromNative<Resource>(NativeCalls.GodotResourceLoaderLoad(Singleton.NativePtr, path));
        }
    }

    /// <summary>Input 静态外观。</summary>
    public partial class Input
    {
        private static Input Singleton
        {
            get { return CreateFromNative<Input>(NativeCalls.GodotStaticGetSingletonPtr(4)); }
        }

        public static bool IsKeyPressed(Key key)
        {
            return NativeCalls.GodotInputIsKeyPressed(Singleton.NativePtr, (int)key);
        }
    }

    /// <summary>DisplayServer 静态外观。</summary>
    public partial class DisplayServer
    {
        private static DisplayServer Singleton
        {
            get { return CreateFromNative<DisplayServer>(NativeCalls.GodotStaticGetSingletonPtr(5)); }
        }

        public static Vector2I WindowGetSize()
        {
            return NativeCalls.GodotDisplayServerWindowGetSize(Singleton.NativePtr);
        }

        public static void WindowSetImeActive(bool active)
        {
            NativeCalls.GodotDisplayServerWindowSetImeActive(Singleton.NativePtr, active);
        }
    }

    /// <summary>Engine 静态外观补充（第三波）。</summary>
    public partial class Engine
    {
        public static int GetFramesDrawn()
        {
            return NativeCalls.GodotEngineGetFramesDrawn(Singleton.NativePtr);
        }

        public static long GetProcessFrames()
        {
            return NativeCalls.GodotEngineGetProcessFrames(Singleton.NativePtr);
        }

        public static float GetFramesPerSecond()
        {
            return NativeCalls.GodotEngineGetFramesPerSecond(Singleton.NativePtr);
        }
    }

    /// <summary>ProjectSettings 静态外观补充。</summary>
    public partial class ProjectSettings
    {
        public static string LocalizePath(string path)
        {
            return NativeCalls.GodotProjectSettingsLocalizePath(Singleton.NativePtr, path);
        }
    }

    public static partial class GD
    {
        public static void PushError(string message)
        {
            Print("[ERROR] " + message);
        }
    }

    /// <summary>预定义颜色集合（官方 GodotSharp 的 Colors 静态类，常用子集）。</summary>
    public static class Colors
    {
        public static readonly Color White = new Color(1f, 1f, 1f, 1f);
        public static readonly Color Black = new Color(0f, 0f, 0f, 1f);
        public static readonly Color Transparent = new Color(0f, 0f, 0f, 0f);
        public static readonly Color Red = new Color(1f, 0f, 0f, 1f);
        public static readonly Color Green = new Color(0f, 1f, 0f, 1f);
        public static readonly Color Blue = new Color(0f, 0f, 1f, 1f);
        public static readonly Color Yellow = new Color(1f, 1f, 0f, 1f);
        public static readonly Color Cyan = new Color(0f, 1f, 1f, 1f);
        public static readonly Color Magenta = new Color(1f, 0f, 1f, 1f);
        public static readonly Color Gray = new Color(0.5f, 0.5f, 0.5f, 1f);
        public static readonly Color Grey = Gray;
        public static readonly Color Orange = new Color(1f, 0.5f, 0f, 1f);
        public static readonly Color Purple = new Color(0.5f, 0f, 0.5f, 1f);
        public static readonly Color Pink = new Color(1f, 0.75f, 0.8f, 1f);
        public static readonly Color LightGray = new Color(0.827f, 0.827f, 0.827f, 1f);
        public static readonly Color DarkGray = new Color(0.25f, 0.25f, 0.25f, 1f);
    }

    /// <summary>Vector 索引器与比较补充。</summary>
    public partial struct Vector2
    {
        public float this[int index]
        {
            get { return index == 0 ? X : Y; }
            set
            {
                if (index == 0) { X = value; }
                else { Y = value; }
            }
        }

        public bool IsEqualApprox(Vector2 to)
        {
            return Mathf.IsEqualApprox(X, to.X) && Mathf.IsEqualApprox(Y, to.Y);
        }
    }

    public partial struct Vector3
    {
        public float this[int index]
        {
            get { return index == 0 ? X : (index == 1 ? Y : Z); }
            set
            {
                if (index == 0) { X = value; }
                else if (index == 1) { Y = value; }
                else { Z = value; }
            }
        }

        public bool IsEqualApprox(Vector3 to)
        {
            return Mathf.IsEqualApprox(X, to.X) && Mathf.IsEqualApprox(Y, to.Y) && Mathf.IsEqualApprox(Z, to.Z);
        }
    }
}

namespace Godot
{
    public partial class GodotObject
    {
        /// <summary>实例有效性静态检查（经 GetInstanceId 非 0 判定）。</summary>
        public static bool IsInstanceValid(GodotObject instance)
        {
            if (instance == null || instance.NativePtr == IntPtr.Zero)
            {
                return false;
            }

            return NativeCalls.GodotGodotObjectGetInstanceId(instance.NativePtr) != 0L;
        }
    }

    public partial class OS
    {
        public static string GetUniqueId() { return NativeCalls.GodotOSGetUniqueId(Singleton.NativePtr); }
        public static string GetLocale() { return NativeCalls.GodotOSGetLocale(Singleton.NativePtr); }
    }

    public partial class DisplayServer
    {
        public static string ClipboardGet() { return NativeCalls.GodotDisplayServerClipboardGet(Singleton.NativePtr); }
        public static void ClipboardSet(string clipboard) { NativeCalls.GodotDisplayServerClipboardSet(Singleton.NativePtr, clipboard); }
        public static bool ClipboardHas() { return NativeCalls.GodotDisplayServerClipboardHas(Singleton.NativePtr); }
    }

    public partial class Time
    {
        public static long GetTicksUsec() { return NativeCalls.GodotTimeGetTicksUsec(Singleton.NativePtr); }
        public static double GetUnixTimeFromSystem() { return NativeCalls.GodotTimeGetUnixTimeFromSystem(Singleton.NativePtr); }
    }

    /// <summary>JavaScriptBridge 静态外观（Web 平台）。</summary>
    public partial class JavaScriptBridge
    {
        private static JavaScriptBridge Singleton
        {
            get { return CreateFromNative<JavaScriptBridge>(NativeCalls.GodotStaticGetSingletonPtr(7)); }
        }

        public static Variant Eval(string code)
        {
            return NativeCalls.GodotJavaScriptBridgeEval(Singleton.NativePtr, code);
        }
    }

    public partial class CanvasItem
    {
        public bool Visible
        {
            get { return NativeCalls.GodotCanvasItemIsVisible(NativePtr); }
            set { NativeCalls.GodotCanvasItemSetVisible(NativePtr, value); }
        }
    }

    public partial struct Vector2
    {
        public bool IsZeroApprox() { return Mathf.IsZeroApprox(X) && Mathf.IsZeroApprox(Y); }
    }

    public partial struct Color
    {
        public static bool operator ==(Color a, Color b) { return a.R == b.R && a.G == b.G && a.B == b.B && a.A == b.A; }
        public static bool operator !=(Color a, Color b) { return !(a == b); }
    }

    public partial struct Transform2D
    {
        public static Transform2D operator *(Transform2D a, Transform2D b)
        {
            return new Transform2D(a.X * b.X.X + a.Y * b.X.Y, a.X * b.Y.X + a.Y * b.Y.Y,
                                   a.X * b.Origin.X + a.Y * b.Origin.Y + a.Origin);
        }

        public static Vector2 operator *(Transform2D t, Vector2 v)
        {
            return new Vector2(t.X.X * v.X + t.Y.X * v.Y + t.Origin.X,
                               t.X.Y * v.X + t.Y.Y * v.Y + t.Origin.Y);
        }
    }
}

namespace Godot
{
    using System.Runtime.CompilerServices;

    internal static partial class NativeCalls
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern void GodotSurfaceToolSetUVEx(IntPtr nativePtr, float x, float y);

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern IntPtr GodotSurfaceToolCommitMesh(IntPtr nativePtr, IntPtr meshPtr);
    }

    public partial class SurfaceTool
    {
        /// <summary>官方 GodotSharp 命名（生成器输出 SetUv）。</summary>
        public void SetUV(Vector2 uv)
        {
            NativeCalls.GodotSurfaceToolSetUVEx(NativePtr, uv.X, uv.Y);
        }

        /// <summary>带目标网格的提交（生成器未生成带默认参数重载）。</summary>
        public ArrayMesh Commit(ArrayMesh mesh)
        {
            return CreateFromNative<ArrayMesh>(NativeCalls.GodotSurfaceToolCommitMesh(NativePtr, mesh.NativePtr));
        }
    }
}
