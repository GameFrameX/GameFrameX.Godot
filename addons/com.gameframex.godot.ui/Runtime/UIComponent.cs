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
//  Any legal disputes and liabilities arising from secondary development based on project
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
using GameFrameX.ObjectPool;
using GameFrameX.Runtime;
using Godot;

namespace GameFrameX.UI.Runtime
{
    /// <summary>
    /// 界面组件。
    /// </summary>
    public partial class UIComponent : GameFrameworkComponent
    {
        private const int DefaultPriority = 0;
        private const string GDGUIRootNodeName = "GDGUI";
        private const string FairyGUIRootNodeName = "FGUI";
        private const string LegacyGDGUIRootNodeName = "GDGUIRoot";
        private const string LegacyFairyGUIRootNodeName = "FairyGUIRoot";

        private IUIManager m_UIManager = null;
        private EventComponent m_EventComponent = null;
        private Node m_GDGUIRoot = null;
        private Node m_FairyGUIRoot = null;

        private readonly List<IUIForm> m_InternalUIFormResults = new List<IUIForm>();
        private const int MaxInitializeRetryFrames = 600;
        private int m_InitializeRetryFrames = 0;
        private bool m_IsRuntimeInitialized = false;

        [Export] private bool m_EnableOpenUIFormSuccessEvent = true;

        [Export] private bool m_EnableOpenUIFormFailureEvent = true;

        [Export] private bool m_EnableOpenUIFormUpdateEvent = false;

        [Export] private bool m_EnableOpenUIFormDependencyAssetEvent = false;

        [Export] private bool m_EnableCloseUIFormCompleteEvent = true;
        [Export] private bool m_IsEnableUIShowAnimation = false;
        [Export] private bool m_IsEnableUIHideAnimation = false;

        [Export(PropertyHint.Range, "30,120,1")]
        private float m_InstanceAutoReleaseInterval = 60f;

        // [Export(PropertyHint.Range, "16,120,1")] 
        private int m_InstanceCapacity = 16;

        [Export(PropertyHint.Range, "30,1200,1")]
        private float m_InstanceExpireTime = 60f;

        // [Export(PropertyHint.Range, "30.0,120.0,1.0")] 
        private int m_RecycleInterval = 60;

        [Export] private NodePath m_UIRootPath;

        [Export] private string m_UIFormHelperTypeName = GetDefaultUIFormHelperTypeName();

        [Export] private UIFormHelperBase m_CustomUIFormHelper = null;

        [Export] private string m_UIGroupHelperTypeName = GetDefaultUIGroupHelperTypeName();

        [Export] private UIGroupHelperBase m_CustomUIGroupHelper = null;

        private UIGroup[] m_UIGroups = new UIGroup[]
        {
            new UIGroup(UIGroupConstants.Hidden.Depth, UIGroupConstants.Hidden.Name),
            new UIGroup(UIGroupConstants.Background.Depth, UIGroupConstants.Background.Name),
            new UIGroup(UIGroupConstants.Scene.Depth, UIGroupConstants.Scene.Name),
            new UIGroup(UIGroupConstants.World.Depth, UIGroupConstants.World.Name),
            new UIGroup(UIGroupConstants.Battle.Depth, UIGroupConstants.Battle.Name),
            new UIGroup(UIGroupConstants.Hud.Depth, UIGroupConstants.Hud.Name),
            new UIGroup(UIGroupConstants.Map.Depth, UIGroupConstants.Map.Name),
            new UIGroup(UIGroupConstants.Floor.Depth, UIGroupConstants.Floor.Name),
            new UIGroup(UIGroupConstants.Normal.Depth, UIGroupConstants.Normal.Name),
            new UIGroup(UIGroupConstants.Fixed.Depth, UIGroupConstants.Fixed.Name),
            new UIGroup(UIGroupConstants.Window.Depth, UIGroupConstants.Window.Name),
            new UIGroup(UIGroupConstants.Tip.Depth, UIGroupConstants.Tip.Name),
            new UIGroup(UIGroupConstants.Guide.Depth, UIGroupConstants.Guide.Name),
            new UIGroup(UIGroupConstants.BlackBoard.Depth, UIGroupConstants.BlackBoard.Name),
            new UIGroup(UIGroupConstants.Dialogue.Depth, UIGroupConstants.Dialogue.Name),
            new UIGroup(UIGroupConstants.Loading.Depth, UIGroupConstants.Loading.Name),
            new UIGroup(UIGroupConstants.Notify.Depth, UIGroupConstants.Notify.Name),
            new UIGroup(UIGroupConstants.System.Depth, UIGroupConstants.System.Name),
        };

