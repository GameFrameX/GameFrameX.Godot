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
using GameFrameX.Editor;
using GameFrameX.Procedure.Runtime;
using Godot;
using Type = System.Type;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GameFrameX.Procedure.Editor
{
    /// <summary>
    /// 流程组件检查器插件。
    /// </summary>
    [Tool]
    public partial class ProcedureComponentInspectorPlugin : ComponentTypeComponentInspector
    {
        /// <summary>
        /// 记录当前检查器实例命中的“可用流程”属性名，避免不同命名风格导致读写错位。
        /// </summary>
        private string m_AvailablePropertyName = "m_AvailableProcedureTypeNames";

        /// <summary>
        /// 记录当前检查器实例命中的“入口流程”属性名，确保入口与可用列表始终绑定到同一对象属性。
        /// </summary>
        private string m_EntrancePropertyName = "m_EntranceProcedureTypeName";

        protected override Type GetComponentType()
        {
            return typeof(ProcedureComponent);
        }

        protected override Type GetManagerType()
        {
            return typeof(IProcedureManager);
        }

        public override bool _ParseProperty(GodotObject @object, Variant.Type type, string name, PropertyHint hintType, string hintString, PropertyUsageFlags usageFlags, bool wide)
        {
            var normalized = NormalizeLower(name);
            if (normalized is "mavailableproceduretypenames" or "availableproceduretypenames" ||
                normalized.Contains("availableproceduretypenames", StringComparison.Ordinal))
            {
                // 命中可用流程字段后，使用自定义多选渲染替代默认数组编辑器。
                m_AvailablePropertyName = name;
                AddPropertyEditor(name, new ProcedureMultiSelectEditorProperty(name, m_EntrancePropertyName));
                return true;
            }

            var isEntranceProperty =
                normalized is "mentranceproceduretypename" or "entranceproceduretypename" ||
                normalized.Contains("entranceproceduretypename", StringComparison.Ordinal) ||
                (normalized.Contains("entrance", StringComparison.Ordinal) &&
                 normalized.Contains("procedure", StringComparison.Ordinal) &&
                 normalized.Contains("type", StringComparison.Ordinal));
            if (isEntranceProperty)
            {
                // 命中入口流程字段后，使用下拉渲染并绑定上一步识别到的可用流程字段名。
                m_EntrancePropertyName = name;
                AddPropertyEditor(name, new EntranceProcedureDropdownEditorProperty(name, m_AvailablePropertyName));
                return true;
            }

            return base._ParseProperty(@object, type, name, hintType, hintString, usageFlags, wide);
        }

        private static string NormalizeLower(string propertyName)
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

        private sealed partial class ProcedureMultiSelectEditorProperty : EditorProperty
        {
            private readonly string m_PropertyName;
            private readonly string m_EntrancePropertyName;
            private readonly VBoxContainer m_Root;
            private readonly Dictionary<string, CheckBox> m_Checks = new Dictionary<string, CheckBox>(StringComparer.Ordinal);
            private readonly string[] m_AllProcedureTypeNames;
            private readonly Label m_EmptyHintLabel;

            /// <summary>
            /// 初始化多选流程列表编辑器
            /// </summary>
            public ProcedureMultiSelectEditorProperty(string propertyName, string entrancePropertyName)
            {
                m_PropertyName = propertyName;
                m_EntrancePropertyName = entrancePropertyName;
                m_AllProcedureTypeNames = BuildAllProcedures();

                m_Root = new VBoxContainer();
                var title = new Label();
                title.Text = "Available Procedures";
                title.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
                m_Root.AddChild(title);

                foreach (var fullName in m_AllProcedureTypeNames)
                {
                    var cb = new CheckBox();
                    cb.Text = fullName;
                    cb.Toggled += OnToggled;
                    m_Checks[fullName] = cb;
                    m_Root.AddChild(cb);
                }

                m_EmptyHintLabel = new Label();
                m_EmptyHintLabel.Text = "There is no available procedure.";
                m_EmptyHintLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.7f, 0.2f));
                m_EmptyHintLabel.Visible = m_AllProcedureTypeNames.Length == 0;
                m_Root.AddChild(m_EmptyHintLabel);

                AddChild(m_Root);
            }

            public override void _UpdateProperty()
            {
                var selected = GetCurrentSelected();
                foreach (var pair in m_Checks)
                {
                    var should = selected.Contains(pair.Key);
                    if (pair.Value.ButtonPressed != should)
                    {
                        pair.Value.SetPressedNoSignal(should);
                    }
                }
            }

            private void OnToggled(bool _pressed)
            {
                var selected = new List<string>();
                foreach (var pair in m_Checks)
                {
                    if (pair.Value.ButtonPressed)
                    {
                        selected.Add(pair.Key);
                    }
                }

                selected.Sort(StringComparer.Ordinal);
                var selectedArray = selected.ToArray();
                var currentSelected = GetEditedObject().Get(m_PropertyName).AsStringArray() ?? Array.Empty<string>();
                var isAvailableChanged = !selectedArray.SequenceEqual(currentSelected, StringComparer.Ordinal);
                if (isAvailableChanged)
                {
                    EmitChanged(m_PropertyName, selectedArray);
                }

                var entrance = GetEditedObject().Get(m_EntrancePropertyName).AsString();
                var nextEntrance = entrance;
                if (!string.IsNullOrEmpty(entrance) && !selected.Contains(entrance))
                {
                    nextEntrance = selected.Count > 0 ? selected[0] : string.Empty;
                }

                // 入口值仅在实际变化时写回，避免无意义的属性写入日志刷屏。
                if (!string.Equals(entrance, nextEntrance, StringComparison.Ordinal))
                {
                    EmitChanged(m_EntrancePropertyName, nextEntrance);
                }
            }

            private static string[] BuildAllProcedures()
            {
                var names = GameFrameX.Runtime.Utility.Assembly.GetRuntimeTypeNames(typeof(ProcedureBase));
                names.Sort(StringComparer.Ordinal);
                return names.ToArray();
            }

            private HashSet<string> GetCurrentSelected()
            {
                var array = GetEditedObject().Get(m_PropertyName).AsStringArray();
                return array != null ? new HashSet<string>(array, StringComparer.Ordinal) : new HashSet<string>(StringComparer.Ordinal);
            }
        }

        private sealed partial class EntranceProcedureDropdownEditorProperty : EditorProperty
        {
            private const string NoneOptionName = "<None>";
            private readonly string m_PropertyName;
            private readonly string m_AvailablePropertyName;
            private readonly OptionButton m_OptionButton;
            private readonly Label m_EmptyHintLabel;

            /// <summary>
            /// 初始化入口流程下拉编辑器
            /// </summary>
            public EntranceProcedureDropdownEditorProperty(string propertyName, string availablePropertyName)
            {
                m_PropertyName = propertyName;
                m_AvailablePropertyName = availablePropertyName;

                var root = new VBoxContainer();
                var container = new HBoxContainer();
                var label = new Label();
                label.Text = "Entrance Procedure";
                label.SizeFlagsHorizontal = SizeFlags.ExpandFill;
                container.AddChild(label);

                m_OptionButton = new OptionButton();
                m_OptionButton.SizeFlagsHorizontal = SizeFlags.ExpandFill;
                m_OptionButton.ItemSelected += OnItemSelected;
                container.AddChild(m_OptionButton);

                root.AddChild(container);

                m_EmptyHintLabel = new Label();
                m_EmptyHintLabel.Text = "Select available procedures first.";
                m_EmptyHintLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.7f, 0.2f));
                root.AddChild(m_EmptyHintLabel);

                AddChild(root);
                AddFocusable(m_OptionButton);
                RefreshItems();
            }

            public override void _UpdateProperty()
            {
                RefreshItems();
                var selectedTypeName = GetEditedObject().Get(m_PropertyName).AsString();
                var idx = IndexOfItem(selectedTypeName);
                if (idx < 0)
                {
                    idx = 0;
                }
                if (m_OptionButton.Selected != idx)
                {
                    m_OptionButton.Select(idx);
                }
            }

            private void OnItemSelected(long index)
            {
                var text = m_OptionButton.GetItemText((int)index);
                var selectedTypeName = string.Equals(text, NoneOptionName, StringComparison.Ordinal) ? string.Empty : text;
                EmitChanged(m_PropertyName, selectedTypeName);
            }

            private void RefreshItems()
            {
                var editedObject = GetEditedObject();
                var available = Array.Empty<string>();
                if (editedObject != null)
                {
                    // 下拉候选只读取当前绑定的可用流程属性，确保与上方勾选列表完全同源。
                    available = editedObject.Get(m_AvailablePropertyName).AsStringArray() ?? Array.Empty<string>();
                }

                m_OptionButton.Clear();
                m_OptionButton.AddItem(NoneOptionName);
                foreach (var name in available.OrderBy(n => n, StringComparer.Ordinal))
                {
                    m_OptionButton.AddItem(name);
                }

                var hasAvailable = available.Length > 0;
                m_OptionButton.Disabled = !hasAvailable;
                m_EmptyHintLabel.Visible = !hasAvailable;
            }

            private int IndexOfItem(string typeName)
            {
                if (string.IsNullOrEmpty(typeName))
                {
                    return 0;
                }

                for (var i = 0; i < m_OptionButton.ItemCount; i++)
                {
                    if (string.Equals(m_OptionButton.GetItemText(i), typeName, StringComparison.Ordinal))
                    {
                        return i;
                    }
                }
                return -1;
            }
        }
    }
}
#endif
