using GameFrameX.UI.Runtime;
using Godot;

namespace GameFrameX.UI.FairyGUI.Runtime
{
    /// <summary>
    /// FairyGUI 界面组辅助器。
    /// </summary>
    public partial class FairyGUIUIGroupHelper : UIGroupHelperBase
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

            var container = new Control
            {
                Name = groupName
            };
            SetFullScreen(container);
            container.MouseFilter = Control.MouseFilterEnum.Ignore;
            root.AddChild(container);
            m_Container = container;

            Name = "UIGroupHelper";
            container.AddChild(this);
            SetDepth(depth);
            return this;
        }

        private static void SetFullScreen(Control control)
        {
            if (control == null)
            {
                return;
            }

            control.LayoutMode = 3;
            control.AnchorsPreset = 15;
            control.AnchorRight = 1.0f;
            control.AnchorBottom = 1.0f;
            control.GrowHorizontal = Control.GrowDirection.Both;
            control.GrowVertical = Control.GrowDirection.Both;
            control.Position = Vector2.Zero;
        }
    }
}

