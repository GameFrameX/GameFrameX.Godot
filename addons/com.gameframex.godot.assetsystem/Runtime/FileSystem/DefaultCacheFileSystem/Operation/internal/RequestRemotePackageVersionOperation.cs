namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    internal class RequestRemotePackageVersionOperation : AsyncOperationBase
    {
        [AssetSystemPreserve]
        private enum ESteps
        {
            None,
            RequestPackageVersion,
            Done,
        }

        private readonly DefaultCacheFileSystem _fileSystem;
        private readonly bool _appendTimeTicks;
        private readonly int _timeout;
        private int _failedTryAgain = 1;
        private WebTextRequestOperation _webTextRequestOp;
        private HttpTextRequestOperation _httpTextRequestOp;
        private int _requestCount = 0;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 包裹版本
        /// </summary>
        internal string PackageVersion { set; get; }


        [AssetSystemPreserve]
        internal RequestRemotePackageVersionOperation(DefaultCacheFileSystem fileSystem, bool appendTimeTicks, int timeout)
        {
            _fileSystem = fileSystem;
            _appendTimeTicks = appendTimeTicks;
            _timeout = timeout;
        }

        [AssetSystemPreserve]
        public override void InternalOnStart()
        {
            _requestCount = WebRequestCounter.GetRequestFailedCount(_fileSystem.PackageName, nameof(RequestRemotePackageVersionOperation));
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
                    var fileName = AssetSystemSettingsData.GetPackageVersionFileName(_fileSystem.PackageName);
                    var url = DownloadSystemHelper.ConvertToWWWPath(GetWebRequestURL(fileName));
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
                    var rawVersion = _httpTextRequestOp != null ? _httpTextRequestOp.Result : _webTextRequestOp.Result;
                    PackageVersion = NormalizeText(rawVersion);
                    if (string.IsNullOrEmpty(PackageVersion))
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = $"Remote package version file content is empty !";
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
                        _requestCount++;
                        _webTextRequestOp = null;
                        _httpTextRequestOp = null;
                        return;
                    }

                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = currentOperation.Error;
                    WebRequestCounter.RecordRequestFailed(_fileSystem.PackageName, nameof(RequestRemotePackageVersionOperation));
                }
            }
        }

        private static string NormalizeText(string value)
        {
            return (value ?? string.Empty).Replace("\uFEFF", string.Empty).Trim();
        }

        [AssetSystemPreserve]
        private string GetWebRequestURL(string fileName)
        {
            string url;

            // 轮流返回请求地址
            if (_requestCount % 2 == 0)
            {
                url = _fileSystem.RemoteServices.GetRemoteMainURL(fileName, "");
            }
            else
            {
                url = _fileSystem.RemoteServices.GetRemoteFallbackURL(fileName, "");
            }

            return url;
        }
    }
}
