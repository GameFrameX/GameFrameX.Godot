using Godot;

namespace GameFrameX.UI.GDGUI.Runtime
{
    /// <summary>
    /// 控件扩展方法。
    /// </summary>
    public static class ControlExtension
    {
        /// <summary>
        /// 设置控件全屏拉伸到父节点。
        /// </summary>
        /// <param name="control">目标控件。</param>
        public static void MakeFullScreen(this Control control)
        {
            if (control == null)
            {
                return;
            }

            control.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            control.SetOffsetsPreset(Control.LayoutPreset.FullRect, Control.LayoutPresetMode.Minsize, 0);
        }
    }
}
