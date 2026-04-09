using System.Collections.Generic;
using System.Reflection;
using Godot;
using GameFrameX.AssetSystem;

[AssetSystemPreserve]
public static class ByteGameFileSystemCreater
{
    [AssetSystemPreserve]
    public static FileSystemParameters CreateByteGameFileSystemParameters(IRemoteServices remoteServices = null)
    {
        if (OS.HasFeature("web") == false)
        {
            throw new System.NotSupportedException("ByteGame file system only supports web platform runtime.");
        }

        var fileSystemClass = typeof(ByteGameFileSystem).FullName;
        var fileSystemParams = new FileSystemParameters(fileSystemClass, null);
        fileSystemParams.AddParameter(FileSystemParametersDefine.REMOTE_SERVICES, remoteServices);
        return fileSystemParams;
    }

    [AssetSystemPreserve]
    public static FileSystemParameters CreateByteGameFileSystemParameters(string buildinPackRoot)
    {
        if (OS.HasFeature("web") == false)
        {
            throw new System.NotSupportedException("ByteGame file system only supports web platform runtime.");
        }

        var fileSystemClass = typeof(ByteGameFileSystem).FullName;
        IRemoteServices remoteServices = new ByteGameFileSystem.WebRemoteServices(buildinPackRoot);
        var fileSystemParams = new FileSystemParameters(fileSystemClass, null);
        fileSystemParams.AddParameter(FileSystemParametersDefine.REMOTE_SERVICES, remoteServices);
        return fileSystemParams;
    }
}

/// <summary>
/// 抖音小游戏文件系统
/// 参考：https://developer.open-douyin.com/docs/resource/zh-CN/mini-game/develop/guide/know
/// </summary>
[AssetSystemPreserve]
internal class ByteGameFileSystem : IFileSystem
{
    [AssetSystemPreserve]
    public sealed class WebRemoteServices : IRemoteServices
    {
        private readonly string _webPackageRoot;
        private readonly Dictionary<string, string> _mapping = new(10000);

        [AssetSystemPreserve]
        public WebRemoteServices(string buildinPackRoot)
        {
            _webPackageRoot = buildinPackRoot;
        }

        [AssetSystemPreserve]
        private string GetFileLoadURL(string fileName)
        {
            if (_mapping.TryGetValue(fileName, out var url) == false)
            {
                var filePath = PathUtility.Combine(_webPackageRoot, fileName);
                url = DownloadSystemHelper.ConvertToWWWPath(filePath);
                _mapping.Add(fileName, url);
            }

            return url;
        }

        [AssetSystemPreserve]
        public string GetRemoteMainURL(string fileName, string packageVersion)
        {
            return GetFileLoadURL(fileName);
        }

        [AssetSystemPreserve]
        public string GetRemoteFallbackURL(string fileName, string packageVersion)
        {
            return GetFileLoadURL(fileName);
        }
    }

    private readonly Dictionary<string, string> _cacheFilePaths = new(10000);
    private object _fileSystemManager;

    /// <summary>
    /// 包裹名称
    /// </summary>
    public string PackageName { private set; get; }

