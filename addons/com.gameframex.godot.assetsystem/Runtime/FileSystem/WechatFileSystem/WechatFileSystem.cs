using System.Collections.Generic;
using System.Reflection;
using Godot;
using GameFrameX.AssetSystem;

[AssetSystemPreserve]
public static class WechatFileSystemCreater
{
    [AssetSystemPreserve]
    public static FileSystemParameters CreateWechatFileSystemParameters(IRemoteServices remoteServices = null)
    {
        if (OS.HasFeature("web") == false)
        {
            throw new System.NotSupportedException("Wechat file system only supports web platform runtime.");
        }

        string fileSystemClass = typeof(WechatFileSystem).FullName;
        var fileSystemParams = new FileSystemParameters(fileSystemClass, null);
        fileSystemParams.AddParameter(FileSystemParametersDefine.REMOTE_SERVICES, remoteServices);
        return fileSystemParams;
    }

    [AssetSystemPreserve]
    public static FileSystemParameters CreateWechatPathFileSystemParameters(string buildinPackRoot)
    {
        if (OS.HasFeature("web") == false)
        {
            throw new System.NotSupportedException("Wechat file system only supports web platform runtime.");
        }

        string fileSystemClass = typeof(WechatFileSystem).FullName;
        var fileSystemParams = new FileSystemParameters(fileSystemClass, null);
        IRemoteServices remoteServices = new WechatFileSystem.WebRemoteServices(buildinPackRoot);
        fileSystemParams.AddParameter(FileSystemParametersDefine.REMOTE_SERVICES, remoteServices);
        return fileSystemParams;
    }
}

/// <summary>
/// 微信小游戏文件系统
/// 参考：https://wechat-miniprogram.github.io/minigame-unity-webgl-transform/Design/UsingAssetBundle.html
/// </summary>
[AssetSystemPreserve]
internal class WechatFileSystem : IFileSystem
{
    [AssetSystemPreserve]
    public class WebRemoteServices : IRemoteServices
    {
        private readonly string _webPackageRoot;
        protected readonly Dictionary<string, string> _mapping = new Dictionary<string, string>(10000);

        [AssetSystemPreserve]
        public WebRemoteServices(string buildinPackRoot)
        {
            _webPackageRoot = buildinPackRoot;
        }

        [AssetSystemPreserve]
        string IRemoteServices.GetRemoteMainURL(string fileName,string packageVersion)
        {
            return GetFileLoadURL(fileName, packageVersion);
        }

        [AssetSystemPreserve]
        string IRemoteServices.GetRemoteFallbackURL(string fileName, string packageVersion)
        {
            return GetFileLoadURL(fileName, packageVersion);
        }

        [AssetSystemPreserve]
        private string GetFileLoadURL(string fileName, string packageVersion)
        {
            if (_mapping.TryGetValue(fileName, out string url) == false)
            {
                var filePath = PathUtility.Combine(_webPackageRoot, fileName);
                url = DownloadSystemHelper.ConvertToWWWPath(filePath);
                _mapping.Add(fileName, url);
            }
            //Debug.LogError($"WeChatFileSystem GetFileLoadURL url:{url}");
            return url;
        }
    }

    private readonly Dictionary<string, string> _cacheFilePaths = new Dictionary<string, string>(10000);
    private object _fileSystemManager;
    private string _fileCacheRoot = string.Empty;

    /// <summary>
    /// 包裹名称
    /// </summary>
    public string PackageName { private set; get; }

    /// <summary>
    /// 文件根目录
    /// </summary>
    public string FileRoot
    {
        get { return _fileCacheRoot; }
    }

    /// <summary>
    /// 文件数量
    /// </summary>
    public int FileCount
    {
        get { return 0; }
    }

    public string PackageVersion { get; set; }

    #region 自定义参数

    /// <summary>
    /// 自定义参数：远程服务接口
    /// </summary>
    public IRemoteServices RemoteServices { private set; get; } = null;

    #endregion

    [AssetSystemPreserve]
    public WechatFileSystem()
    {
    }

    [AssetSystemPreserve]
    public virtual FSInitializeFileSystemOperation InitializeFileSystemAsync()
    {
        var operation = new WXFSInitializeOperation(this);
        OperationSystem.StartOperation(PackageName, operation);
        return operation;
    }

    [AssetSystemPreserve]
    public virtual FSLoadPackageManifestOperation LoadPackageManifestAsync(string packageVersion, int timeout)
    {
        PackageVersion = packageVersion;
        var operation = new WXFSLoadPackageManifestOperation(this, packageVersion, timeout);
        OperationSystem.StartOperation(PackageName, operation);
        return operation;
    }

