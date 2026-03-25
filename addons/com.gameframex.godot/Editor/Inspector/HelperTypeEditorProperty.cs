// ==========================================================================================
//   GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//   GameFrameX organization and its derivative projects' copyrights, trademarks, patents, and related rights
//   均受中华人民共和国及相关国际法律法规保护。
//   are protected by the laws of the People's Republic of China and relevant international regulations.
//   使用本项目须严格遵守相应法律法规及开源许可证之规定。
//   Usage of this project must strictly comply with applicable laws, regulations, and open-source licenses.
//   本项目采用 MIT 许可证与 Apache License 2.0 双许可证分发，
//   This project is dual-licensed under the MIT License and Apache License 2.0,
//   完整许可证文本请参见源代码根目录下的 LICENSE 文件。
//   please refer to the LICENSE file in the root directory of the source code for the full license text.
//   禁止利用本项目实施任何危害国家安全、破坏社会秩序、
//   It is prohibited to use this project to engage in any activities that endanger national security, disrupt social order,
//   侵犯他人合法权益等法律法规所禁止的行为！
//   or infringe upon the legitimate rights and interests of others, as prohibited by laws and regulations!
//   因基于本项目二次开发所产生的一切法律纠纷与责任，
//   Any legal disputes and liabilities arising from secondary development based on this project
//   本项目组织与贡献者概不承担。
//   shall be borne solely by the developer; the project organization and contributors assume no responsibility.
//   GitHub 仓库：https://github.com/GameFrameX
//   GitHub Repository: https://github.com/GameFrameX
//   Gitee  仓库：https://gitee.com/GameFrameX
//   Gitee Repository:  https://gitee.com/GameFrameX
//   CNB  仓库：https://cnb.cool/GameFrameX
//   CNB Repository:  https://cnb.cool/GameFrameX
//   官方文档：https://gameframex.doc.alianblank.com/
//   Official Documentation: https://gameframex.doc.alianblank.com/
//  ==========================================================================================

using System;
using System.Collections.Generic;
using GameFrameX.Runtime;
using Godot;

namespace GameFrameX.Editor;

/// <summary>
/// Helper 类型属性的自定义编辑器，提供下拉列表选择实现类型。
/// </summary>
/// <remarks>
/// Custom editor for Helper type properties that provides a dropdown list to select implementation types.
/// </remarks>
public partial class HelperTypeEditorProperty : EditorProperty
{
    /// <summary>
    /// 无选项的常量名称，用于表示未选择任何实现类型。
    /// </summary>
    public const string NoneOptionName = "<None>";

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