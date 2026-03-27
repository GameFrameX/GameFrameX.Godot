// ==========================================================================================
//  GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//  GameFrameX organization and its derivative projects' copyrights, trademarks, patents, and related rights
//  均受中华人民共和国及相关国际法律法规保护。
//  are protected by the laws of the People's Republic of China and relevant international regulations.
//
//  使用本项目须严格遵守相应法律法规及开源许可证之规定。
//  Usage of this project must strictly comply with applicable laws, regulations, and open-source licenses.
//
//  本项目采用 MIT 许可证与 Apache License 2.0 双许可证分发，
//  This project is dual-licensed under the MIT License and Apache License 2.0,
//  完整许可证文本请参见源代码根目录下的 LICENSE 文件。
//  please refer to the LICENSE file in the root directory of the source code for the full license text.
//
//  禁止利用本项目实施任何危害国家安全、破坏社会秩序、
//  It is prohibited to use this project to engage in any activities that endanger national security, disrupt social order,
//  侵犯他人合法权益等法律法规所禁止的行为！
//  or infringe upon the legitimate rights and interests of others, as prohibited by laws and regulations!
//  因基于本项目二次开发所产生的一切法律纠纷与责任，
//  Any legal disputes and liabilities arising from secondary development based on project
//  本项目组织与贡献者概不承担。
//  shall be borne solely by the developer; the project organization and contributors assume no responsibility.
//
//  GitHub 仓库：https://github.com/GameFrameX
//  GitHub Repository: https://github.com/GameFrameX
//  Gitee  仓库：https://gitee.com/GameFrameX
//  Gitee Repository:  https://gitee.com/GameFrameX
//  官方文档：https://gameframex.doc.alianblank.com/
//  Official Documentation: https://gameframex.doc.alianblank.com/
// ==========================================================================================

#if TOOLS
using System;
using System.Collections.Generic;
using GameFrameX.Runtime;
using GameFrameX.UI.Runtime;
using Godot;

namespace GameFrameX.UI.Editor
{
    /// <summary>
    /// UI 组件检查器插件。
    /// </summary>
    [Tool]
    public partial class UIComponentInspectorPlugin : EditorInspectorPlugin
    {
        private static readonly Dictionary<string, Type> PropertyTypeMap = new Dictionary<string, Type>(StringComparer.Ordinal)
        {
            { "mcomponenttype", typeof(IUIManager) },
            { "componenttype", typeof(IUIManager) },
            { "muiformhelpertypename", typeof(IUIFormHelper) },
            { "uiformhelpertypename", typeof(IUIFormHelper) },
            { "muigrouphelpertypename", typeof(IUIGroupHelper) },
            { "uigrouphelpertypename", typeof(IUIGroupHelper) },
            { "mcustomuiformhelper", typeof(UIFormHelperBase) },
            { "customuiformhelper", typeof(UIFormHelperBase) },
            { "mcustomuigrouphelper", typeof(UIGroupHelperBase) },
            { "customuigrouphelper", typeof(UIGroupHelperBase) },
        };

        public override bool _CanHandle(GodotObject @object)
        {
            return @object is UIComponent;
        }

        public override bool _ParseProperty(GodotObject @object, Variant.Type type, string name, PropertyHint hintType, string hintString, PropertyUsageFlags usageFlags, bool wide)
        {
            var normalizedPropertyName = NormalizePropertyName(name);
            if (!PropertyTypeMap.TryGetValue(normalizedPropertyName, out var interfaceType))
            {
                return false;
            }

            AddPropertyEditor(name, new TypeDropdownEditorProperty(name, interfaceType));
            return true;
        }

        private static string NormalizePropertyName(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return string.Empty;
            }

            var buffer = new char[propertyName.Length];
            var count = 0;
            foreach (var c in propertyName)
            {
                if (char.IsLetterOrDigit(c))
                {
                    buffer[count++] = char.ToLowerInvariant(c);
                }
            }

            return count == 0 ? string.Empty : new string(buffer, 0, count);
        }

        private sealed partial class TypeDropdownEditorProperty : EditorProperty
        {
            private const string NoneOptionName = "<None>";
            private readonly string m_PropertyName;
            private readonly OptionButton m_OptionButton;
            private readonly string[] m_TypeNames;

            public TypeDropdownEditorProperty(string propertyName, Type interfaceType)
            {
                m_PropertyName = propertyName;
                m_TypeNames = BuildTypeNames(interfaceType);
                m_OptionButton = new OptionButton();
                m_OptionButton.SizeFlagsHorizontal = SizeFlags.ExpandFill;
                m_OptionButton.ItemSelected += OnItemSelected;
                AddChild(m_OptionButton);
                AddFocusable(m_OptionButton);
                RefreshItems();
            }

            public override void _UpdateProperty()
            {
                var selectedTypeName = GetEditedObject().Get(m_PropertyName).AsString();
                var selectedIndex = Array.IndexOf(m_TypeNames, selectedTypeName);
                if (selectedIndex < 0)
                {
                    selectedIndex = 0;
                }

                if (m_OptionButton.Selected != selectedIndex)
                {
                    m_OptionButton.Select(selectedIndex);
                }
            }

            private static string[] BuildTypeNames(Type interfaceType)
            {
                var typeNames = new List<string> { NoneOptionName };
                var runtimeTypeNames = Utility.Assembly.GetRuntimeTypeNames(interfaceType);
                runtimeTypeNames.Sort(StringComparer.Ordinal);
                typeNames.AddRange(runtimeTypeNames);
                return typeNames.ToArray();
            }

            private void RefreshItems()
            {
                m_OptionButton.Clear();
                for (var i = 0; i < m_TypeNames.Length; i++)
                {
                    m_OptionButton.AddItem(m_TypeNames[i]);
                }
            }

            private void OnItemSelected(long index)
            {
                var selectedTypeName = index <= 0 ? string.Empty : m_TypeNames[index];
                EmitChanged(m_PropertyName, selectedTypeName);
            }
        }
    }
}
#endif
