namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    internal class DWFSLoadPackageManifestOperation : FSLoadPackageManifestOperation
    {
        [AssetSystemPreserve]
        private enum ESteps
        {
            None,
            CheckPackageVersion,
            RequestWebPackageVersion,
            RequestWebPackageHash,
            LoadWebPackageManifest,
            Done,
        }

        private readonly DefaultWebFileSystem _fileSystem;
        private readonly string _packageVersion;
        private readonly int _timeout;
        private RequestWebPackageVersionOperation _requestWebPackageVersionOp;
        private RequestWebPackageHashOperation _requestWebPackageHashOp;
        private LoadWebPackageManifestOperation _loadWebPackageManifestOp;
        private ESteps _steps = ESteps.None;


        [AssetSystemPreserve]
        public DWFSLoadPackageManifestOperation(DefaultWebFileSystem fileSystem, string packageVersion, int timeout)
        {
            _fileSystem = fileSystem;
            _packageVersion = packageVersion;
            _timeout = timeout;
        }

        [AssetSystemPreserve]
        public override void InternalOnStart()
        {
            _steps = ESteps.CheckPackageVersion;
        }

        [AssetSystemPreserve]
        public override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
            {
                return;
            }

            if (_steps == ESteps.CheckPackageVersion)
            {
                if (string.IsNullOrEmpty(_packageVersion))
                {
                    _steps = ESteps.RequestWebPackageVersion;
                }
                else
                {
                    _steps = ESteps.RequestWebPackageHash;
                }
            }

            if (_steps == ESteps.RequestWebPackageVersion)
            {
                if (_requestWebPackageVersionOp == null)
                {
                    _requestWebPackageVersionOp = new RequestWebPackageVersionOperation(_fileSystem, _timeout, true);
                    OperationSystem.StartOperation(_fileSystem.PackageName, _requestWebPackageVersionOp);
                }

                Progress = _requestWebPackageVersionOp.Progress;
                if (_requestWebPackageVersionOp.IsDone == false)
                {
                    return;
                }

                if (_requestWebPackageVersionOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.RequestWebPackageHash;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _requestWebPackageVersionOp.Error;
                }
            }

            if (_steps == ESteps.RequestWebPackageHash)
            {
                if (_requestWebPackageHashOp == null)
                {
                    var packageVersion = string.IsNullOrEmpty(_packageVersion) ? _requestWebPackageVersionOp.PackageVersion : _packageVersion;
                    _requestWebPackageHashOp = new RequestWebPackageHashOperation(_fileSystem, packageVersion, _timeout, true);
                    OperationSystem.StartOperation(_fileSystem.PackageName, _requestWebPackageHashOp);
                }

                Progress = _requestWebPackageHashOp.Progress;
                if (_requestWebPackageHashOp.IsDone == false)
                {
                    return;
                }

                if (_requestWebPackageHashOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.LoadWebPackageManifest;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _requestWebPackageHashOp.Error;
                }
            }

            if (_steps == ESteps.LoadWebPackageManifest)
            {
                if (_loadWebPackageManifestOp == null)
                {
                    var packageVersion = string.IsNullOrEmpty(_packageVersion) ? _requestWebPackageVersionOp.PackageVersion : _packageVersion;
                    var packageHash = _requestWebPackageHashOp.PackageHash;
                    _loadWebPackageManifestOp = new LoadWebPackageManifestOperation(_fileSystem, packageVersion, packageHash, _timeout, true);
                    OperationSystem.StartOperation(_fileSystem.PackageName, _loadWebPackageManifestOp);
                }

                Progress = _loadWebPackageManifestOp.Progress;
                if (_loadWebPackageManifestOp.IsDone == false)
                {
                    return;
                }

                if (_loadWebPackageManifestOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Manifest = _loadWebPackageManifestOp.Manifest;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _loadWebPackageManifestOp.Error;
                }
            }
        }
    }
}
