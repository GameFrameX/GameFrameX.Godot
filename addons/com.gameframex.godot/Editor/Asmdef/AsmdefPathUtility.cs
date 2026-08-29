#if TOOLS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;

namespace GameFrameX.Editor.Asmdef
{
    public static class AsmdefPathUtility
    {
        public static string GetProjectRootPath()
        {
            return ProjectSettings.GlobalizePath("res://");
        }

        public static List<string> FindAllAsmdefFiles()
        {
            string root = GetProjectRootPath();
            var files = Directory.GetFiles(root, "*.asmdef", SearchOption.AllDirectories)
                                 .Where(path => !IsIgnoredPath(path))
                                 .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                                 .ToList();
            return files;
        }

        /// <summary>
        /// 生成物候选发现（孤儿清理域）：addons 下所有 .csproj 与 .asmdef.uid。
        /// 域外不受影响：主 Godot.csproj 在项目根，Hotfix.csproj 有活跃 asmdef。
        /// </summary>
        public static List<string> FindAllGeneratedArtifactCandidates()
        {
            string addonsRoot = Path.Combine(GetProjectRootPath(), "addons");
            var files = new List<string>();
            if (!Directory.Exists(addonsRoot))
            {
                return files;
            }

            files.AddRange(Directory.GetFiles(addonsRoot, "*.csproj", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles(addonsRoot, "*.asmdef.uid", SearchOption.AllDirectories));
            return files.Where(path => !IsIgnoredPath(path))
                        .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                        .ToList();
        }

        public static string GetCsprojPathForAsmdef(string asmdefFilePath)
        {
            string directory = Path.GetDirectoryName(asmdefFilePath) ?? string.Empty;
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(asmdefFilePath);
            return Path.Combine(directory, fileNameWithoutExtension + ".csproj");
        }

        public static string ToProjectRelativePath(string absolutePath)
        {
            string root = GetProjectRootPath().TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (absolutePath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            {
                string relative = absolutePath.Substring(root.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                return "res://" + relative.Replace('\\', '/');
            }

            return absolutePath;
        }

        private static bool IsIgnoredPath(string path)
        {
            string normalized = path.Replace('\\', '/');
            return normalized.Contains("/.godot/", StringComparison.OrdinalIgnoreCase) ||
                   normalized.Contains("/.git/", StringComparison.OrdinalIgnoreCase) ||
                   normalized.Contains("/bin/", StringComparison.OrdinalIgnoreCase) ||
                   normalized.Contains("/obj/", StringComparison.OrdinalIgnoreCase);
        }
    }
}
#endif
