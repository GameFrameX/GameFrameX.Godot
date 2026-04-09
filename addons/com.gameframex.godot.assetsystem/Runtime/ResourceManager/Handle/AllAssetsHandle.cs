using System;
using System.Collections.Generic;

namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    public sealed class AllAssetsHandle : HandleBase, IDisposable
    {
        private Action<AllAssetsHandle> _callback;

        [AssetSystemPreserve]
        internal AllAssetsHandle(ProviderOperation provider) : base(provider)
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
        public event Action<AllAssetsHandle> Completed
        {
            add
            {
                if (IsValidWithWarning == false)
                {
                    throw new Exception($"{nameof(AllAssetsHandle)} is invalid");
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
                    throw new Exception($"{nameof(AllAssetsHandle)} is invalid");
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
        /// 子资源对象集合
        /// </summary>
        public IReadOnlyList<object> AllAssetObjects
        {
            get
            {
                if (IsValidWithWarning == false)
                {
                    return null;
                }

                return Provider.AllAssetObjects;
            }
        }
    }
}
