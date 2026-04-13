using System;
using System.IO;
using GameFrameX.Runtime;
using Godot;

namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    public static class GodotAssetPath
    {
        private const string BuiltinVirtualRoot = "res://streaming_assets";
        private const string HotfixVirtualRoot = "user://hotfix";
        private const string DefaultHotfixAssemblyFileName = "Hotfix.dll";

        [AssetSystemPreserve]
        public static string GetProjectRoot()
        {
            return PathUtility.RegularPath(ProjectSettings.GlobalizePath("res://"));
        }

        [AssetSystemPreserve]
        public static string GetPersistentRoot()
        {
            var path = PathUtility.RegularPath(PathHelper.GetPersistentDataPath());
            EnsureDirectory(path);
            return path;
        }

        [AssetSystemPreserve]
        public static string GetStreamingAssetsRoot()
        {
            var path = PathUtility.RegularPath(PathHelper.GetStreamingAssetsPath());
            if (path.StartsWith("res://", StringComparison.OrdinalIgnoreCase) == false)
            {
                EnsureDirectory(path);
            }

            return path;
        }

        [AssetSystemPreserve]
        public static string GetBuiltinRootVirtual()
        {
            return BuiltinVirtualRoot;
        }

        [AssetSystemPreserve]
        public static string GetBuiltinPackagesRootVirtual()
        {
            return CombineVirtualPath(BuiltinVirtualRoot, "packages");
        }

        [AssetSystemPreserve]
        public static string GetBuiltinAssetSystemRootVirtual()
        {
            return CombineVirtualPath(BuiltinVirtualRoot, AssetSystemSettingsData.Setting.DefaultAssetSystemFolderName);
        }

        [AssetSystemPreserve]
        public static string GetBuiltinAssetSystemPackageRootVirtual(string packageName)
        {
            return CombineVirtualPath(GetBuiltinAssetSystemRootVirtual(), NormalizePackageSegment(packageName));
        }

        [AssetSystemPreserve]
        public static string GetHotfixRootVirtual()
        {
            return HotfixVirtualRoot;
        }

        [AssetSystemPreserve]
        public static string GetHotfixPackagesRootVirtual()
        {
            return HotfixVirtualRoot;
        }

        [AssetSystemPreserve]
        public static string GetHotfixAssemblyFileVirtual(string assemblyFileName = DefaultHotfixAssemblyFileName)
        {
            var normalized = string.IsNullOrWhiteSpace(assemblyFileName)
                ? DefaultHotfixAssemblyFileName
                : assemblyFileName.Trim();
            return CombineVirtualPath(HotfixVirtualRoot, normalized);
        }

        [AssetSystemPreserve]
        public static string GetHotfixAssetSystemRootVirtual()
        {
            return HotfixVirtualRoot;
        }

        [AssetSystemPreserve]
        public static string GetHotfixCacheRootVirtual()
        {
            return CombineVirtualPath(HotfixVirtualRoot, "cache");
        }

        [AssetSystemPreserve]
        public static string GetHotfixPckFileVirtual(string packageName)
        {
            return CombineVirtualPath(GetHotfixPackagesRootVirtual(), $"{NormalizePackageSegment(packageName)}.pck");
        }

        [AssetSystemPreserve]
        public static string GetHotfixAssetSystemPackageRootVirtual(string packageName)
        {
            return CombineVirtualPath(GetHotfixAssetSystemRootVirtual(), NormalizePackageSegment(packageName));
        }

        [AssetSystemPreserve]
        public static string ResolveAbsolutePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            if (path.StartsWith("res://", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("user://", StringComparison.OrdinalIgnoreCase))
            {
                return PathUtility.RegularPath(ProjectSettings.GlobalizePath(path));
            }

            return PathUtility.RegularPath(path);
        }

        [AssetSystemPreserve]
        public static string NormalizePackageSegment(string packageName)
        {
            if (string.IsNullOrWhiteSpace(packageName))
            {
                return "package";
            }

            var invalidChars = Path.GetInvalidFileNameChars();
            var chars = packageName.Trim().ToCharArray();
            for (var i = 0; i < chars.Length; i++)
            {
                if (chars[i] == '/' || chars[i] == '\\')
                {
                    chars[i] = '_';
                    continue;
                }

                for (var j = 0; j < invalidChars.Length; j++)
                {
                    if (chars[i] == invalidChars[j])
                    {
                        chars[i] = '_';
                        break;
                    }
                }
            }

            return new string(chars);
        }

        [AssetSystemPreserve]
        public static string CombineVirtualPath(string root, string child)
        {
            if (string.IsNullOrWhiteSpace(root))
            {
                return child ?? string.Empty;
            }

            if (string.IsNullOrWhiteSpace(child))
            {
                return root;
            }

            return $"{root.TrimEnd('/')}/{child.TrimStart('/')}";
        }

        private static void EnsureDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return;
            }

            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}

