#if TOOLS
using System;
using System.Collections.Generic;
using GameFrameX.Editor;
using GameFrameX.Runtime;
using Godot;
using Type = System.Type;

/// <summary>
/// BaseComponent 的 Inspector 插件，用于在编辑器中显示 Helper 类型属性的下拉列表。
/// </summary>
/// <remarks>
/// Inspector plugin for BaseComponent that displays dropdown lists for Helper type properties in the editor.
/// </remarks>
[Tool]
public partial class BaseComponentInspector : GameFrameworkInspector
{
    public override HashSet<string> GetHiddenPropertyNames()
    {
        return new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "componentType"
        };
    }

    /// <summary>
    /// 属性名称到 Helper 接口类型的映射表（不区分大小写）。
    /// 键为属性名称（移除特殊字符后），值为对应的接口类型。
    /// </summary>
    /// <remarks>
    /// Mapping table from property names to Helper interface types (case-insensitive).
    /// Key is the property name (with special characters removed), value is the corresponding interface type.
    /// </remarks>
    public override Dictionary<string, Type> GetHelperPropertyTypeMap()
    {
        return new Dictionary<string, System.Type>(StringComparer.OrdinalIgnoreCase)
        {
            { "mTextHelperTypeName", typeof(Utility.Text.ITextHelper) },
            { "TextHelperTypeName", typeof(Utility.Text.ITextHelper) },
            { "mVersionHelperTypeName", typeof(GameFrameX.Runtime.Version.IVersionHelper) },
            { "VersionHelperTypeName", typeof(GameFrameX.Runtime.Version.IVersionHelper) },
            { "mLogHelperTypeName", typeof(GameFrameworkLog.ILogHelper) },
            { "LogHelperTypeName", typeof(GameFrameworkLog.ILogHelper) },
            { "mCompressionHelperTypeName", typeof(Utility.Compression.ICompressionHelper) },
            { "CompressionHelperTypeName", typeof(Utility.Compression.ICompressionHelper) },
            { "mJsonHelperTypeName", typeof(Utility.Json.IJsonHelper) },
            { "JsonHelperTypeName", typeof(Utility.Json.IJsonHelper) },
        };
    }

    protected override bool IsCanHandle(GodotObject @object)
    {
        if (@object is BaseComponent)
        {
            return true;
        }

        if (@object is not Node node)
        {
            return false;
        }

        var scriptVariant = node.GetScript();
        if (scriptVariant.Obj is not CSharpScript cSharpScript)
        {
            return false;
        }

        var componentType = typeof(BaseComponent);
        var componentName = componentType.Name;
        var componentFullName = componentType.FullName;
        var scriptClass = cSharpScript.GetClass();
        var scriptPath = cSharpScript.ResourcePath ?? string.Empty;

        return cSharpScript.IsClass(componentName) ||
               cSharpScript.IsClass(componentFullName) ||
               string.Equals(scriptClass, componentName, StringComparison.Ordinal) ||
               string.Equals(scriptClass, componentFullName, StringComparison.Ordinal) ||
               scriptPath.EndsWith($"/{componentName}.cs", StringComparison.OrdinalIgnoreCase);
    }
}
#endif
