using System.Collections;
using System.Collections.Generic;
namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    internal sealed class BundledAllAssetsProvider : ProviderOperation
    {
        private IBundleAssetLoader _bundleLoader;
        private IBundleAssetLoadRequest _cacheRequest;

        [AssetSystemPreserve]
        public BundledAllAssetsProvider(ResourceManager manager, string providerGUID, AssetInfo assetInfo) : base(manager, providerGUID, assetInfo)
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
                _steps = ESteps.Loading;
            }

            // 2. 加载资源对象
            if (_steps == ESteps.Loading)
            {
                if (IsWaitForAsyncComplete)
                {
                    AllAssetObjects = _bundleLoader.LoadAllAssets(MainAssetInfo.AssetType);
                }
                else
                {
                    _cacheRequest = _bundleLoader.LoadAllAssetsAsync(MainAssetInfo.AssetType);
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
                        AllAssetObjects = _cacheRequest.AllAssetObjects;
                    }
                    else
                    {
                        Progress = _cacheRequest.Progress;
                        if (_cacheRequest.IsDone == false)
                        {
                            return;
                        }

                        AllAssetObjects = _cacheRequest.AllAssetObjects;
                    }
                }

                AllAssetObjects = BundleAssetLoadUtility.FilterByType(AllAssetObjects, MainAssetInfo.AssetType);

                if (AllAssetObjects == null)
                {
                    string error;
                    if (MainAssetInfo.AssetType == null)
                    {
            error = $"Failed to load all assets : {MainAssetInfo.AssetPath} AssetType : null Bundle : {LoadBundleFileOp.BundleFileInfo.Bundle.BundleName}";
                    }
                    else
                    {
            error = $"Failed to load all assets : {MainAssetInfo.AssetPath} AssetType : {MainAssetInfo.AssetType} Bundle : {LoadBundleFileOp.BundleFileInfo.Bundle.BundleName}";
                    }

                    AssetSystemLogger.Error(error);
                    InvokeCompletion(error, EOperationStatus.Failed);
                }
                else if (AllAssetObjects.Length == 0)
                {
                    string error;
                    if (MainAssetInfo.AssetType == null)
                    {
            error = $"Failed to load all assets : {MainAssetInfo.AssetPath} AssetType : null Bundle : {LoadBundleFileOp.BundleFileInfo.Bundle.BundleName}";
                    }
                    else
                    {
            error = $"Failed to load all assets : {MainAssetInfo.AssetPath} AssetType : {MainAssetInfo.AssetType} Bundle : {LoadBundleFileOp.BundleFileInfo.Bundle.BundleName}";
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
    }
}
