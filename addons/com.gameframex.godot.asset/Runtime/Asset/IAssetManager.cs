using System;
using System.Threading.Tasks;
using GameFrameX.AssetSystem;
using Godot;

namespace GameFrameX.Asset.Runtime
{
    /// <summary>
    /// 资源组件。
    /// </summary>
    public interface IAssetManager
    {
        /// <summary>
        /// 同时下载的最大数目。
        /// </summary>
        int DownloadingMaxNum { get; set; }

        /// <summary>
        /// 失败重试最大数目。
        /// </summary>
        int FailedTryAgain { get; set; }

        /// <summary>
        /// 获取或设置资源包名称。
        /// </summary>
        string DefaultPackageName { get; set; }

        /// <summary>
        /// 获取或设置运行模式。
        /// </summary>
        EPlayMode PlayMode { get; }

        /// <summary>
        /// 获取或设置下载文件校验等级。
        /// </summary>
        EFileVerifyLevel VerifyLevel { get; }

        /// <summary>
        /// 获取或设置异步系统参数，每帧执行消耗的最大时间切片（单位：毫秒）。
        /// </summary>
        long Milliseconds { get; set; }

        /// <summary>
        /// 设置运行模式
        /// </summary>
        /// <param name="playMode">运行模式</param>
        void SetPlayMode(EPlayMode playMode);

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        void Initialize();

        /// <summary>
        /// 异步初始化操作
        /// </summary>
        /// <param name="packageName">包名称。</param>
        /// <param name="hostServerURL">热更链接URL。</param>
        /// <param name="fallbackHostServerURL">备用热更链接URL</param>
        /// <param name="isDefaultPackage">是否是默认包，默认是</param>
        /// <returns></returns>
        Task<bool> InitPackageAsync(string packageName, string hostServerURL, string fallbackHostServerURL, bool isDefaultPackage = true);

        /// <summary>
        /// 卸载资源
        /// </summary>
        /// <param name="assetPath"></param>
        void UnloadAsset(string assetPath);

        #region 异步加载子资源对象

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        Task<SubAssetsHandle> LoadSubAssetsAsync(AssetInfo assetInfo);

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type"></param>
        /// <returns></returns>
        Task<SubAssetsHandle> LoadSubAssetsAsync(string path, Type type);

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        Task<SubAssetsHandle> LoadSubAssetsAsync<T>(string path) where T : Resource;

        #endregion

        #region 同步加载子资源对象

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        SubAssetsHandle LoadSubAssetSync(AssetInfo assetInfo);

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type"></param>
        /// <returns></returns>
        SubAssetsHandle LoadSubAssetSync(string path, Type type);

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        SubAssetsHandle LoadSubAssetSync<T>(string path) where T : Resource;

        #endregion

        #region 异步加载原生文件

        /// <summary>
        /// 异步加载原生文件
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        Task<RawFileHandle> LoadRawFileAsync(AssetInfo assetInfo);

        /// <summary>
        /// 异步加载原生文件
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        Task<RawFileHandle> LoadRawFileAsync(string path);

        #endregion

        #region 同步加载原生文件

        /// <summary>
        /// 同步加载原生文件
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        RawFileHandle LoadRawFileSync(AssetInfo assetInfo);

        /// <summary>
        /// 同步加载原生文件
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        RawFileHandle LoadRawFileSync(string path);

        #endregion


        #region 异步加载资源

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        Task<AssetHandle> LoadAssetAsync(AssetInfo assetInfo);

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type">资源类型</param>
        /// <returns></returns>
        Task<AssetHandle> LoadAssetAsync(string path, Type type);

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <typeparam name="T">资源类型</typeparam>
        /// <returns></returns>
        Task<AssetHandle> LoadAssetAsync<T>(string path) where T : Resource;

        /// <summary>
        /// 异步加载全部资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        Task<AllAssetsHandle> LoadAllAssetsAsync<T>(string path) where T : Resource;

        /// <summary>
        /// 异步加载全部资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type">资源类型</param>
        /// <returns></returns>
        Task<AllAssetsHandle> LoadAllAssetsAsync(string path, Type type);

        /// <summary>
        /// 异步加载资源包内所有资源对象
        /// </summary>
        /// <param name="path">资源的定位地址</param>
        Task<AllAssetsHandle> LoadAllAssetsAsync(string path);

        /// <summary>
        /// 异步加载资源包内所有资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        Task<AllAssetsHandle> LoadAllAssetsAsync(AssetInfo assetInfo);

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        Task<AssetHandle> LoadAssetAsync(string path);

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="path">资源的定位地址</param>
        SubAssetsHandle LoadSubAssetsAsync(string path);

        #endregion

        #region 同步加载资源

        /// <summary>
        /// 同步加载资源包内所有资源对象
        /// </summary>
        /// <param name="path">资源的定位地址</param>
        AllAssetsHandle LoadAllAssetsSync(string path);

