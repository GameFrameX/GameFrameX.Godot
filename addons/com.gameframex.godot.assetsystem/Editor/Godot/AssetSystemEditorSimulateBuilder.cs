#if TOOLS
using System;
using System.IO;
using Godot;

namespace GameFrameX.AssetSystem.Editor
{
    public static class AssetSystemEditorSimulateBuilder
    {
        private const string DefaultOutputRoot = "user://assetsystem_builds";
        private const string ManifestFileName = "build_manifest.txt";

        public static SimulateBuildResult SimulateBuild(string buildPipelineName, string packageName)
        {
            var packageRoot = ResolveLatestPackageRoot(packageName);
            if (string.IsNullOrEmpty(packageRoot))
            {
                throw new InvalidOperationException($"Can not resolve simulate build package root. package={packageName}, pipeline={buildPipelineName}");
            }

            return new SimulateBuildResult
            {
                PackageRootDirectory = packageRoot
            };
        }

        private static string ResolveLatestPackageRoot(string packageName)
        {
            var globalOutputRoot = ProjectSettings.GlobalizePath(DefaultOutputRoot);
            if (!Directory.Exists(globalOutputRoot))
            {
                return string.Empty;
            }

            string[] manifestFiles;
            if (!string.IsNullOrWhiteSpace(packageName))
            {
                var packageRoot = Path.Combine(globalOutputRoot, packageName);
                if (!Directory.Exists(packageRoot))
                {
                    return string.Empty;
                }

                manifestFiles = Directory.GetFiles(packageRoot, ManifestFileName, SearchOption.AllDirectories);
            }
            else
            {
                manifestFiles = Directory.GetFiles(globalOutputRoot, ManifestFileName, SearchOption.AllDirectories);
            }

            if (manifestFiles.Length == 0)
            {
                return string.Empty;
            }

            Array.Sort(manifestFiles, (left, right) => File.GetLastWriteTimeUtc(right).CompareTo(File.GetLastWriteTimeUtc(left)));
            return Path.GetDirectoryName(manifestFiles[0]) ?? string.Empty;
        }
    }
}
#endif
