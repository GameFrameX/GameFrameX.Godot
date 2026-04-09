using System.IO;
namespace GameFrameX.AssetSystem
{
    /// <summary>
    /// 加载AssetBundle文件
    /// </summary>
    [AssetSystemPreserve]
    internal class DBFSLoadAssetBundleOperation : FSLoadBundleOperation
    {
        [AssetSystemPreserve]
        private enum ESteps
        {
            None,
            LoadAssetBundle,
            CheckResult,
            Done,
        }

        private readonly DefaultBuildinFileSystem _fileSystem;
        private readonly PackageBundle _bundle;
        private BundleFileCreateRequest _createRequest;
        private bool _isWaitForAsyncComplete = false;
        private ESteps _steps = ESteps.None;


        [AssetSystemPreserve]
        internal DBFSLoadAssetBundleOperation(DefaultBuildinFileSystem fileSystem, PackageBundle bundle)
        {
            _fileSystem = fileSystem;
            _bundle = bundle;
        }

        [AssetSystemPreserve]
        public override void InternalOnStart()
        {
            DownloadProgress = 1f;
            DownloadedBytes = _bundle.FileSize;
            _steps = ESteps.LoadAssetBundle;
        }

        [AssetSystemPreserve]
        public override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
            {
                return;
            }

            if (_steps == ESteps.LoadAssetBundle)
            {
                if (_bundle.Encrypted)
                {
                    if (_fileSystem.DecryptionServices == null)
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = $"The {nameof(IDecryptionServices)} is null !";
                        AssetSystemLogger.Error(Error);
                        return;
                    }
                }

                if (_isWaitForAsyncComplete)
                {
                    if (_bundle.Encrypted)
                    {
                        Result = _fileSystem.LoadEncryptedAssetBundle(_bundle);
                    }
                    else
                    {
                        var filePath = _fileSystem.GetBuildinFileLoadPath(_bundle);
                        Result = BundleFile.LoadFromFile(filePath);
                    }
                }
                else
                {
                    if (_bundle.Encrypted)
                    {
                        _createRequest = _fileSystem.LoadEncryptedAssetBundleAsync(_bundle);
                    }
                    else
                    {
                        var filePath = _fileSystem.GetBuildinFileLoadPath(_bundle);
                        _createRequest = BundleFile.LoadFromFileAsync(filePath);
                    }
                }

                _steps = ESteps.CheckResult;
            }

            if (_steps == ESteps.CheckResult)
            {
                if (_createRequest != null)
                {
                    if (_isWaitForAsyncComplete)
                    {
                        // 强制挂起主线程（注意：该操作会很耗时）
                        AssetSystemLogger.Warning("Suspend the main thread to load unity bundle.");
                        Result = _createRequest.BundleFile;
                    }
                    else
                    {
                        if (_createRequest.isDone == false)
                        {
                            return;
                        }

                        Result = _createRequest.BundleFile;
                    }
                }

                if (Result != null)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                    return;
                }

                if (_bundle.Encrypted)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"Failed to load encrypted buildin asset bundle file : {_bundle.BundleName}";
                    AssetSystemLogger.Error(Error);
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"Failed to load buildin asset bundle file : {_bundle.BundleName}";
                    AssetSystemLogger.Error(Error);
                }
            }
        }

        [AssetSystemPreserve]
        public override void InternalWaitForAsyncComplete()
        {
            _isWaitForAsyncComplete = true;

            while (true)
            {
                if (ExecuteWhileDone())
                {
                    _steps = ESteps.Done;
                    break;
                }
            }
        }

        [AssetSystemPreserve]
        public override void AbortDownloadOperation()
        {
        }
    }

    /// <summary>
    /// 加载原生文件
    /// </summary>
    [AssetSystemPreserve]
    internal class DBFSLoadRawBundleOperation : FSLoadBundleOperation
    {
        [AssetSystemPreserve]
        private enum ESteps
        {
            None,
            LoadBuildinRawBundle,
            Done,
        }

        private readonly DefaultBuildinFileSystem _fileSystem;
        private readonly PackageBundle _bundle;
        private ESteps _steps = ESteps.None;


        [AssetSystemPreserve]
        internal DBFSLoadRawBundleOperation(DefaultBuildinFileSystem fileSystem, PackageBundle bundle)
        {
            _fileSystem = fileSystem;
            _bundle = bundle;
        }

        [AssetSystemPreserve]
        public override void InternalOnStart()
        {
            DownloadProgress = 1f;
            DownloadedBytes = _bundle.FileSize;
            _steps = ESteps.LoadBuildinRawBundle;
        }

        [AssetSystemPreserve]
        public override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
            {
                return;
            }

            if (_steps == ESteps.LoadBuildinRawBundle)
            {
                var filePath = _fileSystem.GetBuildinFileLoadPath(_bundle);
                if (File.Exists(filePath))
                {
                    _steps = ESteps.Done;
                    Result = new RawBundle(_fileSystem, _bundle, filePath);
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"Can not found buildin raw bundle file : {filePath}";
                    AssetSystemLogger.Error(Error);
                }
            }
        }

        [AssetSystemPreserve]
        public override void InternalWaitForAsyncComplete()
        {
            while (true)
            {
                if (ExecuteWhileDone())
                {
                    _steps = ESteps.Done;
                    break;
                }
            }
        }

        [AssetSystemPreserve]
        public override void AbortDownloadOperation()
        {
        }
    }
}
