using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using GameFrameX.Runtime;
using Godot;

namespace Godot.Startup.Hotfix;

internal static class HotfixTypeResolver
{
    private const string HotfixAssemblyName = "Hotfix";
    private static Assembly s_HotfixAssembly;
    private static bool s_LoadAttemptLogged;
    private static bool s_TypeLoadFailureLogged;
    private static readonly HashSet<string> s_TryGetTypeFailureLogged = new(StringComparer.Ordinal);

    internal static Type ResolveOrNull(string typeFullName)
    {
        if (string.IsNullOrWhiteSpace(typeFullName))
        {
            return null;
        }

        EnsureAssemblyLoaded();
        var type = ResolveTypeFromLoadedAssemblies(typeFullName);
        if (type != null)
        {
            return type;
        }

        foreach (var fallbackTypeName in GetFallbackTypeNames(typeFullName))
        {
            type = ResolveTypeFromLoadedAssemblies(fallbackTypeName);
            if (type != null)
            {
                Log.Warning("[HotfixResolver] fallback type matched: request={0}, fallback={1}", typeFullName, fallbackTypeName);
                return type;
            }
        }

        type = ResolveTypeByShortName(typeFullName);
        if (type != null)
        {
            Log.Warning("[HotfixResolver] short-name fallback matched: request={0}, matched={1}", typeFullName, type.FullName);
            return type;
        }

        if (!s_LoadAttemptLogged)
        {
            s_LoadAttemptLogged = true;
            Log.Warning("[HotfixResolver] type not found: {0}. loadedHotfixAssembly={1}", typeFullName, GetAssemblyLocationSafe(s_HotfixAssembly));
            Log.Warning(
                "[HotfixResolver] dependency probe: GDGUI={0}, FGUI={1}, OptionUIGroup={2}, OptionUIConfig={3}",
                Type.GetType("GameFrameX.UI.GDGUI.Runtime.GDGUI, Godot", false) != null,
                Type.GetType("GameFrameX.UI.FairyGUI.Runtime.FGUI, Godot", false) != null,
                Type.GetType("GameFrameX.UI.Runtime.OptionUIGroupAttribute, Godot", false) != null,
                Type.GetType("GameFrameX.UI.Runtime.OptionUIConfigAttribute, Godot", false) != null
            );
            var available = GetTypesSafe(s_HotfixAssembly)
                .Select(static m => m.FullName)
                .Where(static m => !string.IsNullOrWhiteSpace(m) && m.StartsWith("Godot.Hotfix.", StringComparison.Ordinal))
                .Take(24)
                .ToArray();
            Log.Warning("[HotfixResolver] loaded hotfix type samples({0}): {1}", available.Length, string.Join(", ", available));
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies().Where(static m => string.Equals(m.GetName().Name, HotfixAssemblyName, StringComparison.Ordinal)))
            {
                Log.Warning("[HotfixResolver] loaded hotfix candidate: {0}", GetAssemblyLocationSafe(asm));
            }
        }

