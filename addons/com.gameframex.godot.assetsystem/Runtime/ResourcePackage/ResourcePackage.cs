using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using Godot;

namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    public class ResourcePackage
    {
        private bool _isInitialize = false;
        private string _initializeError = string.Empty;
        private EOperationStatus _initializeStatus = EOperationStatus.None;
        private EPlayMode _playMode;

        // 管理器
        private ResourceManager _resourceManager;
        private IBundleQuery _bundleQuery;
        private IPlayMode _playModeImpl;

        /// <summary>
        /// 包裹名
        /// </summary>
        public readonly string PackageName;

        /// <summary>
        /// 初始化状态
        /// </summary>
        public EOperationStatus InitializeStatus
        {
            get { return _initializeStatus; }
        }


        [AssetSystemPreserve]
        internal ResourcePackage(string packageName)
        {
            PackageName = packageName;
        }

        /// <summary>
        /// 更新资源包裹
        /// </summary>
        [AssetSystemPreserve]
        internal void UpdatePackage()
        {
            if (_playModeImpl != null)
            {
                _playModeImpl.UpdatePlayMode();
            }
        }

        /// <summary>
        /// 销毁资源包裹
        /// </summary>
        [AssetSystemPreserve]
        internal void DestroyPackage()
        {
            if (_isInitialize)
            {
                _isInitialize = false;
                _initializeError = string.Empty;
                _initializeStatus = EOperationStatus.None;

                _resourceManager = null;
                _bundleQuery = null;
                _playModeImpl = null;
            }
        }

        /// <summary>
        /// 异步初始化
        /// </summary>
        [AssetSystemPreserve]
        public InitializationOperation InitializeAsync(InitializeParameters parameters)
        {
            // 注意：联机平台因为网络原因可能会初始化失败！
            ResetInitializeAfterFailed();

            // 检测初始化参数合法性
            CheckInitializeParameters(parameters);

            // 创建资源管理器
            InitializationOperation initializeOperation;
            _resourceManager = new ResourceManager(PackageName);
            if (_playMode == EPlayMode.EditorSimulateMode)
            {
                var editorSimulateModeImpl = new EditorSimulateModeImpl(PackageName);
                _bundleQuery = editorSimulateModeImpl;
                _playModeImpl = editorSimulateModeImpl;
                _resourceManager.Initialize(parameters, _bundleQuery);

                var initializeParameters = parameters as EditorSimulateModeParameters;
                initializeOperation = editorSimulateModeImpl.InitializeAsync(initializeParameters);
            }
            else if (_playMode == EPlayMode.OfflinePlayMode)
            {
                var offlinePlayModeImpl = new OfflinePlayModeImpl(PackageName);
                _bundleQuery = offlinePlayModeImpl;
                _playModeImpl = offlinePlayModeImpl;
                _resourceManager.Initialize(parameters, _bundleQuery);

                var initializeParameters = parameters as OfflinePlayModeParameters;
                initializeOperation = offlinePlayModeImpl.InitializeAsync(initializeParameters);
            }
            else if (_playMode == EPlayMode.HostPlayMode)
            {
                var hostPlayModeImpl = new HostPlayModeImpl(PackageName);
                _bundleQuery = hostPlayModeImpl;
                _playModeImpl = hostPlayModeImpl;
                _resourceManager.Initialize(parameters, _bundleQuery);

                var initializeParameters = parameters as HostPlayModeParameters;
                initializeOperation = hostPlayModeImpl.InitializeAsync(initializeParameters);
            }
            else if (_playMode == EPlayMode.WebPlayMode)
            {
                var webPlayModeImpl = new WebPlayModeImpl(PackageName);
                _bundleQuery = webPlayModeImpl;
                _playModeImpl = webPlayModeImpl;
                _resourceManager.Initialize(parameters, _bundleQuery);

                var initializeParameters = parameters as WebPlayModeParameters;
                initializeOperation = webPlayModeImpl.InitializeAsync(initializeParameters);
            }
            else
            {
                throw new NotImplementedException();
            }

            // 监听初始化结果
            _isInitialize = true;
            initializeOperation.Completed += InitializeOperation_Completed;
            return initializeOperation;
        }

        [AssetSystemPreserve]
        private void ResetInitializeAfterFailed()
        {
            if (_isInitialize && _initializeStatus == EOperationStatus.Failed)
            {
                _isInitialize = false;
                _initializeStatus = EOperationStatus.None;
                _initializeError = string.Empty;
            }
        }

        [AssetSystemPreserve]
        private void CheckInitializeParameters(InitializeParameters parameters)
        {
            if (_isInitialize)
            {
                throw new Exception($"{nameof(ResourcePackage)} is initialized yet.");
            }

            if (parameters == null)
            {
                throw new Exception($"{nameof(ResourcePackage)} create parameters is null.");
            }

            if (parameters is EditorSimulateModeParameters)
            {
                if (Engine.IsEditorHint() == false)
                {
                    throw new Exception("Editor simulate mode only support Godot editor.");
                }
            }

            _playMode = ResolvePlayMode(parameters);
            CheckPlayModeParameterConstraints(parameters, _playMode);
            CheckPlayModePlatformConstraints(_playMode);
        }

        [AssetSystemPreserve]
        private EPlayMode ResolvePlayMode(InitializeParameters parameters)
        {
            if (parameters is EditorSimulateModeParameters)
            {
                return EPlayMode.EditorSimulateMode;
            }

            if (parameters is OfflinePlayModeParameters)
            {
                return EPlayMode.OfflinePlayMode;
            }

            if (parameters is HostPlayModeParameters)
            {
                return EPlayMode.HostPlayMode;
            }

            if (parameters is WebPlayModeParameters)
            {
                return EPlayMode.WebPlayMode;
            }

            throw new NotImplementedException();
        }

        [AssetSystemPreserve]
        private void CheckPlayModeParameterConstraints(InitializeParameters parameters, EPlayMode playMode)
        {
            if (playMode == EPlayMode.EditorSimulateMode)
            {
                var editorParameters = parameters as EditorSimulateModeParameters;
                if (editorParameters.EditorFileSystemParameters == null)
                {
                    throw new Exception("Editor file system parameters is null.");
                }
            }
            else if (playMode == EPlayMode.OfflinePlayMode)
            {
                var offlineParameters = parameters as OfflinePlayModeParameters;
                if (offlineParameters.BuildinFileSystemParameters == null)
                {
                    throw new Exception("Buildin file system parameters is null.");
                }
            }
            else if (playMode == EPlayMode.HostPlayMode)
            {
                var hostParameters = parameters as HostPlayModeParameters;
                if (hostParameters.CacheFileSystemParameters == null)
                {
                    throw new Exception("Cache file system parameters is null.");
                }
            }
            else if (playMode == EPlayMode.WebPlayMode)
            {
                var webParameters = parameters as WebPlayModeParameters;
                if (webParameters.WebFileSystemParameters == null)
                {
                    throw new Exception("Web file system parameters is null.");
                }
            }
        }

        [AssetSystemPreserve]
        private void CheckPlayModePlatformConstraints(EPlayMode playMode)
        {
            if (playMode == EPlayMode.EditorSimulateMode)
            {
                return;
            }

            var isWebPlatform = OS.HasFeature("web");
            if (isWebPlatform)
            {
                if (playMode != EPlayMode.WebPlayMode)
                {
                    throw new Exception($"{playMode} can not support web platform !");
                }
            }
            else
            {
                if (playMode == EPlayMode.WebPlayMode)
                {
                    throw new Exception($"{nameof(EPlayMode.WebPlayMode)} only support web platform !");
                }
            }
        }

        [AssetSystemPreserve]
        private void InitializeOperation_Completed(AsyncOperationBase op)
        {
            _initializeStatus = op.Status;
            _initializeError = op.Error;
        }

        /// <summary>
        /// 异步销毁
        /// </summary>
        [AssetSystemPreserve]
        public DestroyOperation DestroyAsync()
        {
            var operation = new DestroyOperation(this);
            OperationSystem.StartOperation(null, operation);
            return operation;
        }

        /// <summary>
        /// 请求本地资源版本
        /// </summary>
        /// <param name="appendTimeTicks">在URL末尾添加时间戳</param>
        /// <param name="timeout">超时时间（默认值：60秒）</param>
        [AssetSystemPreserve]
        public LoadLocalVersionOperation RequestLocalVersionAsync(bool appendTimeTicks = true, int timeout = 60)
        {
            DebugCheckInitialize(false);
            return _playModeImpl.LoadLocalVersionAsync(appendTimeTicks, timeout);
        }

        /// <summary>
        /// 请求本地资源并更新清单
        /// </summary>
        /// <param name="packageVersion">更新的包裹版本</param>
        /// <param name="timeout">超时时间（默认值：60秒）</param>
        [AssetSystemPreserve]
        public LoadLocalManifestOperation RequestLocalManifestAsync(string packageVersion, int timeout = 60)
        {
            DebugCheckInitialize(false);

            // 注意：强烈建议在更新之前保持加载器为空！
            if (_resourceManager.HasAnyLoader())
            {
                AssetSystemLogger.Warning($"Found loaded bundle before update manifest ! Recommended to call the  {nameof(UnloadAllAssetsAsync)} method to release loaded bundle !");
            }

            return _playModeImpl.LoadLocalManifestAsync(packageVersion, timeout);
        }

        /// <summary>
        /// 向网络端请求最新的资源版本
        /// </summary>
        /// <param name="appendTimeTicks">在URL末尾添加时间戳</param>
        /// <param name="timeout">超时时间（默认值：60秒）</param>
        [AssetSystemPreserve]
        public RequestPackageVersionOperation RequestPackageVersionAsync(bool appendTimeTicks = true, int timeout = 60)
        {
            DebugCheckInitialize(false);
            return _playModeImpl.RequestPackageVersionAsync(appendTimeTicks, timeout);
        }

        /// <summary>
        /// 向网络端请求并更新清单
        /// </summary>
        /// <param name="packageVersion">更新的包裹版本</param>
        /// <param name="timeout">超时时间（默认值：60秒）</param>
        [AssetSystemPreserve]
        public UpdatePackageManifestOperation UpdatePackageManifestAsync(string packageVersion, int timeout = 60)
        {
            DebugCheckInitialize(false);

            // 注意：强烈建议在更新之前保持加载器为空！
            if (_resourceManager.HasAnyLoader())
            {
                AssetSystemLogger.Warning($"Found loaded bundle before update manifest ! Recommended to call the  {nameof(UnloadAllAssetsAsync)} method to release loaded bundle !");
            }

            return _playModeImpl.UpdatePackageManifestAsync(packageVersion, timeout);
        }

        /// <summary>
        /// 预下载指定版本的包裹资源
        /// </summary>
        /// <param name="packageVersion">下载的包裹版本</param>
        /// <param name="timeout">超时时间（默认值：60秒）</param>
        [AssetSystemPreserve]
        public PreDownloadContentOperation PreDownloadContentAsync(string packageVersion, int timeout = 60)
        {
            DebugCheckInitialize(false);
            return _playModeImpl.PreDownloadContentAsync(packageVersion, timeout);
        }

        /// <summary>
        /// 清理文件系统所有的资源文件
        /// </summary>
        [AssetSystemPreserve]
        public ClearAllBundleFilesOperation ClearAllBundleFilesAsync()
        {
            DebugCheckInitialize();
            return _playModeImpl.ClearAllBundleFilesAsync();
        }

        /// <summary>
        /// 清理文件系统未使用的资源文件
        /// </summary>
        [AssetSystemPreserve]
        public ClearUnusedBundleFilesOperation ClearUnusedBundleFilesAsync()
        {
            DebugCheckInitialize();
            return _playModeImpl.ClearUnusedBundleFilesAsync();
        }

        /// <summary>
        /// 获取本地包裹的版本信息
        /// </summary>
        [AssetSystemPreserve]
        public string GetPackageVersion()
        {
            DebugCheckInitialize();
            return _playModeImpl.ActiveManifest.PackageVersion;
        }

        #region 资源回收

        /// <summary>
        /// 强制回收所有资源
        /// </summary>
        [AssetSystemPreserve]
        public UnloadAllAssetsOperation UnloadAllAssetsAsync()
        {
            DebugCheckInitialize();
            var operation = new UnloadAllAssetsOperation(_resourceManager);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }

        /// <summary>
        /// 回收不再使用的资源
        /// 说明：卸载引用计数为零的资源
        /// </summary>
        [AssetSystemPreserve]
        public UnloadUnusedAssetsOperation UnloadUnusedAssetsAsync()
        {
            DebugCheckInitialize();
            var operation = new UnloadUnusedAssetsOperation(_resourceManager);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }

        /// <summary>
        /// 资源回收
        /// 说明：尝试卸载指定的资源
        /// </summary>
        [AssetSystemPreserve]
        public void TryUnloadUnusedAsset(string location)
        {
            DebugCheckInitialize();
            var assetInfo = ConvertLocationToAssetInfo(location, null);
            _resourceManager.TryUnloadUnusedAsset(assetInfo);
        }

        /// <summary>
        /// 资源回收
        /// 说明：尝试卸载指定的资源
        /// </summary>
        [AssetSystemPreserve]
        public void TryUnloadUnusedAsset(AssetInfo assetInfo)
        {
            DebugCheckInitialize();
            _resourceManager.TryUnloadUnusedAsset(assetInfo);
        }

        #endregion

        #region 资源信息

        /// <summary>
        /// 是否需要从远端更新下载
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        [AssetSystemPreserve]
        public bool IsNeedDownloadFromRemote(string location)
        {
            DebugCheckInitialize();
            var assetInfo = ConvertLocationToAssetInfo(location, null);
            return IsNeedDownloadFromRemoteInternal(assetInfo);
        }

        /// <summary>
        /// 是否需要从远端更新下载
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        [AssetSystemPreserve]
        public bool IsNeedDownloadFromRemote(AssetInfo assetInfo)
        {
            DebugCheckInitialize();
            return IsNeedDownloadFromRemoteInternal(assetInfo);
        }

        /// <summary>
        /// 获取资源信息列表
        /// </summary>
        /// <param name="tag">资源标签</param>
        [AssetSystemPreserve]
        public AssetInfo[] GetAssetInfos(string tag)
        {
            DebugCheckInitialize();
            var tags = new string[] { tag, };
            return _playModeImpl.ActiveManifest.GetAssetsInfoByTags(tags);
        }

        /// <summary>
        /// 获取资源信息列表
        /// </summary>
        /// <param name="tags">资源标签列表</param>
        [AssetSystemPreserve]
        public AssetInfo[] GetAssetInfos(string[] tags)
        {
            DebugCheckInitialize();
            return _playModeImpl.ActiveManifest.GetAssetsInfoByTags(tags);
        }

        /// <summary>
        /// 获取资源信息
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        [AssetSystemPreserve]
        public AssetInfo GetAssetInfo(string location)
        {
            DebugCheckInitialize();
            return ConvertLocationToAssetInfo(location, null);
        }

        /// <summary>
        /// 获取资源信息
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">资源类型</param>
        [AssetSystemPreserve]
        public AssetInfo GetAssetInfo(string location, Type type)
        {
            DebugCheckInitialize();
            return ConvertLocationToAssetInfo(location, type);
        }

        /// <summary>
        /// 获取资源信息
        /// </summary>
        /// <param name="assetGUID">资源GUID</param>
        [AssetSystemPreserve]
        public AssetInfo GetAssetInfoByGUID(string assetGUID)
        {
            DebugCheckInitialize();
            return ConvertAssetGUIDToAssetInfo(assetGUID, null);
        }

        /// <summary>
        /// 获取资源信息
        /// </summary>
        /// <param name="assetGUID">资源GUID</param>
        /// <param name="type">资源类型</param>
        [AssetSystemPreserve]
        public AssetInfo GetAssetInfoByGUID(string assetGUID, Type type)
        {
            DebugCheckInitialize();
            return ConvertAssetGUIDToAssetInfo(assetGUID, type);
        }

        /// <summary>
        /// 检查资源定位地址是否有效
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        [AssetSystemPreserve]
        public bool CheckLocationValid(string location)
        {
            DebugCheckInitialize();
            var assetPath = _playModeImpl.ActiveManifest.TryMappingToAssetPath(location);
            return string.IsNullOrEmpty(assetPath) == false;
        }

        /// <summary>
        /// 尝试使用不带路径的资源名查询清单内的资源定位地址。
        /// </summary>
        [AssetSystemPreserve]
        public bool TryGetAssetLocationByName(string assetName, out string location)
        {
            DebugCheckInitialize();
            return _playModeImpl.ActiveManifest.TryMappingAssetNameToAssetPath(assetName, out location);
        }

        [AssetSystemPreserve]
        private bool IsNeedDownloadFromRemoteInternal(AssetInfo assetInfo)
        {
            if (assetInfo.IsInvalid)
            {
                AssetSystemLogger.Warning(assetInfo.Error);
                return false;
            }

            var bundleInfo = _bundleQuery.GetMainBundleInfo(assetInfo);
            if (bundleInfo.IsNeedDownloadFromRemote())
            {
                return true;
            }

            var depends = _bundleQuery.GetDependBundleInfos(assetInfo);
            foreach (var depend in depends)
            {
                if (depend.IsNeedDownloadFromRemote())
                {
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region 原生文件

        /// <summary>
        /// 同步加载原生文件
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        [AssetSystemPreserve]
        public RawFileHandle LoadRawFileSync(AssetInfo assetInfo)
        {
            DebugCheckInitialize();
            return LoadRawFileInternal(assetInfo, true, 0);
        }

        /// <summary>
        /// 同步加载原生文件
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        [AssetSystemPreserve]
        public RawFileHandle LoadRawFileSync(string location)
        {
            DebugCheckInitialize();
            var assetInfo = ConvertLocationToAssetInfo(location, null);
            return LoadRawFileInternal(assetInfo, true, 0);
        }

        /// <summary>
        /// 异步加载原生文件
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <param name="priority">加载的优先级</param>
        [AssetSystemPreserve]
        public RawFileHandle LoadRawFileAsync(AssetInfo assetInfo, uint priority = 0)
        {
            DebugCheckInitialize();
            return LoadRawFileInternal(assetInfo, false, priority);
        }

        /// <summary>
        /// 异步加载原生文件
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="priority">加载的优先级</param>
        [AssetSystemPreserve]
        public RawFileHandle LoadRawFileAsync(string location, uint priority = 0)
        {
            DebugCheckInitialize();
            var assetInfo = ConvertLocationToAssetInfo(location, null);
            return LoadRawFileInternal(assetInfo, false, priority);
        }


        [AssetSystemPreserve]
        private RawFileHandle LoadRawFileInternal(AssetInfo assetInfo, bool waitForAsyncComplete, uint priority)
        {
            var handle = _resourceManager.LoadRawFileAsync(assetInfo, priority);
            if (waitForAsyncComplete)
            {
                handle.WaitForAsyncComplete();
            }

            return handle;
        }

        #endregion

        #region 场景加载

        /// <summary>
        /// 同步加载场景
        /// </summary>
        /// <param name="location">场景的定位地址</param>
        /// <param name="sceneMode">场景加载模式</param>
        /// <param name="physicsMode">场景物理模式</param>
        [AssetSystemPreserve]
        public SceneHandle LoadSceneSync(string location, SceneLoadMode sceneMode = SceneLoadMode.Single, ScenePhysicsMode physicsMode = ScenePhysicsMode.None)
        {
            DebugCheckInitialize();
            var assetInfo = ConvertLocationToAssetInfo(location, null);
            return LoadSceneInternal(assetInfo, true, sceneMode, physicsMode, false, 0);
        }

        /// <summary>
        /// 同步加载场景
        /// </summary>
        /// <param name="assetInfo">场景的资源信息</param>
        /// <param name="sceneMode">场景加载模式</param>
        /// <param name="physicsMode">场景物理模式</param>
        [AssetSystemPreserve]
        public SceneHandle LoadSceneSync(AssetInfo assetInfo, SceneLoadMode sceneMode = SceneLoadMode.Single, ScenePhysicsMode physicsMode = ScenePhysicsMode.None)
        {
            DebugCheckInitialize();
            return LoadSceneInternal(assetInfo, true, sceneMode, physicsMode, false, 0);
        }

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="location">场景的定位地址</param>
        /// <param name="sceneMode">场景加载模式</param>
        /// <param name="physicsMode">场景物理模式</param>
        /// <param name="suspendLoad">场景加载到90%自动挂起</param>
        /// <param name="priority">加载的优先级</param>
        [AssetSystemPreserve]
        public SceneHandle LoadSceneAsync(string location, SceneLoadMode sceneMode = SceneLoadMode.Single, ScenePhysicsMode physicsMode = ScenePhysicsMode.None, bool suspendLoad = false, uint priority = 0)
        {
            DebugCheckInitialize();
            var assetInfo = ConvertLocationToAssetInfo(location, null);
            return LoadSceneInternal(assetInfo, false, sceneMode, physicsMode, suspendLoad, priority);
        }

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="assetInfo">场景的资源信息</param>
        /// <param name="sceneMode">场景加载模式</param>
        /// <param name="physicsMode">场景物理模式</param>
        /// <param name="suspendLoad">场景加载到90%自动挂起</param>
        /// <param name="priority">加载的优先级</param>
        [AssetSystemPreserve]
        public SceneHandle LoadSceneAsync(AssetInfo assetInfo, SceneLoadMode sceneMode = SceneLoadMode.Single, ScenePhysicsMode physicsMode = ScenePhysicsMode.None, bool suspendLoad = false, uint priority = 0)
        {
            DebugCheckInitialize();
            return LoadSceneInternal(assetInfo, false, sceneMode, physicsMode, suspendLoad, priority);
        }

        [AssetSystemPreserve]
        private SceneHandle LoadSceneInternal(AssetInfo assetInfo, bool waitForAsyncComplete, SceneLoadMode sceneMode, ScenePhysicsMode physicsMode, bool suspendLoad, uint priority)
        {
            DebugCheckAssetLoadType(assetInfo.AssetType);
            var loadSceneParams = new SceneLoadParameters(sceneMode, physicsMode);
            var handle = _resourceManager.LoadSceneAsync(assetInfo, loadSceneParams, suspendLoad, priority);
            if (waitForAsyncComplete)
            {
                handle.WaitForAsyncComplete();
            }

            return handle;
        }

        #endregion

        #region 资源加载

        /// <summary>
        /// 同步加载资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        [AssetSystemPreserve]
        public AssetHandle LoadAssetSync(AssetInfo assetInfo)
        {
            DebugCheckInitialize();
            return LoadAssetInternal(assetInfo, true, 0);
        }

        /// <summary>
        /// 同步加载资源对象
        /// </summary>
        /// <typeparam name="TObject">资源类型</typeparam>
        /// <param name="location">资源的定位地址</param>
        [AssetSystemPreserve]
        public AssetHandle LoadAssetSync<TObject>(string location)
        {
            DebugCheckInitialize();
            var assetInfo = ConvertLocationToAssetInfo(location, typeof(TObject));
            return LoadAssetInternal(assetInfo, true, 0);
        }

        /// <summary>
        /// 同步加载资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">资源类型</param>
        [AssetSystemPreserve]
        public AssetHandle LoadAssetSync(string location, Type type)
        {
            DebugCheckInitialize();
            var assetInfo = ConvertLocationToAssetInfo(location, type);
            return LoadAssetInternal(assetInfo, true, 0);
        }

        /// <summary>
        /// 同步加载资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        [AssetSystemPreserve]
        public AssetHandle LoadAssetSync(string location)
        {
            DebugCheckInitialize();
            var type = typeof(object);
            var assetInfo = ConvertLocationToAssetInfo(location, type);
            return LoadAssetInternal(assetInfo, true, 0);
        }


        /// <summary>
        /// 异步加载资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <param name="priority">加载的优先级</param>
        [AssetSystemPreserve]
        public AssetHandle LoadAssetAsync(AssetInfo assetInfo, uint priority = 0)
        {
            DebugCheckInitialize();
            return LoadAssetInternal(assetInfo, false, priority);
        }

        /// <summary>
        /// 异步加载资源对象
        /// </summary>
        /// <typeparam name="TObject">资源类型</typeparam>
        /// <param name="location">资源的定位地址</param>
        /// <param name="priority">加载的优先级</param>
        [AssetSystemPreserve]
        public AssetHandle LoadAssetAsync<TObject>(string location, uint priority = 0)
        {
            DebugCheckInitialize();
            var assetInfo = ConvertLocationToAssetInfo(location, typeof(TObject));
            return LoadAssetInternal(assetInfo, false, priority);
        }

        /// <summary>
        /// 异步加载资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">资源类型</param>
        /// <param name="priority">加载的优先级</param>
        [AssetSystemPreserve]
        public AssetHandle LoadAssetAsync(string location, Type type, uint priority = 0)
        {
            DebugCheckInitialize();
            var assetInfo = ConvertLocationToAssetInfo(location, type);
            return LoadAssetInternal(assetInfo, false, priority);
        }

        /// <summary>
        /// 异步加载资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="priority">加载的优先级</param>
        [AssetSystemPreserve]
        public AssetHandle LoadAssetAsync(string location, uint priority = 0)
        {
            DebugCheckInitialize();
            var type = typeof(object);
            var assetInfo = ConvertLocationToAssetInfo(location, type);
            return LoadAssetInternal(assetInfo, false, priority);
        }


        [AssetSystemPreserve]
        private AssetHandle LoadAssetInternal(AssetInfo assetInfo, bool waitForAsyncComplete, uint priority)
        {
            DebugCheckAssetLoadType(assetInfo.AssetType);
            var handle = _resourceManager.LoadAssetAsync(assetInfo, priority);
            if (waitForAsyncComplete)
            {
                handle.WaitForAsyncComplete();
            }

            return handle;
        }

        #endregion

        #region 资源加载

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        [AssetSystemPreserve]
        public SubAssetsHandle LoadSubAssetsSync(AssetInfo assetInfo)
        {
            DebugCheckInitialize();
            return LoadSubAssetsInternal(assetInfo, true, 0);
        }

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <typeparam name="TObject">资源类型</typeparam>
        /// <param name="location">资源的定位地址</param>
        [AssetSystemPreserve]
        public SubAssetsHandle LoadSubAssetsSync<TObject>(string location)
        {
            DebugCheckInitialize();
            var assetInfo = ConvertLocationToAssetInfo(location, typeof(TObject));
            return LoadSubAssetsInternal(assetInfo, true, 0);
        }

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">子对象类型</param>
        [AssetSystemPreserve]
        public SubAssetsHandle LoadSubAssetsSync(string location, Type type)
        {
            DebugCheckInitialize();
            var assetInfo = ConvertLocationToAssetInfo(location, type);
            return LoadSubAssetsInternal(assetInfo, true, 0);
        }

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        [AssetSystemPreserve]
        public SubAssetsHandle LoadSubAssetsSync(string location)
        {
            DebugCheckInitialize();
            var type = typeof(object);
            var assetInfo = ConvertLocationToAssetInfo(location, type);
            return LoadSubAssetsInternal(assetInfo, true, 0);
        }


        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <param name="priority">加载的优先级</param>
        [AssetSystemPreserve]
        public SubAssetsHandle LoadSubAssetsAsync(AssetInfo assetInfo, uint priority = 0)
        {
            DebugCheckInitialize();
            return LoadSubAssetsInternal(assetInfo, false, priority);
        }

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <typeparam name="TObject">资源类型</typeparam>
        /// <param name="location">资源的定位地址</param>
        /// <param name="priority">加载的优先级</param>
        [AssetSystemPreserve]
        public SubAssetsHandle LoadSubAssetsAsync<TObject>(string location, uint priority = 0)
        {
            DebugCheckInitialize();
            var assetInfo = ConvertLocationToAssetInfo(location, typeof(TObject));
            return LoadSubAssetsInternal(assetInfo, false, priority);
        }

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">子对象类型</param>
        /// <param name="priority">加载的优先级</param>
        [AssetSystemPreserve]
        public SubAssetsHandle LoadSubAssetsAsync(string location, Type type, uint priority = 0)
        {
            DebugCheckInitialize();
            var assetInfo = ConvertLocationToAssetInfo(location, type);
            return LoadSubAssetsInternal(assetInfo, false, priority);
        }

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="priority">加载的优先级</param>
        [AssetSystemPreserve]
        public SubAssetsHandle LoadSubAssetsAsync(string location, uint priority = 0)
        {
            DebugCheckInitialize();
            var type = typeof(object);
            var assetInfo = ConvertLocationToAssetInfo(location, type);
            return LoadSubAssetsInternal(assetInfo, false, priority);
        }


        [AssetSystemPreserve]
        private SubAssetsHandle LoadSubAssetsInternal(AssetInfo assetInfo, bool waitForAsyncComplete, uint priority)
        {
            DebugCheckAssetLoadType(assetInfo.AssetType);
            var handle = _resourceManager.LoadSubAssetsAsync(assetInfo, priority);
            if (waitForAsyncComplete)
            {
                handle.WaitForAsyncComplete();
            }

            return handle;
        }

        #endregion

        #region 资源加载

        /// <summary>
        /// 同步加载资源包内所有资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        [AssetSystemPreserve]
        public AllAssetsHandle LoadAllAssetsSync(AssetInfo assetInfo)
        {
            DebugCheckInitialize();
            return LoadAllAssetsInternal(assetInfo, true, 0);
        }

        /// <summary>
        /// 同步加载资源包内所有资源对象
        /// </summary>
        /// <typeparam name="TObject">资源类型</typeparam>
        /// <param name="location">资源的定位地址</param>
        [AssetSystemPreserve]
        public AllAssetsHandle LoadAllAssetsSync<TObject>(string location)
        {
            DebugCheckInitialize();
            var assetInfo = ConvertLocationToAssetInfo(location, typeof(TObject));
            return LoadAllAssetsInternal(assetInfo, true, 0);
        }

        /// <summary>
        /// 同步加载资源包内所有资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">子对象类型</param>
        [AssetSystemPreserve]
        public AllAssetsHandle LoadAllAssetsSync(string location, Type type)
        {
            DebugCheckInitialize();
            var assetInfo = ConvertLocationToAssetInfo(location, type);
            return LoadAllAssetsInternal(assetInfo, true, 0);
        }

        /// <summary>
        /// 同步加载资源包内所有资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        [AssetSystemPreserve]
        public AllAssetsHandle LoadAllAssetsSync(string location)
        {
            DebugCheckInitialize();
            var type = typeof(object);
            var assetInfo = ConvertLocationToAssetInfo(location, type);
            return LoadAllAssetsInternal(assetInfo, true, 0);
        }


        /// <summary>
        /// 异步加载资源包内所有资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <param name="priority">加载的优先级</param>
        [AssetSystemPreserve]
        public AllAssetsHandle LoadAllAssetsAsync(AssetInfo assetInfo, uint priority = 0)
        {
            DebugCheckInitialize();
            return LoadAllAssetsInternal(assetInfo, false, priority);
        }

        /// <summary>
        /// 异步加载资源包内所有资源对象
        /// </summary>
        /// <typeparam name="TObject">资源类型</typeparam>
        /// <param name="location">资源的定位地址</param>
        /// <param name="priority">加载的优先级</param>
        [AssetSystemPreserve]
        public AllAssetsHandle LoadAllAssetsAsync<TObject>(string location, uint priority = 0)
        {
            DebugCheckInitialize();
            var assetInfo = ConvertLocationToAssetInfo(location, typeof(TObject));
            return LoadAllAssetsInternal(assetInfo, false, priority);
        }

        /// <summary>
        /// 异步加载资源包内所有资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="type">子对象类型</param>
        /// <param name="priority">加载的优先级</param>
        [AssetSystemPreserve]
        public AllAssetsHandle LoadAllAssetsAsync(string location, Type type, uint priority = 0)
        {
            DebugCheckInitialize();
            var assetInfo = ConvertLocationToAssetInfo(location, type);
            return LoadAllAssetsInternal(assetInfo, false, priority);
        }

        /// <summary>
        /// 异步加载资源包内所有资源对象
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="priority">加载的优先级</param>
        [AssetSystemPreserve]
        public AllAssetsHandle LoadAllAssetsAsync(string location, uint priority = 0)
        {
            DebugCheckInitialize();
            var type = typeof(object);
            var assetInfo = ConvertLocationToAssetInfo(location, type);
            return LoadAllAssetsInternal(assetInfo, false, priority);
        }


        [AssetSystemPreserve]
        private AllAssetsHandle LoadAllAssetsInternal(AssetInfo assetInfo, bool waitForAsyncComplete, uint priority)
        {
            DebugCheckAssetLoadType(assetInfo.AssetType);
            var handle = _resourceManager.LoadAllAssetsAsync(assetInfo, priority);
            if (waitForAsyncComplete)
            {
                handle.WaitForAsyncComplete();
            }

            return handle;
        }

        #endregion

        #region 资源下载

        /// <summary>
        /// 创建资源下载器，用于下载当前资源版本所有的资源包文件
        /// </summary>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        /// <param name="timeout">超时时间</param>
        [AssetSystemPreserve]
        public ResourceDownloaderOperation CreateResourceDownloader(int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            DebugCheckInitialize();
            return _playModeImpl.CreateResourceDownloaderByAll(downloadingMaxNumber, failedTryAgain, timeout);
        }

        /// <summary>
        /// 创建资源下载器，用于下载指定的资源标签关联的资源包文件
        /// </summary>
        /// <param name="tag">资源标签</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        /// <param name="timeout">超时时间</param>
        [AssetSystemPreserve]
        public ResourceDownloaderOperation CreateResourceDownloader(string tag, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            DebugCheckInitialize();
            return _playModeImpl.CreateResourceDownloaderByTags(new string[] { tag, }, downloadingMaxNumber, failedTryAgain, timeout);
        }

        /// <summary>
        /// 创建资源下载器，用于下载指定的资源标签列表关联的资源包文件
        /// </summary>
        /// <param name="tags">资源标签列表</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        /// <param name="timeout">超时时间</param>
        [AssetSystemPreserve]
        public ResourceDownloaderOperation CreateResourceDownloader(string[] tags, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            DebugCheckInitialize();
            return _playModeImpl.CreateResourceDownloaderByTags(tags, downloadingMaxNumber, failedTryAgain, timeout);
        }

        /// <summary>
        /// 创建资源下载器，用于下载指定的资源依赖的资源包文件
        /// </summary>
        /// <param name="location">资源的定位地址</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        /// <param name="timeout">超时时间</param>
        [AssetSystemPreserve]
        public ResourceDownloaderOperation CreateBundleDownloader(string location, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            DebugCheckInitialize();
            var assetInfo = ConvertLocationToAssetInfo(location, null);
            var assetInfos = new AssetInfo[] { assetInfo, };
            return _playModeImpl.CreateResourceDownloaderByPaths(assetInfos, downloadingMaxNumber, failedTryAgain, timeout);
        }

        /// <summary>
        /// 创建资源下载器，用于下载指定的资源列表依赖的资源包文件
        /// </summary>
        /// <param name="locations">资源的定位地址列表</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        /// <param name="timeout">超时时间</param>
        [AssetSystemPreserve]
        public ResourceDownloaderOperation CreateBundleDownloader(string[] locations, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            DebugCheckInitialize();
            var assetInfos = new List<AssetInfo>(locations.Length);
            foreach (var location in locations)
            {
                var assetInfo = ConvertLocationToAssetInfo(location, null);
                assetInfos.Add(assetInfo);
            }

            return _playModeImpl.CreateResourceDownloaderByPaths(assetInfos.ToArray(), downloadingMaxNumber, failedTryAgain, timeout);
        }

        /// <summary>
        /// 创建资源下载器，用于下载指定的资源依赖的资源包文件
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        /// <param name="timeout">超时时间</param>
        [AssetSystemPreserve]
        public ResourceDownloaderOperation CreateBundleDownloader(AssetInfo assetInfo, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            DebugCheckInitialize();
            var assetInfos = new AssetInfo[] { assetInfo, };
            return _playModeImpl.CreateResourceDownloaderByPaths(assetInfos, downloadingMaxNumber, failedTryAgain, timeout);
        }

        /// <summary>
        /// 创建资源下载器，用于下载指定的资源列表依赖的资源包文件
        /// </summary>
        /// <param name="assetInfos">资源信息列表</param>
        /// <param name="downloadingMaxNumber">同时下载的最大文件数</param>
        /// <param name="failedTryAgain">下载失败的重试次数</param>
        /// <param name="timeout">超时时间</param>
        [AssetSystemPreserve]
        public ResourceDownloaderOperation CreateBundleDownloader(AssetInfo[] assetInfos, int downloadingMaxNumber, int failedTryAgain, int timeout = 60)
        {
            DebugCheckInitialize();
            return _playModeImpl.CreateResourceDownloaderByPaths(assetInfos, downloadingMaxNumber, failedTryAgain, timeout);
        }

        #endregion

        #region 资源解压

        /// <summary>
        /// 创建内置资源解压器，用于解压当前资源版本所有的资源包文件
        /// </summary>
        /// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
        /// <param name="failedTryAgain">解压失败的重试次数</param>
        [AssetSystemPreserve]
        public ResourceUnpackerOperation CreateResourceUnpacker(int unpackingMaxNumber, int failedTryAgain)
        {
            DebugCheckInitialize();
            return _playModeImpl.CreateResourceUnpackerByAll(unpackingMaxNumber, failedTryAgain, int.MaxValue);
        }

        /// <summary>
        /// 创建内置资源解压器，用于解压指定的资源标签关联的资源包文件
        /// </summary>
        /// <param name="tag">资源标签</param>
        /// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
        /// <param name="failedTryAgain">解压失败的重试次数</param>
        [AssetSystemPreserve]
        public ResourceUnpackerOperation CreateResourceUnpacker(string tag, int unpackingMaxNumber, int failedTryAgain)
        {
            DebugCheckInitialize();
            return _playModeImpl.CreateResourceUnpackerByTags(new string[] { tag, }, unpackingMaxNumber, failedTryAgain, int.MaxValue);
        }

        /// <summary>
        /// 创建内置资源解压器，用于解压指定的资源标签列表关联的资源包文件
        /// </summary>
        /// <param name="tags">资源标签列表</param>
        /// <param name="unpackingMaxNumber">同时解压的最大文件数</param>
        /// <param name="failedTryAgain">解压失败的重试次数</param>
        [AssetSystemPreserve]
        public ResourceUnpackerOperation CreateResourceUnpacker(string[] tags, int unpackingMaxNumber, int failedTryAgain)
        {
            DebugCheckInitialize();
            return _playModeImpl.CreateResourceUnpackerByTags(tags, unpackingMaxNumber, failedTryAgain, int.MaxValue);
        }

        #endregion

        #region 资源导入

        /// <summary>
        /// 创建资源导入器
        /// 注意：资源文件名称必须和资源服务器部署的文件名称一致！
        /// </summary>
        /// <param name="filePaths">资源路径列表</param>
        /// <param name="importerMaxNumber">同时导入的最大文件数</param>
        /// <param name="failedTryAgain">导入失败的重试次数</param>
        [AssetSystemPreserve]
        public ResourceImporterOperation CreateResourceImporter(string[] filePaths, int importerMaxNumber, int failedTryAgain)
        {
            DebugCheckInitialize();
            return _playModeImpl.CreateResourceImporterByFilePaths(filePaths, importerMaxNumber, failedTryAgain, int.MaxValue);
        }

        #endregion

        #region 内部方法

        [AssetSystemPreserve]
        private AssetInfo ConvertLocationToAssetInfo(string location, Type assetType)
        {
            return _playModeImpl.ActiveManifest.ConvertLocationToAssetInfo(location, assetType);
        }

        [AssetSystemPreserve]
        private AssetInfo ConvertAssetGUIDToAssetInfo(string assetGUID, Type assetType)
        {
            return _playModeImpl.ActiveManifest.ConvertAssetGUIDToAssetInfo(assetGUID, assetType);
        }

        #endregion

        #region 调试方法

        [AssetSystemPreserve]
        [Conditional("DEBUG")]
        private void DebugCheckInitialize(bool checkActiveManifest = true)
        {
            if (_initializeStatus == EOperationStatus.None)
            {
                throw new Exception("Package initialize not completed !");
            }
            else if (_initializeStatus == EOperationStatus.Failed)
            {
                throw new Exception($"Package initialize failed ! {_initializeError}");
            }

            if (checkActiveManifest)
            {
                if (_playModeImpl.ActiveManifest == null)
                {
                    throw new Exception("Can not found active package manifest !");
                }
            }
        }

        [AssetSystemPreserve]
        [Conditional("DEBUG")]
        private void DebugCheckAssetLoadType(Type type)
        {
            // Godot resources are not rooted in a single common asset base type.
            // Keep this hook for debug builds, but let the backend decide whether a
            // concrete asset type is supported.
        }

        #endregion

        #region 调试信息

        [AssetSystemPreserve]
        internal DebugPackageData GetDebugPackageData()
        {
            var data = new DebugPackageData();
            data.PackageName = PackageName;
            data.ProviderInfos = _resourceManager.GetDebugReportInfos();
            return data;
        }

        #endregion
    }
}
