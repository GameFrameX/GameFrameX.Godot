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
using System.Threading.Tasks;
using GameFrameX.Asset.Runtime;
using GameFrameX.AssetSystem;
using GameFrameX.Runtime;
using GameFrameX.Scene.Runtime;
using GameFrameX.UI.Runtime;
using Godot;
using Xunit;
namespace GameFrameX.UnitTests
{
    /// <summary>
    /// UI 管理器行为测试（Phase 3.1：ui 主包补全，基准 Unity com.gameframex.unity.ui）。
    /// 覆盖 BaseUIManager 查询方法族矩阵、UseSingletonOpenMode（OptionUIAllowMultiInstance 语义）、
    /// SetResourceManager 装配、CloseUIForm 立即/延迟回收（EnableAutoReleaseUIForm 组件开关的底层语义）。
    /// 引擎依赖说明：
    /// 1. UIForm 是 Godot Control，xunit 宿主无法实例化，测试通过 StubUIForm（纯 IUIForm 实现）注入状态。
    /// 2. UIComponent 是 Godot Node，其查询方法为对 m_UIManager 的纯转发，行为矩阵在 manager 级锁定；
    ///    组件级转发与 DesignResolution 归引擎测试（EngineTests/UIDesignResolutionTests）。
    /// 3. UIGroup.AddUIForm/RemoveUIForm 内部使用全局 ReferencePool，遵循 [Collection("ReferencePool")] 串行化惯例。
    /// </summary>
    [Collection("ReferencePool")]
    public sealed class UIManagerTests
    {
        private const string GroupName = "Normal";
        private readonly TestUIManager _manager;
        private readonly UIGroup _group;
        public UIManagerTests()
        {
            _manager = new TestUIManager();
            var helper = new StubUIGroupHelper();
            Assert.True(_manager.AddUIGroup(GroupName, 0, helper), "AddUIGroup should succeed");
            _group = (UIGroup)_manager.GetUIGroup(GroupName);
        }
        private StubUIForm CreateForm(int serialId, string assetName, string fullName)
        {
            var form = new StubUIForm();
            form.SerialId = serialId;
            form.UIFormAssetName = assetName;
            form.FullName = fullName;
            form.AssetPath = "res://UI/" + assetName + ".tscn";
            form.Available = true;
            form.Visible = true;
            form.UIGroup = _group;
            _group.AddUIForm(form);
            return form;
        }
        [Fact]
        public void QueryMatrix_AfterOpen_ReportsLoadedForms()
        {
            var a1 = CreateForm(1, "FormA", "GameFrameX.UnitTests.FormA");
            var a2 = CreateForm(2, "FormA", "GameFrameX.UnitTests.FormA");
            var b1 = CreateForm(3, "FormB", "GameFrameX.UnitTests.FormB");
            Assert.True(_manager.HasUIForm(1));
            Assert.True(_manager.HasUIForm(2));
            Assert.False(_manager.HasUIForm(99));
            Assert.True(_manager.HasUIForm("FormA"));
            Assert.False(_manager.HasUIForm("FormNone"));
            Assert.Same(a2, _manager.GetUIForm(2));
            Assert.Same(a1, _manager.GetUIForm(1));
            Assert.Null(_manager.GetUIForm(99));
            Assert.NotNull(_manager.GetUIForm("FormA"));
            Assert.Null(_manager.GetUIForm("FormNone"));
            Assert.Equal(2, _manager.GetUIForms("FormA").Length);
            Assert.Equal(1, _manager.GetUIForms("FormB").Length);
            Assert.Empty(_manager.GetUIForms("FormNone"));
            var list = new List<IUIForm>();
            _manager.GetUIForms("FormA", list);
            Assert.Equal(2, list.Count);
            Assert.Equal(3, _manager.GetAllLoadedUIForms().Length);
            var all = new List<IUIForm>();
            _manager.GetAllLoadedUIForms(all);
            Assert.Equal(3, all.Count);
            Assert.True(_manager.IsValidUIForm(a1));
            Assert.True(_manager.IsValidUIForm(b1));
            var orphan = new StubUIForm();
            orphan.SerialId = 77;
            orphan.UIFormAssetName = "Orphan";
            Assert.False(_manager.IsValidUIForm(orphan));
            Assert.False(_manager.IsValidUIForm(null));
            Assert.True(_manager.HasUIFormFullName("GameFrameX.UnitTests.FormA"));
            Assert.False(_manager.HasUIFormFullName("GameFrameX.UnitTests.FormNone"));
        }
        [Fact]
        public void QueryMatrix_AfterClose_RemovesFormFromQueries()
        {
            var form = CreateForm(1, "FormA", "GameFrameX.UnitTests.FormA");
            _manager.CloseUIForm(form, true);
            Assert.False(_manager.HasUIForm(1));
            Assert.False(_manager.HasUIForm("FormA"));
            Assert.Null(_manager.GetUIForm(1));
            Assert.Empty(_manager.GetAllLoadedUIForms());
            Assert.False(_manager.HasUIFormFullName("GameFrameX.UnitTests.FormA"));
        }
        [Fact]
        public void LoadingState_ReportedBySerialIdAndAssetName()
        {
            _manager.AddLoading(10, "FormLoading");
            Assert.True(_manager.IsLoadingUIForm(10));
            Assert.False(_manager.IsLoadingUIForm(11));
            Assert.True(_manager.IsLoadingUIForm("FormLoading"));
            Assert.False(_manager.IsLoadingUIForm("FormIdle"));
            Assert.Single(_manager.GetAllLoadingUIFormSerialIds());
            var list = new List<int>();
            _manager.GetAllLoadingUIFormSerialIds(list);
            Assert.Single(list);
            Assert.Equal(10, list[0]);
        }
        [Fact]
        public void CloseUIForm_IsNowRecycle_RecyclesImmediately()
        {
            var form = CreateForm(1, "FormA", "GameFrameX.UnitTests.FormA");
            _manager.CloseUIForm(form, true);
            Assert.False(_manager.HasUIForm(1));
            Assert.Single(_manager.RecycledForms);
            Assert.True(_manager.RecycledForms[0].Value);
            Assert.Same(form, _manager.RecycledForms[0].Key);
            Assert.Equal(1, form.CloseCount);
        }
        [Fact]
        public void CloseUIForm_Deferred_RecyclesOnUpdate()
        {
            var form = CreateForm(1, "FormA", "GameFrameX.UnitTests.FormA");
            _manager.CloseUIForm(form, false);
            Assert.False(_manager.HasUIForm(1));
            Assert.Empty(_manager.RecycledForms);
            _manager.Update(0f, 0f);
            Assert.Single(_manager.RecycledForms);
            Assert.False(_manager.RecycledForms[0].Value);
        }
        [Fact]
        public void UseSingletonOpenMode_FollowsAllowMultiInstanceAttribute()
        {
            Assert.False(_manager.IsSingletonOpenMode(null));
            Assert.True(_manager.IsSingletonOpenMode(typeof(FormWithoutAttribute)));
            Assert.False(_manager.IsSingletonOpenMode(typeof(FormAllowMulti)));
            Assert.True(_manager.IsSingletonOpenMode(typeof(FormSingleExplicit)));
        }
        [Fact]
        public void SetResourceManager_StoresManager_AndRejectsNull()
        {
            Assert.Throws<ArgumentNullException>(() => _manager.SetResourceManager(null));
            var fake = new StubAssetManager();
            _manager.SetResourceManager(fake);
            Assert.Same(fake, _manager.ExposedAssetManager);
        }
        private sealed class FormWithoutAttribute
        {
        }
        [OptionUIAllowMultiInstance]
        private sealed class FormAllowMulti
        {
        }
        [OptionUIAllowMultiInstance(false)]
        private sealed class FormSingleExplicit
        {
        }
        private sealed class TestUIManager : BaseUIManager
        {
            public readonly List<KeyValuePair<IUIForm, bool>> RecycledForms = new List<KeyValuePair<IUIForm, bool>>();
            public IAssetManager ExposedAssetManager
            {
                get { return m_AssetManager; }
            }
            public void AddLoading(int serialId, string assetName)
            {
                m_UIFormsBeingLoaded.Add(serialId, assetName);
            }
            public bool IsSingletonOpenMode(Type uiFormType)
            {
                return UseSingletonOpenMode(uiFormType);
            }
            protected override Task<IUIForm> InnerOpenUIFormAsync(string uiFormAssetPath, Type uiFormType, bool pauseCoveredUIForm, object userData, bool isFullScreen = false)
            {
                return Task.FromResult<IUIForm>(null);
            }
            protected override void RecycleUIForm(IUIForm uiForm, bool isDispose = false)
            {
                RecycledForms.Add(new KeyValuePair<IUIForm, bool>(uiForm, isDispose));
            }
        }
        private sealed class StubUIGroupHelper : IUIGroupHelper
        {
            public int Depth { get; set; }
            public void SetDepth(int depth)
            {
                Depth = depth;
            }
            public IUIGroupHelper Handler(Node root, string groupName, string uiGroupHelperTypeName, IUIGroupHelper customUIGroupHelper, int depth = 0)
            {
                return this;
            }
        }
        private sealed class StubUIForm : IUIForm
        {
            public DateTime ReleaseStartTime { get; set; }
            public int SerialId { get; set; }
            public string FullName { get; set; }
            public string UIFormAssetName { get; set; }
            public string AssetPath { get; set; }
            public bool IsDisableRecycling { get; set; }
            public bool IsDisableClosing { get; set; }
            public bool IsCanRecycle { get; set; }
            public int RecycleInterval { get; set; }
            public bool IsCenter { get; set; }
            public object Handle { get; set; }
            public bool Available { get; set; }
            public bool EnableShowAnimation { get; set; }
            public string ShowAnimationName { get; set; }
            public bool EnableHideAnimation { get; set; }
            public string HideAnimationName { get; set; }
            public bool Visible { get; set; }
            public IUIGroup UIGroup { get; set; }
            public int DepthInUIGroup { get; set; }
            public bool PauseCoveredUIForm { get; set; }
            public bool IsAwake { get; set; }
            public int CloseCount { get; private set; }
            public void OnAwake()
            {
            }
            public void Init(int serialId, string uiFormAssetName, IUIGroup uiGroup, Action<IUIForm> onInitAction, bool pauseCoveredUIForm, bool isNewInstance, object userData, int recycleInterval, bool isFullScreen = false)
            {
            }
            public void OnInit()
            {
            }
            public void OnRecycle()
            {
            }
            public void OnOpen(object userData)
            {
            }
            public void BindEvent()
            {
            }
            public void LoadData()
            {
            }
            public void UpdateLocalization()
            {
            }
            public void Show(IUIFormShowHandler handler, Action complete)
            {
                if (complete != null)
                {
                    complete();
                }
            }
            public void OnClose(bool isShutdown, object userData)
            {
                CloseCount++;
            }
            public void Hide(IUIFormHideHandler handler, Action complete)
            {
                if (complete != null)
                {
                    complete();
                }
            }
            public void OnPause()
            {
            }
            public void OnResume()
            {
            }
            public void OnCover()
            {
            }
            public void OnReveal()
            {
            }
            public void OnRefocus(object userData)
            {
            }
            public void OnUpdate(float elapseSeconds, float realElapseSeconds)
            {
            }
            public void OnDepthChanged(int uiGroupDepth, int depthInUIGroup)
            {
            }
        }
        private sealed class StubAssetManager : IAssetManager
        {
            public int DownloadingMaxNum { get; set; }
            public int FailedTryAgain { get; set; }
            public string DefaultPackageName { get; set; }
            public EPlayMode PlayMode
            {
                get { throw new NotSupportedException(); }
            }
            public EFileVerifyLevel VerifyLevel
            {
                get { throw new NotSupportedException(); }
            }
            public long Milliseconds { get; set; }
            public void SetPlayMode(EPlayMode playMode)
            {
                throw new NotSupportedException();
            }
            public void Initialize()
            {
                throw new NotSupportedException();
            }
            public Task<bool> InitPackageAsync(string packageName, string hostServerURL, string fallbackHostServerURL, bool isDefaultPackage = true)
            {
                throw new NotSupportedException();
            }
            public void UnloadAsset(string assetPath)
            {
                throw new NotSupportedException();
            }
            public Task<SubAssetsHandle> LoadSubAssetsAsync(AssetInfo assetInfo)
            {
                throw new NotSupportedException();
            }
            public Task<SubAssetsHandle> LoadSubAssetsAsync(string path, Type type)
            {
                throw new NotSupportedException();
            }
            public Task<SubAssetsHandle> LoadSubAssetsAsync<T>(string path) where T : Resource
            {
                throw new NotSupportedException();
            }
            public SubAssetsHandle LoadSubAssetSync(AssetInfo assetInfo)
            {
                throw new NotSupportedException();
            }
            public SubAssetsHandle LoadSubAssetSync(string path, Type type)
            {
                throw new NotSupportedException();
            }
            public SubAssetsHandle LoadSubAssetSync<T>(string path) where T : Resource
            {
                throw new NotSupportedException();
            }
            public Task<RawFileHandle> LoadRawFileAsync(AssetInfo assetInfo)
            {
                throw new NotSupportedException();
            }
            public Task<RawFileHandle> LoadRawFileAsync(string path)
            {
                throw new NotSupportedException();
            }
            public RawFileHandle LoadRawFileSync(AssetInfo assetInfo)
            {
                throw new NotSupportedException();
            }
            public RawFileHandle LoadRawFileSync(string path)
            {
                throw new NotSupportedException();
            }
            public Task<AssetHandle> LoadAssetAsync(AssetInfo assetInfo)
            {
                throw new NotSupportedException();
            }
            public Task<AssetHandle> LoadAssetAsync(string path, Type type)
            {
                throw new NotSupportedException();
            }
            public Task<AssetHandle> LoadAssetAsync<T>(string path) where T : Resource
            {
                throw new NotSupportedException();
            }
            public Task<AllAssetsHandle> LoadAllAssetsAsync<T>(string path) where T : Resource
            {
                throw new NotSupportedException();
            }
            public Task<AllAssetsHandle> LoadAllAssetsAsync(string path, Type type)
            {
                throw new NotSupportedException();
            }
            public Task<AllAssetsHandle> LoadAllAssetsAsync(string path)
            {
                throw new NotSupportedException();
            }
            public Task<AllAssetsHandle> LoadAllAssetsAsync(AssetInfo assetInfo)
            {
                throw new NotSupportedException();
            }
            public Task<AssetHandle> LoadAssetAsync(string path)
            {
                throw new NotSupportedException();
            }
            public SubAssetsHandle LoadSubAssetsAsync(string path)
            {
                throw new NotSupportedException();
            }
            public AllAssetsHandle LoadAllAssetsSync(string path)
            {
                throw new NotSupportedException();
            }
            public AllAssetsHandle LoadAllAssetsSync<T>(string path) where T : Resource
            {
                throw new NotSupportedException();
            }
            public AllAssetsHandle LoadAllAssetsSync(string path, Type type)
            {
                throw new NotSupportedException();
            }
            public AllAssetsHandle LoadAllAssetsSync(AssetInfo assetInfo)
            {
                throw new NotSupportedException();
            }
            public SubAssetsHandle LoadSubAssetSync(string path)
            {
                throw new NotSupportedException();
            }
            public AssetHandle LoadAssetSync(string path)
            {
                throw new NotSupportedException();
            }
            public AssetHandle LoadAssetSync(string path, Type type)
            {
                throw new NotSupportedException();
            }
            public AssetHandle LoadAssetSync(AssetInfo assetInfo)
            {
                throw new NotSupportedException();
            }
            public AssetHandle LoadAssetSync<T>(string path) where T : Resource
            {
                throw new NotSupportedException();
            }
            public Task<SceneHandle> LoadSceneAsync(string path, SceneLoadMode sceneMode, bool activateOnLoad = true)
            {
                throw new NotSupportedException();
            }
            public Task<SceneHandle> LoadSceneAsync(AssetInfo assetInfo, SceneLoadMode sceneMode, bool activateOnLoad = true)
            {
                throw new NotSupportedException();
            }
            public ResourcePackage CreateAssetsPackage(string packageName)
            {
                throw new NotSupportedException();
            }
            public ResourcePackage TryGetAssetsPackage(string packageName)
            {
                throw new NotSupportedException();
            }
            public bool HasAssetsPackage(string packageName)
            {
                throw new NotSupportedException();
            }
            public ResourcePackage GetAssetsPackage(string packageName)
            {
                throw new NotSupportedException();
            }
            public bool IsNeedDownload(AssetInfo assetInfo)
            {
                throw new NotSupportedException();
            }
            public bool IsNeedDownload(string path)
            {
                throw new NotSupportedException();
            }
            public AssetInfo[] GetAssetInfos(string[] assetTags)
            {
                throw new NotSupportedException();
            }
            public AssetInfo[] GetAssetInfos(string assetTag)
            {
                throw new NotSupportedException();
            }
            public AssetInfo GetAssetInfo(string path)
            {
                throw new NotSupportedException();
            }
            public bool HasAssetPath(string assetPath)
            {
                throw new NotSupportedException();
            }
            public void SetDefaultAssetsPackage(ResourcePackage resourcePackage)
            {
                throw new NotSupportedException();
            }
            public void ClearUnusedBundleFilesAsync(string packageName = null)
            {
                throw new NotSupportedException();
            }
            public void ClearAllBundleFilesAsync(string packageName = null)
            {
                throw new NotSupportedException();
            }
            public void UnloadUnusedAssetsAsync(string packageName = null)
            {
                throw new NotSupportedException();
            }
            public void UnloadAllAssetsAsync(string packageName = null)
            {
                throw new NotSupportedException();
            }
            public void UnloadAsset(string packageName, string assetPath)
            {
                throw new NotSupportedException();
            }
        }
    }
}
