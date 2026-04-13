using System;
using System.Collections.Generic;
using FairyGUI;
using GameFrameX.AssetSystem;
using Godot;

namespace Godot.Hotfix.FairyGUI
{
    internal static class FairyGuiRuntimeBridge
    {
        private const string DefaultBundleRootPath = "res://Assets/Bundles/UI/FGUI";

        private static readonly string[] RequiredPackages =
        {
            "UICommon",
            "UICommonAvatar",
            "UILauncher",
            "UILogin",
            "UIMain"
        };

        private static bool s_Initialized;
        private static readonly Dictionary<string, GComponent> s_GroupLayers = new Dictionary<string, GComponent>(StringComparer.Ordinal);

        internal static void EnsureInitialized()
        {
            if (!s_Initialized)
            {
                _ = Stage.inst;
                s_Initialized = true;
            }

            // Re-run package probing on every call so packages mounted later (for example via pck)
            // can still be added into FairyGUI runtime.
            LoadRequiredPackages(DefaultBundleRootPath);
        }

        internal static bool TryEnsurePackageReady(string packageName, out string error)
        {
            error = string.Empty;
            if (string.IsNullOrWhiteSpace(packageName))
            {
                error = "package name is empty";
                return false;
            }

            EnsureInitialized();
            if (UIPackage.GetByName(packageName) != null)
            {
                return true;
            }

            if (!TryResolvePackagePath(DefaultBundleRootPath, packageName, out var packagePath))
            {
                error = $"package file missing: {packageName}";
                return false;
            }

            var package = UIPackage.AddPackage(packagePath, LoadResourceWithFallback);
            if (package != null)
            {
                return true;
            }

            error = $"add package failed: {packageName}, path={packagePath}";
            return false;
        }

        internal static GComponent CreateFullScreenView(string packageName, string componentName, string uiGroupName = null)
        {
            EnsureInitialized();

            var gObject = UIPackage.CreateObject(packageName, componentName);
            if (gObject == null)
            {
                GD.PushError($"[FGUIBridge] CreateObject failed: {packageName}/{componentName}");
                return null;
            }

            var component = gObject.asCom;
            if (component == null)
            {
                gObject.Dispose();
                GD.PushError($"[FGUIBridge] Object is not GComponent: {packageName}/{componentName}");
                return null;
            }

            component.MakeFullScreen(true);
            var layer = GetOrCreateGroupLayer(uiGroupName);
            layer.AddChild(component);
            return component;
        }

        internal static void DisposeView(ref GComponent component)
        {
            if (component == null)
            {
                return;
            }

            component.Dispose();
            component = null;
        }

        private static void LoadRequiredPackages(string bundleRootPath)
        {
            for (var i = 0; i < RequiredPackages.Length; i++)
            {
                var packageName = RequiredPackages[i];
                if (UIPackage.GetByName(packageName) != null)
                {
                    continue;
                }

                if (!TryResolvePackagePath(bundleRootPath, packageName, out var packagePath))
                {
                    GD.PushWarning($"[FGUIBridge] package file missing. package={packageName}");
                    continue;
                }

                var package = UIPackage.AddPackage(packagePath, LoadResourceWithFallback);
                if (package == null)
                {
                    GD.PushWarning($"[FGUIBridge] add package failed or package file missing: {packagePath}");
                }
            }
        }

        private static bool TryResolvePackagePath(string bundleRootPath, string packageName, out string resolvedPath)
        {
            resolvedPath = string.Empty;
            if (string.IsNullOrWhiteSpace(bundleRootPath) || string.IsNullOrWhiteSpace(packageName))
            {
                return false;
            }

            var candidates = new[]
            {
                $"{bundleRootPath}/{packageName}/{packageName}_fui.bytes",
                $"{bundleRootPath}/{packageName}_fui.bytes"
            };

            for (var i = 0; i < candidates.Length; i++)
            {
                if (FileAccess.FileExists(candidates[i]))
                {
                    resolvedPath = candidates[i];
                    return true;
                }
            }

            return false;
        }

        private static GComponent GetOrCreateGroupLayer(string uiGroupName)
        {
            var groupName = string.IsNullOrWhiteSpace(uiGroupName) ? "Default" : uiGroupName.Trim();
            if (s_GroupLayers.TryGetValue(groupName, out var existing) && existing != null && !existing.isDisposed)
            {
                return existing;
            }

            var layer = new GComponent
            {
                name = $"Group_{groupName}"
            };
            layer.MakeFullScreen(true);
            GRoot.inst.AddChild(layer);
            s_GroupLayers[groupName] = layer;
            return layer;
        }

        private static object LoadResourceWithFallback(string path, Type type, out DestroyMethod destroyMethod)
        {
            destroyMethod = DestroyMethod.Unload;

            if (type == typeof(byte[]))
            {
                return AssetSystemResources.Load<byte[]>(path);
            }

            var resource = TryLoad(path);
            if (resource == null && !string.IsNullOrEmpty(path))
            {
                resource = TryLoad(path + ".png")
                    ?? TryLoad(path + ".jpg")
                    ?? TryLoad(path + ".jpeg")
                    ?? TryLoad(path + ".webp")
                    ?? TryLoad(path + ".bmp")
                    ?? TryLoad(path + ".tga")
                    ?? TryLoad(path + ".wav")
                    ?? TryLoad(path + ".ogg")
                    ?? TryLoad(path + ".mp3");
            }

            if (resource == null)
            {
                return null;
            }

            if (type != null && !type.IsInstanceOfType(resource))
            {
                return null;
            }

            return resource;
        }

        private static Resource TryLoad(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            return AssetSystemResources.Load<Resource>(path);
        }
    }
}
