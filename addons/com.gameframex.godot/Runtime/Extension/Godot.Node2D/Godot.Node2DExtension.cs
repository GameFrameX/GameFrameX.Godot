// ==========================================================================================
//  GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//  GameFrameX organization and its derivative projects' copyrights, trademarks, patents, and related rights
//  均受中华人民共和国及相关国际法律法规保护。
//  are protected by the laws of the People's Republic of China and related international regulations.
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
//  or infringe upon the legitimate rights and interests of others as prohibited by laws and regulations.
//  因基于本项目二次开发所产生的一切法律纠纷与责任，
//  Any legal disputes or liabilities arising from secondary development based on this project
//  本项目组织与贡献者概不承担。
//  shall be borne solely by the developer; the project organization and contributors assume no responsibility.
// 
//  GitHub 仓库：https://github.com/GameFrameX
//  GitHub Repository:  https://github.com/GameFrameX
//  Gitee  仓库：https://gitee.com/GameFrameX
//  Gitee Repository:  https://gitee.com/GameFrameX
//  官方文档：https://gameframex.doc.alianblank.com/
//  Official Documentation: https://gameframex.doc.alianblank.com/
// ==========================================================================================

// 迁移备注：映射自 Unity 基准 com.gameframex.unity/Runtime/Extension/UnityEngine.Transform/UnityEngine.TransformExtension.cs 中的 LookAt2D。
// Unity Transform(3D) + Quaternion.LookRotation 的 2D 朝向语义，在 Godot 侧由 Node2D.LookAt 原生覆盖。

using Godot;

namespace GameFrameX.Runtime
{
    /// <summary>
    /// Node2D 扩展方法
    /// </summary>
    public static class GodotNode2DExtension
    {
        /// <summary>
        /// 让 2D 节点朝向指定点。
        /// </summary>
        /// <remarks>
        /// Makes the 2D node face the specified point.
        /// 对应 Unity 的 Transform.LookAt2D：原地朝向目标点。
        /// </remarks>
        /// <param name="node">目标节点 / The target node</param>
        /// <param name="lookAtPoint2D">朝向的目标点 / The point to look at</param>
        public static void LookAt2D(this Node2D node, Vector2 lookAtPoint2D)
        {
            if (node.GlobalPosition == lookAtPoint2D)
            {
                return;
            }

            node.LookAt(lookAtPoint2D);
        }
    }
}
