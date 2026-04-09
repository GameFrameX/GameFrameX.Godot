using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    internal class DCFSLoadLocalPackageVersionOperation : FSRequestPackageVersionOperation
    {
        [AssetSystemPreserve]
        private enum ESteps
        {
            None,
            GetPackageVersion,
            Done,
        }

        private readonly DefaultCacheFileSystem _fileSystem;
        private readonly bool _appendTimeTicks;
        private readonly int _timeout;
        private LoadCachePackageVersionOperation _loadCachePackageVersionOp;
        private ESteps _steps = ESteps.None;


        [AssetSystemPreserve]
        internal DCFSLoadLocalPackageVersionOperation(DefaultCacheFileSystem fileSystem, bool appendTimeTicks, int timeout)
        {
            _fileSystem = fileSystem;
            _appendTimeTicks = appendTimeTicks;
            _timeout = timeout;
        }

        [AssetSystemPreserve]
        public override void InternalOnStart()
        {
            _steps = ESteps.GetPackageVersion;
        }

        [AssetSystemPreserve]
        public override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
            {
                return;
            }

            if (_steps == ESteps.GetPackageVersion)
            {
                if (_loadCachePackageVersionOp == null)
                {
                    _loadCachePackageVersionOp = new LoadCachePackageVersionOperation(_fileSystem);
                    OperationSystem.StartOperation(_fileSystem.PackageName, _loadCachePackageVersionOp);
                }

                Progress = _loadCachePackageVersionOp.Progress;
                if (_loadCachePackageVersionOp.IsDone == false)
                {
                    return;
                }

                if (_loadCachePackageVersionOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    PackageVersion = _loadCachePackageVersionOp.PackageVersion;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _loadCachePackageVersionOp.Error;
                }
            }
        }
    }
}