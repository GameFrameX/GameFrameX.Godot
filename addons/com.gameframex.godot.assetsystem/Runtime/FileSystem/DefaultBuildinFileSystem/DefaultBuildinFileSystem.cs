using System;
using System.IO;
using System.Collections.Generic;
namespace GameFrameX.AssetSystem
{
    /// <summary>
    /// 内置文件系统
    /// </summary>
    [AssetSystemPreserve]
    internal class DefaultBuildinFileSystem : IFileSystem
    {
        [AssetSystemPreserve]
        private class UnpackRemoteServices : IRemoteServices
        {
            private readonly string _buildinPackageRoot;
            protected readonly Dictionary<string, string> _mapping = new(10000);

            [AssetSystemPreserve]
            public UnpackRemoteServices(string buildinPackRoot)
            {
                _buildinPackageRoot = buildinPackRoot;
            }

            [AssetSystemPreserve]
            string IRemoteServices.GetRemoteMainURL(string fileName, string packageVersion)
            {
                return GetFileLoadURL(fileName);
            }

            [AssetSystemPreserve]
            string IRemoteServices.GetRemoteFallbackURL(string fileName, string packageVersion)
            {
                return GetFileLoadURL(fileName);
            }

            [AssetSystemPreserve]
            private string GetFileLoadURL(string fileName)
            {
                if (_mapping.TryGetValue(fileName, out var url) == false)
                {
                    var filePath = PathUtility.Combine(_buildinPackageRoot, fileName);
                    url = DownloadSystemHelper.ConvertToWWWPath(filePath);
                    _mapping.Add(fileName, url);
                }

                return url;
            }
        }

        [AssetSystemPreserve]
        public class FileWrapper
        {
            public string FileName { private set; get; }

            [AssetSystemPreserve]
            public FileWrapper(string fileName)
            {
                FileName = fileName;
            }
        }

        protected readonly Dictionary<string, FileWrapper> _wrappers = new(10000);
        protected readonly Dictionary<string, Stream> _loadedStream = new(10000);
        protected readonly Dictionary<string, string> _buildinFilePaths = new(10000);
        protected IFileSystem _unpackFileSystem;
        protected string _packageRoot;

        /// <summary>
        /// 包裹名称
        /// </summary>
        public string PackageName { private set; get; }

        /// <summary>
        /// 文件根目录
        /// </summary>
        public string FileRoot
        {
            get { return _packageRoot; }
        }

        /// <summary>
        /// 文件数量
        /// </summary>
        public int FileCount
        {
            get { return _wrappers.Count; }
        }

        #region 自定义参数

        /// <summary>
        /// 自定义参数：初始化的时候缓存文件校验级别
        /// </summary>
        public EFileVerifyLevel FileVerifyLevel { private set; get; } = EFileVerifyLevel.Middle;

        /// <summary>
        /// 自定义参数：数据文件追加文件格式
        /// </summary>
        public bool AppendFileExtension { private set; get; } = false;

        /// <summary>
        /// 自定义参数：原生文件构建管线
        /// </summary>
        public bool RawFileBuildPipeline { private set; get; } = false;

        /// <summary>
        ///  自定义参数：解密方法类
        /// </summary>
        public IDecryptionServices DecryptionServices { private set; get; }

        #endregion


        [AssetSystemPreserve]
        public DefaultBuildinFileSystem()
        {
        }

        [AssetSystemPreserve]
        public virtual FSInitializeFileSystemOperation InitializeFileSystemAsync()
        {
            var operation = new DBFSInitializeOperation(this);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }

        [AssetSystemPreserve]
        public virtual FSRequestPackageVersionOperation LoadLocalPackageVersionAsync(bool appendTimeTicks, int timeout)
        {
            var operation = new DBFSRequestPackageVersionOperation(this);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }

        [AssetSystemPreserve]
        public virtual FSLoadPackageManifestOperation LoadLocalPackageManifestAsync(string packageVersion, int timeout)
        {
            var operation = new DBFSLoadPackageManifestOperation(this, packageVersion);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }

