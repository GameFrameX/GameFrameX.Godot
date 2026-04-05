using System;
using GameFrameX.UI.Runtime;
using Godot;

namespace GameFrameX.UI.GDGUI.Runtime
{
    /// <summary>
    /// GDGUI 界面基类。
    /// </summary>
    public partial class GDGUI : UIForm
    {
        /// <summary>
        /// 执行界面显示。
        /// </summary>
        /// <param name="handler">显示处理器。</param>
        /// <param name="complete">完成回调。</param>
        public override void Show(IUIFormShowHandler handler, Action complete)
        {
            if (handler != null)
            {
                handler.Handler(Handle, EnableShowAnimation, ShowAnimationName, complete);
                return;
            }

            complete?.Invoke();
        }

        /// <summary>
        /// 执行界面隐藏。
        /// </summary>
        /// <param name="handler">隐藏处理器。</param>
        /// <param name="complete">完成回调。</param>
        public override void Hide(IUIFormHideHandler handler, Action complete)
        {
            if (handler != null)
            {
                handler.Handler(Handle, EnableHideAnimation, HideAnimationName, complete);
                return;
            }

            complete?.Invoke();
        }

        /// <summary>
        /// 设置界面可见性。
        /// </summary>
        /// <param name="visible">是否可见。</param>
        protected override void InternalSetVisible(bool visible)
        {
            var currentVisible = ((CanvasItem)this).Visible;
            if (currentVisible == visible)
            {
                return;
            }

            ((CanvasItem)this).Visible = visible;
        }

        /// <summary>
        /// 设置界面全屏。
        /// </summary>
        protected internal override void MakeFullScreen()
        {
            ControlExtension.MakeFullScreen(this);
        }
    }
}
