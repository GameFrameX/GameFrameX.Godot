using GameFrameX.UI.Runtime;
using Godot;

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

            object target = uiForm.Handle;
            try
            {
                m_InstancePool.Unspawn(target);
            }
            catch (GameFrameX.Runtime.GameFrameworkException)
            {
                if (target is Node uiNode && uiNode.HasNode("ViewRoot"))
                {
                    var viewRoot = uiNode.GetNode("ViewRoot");
                    m_InstancePool.Unspawn(viewRoot);
                    target = viewRoot;
                }
                else
                {
                    throw;
                }
            }

            if (isDispose)
            {
                m_InstancePool.ReleaseObject(target);
            }
        }
    }
}
