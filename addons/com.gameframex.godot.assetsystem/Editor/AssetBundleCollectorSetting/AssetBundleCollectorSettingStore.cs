using System;
using System.IO;
using System.Text.Json;

namespace GameFrameX.AssetSystem.Editor
{
    /// <summary>
    /// Collector 配置持久化(JSON 文件读写)。
    /// ponytail: 尚未接入 AssetSystemEditorPlugin 的 Collector Tab UI(接入评估见 Phase 2.5);
    /// user:// 虚拟路径到物理路径的解析由 Plugin 侧挂点完成(依赖 Godot API),本类保持纯托管便于单测。
    /// </summary>
    public static class AssetBundleCollectorSettingStore
    {
        private static readonly JsonSerializerOptions SerializeOptions = new JsonSerializerOptions
        {
            IncludeFields = true,
            WriteIndented = true
        };

        /// <summary>
        /// 从指定文件加载配置;文件不存在或解析失败时返回默认配置。
        /// </summary>
        public static AssetBundleCollectorSetting LoadOrCreate(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("配置文件路径不能为空", nameof(filePath));
            }

            if (File.Exists(filePath) == false)
            {
                return new AssetBundleCollectorSetting();
            }

            var setting = DeserializeFromText(File.ReadAllText(filePath));
            if (setting == null)
            {
                return new AssetBundleCollectorSetting();
            }

            return setting;
        }

        /// <summary>
        /// 保存配置到指定文件。
        /// </summary>
        public static void Save(AssetBundleCollectorSetting setting, string filePath)
        {
            if (setting == null)
            {
                throw new ArgumentNullException(nameof(setting));
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("配置文件路径不能为空", nameof(filePath));
            }

            var directory = Path.GetDirectoryName(filePath);
            if (string.IsNullOrEmpty(directory) == false)
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(filePath, SerializeToText(setting));
        }

        /// <summary>
        /// 序列化为 JSON 文本。
        /// </summary>
        public static string SerializeToText(AssetBundleCollectorSetting setting)
        {
            if (setting == null)
            {
                throw new ArgumentNullException(nameof(setting));
            }

            return JsonSerializer.Serialize(setting, SerializeOptions);
        }

        /// <summary>
        /// 从 JSON 文本反序列化;文本无效时返回 null。
        /// </summary>
        public static AssetBundleCollectorSetting DeserializeFromText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            return JsonSerializer.Deserialize<AssetBundleCollectorSetting>(text, SerializeOptions);
        }
    }
}
