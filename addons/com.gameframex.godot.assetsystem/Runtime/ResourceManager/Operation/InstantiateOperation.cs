using Godot;

namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    public sealed class InstantiateOperation : AsyncOperationBase
    {
        [AssetSystemPreserve]
        private enum ESteps
        {
            None,
            Clone,
            Done,
        }

        private readonly AssetHandle _handle;
        private readonly Node _parent;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 实例化的 Godot 节点
        /// </summary>
        public Node Result = null;


        [AssetSystemPreserve]
        internal InstantiateOperation(AssetHandle handle, Node parent)
        {
            _handle = handle;
            _parent = parent;
        }

        [AssetSystemPreserve]
        public override void InternalOnStart()
        {
            _steps = ESteps.Clone;
        }

        [AssetSystemPreserve]
        public override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
            {
                return;
            }

            if (_steps == ESteps.Clone)
            {
                if (_handle.IsValidWithWarning == false)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"{nameof(AssetHandle)} is invalid.";
                    return;
                }

                if (_handle.IsDone == false)
                {
                    return;
                }

                if (_handle.AssetObject == null)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = $"{nameof(AssetHandle.AssetObject)} is null.";
                    return;
                }

                // Migration note (scheme 2): instantiate by Godot runtime backend.
                Result = InstantiateInternal(_handle.AssetObject, _parent);

                _steps = ESteps.Done;
                Status = EOperationStatus.Succeed;
            }
        }

        [AssetSystemPreserve]
        public override void InternalWaitForAsyncComplete()
        {
            while (true)
            {
                // 等待句柄完成
                if (_handle != null)
                {
                    _handle.WaitForAsyncComplete();
                }

                if (ExecuteWhileDone())
                {
                    _steps = ESteps.Done;
                    break;
                }
            }
        }

        [AssetSystemPreserve]
        internal override void InternalOnAbort()
        {
            if (Result != null)
            {
                BundleAssetLoaderFactory.Backend.Destroy(Result);
                Result = null;
            }
        }

        /// <summary>
        /// 取消实例化对象操作
        /// </summary>
        [AssetSystemPreserve]
        public void Cancel()
        {
            SetAbort();
        }

        [AssetSystemPreserve]
        internal static Node InstantiateInternal(object assetObject, Node parent)
        {
            if (assetObject == null)
            {
                return null;
            }

            return BundleAssetLoaderFactory.Backend.Instantiate(assetObject, parent);
        }
    }
}