        /// <summary>
        /// 同步加载资源包内所有资源对象
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="path">资源的定位地址</param>
        AllAssetsHandle LoadAllAssetsSync<T>(string path) where T : Resource;

        /// <summary>
        /// 同步加载资源包内所有资源对象
        /// </summary>
        /// <param name="path">资源的定位地址</param>
        /// <param name="type">子对象类型</param>
        AllAssetsHandle LoadAllAssetsSync(string path, Type type);

        /// <summary>
        /// 同步加载包内全部资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        AllAssetsHandle LoadAllAssetsSync(AssetInfo assetInfo);

        /// <summary>
        /// 同步加载子资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        SubAssetsHandle LoadSubAssetSync(string path);

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        AssetHandle LoadAssetSync(string path);

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type"></param>
        /// <returns></returns>
        AssetHandle LoadAssetSync(string path, Type type);

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        AssetHandle LoadAssetSync(AssetInfo assetInfo);

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        AssetHandle LoadAssetSync<T>(string path) where T : Resource;

        #endregion

        #region 加载场景

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="sceneMode">场景模式</param>
        /// <param name="activateOnLoad">是否加载完成自动激活</param>
        /// <returns></returns>
        Task<SceneHandle> LoadSceneAsync(string path, SceneLoadMode sceneMode, bool activateOnLoad = true);

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="assetInfo">资源路径</param>
        /// <param name="sceneMode">场景模式</param>
        /// <param name="activateOnLoad">是否加载完成自动激活</param>
        /// <returns></returns>
        Task<SceneHandle> LoadSceneAsync(AssetInfo assetInfo, SceneLoadMode sceneMode, bool activateOnLoad = true);

        #endregion

        #region 资源包

        /// <summary>
        /// 创建资源包
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        /// <returns></returns>
        ResourcePackage CreateAssetsPackage(string packageName);

        /// <summary>
        /// 尝试获取资源包
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        /// <returns></returns>
        ResourcePackage TryGetAssetsPackage(string packageName);

        /// <summary>
        /// 检查资源包是否存在
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        /// <returns></returns>
        bool HasAssetsPackage(string packageName);

        /// <summary>
        /// 获取资源包
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        /// <returns></returns>
        ResourcePackage GetAssetsPackage(string packageName);

        #endregion

        /// <summary>
        /// 是否需要下载
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        bool IsNeedDownload(AssetInfo assetInfo);

        /// <summary>
        /// 是否需要下载
        /// </summary>
        /// <param name="path">资源地址</param>
        /// <returns></returns>
        bool IsNeedDownload(string path);

        /// <summary>
        /// 获取资源信息
        /// </summary>
        /// <param name="assetTags">资源标签列表</param>
        /// <returns></returns>
        AssetInfo[] GetAssetInfos(string[] assetTags);

        /// <summary>
        /// 获取资源信息
        /// </summary>
        /// <param name="assetTag">资源标签</param>
        /// <returns></returns>
        AssetInfo[] GetAssetInfos(string assetTag);

        /// <summary>
        /// 获取资源信息
        /// </summary>
        AssetInfo GetAssetInfo(string path);

        /// <summary>
        /// 检查指定的资源路径是否有效。
        /// </summary>
        /// <param name="assetPath">要检查的资源路径。</param>
        /// <returns>如果资源路径有效，则返回 true；否则返回 false。</returns>
        bool HasAssetPath(string assetPath);

        /// <summary>
        /// 设置默认资源包
        /// </summary>
        /// <param name="resourcePackage">资源信息</param>
        /// <returns></returns>
        void SetDefaultAssetsPackage(ResourcePackage resourcePackage);

        /// <summary>
        /// 清理无用资源
        /// </summary>
        /// <param name="packageName">资源包名称,当packageName为 null 时,清理[默认]资源包</param>
        void ClearUnusedBundleFilesAsync(string packageName = null);

        /// <summary>
        /// 清理所有资源
        /// </summary>
        /// <param name="packageName">资源包名称,当packageName为 null 时,清理[默认]资源包</param>
        void ClearAllBundleFilesAsync(string packageName = null);

        /// <summary>
        /// 卸载无用资源
        /// </summary>
        /// <param name="packageName">资源包名称,当packageName为 null 时,卸载[默认]资源包</param>
        void UnloadUnusedAssetsAsync(string packageName = null);

        /// <summary>
        /// 强制回收所有资源
        /// </summary>
        /// <param name="packageName">资源包名称,当packageName为 null 时,卸载[默认]资源包</param>
        void UnloadAllAssetsAsync(string packageName = null);

        /// <summary>
        /// 卸载资源
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        /// <param name="assetPath">资源路径</param>
        void UnloadAsset(string packageName, string assetPath);
    }
}
