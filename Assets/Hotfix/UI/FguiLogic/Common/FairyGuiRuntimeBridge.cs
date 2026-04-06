using System;
using FairyGUI;
using Godot;
using FileAccess = Godot.FileAccess;

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

        internal static void EnsureInitialized()
        {
            if (s_Initialized)
            {
                return;
            }

            _ = Stage.inst;
            LoadRequiredPackages(DefaultBundleRootPath);
            s_Initialized = true;
        }

        internal static GComponent CreateFullScreenView(string packageName, string componentName)
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
            GRoot.inst.AddChild(component);
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

                var packagePath = $"{bundleRootPath}/{packageName}/{packageName}_fui.bytes";
                if (!FileAccess.FileExists(packagePath))
                {
                    GD.PushWarning($"[FGUIBridge] package file missing: {packagePath}");
                    continue;
                }

                var package = UIPackage.AddPackage(packagePath, LoadResourceWithFallback);
                if (package == null)
                {
                    GD.PushWarning($"[FGUIBridge] add package failed: {packagePath}");
                }
            }
        }

        private static object LoadResourceWithFallback(string path, Type type, out DestroyMethod destroyMethod)
        {
            destroyMethod = DestroyMethod.Unload;

            if (type == typeof(byte[]))
            {
                if (!FileAccess.FileExists(path))
                {
                    return null;
                }

                using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
                return file.GetBuffer((long)file.GetLength());
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

            return ResourceLoader.Load(path);
        }
    }
}
