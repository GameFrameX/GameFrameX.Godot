using System.Collections.Generic;
using System.Reflection;
using Godot;
using UnityEngine;
using YooAsset;
using UnityEngine.Scripting;

[Preserve]
public static class ByteGameFileSystemCreater
{
    [UnityEngine.Scripting.Preserve]
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

    [UnityEngine.Scripting.Preserve]
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
[UnityEngine.Scripting.Preserve]
internal class ByteGameFileSystem : IFileSystem
{
    [Preserve]
    public sealed class WebRemoteServices : IRemoteServices
    {
        private readonly string _webPackageRoot;
        private readonly Dictionary<string, string> _mapping = new(10000);

        [UnityEngine.Scripting.Preserve]
        public WebRemoteServices(string buildinPackRoot)
        {
            _webPackageRoot = buildinPackRoot;
        }

        [UnityEngine.Scripting.Preserve]
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

        [UnityEngine.Scripting.Preserve]
        public string GetRemoteMainURL(string fileName, string packageVersion)
        {
            return GetFileLoadURL(fileName);
        }

        [UnityEngine.Scripting.Preserve]
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

    [Preserve]
    public ByteGameFileSystem()
    {
        PackageName = string.Empty;
    }

    [UnityEngine.Scripting.Preserve]
    public virtual FSInitializeFileSystemOperation InitializeFileSystemAsync()
    {
        var operation = new BGFSInitializeOperation(this);
        OperationSystem.StartOperation(PackageName, operation);
        return operation;
    }

    [UnityEngine.Scripting.Preserve]
    public FSRequestPackageVersionOperation LoadLocalPackageVersionAsync(bool appendTimeTicks, int timeout)
    {
        var operation = new BGFSRequestPackageVersionOperation(this, timeout);
        OperationSystem.StartOperation(PackageName, operation);
        return operation;
    }

    [UnityEngine.Scripting.Preserve]
    public FSLoadPackageManifestOperation LoadLocalPackageManifestAsync(string packageVersion, int timeout)
    {
        var operation = new BGFSLoadPackageManifestOperation(this, packageVersion, timeout);
        OperationSystem.StartOperation(PackageName, operation);
        return operation;
    }

    [UnityEngine.Scripting.Preserve]
    public FSLoadPackageManifestOperation RequestRemotePackageManifestAsync(string packageVersion, int timeout)
    {
        var operation = new BGFSLoadPackageManifestOperation(this, packageVersion, timeout);
        OperationSystem.StartOperation(PackageName, operation);
        return operation;
    }

    [UnityEngine.Scripting.Preserve]
    public FSRequestPackageVersionOperation RequestRemotePackageVersionAsync(bool appendTimeTicks, int timeout)
    {
        var operation = new BGFSRequestPackageVersionOperation(this, timeout);
        OperationSystem.StartOperation(PackageName, operation);
        return operation;
    }

    [UnityEngine.Scripting.Preserve]
    public virtual FSLoadPackageManifestOperation LoadPackageManifestAsync(string packageVersion, int timeout)
    {
        var operation = new BGFSLoadPackageManifestOperation(this, packageVersion, timeout);
        OperationSystem.StartOperation(PackageName, operation);
        return operation;
    }

    [UnityEngine.Scripting.Preserve]
    public virtual FSRequestPackageVersionOperation RequestPackageVersionAsync(bool appendTimeTicks, int timeout)
    {
        var operation = new BGFSRequestPackageVersionOperation(this, timeout);
        OperationSystem.StartOperation(PackageName, operation);
        return operation;
    }

    [UnityEngine.Scripting.Preserve]
    public virtual FSClearAllBundleFilesOperation ClearAllBundleFilesAsync()
    {
        var operation = new FSClearAllBundleFilesCompleteOperation();
        OperationSystem.StartOperation(PackageName, operation);
        return operation;
    }

    [UnityEngine.Scripting.Preserve]
    public virtual FSClearUnusedBundleFilesOperation ClearUnusedBundleFilesAsync(PackageManifest manifest)
    {
        var operation = new FSClearUnusedBundleFilesCompleteOperation();
        OperationSystem.StartOperation(PackageName, operation);
        return operation;
    }

