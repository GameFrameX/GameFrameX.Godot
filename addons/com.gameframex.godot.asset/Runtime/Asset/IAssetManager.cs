using System;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using YooAsset;
using Object = UnityEngine.Object;

namespace GameFrameX.Asset.Runtime
{
    /// <summary>
    /// 资源组件。
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public interface IAssetManager
    {
        /// <summary>
        /// 同时下载的最大数目。
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        int DownloadingMaxNum { get; set; }

        /// <summary>
        /// 失败重试最大数目。
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        int FailedTryAgain { get; set; }

        /// <summary>
        /// 获取资源只读区路径。
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        string ReadOnlyPath { get; }

        /// <summary>
        /// 获取资源读写区路径。
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        string ReadWritePath { get; }

        /// <summary>
        /// 设置资源只读区路径。
        /// </summary>
        /// <param name="readOnlyPath">资源只读区路径。</param>
        [UnityEngine.Scripting.Preserve]
        void SetReadOnlyPath(string readOnlyPath);

        /// <summary>
        /// 设置资源读写区路径。
        /// </summary>
        /// <param name="readWritePath">资源读写区路径。</param>
        [UnityEngine.Scripting.Preserve]
        void SetReadWritePath(string readWritePath);

        /// <summary>
        /// 获取或设置资源包名称。
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        string DefaultPackageName { get; set; }

        /// <summary>
        /// 获取或设置运行模式。
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        EPlayMode PlayMode { get; }

        /// <summary>
        /// 获取或设置下载文件校验等级。
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        EFileVerifyLevel VerifyLevel { get; }

        /// <summary>
        /// 获取或设置异步系统参数，每帧执行消耗的最大时间切片（单位：毫秒）。
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        long Milliseconds { get; set; }

        /// <summary>
        /// 设置运行模式
        /// </summary>
        /// <param name="playMode">运行模式</param>
        [UnityEngine.Scripting.Preserve]
        void SetPlayMode(EPlayMode playMode);

        /// <summary>
        /// 初始化
        /// </summary>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        void Initialize();

        /// <summary>
        /// 异步初始化操作
        /// </summary>
        /// <param name="packageName">包名称。</param>
        /// <param name="hostServerURL">热更链接URL。</param>
        /// <param name="fallbackHostServerURL">备用热更链接URL</param>
        /// <param name="isDefaultPackage">是否是默认包，默认是</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        Task<bool> InitPackageAsync(string packageName, string hostServerURL, string fallbackHostServerURL, bool isDefaultPackage = true);

        /// <summary>
        /// 卸载资源
        /// </summary>
        /// <param name="assetPath"></param>
        [UnityEngine.Scripting.Preserve]
        void UnloadAsset(string assetPath);

        #region 异步加载子资源对象

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        Task<SubAssetsHandle> LoadSubAssetsAsync(AssetInfo assetInfo);

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type"></param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        Task<SubAssetsHandle> LoadSubAssetsAsync(string path, Type type);

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        Task<SubAssetsHandle> LoadSubAssetsAsync<T>(string path) where T : Object;

        #endregion

        #region 同步加载子资源对象

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        SubAssetsHandle LoadSubAssetSync(AssetInfo assetInfo);

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type"></param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        SubAssetsHandle LoadSubAssetSync(string path, Type type);

        /// <summary>
        /// 同步加载子资源对象
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        SubAssetsHandle LoadSubAssetSync<T>(string path) where T : Object;

        #endregion

        #region 异步加载原生文件

        /// <summary>
        /// 异步加载原生文件
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        Task<RawFileHandle> LoadRawFileAsync(AssetInfo assetInfo);

        /// <summary>
        /// 异步加载原生文件
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        Task<RawFileHandle> LoadRawFileAsync(string path);

        #endregion

        #region 同步加载原生文件

        /// <summary>
        /// 同步加载原生文件
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        RawFileHandle LoadRawFileSync(AssetInfo assetInfo);

        /// <summary>
        /// 同步加载原生文件
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        RawFileHandle LoadRawFileSync(string path);

        #endregion


        #region 异步加载资源

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        Task<AssetHandle> LoadAssetAsync(AssetInfo assetInfo);

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type">资源类型</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        Task<AssetHandle> LoadAssetAsync(string path, Type type);

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <typeparam name="T">资源类型</typeparam>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        Task<AssetHandle> LoadAssetAsync<T>(string path) where T : Object;

        /// <summary>
        /// 异步加载全部资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        Task<AllAssetsHandle> LoadAllAssetsAsync<T>(string path) where T : Object;

        /// <summary>
        /// 异步加载全部资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type">资源类型</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        Task<AllAssetsHandle> LoadAllAssetsAsync(string path, Type type);

        /*/// <summary>
        /// 异步加载资源。
        /// </summary>
        /// <param name="assetPath"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        System.Threading.Tasks.Task<T> LoadAssetTaskAsync<T>(string assetPath) where T : UnityEngine.Object;*/


        /// <summary>
        /// 异步加载资源包内所有资源对象
        /// </summary>
        /// <param name="path">资源的定位地址</param>
        [UnityEngine.Scripting.Preserve]
        Task<AllAssetsHandle> LoadAllAssetsAsync(string path);

        /// <summary>
        /// 异步加载资源包内所有资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        [UnityEngine.Scripting.Preserve]
        Task<AllAssetsHandle> LoadAllAssetsAsync(AssetInfo assetInfo);

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        Task<AssetHandle> LoadAssetAsync(string path);

        /// <summary>
        /// 异步加载子资源对象
        /// </summary>
        /// <param name="path">资源的定位地址</param>
        [UnityEngine.Scripting.Preserve]
        SubAssetsHandle LoadSubAssetsAsync(string path);

        #endregion

        #region 同步加载资源

        /// <summary>
        /// 同步加载资源包内所有资源对象
        /// </summary>
        /// <param name="path">资源的定位地址</param>
        [UnityEngine.Scripting.Preserve]
        AllAssetsHandle LoadAllAssetsSync(string path);

        /// <summary>
        /// 同步加载资源包内所有资源对象
        /// </summary>
        /// <typeparam name="T">资源类型</typeparam>
        /// <param name="path">资源的定位地址</param>
        [UnityEngine.Scripting.Preserve]
        AllAssetsHandle LoadAllAssetsSync<T>(string path) where T : Object;

        /// <summary>
        /// 同步加载资源包内所有资源对象
        /// </summary>
        /// <param name="path">资源的定位地址</param>
        /// <param name="type">子对象类型</param>
        [UnityEngine.Scripting.Preserve]
        AllAssetsHandle LoadAllAssetsSync(string path, Type type);

        /// <summary>
        /// 同步加载包内全部资源对象
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        AllAssetsHandle LoadAllAssetsSync(AssetInfo assetInfo);

        /// <summary>
        /// 同步加载子资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        SubAssetsHandle LoadSubAssetSync(string path);

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        AssetHandle LoadAssetSync(string path);

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <param name="type"></param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        AssetHandle LoadAssetSync(string path, Type type);

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        AssetHandle LoadAssetSync(AssetInfo assetInfo);

        /// <summary>
        /// 同步加载资源
        /// </summary>
        /// <param name="path">资源路径</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        AssetHandle LoadAssetSync<T>(string path) where T : Object;

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
        Task<SceneHandle> LoadSceneAsync(string path, LoadSceneMode sceneMode, bool activateOnLoad = true);

        /// <summary>
        /// 异步加载场景
        /// </summary>
        /// <param name="assetInfo">资源路径</param>
        /// <param name="sceneMode">场景模式</param>
        /// <param name="activateOnLoad">是否加载完成自动激活</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        Task<SceneHandle> LoadSceneAsync(AssetInfo assetInfo, LoadSceneMode sceneMode, bool activateOnLoad = true);

        #endregion

        #region 资源包

        /// <summary>
        /// 创建资源包
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        ResourcePackage CreateAssetsPackage(string packageName);

        /// <summary>
        /// 尝试获取资源包
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        ResourcePackage TryGetAssetsPackage(string packageName);

        /// <summary>
        /// 检查资源包是否存在
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        bool HasAssetsPackage(string packageName);

        /// <summary>
        /// 获取资源包
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        ResourcePackage GetAssetsPackage(string packageName);

        #endregion

        /// <summary>
        /// 是否需要下载
        /// </summary>
        /// <param name="assetInfo">资源信息</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        bool IsNeedDownload(AssetInfo assetInfo);

        /// <summary>
        /// 是否需要下载
        /// </summary>
        /// <param name="path">资源地址</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        bool IsNeedDownload(string path);

        /// <summary>
        /// 获取资源信息
        /// </summary>
        /// <param name="assetTags">资源标签列表</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        AssetInfo[] GetAssetInfos(string[] assetTags);

        /// <summary>
        /// 获取资源信息
        /// </summary>
        /// <param name="assetTag">资源标签</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        AssetInfo[] GetAssetInfos(string assetTag);

        /// <summary>
        /// 获取资源信息
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        AssetInfo GetAssetInfo(string path);

        /// <summary>
        /// 检查指定的资源路径是否有效。
        /// </summary>
        /// <param name="assetPath">要检查的资源路径。</param>
        /// <returns>如果资源路径有效，则返回 true；否则返回 false。</returns>
        [UnityEngine.Scripting.Preserve]
        bool HasAssetPath(string assetPath);

        /// <summary>
        /// 设置默认资源包
        /// </summary>
        /// <param name="resourcePackage">资源信息</param>
        /// <returns></returns>
        [UnityEngine.Scripting.Preserve]
        void SetDefaultAssetsPackage(ResourcePackage resourcePackage);

        /// <summary>
        /// 清理无用资源
        /// </summary>
        /// <param name="packageName">资源包名称,当packageName为 null 时,清理[默认]资源包</param>
        [UnityEngine.Scripting.Preserve]
        void ClearUnusedBundleFilesAsync(string packageName = null);

        /// <summary>
        /// 清理所有资源
        /// </summary>
        /// <param name="packageName">资源包名称,当packageName为 null 时,清理[默认]资源包</param>
        [UnityEngine.Scripting.Preserve]
        void ClearAllBundleFilesAsync(string packageName = null);

        /// <summary>
        /// 卸载无用资源
        /// </summary>
        /// <param name="packageName">资源包名称,当packageName为 null 时,卸载[默认]资源包</param>
        [UnityEngine.Scripting.Preserve]
        void UnloadUnusedAssetsAsync(string packageName = null);

        /// <summary>
        /// 强制回收所有资源
        /// </summary>
        /// <param name="packageName">资源包名称,当packageName为 null 时,卸载[默认]资源包</param>
        [UnityEngine.Scripting.Preserve]
        void UnloadAllAssetsAsync(string packageName = null);

        /// <summary>
        /// 卸载资源
        /// </summary>
        /// <param name="packageName">资源包名称</param>
        /// <param name="assetPath">资源路径</param>
        [UnityEngine.Scripting.Preserve]
        void UnloadAsset(string packageName, string assetPath);
    }
}