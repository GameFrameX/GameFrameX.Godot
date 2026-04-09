namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    internal class RequestWebPackageVersionOperation : AsyncOperationBase
    {
        [AssetSystemPreserve]
        private enum ESteps
        {
            None,
            RequestPackageVersion,
            Done,
        }

        private readonly DefaultWebFileSystem _fileSystem;
        private readonly int _timeout;
        private readonly bool _appendTimeTicks;
        private int _failedTryAgain = 1;
        private WebTextRequestOperation _webTextRequestOp;
        private HttpTextRequestOperation _httpTextRequestOp;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 包裹版本
        /// </summary>
        public string PackageVersion { private set; get; }


        [AssetSystemPreserve]
        internal RequestWebPackageVersionOperation(DefaultWebFileSystem fileSystem, int timeout, bool appendTimeTicks)
        {
            _fileSystem = fileSystem;
            _timeout = timeout;
            _appendTimeTicks = appendTimeTicks;
        }

        [AssetSystemPreserve]
        public override void InternalOnStart()
        {
            _steps = ESteps.RequestPackageVersion;
        }

        [AssetSystemPreserve]
        public override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
            {
                return;
            }

            if (_steps == ESteps.RequestPackageVersion)
            {
                if (_webTextRequestOp == null && _httpTextRequestOp == null)
                {
                    var filePath = _fileSystem.GetWebPackageVersionFilePath();
                    var url = DownloadSystemHelper.ConvertToWWWPath(filePath);
                    if (DownloadSystemHelper.HttpTransport != null)
                    {
                        _httpTextRequestOp = new HttpTextRequestOperation(url, _timeout, _appendTimeTicks);
                        OperationSystem.StartOperation(_fileSystem.PackageName, _httpTextRequestOp);
                    }
                    else
                    {
                        _webTextRequestOp = new WebTextRequestOperation(url, _timeout, _appendTimeTicks);
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
                    PackageVersion = _httpTextRequestOp != null ? _httpTextRequestOp.Result : _webTextRequestOp.Result;
                    if (string.IsNullOrEmpty(PackageVersion))
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = $"Web package version file content is empty !";
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
