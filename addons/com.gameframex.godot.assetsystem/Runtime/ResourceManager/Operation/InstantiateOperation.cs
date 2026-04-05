using UnityEngine;

namespace YooAsset
{
    [UnityEngine.Scripting.Preserve]
    public sealed class InstantiateOperation : AsyncOperationBase
    {
        [UnityEngine.Scripting.Preserve]
        private enum ESteps
        {
            None,
            Clone,
            Done,
        }

        private readonly AssetHandle _handle;
        private readonly bool _setPositionAndRotation;
        private readonly Vector3 _position;
        private readonly Quaternion _rotation;
        private readonly Transform _parent;
        private readonly bool _worldPositionStays;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 实例化的游戏对象
        /// </summary>
        public GameObject Result = null;


        [UnityEngine.Scripting.Preserve]
        internal InstantiateOperation(AssetHandle handle, bool setPositionAndRotation, Vector3 position, Quaternion rotation, Transform parent, bool worldPositionStays)
        {
            _handle = handle;
            _setPositionAndRotation = setPositionAndRotation;
            _position = position;
            _rotation = rotation;
            _parent = parent;
            _worldPositionStays = worldPositionStays;
        }

        [UnityEngine.Scripting.Preserve]
        public override void InternalOnStart()
        {
            _steps = ESteps.Clone;
        }

        [UnityEngine.Scripting.Preserve]
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

                // 实例化游戏对象
                Result = InstantiateInternal(_handle.AssetObject, _setPositionAndRotation, _position, _rotation, _parent, _worldPositionStays);

                _steps = ESteps.Done;
                Status = EOperationStatus.Succeed;
            }
        }

        [UnityEngine.Scripting.Preserve]
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

        [UnityEngine.Scripting.Preserve]
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
        [UnityEngine.Scripting.Preserve]
        public void Cancel()
        {
            SetAbort();
        }

        [UnityEngine.Scripting.Preserve]
        internal static GameObject InstantiateInternal(UnityEngine.Object assetObject, bool setPositionAndRotation, Vector3 position, Quaternion rotation, Transform parent, bool worldPositionStays)
        {
            if (assetObject == null)
            {
                return null;
            }

            return BundleAssetLoaderFactory.Backend.Instantiate(assetObject, setPositionAndRotation, position, rotation, parent, worldPositionStays);
        }
    }
}