        return type;
    }

    internal static bool TrySubscribeEvent(object target, string eventName, Delegate handler)
    {
        return TryBindEvent(target, eventName, handler, true);
    }

    internal static bool TryUnsubscribeEvent(object target, string eventName, Delegate handler)
    {
        return TryBindEvent(target, eventName, handler, false);
    }

    internal static bool TryInvokeMethod(object target, string methodName, params object[] args)
    {
        if (target == null || string.IsNullOrWhiteSpace(methodName))
        {
            return false;
        }

        try
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method == null)
            {
                return false;
            }

            method.Invoke(target, args);
            return true;
        }
        catch (Exception exception)
        {
            Log.Warning("[HotfixResolver] invoke method failed. type={0}, method={1}, exception={2}", target.GetType().FullName, methodName, exception.Message);
            return false;
        }
    }

    private static bool TryBindEvent(object target, string eventName, Delegate handler, bool subscribe)
    {
        if (target == null || handler == null || string.IsNullOrWhiteSpace(eventName))
        {
            return false;
        }

        try
        {
            var eventInfo = target.GetType().GetEvent(eventName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (eventInfo == null)
            {
                return false;
            }

            if (subscribe)
            {
                eventInfo.AddEventHandler(target, handler);
            }
            else
            {
                eventInfo.RemoveEventHandler(target, handler);
            }

            return true;
        }
        catch (Exception exception)
        {
            var action = subscribe ? "subscribe" : "unsubscribe";
            Log.Warning("[HotfixResolver] {0} event failed. type={1}, event={2}, exception={3}", action, target.GetType().FullName, eventName, exception.Message);
            return false;
        }
    }

    private static void EnsureAssemblyLoaded()
    {
        if (s_HotfixAssembly != null)
        {
            return;
        }

        var runtimeContext = GetRuntimeLoadContext();
        s_HotfixAssembly = runtimeContext.Assemblies
            .FirstOrDefault(static m => string.Equals(m.GetName().Name, HotfixAssemblyName, StringComparison.Ordinal));
        if (s_HotfixAssembly != null)
        {
            return;
        }

        s_HotfixAssembly = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(static m => string.Equals(m.GetName().Name, HotfixAssemblyName, StringComparison.Ordinal));
        if (s_HotfixAssembly != null)
        {
            return;
        }

        try
        {
            s_HotfixAssembly = Assembly.Load(new AssemblyName(HotfixAssemblyName));
        }
        catch
        {
            s_HotfixAssembly = null;
        }

        if (s_HotfixAssembly != null)
        {
            return;
        }

        // 优先按已知路径加载，避免被探测到旧版本同名程序集。
        s_HotfixAssembly = TryLoadFromKnownPaths();
        if (s_HotfixAssembly != null)
        {
            return;
        }

        try
        {
            s_HotfixAssembly = runtimeContext.LoadFromAssemblyName(new AssemblyName(HotfixAssemblyName));
        }
        catch
        {
            s_HotfixAssembly = null;
        }
    }

    private static Type ResolveTypeFromLoadedAssemblies(string typeFullName)
    {
        var type = TryGetTypeSafe(s_HotfixAssembly, typeFullName);
        if (type != null)
        {
            return type;
        }

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (ReferenceEquals(assembly, s_HotfixAssembly))
            {
                continue;
            }

            type = TryGetTypeSafe(assembly, typeFullName);
            if (type != null)
            {
                return type;
            }
        }

        return Type.GetType($"{typeFullName}, {HotfixAssemblyName}", throwOnError: false);
    }

    private static Type ResolveTypeByShortName(string typeFullName)
    {
        var shortNameIndex = typeFullName.LastIndexOf('.');
        if (shortNameIndex < 0 || shortNameIndex >= typeFullName.Length - 1)
        {
            return null;
        }

        var shortName = typeFullName[(shortNameIndex + 1)..];
        var preferPrefix = GetPreferredPrefix(typeFullName);
        var matches = new List<Type>();
        foreach (var assembly in EnumerateCandidateAssemblies())
        {
            foreach (var type in GetTypesSafe(assembly))
            {
                if (!string.Equals(type.Name, shortName, StringComparison.Ordinal))
                {
                    continue;
                }

                var fullName = type.FullName;
                if (!string.IsNullOrWhiteSpace(preferPrefix) &&
                    (string.IsNullOrWhiteSpace(fullName) || !fullName.StartsWith(preferPrefix, StringComparison.Ordinal)))
                {
                    continue;
                }

                matches.Add(type);
            }
        }

        return matches.Count == 1 ? matches[0] : null;
    }

    private static IEnumerable<Assembly> EnumerateCandidateAssemblies()
    {
        if (s_HotfixAssembly != null)
        {
            yield return s_HotfixAssembly;
        }

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (ReferenceEquals(assembly, s_HotfixAssembly))
            {
                continue;
            }

            yield return assembly;
        }
    }

    private static Type TryGetTypeSafe(Assembly assembly, string typeFullName)
    {
        if (assembly == null)
        {
            return null;
        }

        try
        {
            return assembly.GetType(typeFullName, throwOnError: false);
        }
        catch (ReflectionTypeLoadException exception)
        {
            LogTryGetTypeFailure(typeFullName, exception);
            return null;
        }
        catch (Exception exception)
        {
            LogTryGetTypeFailure(typeFullName, exception);
            return null;
        }
    }

    private static void LogTryGetTypeFailure(string typeFullName, Exception exception)
    {
        if (!s_TryGetTypeFailureLogged.Add(typeFullName))
        {
            return;
        }

        Log.Warning("[HotfixResolver] TryGetType failed. type={0}, exception={1}: {2}", typeFullName, exception.GetType().Name, exception.Message);
        if (exception is ReflectionTypeLoadException typeLoadException)
        {
            var details = typeLoadException.LoaderExceptions?
                .Where(static m => m != null)
                .Select(static m => m.Message)
                .Distinct(StringComparer.Ordinal)
                .Take(8)
                .ToArray();
            if (details is { Length: > 0 })
            {
                Log.Warning("[HotfixResolver] TryGetType loader exceptions: {0}", string.Join(" | ", details));
            }
        }
    }

    private static IEnumerable<Type> GetTypesSafe(Assembly assembly)
    {
        if (assembly == null)
        {
            yield break;
        }

        Type[] types;
        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException exception)
        {
            if (!s_TypeLoadFailureLogged)
            {
                s_TypeLoadFailureLogged = true;
                var details = exception.LoaderExceptions?
                    .Where(static m => m != null)
                    .Select(static m => m.Message)
                    .Distinct(StringComparer.Ordinal)
                    .Take(8)
                    .ToArray();
                if (details is { Length: > 0 })
                {
                    Log.Warning("[HotfixResolver] GetTypes loader exceptions: {0}", string.Join(" | ", details));
                }
            }

            types = exception.Types;
        }
        catch
        {
            yield break;
        }

        foreach (var type in types)
        {
            if (type != null)
            {
                yield return type;
            }
        }
    }

    private static string GetPreferredPrefix(string typeFullName)
    {
        if (typeFullName.StartsWith("Godot.Hotfix.GodotGUI.", StringComparison.Ordinal))
        {
            return "Godot.Hotfix.GodotGUI.";
        }

        if (typeFullName.StartsWith("Godot.Hotfix.FairyGUI.", StringComparison.Ordinal))
        {
            return "Godot.Hotfix.FairyGUI.";
        }

        return string.Empty;
    }

    private static Assembly TryLoadFromKnownPaths()
    {
        var runtimeContext = GetRuntimeLoadContext();
        foreach (var candidate in GetCandidateAssemblyPaths())
        {
            try
            {
                if (!File.Exists(candidate))
                {
                    continue;
                }

                var loaded = runtimeContext.LoadFromAssemblyPath(candidate);
                Log.Info("[HotfixResolver] loaded assembly: {0}", candidate);
                return loaded;
            }
            catch (Exception exception)
            {
                Log.Warning("[HotfixResolver] load assembly failed. path={0}, exception={1}", candidate, exception.Message);
            }
        }

        return null;
    }

    private static AssemblyLoadContext GetRuntimeLoadContext()
    {
        return AssemblyLoadContext.GetLoadContext(typeof(HotfixTypeResolver).Assembly) ?? AssemblyLoadContext.Default;
    }

    private static string[] GetCandidateAssemblyPaths()
    {
        return
        [
            ProjectSettings.GlobalizePath("res://BuildArtifacts/Assemblies/Debug/Hotfix.dll"),
            ProjectSettings.GlobalizePath("res://BuildArtifacts/Assemblies/Release/Hotfix.dll"),
            ProjectSettings.GlobalizePath("res://BuildArtifacts/Assemblies/ExportDebug/Hotfix.dll"),
            ProjectSettings.GlobalizePath("res://BuildArtifacts/Assemblies/ExportRelease/Hotfix.dll"),
            Path.Combine(AppContext.BaseDirectory, "Hotfix.dll"),
            ProjectSettings.GlobalizePath("res://.godot/mono/temp/bin/Debug/Hotfix.dll"),
            ProjectSettings.GlobalizePath("res://.godot/mono/temp/bin/Release/Hotfix.dll"),
            ProjectSettings.GlobalizePath("res://.godot/mono/temp/bin/ExportDebug/Hotfix.dll"),
            ProjectSettings.GlobalizePath("res://.godot/mono/temp/bin/ExportRelease/Hotfix.dll")
        ];
    }

    private static IEnumerable<string> GetFallbackTypeNames(string typeFullName)
    {
        if (typeFullName.StartsWith("Godot.Hotfix.GodotGUI.", StringComparison.Ordinal))
        {
            var shortName = typeFullName["Godot.Hotfix.GodotGUI.".Length..];
            yield return $"Godot.Startup.Demo.GodotUI.{shortName}";
        }

        if (typeFullName.StartsWith("Godot.Hotfix.FairyGUI.", StringComparison.Ordinal))
        {
            var shortName = typeFullName["Godot.Hotfix.FairyGUI.".Length..];
            yield return $"Godot.Startup.Demo.FairyGUI.{shortName}";
        }
    }

    private static string GetAssemblyLocationSafe(Assembly assembly)
    {
        if (assembly == null)
        {
            return "<null>";
        }

        try
        {
            return string.IsNullOrWhiteSpace(assembly.Location) ? assembly.FullName : assembly.Location;
        }
        catch
        {
            return assembly.FullName;
        }
    }
}
