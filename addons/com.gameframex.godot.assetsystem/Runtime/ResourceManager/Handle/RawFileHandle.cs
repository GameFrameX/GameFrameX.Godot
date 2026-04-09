using System;
using System.IO;
using System.Text;

namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    public class RawFileHandle : HandleBase, IDisposable
    {
        private Action<RawFileHandle> _callback;

        [AssetSystemPreserve]
        internal RawFileHandle(ProviderOperation provider) : base(provider)
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
        public event Action<RawFileHandle> Completed
        {
            add
            {
                if (IsValidWithWarning == false)
                {
                    throw new Exception($"{nameof(RawFileHandle)} is invalid");
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
                    throw new Exception($"{nameof(RawFileHandle)} is invalid");
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
        /// 获取原生文件的二进制数据
        /// </summary>
        [AssetSystemPreserve]
        public byte[] GetRawFileData()
        {
            var rawBundle = GetRawBundleObject();
            if (rawBundle == null)
            {
                return null;
            }

            return rawBundle.ReadFileData();
        }

        /// <summary>
        /// 获取原生文件的文本数据
        /// </summary>
        [AssetSystemPreserve]
        public string GetRawFileText()
        {
            var rawBundle = GetRawBundleObject();
            if (rawBundle == null)
            {
                return null;
            }

            return rawBundle.ReadFileText();
        }

        /// <summary>
        /// 获取原生文件的路径
        /// </summary>
        [AssetSystemPreserve]
        public string GetRawFilePath()
        {
            var rawBundle = GetRawBundleObject();
            if (rawBundle == null)
            {
                return string.Empty;
            }

            return rawBundle.GetFilePath();
        }

        /// <summary>
        /// 获取已完成加载的原生文件对象
        /// </summary>
        [AssetSystemPreserve]
        private RawBundle GetRawBundleObject()
        {
            if (IsValidWithWarning == false)
            {
                return null;
            }

            if (Provider.IsDone == false)
            {
                AssetSystemLogger.Warning("Raw file is still loading.");
                return null;
            }

            if (Provider.Status != EOperationStatus.Succeed)
            {
                AssetSystemLogger.Warning($"Raw file load failed : {Provider.Error}");
                return null;
            }

            if (Provider.RawBundleObject == null)
            {
                AssetSystemLogger.Warning("Raw file object is null.");
                return null;
            }

            return Provider.RawBundleObject;
        }
    }
}
