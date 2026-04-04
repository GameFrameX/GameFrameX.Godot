namespace YooAsset
{
    [UnityEngine.Scripting.Preserve]
    internal class LoadWebPackageManifestOperation : AsyncOperationBase
    {
        [UnityEngine.Scripting.Preserve]
        private enum ESteps
        {
            None,
            RequestFileData,
            VerifyFileData,
            LoadManifest,
            Done,
        }

        private readonly DefaultWebFileSystem _fileSystem;
        private readonly string _packageVersion;
        private readonly string _packageHash;
        private readonly int _timeout;
        private readonly bool _appendTimeTicks;
        private int _failedTryAgain = 1;
        private UnityWebDataRequestOperation _webDataRequestOp;
        private HttpDataRequestOperation _httpDataRequestOp;
        private DeserializeManifestOperation _deserializer;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 包裹清单
        /// </summary>
        public PackageManifest Manifest { private set; get; }


        [UnityEngine.Scripting.Preserve]
        internal LoadWebPackageManifestOperation(DefaultWebFileSystem fileSystem, string packageVersion, string packageHash, int timeout, bool appendTimeTicks)
        {
            _fileSystem = fileSystem;
            _packageVersion = packageVersion;
            _packageHash = packageHash;
            _timeout = timeout;
            _appendTimeTicks = appendTimeTicks;
        }

        [UnityEngine.Scripting.Preserve]
        public override void InternalOnStart()
        {
            _steps = ESteps.RequestFileData;
        }

        [UnityEngine.Scripting.Preserve]
        public override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
            {
                return;
            }

            if (_steps == ESteps.RequestFileData)
            {
                if (_webDataRequestOp == null && _httpDataRequestOp == null)
                {
                    var filePath = _fileSystem.GetWebPackageManifestFilePath(_packageVersion);
                    var url = DownloadSystemHelper.ConvertToWWWPath(filePath);
                    if (DownloadSystemHelper.HttpTransport != null)
                    {
                        _httpDataRequestOp = new HttpDataRequestOperation(url, _timeout, _appendTimeTicks);
                        OperationSystem.StartOperation(_fileSystem.PackageName, _httpDataRequestOp);
                    }
                    else
                    {
                        _webDataRequestOp = new UnityWebDataRequestOperation(url, _timeout, _appendTimeTicks);
                        OperationSystem.StartOperation(_fileSystem.PackageName, _webDataRequestOp);
                    }
                }

                var currentOperation = (AsyncOperationBase)_httpDataRequestOp ?? _webDataRequestOp;
                if (currentOperation == null)
                {
                    return;
                }

                Progress = currentOperation.Progress;
                if (currentOperation.IsDone == false)
                {
                    return;
                }

                if (currentOperation.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.VerifyFileData;
                }
                else
                {
                    if (_failedTryAgain > 0)
                    {
                        _failedTryAgain--;
                        _webDataRequestOp = null;
                        _httpDataRequestOp = null;
                        return;
                    }

                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = currentOperation.Error;
                }
            }

            if (_steps == ESteps.VerifyFileData)
            {
                var manifestBytes = _httpDataRequestOp != null ? _httpDataRequestOp.Result : _webDataRequestOp.Result;
                var fileHash = HashUtility.BytesMD5(manifestBytes);
                if (fileHash == _packageHash)
                {
                    _steps = ESteps.LoadManifest;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = "Failed to verify web package manifest file!";
                }
            }

            if (_steps == ESteps.LoadManifest)
            {
                if (_deserializer == null)
                {
                    var manifestBytes = _httpDataRequestOp != null ? _httpDataRequestOp.Result : _webDataRequestOp.Result;
                    _deserializer = new DeserializeManifestOperation(manifestBytes);
                    OperationSystem.StartOperation(_fileSystem.PackageName, _deserializer);
                }

                Progress = _deserializer.Progress;
                if (_deserializer.IsDone == false)
                {
                    return;
                }

                if (_deserializer.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Manifest = _deserializer.Manifest;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _deserializer.Error;
                }
            }
        }
    }
}