        [AssetSystemPreserve]
        public virtual FSLoadPackageManifestOperation RequestRemotePackageManifestAsync(string packageVersion, int timeout)
        {
            var operation = new DBFSLoadPackageManifestOperation(this, packageVersion);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }

        [AssetSystemPreserve]
        public virtual FSRequestPackageVersionOperation RequestRemotePackageVersionAsync(bool appendTimeTicks, int timeout)
        {
            var operation = new DBFSRequestPackageVersionOperation(this);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }

        [AssetSystemPreserve]
        public virtual FSClearAllBundleFilesOperation ClearAllBundleFilesAsync()
        {
            return _unpackFileSystem.ClearAllBundleFilesAsync();
        }

        [AssetSystemPreserve]
        public virtual FSClearUnusedBundleFilesOperation ClearUnusedBundleFilesAsync(PackageManifest manifest)
        {
            return _unpackFileSystem.ClearUnusedBundleFilesAsync(manifest);
        }

        [AssetSystemPreserve]
        public virtual FSDownloadFileOperation DownloadFileAsync(PackageBundle bundle, DownloadParam param)
        {
            param.ImportFilePath = GetBuildinFileLoadPath(bundle);
            return _unpackFileSystem.DownloadFileAsync(bundle, param);
        }

        [AssetSystemPreserve]
        public virtual FSLoadBundleOperation LoadBundleFile(PackageBundle bundle)
        {
            if (NeedUnpack(bundle))
            {
                return _unpackFileSystem.LoadBundleFile(bundle);
            }

            if (RawFileBuildPipeline)
            {
                var operation = new DBFSLoadRawBundleOperation(this, bundle);
                OperationSystem.StartOperation(PackageName, operation);
                return operation;
            }
            else
            {
                var operation = new DBFSLoadAssetBundleOperation(this, bundle);
                OperationSystem.StartOperation(PackageName, operation);
                return operation;
            }
        }

        [AssetSystemPreserve]
        public virtual void UnloadBundleFile(PackageBundle bundle, object result)
        {
            var assetBundle = result as BundleFile;
            if (assetBundle == null)
            {
                return;
            }

            if (_unpackFileSystem.Exists(bundle))
            {
                _unpackFileSystem.UnloadBundleFile(bundle, assetBundle);
            }
            else
            {
                if (assetBundle != null)
                {
                    assetBundle.Unload(true);
                }

                if (_loadedStream.TryGetValue(bundle.BundleGUID, out var managedStream))
                {
                    if (managedStream != null)
                    {
                        managedStream.Close();
                        managedStream.Dispose();
                    }

                    _loadedStream.Remove(bundle.BundleGUID);
                }
            }
        }

        [AssetSystemPreserve]
        public virtual void SetParameter(string name, object value)
        {
            if (name == FileSystemParametersDefine.FILE_VERIFY_LEVEL)
            {
                FileVerifyLevel = (EFileVerifyLevel)value;
            }
            else if (name == FileSystemParametersDefine.APPEND_FILE_EXTENSION)
            {
                AppendFileExtension = (bool)value;
            }
            else if (name == FileSystemParametersDefine.RAW_FILE_BUILD_PIPELINE)
            {
                RawFileBuildPipeline = (bool)value;
            }
            else if (name == FileSystemParametersDefine.DECRYPTION_SERVICES)
            {
                DecryptionServices = (IDecryptionServices)value;
            }
            else
            {
                AssetSystemLogger.Warning($"Invalid parameter : {name}");
            }
        }

        [AssetSystemPreserve]
        public virtual void OnCreate(string packageName, string rootDirectory)
        {
            PackageName = packageName;

            rootDirectory = ResolveRootDirectory(rootDirectory);

            _packageRoot = PathUtility.Combine(rootDirectory, packageName);

            // 创建解压文件系统
            var remoteServices = new UnpackRemoteServices(_packageRoot);
            _unpackFileSystem = new DefaultUnpackFileSystem();
            _unpackFileSystem.SetParameter(FileSystemParametersDefine.REMOTE_SERVICES, remoteServices);
            _unpackFileSystem.SetParameter(FileSystemParametersDefine.FILE_VERIFY_LEVEL, FileVerifyLevel);
            _unpackFileSystem.SetParameter(FileSystemParametersDefine.APPEND_FILE_EXTENSION, AppendFileExtension);
            _unpackFileSystem.SetParameter(FileSystemParametersDefine.RAW_FILE_BUILD_PIPELINE, RawFileBuildPipeline);
            _unpackFileSystem.SetParameter(FileSystemParametersDefine.DECRYPTION_SERVICES, DecryptionServices);
            _unpackFileSystem.OnCreate(packageName, null);
        }

