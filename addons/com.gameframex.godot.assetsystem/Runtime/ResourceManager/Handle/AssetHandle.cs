using System;
using System.Collections.Generic;
using Godot;
namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    public sealed class AssetHandle : HandleBase, IDisposable
    {
        private Action<AssetHandle> _callback;

        [AssetSystemPreserve]
        internal AssetHandle(ProviderOperation provider) : base(provider)
        {
        }

        [AssetSystemPreserve]
        internal override void InvokeCallback()
        {
            _callback?.Invoke(this);
        }

        /// <summary>
        /// 完成委托
        /// </summary>
        public event Action<AssetHandle> Completed
        {
            add
            {
                if (IsValidWithWarning == false)
                {
                    throw new Exception($"{nameof(AssetHandle)} is invalid");
                }

                if (Provider.IsDone)
                {
                    value.Invoke(this);
                }
                else
                {
                    _callback += value;
                }
            }
            remove
            {
                if (IsValidWithWarning == false)
                {
                    throw new Exception($"{nameof(AssetHandle)} is invalid");
                }

                _callback -= value;
            }
        }

        /// <summary>
        /// 等待异步执行完毕
        /// </summary>
        [AssetSystemPreserve]
        public void WaitForAsyncComplete()
        {
            if (IsValidWithWarning == false)
            {
                return;
            }

            Provider.WaitForAsyncComplete();
        }

        /// <summary>
        /// 释放资源句柄
        /// </summary>
        [AssetSystemPreserve]
        public void Release()
        {
            ReleaseInternal();
        }

        /// <summary>
        /// 释放资源句柄
        /// </summary>
        [AssetSystemPreserve]
        public void Dispose()
        {
            ReleaseInternal();
        }


        /// <summary>
        /// 资源对象
        /// </summary>
        public object AssetObject
        {
            get
            {
                if (IsValidWithWarning == false)
                {
                    return null;
                }

                return Provider.AssetObject;
            }
        }

        /// <summary>
        /// 获取资源对象
        /// </summary>
        /// <typeparam name="TAsset">资源类型</typeparam>
        [AssetSystemPreserve]
        public TAsset GetAssetObject<TAsset>() where TAsset : class
        {
            if (IsValidWithWarning == false)
            {
                return null;
            }

            return Provider.AssetObject as TAsset;
        }

        /// <summary>
        /// 同步实例化 Godot 节点（方案2：直接走 Godot API）。
        /// </summary>
        [AssetSystemPreserve]
        public Node InstantiateSync()
        {
            return InstantiateSyncInternal(null);
        }

        [AssetSystemPreserve]
        public Node InstantiateSync(Node parent)
        {
            return InstantiateSyncInternal(parent);
        }

        /// <summary>
        /// 异步实例化 Godot 节点（方案2：直接走 Godot API）。
        /// </summary>
        [AssetSystemPreserve]
        public InstantiateOperation InstantiateAsync()
        {
            return InstantiateAsyncInternal(null);
        }

        [AssetSystemPreserve]
        public InstantiateOperation InstantiateAsync(Node parent)
        {
            return InstantiateAsyncInternal(parent);
        }

        [AssetSystemPreserve]
        private Node InstantiateSyncInternal(Node parent)
        {
            if (IsValidWithWarning == false)
            {
                return null;
            }

            if (Provider.AssetObject == null)
            {
                return null;
            }

            return InstantiateOperation.InstantiateInternal(Provider.AssetObject, parent);
        }

        [AssetSystemPreserve]
        private InstantiateOperation InstantiateAsyncInternal(Node parent)
        {
            var packageName = GetAssetInfo().PackageName;
            var operation = new InstantiateOperation(this, parent);
            OperationSystem.StartOperation(packageName, operation);
            return operation;
        }
    }
}
