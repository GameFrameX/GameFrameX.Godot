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

using System;
using System.Collections.Generic;
using GameFrameX.Event.Runtime;
using GameFrameX.Localization.Runtime;
using GameFrameX.Runtime;
using Godot;

namespace GameFrameX.UI.Runtime
{
    /// <summary>
    /// 界面。
    /// </summary>
    public abstract partial class UIForm : Control, IUIForm
    {
        private bool m_Available = false;
        private bool m_Visible = false;
        private bool m_IsInit = false;
        private bool m_IsDisableRecycling = false;
        private bool m_IsCenter = false;
        private bool m_IsDisableClosing = false;
        private int m_SerialId;
        private int m_OriginalLayer = 0;
        private string m_UIFormAssetName;
        private string m_AssetPath;
        private int m_DepthInUIGroup;
        private bool m_PauseCoveredUIForm;
        private string m_FullName;
        private bool m_EnableShowAnimation;
        private string m_ShowAnimationName;
        private bool m_EnableHideAnimation;
        private string m_HideAnimationName;
        private IUIGroup m_UIGroup;
        private UIEventSubscriber m_EventSubscriber = null;
        private object m_UserData = null;

        /// <summary>
        /// 获取用户自定义数据。
        /// </summary>
        public object UserData
        {
            get { return m_UserData; }
        }

        /// <summary>
        /// 获取界面事件订阅器。
        /// </summary>
        protected UIEventSubscriber EventSubscriber
        {
            get { return m_EventSubscriber; }
        }

        /// <summary>
        /// 获取界面是否来自对象池。
        /// </summary>
        protected bool IsFromPool { get; set; }

        /// <summary>
        /// 获取界面是否已被销毁。
        /// </summary>
        protected bool IsDisposed { get; set; }

        /// <summary>
        /// 界面回收开始时间
        /// </summary>
        public DateTime ReleaseStartTime { get; private set; } = DateTime.MaxValue;

        /// <summary>
        /// 获取界面序列编号。
        /// </summary>
        public int SerialId
        {
            get { return m_SerialId; }
        }

        /// <summary>
        /// 获取界面完整名称。
        /// </summary>
        public string FullName
        {
            get { return m_FullName; }
        }

        /// <summary>
        /// 获取或设置界面名称。
        /// </summary>
        public new string Name
        {
            get { return this.Name; }
            set { this.Name = value; }
        }

        /// <summary>
        /// 获取界面是否可用。
        /// </summary>
        public bool Available
        {
            get { return m_Available; }
        }

        /// <summary>
        /// 是否启用显示动画
        /// </summary>
        public bool EnableShowAnimation
        {
            get { return m_EnableShowAnimation; }
            set { m_EnableShowAnimation = value; }
        }

        /// <summary>
        /// 显示动画名称
        /// </summary>
        public string ShowAnimationName
        {
            get { return m_ShowAnimationName; }
            set { m_ShowAnimationName = value; }
        }

        /// <summary>
        /// 是否启用隐藏动画
        /// </summary>
        public bool EnableHideAnimation
        {
            get { return m_EnableHideAnimation; }
            set { m_EnableHideAnimation = value; }
        }

        /// <summary>
        /// 隐藏动画名称
        /// </summary>
        public string HideAnimationName
        {
            get { return m_HideAnimationName; }
            set { m_HideAnimationName = value; }
        }

        /// <summary>
        /// 获取或设置界面是否可见。
        /// </summary>
        public new virtual bool Visible
        {
            get { return m_Available && m_Visible; }
            protected set
            {
                if (!m_Available)
                {
                    Log.Warning("UI form '{0}' is not available.", Name);
                    return;
                }

                if (m_Visible == value)
                {
                    return;
                }

                m_Visible = value;
                InternalSetVisible(value);
            }
        }

        /// <summary>
        /// 获取界面资源名称。
        /// </summary>
        public string UIFormAssetName
        {
            get { return m_UIFormAssetName; }
        }

        /// <summary>
        /// 是否禁用回收，禁用回收的界面不会被回收
        /// </summary>
        public bool IsDisableRecycling
        {
            get { return m_IsDisableRecycling; }
            protected set { m_IsDisableRecycling = value; }
        }

        /// <summary>
        /// 是否禁用关闭，禁用关闭的界面不会被关闭
        /// </summary>
        public bool IsDisableClosing
        {
            get { return m_IsDisableClosing; }
            protected set { m_IsDisableClosing = value; }
        }

