using System;
using System.Threading.Tasks;
using GameFrameX.Runtime;
using UnityEngine.SceneManagement;
using YooAsset;
using Object = UnityEngine.Object;

namespace GameFrameX.Asset.Runtime
{
    /// <summary>
    /// 资源组件。
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public partial class AssetManager : GameFrameworkModule, IAssetManager
    {
        /// <summary>
        /// 默认包名称
        /// </summary>
        public string DefaultPackageName { get; set; } = ConstDefaultPackageName;

        /// <summary>
        /// 最大并发下载数量
        /// </summary>
        public int DownloadingMaxNum { get; set; }

        /// <summary>
        /// 失败重试次数
        /// </summary>
        public int FailedTryAgain { get; set; }

        /// <summary>
        /// 文件验证等级
        /// </summary>
        public EFileVerifyLevel VerifyLevel { get; set; }

        /// <summary>
        /// 操作系统最大时间片（单位：毫秒）
        /// </summary>
        public long Milliseconds { get; set; }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        public void Initialize()
        {
            BetterStreamingAssets.Initialize();
            Log.Info($"资源系统运行模式：{PlayMode}");
            YooAssets.SetAutoCreateDriver(false);
            YooAssets.Initialize();
            YooAssets.SetOperationSystemMaxTimeSlice(30);
            // YooAssets.SetCacheSystemCachedFileVerifyLevel(EVerifyLevel.High);
            // YooAssets.SetDownloadSystemBreakpointResumeFileSize(4096 * 8);

            Log.Info("Asset Init Over");
        }


        /// <summary>
        /// 初始化操作。
        /// </summary>
        /// <param name="packageName">包名称</param>
        /// <param name="hostServerURL">热更链接URL。</param>
        /// <param name="fallbackHostServerURL">备用热更链接URL</param>
        /// <param name="isDefaultPackage">是否是默认包</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        public Task<bool> InitPackageAsync(string packageName, string hostServerURL, string fallbackHostServerURL, bool isDefaultPackage = false)
        {
            var taskCompletionSource = new TaskCompletionSource<bool>();
            GameFrameworkGuard.NotNull(packageName, nameof(packageName));
            GameFrameworkGuard.NotNull(hostServerURL, nameof(hostServerURL));
            GameFrameworkGuard.NotNull(fallbackHostServerURL, nameof(fallbackHostServerURL));

            // 创建默认的资源包
            var resourcePackage = YooAssets.TryGetPackage(packageName);
            if (resourcePackage == null)
            {
                resourcePackage = YooAssets.CreatePackage(packageName);
                if (isDefaultPackage)
                {
                    // 设置该资源包为默认的资源包，可以使用YooAssets相关加载接口加载该资源包内容。
                    YooAssets.SetDefaultPackage(resourcePackage);
                }
            }

            var initializationOperationHandler = CreateInitializationOperationHandler(resourcePackage, hostServerURL, fallbackHostServerURL);
            initializationOperationHandler.Completed += asyncOperationBase =>
            {
                if (asyncOperationBase.Error == null && asyncOperationBase.Status == EOperationStatus.Succeed && asyncOperationBase.IsDone)
                {
                    taskCompletionSource.TrySetResult(true);
                }
                else
                {
                    taskCompletionSource.TrySetException(new Exception(asyncOperationBase.Error));
                }
            };
            return taskCompletionSource.Task;
        }

        /// <summary>
        /// 卸载资源
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        [UnityEngine.Scripting.Preserve]
        public void UnloadAsset(string assetPath)
        {
            GameFrameworkGuard.NotNull(assetPath, nameof(assetPath));
            var package = YooAssets.GetPackage(DefaultPackageName);
            package.TryUnloadUnusedAsset(assetPath);
        }

        /// <summary>
        /// 卸载资源
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        /// <param name="assetPath">资源路径</param>
        [UnityEngine.Scripting.Preserve]
        public void UnloadAsset(string packageName, string assetPath)
        {
            GameFrameworkGuard.NotNull(packageName, nameof(packageName));
            GameFrameworkGuard.NotNull(assetPath, nameof(assetPath));
            var package = YooAssets.GetPackage(packageName);
            package.TryUnloadUnusedAsset(assetPath);
        }


        /// <summary>
        /// 强制回收所有资源
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        [UnityEngine.Scripting.Preserve]
        public void UnloadAllAssetsAsync(string packageName = null)
        {
            if (string.IsNullOrWhiteSpace(packageName))
            {
                packageName = ConstDefaultPackageName;
            }

            var package = YooAssets.GetPackage(packageName);
            if (package != null)
            {
                package.UnloadAllAssetsAsync();
            }
        }

        /// <summary>
        /// 卸载无用资源
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        [UnityEngine.Scripting.Preserve]
        public void UnloadUnusedAssetsAsync(string packageName = null)
        {
            if (string.IsNullOrWhiteSpace(packageName))
            {
                packageName = ConstDefaultPackageName;
            }

            var package = YooAssets.GetPackage(packageName);
            if (package != null)
            {
                package.UnloadUnusedAssetsAsync();
            }
        }

        /// <summary>
        /// 清理所有资源
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        [UnityEngine.Scripting.Preserve]
        public void ClearAllBundleFilesAsync(string packageName = null)
        {
            if (string.IsNullOrWhiteSpace(packageName))
            {
                packageName = ConstDefaultPackageName;
            }

            var package = YooAssets.GetPackage(packageName);
            if (package != null)
            {
                package.ClearUnusedBundleFilesAsync();
            }
        }

        /// <summary>
        /// 清理无用资源
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        [UnityEngine.Scripting.Preserve]
        public void ClearUnusedBundleFilesAsync(string packageName = null)
        {
            if (string.IsNullOrWhiteSpace(packageName))
            {
                packageName = ConstDefaultPackageName;
            }

            var package = YooAssets.GetPackage(packageName);

            if (package != null)
            {
                package.UnloadUnusedAssetsAsync();
            }
        }


        #region 异步加载子资源对象

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        public Task<SubAssetsHandle> LoadSubAssetsAsync(AssetInfo assetInfo)
        {
            var taskCompletionSource = new TaskCompletionSource<SubAssetsHandle>();
            var assetHandle = YooAssets.LoadSubAssetsAsync(assetInfo);
            assetHandle.Completed += handle => { taskCompletionSource.TrySetResult(handle); };
            return taskCompletionSource.Task;
        }

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type"></param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        public Task<SubAssetsHandle> LoadSubAssetsAsync(string path, Type type)
        {
            var taskCompletionSource = new TaskCompletionSource<SubAssetsHandle>();
            var assetHandle = YooAssets.LoadSubAssetsAsync(path, type);
            assetHandle.Completed += handle => { taskCompletionSource.TrySetResult(handle); };
            return taskCompletionSource.Task;
        }

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        public Task<SubAssetsHandle> LoadSubAssetsAsync<T>(string path) where T : Object
        {
            var taskCompletionSource = new TaskCompletionSource<SubAssetsHandle>();
            var assetHandle = YooAssets.LoadSubAssetsAsync<T>(path);
            assetHandle.Completed += handle => { taskCompletionSource.TrySetResult(handle); };
            return taskCompletionSource.Task;
        }

        #endregion

        #region 异步加载子资源对象

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        public SubAssetsHandle LoadSubAssetSync(AssetInfo assetInfo)
        {
            return YooAssets.LoadSubAssetsSync(assetInfo);
        }

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type"></param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        public SubAssetsHandle LoadSubAssetSync(string path, Type type)
        {
            return YooAssets.LoadSubAssetsSync(path, type);
        }

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        public SubAssetsHandle LoadSubAssetSync<T>(string path) where T : Object
        {
            return YooAssets.LoadSubAssetsSync<T>(path);
        }

        #endregion

        #region 异步加载原生文件

        /// <summary>
        /// 异步加载原生文件
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        public Task<RawFileHandle> LoadRawFileAsync(AssetInfo assetInfo)
        {
            var taskCompletionSource = new TaskCompletionSource<RawFileHandle>();
            var assetHandle = YooAssets.LoadRawFileAsync(assetInfo);
            assetHandle.Completed += handle => { taskCompletionSource.TrySetResult(handle); };
            return taskCompletionSource.Task;
        }

        /// <summary>
        /// 异步加载原生文件
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        public Task<RawFileHandle> LoadRawFileAsync(string path)
        {
            var taskCompletionSource = new TaskCompletionSource<RawFileHandle>();
            var assetHandle = YooAssets.LoadRawFileAsync(path);
            assetHandle.Completed += handle => { taskCompletionSource.TrySetResult(handle); };
            return taskCompletionSource.Task;
        }

        #endregion

        #region 同步加载原生文件

        /// <summary>
        /// 同步加载原生文件
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        public RawFileHandle LoadRawFileSync(AssetInfo assetInfo)
        {
            return YooAssets.LoadRawFileSync(assetInfo);
        }

        /// <summary>
        /// 同步加载原生文件
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        public RawFileHandle LoadRawFileSync(string path)
        {
            return YooAssets.LoadRawFileSync(path);
        }

        #endregion


        #region 异步加载资源

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        public Task<AssetHandle> LoadAssetAsync(AssetInfo assetInfo)
        {
            var taskCompletionSource = new TaskCompletionSource<AssetHandle>();
            var assetHandle = YooAssets.LoadAssetAsync(assetInfo);
            assetHandle.Completed += handle => { taskCompletionSource.TrySetResult(handle); };
            return taskCompletionSource.Task;
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type">资源类型</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        public Task<AssetHandle> LoadAssetAsync(string path, Type type)
        {
            var taskCompletionSource = new TaskCompletionSource<AssetHandle>();
            var assetHandle = YooAssets.LoadAssetAsync(path, type);
            assetHandle.Completed += handle => { taskCompletionSource.TrySetResult(handle); };
            return taskCompletionSource.Task;
        }

        /// <summary>
        /// 异步加载全部资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        public Task<AllAssetsHandle> LoadAllAssetsAsync<T>(string path) where T : Object
        {
            var taskCompletionSource = new TaskCompletionSource<AllAssetsHandle>();
            var assetHandle = YooAssets.LoadAllAssetsAsync<T>(path);
            assetHandle.Completed += handle => { taskCompletionSource.TrySetResult(handle); };
            return taskCompletionSource.Task;
        }

        /// <summary>
        /// 异步加载全部资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type">资源类型</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        public Task<AllAssetsHandle> LoadAllAssetsAsync(string path, Type type)
        {
            var taskCompletionSource = new TaskCompletionSource<AllAssetsHandle>();
            var assetHandle = YooAssets.LoadAllAssetsAsync(path, type);
            assetHandle.Completed += handle => { taskCompletionSource.TrySetResult(handle); };
            return taskCompletionSource.Task;
        }

        /// <summary>
        /// 异步加载资源包内所有资源对象
        /// </summary>
        /// <param name="path">资源的定位地址</param>
        [UnityEngine.Scripting.Preserve]
        public Task<AllAssetsHandle> LoadAllAssetsAsync(string path)
        {
            var taskCompletionSource = new TaskCompletionSource<AllAssetsHandle>();
            var assetHandle = YooAssets.LoadAllAssetsAsync(path);
            assetHandle.Completed += handle => { taskCompletionSource.TrySetResult(handle); };
            return taskCompletionSource.Task;
        }

        /// <summary>
        /// 异步加载资源包内所有资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        [UnityEngine.Scripting.Preserve]
        public Task<AllAssetsHandle> LoadAllAssetsAsync(AssetInfo assetInfo)
        {
            var taskCompletionSource = new TaskCompletionSource<AllAssetsHandle>();
            var assetHandle = YooAssets.LoadAllAssetsAsync(assetInfo);
            assetHandle.Completed += handle => { taskCompletionSource.TrySetResult(handle); };
            return taskCompletionSource.Task;
        }

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="path">资源的定位地址</param>
        [UnityEngine.Scripting.Preserve]
        public SubAssetsHandle LoadSubAssetsAsync(string path)
        {
            return YooAssets.LoadSubAssetsAsync(path);
        }


        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        public Task<AssetHandle> LoadAssetAsync(string path)
        {
            var taskCompletionSource = new TaskCompletionSource<AssetHandle>();
            var assetHandle = YooAssets.LoadAssetAsync(path);
            assetHandle.Completed += handle => { taskCompletionSource.TrySetResult(handle); };
            return taskCompletionSource.Task;
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <typeparam name="T">资源类型</typeparam>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        public Task<AssetHandle> LoadAssetAsync<T>(string path) where T : Object
        {
            var taskCompletionSource = new TaskCompletionSource<AssetHandle>();
            var assetHandle = YooAssets.LoadAssetAsync<T>(path);

            void OnAssetHandleOnCompleted(AssetHandle handle)
            {
                taskCompletionSource.TrySetResult(handle);
            }

            assetHandle.Completed += OnAssetHandleOnCompleted;
            return taskCompletionSource.Task;
        }

        /*
         [UnityEngine.Scripting.Preserve]
        public async Task<TObject> LoadAssetTaskAsync<TObject>(string assetPath) where TObject : Object
        {
            ResourcePackage assetPackage = YooAssets.TryGetPackage(DefaultPackageName);
            var handle = assetPackage.LoadAssetAsync<TObject>(assetPath);
            await handle.Task;
            if (handle == null || handle.AssetObject == null || handle.Status == EOperationStatus.Failed)
            {
                string errorMessage = Utility.Text.Format("Can not load asset '{0}'.", assetPath);
                throw new GameFrameworkException(errorMessage);
            }

            var result = handle.AssetObject as TObject;
            if (result == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("TObject '{0}' is invalid.", typeof(TObject).FullName));
            }

            return result;
        }*/

        #endregion

        #region 同步加载资源

        /// <summary>
        /// 同步加载资源包内所有资源对象
        /// </summary>
        /// <param name="path">资源的定位地址</param>
        [UnityEngine.Scripting.Preserve]
        public AllAssetsHandle LoadAllAssetsSync(string path)
        {
            return YooAssets.LoadAllAssetsSync(path);
        }

        /// <summary>
        /// 同步加载资源包内所有资源对象
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="path">资源的定位地址</param>
        [UnityEngine.Scripting.Preserve]
        public AllAssetsHandle LoadAllAssetsSync<T>(string path) where T : Object
        {
            return YooAssets.LoadAllAssetsSync<T>(path);
        }

        /// <summary>
        /// 同步加载资源包内所有资源对象
        /// </summary>
        /// <param name="path">资源的定位地址</param>
        /// <param name="type">子对象类型</param>
        [UnityEngine.Scripting.Preserve]
        public AllAssetsHandle LoadAllAssetsSync(string path, Type type)
        {
            return YooAssets.LoadAllAssetsSync(path, type);
        }

        /// <summary>
        /// 同步加载包内全部资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        public AllAssetsHandle LoadAllAssetsSync(AssetInfo assetInfo)
        {
            return YooAssets.LoadAllAssetsSync(assetInfo);
        }

        /// <summary>
        /// 同步加载子资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        public SubAssetsHandle LoadSubAssetSync(string path)
        {
            return YooAssets.LoadSubAssetsSync(path);
        }

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        public AssetHandle LoadAssetSync(string path)
        {
            return YooAssets.LoadAssetSync(path);
        }

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type"></param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        public AssetHandle LoadAssetSync(string path, Type type)
        {
            return YooAssets.LoadAssetSync(path, type);
        }

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        public AssetHandle LoadAssetSync(AssetInfo assetInfo)
        {
            return YooAssets.LoadAssetSync(assetInfo);
        }

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        public AssetHandle LoadAssetSync<T>(string path) where T : Object
        {
            return YooAssets.LoadAssetSync<T>(path);
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
        [UnityEngine.Scripting.Preserve]
        public Task<SceneHandle> LoadSceneAsync(string path, LoadSceneMode sceneMode, bool activateOnLoad = true)
        {
            var taskCompletionSource = new TaskCompletionSource<SceneHandle>();
            var sceneHandle = YooAssets.LoadSceneAsync(path, sceneMode, LocalPhysicsMode.None, !activateOnLoad);
            sceneHandle.Completed += handle => { taskCompletionSource.TrySetResult(handle); };
            return taskCompletionSource.Task;
        }

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="assetInfo">资源路径</param>
        /// <param name="sceneMode">场景模式</param>
        /// <param name="activateOnLoad">是否加载完成自动激活</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        public Task<SceneHandle> LoadSceneAsync(AssetInfo assetInfo, LoadSceneMode sceneMode, bool activateOnLoad = true)
        {
            var taskCompletionSource = new TaskCompletionSource<SceneHandle>();
            var sceneHandle = YooAssets.LoadSceneAsync(assetInfo, sceneMode, LocalPhysicsMode.None, !activateOnLoad);
            sceneHandle.Completed += handle => { taskCompletionSource.TrySetResult(handle); };
            return taskCompletionSource.Task;
        }

        #endregion

        #region 资源包

        /// <summary>
        /// 创建资源包
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        public ResourcePackage CreateAssetsPackage(string packageName)
        {
            return YooAssets.CreatePackage(packageName);
        }

        /// <summary>
        /// 尝试获取资源包
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        public ResourcePackage TryGetAssetsPackage(string packageName)
        {
            return YooAssets.TryGetPackage(packageName);
        }

        /// <summary>
        /// 检查资源包是否存在
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        public bool HasAssetsPackage(string packageName)
        {
            return YooAssets.TryGetPackage(packageName) != null;
        }

        /// <summary>
        /// 获取资源包
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        public ResourcePackage GetAssetsPackage(string packageName)
        {
            return YooAssets.GetPackage(packageName);
        }

        #endregion

        /// <summary>
        /// 是否需要下载
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        public bool IsNeedDownload(AssetInfo assetInfo)
        {
            return YooAssets.IsNeedDownloadFromRemote(assetInfo);
        }

        /// <summary>
        /// 是否需要下载
        /// </summary>
        /// <param name="path">资源地址</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        public bool IsNeedDownload(string path)
        {
            return YooAssets.IsNeedDownloadFromRemote(path);
        }

        /// <summary>
        /// 获取资源信息
        /// </summary>
        /// <param name="assetTags">资源标签列表</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        public AssetInfo[] GetAssetInfos(string[] assetTags)
        {
            return YooAssets.GetAssetInfos(assetTags);
        }

        /// <summary>
        /// 获取资源信息
        /// </summary>
        /// <param name="assetTag">资源标签</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        public AssetInfo[] GetAssetInfos(string assetTag)
        {
            return YooAssets.GetAssetInfos(assetTag);
        }

        /// <summary>
        /// 获取资源信息
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public AssetInfo GetAssetInfo(string path)
        {
            return YooAssets.GetAssetInfo(path);
        }

        /// <summary>
        /// 检查指定的资源路径是否有效。
        /// </summary>
        /// <param name="path">要检查的资源路径。</param>
        /// <returns>如果资源路径有效，则返回 true；否则返回 false。</returns>
        [UnityEngine.Scripting.Preserve]
        public bool HasAssetPath(string path)
        {
            return YooAssets.CheckLocationValid(path);
        }

        /// <summary>
        /// 设置默认资源包
        /// </summary>
        /// <param name="resourcePackage">资源信息</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        public void SetDefaultAssetsPackage(ResourcePackage resourcePackage)
        {
            YooAssets.SetDefaultPackage(resourcePackage);
        }


        /// <summary>
        /// 每帧更新资源系统驱动。
        /// </summary>
        /// <param name="elapseSeconds">逻辑帧间隔时间。</param>
        /// <param name="realElapseSeconds">真实帧间隔时间。</param>
        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
            YooAssets.Tick();
        }

        /// <summary>
        /// 关闭资源系统模块。
        /// </summary>
        public override void Shutdown()
        {
        }


        /// <summary>
        /// 获取或设置运行模式。
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public EPlayMode PlayMode { get; private set; }

        /// <summary>
        /// 设置运行模式
        /// </summary>
        /// <param name="playMode">运行模式</param>
        [UnityEngine.Scripting.Preserve]
        public void SetPlayMode(EPlayMode playMode)
        {
            PlayMode = playMode;
        }

        /// <summary>
        /// 获取资源只读区路径。
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public string ReadOnlyPath { get; private set; }

        /// <summary>
        /// 设置资源只读区路径。
        /// </summary>
        /// <param name="readOnlyPath">资源只读区路径。</param>
        [UnityEngine.Scripting.Preserve]
        public void SetReadOnlyPath(string readOnlyPath)
        {
            GameFrameworkGuard.NotNull(readOnlyPath, nameof(readOnlyPath));
            ReadOnlyPath = readOnlyPath;
        }

        /// <summary>
        /// 获取资源读写区路径。
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public string ReadWritePath { get; private set; }

        /// <summary>
        /// 设置资源读写区路径。
        /// </summary>
        /// <param name="readWritePath">资源读写区路径。</param>
        [UnityEngine.Scripting.Preserve]
        public void SetReadWritePath(string readWritePath)
        {
            GameFrameworkGuard.NotNull(readWritePath, nameof(readWritePath));
            ReadWritePath = readWritePath;
        }
    }
}
