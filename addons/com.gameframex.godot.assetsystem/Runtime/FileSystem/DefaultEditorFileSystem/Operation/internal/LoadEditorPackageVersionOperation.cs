using System.IO;

namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    internal class LoadEditorPackageVersionOperation : AsyncOperationBase
    {
        [AssetSystemPreserve]
        private enum ESteps
        {
            None,
            LoadVersion,
            Done,
        }

        private readonly DefaultEditorFileSystem _fileSystem;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 包裹版本
        /// </summary>
        public string PackageVersion { private set; get; }


        [AssetSystemPreserve]
        internal LoadEditorPackageVersionOperation(DefaultEditorFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        [AssetSystemPreserve]
        public override void InternalOnStart()
        {
            _steps = ESteps.LoadVersion;
        }

        [AssetSystemPreserve]
        public override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
            {
                return;
            }

            if (_steps == ESteps.LoadVersion)
            {
                var versionFilePath = _fileSystem.GetEditorPackageVersionFilePath();
                if (File.Exists(versionFilePath))
                {
                    _steps = ESteps.Done;
                    PackageVersion = FileUtility.ReadAllText(versionFilePath);
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"Can not found simulation package version file : {versionFilePath}";
                }
            }
        }
    }
}