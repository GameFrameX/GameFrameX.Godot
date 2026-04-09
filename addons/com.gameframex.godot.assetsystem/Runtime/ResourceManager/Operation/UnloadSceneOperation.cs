using Godot;

namespace GameFrameX.AssetSystem
{
    /// <summary>
    /// 场景卸载异步操作类
    /// </summary>
    [AssetSystemPreserve]
    public sealed class UnloadSceneOperation : AsyncOperationBase
    {
        [AssetSystemPreserve]
        private enum ESteps
        {
            None,
            CheckError,
            PrepareDone,
            UnLoadScene,
            Done,
        }

        private ESteps _steps = ESteps.None;
        private readonly string _error;
        private readonly ProviderOperation _provider;

        [AssetSystemPreserve]
        internal UnloadSceneOperation(string error)
        {
            _error = error;
        }

        [AssetSystemPreserve]
        internal UnloadSceneOperation(ProviderOperation provider)
        {
            _error = null;
            _provider = provider;

            if (provider is ISceneLoadController sceneLoadController)
            {
                sceneLoadController.UnSuspendLoad();
            }
            else
            {
                throw new System.NotImplementedException();
            }
        }

        [AssetSystemPreserve]
        public override void InternalOnStart()
        {
            _steps = ESteps.CheckError;
        }

        [AssetSystemPreserve]
        public override void InternalOnUpdate()
        {
            if (_steps == ESteps.None || _steps == ESteps.Done)
            {
                return;
            }

            if (_steps == ESteps.CheckError)
            {
                if (string.IsNullOrEmpty(_error) == false)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = _error;
                    return;
                }

                _steps = ESteps.PrepareDone;
            }

            if (_steps == ESteps.PrepareDone)
            {
                if (_provider.IsDone == false)
                {
                    return;
                }

                if (_provider.SceneNode == null || GodotObject.IsInstanceValid(_provider.SceneNode) == false)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = "Scene node is invalid !";
                    return;
                }

                _steps = ESteps.UnLoadScene;
            }

            if (_steps == ESteps.UnLoadScene)
            {
                var sceneNode = _provider.SceneNode;
                if (sceneNode == null || GodotObject.IsInstanceValid(sceneNode) == false)
                {
                    _steps = ESteps.Done;
                    Status = EOperationStatus.Failed;
                    Error = "Scene node is invalid !";
                    return;
                }

                // Migration note (scheme 2): unload scene by Godot node lifecycle.
                sceneNode.QueueFree();
                _provider.SceneNode = null;
                _provider.SceneInfo = default;
                _provider.ResourceMgr.UnloadSubScene(_provider.SceneName);
                Progress = 1f;

                _provider.ResourceMgr.TryUnloadUnusedAsset(_provider.MainAssetInfo);
                _steps = ESteps.Done;
                Status = EOperationStatus.Succeed;
            }
        }
    }
}