        /// <summary>
        /// 是否可以回收，true:界面可以被回收，false:界面不可以被回收
        /// </summary>
        public bool IsCanRecycle
        {
            get
            {
                if (m_IsDisableRecycling)
                {
                    return false;
                }

                return (DateTime.Now - ReleaseStartTime).TotalSeconds >= RecycleInterval;
            }
        }

        /// <summary>
        /// 界面回收间隔，单位：秒
        /// </summary>
        public int RecycleInterval { get; private set; }

        /// <summary>
        /// 是否开启组件居中，true:组件生成后默认父组件居中
        /// </summary>
        public bool IsCenter
        {
            get { return m_IsCenter; }
            protected set { m_IsCenter = value; }
        }

        /// <summary>
        /// 获取界面资源名称。
        /// </summary>
        public string AssetPath
        {
            get { return m_AssetPath; }
            protected set { m_AssetPath = value; }
        }

        /// <summary>
        /// 获取界面实例。
        /// </summary>
        public object Handle
        {
            get { return this; }
        }

        /// <summary>
        /// 获取界面所属的界面组。
        /// </summary>
        public virtual IUIGroup UIGroup
        {
            get { return m_UIGroup; }
            set { m_UIGroup = value; }
        }

        /// <summary>
        /// 获取界面深度。
        /// </summary>
        public int DepthInUIGroup
        {
            get { return m_DepthInUIGroup; }
        }

        /// <summary>
        /// 获取是否暂停被覆盖的界面。
        /// </summary>
        public bool PauseCoveredUIForm
        {
            get { return m_PauseCoveredUIForm; }
        }

        public bool IsAwake { get; private set; }

        /// <summary>
        /// 界面初始化前执行
        /// </summary>
        public virtual void OnAwake()
        {
            IsAwake = true;
        }

        /// <summary>
        /// Godot生命周期_Ready方法
        /// 在此处初始化事件订阅器并订阅本地化语言变更事件
        /// </summary>
        public override void _Ready()
        {
            m_EventSubscriber = UIEventSubscriber.Create(this);
            m_EventSubscriber.CheckSubscribe(LocalizationLanguageChangeEventArgs.EventId, OnLocalizationLanguageChanged);
            OnAwake();
        }

        /// <summary>
        /// Godot生命周期_EnterTree方法
        /// 在界面启用时触发事件订阅
        /// </summary>
        public override void _EnterTree()
        {
            OnEventSubscribe();
        }

        /// <summary>
        /// Godot生命周期_ExitTree方法
        /// 在界面禁用时取消事件订阅,忽略本地化语言变更事件的订阅
        /// </summary>
        public override void _ExitTree()
        {
            OnEventUnSubscribe();
            m_EventSubscriber?.UnSubscribeAll(new List<string>() { LocalizationLanguageChangeEventArgs.EventId });
        }

        /// <summary>
        /// 订阅事件时调用
        /// 在界面启用(_EnterTree)时触发,可在此处订阅界面所需的事件
        /// 继承类通过重写此方法来注册自己需要的事件
        /// </summary>
        protected virtual void OnEventSubscribe()
        {
        }

        /// <summary>
        /// 取消订阅事件时调用
        /// 在界面禁用(_ExitTree)时触发,可在此处取消订阅界面的事件
        /// 继承类通过重写此方法来取消注册自己的事件
        /// </summary>
        protected virtual void OnEventUnSubscribe()
        {
        }

        /// <summary>
        /// 界面初始化。
        /// </summary>
        protected virtual void InitView()
        {
        }

