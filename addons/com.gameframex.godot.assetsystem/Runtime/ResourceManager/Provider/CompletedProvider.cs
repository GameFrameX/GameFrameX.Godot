namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    internal sealed class CompletedProvider : ProviderOperation
    {
        [AssetSystemPreserve]
        public CompletedProvider(ResourceManager manager, AssetInfo assetInfo) : base(manager, string.Empty, assetInfo)
        {
        }

        [AssetSystemPreserve]
        public override void InternalOnStart()
        {
            BeginLoadTimeRecord();
        }

        [AssetSystemPreserve]
        public override void InternalOnUpdate()
        {
        }

        [AssetSystemPreserve]
        public void SetCompleted(string error)
        {
            if (_steps == ESteps.None)
            {
                InvokeCompletion(error, EOperationStatus.Failed);
            }
        }
    }
}
