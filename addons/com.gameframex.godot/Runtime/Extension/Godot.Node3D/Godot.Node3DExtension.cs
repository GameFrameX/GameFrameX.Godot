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

// 迁移备注：映射自 Unity 基准 com.gameframex.unity/Runtime/Extension/UnityEngine.Transform/UnityEngine.TransformExtension.cs（除 LookAt2D，其移至 GodotNode2DExtension）。
// 映射关系：UnityEngine.Transform -> Godot.Node3D；
//   transform.position -> Node3D.GlobalPosition，transform.localPosition -> Node3D.Position，transform.localScale -> Node3D.Scale。
// 坐标系差异按仓内迁移惯例（见 PositionHelper）：直接使用 Godot 原生坐标分量 X/Y/Z，不做左手系/右手系轴向翻转。

using Godot;

namespace GameFrameX.Runtime
{
    /// <summary>
    /// Node3D 扩展方法
    /// </summary>
    public static class GodotNode3DExtension
    {
        /// <summary>
        /// 设置绝对位置的 X 坐标。
        /// </summary>
        /// <param name="node">目标节点。</param>
        /// <param name="newValue">X 坐标值。</param>
        public static void SetPositionX(this Node3D node, float newValue)
        {
            Vector3 v = node.GlobalPosition;
            v.X = newValue;
            node.GlobalPosition = v;
        }

        /// <summary>
        /// 设置绝对位置的 Y 坐标。
        /// </summary>
        /// <param name="node">目标节点。</param>
        /// <param name="newValue">Y 坐标值。</param>
        public static void SetPositionY(this Node3D node, float newValue)
        {
            Vector3 v = node.GlobalPosition;
            v.Y = newValue;
            node.GlobalPosition = v;
        }

        /// <summary>
        /// 设置绝对位置的 Z 坐标。
        /// </summary>
        /// <param name="node">目标节点。</param>
        /// <param name="newValue">Z 坐标值。</param>
        public static void SetPositionZ(this Node3D node, float newValue)
        {
            Vector3 v = node.GlobalPosition;
            v.Z = newValue;
            node.GlobalPosition = v;
        }

        /// <summary>
        /// 增加绝对位置的 X 坐标。
        /// </summary>
        /// <param name="node">目标节点。</param>
        /// <param name="deltaValue">X 坐标值增量。</param>
        public static void AddPositionX(this Node3D node, float deltaValue)
        {
            Vector3 v = node.GlobalPosition;
            v.X += deltaValue;
            node.GlobalPosition = v;
        }

        /// <summary>
        /// 增加绝对位置的 Y 坐标。
        /// </summary>
        /// <param name="node">目标节点。</param>
        /// <param name="deltaValue">Y 坐标值增量。</param>
        public static void AddPositionY(this Node3D node, float deltaValue)
        {
            Vector3 v = node.GlobalPosition;
            v.Y += deltaValue;
            node.GlobalPosition = v;
        }

        /// <summary>
        /// 增加绝对位置的 Z 坐标。
        /// </summary>
        /// <param name="node">目标节点。</param>
        /// <param name="deltaValue">Z 坐标值增量。</param>
        public static void AddPositionZ(this Node3D node, float deltaValue)
        {
            Vector3 v = node.GlobalPosition;
            v.Z += deltaValue;
            node.GlobalPosition = v;
        }

        /// <summary>
        /// 设置相对位置的 X 坐标。
        /// </summary>
        /// <param name="node">目标节点。</param>
        /// <param name="newValue">X 坐标值。</param>
        public static void SetLocalPositionX(this Node3D node, float newValue)
        {
            Vector3 v = node.Position;
            v.X = newValue;
            node.Position = v;
        }

        /// <summary>
        /// 设置相对位置的 Y 坐标。
        /// </summary>
        /// <param name="node">目标节点。</param>
        /// <param name="newValue">Y 坐标值。</param>
        public static void SetLocalPositionY(this Node3D node, float newValue)
        {
            Vector3 v = node.Position;
            v.Y = newValue;
            node.Position = v;
        }

        /// <summary>
        /// 设置相对位置的 Z 坐标。
        /// </summary>
        /// <param name="node">目标节点。</param>
        /// <param name="newValue">Z 坐标值。</param>
        public static void SetLocalPositionZ(this Node3D node, float newValue)
        {
            Vector3 v = node.Position;
            v.Z = newValue;
            node.Position = v;
        }

