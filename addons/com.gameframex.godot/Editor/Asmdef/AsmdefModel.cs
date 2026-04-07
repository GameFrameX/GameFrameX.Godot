#if TOOLS
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GameFrameX.Editor.Asmdef
{
    /// <summary>
    /// Unity asmdef JSON 模型（兼容扩展字段）。
    /// </summary>
    public sealed class AsmdefModel
    {
        [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
        [JsonPropertyName("rootNamespace")] public string RootNamespace { get; set; } = string.Empty;
        [JsonPropertyName("references")] public List<string> References { get; set; } = new List<string>();
        [JsonPropertyName("includePlatforms")] public List<string> IncludePlatforms { get; set; } = new List<string>();
        [JsonPropertyName("excludePlatforms")] public List<string> ExcludePlatforms { get; set; } = new List<string>();
        [JsonPropertyName("allowUnsafeCode")] public bool AllowUnsafeCode { get; set; }
        [JsonPropertyName("overrideReferences")] public bool OverrideReferences { get; set; }
        [JsonPropertyName("precompiledReferences")] public List<string> PrecompiledReferences { get; set; } = new List<string>();
        [JsonPropertyName("autoReferenced")] public bool AutoReferenced { get; set; } = true;
        [JsonPropertyName("defineConstraints")] public List<string> DefineConstraints { get; set; } = new List<string>();
        [JsonPropertyName("versionDefines")] public List<AsmdefVersionDefine> VersionDefines { get; set; } = new List<AsmdefVersionDefine>();
        [JsonPropertyName("noEngineReferences")] public bool NoEngineReferences { get; set; }

        // 扩展字段：用于增强 Godot 场景下的宏配置能力
        [JsonPropertyName("defines")] public List<string> Defines { get; set; } = new List<string>();
        [JsonPropertyName("platformDefines")] public Dictionary<string, List<string>> PlatformDefines { get; set; } = new Dictionary<string, List<string>>();
        [JsonPropertyName("editorOnly")] public bool EditorOnly { get; set; }
    }

    public sealed class AsmdefVersionDefine
    {
        [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
        [JsonPropertyName("expression")] public string Expression { get; set; } = string.Empty;
        [JsonPropertyName("define")] public string Define { get; set; } = string.Empty;
    }

    public sealed class AsmdefDocument
    {
        public string FilePath { get; set; } = string.Empty;
        public AsmdefModel Model { get; set; } = new AsmdefModel();
    }
}
#endif
