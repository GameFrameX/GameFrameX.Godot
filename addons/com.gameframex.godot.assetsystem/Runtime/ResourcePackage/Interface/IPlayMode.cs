namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    internal interface IPlayMode
    {
        /// <summary>
        /// 当前激活的清单
        /// </summary>
        PackageManifest ActiveManifest { set; get; }

        /// <summary>
        /// 更新游戏模式
        /// </summary>
        [AssetSystemPreserve]
        void UpdatePlayMode();

        /// <summary>
        /// 获取本地最新的资源版本
        /// </summary>
        [AssetSystemPreserve]
        LoadLocalVersionOperation LoadLocalVersionAsync(bool appendTimeTicks, int timeout);

        /// <summary>
        /// 获取本地最新的资源清单
        /// </summary>
        [AssetSystemPreserve]
        LoadLocalManifestOperation LoadLocalManifestAsync(string packageVersion, int timeout);

        /// <summary>
        /// 向网络端请求最新的资源版本
        /// </summary>
        [AssetSystemPreserve]
        RequestPackageVersionOperation RequestPackageVersionAsync(bool appendTimeTicks, int timeout);

        /// <summary>
        /// 向网络端请求并更新清单
        /// </summary>
        [AssetSystemPreserve]
        UpdatePackageManifestOperation UpdatePackageManifestAsync(string packageVersion, int timeout);

        /// <summary>
        /// 预下载指定版本的包裹内容
        /// </summary>
        [AssetSystemPreserve]
        PreDownloadContentOperation PreDownloadContentAsync(string packageVersion, int timeout);

        /// <summary>
        /// 清空所有文件
        /// </summary>
        [AssetSystemPreserve]
        ClearAllBundleFilesOperation ClearAllBundleFilesAsync();

        /// <summary>
        /// 清空未使用的文件
        /// </summary>
        [AssetSystemPreserve]
        ClearUnusedBundleFilesOperation ClearUnusedBundleFilesAsync();

        // 下载相关
        [AssetSystemPreserve]
        ResourceDownloaderOperation CreateResourceDownloaderByAll(int downloadingMaxNumber, int failedTryAgain, int timeout);
        [AssetSystemPreserve]
        ResourceDownloaderOperation CreateResourceDownloaderByTags(string[] tags, int downloadingMaxNumber, int failedTryAgain, int timeout);
        [AssetSystemPreserve]
        ResourceDownloaderOperation CreateResourceDownloaderByPaths(AssetInfo[] assetInfos, int downloadingMaxNumber, int failedTryAgain, int timeout);

        // 解压相关
        [AssetSystemPreserve]
        ResourceUnpackerOperation CreateResourceUnpackerByAll(int upackingMaxNumber, int failedTryAgain, int timeout);
        [AssetSystemPreserve]
        ResourceUnpackerOperation CreateResourceUnpackerByTags(string[] tags, int upackingMaxNumber, int failedTryAgain, int timeout);

        // 导入相关
        [AssetSystemPreserve]
        ResourceImporterOperation CreateResourceImporterByFilePaths(string[] filePaths, int importerMaxNumber, int failedTryAgain, int timeout);
    }
}