        [AssetSystemPreserve]
        public virtual void OnUpdate()
        {
        }

        [AssetSystemPreserve]
        public virtual bool Belong(PackageBundle bundle)
        {
            return _wrappers.ContainsKey(bundle.BundleGUID);
        }

        [AssetSystemPreserve]
        public virtual bool Exists(PackageBundle bundle)
        {
            return _wrappers.ContainsKey(bundle.BundleGUID);
        }

        [AssetSystemPreserve]
        public virtual bool NeedDownload(PackageBundle bundle)
        {
            return false;
        }

        [AssetSystemPreserve]
        public virtual bool NeedUnpack(PackageBundle bundle)
        {
            if (Belong(bundle) == false)
            {
                return false;
            }

#if UNITY_ANDROID
            return RawFileBuildPipeline || bundle.Encrypted;
#else
            return false;
#endif
        }

        [AssetSystemPreserve]
        public virtual bool NeedImport(PackageBundle bundle)
        {
            return false;
        }

        [AssetSystemPreserve]
        public virtual byte[] ReadFileData(PackageBundle bundle)
        {
            if (NeedUnpack(bundle))
            {
                return _unpackFileSystem.ReadFileData(bundle);
            }

            if (Exists(bundle) == false)
            {
                return null;
            }

            if (bundle.Encrypted)
            {
                if (DecryptionServices == null)
                {
                    AssetSystemLogger.Error($"The {nameof(IDecryptionServices)} is null !");
                    return null;
                }

                var filePath = GetBuildinFileLoadPath(bundle);
                var fileInfo = new DecryptFileInfo
                {
                    BundleName = bundle.BundleName,
                    FileLoadCRC = bundle.UnityCRC,
                    FileLoadPath = filePath,
                };
                return DecryptionServices.ReadFileData(fileInfo);
            }
            else
            {
                var filePath = GetBuildinFileLoadPath(bundle);
                return FileUtility.ReadAllBytes(filePath);
            }
        }

        [AssetSystemPreserve]
        public virtual string ReadFileText(PackageBundle bundle)
        {
            if (NeedUnpack(bundle))
            {
                return _unpackFileSystem.ReadFileText(bundle);
            }

            if (Exists(bundle) == false)
            {
                return null;
            }

            if (bundle.Encrypted)
            {
                if (DecryptionServices == null)
                {
                    AssetSystemLogger.Error($"The {nameof(IDecryptionServices)} is null !");
                    return null;
                }

                var filePath = GetBuildinFileLoadPath(bundle);
                var fileInfo = new DecryptFileInfo
                {
                    BundleName = bundle.BundleName,
                    FileLoadCRC = bundle.UnityCRC,
                    FileLoadPath = filePath,
                };
                return DecryptionServices.ReadFileText(fileInfo);
            }
            else
            {
                var filePath = GetBuildinFileLoadPath(bundle);
                return FileUtility.ReadAllText(filePath);
            }
        }

        #region 内部方法

        [AssetSystemPreserve]
        protected string GetDefaultRoot()
        {
            return PathUtility.Combine(GodotAssetPath.GetStreamingAssetsRoot(), AssetSystemSettingsData.Setting.DefaultYooFolderName);
        }

        /// <summary>
        /// 解析文件系统根目录
        /// </summary>
        [AssetSystemPreserve]
        protected string ResolveRootDirectory(string rootDirectory)
        {
            var resolvedRoot = string.IsNullOrEmpty(rootDirectory) ? GetDefaultRoot() : rootDirectory;
            resolvedRoot = PathUtility.ConvertToAbsolutePath(resolvedRoot, GodotAssetPath.GetProjectRoot(), GodotAssetPath.GetPersistentRoot());
            return PathUtility.RegularPath(resolvedRoot);
        }

