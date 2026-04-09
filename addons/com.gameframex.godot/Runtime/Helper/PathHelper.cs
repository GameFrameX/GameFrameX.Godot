using System;
using System.IO;
using System.Net.Mime;
using System.Text;
using Godot;

namespace GameFrameX.Runtime
{
    public static class PathHelper
    {
        /// <summary>
        /// 获取项目名称
        /// </summary>
        /// <returns>项目/产品名称字符串</returns>
        public static string GetProductName()
        {
            // 读取 project.godot 中 [application] 下的 config/name 字段
            Variant productNameVar = ProjectSettings.GetSetting("application/config/name");
            // 兜底：若未配置，返回默认名
            string productName = productNameVar.IsNull() ? "Unnamed Project" : productNameVar.AsString();
            return productName;
        }

        /// <summary>
        ///应用程序外部资源路径存放路径(热更新资源路径)
        /// </summary>
        public static string AppHotfixResPath
        {
            get
            {
                string game = GetProductName();
                string path = $"{GetPersistentDataPath()}/{game}/";
                return path;
            }
        }

        /// <summary>
        /// 获取持久化目录（返回绝对系统路径）
        /// </summary>
        /// <returns>跨平台持久化目录的绝对路径</returns>
        public static string GetPersistentDataPath()
        {
            // 将 Godot 虚拟路径 user:// 转换为系统绝对路径
            string absolutePath = ProjectSettings.GlobalizePath("user://");
            // 确保目录存在（首次使用时创建）
            if (!Directory.Exists(absolutePath))
            {
                Directory.CreateDirectory(absolutePath);
            }

            return absolutePath;
        }

        /// <summary>
        /// 应用程序内部资源路径存放路径
        /// </summary>
        public static string AppResPath
        {
            get { return NormalizePath(GetStreamingAssetsPath()); }
        }

        /// <summary>
        /// 应用程序内部资源路径存放路径（网络请求专用）
        /// </summary>
        public static string AppResPath4Web
        {
            get
            {
                var absoluteOrVirtual = NormalizePath(GetStreamingAssetsPath());
                if (absoluteOrVirtual.StartsWith("res://", StringComparison.OrdinalIgnoreCase) ||
                    absoluteOrVirtual.StartsWith("user://", StringComparison.OrdinalIgnoreCase) ||
                    absoluteOrVirtual.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
                {
                    return absoluteOrVirtual;
                }

                return $"file://{absoluteOrVirtual}";
            }
        }

        /// <summary>
        /// 获取 StreamingAssets 路径
        /// </summary>
        /// <returns>跨平台的 StreamingAssets 路径</returns>
        public static string GetStreamingAssetsPath()
        {
            string basePath;
            if (Engine.IsEditorHint())
            {
                // 开发期（编辑器内）：直接使用 res:// 下的 streaming_assets 目录
                basePath = ProjectSettings.GlobalizePath("res://streaming_assets/");
            }
            else
            {
                // 打包后：根据平台适配路径
                switch (OS.GetName())
                {
                    case "Android":
                        // Android 打包后，res:// 会被打包到 APK 的 assets 目录，需用 user:// 或读取 APK 内资源
                        // 注意：Android 中 res:// 打包后为只读，无法写入，需用 FileAccess 读取
                        basePath = "res://streaming_assets/";
                        break;
                    case "iOS":
                        // iOS 打包后，res:// 对应 APP 包内的 Resources 目录
                        basePath = "res://streaming_assets/";
                        break;
                    case "Windows":
                    case "macOS":
                    case "Linux":
                        // 桌面平台：可执行文件同级的 streaming_assets 目录
                        string exeDir = Path.GetDirectoryName(OS.GetExecutablePath());
                        basePath = Path.Combine(exeDir, "streaming_assets/");
                        break;
                    case "HTML5":
                        // Web 平台：res:// 对应编译后的资源目录
                        basePath = "res://streaming_assets/";
                        break;
                    default:
                        basePath = "res://streaming_assets/";
                        break;
                }
            }

            // 确保目录存在（打包后桌面平台需手动创建）
            if (!Directory.Exists(basePath) && !OS.HasFeature("editor"))
            {
                Directory.CreateDirectory(basePath);
            }

            return basePath;
        }

        /// <summary>
        /// 规范化路径
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string NormalizePath(string path)
        {
            return path.Replace('\\', '/').Replace("\\", "/");
        }

        static readonly StringBuilder CombineStringBuilder = new StringBuilder();

        /// <summary>
        /// 拼接路径
        /// </summary>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static string Combine(params string[] paths)
        {
            CombineStringBuilder.Clear();
            const string separatorA = "/";
            const string separatorB = "\\";
            for (var index = 0; index < paths.Length - 1; index++)
            {
                var path = paths[index];
                CombineStringBuilder.Append(path);
                if (path.EndsWithFast(separatorA) || path.EndsWithFast(separatorB))
                {
                    continue;
                }

                if (path.StartsWithFast(separatorA) || path.StartsWithFast(separatorB))
                {
                    continue;
                }

                CombineStringBuilder.Append(separatorA);
            }

            CombineStringBuilder.Append(paths[paths.Length - 1]);
            return CombineStringBuilder.ToString();
        }
    }
}
