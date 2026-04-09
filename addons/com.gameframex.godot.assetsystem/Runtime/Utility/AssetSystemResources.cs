using System;
using System.Collections.Generic;
using System.IO;

namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    public static class AssetSystemResources
    {
        [AssetSystemPreserve]
        public static T Load<T>(string path) where T : class
        {
            if (typeof(global::Godot.Resource).IsAssignableFrom(typeof(T)))
            {
                return global::Godot.ResourceLoader.Load(path) as T;
            }

            var resolvedPath = ResolveResourcePath(path);
            if (string.IsNullOrEmpty(resolvedPath))
            {
                return null;
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

        private static void AddPathCandidate(ICollection<string> candidates, string path)
        {
            if (!string.IsNullOrWhiteSpace(path) && !candidates.Contains(path))
            {
                candidates.Add(path);
            }
        }
    }
}
