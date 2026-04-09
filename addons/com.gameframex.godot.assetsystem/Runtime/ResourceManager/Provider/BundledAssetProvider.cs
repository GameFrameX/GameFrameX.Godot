using System.Collections;
using System.Collections.Generic;
namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    internal sealed class BundledAssetProvider : ProviderOperation
    {
        private IBundleAssetLoader _bundleLoader;
        private IBundleAssetLoadRequest _cacheRequest;
        private List<string> _assetPathCandidates;
        private int _assetPathIndex;

        [AssetSystemPreserve]
        public BundledAssetProvider(ResourceManager manager, string providerGUID, AssetInfo assetInfo) : base(manager, providerGUID, assetInfo)
        {
        }

        [AssetSystemPreserve]
        public override void InternalOnStart()
        {
            BeginLoadTimeRecord();
            DebugBeginRecording();
        }

        [AssetSystemPreserve]
        public override void InternalOnUpdate()
        {
            if (IsDone)
            {
                return;
            }

            if (_steps == ESteps.None)
            {
                _steps = ESteps.CheckBundle;
            }

            // 1. 检测资源包
            if (_steps == ESteps.CheckBundle)
            {
                if (LoadDependBundleFileOp.IsDone == false)
                {
                    return;
                }

                if (LoadBundleFileOp.IsDone == false)
                {
                    return;
                }

                if (LoadDependBundleFileOp.Status != EOperationStatus.Succeed)
                {
                    InvokeCompletion(LoadDependBundleFileOp.Error, EOperationStatus.Failed);
                    return;
                }

                if (LoadBundleFileOp.Status != EOperationStatus.Succeed)
                {
                    InvokeCompletion(LoadBundleFileOp.Error, EOperationStatus.Failed);
                    return;
                }

                if (LoadBundleFileOp.Result == null)
                {
                    ProcessFatalEvent();
                    return;
                }

                if (BundleAssetLoaderFactory.TryCreate(LoadBundleFileOp.Result, out var bundleLoader, out var errorMessage) == false)
                {
                    InvokeCompletion(errorMessage, EOperationStatus.Failed);
                    return;
                }

                _bundleLoader = bundleLoader;
                _assetPathCandidates = BundleAssetLoadUtility.GetPathCandidates(MainAssetInfo);
                _assetPathIndex = 0;
                if (_assetPathCandidates.Count == 0)
                {
                    InvokeCompletion($"Asset path is invalid : {MainAssetInfo.AssetPath}", EOperationStatus.Failed);
                    return;
                }

                _steps = ESteps.Loading;
            }

            // 2. 加载资源对象
            if (_steps == ESteps.Loading)
            {
                if (IsWaitForAsyncComplete)
                {
                    AssetObject = TryLoadAssetSync();
                }
                else
                {
                    _cacheRequest = _bundleLoader.LoadAssetAsync(_assetPathCandidates[_assetPathIndex], MainAssetInfo.AssetType);
                }

                _steps = ESteps.Checking;
            }

            // 3. 检测加载结果
            if (_steps == ESteps.Checking)
            {
                if (_cacheRequest != null)
                {
                    if (IsWaitForAsyncComplete)
                    {
                        // 强制挂起主线程（注意：该操作会很耗时）
                AssetSystemLogger.Warning("Suspend the main thread to load asset synchronously.");
                        AssetObject = _cacheRequest.AssetObject;
                    }
                    else
                    {
                        Progress = _cacheRequest.Progress;
                        if (_cacheRequest.IsDone == false)
                        {
                            return;
                        }

                        AssetObject = _cacheRequest.AssetObject;
                    }
                }

                if (BundleAssetLoadUtility.IsTypeMatch(AssetObject, MainAssetInfo.AssetType) == false)
                {
                    if (IsWaitForAsyncComplete == false && TryLoadNextAssetPathAsync())
                    {
                        return;
                    }

                    AssetObject = null;
                }

                if (AssetObject == null)
                {
                    string error;
                    if (MainAssetInfo.AssetType == null)
                    {
                error = $"Failed to load asset : {MainAssetInfo.AssetPath} AssetType : null Bundle : {LoadBundleFileOp.BundleFileInfo.Bundle.BundleName}";
                    }
                    else
                    {
                error = $"Failed to load asset : {MainAssetInfo.AssetPath} AssetType : {MainAssetInfo.AssetType} Bundle : {LoadBundleFileOp.BundleFileInfo.Bundle.BundleName}";
                    }

                    AssetSystemLogger.Error(error);
                    InvokeCompletion(error, EOperationStatus.Failed);
                }
                else
                {
                    InvokeCompletion(string.Empty, EOperationStatus.Succeed);
                }
            }
        }

        private object TryLoadAssetSync()
        {
            for (var i = 0; i < _assetPathCandidates.Count; i++)
            {
                var assetObject = _bundleLoader.LoadAsset(_assetPathCandidates[i], MainAssetInfo.AssetType);
                if (BundleAssetLoadUtility.IsTypeMatch(assetObject, MainAssetInfo.AssetType))
                {
                    return assetObject;
                }
            }

            return null;
        }

        private bool TryLoadNextAssetPathAsync()
        {
            _assetPathIndex++;
            if (_assetPathIndex >= _assetPathCandidates.Count)
            {
                return false;
            }

            _cacheRequest = _bundleLoader.LoadAssetAsync(_assetPathCandidates[_assetPathIndex], MainAssetInfo.AssetType);
            return true;
        }
    }
}
