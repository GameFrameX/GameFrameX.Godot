namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    internal class DWFSInitializeOperation : FSInitializeFileSystemOperation
    {
        [AssetSystemPreserve]
        private enum ESteps
        {
            None,
            LoadCatalogFile,
            Done,
        }

        private readonly DefaultWebFileSystem _fileSystem;
        private LoadWebCatalogFileOperation _loadCatalogFileOp;
        private ESteps _steps = ESteps.None;


        [AssetSystemPreserve]
        public DWFSInitializeOperation(DefaultWebFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        [AssetSystemPreserve]
        public override void InternalOnStart()
        {
            _steps = ESteps.LoadCatalogFile;
        }

        [AssetSystemPreserve]
        public override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
            {
                return;
            }

            if (_steps == ESteps.LoadCatalogFile)
            {
                if (_loadCatalogFileOp == null)
                {
                    _loadCatalogFileOp = new LoadWebCatalogFileOperation(_fileSystem);
                    OperationSystem.StartOperation(_fileSystem.PackageName, _loadCatalogFileOp);
                }

                if (_loadCatalogFileOp.IsDone == false)
                {
                    return;
                }

                if (_loadCatalogFileOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _loadCatalogFileOp.Error;
                }
            }
        }
    }
}
