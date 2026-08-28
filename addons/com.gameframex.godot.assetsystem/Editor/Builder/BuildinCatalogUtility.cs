using System.Collections.Generic;
using System.Text.Json;

namespace GameFrameX.AssetSystem.Editor
{
    /// <summary>
    /// BuildinCatalog(内置资源清单目录)构建工具。
    /// 输出 JSON 的字段名与运行时 DefaultBuildinFileCatalog 严格对齐:
    /// PackageName / PackageVersion / Wrappers[].BundleGUID / Wrappers[].FileName,
    /// 运行时通过 AssetSystemResources.Load 读取候选路径下的文件并按 JSON 反序列化。
    /// </summary>
    public static class BuildinCatalogUtility
    {
        private static readonly JsonSerializerOptions SerializeOptions = new JsonSerializerOptions
        {
            IncludeFields = true,
            WriteIndented = true
        };

        /// <summary>
        /// 生成 BuildinCatalog 的 JSON 文本。
        /// </summary>
        public static string BuildJson(string packageName, string packageVersion, IReadOnlyList<BuildinCatalogFileEntry> entries)
        {
            if (entries == null)
            {
                entries = new List<BuildinCatalogFileEntry>();
            }

            // ponytail: 用匿名类型承载序列化字段名,与运行时 internal DefaultBuildinFileCatalog 的 public 字段名手工对齐;
            // 任一侧字段名变更时必须同步修改另一侧,并由 AssetSystemBuilderTests 的字段名断言兜底。
            var wrappers = new List<object>(entries.Count);
            foreach (var entry in entries)
            {
                wrappers.Add(new
                {
                    entry.BundleGUID,
                    entry.FileName
                });
            }

            var document = new
            {
                PackageName = packageName,
                PackageVersion = packageVersion,
                Wrappers = wrappers
            };

            return JsonSerializer.Serialize(document, SerializeOptions);
        }
    }
}
