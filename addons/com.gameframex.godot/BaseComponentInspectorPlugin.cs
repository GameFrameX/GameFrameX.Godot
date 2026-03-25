#if TOOLS
using System;
using System.Collections.Generic;
using GameFrameX.Runtime;
using Godot;

/// <summary>
/// BaseComponent 的 Inspector 插件，用于在编辑器中显示 Helper 类型属性的下拉列表。
/// </summary>
/// <remarks>
/// Inspector plugin for BaseComponent that displays dropdown lists for Helper type properties in the editor.
/// </remarks>
[Tool]
public partial class BaseComponentInspectorPlugin : EditorInspectorPlugin
{
    /// <summary>
    /// 表示"无"选项的显示名称。
    /// </summary>
    /// <remarks>
    /// Display name for the "None" option.
    /// </remarks>
    private const string NoneOptionName = "<None>";

    /// <summary>
    /// 属性名称到 Helper 接口类型的映射表。
    /// 键为规范化后的属性名称（小写、无特殊字符），值为对应的接口类型。
    /// </summary>
    /// <remarks>
    /// Mapping table from property names to Helper interface types.
    /// Key is the normalized property name (lowercase, no special characters), value is the corresponding interface type.
    /// </remarks>
    private static readonly Dictionary<string, System.Type> HelperPropertyTypeMap = new Dictionary<string, System.Type>(StringComparer.Ordinal)
    {
        { "mtexthelpertypename", typeof(Utility.Text.ITextHelper) },
        { "texthelpertypename", typeof(Utility.Text.ITextHelper) },
        { "mversionhelpertypename", typeof(GameFrameX.Runtime.Version.IVersionHelper) },
        { "versionhelpertypename", typeof(GameFrameX.Runtime.Version.IVersionHelper) },
        { "mloghelpertypename", typeof(GameFrameworkLog.ILogHelper) },
        { "loghelpertypename", typeof(GameFrameworkLog.ILogHelper) },
        { "mcompressionhelpertypename", typeof(Utility.Compression.ICompressionHelper) },
        { "compressionhelpertypename", typeof(Utility.Compression.ICompressionHelper) },
        { "mjsonhelpertypename", typeof(Utility.Json.IJsonHelper) },
        { "jsonhelpertypename", typeof(Utility.Json.IJsonHelper) },
        { "componenttype", typeof(GameFrameworkComponent) },
    };

    /// <summary>
    /// 判断此 Inspector 插件是否可以处理指定的对象。
    /// </summary>
    /// <remarks>
    /// Determines whether this Inspector plugin can handle the specified object.
    /// </remarks>
    /// <param name="object">要检查的 Godot 对象 / The Godot object to check</param>
    /// <returns>如果对象不为 null 则返回 <c>true</c>；否则返回 <c>false</c> / <c>true</c> if the object is not null; otherwise <c>false</c></returns>
    public override bool _CanHandle(GodotObject @object)
    {
        return @object != null;
    }

    /// <summary>
    /// 解析属性并为匹配的 Helper 类型属性添加自定义编辑器。
    /// </summary>
    /// <remarks>
    /// Parses properties and adds custom editors for matching Helper type properties.
    /// </remarks>
    /// <param name="object">包含属性的对象 / The object containing the property</param>
    /// <param name="type">属性的类型 / The type of the property</param>
    /// <param name="name">属性名称 / The name of the property</param>
    /// <param name="hintType">属性提示类型 / The property hint type</param>
    /// <param name="hintString">属性提示字符串 / The property hint string</param>
    /// <param name="usageFlags">属性使用标志 / The property usage flags</param>
    /// <param name="wide">是否为宽属性 / Whether the property is wide</param>
    /// <returns>如果属性被处理则返回 <c>true</c>；否则返回 <c>false</c> / <c>true</c> if the property was handled; otherwise <c>false</c></returns>
    public override bool _ParseProperty(GodotObject @object, Variant.Type type, string name, PropertyHint hintType, string hintString, PropertyUsageFlags usageFlags, bool wide)
    {
        string normalizedPropertyName = NormalizePropertyName(name);
        if (!HelperPropertyTypeMap.TryGetValue(normalizedPropertyName, out System.Type helperInterfaceType))
        {
            return false;
        }

        AddPropertyEditor(name, new HelperTypeEditorProperty(name, helperInterfaceType));
        return true;
    }

