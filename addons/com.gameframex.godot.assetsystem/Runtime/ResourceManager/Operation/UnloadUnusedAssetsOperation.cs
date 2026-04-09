using System.Collections;
using System.Collections.Generic;

namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    public sealed class UnloadUnusedAssetsOperation : AsyncOperationBase
    {
        [AssetSystemPreserve]
        private enum ESteps
        {
            None,
            UnloadUnused,
            Done,
        }

        private readonly ResourceManager _resManager;
        private ESteps _steps = ESteps.None;

        [AssetSystemPreserve]
        internal UnloadUnusedAssetsOperation(ResourceManager resourceManager)
        {
            _resManager = resourceManager;
        }

        [AssetSystemPreserve]
        public override void InternalOnStart()
        {
            _steps = ESteps.UnloadUnused;
        }

        [AssetSystemPreserve]
        public override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
            {
                return;
            }

            if (_steps == ESteps.UnloadUnused)
            {
                var loaderDic = _resManager._loaderDic;
                var removeList = new List<LoadBundleFileOperation>(loaderDic.Count);

                // 注意：优先销毁资源提供者
                foreach (var loader in loaderDic.Values)
                {
                    loader.TryDestroyProviders();
                }

                // 获取销毁列表
                foreach (var loader in loaderDic.Values)
                {
                    if (loader.CanDestroyLoader())
                    {
                        removeList.Add(loader);
                    }
                }

                // 销毁文件加载器
                foreach (var loader in removeList)
                {
                    var bundleName = loader.BundleFileInfo.Bundle.BundleName;
                    loader.DestroyLoader();
                    _resManager._loaderDic.Remove(bundleName);
                }

                // Godot runtime has no direct equivalent for Unity global unused-resource unload API.
                // Loader/provider destruction above is the authoritative cleanup path.

                _steps = ESteps.Done;
                Status = EOperationStatus.Succeed;
            }
        }

        [AssetSystemPreserve]
        public override void InternalWaitForAsyncComplete()
        {
            while (true)
            {
                if (ExecuteWhileDone())
                {
                    _steps = ESteps.Done;
                    break;
                }
            }
        }
    }
}
