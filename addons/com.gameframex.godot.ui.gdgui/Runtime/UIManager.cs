using GameFrameX.UI.Runtime;

namespace GameFrameX.UI.GDGUI.Runtime
{
    /// <summary>
    /// GDGUI 界面管理器。
    /// </summary>
    internal sealed partial class UIManager : BaseUIManager
    {
        /// <summary>
        /// 初始化管理器默认状态。
        /// </summary>
        public UIManager()
        {
            // m_AssetManager = null;
            m_InstancePool = null;
            m_UIFormHelper = null;
            m_Serial = 0;
            m_RecycleTime = 0;
            if (m_RecycleInterval < 10)
            {
                m_RecycleInterval = 10;
            }

            m_IsShutdown = false;
            m_OpenUIFormSuccessEventHandler = null;
            m_OpenUIFormFailureEventHandler = null;
            m_CloseUIFormCompleteEventHandler = null;
        }
    }
}
