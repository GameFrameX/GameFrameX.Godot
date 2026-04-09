using System.IO;

namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    internal class DownloadPackageManifestOperation : AsyncOperationBase
    {
        [AssetSystemPreserve]
        private enum ESteps
        {
            None,
            CheckExist,
            DownloadFile,
            Done,
        }

        private readonly DefaultCacheFileSystem _fileSystem;
        private readonly string _packageVersion;
        private readonly int _timeout;
        private WebFileRequestOperation _webFileRequestOp;
        private HttpDataRequestOperation _httpDataRequestOp;
        private int _requestCount = 0;
        private ESteps _steps = ESteps.None;


        [AssetSystemPreserve]
        internal DownloadPackageManifestOperation(DefaultCacheFileSystem fileSystem, string packageVersion, int timeout)
        {
            _fileSystem = fileSystem;
            _packageVersion = packageVersion;
            _timeout = timeout;
        }

        [AssetSystemPreserve]
        public override void InternalOnStart()
        {
            _requestCount = WebRequestCounter.GetRequestFailedCount(_fileSystem.PackageName, nameof(DownloadPackageManifestOperation));
            _steps = ESteps.DownloadFile;
        }

        [AssetSystemPreserve]
        public override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
            {
                return;
            }

            if (_steps == ESteps.CheckExist)
            {
                var filePath = _fileSystem.GetCachePackageManifestFilePath(_packageVersion);
                if (File.Exists(filePath))
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.DownloadFile;
                }
            }

            if (_steps == ESteps.DownloadFile)
            {
                if (_webFileRequestOp == null && _httpDataRequestOp == null)
                {
                    var savePath = _fileSystem.GetCachePackageManifestFilePath(_packageVersion);
                    var fileName = AssetSystemSettingsData.GetManifestBinaryFileName(_fileSystem.PackageName, _packageVersion);
                    var webURL = DownloadSystemHelper.ConvertToWWWPath(GetDownloadRequestURL(fileName));
                    if (DownloadSystemHelper.HttpTransport != null)
                    {
                        _httpDataRequestOp = new HttpDataRequestOperation(webURL, _timeout, appendTimeTicks: false);
                        OperationSystem.StartOperation(_fileSystem.PackageName, _httpDataRequestOp);
                    }
                    else
                    {
                        _webFileRequestOp = new WebFileRequestOperation(webURL, savePath, _timeout);
                        OperationSystem.StartOperation(_fileSystem.PackageName, _webFileRequestOp);
                    }
                }

                var currentOperation = (AsyncOperationBase)_httpDataRequestOp ?? _webFileRequestOp;
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
                    if (_httpDataRequestOp != null)
                    {
                        var savePath = _fileSystem.GetCachePackageManifestFilePath(_packageVersion);
                        if (WriteDownloadedData(savePath, _httpDataRequestOp.Result) == false)
                        {
                            _steps = ESteps.Done;
                            Status = EOperationStatus.Failed;
                            Error = $"Failed to write manifest file : {savePath}";
                            WebRequestCounter.RecordRequestFailed(_fileSystem.PackageName, nameof(DownloadPackageManifestOperation));
                            return;
                        }
                    }

                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = currentOperation.Error;
                    WebRequestCounter.RecordRequestFailed(_fileSystem.PackageName, nameof(DownloadPackageManifestOperation));
                }
            }
        }

        private static bool WriteDownloadedData(string savePath, byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return false;
            }

            FileUtility.CreateFileDirectory(savePath);
            File.WriteAllBytes(savePath, data);
            return true;
        }

        [AssetSystemPreserve]
        private string GetDownloadRequestURL(string fileName)
        {
            // 轮流返回请求地址
            if (_requestCount % 2 == 0)
            {
                return _fileSystem.RemoteServices.GetRemoteMainURL(fileName, _packageVersion);
            }
            else
            {
                return _fileSystem.RemoteServices.GetRemoteFallbackURL(fileName, _packageVersion);
            }
        }
    }
}
