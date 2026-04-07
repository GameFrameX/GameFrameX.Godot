#if TOOLS
using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace GameFrameX.Editor.Asmdef
{
    public static class AsmdefIO
    {
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        public static AsmdefDocument LoadDocument(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("asmdef 文件路径不能为空。", nameof(filePath));
            }

            string json = File.ReadAllText(filePath, Encoding.UTF8);
            if (string.IsNullOrWhiteSpace(json))
            {
                string defaultName = Path.GetFileNameWithoutExtension(filePath) ?? string.Empty;
                var fallbackDocument = new AsmdefDocument
                {
                    FilePath = filePath,
                    Model = new AsmdefModel
                    {
                        Name = defaultName,
                        RootNamespace = defaultName
                    }
                };

                // 优化：空文件首次加载时，自动写回完整 JSON，避免后续再次被当作无效文件。
                SaveDocument(filePath, fallbackDocument.Model);
                return fallbackDocument;
            }

            AsmdefModel model = JsonSerializer.Deserialize<AsmdefModel>(json, SerializerOptions) ?? new AsmdefModel();
            return new AsmdefDocument
            {
                FilePath = filePath,
                Model = model
            };
        }

        public static void SaveDocument(string filePath, AsmdefModel model)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException("asmdef 文件路径不能为空。", nameof(filePath));
            }

            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            string json = JsonSerializer.Serialize(model, SerializerOptions);
            string normalized = EnsureEndsWithNewLine(json);
            WriteTextIfChanged(filePath, normalized);
        }

        public static bool WriteTextIfChanged(string filePath, string content)
        {
            string normalized = content ?? string.Empty;
            if (File.Exists(filePath))
            {
                string oldContent = File.ReadAllText(filePath, Encoding.UTF8);
                if (string.Equals(oldContent, normalized, StringComparison.Ordinal))
                {
                    return false;
                }
            }

            string directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string tempPath = filePath + ".tmp";
            File.WriteAllText(tempPath, normalized, new UTF8Encoding(false));
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            File.Move(tempPath, filePath);
            return true;
        }

        private static string EnsureEndsWithNewLine(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return Environment.NewLine;
            }

            if (content.EndsWith("\n", StringComparison.Ordinal))
            {
                return content;
            }

            return content + Environment.NewLine;
        }
    }
}
#endif
