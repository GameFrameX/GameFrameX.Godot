#if TOOLS
using System;
using System.Collections.Generic;
using GameFrameX.Runtime;
using Godot;

[Tool]
public partial class BaseComponentInspectorPlugin : EditorInspectorPlugin
{
    private const string NoneOptionName = "<None>";
    private static readonly Dictionary<string, Type> HelperPropertyTypeMap = new Dictionary<string, Type>(StringComparer.Ordinal)
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
        { "jsonhelpertypename", typeof(Utility.Json.IJsonHelper) }
    };

    public override bool _CanHandle(GodotObject @object)
    {
        return @object != null;
    }

    public override bool _ParseProperty(GodotObject @object, Variant.Type type, string name, PropertyHint hintType, string hintString, PropertyUsageFlags usageFlags, bool wide)
    {
        string normalizedPropertyName = NormalizePropertyName(name);
        if (!HelperPropertyTypeMap.TryGetValue(normalizedPropertyName, out Type helperInterfaceType))
        {
            return false;
        }

        AddPropertyEditor(name, new HelperTypeEditorProperty(name, helperInterfaceType));
        return true;
    }

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

    private sealed partial class HelperTypeEditorProperty : EditorProperty
    {
        private readonly string m_PropertyName;
        private readonly OptionButton m_OptionButton;
        private readonly string[] m_TypeNames;

        public HelperTypeEditorProperty(string propertyName, Type helperInterfaceType)
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

        private static string[] BuildTypeNames(Type helperInterfaceType)
        {
            List<string> typeNames = new List<string> { NoneOptionName };
            List<string> runtimeTypeNames = Utility.Assembly.GetRuntimeTypeNames(helperInterfaceType);
            runtimeTypeNames.Sort(StringComparer.Ordinal);
            typeNames.AddRange(runtimeTypeNames);
            return typeNames.ToArray();
        }

        private void RefreshItems()
        {
            m_OptionButton.Clear();
            for (int i = 0; i < m_TypeNames.Length; i++)
            {
                m_OptionButton.AddItem(m_TypeNames[i]);
            }
        }

        private void OnItemSelected(long index)
        {
            string selectedTypeName = index <= 0 ? string.Empty : m_TypeNames[index];
            EmitChanged(m_PropertyName, selectedTypeName);
        }
    }
}
#endif
