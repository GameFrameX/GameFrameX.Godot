using System;
using System.Threading.Tasks;
using GameFrameX.AssetSystem;
using GameFrameX.Runtime;
using Godot;

namespace GameFrameX.Asset.Runtime
{
    /// <summary>
    /// 资源组件。
    /// </summary>
    [GlobalClass]
    public sealed partial class AssetComponent : GameFrameworkComponent
    {
        /// <summary>
        /// 内置资源包名称。
        /// </summary>
        public const string BuildInPackageName = "DefaultPackage";

        [Export] private EPlayMode m_GamePlayMode = EPlayMode.EditorSimulateMode;

        private IAssetManager _assetManager = null;

        /// <summary>
        /// 获取或设置运行模式。
        /// </summary>
        public EPlayMode GamePlayMode
        {
            get { return m_GamePlayMode; }
            set
            {
                m_GamePlayMode = value;
                SetPlayMode(m_GamePlayMode);
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            InitializeInternal();
        }

        /// <summary>
        /// 异步初始化操作
        /// </summary>
        /// <param name="packageName">包名称。</param>
        /// <param name="hostServerURL">热更链接URL。</param>
        /// <param name="fallbackHostServerURL">备用热更链接URL</param>
        /// <param name="isDefaultPackage">是否是默认包，默认是</param>
        /// <returns></returns>
        public async Task<bool> InitPackageAsync(string packageName, string hostServerURL, string fallbackHostServerURL, bool isDefaultPackage = true)
        {
            return await _assetManager.InitPackageAsync(packageName, hostServerURL, fallbackHostServerURL, isDefaultPackage);
        }

        /// <summary>
        /// 卸载资源
        /// </summary>
        /// <param name="assetPath"></param>
        public void UnloadAsset(string assetPath)
        {
            _assetManager.UnloadAsset(assetPath);
        }

        /// <summary>
        /// 卸载资源
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        /// <param name="assetPath">资源路径</param>
        public void UnloadAsset(string packageName, string assetPath)
        {
            _assetManager.UnloadAsset(packageName, assetPath);
        }

        /// <summary>
        /// 释放资源句柄。
        /// </summary>
        /// <param name="assetHandle">资源句柄。</param>
        public void UnloadAssetHandle(AssetHandle assetHandle)
        {
            if (assetHandle == null)
            {
                return;
            }

            assetHandle.Release();
        }

        public override void _Ready()
        {
            // 说明：Godot 编辑器导出/运行时无模拟构建上下文，与 Unity 一样在非编辑器环境下将 EditorSimulateMode 回退为 HostPlayMode。
#if !TOOLS
            if (m_GamePlayMode == EPlayMode.EditorSimulateMode)
            {
                m_GamePlayMode = EPlayMode.HostPlayMode;
            }

            // 说明：替代 UNITY_WEBGL 条件编译，Web 平台运行时切换为 WebPlayMode。
            if (OS.HasFeature("web"))
            {
                GamePlayMode = EPlayMode.WebPlayMode;
            }
#endif
            ImplementationComponentType = Utility.Assembly.GetType(componentType);
            InterfaceComponentType = typeof(IAssetManager);
            base._Ready();
            _assetManager = GameFrameworkEntry.GetModule<IAssetManager>();

            // 说明：镜像 Unity 侧 Start 生命周期，初始化推迟到节点进入场景树后执行。
            CallDeferred(nameof(InitializeInternal));
        }

        private void InitializeInternal()
        {
            _assetManager.Initialize();
        }

        /// <summary>
        /// 设置运行模式
        /// </summary>
        /// <param name="playMode">运行模式</param>
        public void SetPlayMode(EPlayMode playMode)
        {
            _assetManager.SetPlayMode(playMode);
        }

        #region 异步加载子资源对象

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        public Task<SubAssetsHandle> LoadSubAssetsAsync(AssetInfo assetInfo)
        {
            return _assetManager.LoadSubAssetsAsync(assetInfo);
        }

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type"></param>
        /// <returns></returns>
        public Task<SubAssetsHandle> LoadSubAssetsAsync(string path, Type type)
        {
            return _assetManager.LoadSubAssetsAsync(path, type);
        }

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        public Task<SubAssetsHandle> LoadSubAssetsAsync<T>(string path) where T : Resource
        {
            return _assetManager.LoadSubAssetsAsync<T>(path);
        }

        #endregion

        #region 同步加载子资源对象

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        public SubAssetsHandle LoadSubAssetSync(AssetInfo assetInfo)
        {
            return _assetManager.LoadSubAssetSync(assetInfo);
        }

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type"></param>
        /// <returns></returns>
        public SubAssetsHandle LoadSubAssetSync(string path, Type type)
        {
            return _assetManager.LoadSubAssetSync(path, type);
        }

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        public SubAssetsHandle LoadSubAssetSync<T>(string path) where T : Resource
        {
            return _assetManager.LoadSubAssetSync<T>(path);
        }

        #endregion

        #region 异步加载原生文件

        /// <summary>
        /// 异步加载原生文件
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        public Task<RawFileHandle> LoadRawFileAsync(AssetInfo assetInfo)
        {
            return _assetManager.LoadRawFileAsync(assetInfo);
        }

        /// <summary>
        /// 异步加载原生文件
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        public Task<RawFileHandle> LoadRawFileAsync(string path)
        {
            return _assetManager.LoadRawFileAsync(path);
        }

        #endregion

        #region 同步加载原生文件

        /// <summary>
        /// 同步加载原生文件
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        public RawFileHandle LoadRawFileSync(AssetInfo assetInfo)
        {
            return _assetManager.LoadRawFileSync(assetInfo);
        }

        /// <summary>
        /// 同步加载原生文件
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        public RawFileHandle LoadRawFileSync(string path)
        {
            return _assetManager.LoadRawFileSync(path);
        }

        #endregion

        #region 异步加载资源

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        public Task<AssetHandle> LoadAssetAsync(AssetInfo assetInfo)
        {
            return _assetManager.LoadAssetAsync(assetInfo);
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type">资源类型</param>
        /// <returns></returns>
        public Task<AssetHandle> LoadAssetAsync(string path, Type type)
        {
            return _assetManager.LoadAssetAsync(path, type);
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <typeparam name="T">资源类型</typeparam>
        /// <returns></returns>
        public Task<AssetHandle> LoadAssetAsync<T>(string path) where T : Resource
        {
            return _assetManager.LoadAssetAsync<T>(path);
        }

        /// <summary>
        /// 异步加载全部资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        public Task<AllAssetsHandle> LoadAllAssetsAsync<T>(string path) where T : Resource
        {
            return _assetManager.LoadAllAssetsAsync<T>(path);
        }

        /// <summary>
        /// 异步加载全部资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type">资源类型</param>
        /// <returns></returns>
        public Task<AllAssetsHandle> LoadAllAssetsAsync(string path, Type type)
        {
            return _assetManager.LoadAllAssetsAsync(path, type);
        }

        /// <summary>
        /// 异步加载资源包内所有资源对象
        /// </summary>
        /// <param name="path">资源的定位地址</param>
        public Task<AllAssetsHandle> LoadAllAssetsAsync(string path)
        {
            return _assetManager.LoadAllAssetsAsync(path);
        }

        /// <summary>
        /// 异步加载资源包内所有资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        public Task<AllAssetsHandle> LoadAllAssetsAsync(AssetInfo assetInfo)
        {
            return _assetManager.LoadAllAssetsAsync(assetInfo);
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        public Task<AssetHandle> LoadAssetAsync(string path)
        {
            return _assetManager.LoadAssetAsync(path);
        }

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="path">资源的定位地址</param>
        public SubAssetsHandle LoadSubAssetsAsync(string path)
        {
            return _assetManager.LoadSubAssetsAsync(path);
        }

        #endregion

        #region 同步加载资源

        /// <summary>
        /// 同步加载资源包内所有资源对象
        /// </summary>
        /// <param name="path">资源的定位地址</param>
        public AllAssetsHandle LoadAllAssetsSync(string path)
        {
            return _assetManager.LoadAllAssetsSync(path);
        }

        /// <summary>
        /// 同步加载资源包内所有资源对象
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="path">资源的定位地址</param>
        public AllAssetsHandle LoadAllAssetsSync<T>(string path) where T : Resource
        {
            return _assetManager.LoadAllAssetsSync<T>(path);
        }

        /// <summary>
        /// 同步加载资源包内所有资源对象
        /// </summary>
        /// <param name="path">资源的定位地址</param>
        /// <param name="type">子对象类型</param>
        public AllAssetsHandle LoadAllAssetsSync(string path, Type type)
        {
            return _assetManager.LoadAllAssetsSync(path, type);
        }

        /// <summary>
        /// 同步加载包内全部资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        public AllAssetsHandle LoadAllAssetsSync(AssetInfo assetInfo)
        {
            return _assetManager.LoadAllAssetsSync(assetInfo);
        }

        /// <summary>
        /// 同步加载子资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        public SubAssetsHandle LoadSubAssetSync(string path)
        {
            return _assetManager.LoadSubAssetSync(path);
        }

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        public AssetHandle LoadAssetSync(string path)
        {
            return _assetManager.LoadAssetSync(path);
        }

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type"></param>
        /// <returns></returns>
        public AssetHandle LoadAssetSync(string path, Type type)
        {
            return _assetManager.LoadAssetSync(path, type);
        }

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        public AssetHandle LoadAssetSync(AssetInfo assetInfo)
        {
            return _assetManager.LoadAssetSync(assetInfo);
        }

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        public AssetHandle LoadAssetSync<T>(string path) where T : Resource
        {
            return _assetManager.LoadAssetSync<T>(path);
        }

        #endregion

        #region 加载场景

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="sceneMode">场景模式</param>
        /// <param name="activateOnLoad">是否加载完成自动激活</param>
        /// <returns></returns>
        public Task<SceneHandle> LoadSceneAsync(string path, SceneLoadMode sceneMode, bool activateOnLoad = true)
        {
            return _assetManager.LoadSceneAsync(path, sceneMode, activateOnLoad);
        }

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="assetInfo">资源路径</param>
        /// <param name="sceneMode">场景模式</param>
        /// <param name="activateOnLoad">是否加载完成自动激活</param>
        /// <returns></returns>
        public Task<SceneHandle> LoadSceneAsync(AssetInfo assetInfo, SceneLoadMode sceneMode, bool activateOnLoad = true)
        {
            return _assetManager.LoadSceneAsync(assetInfo, sceneMode, activateOnLoad);
        }

        #endregion

        #region 资源包

        /// <summary>
        /// 创建资源包
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        /// <returns></returns>
        public ResourcePackage CreateAssetsPackage(string packageName)
        {
            return _assetManager.CreateAssetsPackage(packageName);
        }

        /// <summary>
        /// 尝试获取资源包
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        /// <returns></returns>
        public ResourcePackage TryGetAssetsPackage(string packageName)
        {
            return _assetManager.TryGetAssetsPackage(packageName);
        }

        /// <summary>
        /// 检查资源包是否存在
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        /// <returns></returns>
        public bool HasAssetsPackage(string packageName)
        {
            return _assetManager.HasAssetsPackage(packageName);
        }

        /// <summary>
        /// 获取资源包
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        /// <returns></returns>
        public ResourcePackage GetAssetsPackage(string packageName)
        {
            return _assetManager.GetAssetsPackage(packageName);
        }

        #endregion

        /// <summary>
        /// 是否需要下载
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        public bool IsNeedDownload(AssetInfo assetInfo)
        {
            return _assetManager.IsNeedDownload(assetInfo);
        }

        /// <summary>
        /// 是否需要下载
        /// </summary>
        /// <param name="path">资源地址</param>
        /// <returns></returns>
        public bool IsNeedDownload(string path)
        {
            return _assetManager.IsNeedDownload(path);
        }

        /// <summary>
        /// 获取资源信息
        /// </summary>
        /// <param name="assetTags">资源标签列表</param>
        /// <returns></returns>
        public AssetInfo[] GetAssetInfos(string[] assetTags)
        {
            return _assetManager.GetAssetInfos(assetTags);
        }

        /// <summary>
        /// 获取资源信息
        /// </summary>
        /// <param name="assetTag">资源标签</param>
        /// <returns></returns>
        public AssetInfo[] GetAssetInfos(string assetTag)
        {
            return _assetManager.GetAssetInfos(assetTag);
        }

        /// <summary>
        /// 获取资源信息
        /// </summary>
        public AssetInfo GetAssetInfo(string path)
        {
            return _assetManager.GetAssetInfo(path);
        }

        /// <summary>
        /// 检查指定的资源路径是否有效。
        /// </summary>
        /// <param name="assetPath">要检查的资源路径。</param>
        /// <returns>如果资源路径有效，则返回 true；否则返回 false。</returns>
        public bool HasAssetPath(string assetPath)
        {
            return _assetManager.HasAssetPath(assetPath);
        }

        /// <summary>
        /// 设置默认资源包
        /// </summary>
        /// <param name="resourcePackage">资源信息</param>
        /// <returns></returns>
        public void SetDefaultAssetsPackage(ResourcePackage resourcePackage)
        {
            _assetManager.SetDefaultAssetsPackage(resourcePackage);
        }

        /// <summary>
        /// 清理无用资源
        /// </summary>
        /// <param name="packageName">资源包名称,当packageName为 null 时,清理[默认]资源包</param>
        public void ClearUnusedBundleFilesAsync(string packageName = null)
        {
            _assetManager.ClearUnusedBundleFilesAsync(packageName);
        }

        /// <summary>
        /// 清理所有资源
        /// </summary>
        /// <param name="packageName">资源包名称,当packageName为 null 时,清理[默认]资源包</param>
        public void ClearAllBundleFilesAsync(string packageName = null)
        {
            _assetManager.ClearAllBundleFilesAsync(packageName);
        }

        /// <summary>
        /// 卸载无用资源
        /// </summary>
        /// <param name="packageName">资源包名称,当packageName为 null 时,卸载[默认]资源包</param>
        public void UnloadUnusedAssetsAsync(string packageName = null)
        {
            _assetManager.UnloadUnusedAssetsAsync(packageName);
        }

        /// <summary>
        /// 强制回收所有资源
        /// </summary>
        /// <param name="packageName">资源包名称,当packageName为 null 时,卸载[默认]资源包</param>
        public void UnloadAllAssetsAsync(string packageName = null)
        {
            _assetManager.UnloadAllAssetsAsync(packageName);
        }
    }
}
