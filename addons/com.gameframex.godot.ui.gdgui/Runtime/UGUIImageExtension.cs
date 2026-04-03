using System;
using System.Threading.Tasks;
using GameFrameX.Runtime;
using Godot;

namespace GameFrameX.UI.GDGUI.Runtime
{
    /// <summary>
    /// GDGUI 图片扩展。
    /// </summary>
    public static class UGUIImageExtension
    {
        /// <summary>
        /// 异步设置图标。
        /// </summary>
        /// <param name="self">目标图片节点。</param>
        /// <param name="icon">图标资源路径。</param>
        /// <returns>异步任务。</returns>
        public static async Task SetIconAsync(this TextureRect self, string icon)
        {
            if (self == null || string.IsNullOrWhiteSpace(icon))
            {
                return;
            }

            await Task.Yield();
            try
            {
                var normalizedPath = NormalizeToResourcePath(icon);
                if (string.IsNullOrWhiteSpace(normalizedPath) || !ResourceLoader.Exists(normalizedPath))
                {
                    Log.Warning("Icon resource does not exist: {0}", icon);
                    return;
                }

                var texture = ResourceLoader.Load<Texture2D>(normalizedPath);
                self.Texture = texture;
            }
            catch (Exception e)
            {
                Log.Error($"Failed to load icon '{icon}': {e.Message}");
            }
        }

        /// <summary>
        /// 将路径规范化为 Godot 资源路径。
        /// </summary>
        /// <param name="path">原始路径。</param>
        /// <returns>规范化路径。</returns>
        private static string NormalizeToResourcePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return null;
            }

            var normalized = path.Replace('\\', '/');
            if (normalized.StartsWith("res://", StringComparison.OrdinalIgnoreCase) || normalized.StartsWith("user://", StringComparison.OrdinalIgnoreCase))
            {
                return normalized;
            }

            const string marker = "/Godot/";
            var index = normalized.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                normalized = "res://" + normalized.Substring(index + marker.Length);
            }

            return normalized;
        }
    }
}
