namespace YooAsset
{
    [UnityEngine.Scripting.Preserve]
    internal class RequestWebPackageHashOperation : AsyncOperationBase
    {
        [UnityEngine.Scripting.Preserve]
        private enum ESteps
        {
            None,
            RequestPackageHash,
            Done,
        }

        private readonly DefaultWebFileSystem _fileSystem;
        private readonly string _packageVersion;
        private readonly int _timeout;
        private readonly bool _appendTimeTicks;
        private int _failedTryAgain = 1;
        private UnityWebTextRequestOperation _webTextRequestOp;
        private HttpTextRequestOperation _httpTextRequestOp;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 包裹哈希值
        /// </summary>
        public string PackageHash { private set; get; }


        [UnityEngine.Scripting.Preserve]
        public RequestWebPackageHashOperation(DefaultWebFileSystem fileSystem, string packageVersion, int timeout, bool appendTimeTicks)
        {
            _fileSystem = fileSystem;
            _packageVersion = packageVersion;
            _timeout = timeout;
            _appendTimeTicks = appendTimeTicks;
        }

        [UnityEngine.Scripting.Preserve]
        public override void InternalOnStart()
        {
            _steps = ESteps.RequestPackageHash;
        }

        [UnityEngine.Scripting.Preserve]
        public override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
            {
                return;
            }

            if (_steps == ESteps.RequestPackageHash)
            {
                if (_webTextRequestOp == null && _httpTextRequestOp == null)
                {
                    var filePath = _fileSystem.GetWebPackageHashFilePath(_packageVersion);
                    var url = DownloadSystemHelper.ConvertToWWWPath(filePath);
                    if (DownloadSystemHelper.HttpTransport != null)
                    {
                        _httpTextRequestOp = new HttpTextRequestOperation(url, _timeout, _appendTimeTicks);
                        OperationSystem.StartOperation(_fileSystem.PackageName, _httpTextRequestOp);
                    }
                    else
                    {
                        _webTextRequestOp = new UnityWebTextRequestOperation(url, _timeout, _appendTimeTicks);
                        OperationSystem.StartOperation(_fileSystem.PackageName, _webTextRequestOp);
                    }
                }

                var currentOperation = (AsyncOperationBase)_httpTextRequestOp ?? _webTextRequestOp;
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
                    PackageHash = _httpTextRequestOp != null ? _httpTextRequestOp.Result : _webTextRequestOp.Result;
                    if (string.IsNullOrEmpty(PackageHash))
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = $"Web package hash file content is empty !";
                    }
                    else
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Succeed;
                    }
                }
                else
                {
                    if (_failedTryAgain > 0)
                    {
                        _failedTryAgain--;
                        _webTextRequestOp = null;
                        _httpTextRequestOp = null;
                        return;
                    }

                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = currentOperation.Error;
                }
            }
        }
    }
}