    [UnityEngine.Scripting.Preserve]
    public virtual FSDownloadFileOperation DownloadFileAsync(PackageBundle bundle, DownloadParam param)
    {
        param.MainURL = RemoteServices.GetRemoteMainURL(bundle.FileName, null);
        param.FallbackURL = RemoteServices.GetRemoteFallbackURL(bundle.FileName, null);
        var operation = new BGFSDownloadFileOperation(this, bundle, param);
        OperationSystem.StartOperation(PackageName, operation);
        return operation;
    }

    [UnityEngine.Scripting.Preserve]
    public virtual FSLoadBundleOperation LoadBundleFile(PackageBundle bundle)
    {
        var operation = new BGFSLoadBundleOperation(this, bundle);
        OperationSystem.StartOperation(PackageName, operation);
        return operation;
    }

    [UnityEngine.Scripting.Preserve]
    public virtual void UnloadBundleFile(PackageBundle bundle, object result)
    {
        var assetBundle = result as AssetBundle;
        if (assetBundle != null)
        {
            assetBundle.Unload(true);
        }
    }

    [UnityEngine.Scripting.Preserve]
    public virtual void SetParameter(string name, object value)
    {
        if (name == FileSystemParametersDefine.REMOTE_SERVICES)
        {
            RemoteServices = (IRemoteServices)value;
        }
        else
        {
            YooLogger.Warning($"Invalid parameter : {name}");
        }
    }

    [UnityEngine.Scripting.Preserve]
    public virtual void OnCreate(string packageName, string rootDirectory)
    {
        PackageName = packageName;

        // 注意：CDN服务未启用的情况下，使用抖音WEB服务器
        if (RemoteServices == null)
        {
            var webRoot = PathUtility.Combine(Application.streamingAssetsPath, YooAssetSettingsData.Setting.DefaultYooFolderName, packageName);
            RemoteServices = new WebRemoteServices(webRoot);
        }

        _fileSystemManager = GetStarkFileSystemManager();
    }

    [UnityEngine.Scripting.Preserve]
    public virtual void OnUpdate()
    {
    }

    [UnityEngine.Scripting.Preserve]
    public virtual bool Belong(PackageBundle bundle)
    {
        return true;
    }

    [UnityEngine.Scripting.Preserve]
    public virtual bool Exists(PackageBundle bundle)
    {
        if (_fileSystemManager == null)
        {
            return false;
        }

        var filePath = GetCacheFileLoadPath(bundle);
        return InvokeStarkAccessSync(_fileSystemManager, filePath);
    }

    [UnityEngine.Scripting.Preserve]
    public virtual bool NeedDownload(PackageBundle bundle)
    {
        if (Belong(bundle) == false)
        {
            return false;
        }

        return Exists(bundle) == false;
    }

    [UnityEngine.Scripting.Preserve]
    public virtual bool NeedUnpack(PackageBundle bundle)
    {
        return false;
    }

    [UnityEngine.Scripting.Preserve]
    public virtual bool NeedImport(PackageBundle bundle)
    {
        return false;
    }

    [UnityEngine.Scripting.Preserve]
    public virtual byte[] ReadFileData(PackageBundle bundle)
    {
        throw new System.NotImplementedException();
    }

    [UnityEngine.Scripting.Preserve]
    public virtual string ReadFileText(PackageBundle bundle)
    {
        throw new System.NotImplementedException();
    }

    #region 内部方法

    [UnityEngine.Scripting.Preserve]
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

    [UnityEngine.Scripting.Preserve]
    private static object GetStarkFileSystemManager()
    {
        var starkType = System.Type.GetType("StarkSDKSpace.StarkSDK, StarkWebGL");
        var apiProperty = starkType?.GetProperty("API", BindingFlags.Public | BindingFlags.Static);
        var apiObject = apiProperty?.GetValue(null);
        var getManagerMethod = apiObject?.GetType().GetMethod("GetStarkFileSystemManager", BindingFlags.Public | BindingFlags.Instance);
        return getManagerMethod?.Invoke(apiObject, null);
    }

    [UnityEngine.Scripting.Preserve]
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

    [UnityEngine.Scripting.Preserve]
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
