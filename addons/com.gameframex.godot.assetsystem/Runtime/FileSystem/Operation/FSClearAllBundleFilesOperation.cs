namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    internal abstract class FSClearAllBundleFilesOperation : AsyncOperationBase
    {
    }

    [AssetSystemPreserve]
    internal sealed class FSClearAllBundleFilesCompleteOperation : FSClearAllBundleFilesOperation
    {
        [AssetSystemPreserve]
        internal FSClearAllBundleFilesCompleteOperation()
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