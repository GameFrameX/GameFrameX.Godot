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
using GameFrameX.Event.Runtime;
using Godot;

namespace GameFrameX.Event.Editor
{
    /// <summary>
    /// 事件组件检查器插件。
    /// </summary>
    [Tool]
    public partial class EventComponentInspectorPlugin : ComponentTypeComponentInspector
    {
        protected override System.Type GetManagerType()
        {
            return typeof(IEventManager);
        }

        protected override System.Type GetComponentType()
        {
            return typeof(EventComponent);
        }
        //
        // public override void _ParseBegin(GodotObject @object)
        // {
        //     if (!(@object is EventComponent eventComponent))
        //     {
        //         return;
        //     }
        //
        //     var infoLabel = new Label();
        //     infoLabel.Text = "Event Component - Runtime Info";
        //     infoLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
        //     AddCustomControl(infoLabel);
        //
        //     if (!Engine.IsEditorHint())
        //     {
        //         var runtimeInfo = new Label();
        //         runtimeInfo.Text = $"Event Handler Count: {eventComponent.EventHandlerCount}\nEvent Count: {eventComponent.EventCount}";
        //         AddCustomControl(runtimeInfo);
        //     }
        //     else
        //     {
        //         var hintLabel = new Label();
        //         hintLabel.Text = "Available during runtime only.";
        //         hintLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));
        //         AddCustomControl(hintLabel);
        //     }
        // }
        // protected override bool IsCanHandle(GodotObject @object)
        // {
        //     GD.Print($"[事件检查器] 开始判断 IsCanHandle，对象类型={@object?.GetType().FullName}");
        //     if (@object is EventComponent)
        //     {
        //         GD.Print("[事件检查器] 命中路径A：对象本身就是 EventComponent，返回 true");
        //         return true;
        //     }
        //
        //     if (@object is not Node node)
        //     {
        //         GD.Print("[事件检查器] 未命中路径A：对象不是 Node，返回 false");
        //         return false;
        //     }
        //
        //     GD.Print($"[事件检查器] 进入路径B：检查选中节点脚本，节点名={node.Name}");
        //     var scriptVariant = node.GetScript();
        //     if (scriptVariant.Obj is CSharpScript cSharpScript)
        //     {
        //         var scriptClass = cSharpScript.GetClass();
        //         var scriptPath = cSharpScript.ResourcePath;
        //         GD.Print($"[事件检查器] 节点脚本信息：class={scriptClass}, path={scriptPath}");
        //
        //         if (IsMatchedEventComponentScript(cSharpScript, typeof(EventComponent), scriptPath))
        //         {
        //             return true;
        //         }
        //     }
        //     GD.Print("[事件检查器] 未命中路径B：当前节点脚本不是 EventComponent");
        //
        //     var childCount = node.GetChildCount(true);
        //     GD.Print($"[事件检查器] 进入路径C：检查子节点，数量={childCount}");
        //     for (var i = 0; i < childCount; i++)
        //     {
        //         var child = node.GetChild(i, true);
        //         GD.Print($"[事件检查器] 子节点[{i}] 名称={child.Name}, 类型={child.GetType().FullName}");
        //         if (child is EventComponent)
        //         {
        //             GD.Print($"[事件检查器] 命中路径C：子节点[{i}] 是 EventComponent，返回 true");
        //             return true;
        //         }
        //     }
        //
        //     GD.Print("[事件检查器] 路径A/B/C均未命中，返回 false");
        //     return false;
        // }
    }
}
#endif