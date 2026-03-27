// ==========================================================================================
//  GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//  GameFrameX organization and its derivative projects' copyrights, trademarks, patents, and related rights
//  均受中华人民共和国及相关国际法律法规保护。
//  are protected by the laws of the People's Republic of China and relevant international regulations.
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
//  or infringe upon the legitimate rights and interests of others, as prohibited by laws and regulations!
//  因基于本项目二次开发所产生的一切法律纠纷与责任，
//  Any legal disputes and liabilities arising from secondary development based on this project
//  本项目组织与贡献者概不承担。
//  shall be borne solely by the developer; the project organization and contributors assume no responsibility.
//
//  GitHub 仓库：https://github.com/GameFrameX
//  GitHub Repository: https://github.com/GameFrameX
//  Gitee  仓库：https://gitee.com/GameFrameX
//  Gitee Repository:  https://gitee.com/GameFrameX
//  官方文档：https://gameframex.doc.alianblank.com/
//  Official Documentation: https://gameframex.doc.alianblank.com/
// ==========================================================================================

using System.Collections.Generic;
using GameFrameX.Asset.Runtime;
using GameFrameX.ObjectPool;
using GameFrameX.Runtime;

namespace GameFrameX.UI.Runtime
{
    public abstract partial class BaseUIManager : GameFrameworkModule, IUIManager
    {
        /// <summary>
        /// 当前加载的界面实例对象池。
        /// </summary>
        protected readonly Dictionary<int, string> m_UIFormsBeingLoaded = new Dictionary<int, string>();

        /// <summary>
        /// 需要释放的界面实例对象池。
        /// </summary>
        protected readonly HashSet<int> m_UIFormsToReleaseOnLoad = new HashSet<int>();

        /// <summary>
        /// 待释放的界面实例队列。
        /// </summary>
        private Queue<IUIForm> m_RecycleQueue = new Queue<IUIForm>();

        /// <summary>
        /// 界面实例对象池回收间隔秒数。
        /// </summary>
        protected int m_RecycleInterval = 60;

        /// <summary>
        /// 界面实例对象池回收时间。
        /// </summary>
        protected float m_RecycleTime = 0;

        protected int m_Serial;

        /*/// <summary>
        /// 资源管理器。
        /// </summary>
        protected IAssetManager m_AssetManager;*/

        /// <summary>
        /// 界面辅助器。
        /// </summary>
        protected IUIFormHelper m_UIFormHelper;

        /// <summary>
        /// 对象池管理器。
        /// </summary>
        protected IObjectPoolManager m_ObjectPoolManager;

        /// <summary>
        /// 获取或设置界面实例对象池自动释放可释放对象的间隔秒数。
        /// </summary>
        public float InstanceAutoReleaseInterval
        {
            get { return m_InstancePool.AutoReleaseInterval; }
            set { m_InstancePool.AutoReleaseInterval = value; }
        }

        /// <summary>
        /// 获取或设置界面实例对象池的回收间隔秒数。
        /// </summary>
        public int RecycleInterval
        {
            get { return m_RecycleInterval; }
            set { m_RecycleInterval = value; }
        }

        /// <summary>
        /// 获取或设置界面实例对象池的容量。
        /// </summary>
        public int InstanceCapacity
        {
            get { return m_InstancePool.Capacity; }
            set { m_InstancePool.Capacity = value; }
        }

        private bool m_IsEnableUIHideAnimation = false;

        /// <summary>
        /// 获取或设置是否启用界面隐藏动画。
        /// </summary>
        public bool IsEnableUIHideAnimation
        {
            get { return m_IsEnableUIHideAnimation; }
            set { m_IsEnableUIHideAnimation = value; }
        }

        /// <summary>
        /// 获取或设置界面实例对象池对象过期秒数。
        /// </summary>
        public float InstanceExpireTime
        {
            get { return m_InstancePool.ExpireTime; }
            set { m_InstancePool.ExpireTime = value; }
        }


        /// <summary>
        /// 获取或设置是否启用界面显示动画。
        /// </summary>
        private bool m_IsEnableUIShowAnimation = false;

        /// <summary>
        /// 获取或设置是否启用界面显示动画。
        /// </summary>
        public bool IsEnableUIShowAnimation
        {
            get { return m_IsEnableUIShowAnimation; }
            set { m_IsEnableUIShowAnimation = value; }
        }

        protected IObjectPool<UIFormInstanceObject> m_InstancePool = null;
        protected bool m_IsShutdown = false;
        protected IUIFormShowHandler m_UIFormShowHandler;
        private IUIFormHideHandler m_UIFormHideHandler;

        /// <summary>
        /// 界面管理器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
            while (m_RecycleQueue.Count > 0)
            {
                var uiForm = m_RecycleQueue.Dequeue();
                RecycleUIForm(uiForm);
            }

            foreach (var uiGroup in m_UIGroups)
            {
                uiGroup.Value.Update(elapseSeconds, realElapseSeconds);
            }
        }

        /// <summary>
        /// 关闭并清理界面管理器。
        /// </summary>
        public override void Shutdown()
        {
            m_IsShutdown = true;
            CloseAllLoadedUIForms();
            m_UIGroups.Clear();
            m_UIFormsBeingLoaded.Clear();
            m_UIFormsToReleaseOnLoad.Clear();
            m_RecycleQueue.Clear();
        }

        /// <summary>
        /// 设置对象池管理器。
        /// </summary>
        /// <param name="objectPoolManager">对象池管理器。</param>
        public void SetObjectPoolManager(IObjectPoolManager objectPoolManager)
        {
            GameFrameworkGuard.NotNull(objectPoolManager, nameof(objectPoolManager));

            m_ObjectPoolManager = objectPoolManager;
            m_InstancePool = m_ObjectPoolManager.CreateMultiSpawnObjectPool<UIFormInstanceObject>("UI Instance Pool");
        }

        /*/// <summary>
        /// 设置资源管理器。
        /// </summary>
        /// <param name="assetManager">资源管理器。</param>
        public virtual void SetResourceManager(IAssetManager assetManager)
        {
            GameFrameworkGuard.NotNull(assetManager, nameof(assetManager));

            m_AssetManager = assetManager;
        }*/

        /// <summary>
        /// 设置界面辅助器。
        /// </summary>
        /// <param name="uiFormHelper">界面辅助器。</param>
        public void SetUIFormHelper(IUIFormHelper uiFormHelper)
        {
            GameFrameworkGuard.NotNull(uiFormHelper, nameof(uiFormHelper));

            m_UIFormHelper = uiFormHelper;
        }

        /// <summary>
        /// 设置界面显示处理接口
        /// </summary>
        /// <param name="handler">界面显示处理接口</param>
        public void SetUIFormShowHandler(IUIFormShowHandler handler)
        {
            m_UIFormShowHandler = handler;
        }

        /// <summary>
        /// 设置界面隐藏处理接口
        /// </summary>
        /// <param name="handler">界面隐藏处理接口</param>
        public void SetUIFormHideHandler(IUIFormHideHandler handler)
        {
            m_UIFormHideHandler = handler;
        }
    }
}