        /// <summary>
        /// 增加相对位置的 X 坐标。
        /// </summary>
        /// <param name="node">目标节点。</param>
        /// <param name="deltaValue">X 坐标值增量。</param>
        public static void AddLocalPositionX(this Node3D node, float deltaValue)
        {
            Vector3 v = node.Position;
            v.X += deltaValue;
            node.Position = v;
        }

        /// <summary>
        /// 增加相对位置的 Y 坐标。
        /// </summary>
        /// <param name="node">目标节点。</param>
        /// <param name="deltaValue">Y 坐标值增量。</param>
        public static void AddLocalPositionY(this Node3D node, float deltaValue)
        {
            Vector3 v = node.Position;
            v.Y += deltaValue;
            node.Position = v;
        }

        /// <summary>
        /// 增加相对位置的 Z 坐标。
        /// </summary>
        /// <param name="node">目标节点。</param>
        /// <param name="deltaValue">Z 坐标值增量。</param>
        public static void AddLocalPositionZ(this Node3D node, float deltaValue)
        {
            Vector3 v = node.Position;
            v.Z += deltaValue;
            node.Position = v;
        }

        /// <summary>
        /// 设置相对尺寸的 X 分量。
        /// </summary>
        /// <param name="node">目标节点。</param>
        /// <param name="newValue">X 分量值。</param>
        public static void SetLocalScaleX(this Node3D node, float newValue)
        {
            Vector3 v = node.Scale;
            v.X = newValue;
            node.Scale = v;
        }

        /// <summary>
        /// 设置相对尺寸的 Y 分量。
        /// </summary>
        /// <param name="node">目标节点。</param>
        /// <param name="newValue">Y 分量值。</param>
        public static void SetLocalScaleY(this Node3D node, float newValue)
        {
            Vector3 v = node.Scale;
            v.Y = newValue;
            node.Scale = v;
        }

        /// <summary>
        /// 设置相对尺寸的 Z 分量。
        /// </summary>
        /// <param name="node">目标节点。</param>
        /// <param name="newValue">Z 分量值。</param>
        public static void SetLocalScaleZ(this Node3D node, float newValue)
        {
            Vector3 v = node.Scale;
            v.Z = newValue;
            node.Scale = v;
        }

        /// <summary>
        /// 增加相对尺寸的 X 分量。
        /// </summary>
        /// <param name="node">目标节点。</param>
        /// <param name="deltaValue">X 分量增量。</param>
        public static void AddLocalScaleX(this Node3D node, float deltaValue)
        {
            Vector3 v = node.Scale;
            v.X += deltaValue;
            node.Scale = v;
        }

        /// <summary>
        /// 增加相对尺寸的 Y 分量。
        /// </summary>
        /// <param name="node">目标节点。</param>
        /// <param name="deltaValue">Y 分量增量。</param>
        public static void AddLocalScaleY(this Node3D node, float deltaValue)
        {
            Vector3 v = node.Scale;
            v.Y += deltaValue;
            node.Scale = v;
        }

        /// <summary>
        /// 增加相对尺寸的 Z 分量。
        /// </summary>
        /// <param name="node">目标节点。</param>
        /// <param name="deltaValue">Z 分量增量。</param>
        public static void AddLocalScaleZ(this Node3D node, float deltaValue)
        {
            Vector3 v = node.Scale;
            v.Z += deltaValue;
            node.Scale = v;
        }

        /// <summary>
        /// 设置父节点并重置本地变换。
        /// </summary>
        /// <remarks>
        /// Sets the parent node and resets the local transform.
        /// 对应 Unity 的 SetParent + Reset：先 Reparent 到父节点（Godot 无 SetParent，用 Reparent 等价），再将本地变换归零。
        /// </remarks>
        /// <param name="node">目标节点。</param>
        /// <param name="parent">父节点。</param>
        public static void SetParentAndReset(this Node3D node, Node parent)
        {
            node.Reparent(parent);
            node.Reset();
        }

        /// <summary>
        /// 重置本地变换（位置归零、旋转归零、缩放归一）。
        /// </summary>
        /// <remarks>
        /// Resets the local transform (position to zero, rotation to identity, scale to one).
        /// 对应 Unity 的 transform.Reset()。
        /// </remarks>
        /// <param name="node">目标节点。</param>
        public static void Reset(this Node3D node)
        {
            node.Position = Vector3.Zero;
            node.Rotation = Vector3.Zero;
            node.Scale = Vector3.One;
        }
    }
}