    [AssetSystemPreserve]
    public virtual FSRequestPackageVersionOperation RequestPackageVersionAsync(bool appendTimeTicks, int timeout)
    {
        var operation = new WXFSRequestPackageVersionOperation(this, appendTimeTicks, timeout);
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
        param.MainURL = RemoteServices.GetRemoteMainURL(bundle.FileName,PackageVersion);
        param.FallbackURL = RemoteServices.GetRemoteFallbackURL(bundle.FileName,PackageVersion);
        var operation = new WXFSDownloadFileOperation(this, bundle, param);
        OperationSystem.StartOperation(PackageName, operation);
        return operation;
    }

    [AssetSystemPreserve]
    public virtual FSLoadBundleOperation LoadBundleFile(PackageBundle bundle)
    {
        var operation = new WXFSLoadBundleOperation(this, bundle,PackageVersion);
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

        // 注意：CDN服务未启用的情况下，使用微信WEB服务器
        if (RemoteServices == null)
        {
            string webRoot = PathUtility.Combine(GodotAssetPath.GetStreamingAssetsRoot(), AssetSystemSettingsData.Setting.DefaultAssetSystemFolderName, packageName);
            RemoteServices = new WebRemoteServices(webRoot);
        }
        
        _fileSystemManager = GetWechatFileSystemManager();
        _fileCacheRoot = GetWechatUserDataPath();
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
        string filePath = GetCacheFileLoadPath(bundle);
        if (_fileSystemManager == null)
        {
            return false;
        }

        string result = InvokeWechatAccessSync(_fileSystemManager, filePath);
        return result.Equals("access:ok");
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
        if (_cacheFilePaths.TryGetValue(bundle.BundleGUID, out string filePath) == false)
        {
            filePath = PathUtility.Combine(_fileCacheRoot, bundle.FileName);
            _cacheFilePaths.Add(bundle.BundleGUID, filePath);
        }

        return filePath;
    }

    [AssetSystemPreserve]
    private static object GetWechatFileSystemManager()
    {
        var wxBaseType = System.Type.GetType("WeChatWASM.WXBase, Wx");
        var getManagerMethod = wxBaseType?.GetMethod("GetFileSystemManager", BindingFlags.Public | BindingFlags.Static);
        return getManagerMethod?.Invoke(null, null);
    }

    [AssetSystemPreserve]
    private static string GetWechatUserDataPath()
    {
        var wxType = System.Type.GetType("WeChatWASM.WX, Wx");
        var envProperty = wxType?.GetProperty("env", BindingFlags.Public | BindingFlags.Static);
        var envObject = envProperty?.GetValue(null);
        var userDataPathProperty = envObject?.GetType().GetProperty("USER_DATA_PATH", BindingFlags.Public | BindingFlags.Instance);
        return userDataPathProperty?.GetValue(envObject)?.ToString() ?? string.Empty;
    }

    [AssetSystemPreserve]
    private static string InvokeWechatAccessSync(object fileSystemManager, string filePath)
    {
        var accessSyncMethod = fileSystemManager.GetType().GetMethod("AccessSync", BindingFlags.Public | BindingFlags.Instance);
        if (accessSyncMethod == null)
        {
            return string.Empty;
        }

        return accessSyncMethod.Invoke(fileSystemManager, new object[] { filePath })?.ToString() ?? string.Empty;
    }

    [AssetSystemPreserve]
    public FSRequestPackageVersionOperation LoadLocalPackageVersionAsync(bool appendTimeTicks, int timeout)
    {
        var operation = new WXFSRequestPackageVersionOperation(this, appendTimeTicks, timeout);
        OperationSystem.StartOperation(PackageName, operation);
        return operation;
    }

    [AssetSystemPreserve]
    public FSLoadPackageManifestOperation LoadLocalPackageManifestAsync(string packageVersion, int timeout)
    {
        PackageVersion = packageVersion;
        var operation = new WXFSLoadPackageManifestOperation(this, packageVersion, timeout);
        OperationSystem.StartOperation(PackageName, operation);
        return operation;
    }

    [AssetSystemPreserve]
    public FSLoadPackageManifestOperation RequestRemotePackageManifestAsync(string packageVersion, int timeout)
    {
        PackageVersion = packageVersion;
        var operation = new WXFSLoadPackageManifestOperation(this, packageVersion, timeout);
        OperationSystem.StartOperation(PackageName, operation);
        return operation;
    }

    [AssetSystemPreserve]
    public FSRequestPackageVersionOperation RequestRemotePackageVersionAsync(bool appendTimeTicks, int timeout)
    {
        var operation = new WXFSRequestPackageVersionOperation(this, appendTimeTicks, timeout);
        OperationSystem.StartOperation(PackageName, operation);
        return operation;
    }

    #endregion
}

