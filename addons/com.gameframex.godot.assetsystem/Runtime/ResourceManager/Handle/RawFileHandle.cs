using System;
using System.IO;
using System.Text;

namespace YooAsset
{
    [UnityEngine.Scripting.Preserve]
    public class RawFileHandle : HandleBase, IDisposable
    {
        private Action<RawFileHandle> _callback;

        [UnityEngine.Scripting.Preserve]
        internal RawFileHandle(ProviderOperation provider) : base(provider)
        {
        }

        [UnityEngine.Scripting.Preserve]
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
        [UnityEngine.Scripting.Preserve]
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
        [UnityEngine.Scripting.Preserve]
        public void Release()
        {
            ReleaseInternal();
        }

        /// <summary>
        /// 释放资源句柄
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public void Dispose()
        {
            ReleaseInternal();
        }


        /// <summary>
        /// 获取原生文件的二进制数据
        /// </summary>
        [UnityEngine.Scripting.Preserve]
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
        [UnityEngine.Scripting.Preserve]
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
        [UnityEngine.Scripting.Preserve]
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
        [UnityEngine.Scripting.Preserve]
        private RawBundle GetRawBundleObject()
        {
            if (IsValidWithWarning == false)
            {
                return null;
            }

            if (Provider.IsDone == false)
            {
                YooLogger.Warning("Raw file is still loading.");
                return null;
            }

            if (Provider.Status != EOperationStatus.Succeed)
            {
                YooLogger.Warning($"Raw file load failed : {Provider.Error}");
                return null;
            }

            if (Provider.RawBundleObject == null)
            {
                YooLogger.Warning("Raw file object is null.");
                return null;
            }

            return Provider.RawBundleObject;
        }
    }
}
