using GameFrameX.AssetSystem;

[AssetSystemPreserve]
internal partial class WXFSInitializeOperation : FSInitializeFileSystemOperation
{
    private readonly WechatFileSystem _fileSystem;

    [AssetSystemPreserve]
    public WXFSInitializeOperation(WechatFileSystem fileSystem)
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
