using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace YooAsset
{
    [UnityEngine.Scripting.Preserve]
    internal sealed class DatabaseSceneProvider : ProviderOperation, ISceneLoadController
    {
        public readonly LoadSceneParameters LoadSceneParams;
        private AsyncOperation _asyncOperation;
        private bool _suspendLoadMode;
        private bool _cancelLoadMode;
        private List<string> _scenePathCandidates;
        private int _scenePathIndex;

        /// <summary>
        /// 场景加载模式
        /// </summary>
        public LoadSceneMode SceneMode
        {
            get { return LoadSceneParams.loadSceneMode; }
        }

        [UnityEngine.Scripting.Preserve]
        public DatabaseSceneProvider(ResourceManager manager, string providerGUID, AssetInfo assetInfo, LoadSceneParameters loadSceneParams, bool suspendLoad) : base(manager, providerGUID, assetInfo)
        {
            LoadSceneParams = loadSceneParams;
            SceneName = Path.GetFileNameWithoutExtension(assetInfo.AssetPath);
            _suspendLoadMode = suspendLoad;
        }

        [UnityEngine.Scripting.Preserve]
        public override void InternalOnStart()
        {
            BeginLoadTimeRecord();
            DebugBeginRecording();
        }

        [UnityEngine.Scripting.Preserve]
        public override void InternalOnUpdate()
        {
            if (IsDone)
            {
                return;
            }

            if (_cancelLoadMode)
            {
                TryRecycleLoadedScene();
                InvokeCompletion($"Scene load canceled : {MainAssetInfo.AssetPath}", EOperationStatus.Failed);
                return;
            }

            if (_steps == ESteps.None)
            {
                _steps = ESteps.CheckBundle;
            }

            // 1. 检测资源包
            if (_steps == ESteps.CheckBundle)
            {
                if (LoadBundleFileOp.IsDone == false)
                {
                    return;
                }

                if (LoadBundleFileOp.Status != EOperationStatus.Succeed)
                {
                    InvokeCompletion(LoadBundleFileOp.Error, EOperationStatus.Failed);
                    return;
                }

                _scenePathCandidates = BundleAssetLoadUtility.GetPathCandidates(MainAssetInfo);
                _scenePathIndex = 0;
                if (_scenePathCandidates.Count == 0)
                {
                    InvokeCompletion($"Scene path is invalid : {MainAssetInfo.AssetPath}", EOperationStatus.Failed);
                    return;
                }

                _steps = ESteps.Loading;
            }

            // 2. 加载资源对象
            if (_steps == ESteps.Loading)
            {
                if (IsWaitForAsyncComplete)
                {
                    SceneObject = TryLoadSceneSync();
                    if (SceneObject.IsValid() == false)
                    {
                        var error = $"Failed to load scene : {MainAssetInfo.AssetPath}";
                        YooLogger.Error(error);
                        InvokeCompletion(error, EOperationStatus.Failed);
                    }
                    else
                    {
                        _steps = ESteps.Checking;
                    }
                }
                else
                {
                    if (TryLoadSceneAsyncCurrentPath())
                    {
                        _steps = ESteps.Checking;
                    }
                    else
                    {
                        var error = $"Failed to load scene : {MainAssetInfo.AssetPath}";
                        YooLogger.Error(error);
                        InvokeCompletion(error, EOperationStatus.Failed);
                    }
                }
            }

            // 3. 检测加载结果
            if (_steps == ESteps.Checking)
            {
                if (_asyncOperation != null)
                {
                    if (IsWaitForAsyncComplete)
                    {
                        // 场景加载无法强制异步转同步
                        YooLogger.Error("The scene is loading asyn !");
                    }
                    else
                    {
                        // 注意：在业务层中途可以取消挂起
                        if (_asyncOperation.allowSceneActivation == false)
                        {
                            if (_suspendLoadMode == false)
                            {
                                _asyncOperation.allowSceneActivation = true;
                            }
                        }

                        Progress = _asyncOperation.progress;
                        if (_asyncOperation.isDone == false)
                        {
                            return;
                        }

                        SceneObject = SceneManager.GetSceneByName(SceneName);
                    }
                }

                if (SceneObject.IsValid())
                {
                    InvokeCompletion(string.Empty, EOperationStatus.Succeed);
                }
                else
                {
                    var error = $"The loaded scene is invalid : {MainAssetInfo.AssetPath}";
                    YooLogger.Error(error);
                    InvokeCompletion(error, EOperationStatus.Failed);
                }
            }
        }

        /// <summary>
        /// 解除场景加载挂起操作
        /// </summary>
        [UnityEngine.Scripting.Preserve]
        public void UnSuspendLoad()
        {
            if (IsDone == false)
            {
                _suspendLoadMode = false;
            }
        }

        [UnityEngine.Scripting.Preserve]
        public void CancelLoad()
        {
            if (IsDone == false)
            {
                _cancelLoadMode = true;
                _suspendLoadMode = false;
            }
        }

        private Scene TryLoadSceneSync()
        {
            for (var i = 0; i < _scenePathCandidates.Count; i++)
            {
                var scenePath = _scenePathCandidates[i];
                var sceneObject = SceneManager.LoadScene(scenePath, LoadSceneParams);
                if (sceneObject.IsValid())
                {
                    SceneName = Path.GetFileNameWithoutExtension(scenePath);
                    return sceneObject;
                }
            }

            return new Scene();
        }

        private bool TryLoadSceneAsyncCurrentPath()
        {
            while (_scenePathIndex < _scenePathCandidates.Count)
            {
                var scenePath = _scenePathCandidates[_scenePathIndex];
                _asyncOperation = SceneManager.LoadSceneAsync(scenePath, LoadSceneParams);
                if (_asyncOperation != null)
                {
                    _asyncOperation.allowSceneActivation = !_suspendLoadMode;
                    _asyncOperation.priority = 100;
                    SceneName = Path.GetFileNameWithoutExtension(scenePath);
                    return true;
                }

                _scenePathIndex++;
            }

            return false;
        }

        private void TryRecycleLoadedScene()
        {
            if (SceneObject.IsValid() && SceneObject.isLoaded)
            {
                SceneManager.UnloadSceneAsync(SceneObject);
                ResourceMgr.UnloadSubScene(SceneName);
            }
        }
    }
}
