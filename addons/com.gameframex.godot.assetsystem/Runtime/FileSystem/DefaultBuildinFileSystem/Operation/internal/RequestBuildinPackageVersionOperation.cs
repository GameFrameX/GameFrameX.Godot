namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    internal class RequestBuildinPackageVersionOperation : AsyncOperationBase
    {
        [AssetSystemPreserve]
        private enum ESteps
        {
            None,
            RequestPackageVersion,
            Done,
        }

        private readonly DefaultBuildinFileSystem _fileSystem;
        private WebTextRequestOperation _webTextRequestOp;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 包裹版本
        /// </summary>
        public string PackageVersion { private set; get; }


        [AssetSystemPreserve]
        internal RequestBuildinPackageVersionOperation(DefaultBuildinFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
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
                if (_webTextRequestOp == null)
                {
                    var filePath = _fileSystem.GetBuildinPackageVersionFilePath();
                    var url = DownloadSystemHelper.ConvertToWWWPath(filePath);
                    _webTextRequestOp = new WebTextRequestOperation(url);
                    OperationSystem.StartOperation(_fileSystem.PackageName, _webTextRequestOp);
                }

                if (_webTextRequestOp.IsDone == false)
                {
                    return;
                }

                if (_webTextRequestOp.Status == EOperationStatus.Succeed)
                {
                    PackageVersion = _webTextRequestOp.Result;
                    if (string.IsNullOrEmpty(PackageVersion))
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Failed;
                        Error = $"Buildin package version file content is empty !";
                    }
                    else
                    {
                        _steps = ESteps.Done;
                        Status = EOperationStatus.Succeed;
                    }
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _webTextRequestOp.Error;
                }
            }
        }
    }
}