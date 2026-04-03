using GameFrameX.UI.Runtime;

namespace GameFrameX.UI.GDGUI.Runtime
{
    /// <summary>
    /// GDGUI 界面管理器关闭逻辑。
    /// </summary>
    internal sealed partial class UIManager
    {
        /// <summary>
        /// 回收界面实例对象。
        /// </summary>
        /// <param name="uiForm">界面实例。</param>
        /// <param name="isDispose">是否立即销毁。</param>
        protected override void RecycleUIForm(IUIForm uiForm, bool isDispose = false)
        {
            uiForm?.OnRecycle();
            if (uiForm?.Handle == null)
            {
                return;
            }

            m_InstancePool.Unspawn(uiForm.Handle);
            if (isDispose)
            {
                m_InstancePool.ReleaseObject(uiForm.Handle);
            }
        }
    }
}