        /// <summary>
        /// 获取 FairyGUI UI 根节点。
        /// </summary>
        public Node FairyGUIRoot
        {
            get { return m_FairyGUIRoot; }
        }

        /// <summary>
        /// 获取 GDGUI  UI 根节点。
        /// </summary>
        public Node GdguiRoot
        {
            get { return m_GDGUIRoot; }
        }

        /// <summary>
        /// 获取是否启用界面显示动画。
        /// </summary>
        public bool IsEnableUIShowAnimation
        {
            get { return m_UIManager.IsEnableUIShowAnimation; }
        }

        /// <summary>
        /// 获取是否启用界面隐藏动画。
        /// </summary>
        public bool IsEnableUIHideAnimation
        {
            get { return m_UIManager.IsEnableUIHideAnimation; }
        }

        /// <summary>
        /// 获取界面组数量。
        /// </summary>
        public int UIGroupCount
        {
            get { return m_UIManager.UIGroupCount; }
        }

        /// <summary>
        /// 获取 UI 组件是否完成初始化。
        /// </summary>
        public bool IsInitialized
        {
            get
            {
                var baseManager = m_UIManager as BaseUIManager;
                return m_IsRuntimeInitialized && baseManager != null && baseManager.IsRuntimeReady;
            }
        }

        /// <summary>
        /// 获取当前 UI 管理器后端类型名称。
        /// </summary>
        public string RuntimeBackendTypeName
        {
            get { return m_UIManager?.GetType().FullName ?? "<null>"; }
        }

        /// <summary>
        /// 获取或设置界面实例对象池自动回收可回收对象的间隔秒数。
        /// </summary>
        public int RecycleInterval
        {
            get { return m_UIManager.RecycleInterval; }
            set { m_UIManager.RecycleInterval = m_RecycleInterval = value; }
        }

        /// <summary>
        /// 获取或设置界面实例对象池自动释放可释放对象的间隔秒数。
        /// </summary>
        public float InstanceAutoReleaseInterval
        {
            get { return m_UIManager.InstanceAutoReleaseInterval; }
            set { m_UIManager.InstanceAutoReleaseInterval = m_InstanceAutoReleaseInterval = value; }
        }

        /// <summary>
        /// 获取或设置界面实例对象池的容量。
        /// </summary>
        public int InstanceCapacity
        {
            get { return m_UIManager.InstanceCapacity; }
            set { m_UIManager.InstanceCapacity = m_InstanceCapacity = value; }
        }

        /// <summary>
        /// 获取或设置界面实例对象池对象过期秒数。
        /// </summary>
        public float InstanceExpireTime
        {
            get { return m_UIManager.InstanceExpireTime; }
            set { m_UIManager.InstanceExpireTime = m_InstanceExpireTime = value; }
        }

