using System;
using System.Threading.Tasks;
using GameFrameX.AssetSystem;
using GameFrameX.Runtime;
using Godot;

namespace GameFrameX.Asset.Runtime
{
    // 说明：静态类 AssetSystem 与其所在命名空间 GameFrameX.AssetSystem 同名，
    // 在 GameFrameX.Asset.Runtime 内直接写 AssetSystem.xxx 会解析到同名命名空间（CS0234），
    // 因此别名必须声明在 namespace 块内才能在名字查找中胜出。
    using AssetSystem = GameFrameX.AssetSystem.AssetSystem;

    /// <summary>
    /// 资源组件。
    /// </summary>
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
        public void Initialize()
        {
            Log.Info($"资源系统运行模式：{PlayMode}");
            AssetSystem.Initialize();
            AssetSystem.SetOperationSystemMaxTimeSlice(Milliseconds > 0 ? Milliseconds : 30);

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
        public async Task<bool> InitPackageAsync(string packageName, string hostServerURL, string fallbackHostServerURL, bool isDefaultPackage = true)
        {
            GameFrameworkGuard.NotNull(packageName, nameof(packageName));
            GameFrameworkGuard.NotNull(hostServerURL, nameof(hostServerURL));
            GameFrameworkGuard.NotNull(fallbackHostServerURL, nameof(fallbackHostServerURL));

            // 创建默认的资源包
            var resourcePackage = AssetSystem.TryGetPackage(packageName);
            if (resourcePackage == null)
            {
                resourcePackage = AssetSystem.CreatePackage(packageName);
                if (isDefaultPackage)
                {
                    // 设置该资源包为默认的资源包，可以使用AssetSystem相关加载接口加载该资源包内容。
                    AssetSystem.SetDefaultPackage(resourcePackage);
                }
            }

            var initializationOperationHandler = CreateInitializationOperationHandler(resourcePackage, hostServerURL, fallbackHostServerURL);
            await initializationOperationHandler.Task;
            if (initializationOperationHandler.Error == null && initializationOperationHandler.Status == EOperationStatus.Succeed && initializationOperationHandler.IsDone)
            {
                return true;
            }

            throw new Exception(initializationOperationHandler.Error);
        }

        /// <summary>
        /// 卸载资源
        /// </summary>
        /// <param name="assetPath">资源路径</param>
        public void UnloadAsset(string assetPath)
        {
            GameFrameworkGuard.NotNull(assetPath, nameof(assetPath));
            var package = AssetSystem.GetPackage(DefaultPackageName);
            package.TryUnloadUnusedAsset(assetPath);
        }

        /// <summary>
        /// 卸载资源
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        /// <param name="assetPath">资源路径</param>
        public void UnloadAsset(string packageName, string assetPath)
        {
            GameFrameworkGuard.NotNull(packageName, nameof(packageName));
            GameFrameworkGuard.NotNull(assetPath, nameof(assetPath));
            var package = AssetSystem.GetPackage(packageName);
            package.TryUnloadUnusedAsset(assetPath);
        }


        /// <summary>
        /// 强制回收所有资源
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        public void UnloadAllAssetsAsync(string packageName = null)
        {
            if (string.IsNullOrWhiteSpace(packageName))
            {
                packageName = ConstDefaultPackageName;
            }

            var package = AssetSystem.GetPackage(packageName);
            if (package != null)
            {
                package.UnloadAllAssetsAsync();
            }
        }

        /// <summary>
        /// 卸载无用资源
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        public void UnloadUnusedAssetsAsync(string packageName = null)
        {
            if (string.IsNullOrWhiteSpace(packageName))
            {
                packageName = ConstDefaultPackageName;
            }

            var package = AssetSystem.GetPackage(packageName);
            if (package != null)
            {
                package.UnloadUnusedAssetsAsync();
            }
        }

        /// <summary>
        /// 清理所有资源
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        public void ClearAllBundleFilesAsync(string packageName = null)
        {
            if (string.IsNullOrWhiteSpace(packageName))
            {
                packageName = ConstDefaultPackageName;
            }

            var package = AssetSystem.GetPackage(packageName);
            if (package != null)
            {
                package.UnloadAllAssetsAsync();
                package.ClearUnusedBundleFilesAsync();
            }
        }

        /// <summary>
        /// 清理无用资源包文件
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        public void ClearUnusedBundleFilesAsync(string packageName = null)
        {
            if (string.IsNullOrWhiteSpace(packageName))
            {
                packageName = ConstDefaultPackageName;
            }

            var package = AssetSystem.GetPackage(packageName);
            if (package != null)
            {
                package.ClearUnusedBundleFilesAsync();
            }
        }


        #region 异步加载子资源对象

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        public async Task<SubAssetsHandle> LoadSubAssetsAsync(AssetInfo assetInfo)
        {
            var assetHandle = AssetSystem.LoadSubAssetsAsync(assetInfo);
            await assetHandle.Task;
            return assetHandle;
        }

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<SubAssetsHandle> LoadSubAssetsAsync(string path, Type type)
        {
            var assetHandle = AssetSystem.LoadSubAssetsAsync(path, type);
            await assetHandle.Task;
            return assetHandle;
        }

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        public async Task<SubAssetsHandle> LoadSubAssetsAsync<T>(string path) where T : Resource
        {
            var assetHandle = AssetSystem.LoadSubAssetsAsync<T>(path);
            await assetHandle.Task;
            return assetHandle;
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
            return AssetSystem.LoadSubAssetsSync(assetInfo);
        }

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type"></param>
        /// <returns></returns>
        public SubAssetsHandle LoadSubAssetSync(string path, Type type)
        {
            return AssetSystem.LoadSubAssetsSync(path, type);
        }

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        public SubAssetsHandle LoadSubAssetSync<T>(string path) where T : Resource
        {
            return AssetSystem.LoadSubAssetsSync<T>(path);
        }

        #endregion

        #region 异步加载原生文件

        /// <summary>
        /// 异步加载原生文件
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        public async Task<RawFileHandle> LoadRawFileAsync(AssetInfo assetInfo)
        {
            var assetHandle = AssetSystem.LoadRawFileAsync(assetInfo);
            await assetHandle.Task;
            return assetHandle;
        }

        /// <summary>
        /// 异步加载原生文件
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        public async Task<RawFileHandle> LoadRawFileAsync(string path)
        {
            var assetHandle = AssetSystem.LoadRawFileAsync(path);
            await assetHandle.Task;
            return assetHandle;
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
            return AssetSystem.LoadRawFileSync(assetInfo);
        }

        /// <summary>
        /// 同步加载原生文件
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        public RawFileHandle LoadRawFileSync(string path)
        {
            return AssetSystem.LoadRawFileSync(path);
        }

        #endregion


        #region 异步加载资源

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        public async Task<AssetHandle> LoadAssetAsync(AssetInfo assetInfo)
        {
            var assetHandle = AssetSystem.LoadAssetAsync(assetInfo);
            await assetHandle.Task;
            return assetHandle;
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type">资源类型</param>
        /// <returns></returns>
        public async Task<AssetHandle> LoadAssetAsync(string path, Type type)
        {
            var assetHandle = AssetSystem.LoadAssetAsync(path, type);
            await assetHandle.Task;
            return assetHandle;
        }

        /// <summary>
        /// 异步加载全部资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        public async Task<AllAssetsHandle> LoadAllAssetsAsync<T>(string path) where T : Resource
        {
            var assetHandle = AssetSystem.LoadAllAssetsAsync<T>(path);
            await assetHandle.Task;
            return assetHandle;
        }

        /// <summary>
        /// 异步加载全部资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type">资源类型</param>
        /// <returns></returns>
        public async Task<AllAssetsHandle> LoadAllAssetsAsync(string path, Type type)
        {
            var assetHandle = AssetSystem.LoadAllAssetsAsync(path, type);
            await assetHandle.Task;
            return assetHandle;
        }

        /// <summary>
        /// 异步加载资源包内所有资源对象
        /// </summary>
        /// <param name="path">资源的定位地址</param>
        public async Task<AllAssetsHandle> LoadAllAssetsAsync(string path)
        {
            var assetHandle = AssetSystem.LoadAllAssetsAsync(path);
            await assetHandle.Task;
            return assetHandle;
        }

        /// <summary>
        /// 异步加载资源包内所有资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        public async Task<AllAssetsHandle> LoadAllAssetsAsync(AssetInfo assetInfo)
        {
            var assetHandle = AssetSystem.LoadAllAssetsAsync(assetInfo);
            await assetHandle.Task;
            return assetHandle;
        }

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="path">资源的定位地址</param>
        public SubAssetsHandle LoadSubAssetsAsync(string path)
        {
            return AssetSystem.LoadSubAssetsAsync(path);
        }


        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        public async Task<AssetHandle> LoadAssetAsync(string path)
        {
            var assetHandle = AssetSystem.LoadAssetAsync(path);
            await assetHandle.Task;
            return assetHandle;
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <typeparam name="T">资源类型</typeparam>
        /// <returns></returns>
        public async Task<AssetHandle> LoadAssetAsync<T>(string path) where T : Resource
        {
            var assetHandle = AssetSystem.LoadAssetAsync<T>(path);
            await assetHandle.Task;
            return assetHandle;
        }

        #endregion

        #region 同步加载资源

        /// <summary>
        /// 同步加载资源包内所有资源对象
        /// </summary>
        /// <param name="path">资源的定位地址</param>
        public AllAssetsHandle LoadAllAssetsSync(string path)
        {
            return AssetSystem.LoadAllAssetsSync(path);
        }

        /// <summary>
        /// 同步加载资源包内所有资源对象
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="path">资源的定位地址</param>
        public AllAssetsHandle LoadAllAssetsSync<T>(string path) where T : Resource
        {
            return AssetSystem.LoadAllAssetsSync<T>(path);
        }

        /// <summary>
        /// 同步加载资源包内所有资源对象
        /// </summary>
        /// <param name="path">资源的定位地址</param>
        /// <param name="type">子对象类型</param>
        public AllAssetsHandle LoadAllAssetsSync(string path, Type type)
        {
            return AssetSystem.LoadAllAssetsSync(path, type);
        }

        /// <summary>
        /// 同步加载包内全部资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        public AllAssetsHandle LoadAllAssetsSync(AssetInfo assetInfo)
        {
            return AssetSystem.LoadAllAssetsSync(assetInfo);
        }

        /// <summary>
        /// 同步加载子资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        public SubAssetsHandle LoadSubAssetSync(string path)
        {
            return AssetSystem.LoadSubAssetsSync(path);
        }

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        public AssetHandle LoadAssetSync(string path)
        {
            return AssetSystem.LoadAssetSync(path);
        }

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type"></param>
        /// <returns></returns>
        public AssetHandle LoadAssetSync(string path, Type type)
        {
            return AssetSystem.LoadAssetSync(path, type);
        }

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        public AssetHandle LoadAssetSync(AssetInfo assetInfo)
        {
            return AssetSystem.LoadAssetSync(assetInfo);
        }

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        public AssetHandle LoadAssetSync<T>(string path) where T : Resource
        {
            return AssetSystem.LoadAssetSync<T>(path);
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
        public async Task<SceneHandle> LoadSceneAsync(string path, SceneLoadMode sceneMode, bool activateOnLoad = true)
        {
            var sceneHandle = AssetSystem.LoadSceneAsync(path, sceneMode, ScenePhysicsMode.None, !activateOnLoad);
            await sceneHandle.Task;
            return sceneHandle;
        }

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="assetInfo">资源路径</param>
        /// <param name="sceneMode">场景模式</param>
        /// <param name="activateOnLoad">是否加载完成自动激活</param>
        /// <returns></returns>
        public async Task<SceneHandle> LoadSceneAsync(AssetInfo assetInfo, SceneLoadMode sceneMode, bool activateOnLoad = true)
        {
            var sceneHandle = AssetSystem.LoadSceneAsync(assetInfo, sceneMode, ScenePhysicsMode.None, !activateOnLoad);
            await sceneHandle.Task;
            return sceneHandle;
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
            return AssetSystem.CreatePackage(packageName);
        }

        /// <summary>
        /// 尝试获取资源包
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        /// <returns></returns>
        public ResourcePackage TryGetAssetsPackage(string packageName)
        {
            return AssetSystem.TryGetPackage(packageName);
        }

        /// <summary>
        /// 检查资源包是否存在
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        /// <returns></returns>
        public bool HasAssetsPackage(string packageName)
        {
            return AssetSystem.TryGetPackage(packageName) != null;
        }

        /// <summary>
        /// 获取资源包
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        /// <returns></returns>
        public ResourcePackage GetAssetsPackage(string packageName)
        {
            return AssetSystem.GetPackage(packageName);
        }

        #endregion

        /// <summary>
        /// 是否需要下载
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        public bool IsNeedDownload(AssetInfo assetInfo)
        {
            return AssetSystem.IsNeedDownloadFromRemote(assetInfo);
        }

        /// <summary>
        /// 是否需要下载
        /// </summary>
        /// <param name="path">资源地址</param>
        /// <returns></returns>
        public bool IsNeedDownload(string path)
        {
            return AssetSystem.IsNeedDownloadFromRemote(path);
        }

        /// <summary>
        /// 获取资源信息
        /// </summary>
        /// <param name="assetTags">资源标签列表</param>
        /// <returns></returns>
        public AssetInfo[] GetAssetInfos(string[] assetTags)
        {
            return AssetSystem.GetAssetInfos(assetTags);
        }

        /// <summary>
        /// 获取资源信息
        /// </summary>
        /// <param name="assetTag">资源标签</param>
        /// <returns></returns>
        public AssetInfo[] GetAssetInfos(string assetTag)
        {
            return AssetSystem.GetAssetInfos(assetTag);
        }

        /// <summary>
        /// 获取资源信息
        /// </summary>
        public AssetInfo GetAssetInfo(string path)
        {
            return AssetSystem.GetAssetInfo(path);
        }

        /// <summary>
        /// 检查指定的资源路径是否有效。
        /// </summary>
        /// <param name="path">要检查的资源路径。</param>
        /// <returns>如果资源路径有效，则返回 true；否则返回 false。</returns>
        public bool HasAssetPath(string path)
        {
            return AssetSystem.CheckLocationValid(path);
        }

        /// <summary>
        /// 设置默认资源包
        /// </summary>
        /// <param name="resourcePackage">资源信息</param>
        /// <returns></returns>
        public void SetDefaultAssetsPackage(ResourcePackage resourcePackage)
        {
            AssetSystem.SetDefaultPackage(resourcePackage);
        }


        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        public override void Shutdown()
        {
        }

        /// <summary>
        /// 获取或设置运行模式。
        /// </summary>
        public EPlayMode PlayMode { get; private set; }

        /// <summary>
        /// 设置运行模式
        /// </summary>
        /// <param name="playMode">运行模式</param>
        public void SetPlayMode(EPlayMode playMode)
        {
            PlayMode = playMode;
        }
    }
}
