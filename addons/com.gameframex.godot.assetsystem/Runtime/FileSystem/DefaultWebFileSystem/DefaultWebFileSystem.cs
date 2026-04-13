using System;
using System.IO;
using System.Collections.Generic;

namespace GameFrameX.AssetSystem
{
    /// <summary>
    /// Web文件系统
    /// </summary>
    [AssetSystemPreserve]
    internal class DefaultWebFileSystem : IFileSystem
    {
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
        protected readonly Dictionary<string, string> _webFilePaths = new(10000);
        protected string _webPackageRoot = string.Empty;

        /// <summary>
        /// 包裹名称
        /// </summary>
        public string PackageName { private set; get; }

        /// <summary>
        /// 文件根目录
        /// </summary>
        public string FileRoot
        {
            get { return _webPackageRoot; }
        }

        /// <summary>
        /// 文件数量
        /// </summary>
        public int FileCount
        {
            get { return 0; }
        }

        #region 自定义参数

        /// <summary>
        /// 禁用Unity的网络缓存
        /// </summary>
        public bool DisableUnityWebCache { private set; get; } = false;

        #endregion


        [AssetSystemPreserve]
        public DefaultWebFileSystem()
        {
        }

        [AssetSystemPreserve]
        public virtual FSInitializeFileSystemOperation InitializeFileSystemAsync()
        {
            var operation = new DWFSInitializeOperation(this);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }

        [AssetSystemPreserve]
        public virtual FSRequestPackageVersionOperation LoadLocalPackageVersionAsync(bool appendTimeTicks, int timeout)
        {
            var operation = new DWFSRequestPackageVersionOperation(this, appendTimeTicks, timeout);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }

        [AssetSystemPreserve]
        public virtual FSLoadPackageManifestOperation LoadLocalPackageManifestAsync(string packageVersion, int timeout)
        {
            var operation = new DWFSLoadPackageManifestOperation(this, packageVersion, timeout);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }

        [AssetSystemPreserve]
        public virtual FSLoadPackageManifestOperation RequestRemotePackageManifestAsync(string packageVersion, int timeout)
        {
            var operation = new DWFSLoadPackageManifestOperation(this, packageVersion, timeout);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }

        [AssetSystemPreserve]
        public virtual FSRequestPackageVersionOperation RequestRemotePackageVersionAsync(bool appendTimeTicks, int timeout)
        {
            var operation = new DWFSRequestPackageVersionOperation(this, appendTimeTicks, timeout);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }

        [AssetSystemPreserve]
        public virtual FSClearAllBundleFilesOperation ClearAllBundleFilesAsync()
        {
            var operation = new FSClearAllBundleFilesCompleteOperation();
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }

        [AssetSystemPreserve]
        public virtual FSClearUnusedBundleFilesOperation ClearUnusedBundleFilesAsync(PackageManifest manifest)
        {
            var operation = new FSClearUnusedBundleFilesCompleteOperation();
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }

        [AssetSystemPreserve]
        public virtual FSDownloadFileOperation DownloadFileAsync(PackageBundle bundle, DownloadParam param)
        {
            throw new NotImplementedException();
        }

        [AssetSystemPreserve]
        public virtual FSLoadBundleOperation LoadBundleFile(PackageBundle bundle)
        {
            var operation = new DWFSLoadAssetBundleOperation(this, bundle);
            OperationSystem.StartOperation(PackageName, operation);
            return operation;
        }

        [AssetSystemPreserve]
        public virtual void UnloadBundleFile(PackageBundle bundle, object result)
        {
            var assetBundle = result as BundleFile;
            if (assetBundle == null)
            {
                return;
            }

            if (assetBundle != null)
            {
                assetBundle.Unload(true);
            }
        }

        [AssetSystemPreserve]
        public virtual void SetParameter(string name, object value)
        {
            if (name == FileSystemParametersDefine.DISABLE_UNITY_WEB_CACHE)
            {
                DisableUnityWebCache = (bool)value;
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

            if (string.IsNullOrEmpty(rootDirectory))
            {
                rootDirectory = GetDefaultWebRoot();
            }

            _webPackageRoot = PathUtility.Combine(rootDirectory, packageName);
        }

        [AssetSystemPreserve]
        public virtual void OnUpdate()
        {
        }

        [AssetSystemPreserve]
        public virtual bool Belong(PackageBundle bundle)
        {
            return true;
        }

        [AssetSystemPreserve]
        public virtual bool Exists(PackageBundle bundle)
        {
            return true;
        }

        [AssetSystemPreserve]
        public virtual bool NeedDownload(PackageBundle bundle)
        {
            return false;
        }

        [AssetSystemPreserve]
        public virtual bool NeedUnpack(PackageBundle bundle)
        {
            return false;
        }

        [AssetSystemPreserve]
        public virtual bool NeedImport(PackageBundle bundle)
        {
            return false;
        }

        [AssetSystemPreserve]
        public virtual byte[] ReadFileData(PackageBundle bundle)
        {
            throw new NotImplementedException();
        }

        [AssetSystemPreserve]
        public virtual string ReadFileText(PackageBundle bundle)
        {
            throw new NotImplementedException();
        }

        #region 内部方法

        [AssetSystemPreserve]
        protected string GetDefaultWebRoot()
        {
            var path = PathUtility.Combine(GodotAssetPath.GetStreamingAssetsRoot(), AssetSystemSettingsData.Setting.DefaultAssetSystemFolderName);
            return path;
        }

        [AssetSystemPreserve]
        public string GetWebFileLoadPath(PackageBundle bundle)
        {
            if (_webFilePaths.TryGetValue(bundle.BundleGUID, out var filePath) == false)
            {
                filePath = PathUtility.Combine(_webPackageRoot, bundle.FileName);
                _webFilePaths.Add(bundle.BundleGUID, filePath);
            }

            return filePath;
        }

        [AssetSystemPreserve]
        public string GetCatalogFileLoadPath()
        {
            var fileName = Path.GetFileNameWithoutExtension(DefaultBuildinFileSystemDefine.BuildinCatalogFileName);
            return PathUtility.Combine(AssetSystemSettingsData.Setting.DefaultAssetSystemFolderName, PackageName, fileName);
        }

        [AssetSystemPreserve]
        public string GetWebPackageVersionFilePath()
        {
            var fileName = AssetSystemSettingsData.GetPackageVersionFileName(PackageName);
            AssetSystemLogger.Error(FileRoot + "    " + fileName);
            return PathUtility.Combine(FileRoot, fileName);
        }

        [AssetSystemPreserve]
        public string GetWebPackageHashFilePath(string packageVersion)
        {
            AssetSystemLogger.Error("packageVersion   " + packageVersion);
            var fileName = AssetSystemSettingsData.GetPackageHashFileName(PackageName, packageVersion);
            return PathUtility.Combine(FileRoot, fileName);
        }

        [AssetSystemPreserve]
        public string GetWebPackageManifestFilePath(string packageVersion)
        {
            var fileName = AssetSystemSettingsData.GetManifestBinaryFileName(PackageName, packageVersion);
            return PathUtility.Combine(FileRoot, fileName);
        }

        [AssetSystemPreserve]
        public string GetStreamingAssetsPackageRoot()
        {
            var rootPath = PathUtility.Combine(GodotAssetPath.GetStreamingAssetsRoot(), AssetSystemSettingsData.Setting.DefaultAssetSystemFolderName);
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
                AssetSystemLogger.Error($"{nameof(DefaultWebFileSystem)} has element : {bundleGUID}");
                return false;
            }

            _wrappers.Add(bundleGUID, wrapper);
            return true;
        }

        #endregion
    }
}

