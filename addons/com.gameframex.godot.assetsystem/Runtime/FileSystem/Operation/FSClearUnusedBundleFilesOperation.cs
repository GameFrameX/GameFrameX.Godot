namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    internal abstract class FSClearUnusedBundleFilesOperation : AsyncOperationBase
    {
    }

    [AssetSystemPreserve]
    internal sealed class FSClearUnusedBundleFilesCompleteOperation : FSClearUnusedBundleFilesOperation
    {
        [AssetSystemPreserve]
        internal FSClearUnusedBundleFilesCompleteOperation()
        {
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