        /// <summary>
        /// 游戏框架组件初始化。
        /// </summary>
        public override void _Ready()
        {
            componentType = ResolveUIManagerComponentTypeName();
            ImplementationComponentType = Utility.Assembly.GetType(componentType);
            InterfaceComponentType = typeof(IUIManager);
            base._Ready();
            InitializeUIRoots();

            m_UIManager = GameFrameworkEntry.GetModule<IUIManager>();
            if (m_UIManager == null)
            {
                Log.Error("UI manager is invalid.");
                return;
            }

            Log.Info("[UIComponent] backend type={0}, helperType={1}, groupHelperType={2}",
                m_UIManager.GetType().FullName,
                m_UIFormHelperTypeName,
                m_UIGroupHelperTypeName);

            if (m_EnableOpenUIFormSuccessEvent)
            {
                m_UIManager.OpenUIFormSuccess += OnOpenUIFormSuccess;
            }

            m_UIManager.OpenUIFormFailure += OnOpenUIFormFailure;

            if (m_EnableOpenUIFormUpdateEvent)
            {
                m_UIManager.OpenUIFormUpdate += OnOpenUIFormUpdate;
            }

            if (m_EnableOpenUIFormDependencyAssetEvent)
            {
                m_UIManager.OpenUIFormDependencyAsset += OnOpenUIFormDependencyAsset;
            }

            if (m_EnableCloseUIFormCompleteEvent)
            {
                m_UIManager.CloseUIFormComplete += OnCloseUIFormComplete;
            }

            InitializeUIManager();
        }

        private bool EnsureRuntimeInitialized()
        {
            if (IsInitialized)
            {
                return true;
            }

            InitializeUIManager();
            return IsInitialized;
        }

        /// <summary>
        /// 解析界面管理器实现类型名称。
        /// </summary>
        /// <returns>可用的界面管理器实现类型名称。</returns>
        private string ResolveUIManagerComponentTypeName()
        {
            const string gdGuiUIManagerType = "GameFrameX.UI.GDGUI.Runtime.UIManager";
            const string runtimeUIManagerType = "GameFrameX.UI.Runtime.UIManager";

#if FAIRY_GUI
            const string fairyGuiUIManagerType = "GameFrameX.UI.FairyGUI.Runtime.UIManager";
            if (Utility.Assembly.GetType(fairyGuiUIManagerType) != null)
            {
                return fairyGuiUIManagerType;
            }
#endif

            if (!string.IsNullOrWhiteSpace(componentType))
            {
                return componentType;
            }

            if (Utility.Assembly.GetType(gdGuiUIManagerType) != null)
            {
                return gdGuiUIManagerType;
            }

            return runtimeUIManagerType;
        }

        private void InitializeUIManager()
        {
            if (m_IsRuntimeInitialized)
            {
                return;
            }

            BaseComponent baseComponent = GameEntry.GetComponent<BaseComponent>();
            if (baseComponent == null)
            {
                ScheduleInitializeRetry("BaseComponent not ready");
                return;
            }

            m_EventComponent = GameEntry.GetComponent<EventComponent>();
            if (m_EventComponent == null)
            {
                ScheduleInitializeRetry("EventComponent not ready");
                return;
            }

            var objectPoolManager = GameFrameworkEntry.GetModule<IObjectPoolManager>();
            if (objectPoolManager == null)
            {
                ScheduleInitializeRetry("IObjectPoolManager not ready");
                return;
            }

            m_UIManager.SetObjectPoolManager(objectPoolManager);
            m_UIManager.InstanceAutoReleaseInterval = m_InstanceAutoReleaseInterval;
            m_UIManager.InstanceCapacity = m_InstanceCapacity;
            m_UIManager.InstanceExpireTime = m_InstanceExpireTime;
            m_UIManager.RecycleInterval = m_RecycleInterval;
            m_UIManager.IsEnableUIHideAnimation = m_IsEnableUIHideAnimation;
            m_UIManager.IsEnableUIShowAnimation = m_IsEnableUIShowAnimation;
            EnsureBackendHelperDefaults();

            if (Utility.Assembly.GetType(m_UIGroupHelperTypeName) == null)
            {
                m_UIGroupHelperTypeName = "GameFrameX.UI.Runtime.DefaultUIGroupHelper";
            }

            if (Utility.Assembly.GetType(m_UIFormHelperTypeName) == null)
            {
                m_UIFormHelperTypeName = "GameFrameX.UI.Runtime.DefaultUIFormHelper";
            }

            UIFormHelperBase uiFormHelper = Helper.CreateHelper(m_UIFormHelperTypeName, m_CustomUIFormHelper);
            if (uiFormHelper == null)
            {
                Log.Error("Can not create UI form helper.");
                return;
            }

            uiFormHelper.Name = "UIFormHelper";
            AddChild(uiFormHelper);

            m_UIManager.SetUIFormHelper(uiFormHelper);

            for (int i = 0; i < m_UIGroups.Length; i++)
            {
                var uiGroup = m_UIGroups[i];
                if (!AddUIGroup(uiGroup.Name, uiGroup.Depth))
                {
                    Log.Warning("Add UI group '{0}' failure.", uiGroup.Name);
                    continue;
                }
            }

            m_IsRuntimeInitialized = true;
            m_InitializeRetryFrames = 0;
            Log.Info("[UIComponent] initialized groups={0} gdRoot={1} fairyRoot={2}",
                m_UIManager.UIGroupCount,
                m_GDGUIRoot?.Name ?? "<null>",
                m_FairyGUIRoot?.Name ?? "<null>");

            var baseManager = m_UIManager as BaseUIManager;
            if (baseManager != null && !baseManager.IsRuntimeReady)
            {
                m_IsRuntimeInitialized = false;
                ScheduleInitializeRetry("UI manager runtime dependencies not ready");
            }
        }

