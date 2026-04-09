using GameFrameX.AssetSystem;

[AssetSystemPreserve]
internal partial class BGFSInitializeOperation : FSInitializeFileSystemOperation
{
    private readonly ByteGameFileSystem _fileSystem;

    [AssetSystemPreserve]
    public BGFSInitializeOperation(ByteGameFileSystem fileSystem)
    {
        _fileSystem = fileSystem;
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