    /// <summary>
    /// 文件根目录
    /// </summary>
    public string FileRoot
    {
        get { return string.Empty; }
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
    /// 自定义参数：远程服务接口
    /// </summary>
    public IRemoteServices RemoteServices { private set; get; } = null;

    #endregion

    [AssetSystemPreserve]
    public ByteGameFileSystem()
    {
        PackageName = string.Empty;
    }

    [AssetSystemPreserve]
    public virtual FSInitializeFileSystemOperation InitializeFileSystemAsync()
    {
        var operation = new BGFSInitializeOperation(this);
        OperationSystem.StartOperation(PackageName, operation);
        return operation;
    }

    [AssetSystemPreserve]
    public FSRequestPackageVersionOperation LoadLocalPackageVersionAsync(bool appendTimeTicks, int timeout)
    {
        var operation = new BGFSRequestPackageVersionOperation(this, timeout);
        OperationSystem.StartOperation(PackageName, operation);
        return operation;
    }

    [AssetSystemPreserve]
    public FSLoadPackageManifestOperation LoadLocalPackageManifestAsync(string packageVersion, int timeout)
    {
        var operation = new BGFSLoadPackageManifestOperation(this, packageVersion, timeout);
        OperationSystem.StartOperation(PackageName, operation);
        return operation;
    }

    [AssetSystemPreserve]
    public FSLoadPackageManifestOperation RequestRemotePackageManifestAsync(string packageVersion, int timeout)
    {
        var operation = new BGFSLoadPackageManifestOperation(this, packageVersion, timeout);
        OperationSystem.StartOperation(PackageName, operation);
        return operation;
    }

    [AssetSystemPreserve]
    public FSRequestPackageVersionOperation RequestRemotePackageVersionAsync(bool appendTimeTicks, int timeout)
    {
        var operation = new BGFSRequestPackageVersionOperation(this, timeout);
        OperationSystem.StartOperation(PackageName, operation);
        return operation;
    }

    [AssetSystemPreserve]
    public virtual FSLoadPackageManifestOperation LoadPackageManifestAsync(string packageVersion, int timeout)
    {
        var operation = new BGFSLoadPackageManifestOperation(this, packageVersion, timeout);
        OperationSystem.StartOperation(PackageName, operation);
        return operation;
    }

    [AssetSystemPreserve]
    public virtual FSRequestPackageVersionOperation RequestPackageVersionAsync(bool appendTimeTicks, int timeout)
    {
        var operation = new BGFSRequestPackageVersionOperation(this, timeout);
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
        param.MainURL = RemoteServices.GetRemoteMainURL(bundle.FileName, null);
        param.FallbackURL = RemoteServices.GetRemoteFallbackURL(bundle.FileName, null);
        var operation = new BGFSDownloadFileOperation(this, bundle, param);
        OperationSystem.StartOperation(PackageName, operation);
        return operation;
    }

    [AssetSystemPreserve]
    public virtual FSLoadBundleOperation LoadBundleFile(PackageBundle bundle)
    {
        var operation = new BGFSLoadBundleOperation(this, bundle);
        OperationSystem.StartOperation(PackageName, operation);
        return operation;
    }

    [AssetSystemPreserve]
    public virtual void UnloadBundleFile(PackageBundle bundle, object result)
    {
        var assetBundle = result as BundleFile;
        if (assetBundle != null)
        {
            assetBundle.Unload(true);
        }
    }

    [AssetSystemPreserve]
    public virtual void SetParameter(string name, object value)
    {
        if (name == FileSystemParametersDefine.REMOTE_SERVICES)
        {
            RemoteServices = (IRemoteServices)value;
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

        // 注意：CDN服务未启用的情况下，使用抖音WEB服务器
        if (RemoteServices == null)
        {
            var webRoot = PathUtility.Combine(GodotAssetPath.GetStreamingAssetsRoot(), AssetSystemSettingsData.Setting.DefaultYooFolderName, packageName);
            RemoteServices = new WebRemoteServices(webRoot);
        }

        _fileSystemManager = GetStarkFileSystemManager();
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
        if (_fileSystemManager == null)
        {
            return false;
        }

        var filePath = GetCacheFileLoadPath(bundle);
        return InvokeStarkAccessSync(_fileSystemManager, filePath);
    }

    [AssetSystemPreserve]
    public virtual bool NeedDownload(PackageBundle bundle)
    {
        if (Belong(bundle) == false)
        {
            return false;
        }

        return Exists(bundle) == false;
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
        throw new System.NotImplementedException();
    }

    [AssetSystemPreserve]
    public virtual string ReadFileText(PackageBundle bundle)
    {
        throw new System.NotImplementedException();
    }

    #region 内部方法

    [AssetSystemPreserve]
    private string GetCacheFileLoadPath(PackageBundle bundle)
    {
        if (_cacheFilePaths.TryGetValue(bundle.BundleGUID, out var filePath) == false)
        {
            if (_fileSystemManager == null)
            {
                filePath = bundle.FileName;
            }
            else
            {
                filePath = InvokeStarkGetLocalCachedPathForUrl(_fileSystemManager, bundle.FileName);
            }
            _cacheFilePaths.Add(bundle.BundleGUID, filePath);
        }

        return filePath;
    }

    [AssetSystemPreserve]
    private static object GetStarkFileSystemManager()
    {
        var starkType = System.Type.GetType("StarkSDKSpace.StarkSDK, StarkWebGL");
        var apiProperty = starkType?.GetProperty("API", BindingFlags.Public | BindingFlags.Static);
        var apiObject = apiProperty?.GetValue(null);
        var getManagerMethod = apiObject?.GetType().GetMethod("GetStarkFileSystemManager", BindingFlags.Public | BindingFlags.Instance);
        return getManagerMethod?.Invoke(apiObject, null);
    }

    [AssetSystemPreserve]
    private static bool InvokeStarkAccessSync(object fileSystemManager, string filePath)
    {
        var accessSyncMethod = fileSystemManager.GetType().GetMethod("AccessSync", BindingFlags.Public | BindingFlags.Instance);
        if (accessSyncMethod == null)
        {
            return false;
        }

        var result = accessSyncMethod.Invoke(fileSystemManager, new object[] { filePath });
        if (result is bool exists)
        {
            return exists;
        }

        if (result is string text)
        {
            return bool.TryParse(text, out var parsed) && parsed;
        }

        return false;
    }

    [AssetSystemPreserve]
    private static string InvokeStarkGetLocalCachedPathForUrl(object fileSystemManager, string fileName)
    {
        var getPathMethod = fileSystemManager.GetType().GetMethod("GetLocalCachedPathForUrl", BindingFlags.Public | BindingFlags.Instance);
        if (getPathMethod == null)
        {
            return fileName;
        }

        return getPathMethod.Invoke(fileSystemManager, new object[] { fileName })?.ToString() ?? fileName;
    }

    #endregion
}
