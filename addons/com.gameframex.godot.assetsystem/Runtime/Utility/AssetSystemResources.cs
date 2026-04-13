using System;
using System.Collections.Generic;
using System.IO;

namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    public static class AssetSystemResources
    {
        /// <summary>
        /// 路径资源加载入口（Path 模式）。
        /// 适用：res://、user://、绝对路径、相对路径。
        /// 不适用：AssetSystem 清单内的 location（请改用 AssetSystem.LoadAsset*/LoadRawFile* 或 TryGetPackageAsset）。
        /// </summary>
        /// <param name="path">资源路径（非包内 location）。</param>
        [AssetSystemPreserve]
        public static T Load<T>(string path) where T : class
        {
            if (typeof(global::Godot.Resource).IsAssignableFrom(typeof(T)))
            {
                var resourcePath = ResolveResourceLoaderPath(path);
                if (string.IsNullOrWhiteSpace(resourcePath))
                {
                    return null;
                }

                if (!global::Godot.ResourceLoader.Exists(resourcePath))
                {
                    return null;
                }

                return global::Godot.ResourceLoader.Load(resourcePath) as T;
            }

            var resolvedPath = ResolveResourcePath(path);
            if (string.IsNullOrEmpty(resolvedPath))
            {
                return null;
            }

            if (typeof(T) == typeof(byte[]))
            {
                return (T)(object)File.ReadAllBytes(resolvedPath);
            }

            if (typeof(T) == typeof(string))
            {
                return File.ReadAllText(resolvedPath) as T;
            }

            try
            {
                return Activator.CreateInstance(typeof(T)) as T;
            }
            catch
            {
                return null;
            }
        }

        [AssetSystemPreserve]
        public static BundleFileCreateRequest UnloadUnusedAssets()
        {
            return new BundleFileCreateRequest();
        }

        private static string ResolveResourcePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            var candidates = new List<string>(8);
            AddPathCandidate(candidates, path);

            if (path.StartsWith("res://", StringComparison.OrdinalIgnoreCase) || path.StartsWith("user://", StringComparison.OrdinalIgnoreCase))
            {
                AddPathCandidate(candidates, global::Godot.ProjectSettings.GlobalizePath(path));
            }
            else if (Path.IsPathRooted(path))
            {
                AddPathCandidate(candidates, path);
            }
            else
            {
                var normalized = path.Replace('\\', '/');
                AddPathCandidate(candidates, global::Godot.ProjectSettings.GlobalizePath($"res://{normalized}"));
                AddPathCandidate(candidates, global::Godot.ProjectSettings.GlobalizePath($"res://{normalized}.tres"));
                AddPathCandidate(candidates, global::Godot.ProjectSettings.GlobalizePath($"res://{normalized}.res"));
                AddPathCandidate(candidates, global::Godot.ProjectSettings.GlobalizePath($"res://{normalized}.txt"));
                AddPathCandidate(candidates, global::Godot.ProjectSettings.GlobalizePath($"res://{normalized}.png"));
                AddPathCandidate(candidates, global::Godot.ProjectSettings.GlobalizePath($"res://{normalized}.webp"));
            }

            for (var i = 0; i < candidates.Count; i++)
            {
                if (File.Exists(candidates[i]))
                {
                    return candidates[i];
                }
            }

            return string.Empty;
        }

        private static string ResolveResourceLoaderPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            var normalized = path.Replace('\\', '/');
            if (normalized.StartsWith("res://", StringComparison.OrdinalIgnoreCase) ||
                normalized.StartsWith("user://", StringComparison.OrdinalIgnoreCase))
            {
                return normalized;
            }

            if (Path.IsPathRooted(normalized))
            {
                var localized = global::Godot.ProjectSettings.LocalizePath(normalized);
                if (localized.StartsWith("res://", StringComparison.OrdinalIgnoreCase) ||
                    localized.StartsWith("user://", StringComparison.OrdinalIgnoreCase))
                {
                    return localized;
                }
            }

            return $"res://{normalized.TrimStart('/')}";
        }

        private static void AddPathCandidate(ICollection<string> candidates, string path)
        {
            if (!string.IsNullOrWhiteSpace(path) && !candidates.Contains(path))
            {
                candidates.Add(path);
            }
        }
    }
}