        /// <summary>
        /// 初始化界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <param name="uiGroup">界面所处的界面组。</param>
        /// <param name="onInitAction">初始化界面前的委托。</param>
        /// <param name="pauseCoveredUIForm">是否暂停被覆盖的界面。</param>
        /// <param name="isNewInstance">是否是新实例。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <param name="isFullScreen">是否全屏</param>
        /// <param name="recycleInterval"></param>
        public void Init(int serialId, string uiFormAssetName, IUIGroup uiGroup, Action<IUIForm> onInitAction, bool pauseCoveredUIForm, bool isNewInstance, object userData, int recycleInterval, bool isFullScreen = false)
        {
            RecycleInterval = recycleInterval;
            ReleaseStartTime = DateTime.MaxValue;
            m_UserData = userData;
            if (serialId >= 0)
            {
                m_SerialId = serialId;
            }

            m_PauseCoveredUIForm = pauseCoveredUIForm;
            m_UIGroup = uiGroup;
            if (m_IsInit)
            {
                return;
            }

            m_UIFormAssetName = uiFormAssetName;
            m_FullName = GetType().FullName;
            m_DepthInUIGroup = 0;
            if (!isNewInstance)
            {
                return;
            }

            try
            {
                onInitAction?.Invoke(this);
                InitView();
                if (isFullScreen)
                {
                    MakeFullScreen();
                }

                OnInit();
            }
            catch (Exception exception)
            {
                Log.Error("UI form '[{0}]{1}' OnInit with exception '{2}'.", m_SerialId, m_UIFormAssetName, exception);
            }

            m_IsInit = true;
        }

        /// <summary>
        /// 初始化界面。
        /// </summary>
        public virtual void OnInit()
        {
        }

        private void OnLocalizationLanguageChanged(object sender, GameEventArgs e)
        {
            UpdateLocalization();
        }

        /// <summary>
        /// 界面回收。
        /// </summary>
        public virtual void OnRecycle()
        {
            m_DepthInUIGroup = 0;
            m_PauseCoveredUIForm = true;
        }

        /// <summary>
        /// 界面打开。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        public virtual void OnOpen(object userData)
        {
            m_Available = true;
            Visible = true;
            m_UserData = userData;
        }


        /// <summary>
        /// 绑定事件
        /// </summary>
        public virtual void BindEvent()
        {
        }

        /// <summary>
        /// 加载数据
        /// </summary>
        public virtual void LoadData()
        {
        }

        /// <summary>
        /// 界面更新本地化。
        /// </summary>
        public virtual void UpdateLocalization()
        {
        }

        /// <summary>
        /// 界面显示。
        /// </summary>
        /// <param name="handler">界面显示处理接口</param>
        /// <param name="complete">完成回调</param>
        public abstract void Show(IUIFormShowHandler handler, Action complete);

        /// <summary>
        /// 界面关闭。
        /// </summary>
        /// <param name="isShutdown">是否是关闭界面管理器时触发。</param>
        /// <param name="userData">用户自定义数据。</param>
        public virtual void OnClose(bool isShutdown, object userData)
        {
            m_Available = false;
            Visible = false;
            if (m_IsDisableRecycling)
            {
                return;
            }

            ReleaseStartTime = DateTime.Now;
        }


        /// <summary>
        /// 界面隐藏。
        /// </summary>
        /// <param name="handler">界面隐藏处理接口</param>
        /// <param name="complete">完成回调</param>
        public abstract void Hide(IUIFormHideHandler handler, Action complete);

        /// <summary>
        /// 界面暂停。
        /// </summary>
        public virtual void OnPause()
        {
            m_Available = false;
            Visible = false;
        }

        /// <summary>
        /// 界面暂停恢复。
        /// </summary>
        public virtual void OnResume()
        {
            m_Available = true;
            Visible = true;
        }

        /// <summary>
        /// 界面遮挡。
        /// </summary>
        public virtual void OnCover()
        {
        }

        /// <summary>
        /// 界面遮挡恢复。
        /// </summary>
        public virtual void OnReveal()
        {
        }

        /// <summary>
        /// 界面激活。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        public virtual void OnRefocus(object userData)
        {
        }

        /// <summary>
        /// 界面轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public virtual void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
        }

        /// <summary>
        /// 界面深度改变。
        /// </summary>
        /// <param name="uiGroupDepth">界面组深度。</param>
        /// <param name="depthInUIGroup">界面在界面组中的深度。</param>
        public void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
        {
            m_DepthInUIGroup = depthInUIGroup;
        }

        /// <summary>
        /// 销毁界面.
        /// </summary>
        public new virtual void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }

            if (m_EventSubscriber != null)
            {
                m_EventSubscriber.UnSubscribeAll();
                ReferencePool.Release(m_EventSubscriber);
            }

            m_EventSubscriber = null;
            IsDisposed = true;
        }

        /// <summary>
        /// 设置界面的可见性。
        /// </summary>
        /// <param name="visible">界面的可见性。</param>
        protected abstract void InternalSetVisible(bool visible);

        /// <summary>
        /// 设置界面为全屏
        /// </summary>
        protected internal abstract void MakeFullScreen();
    }
}
