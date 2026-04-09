namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    internal class DEFSLoadBundleOperation : FSLoadBundleOperation
    {
        private readonly DefaultEditorFileSystem _fileSystem;
        private readonly PackageBundle _bundle;

        [AssetSystemPreserve]
        internal DEFSLoadBundleOperation(DefaultEditorFileSystem fileSystem, PackageBundle bundle)
        {
            _fileSystem = fileSystem;
            _bundle = bundle;
        }

        [AssetSystemPreserve]
        public override void InternalOnStart()
        {
            DownloadProgress = 1f;
            DownloadedBytes = _bundle.FileSize;
            Status = EOperationStatus.Succeed;
        }

        [AssetSystemPreserve]
        public override void InternalOnUpdate()
        {
        }

        [AssetSystemPreserve]
        public override void InternalWaitForAsyncComplete()
        {
        }

        [AssetSystemPreserve]
        public override void AbortDownloadOperation()
        {
        }
    }
}