        [AssetSystemPreserve]
        public string GetBuildinFileLoadPath(PackageBundle bundle)
        {
            if (_buildinFilePaths.TryGetValue(bundle.BundleGUID, out var filePath) == false)
            {
                filePath = PathUtility.Combine(_packageRoot, bundle.FileName);
                _buildinFilePaths.Add(bundle.BundleGUID, filePath);
            }

            return filePath;
        }

        [AssetSystemPreserve]
        public string GetBuildinCatalogFileLoadPath()
        {
            var fileName = Path.GetFileNameWithoutExtension(DefaultBuildinFileSystemDefine.BuildinCatalogFileName);
            return PathUtility.Combine(AssetSystemSettingsData.Setting.DefaultYooFolderName, PackageName, fileName);
        }

        [AssetSystemPreserve]
        public string GetBuildinPackageVersionFilePath()
        {
            var fileName = AssetSystemSettingsData.GetPackageVersionFileName(PackageName);
            return PathUtility.Combine(FileRoot, fileName);
        }

        [AssetSystemPreserve]
        public string GetBuildinPackageHashFilePath(string packageVersion)
        {
            var fileName = AssetSystemSettingsData.GetPackageHashFileName(PackageName, packageVersion);
            return PathUtility.Combine(FileRoot, fileName);
        }

        [AssetSystemPreserve]
        public string GetBuildinPackageManifestFilePath(string packageVersion)
        {
            var fileName = AssetSystemSettingsData.GetManifestBinaryFileName(PackageName, packageVersion);
            return PathUtility.Combine(FileRoot, fileName);
        }

        [AssetSystemPreserve]
        public string GetStreamingAssetsPackageRoot()
        {
            var rootPath = PathUtility.Combine(GodotAssetPath.GetStreamingAssetsRoot(), AssetSystemSettingsData.Setting.DefaultYooFolderName);
            return PathUtility.Combine(rootPath, PackageName);
        }

        /// <summary>
        /// 记录文件信息
        /// </summary>
        [AssetSystemPreserve]
        public bool RecordFile(string bundleGUID, FileWrapper wrapper)
        {
            if (_wrappers.ContainsKey(bundleGUID))
            {
                AssetSystemLogger.Error($"{nameof(DefaultBuildinFileSystem)} has element : {bundleGUID}");
                return false;
            }

            _wrappers.Add(bundleGUID, wrapper);
            return true;
        }

        /// <summary>
        /// 初始化解压文件系统
        /// </summary>
        [AssetSystemPreserve]
        public FSInitializeFileSystemOperation InitializeUpackFileSystem()
        {
            return _unpackFileSystem.InitializeFileSystemAsync();
        }

        /// <summary>
        /// 加载加密资源文件
        /// </summary>
        [AssetSystemPreserve]
        public BundleFile LoadEncryptedAssetBundle(PackageBundle bundle)
        {
            var filePath = GetBuildinFileLoadPath(bundle);
            var fileInfo = new DecryptFileInfo
            {
                BundleName = bundle.BundleName,
                FileLoadCRC = bundle.UnityCRC,
                FileLoadPath = filePath,
            };

            var assetBundle = DecryptionServices.LoadAssetBundle(fileInfo, out var managedStream);
            _loadedStream.Add(bundle.BundleGUID, managedStream);
            return assetBundle;
        }

        /// <summary>
        /// 加载加密资源文件
        /// </summary>
        [AssetSystemPreserve]
        public BundleFileCreateRequest LoadEncryptedAssetBundleAsync(PackageBundle bundle)
        {
            var filePath = GetBuildinFileLoadPath(bundle);
            var fileInfo = new DecryptFileInfo
            {
                BundleName = bundle.BundleName,
                FileLoadCRC = bundle.UnityCRC,
                FileLoadPath = filePath,
            };

            var createRequest = DecryptionServices.LoadAssetBundleAsync(fileInfo, out var managedStream);
            _loadedStream.Add(bundle.BundleGUID, managedStream);
            return createRequest;
        }

        #endregion
    }
}
