namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    internal class DEFSInitializeOperation : FSInitializeFileSystemOperation
    {
        private readonly DefaultEditorFileSystem _fileSytem;

        [AssetSystemPreserve]
        internal DEFSInitializeOperation(DefaultEditorFileSystem fileSystem)
        {
            _fileSytem = fileSystem;
        }

        [AssetSystemPreserve]
        public override void InternalOnStart()
        {
            Status = EOperationStatus.Succeed;
        }

        [AssetSystemPreserve]
        public override void InternalOnUpdate()
        {
        }
    }
}