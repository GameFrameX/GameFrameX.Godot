using System;
using System.IO;

namespace GameFrameX.AssetSystem.Editor
{
    /// <summary>
    /// 构建 PCK 内部路径的纯函数工具。
    /// 路径约定(锁定,Phase 2.1 加载侧依赖):
    /// PCK 内部路径 = 构建产物物理路径相对构建输出根(GlobalOutputPath)的正斜杠路径。
    /// 例如输出根为 .../Bundles 时,.../Bundles/DefaultPackage/1.0.0/abc123.bundle
    /// 在 PCK 内的路径为 DefaultPackage/1.0.0/abc123.bundle。
    /// 加载侧只需把磁盘物理路径替换为 PCK 挂载后的等价路径,即可复用同一映射规则。
    /// </summary>
    public static class AssetSystemPckPathUtility
    {
        /// <summary>
        /// 计算构建产物在 PCK 内的路径。
        /// </summary>
        public static string GetPckInnerPath(string physicalFilePath, string buildOutputRoot)
        {
            if (string.IsNullOrWhiteSpace(physicalFilePath))
            {
                throw new ArgumentException("构建产物路径不能为空", nameof(physicalFilePath));
            }

            if (string.IsNullOrWhiteSpace(buildOutputRoot))
            {
                throw new ArgumentException("构建输出根路径不能为空", nameof(buildOutputRoot));
            }

            var fullFilePath = Path.GetFullPath(physicalFilePath);
            var fullRootPath = Path.GetFullPath(buildOutputRoot);
            var relativePath = Path.GetRelativePath(fullRootPath, fullFilePath);
            if (relativePath == "." || relativePath.StartsWith("..", StringComparison.Ordinal))
            {
                throw new ArgumentException($"构建产物路径不在输出根内: {physicalFilePath} -> {buildOutputRoot}");
            }

            return relativePath.Replace('\\', '/');
        }
    }
}