        private void ScheduleInitializeRetry(string reason)
        {
            if (m_IsRuntimeInitialized)
            {
                return;
            }

            m_InitializeRetryFrames++;
            if (m_InitializeRetryFrames > MaxInitializeRetryFrames)
            {
                GD.PushError($"[UIComponent] initialize failed after {MaxInitializeRetryFrames} retries. reason={reason}");
                return;
            }

            if (m_InitializeRetryFrames == 1 || m_InitializeRetryFrames % 60 == 0)
            {
                GD.PushWarning($"[UIComponent] initialize deferred retry={m_InitializeRetryFrames}/{MaxInitializeRetryFrames} reason={reason}");
            }

            CallDeferred(nameof(InitializeUIManager));
        }

        /// <summary>
        /// 功能：初始化 GDGUI 与 FairyGUI 根节点，并根据当前 Helper 名称选择主根节点。
        /// </summary>
        private void InitializeUIRoots()
        {
            Node configuredRoot = null;
            if (!string.IsNullOrEmpty(m_UIRootPath) && HasNode(m_UIRootPath))
            {
                configuredRoot = GetNode<Node>(m_UIRootPath);
            }

            var uiContainerRoot = ResolveUIContainerRoot(configuredRoot);
            m_GDGUIRoot = FindOrCreateRoot(uiContainerRoot, GDGUIRootNodeName, LegacyGDGUIRootNodeName);
            m_FairyGUIRoot = FindOrCreateRoot(uiContainerRoot, FairyGUIRootNodeName, LegacyFairyGUIRootNodeName);
            EnsureFairyGuiDisplayRootAttached();
        }

