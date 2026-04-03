using System;
using Godot;

namespace GameFrameX.UI.GDGUI.Runtime
{
    /// <summary>
    /// GDGUI 裁剪辅助器，防止 AOT/裁剪丢失类型引用。
    /// </summary>
    public partial class GameFrameXUIGDGUICroppingHelper : Node
    {
        private Type[] m_Types;

        /// <summary>
        /// 缓存关键类型引用。
        /// </summary>
        public override void _Ready()
        {
            m_Types = new[]
            {
                typeof(ControlExtension),
                typeof(UIManager),
                typeof(UGUIFormHelper),
                typeof(UGUI),
                typeof(UGUIButtonExtension),
                typeof(UGUIElementPropertyAttribute),
                typeof(UGUIUIGroupHelper),
                typeof(UIImage),
                typeof(UGUIImageExtension),
            };
        }
    }
}
