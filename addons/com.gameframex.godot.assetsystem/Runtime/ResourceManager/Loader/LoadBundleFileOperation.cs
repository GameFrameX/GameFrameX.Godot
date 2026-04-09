using System;
using System.Collections;
using System.Collections.Generic;

namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    internal class LoadBundleFileOperation : AsyncOperationBase
    {
        [AssetSystemPreserve]
        private enum ESteps
        {
            None,
            LoadFile,
            Done,
        }

        private readonly ResourceManager _resourceManager;
        private readonly List<ProviderOperation> _providers = new(100);
        private readonly List<ProviderOperation> _removeList = new(100);
        private FSLoadBundleOperation _loadBundleOp;
        private ESteps _steps = ESteps.None;

        /// <summary>
        /// 资源包文件信息
        /// </summary>
        public BundleInfo BundleFileInfo { private set; get; }

        /// <summary>
        /// 是否已经销毁
        /// </summary>
        public bool IsDestroyed { private set; get; } = false;

        /// <summary>
        /// 引用计数
        /// </summary>
        public int RefCount { private set; get; } = 0;

        /// <summary>
        /// 下载进度
        /// </summary>
        public float DownloadProgress { set; get; } = 0;

        /// <summary>
        /// 下载大小
        /// </summary>
        public long DownloadedBytes { set; get; } = 0;

        /// <summary>
        /// 加载结果
        /// </summary>
        public object Result { set; get; }


        [AssetSystemPreserve]
        internal LoadBundleFileOperation(ResourceManager resourceManager, BundleInfo bundleInfo)
        {
            _resourceManager = resourceManager;
            BundleFileInfo = bundleInfo;
        }

        [AssetSystemPreserve]
        public override void InternalOnStart()
        {
            _steps = ESteps.LoadFile;
        }

        [AssetSystemPreserve]
        public override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
            {
                return;
            }

            if (_steps == ESteps.LoadFile)
            {
                if (_loadBundleOp == null)
                {
                    _loadBundleOp = BundleFileInfo.LoadBundleFile();
                }

                DownloadProgress = _loadBundleOp.DownloadProgress;
                DownloadedBytes = _loadBundleOp.DownloadedBytes;
                if (_loadBundleOp.IsDone == false)
                {
                    return;
                }

                if (_loadBundleOp.Status == EOperationStatus.Succeed)
                {
                    _steps = ESteps.Done;
                    Result = _loadBundleOp.Result;
                    Status = EOperationStatus.Succeed;
                }
                else
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _loadBundleOp.Error;
                }
            }
        }

        [AssetSystemPreserve]
        public override void InternalWaitForAsyncComplete()
        {
            while (true)
            {
                if (_loadBundleOp != null)
                {
                    _loadBundleOp.WaitForAsyncComplete();
                }

                if (ExecuteWhileDone())
                {
                    _steps = ESteps.Done;
                    break;
                }
            }
        }

        /// <summary>
        /// 引用（引用计数递加）
        /// </summary>
        [AssetSystemPreserve]
        public void Reference()
        {
            RefCount++;
        }

        /// <summary>
        /// 释放（引用计数递减）
        /// </summary>
        [AssetSystemPreserve]
        public void Release()
        {
            RefCount--;
        }

        /// <summary>
        /// 销毁
        /// </summary>
        [AssetSystemPreserve]
        public void DestroyLoader()
        {
            IsDestroyed = true;

            // Check fatal
            if (RefCount > 0)
            {
                throw new Exception($"Bundle file loader ref is not zero : {BundleFileInfo.Bundle.BundleName}");
            }

            if (IsDone == false)
            {
                throw new Exception($"Bundle file loader is not done : {BundleFileInfo.Bundle.BundleName}");
            }

            BundleFileInfo.UnloadBundleFile(Result);
        }

        /// <summary>
        /// 是否可以销毁
        /// </summary>
        [AssetSystemPreserve]
        public bool CanDestroyLoader()
        {
            if (IsDone == false)
            {
                return false;
            }

            return RefCount <= 0;
        }

        /// <summary>
        /// 添加附属的资源提供者
        /// </summary>
        [AssetSystemPreserve]
        public void AddProvider(ProviderOperation provider)
        {
            if (_providers.Contains(provider) == false)
            {
                _providers.Add(provider);
            }
        }

        /// <summary>
        /// 尝试销毁资源提供者
        /// </summary>
        [AssetSystemPreserve]
        public void TryDestroyProviders()
        {
            // 获取移除列表
            _removeList.Clear();
            foreach (var provider in _providers)
            {
                if (provider.CanDestroyProvider())
                {
                    _removeList.Add(provider);
                }
            }

            // 销毁资源提供者
            foreach (var provider in _removeList)
            {
                _providers.Remove(provider);
                provider.DestroyProvider();
            }

            // 移除资源提供者
            if (_removeList.Count > 0)
            {
                _resourceManager.RemoveBundleProviders(_removeList);
                _removeList.Clear();
            }
        }

        /// <summary>
        /// 终止下载任务
        /// </summary>
        [AssetSystemPreserve]
        public void AbortDownloadOperation()
        {
            if (_loadBundleOp != null)
            {
                _loadBundleOp.AbortDownloadOperation();
            }
        }
    }
}