    /// <summary>
    /// 规范化属性名称，移除特殊字符并转换为小写。
    /// </summary>
    /// <remarks>
    /// Normalizes the property name by removing special characters and converting to lowercase.
    /// </remarks>
    /// <param name="propertyName">要规范化的属性名称 / The property name to normalize</param>
    /// <returns>规范化后的属性名称 / The normalized property name</returns>
    private static string NormalizePropertyName(string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
        {
            return string.Empty;
        }

        char[] buffer = new char[propertyName.Length];
        int count = 0;
        for (int i = 0; i < propertyName.Length; i++)
        {
            char c = propertyName[i];
            if (char.IsLetterOrDigit(c))
            {
                buffer[count++] = char.ToLowerInvariant(c);
            }
        }

        return count == 0 ? string.Empty : new string(buffer, 0, count);
    }

    /// <summary>
    /// Helper 类型属性的自定义编辑器，提供下拉列表选择实现类型。
    /// </summary>
    /// <remarks>
    /// Custom editor for Helper type properties that provides a dropdown list to select implementation types.
    /// </remarks>
    private sealed partial class HelperTypeEditorProperty : EditorProperty
    {
        /// <summary>
        /// 关联的属性名称。
        /// </summary>
        /// <remarks>
        /// The associated property name.
        /// </remarks>
        private readonly string m_PropertyName;

        /// <summary>
        /// 用于选择类型的下拉按钮控件。
        /// </summary>
        /// <remarks>
        /// The dropdown button control for selecting types.
        /// </remarks>
        private readonly OptionButton m_OptionButton;

        /// <summary>
        /// 可选的类型名称数组，包含 "&lt;None&gt;" 和所有实现类型。
        /// </summary>
        /// <remarks>
        /// Array of selectable type names, including "&lt;None&gt;" and all implementation types.
        /// </remarks>
        private readonly string[] m_TypeNames;

        /// <summary>
        /// 初始化 HelperTypeEditorProperty 的新实例。
        /// </summary>
        /// <remarks>
        /// Initializes a new instance of HelperTypeEditorProperty.
        /// </remarks>
        /// <param name="propertyName">要编辑的属性名称 / The property name to edit</param>
        /// <param name="helperInterfaceType">Helper 接口类型，用于查找所有实现类型 / The Helper interface type to find all implementations</param>
        public HelperTypeEditorProperty(string propertyName, System.Type helperInterfaceType)
        {
            m_PropertyName = propertyName;
            m_TypeNames = BuildTypeNames(helperInterfaceType);
            m_OptionButton = new OptionButton();
            m_OptionButton.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            m_OptionButton.ItemSelected += OnItemSelected;
            AddChild(m_OptionButton);
            AddFocusable(m_OptionButton);
            RefreshItems();
        }

        /// <summary>
        /// 更新属性显示，同步下拉列表的选中状态与属性值。
        /// </summary>
        /// <remarks>
        /// Updates the property display, synchronizing the dropdown selection with the property value.
        /// </remarks>
        public override void _UpdateProperty()
        {
            string selectedTypeName = GetEditedObject().Get(m_PropertyName).AsString();
            int selectedIndex = Array.IndexOf(m_TypeNames, selectedTypeName);
            if (selectedIndex < 0)
            {
                selectedIndex = 0;
            }

            if (m_OptionButton.Selected != selectedIndex)
            {
                m_OptionButton.Select(selectedIndex);
            }
        }

        /// <summary>
        /// 构建可选类型名称列表。
        /// </summary>
        /// <remarks>
        /// Builds the list of selectable type names.
        /// </remarks>
        /// <param name="helperInterfaceType">Helper 接口类型 / The Helper interface type</param>
        /// <returns>包含 "&lt;None&gt;" 和所有实现类型名称的数组 / Array containing "&lt;None&gt;" and all implementation type names</returns>
        private static string[] BuildTypeNames(System.Type helperInterfaceType)
        {
            List<string> typeNames = new List<string> { NoneOptionName };
            List<string> runtimeTypeNames = Utility.Assembly.GetRuntimeTypeNames(helperInterfaceType);
            runtimeTypeNames.Sort(StringComparer.Ordinal);
            typeNames.AddRange(runtimeTypeNames);
            return typeNames.ToArray();
        }

        /// <summary>
        /// 刷新下拉列表项。
        /// </summary>
        /// <remarks>
        /// Refreshes the dropdown list items.
        /// </remarks>
        private void RefreshItems()
        {
            m_OptionButton.Clear();
            for (int i = 0; i < m_TypeNames.Length; i++)
            {
                m_OptionButton.AddItem(m_TypeNames[i]);
            }
        }

        /// <summary>
        /// 处理下拉列表选中项变更事件。
        /// </summary>
        /// <remarks>
        /// Handles the dropdown list selection changed event.
        /// </remarks>
        /// <param name="index">选中的项索引 / The index of the selected item</param>
        private void OnItemSelected(long index)
        {
            string selectedTypeName = index <= 0 ? string.Empty : m_TypeNames[index];
            EmitChanged(m_PropertyName, selectedTypeName);
        }
    }
}
#endif