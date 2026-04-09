using Godot;

namespace GameFrameX.Runtime
{
    /// <summary>
    /// 游戏对象帮助类
    /// </summary>
    public static class GameObjectHelper
    {
        /// <summary>
        /// 销毁子物体
        /// </summary>
        /// <param name="node">要销毁子物体的节点</param>
        public static void RemoveChildren(Node node)
        {
            foreach (Node child in node.GetChildren())
            {
                child.QueueFree();
            }
        }

        /// <summary>
        /// 销毁节点
        /// </summary>
        /// <param name="node">要销毁的节点</param>
        public static void DestroyObject(this Node node)
        {
            if (!ReferenceEquals(node, null))
            {
                node.QueueFree();
            }
        }

        /// <summary>
        /// 销毁节点
        /// </summary>
        /// <param name="node">要销毁的节点</param>
        public static void Destroy(Node node)
        {
            node.DestroyObject();
        }

        /// <summary>
        /// 在场景树中查找特定名称的节点。
        /// </summary>
        /// <param name="nodeName">节点名称。</param>
        /// <param name="rootNode">根节点，如果为null则使用场景根节点。</param>
        /// <returns>找到的节点，如果没有找到返回null。</returns>
        public static Node FindChildNodeByName(string nodeName, Node rootNode = null)
        {
            if (rootNode == null)
            {
                var sceneTree = Engine.GetMainLoop() as SceneTree;
                if (sceneTree != null && sceneTree.Root != null)
                {
                    rootNode = sceneTree.Root;
                }
                else
                {
                    return null;
                }
            }

            return FindChildNodeByNameRecursive(rootNode, nodeName);
        }

        private static Node FindChildNodeByNameRecursive(Node parent, string name)
        {
            foreach (Node child in parent.GetChildren())
            {
                if (child.Name == name)
                {
                    return child;
                }

                var result = FindChildNodeByNameRecursive(child, name);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// 根据节点名称查询子对象
        /// </summary>
        /// <param name="node">父节点</param>
        /// <param name="name">子节点名称</param>
        /// <returns>找到的节点</returns>
        public static Node FindChildNodeByName(Node node, string name)
        {
            return node.FindChild(name, true, false);
        }

        /// <summary>
        /// 创建节点
        /// </summary>
        /// <param name="parent">父节点</param>
        /// <param name="name">节点名称</param>
        /// <returns>创建的节点</returns>
        public static Node Create(Node parent, string name)
        {
            if (parent == null)
            {
                throw new System.ArgumentNullException(nameof(parent));
            }

            var node = new Node();
            node.Name = name;
            parent.AddChild(node);
            return node;
        }

        /// <summary>
        /// 重置节点的变换数据
        /// </summary>
        /// <param name="node">要重置的节点</param>
        public static void ResetTransform(Node node)
        {
            if (node is Node2D node2D)
            {
                node2D.Scale = new Vector2(1, 1);
                node2D.Position = new Vector2(0, 0);
                node2D.RotationDegrees = 0;
            }
            else if (node is Node3D node3D)
            {
                node3D.Scale = new Vector3(1, 1, 1);
                node3D.Position = new Vector3(0, 0, 0);
                node3D.RotationDegrees = new Vector3(0, 0, 0);
            }
        }

        /// <summary>
        /// 设置对象的显示排序层
        /// Godot uses Z-index and canvas layers instead of sorting layers
        /// </summary>
        /// <param name="node">节点</param>
        /// <param name="zIndex">Z轴索引</param>
        public static void SetZIndex(Node node, int zIndex)
        {
            if (node is Node2D node2D)
            {
                node2D.ZIndex = zIndex;
            }
        }

        /// <summary>
        /// 设置对象的层
        /// In Godot, use collision layers/masks for physics or canvas layers for rendering
        /// </summary>
        /// <param name="node">节点</param>
        /// <param name="layer">层</param>
        /// <param name="children">是否设置子物体</param>
        public static void SetLayer(Node node, int layer, bool children = true)
        {
                // Godot and this runtime use different layer/tag model
            // For physics, use Collision layers/masks
            // For rendering, use Canvas layers
            // This is a placeholder for compatibility
            if (children)
            {
                foreach (Node child in node.GetChildren())
                {
                    SetLayer(child, layer, true);
                }
            }
        }
    }
}
