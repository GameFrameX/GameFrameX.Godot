using GameFrameX.UI.Runtime;
using Godot;

namespace GameFrameX.UI.GDGUI.Runtime
{
    /// <summary>
    /// GDGUI 界面组辅助器。
    /// </summary>
    public partial class GDGUIUIGroupHelper : UIGroupHelperBase
    {
        private int m_Depth;
        private Control m_Container;

        /// <summary>
        /// 获取界面组深度。
        /// </summary>
        public override int Depth
        {
            get { return m_Depth; }
            protected set { m_Depth = value; }
        }

        /// <summary>
        /// 设置界面组深度。
        /// </summary>
        /// <param name="depth">界面组深度。</param>
        public override void SetDepth(int depth)
        {
            m_Depth = depth;
            if (m_Container != null)
            {
                m_Container.ZIndex = depth;
            }
        }

        /// <summary>
        /// 创建并挂载界面组辅助器。
        /// </summary>
        /// <param name="root">UI 根节点。</param>
        /// <param name="groupName">界面组名称。</param>
        /// <param name="uiGroupHelperTypeName">界面组辅助器类型名。</param>
        /// <param name="customUIGroupHelper">自定义界面组辅助器。</param>
        /// <param name="depth">界面组深度。</param>
        /// <returns>界面组辅助器实例。</returns>
        public override IUIGroupHelper Handler(Node root, string groupName, string uiGroupHelperTypeName, IUIGroupHelper customUIGroupHelper, int depth = 0)
        {
            if (root == null)
            {
                return null;
            }

            // 对齐 Unity UGUIUIGroupHelper：建组时把 UIComponent.DesignResolution 应用到显示层。
            ApplyDesignResolution(FindUIComponent(root)?.DesignResolution);

            var container = new Control
            {
                Name = groupName
            };
            container.MakeFullScreen();
            container.MouseFilter = Control.MouseFilterEnum.Ignore;
            root.AddChild(container);
            m_Container = container;

            Name = "UIGroupHelper";
            container.AddChild(this);
            SetDepth(depth);
            return this;
        }

        /// <summary>
        /// 沿父链查找所属界面组件。
        /// </summary>
        /// <param name="root">UI 根节点。</param>
        /// <returns>所属界面组件，未找到返回 null。</returns>
        private static UIComponent FindUIComponent(Node root)
        {
            var current = root;
            while (current != null)
            {
                if (current is UIComponent component)
                {
                    return component;
                }

                current = current.GetParent();
            }

            return null;
        }

        /// <summary>
        /// 把设计分辨率配置应用到 GDGUI 显示层。
        /// </summary>
        /// <param name="designResolution">设计分辨率组件，可为 null。</param>
        private static void ApplyDesignResolution(UIDesignResolutionComponent designResolution)
        {
            if (designResolution == null)
            {
                return;
            }

            // 等价 Unity 侧 CanvasScaler：配置写入 Window content scale，GDGUI 控件随 Viewport 拉伸自动适配。
            // ReferencePixelsPerUnit 在 Godot 无对应机制，仅作为配置保留在组件上。
            designResolution.Apply();
        }
    }
}
