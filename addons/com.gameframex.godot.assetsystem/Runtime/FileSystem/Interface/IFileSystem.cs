namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    internal interface IFileSystem
    {
        /// <summary>
        /// 包裹名称
        /// </summary>
        string PackageName { get; }

        /// <summary>
        /// 文件根目录
        /// </summary>
        string FileRoot { get; }

        /// <summary>
        /// 文件数量
        /// </summary>
        int FileCount { get; }


        /// <summary>
        /// 初始化缓存系统
        /// </summary>
        [AssetSystemPreserve]
        FSInitializeFileSystemOperation InitializeFileSystemAsync();

        /// <summary>
        /// 加载本地最新的版本
        /// </summary>
        [AssetSystemPreserve]
        FSRequestPackageVersionOperation LoadLocalPackageVersionAsync(bool appendTimeTicks, int timeout);

        /// <summary>
        /// 加载本地包裹清单
        /// </summary>
        [AssetSystemPreserve]
        FSLoadPackageManifestOperation LoadLocalPackageManifestAsync(string packageVersion, int timeout);

        /// <summary>
        /// 加载网络包裹清单
        /// </summary>
        [AssetSystemPreserve]
        FSLoadPackageManifestOperation RequestRemotePackageManifestAsync(string packageVersion, int timeout);

        /// <summary>
        /// 查询网络最新的版本
        /// </summary>
        [AssetSystemPreserve]
        FSRequestPackageVersionOperation RequestRemotePackageVersionAsync(bool appendTimeTicks, int timeout);

        /// <summary>
        /// 清空所有的文件
        /// </summary>
        [AssetSystemPreserve]
        FSClearAllBundleFilesOperation ClearAllBundleFilesAsync();

        /// <summary>
        /// 清空未使用的文件
        /// </summary>
        [AssetSystemPreserve]
        FSClearUnusedBundleFilesOperation ClearUnusedBundleFilesAsync(PackageManifest manifest);

        /// <summary>
        /// 下载远端文件
        /// </summary>
        [AssetSystemPreserve]
        FSDownloadFileOperation DownloadFileAsync(PackageBundle bundle, DownloadParam param);

        /// <summary>
        /// 加载Bundle文件
        /// </summary>
        [AssetSystemPreserve]
        FSLoadBundleOperation LoadBundleFile(PackageBundle bundle);

        /// <summary>
        /// 卸载Bundle文件
        /// </summary>
        [AssetSystemPreserve]
        void UnloadBundleFile(PackageBundle bundle, object result);


        /// <summary>
        /// 设置自定义参数
        /// </summary>
        [AssetSystemPreserve]
        void SetParameter(string name, object value);

        /// <summary>
        /// 创建缓存系统
        /// </summary>
        [AssetSystemPreserve]
        void OnCreate(string packageName, string rootDirectory);

        /// <summary>
        /// 更新文件系统
        /// </summary>
        [AssetSystemPreserve]
        void OnUpdate();


        /// <summary>
        /// 查询文件归属
        /// </summary>
        [AssetSystemPreserve]
        bool Belong(PackageBundle bundle);

        /// <summary>
        /// 查询文件是否存在
        /// </summary>
        [AssetSystemPreserve]
        bool Exists(PackageBundle bundle);

        /// <summary>
        /// 是否需要下载
        /// </summary>
        [AssetSystemPreserve]
        bool NeedDownload(PackageBundle bundle);

        /// <summary>
        /// 是否需要解压
        /// </summary>
        [AssetSystemPreserve]
        bool NeedUnpack(PackageBundle bundle);

        /// <summary>
        /// 是否需要导入
        /// </summary>
        [AssetSystemPreserve]
        bool NeedImport(PackageBundle bundle);


        /// <summary>
        /// 读取文件二进制数据
        /// </summary>
        [AssetSystemPreserve]
        byte[] ReadFileData(PackageBundle bundle);

        /// <summary>
        /// 读取文件文本数据
        /// </summary>
        [AssetSystemPreserve]
        string ReadFileText(PackageBundle bundle);
    }
}