        /// <summary>
        /// 功能：根据 Helper 类型名称判断当前是否为 FairyGUI 运行模式。
        /// </summary>
        /// <returns>若命中 FairyGUI 关键字返回 true，否则返回 false。</returns>
        private bool IsFairyGUIRuntime()
        {
            if (ContainsFairyGUIKeyword(m_UIGroupHelperTypeName))
            {
                return true;
            }

            if (ContainsFairyGUIKeyword(m_UIFormHelperTypeName))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 功能：判断类型名称中是否包含 FairyGUI 关键字。
        /// </summary>
        /// <param name="typeName">待判断的类型名称。</param>
        /// <returns>包含 FairyGUI 关键字返回 true，否则返回 false。</returns>
        private static bool ContainsFairyGUIKeyword(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
            {
                return false;
            }

            return typeName.IndexOf("fairygui", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string GetDefaultUIFormHelperTypeName()
        {
#if FAIRY_GUI
            return "GameFrameX.UI.FairyGUI.Runtime.FairyGUIFormHelper";
#else
            return "GameFrameX.UI.GDGUI.Runtime.GDGUIFormHelper";
#endif
        }

        private static string GetDefaultUIGroupHelperTypeName()
        {
#if FAIRY_GUI
            return "GameFrameX.UI.FairyGUI.Runtime.FairyGUIUIGroupHelper";
#else
            return "GameFrameX.UI.GDGUI.Runtime.GDGUIUIGroupHelper";
#endif
        }

        private void EnsureBackendHelperDefaults()
        {
            var defaultFormHelper = GetDefaultUIFormHelperTypeName();
            if (!string.Equals(m_UIFormHelperTypeName, defaultFormHelper, StringComparison.Ordinal))
            {
                m_UIFormHelperTypeName = defaultFormHelper;
            }

            var defaultGroupHelper = GetDefaultUIGroupHelperTypeName();
            if (!string.Equals(m_UIGroupHelperTypeName, defaultGroupHelper, StringComparison.Ordinal))
            {
                m_UIGroupHelperTypeName = defaultGroupHelper;
            }
        }

        /// <summary>
        /// 功能：获取当前 UI 系统应使用的根节点。
        /// </summary>
        /// <returns>当前系统对应的根节点。</returns>
        private Node GetCurrentUIRoot()
        {
#if FAIRY_GUI
            return m_FairyGUIRoot ?? m_GDGUIRoot;
#else
            return m_GDGUIRoot ?? m_FairyGUIRoot;
#endif
        }

        /// <summary>
        /// 功能：确保 FairyGUI 的真实显示根节点挂到 UI/FGUI 下，便于与 GDGUI 一致地在场景树观察。
        /// </summary>
        private void EnsureFairyGuiDisplayRootAttached()
        {
#if FAIRY_GUI
            if (m_FairyGUIRoot == null)
            {
                return;
            }

            _ = global::FairyGUI.Stage.inst;
            var displayRoot = global::FairyGUI.GRoot.inst.displayObject?.node;
            if (displayRoot == null)
            {
                return;
            }

            var currentParent = displayRoot.GetParent();
            if (currentParent == m_FairyGUIRoot)
            {
                return;
            }

            currentParent?.RemoveChild(displayRoot);
            m_FairyGUIRoot.AddChild(displayRoot);
#endif
        }

        /// <summary>
        /// 功能：按名称查找或创建根节点。
        /// </summary>
        /// <param name="rootNodeName">根节点名称。</param>
        /// <returns>已存在或新建的根节点。</returns>
        private Node ResolveUIContainerRoot(Node configuredRoot)
        {
            if (configuredRoot == null)
            {
                return this;
            }

            var configuredName = configuredRoot.Name.ToString();
            if (string.Equals(configuredName, GDGUIRootNodeName, StringComparison.Ordinal) ||
                string.Equals(configuredName, FairyGUIRootNodeName, StringComparison.Ordinal) ||
                string.Equals(configuredName, LegacyGDGUIRootNodeName, StringComparison.Ordinal) ||
                string.Equals(configuredName, LegacyFairyGUIRootNodeName, StringComparison.Ordinal))
            {
                return configuredRoot.GetParent() ?? this;
            }

            return configuredRoot;
        }

        private Node FindOrCreateRoot(Node parent, string rootNodeName, string legacyNodeName = null)
        {
            var rootParent = parent ?? this;
            Node rootNode = rootParent.GetNodeOrNull<Node>(rootNodeName);
            if (rootNode != null)
            {
                return rootNode;
            }

            if (!string.IsNullOrWhiteSpace(legacyNodeName))
            {
                rootNode = rootParent.GetNodeOrNull<Node>(legacyNodeName);
                if (rootNode != null)
                {
                    rootNode.Name = rootNodeName;
                    return rootNode;
                }
            }

            rootNode = new Node();
            rootNode.Name = rootNodeName;
            rootParent.AddChild(rootNode);
            return rootNode;
        }

        /// <summary>
        /// 是否存在界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>是否存在界面。</returns>
        public bool HasUIForm(int serialId)
        {
            return m_UIManager.HasUIForm(serialId);
        }

        /// <summary>
        /// 是否存在界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>是否存在界面。</returns>
        public bool HasUIForm(string uiFormAssetName)
        {
            return m_UIManager.HasUIForm(uiFormAssetName);
        }

        /// <summary>
        /// 是否正在加载界面。
        /// </summary>
        /// <param name="serialId">界面序列编号。</param>
        /// <returns>是否正在加载界面。</returns>
        public bool IsLoadingUIForm(int serialId)
        {
            return m_UIManager.IsLoadingUIForm(serialId);
        }

        /// <summary>
        /// 是否正在加载界面。
        /// </summary>
        /// <param name="uiFormAssetName">界面资源名称。</param>
        /// <returns>是否正在加载界面。</returns>
        public bool IsLoadingUIForm(string uiFormAssetName)
        {
            return m_UIManager.IsLoadingUIForm(uiFormAssetName);
        }

        /// <summary>
        /// 是否是合法的界面。
        /// </summary>
        /// <param name="uiForm">界面。</param>
        /// <returns>界面是否合法。</returns>
        public bool IsValidUIForm(IUIForm uiForm)
        {
            return m_UIManager.IsValidUIForm(uiForm);
        }

        /// <summary>
        /// 激活界面。
        /// </summary>
        /// <param name="uiForm">要激活的界面。</param>
        public void RefocusUIForm(UIForm uiForm)
        {
            m_UIManager.RefocusUIForm(uiForm);
        }

        /// <summary>
        /// 激活界面。
        /// </summary>
        /// <param name="uiForm">要激活的界面。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void RefocusUIForm(UIForm uiForm, object userData)
        {
            m_UIManager.RefocusUIForm(uiForm, userData);
        }

        /// <summary>
        /// 设置界面是否被加锁。
        /// </summary>
        /// <param name="uiForm">要设置是否被加锁的界面。</param>
        /// <param name="locked">界面是否被加锁。</param>
        public void SetUIFormInstanceLocked(UIForm uiForm, bool locked)
        {
            if (uiForm == null)
            {
                Log.Warning("UI form is invalid.");
                return;
            }

            m_UIManager.SetUIFormInstanceLocked(uiForm, locked);
        }

        private void OnOpenUIFormSuccess(object sender, OpenUIFormSuccessEventArgs e)
        {
            m_EventComponent.Fire(this, e);
        }

        private void OnOpenUIFormFailure(object sender, OpenUIFormFailureEventArgs e)
        {
            Log.Warning($"Open UI form failure, asset name '{e.UIFormAssetName}',  pause covered UI form '{e.PauseCoveredUIForm}', error message '{e.ErrorMessage}'.");
            if (m_EnableOpenUIFormFailureEvent)
            {
                m_EventComponent.Fire(this, e);
            }
        }

        private void OnOpenUIFormUpdate(object sender, OpenUIFormUpdateEventArgs e)
        {
            m_EventComponent.Fire(this, e);
        }

        private void OnOpenUIFormDependencyAsset(object sender, OpenUIFormDependencyAssetEventArgs e)
        {
            m_EventComponent.Fire(this, e);
        }

        /// <summary>
        /// 设置界面显示处理器。
        /// </summary>
        /// <param name="uiFormShowHandler">界面显示处理器。</param>
        public void SetShowUIFormHandler(IUIFormShowHandler uiFormShowHandler)
        {
            m_UIManager.SetUIFormShowHandler(uiFormShowHandler);
        }

        /// <summary>
        /// 设置界面隐藏处理器。
        /// </summary>
        /// <param name="uiFormHideHandler">界面隐藏处理器。</param>
        public void SetHideUIFormHandler(IUIFormHideHandler uiFormHideHandler)
        {
            m_UIManager.SetUIFormHideHandler(uiFormHideHandler);
        }

        private void OnCloseUIFormComplete(object sender, CloseUIFormCompleteEventArgs e)
        {
            m_EventComponent.Fire(this, e);
        }
    }
}
