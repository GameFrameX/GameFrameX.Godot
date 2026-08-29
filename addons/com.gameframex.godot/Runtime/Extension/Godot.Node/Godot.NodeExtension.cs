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

// 迁移备注：映射自 Unity 基准 com.gameframex.unity/Runtime/Extension/UnityEngine.GameObject/UnityEngine.GameObjectExtension.cs。
// UnityEngine.GameObject -> Godot.Node、UnityEngine.Component -> Godot.Node（Godot 无组件模型，以子节点近似）。
// 基准中的 SetLayerRecursively 在 Godot 无等价概念（Node 无 layer；物理层挂在 CollisionObject 上），备案不迁移。

using System;
using Godot;

namespace GameFrameX.Runtime
{
    /// <summary>
    /// Node 扩展方法
    /// </summary>
    public static class GodotNodeExtension
    {
        /// <summary>
        /// 销毁指定类型的第一个子节点。
        /// </summary>
        /// <remarks>
        /// Destroys the first child node of the specified type.
        /// ponytail: Unity 的 Component 概念在 Godot 无直接等价，此处以"同类型子节点"近似；
        /// 若业务需要精确组件语义，应改为显式的子节点管理。
        /// </remarks>
        /// <typeparam name="T">要销毁的节点类型 / The type of the node to destroy</typeparam>
        /// <param name="node">目标节点 / The target node</param>
        public static void DestroyComponent<T>(this Node node) where T : Node
        {
            foreach (var child in node.GetChildren())
            {
                if (child is T matched)
                {
                    node.RemoveChild(matched);
                    matched.QueueFree();
                    return;
                }
            }
        }

        /// <summary>
        /// 获取或添加指定类型的子节点。
        /// </summary>
        /// <remarks>
        /// Gets the child node of the specified type if it exists, otherwise creates, adds and returns a new one.
        /// ponytail: Unity 的 GetComponent/AddComponent 语义在 Godot 以"查找/新建子节点"近似。
        /// </remarks>
        /// <typeparam name="T">要获取或添加的节点类型 / The type of the node to get or add</typeparam>
        /// <param name="node">目标节点 / The target node</param>
        /// <returns>获取或添加的子节点 / The existing or newly added child node</returns>
        public static T GetOrAddComponent<T>(this Node node) where T : Node, new()
        {
            foreach (var child in node.GetChildren())
            {
                if (child is T existing)
                {
                    return existing;
                }
            }

            var created = new T();
            node.AddChild(created);
            return created;
        }

        /// <summary>
        /// 获取或添加指定类型的子节点。
        /// </summary>
        /// <remarks>
        /// Gets the child node of the specified type if it exists, otherwise creates, adds and returns a new one.
        /// </remarks>
        /// <param name="node">目标节点 / The target node</param>
        /// <param name="type">要获取或添加的节点类型，必须是 Node 派生类型 / The type of the node to get or add, must derive from Node</param>
        /// <returns>获取或添加的子节点 / The existing or newly added child node</returns>
        public static Node GetOrAddComponent(this Node node, Type type)
        {
            foreach (var child in node.GetChildren())
            {
                if (type.IsInstanceOfType(child))
                {
                    return child;
                }
            }

            var created = (Node)Activator.CreateInstance(type);
            node.AddChild(created);
            return created;
        }

        /// <summary>
        /// 获取节点是否在场景树中。
        /// </summary>
        /// <remarks>
        /// Checks whether the node is inside the scene tree.
        /// ponytail: Unity InScene 判断"是场景实例而非预制体资产"；Godot 以 IsInsideTree 近似（未入树≈未实例化到场景）。
        /// </remarks>
        /// <param name="node">目标节点 / The target node</param>
        /// <returns>是否在场景树中 / Whether the node is inside the scene tree</returns>
        public static bool InScene(this Node node)
        {
            return node.IsInsideTree();
        }
    }
}
