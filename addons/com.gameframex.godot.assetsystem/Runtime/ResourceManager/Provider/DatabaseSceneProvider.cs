using System.Collections.Generic;
using System.IO;
using Godot;

namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    internal sealed class DatabaseSceneProvider : ProviderOperation, ISceneLoadController
    {
        public readonly SceneLoadParameters LoadSceneParams;
        private bool _suspendLoadMode;
        private bool _cancelLoadMode;
        private List<string> _scenePathCandidates;
        private int _scenePathIndex;
        private Node _pendingSceneNode;

        public SceneLoadMode SceneMode => LoadSceneParams.SceneMode;

        [AssetSystemPreserve]
        public DatabaseSceneProvider(ResourceManager manager, string providerGUID, AssetInfo assetInfo, SceneLoadParameters loadSceneParams, bool suspendLoad)
            : base(manager, providerGUID, assetInfo)
        {
            LoadSceneParams = loadSceneParams;
            SceneName = Path.GetFileNameWithoutExtension(assetInfo.AssetPath);
            _suspendLoadMode = suspendLoad;
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

            if (_steps == ESteps.Loading)
            {
                if (TryLoadSceneNode(out var loadedNode, out var loadError) == false)
                {
                    AssetSystemLogger.Error(loadError);
                    InvokeCompletion(loadError, EOperationStatus.Failed);
                    return;
                }

                // Migration note (scheme 2): suspend now pauses node attachment instead of Unity scene activation.
                if (_suspendLoadMode && IsWaitForAsyncComplete == false)
                {
                    _pendingSceneNode = loadedNode;
                    Progress = 0.9f;
                    _steps = ESteps.Checking;
                    return;
                }

                if (TryAttachSceneNode(loadedNode, out var attachError) == false)
                {
                    AssetSystemLogger.Error(attachError);
                    InvokeCompletion(attachError, EOperationStatus.Failed);
                    return;
                }

                _steps = ESteps.Checking;
            }

            if (_steps == ESteps.Checking)
            {
                if (_pendingSceneNode != null)
                {
                    if (_suspendLoadMode && IsWaitForAsyncComplete == false)
                    {
                        Progress = 0.9f;
                        return;
                    }

                    if (TryAttachSceneNode(_pendingSceneNode, out var attachError) == false)
                    {
                        AssetSystemLogger.Error(attachError);
                        InvokeCompletion(attachError, EOperationStatus.Failed);
                        return;
                    }

                    _pendingSceneNode = null;
                }

                if (SceneNode != null && GodotObject.IsInstanceValid(SceneNode))
                {
                    Progress = 1f;
                    InvokeCompletion(string.Empty, EOperationStatus.Succeed);
                }
                else
                {
                    var error = $"The loaded scene node is invalid : {MainAssetInfo.AssetPath}";
                    AssetSystemLogger.Error(error);
                    InvokeCompletion(error, EOperationStatus.Failed);
                }
            }
        }

        [AssetSystemPreserve]
        public void UnSuspendLoad()
        {
            if (IsDone == false)
            {
                _suspendLoadMode = false;
            }
        }

        [AssetSystemPreserve]
        public void CancelLoad()
        {
            if (IsDone == false)
            {
                _cancelLoadMode = true;
                _suspendLoadMode = false;
            }
        }

        private bool TryLoadSceneNode(out Node sceneNode, out string error)
        {
            while (_scenePathIndex < _scenePathCandidates.Count)
            {
                var scenePath = _scenePathCandidates[_scenePathIndex++];
                foreach (var candidate in EnumerateSceneCandidates(scenePath))
                {
                    if (ResourceLoader.Exists(candidate) == false)
                    {
                        continue;
                    }

                    var packedScene = ResourceLoader.Load<PackedScene>(candidate);
                    if (packedScene == null)
                    {
                        continue;
                    }

                    sceneNode = packedScene.Instantiate();
                    SceneName = Path.GetFileNameWithoutExtension(candidate);
                    error = string.Empty;
                    return true;
                }
            }

            sceneNode = null;
            error = $"Failed to load scene : {MainAssetInfo.AssetPath}";
            return false;
        }

        private bool TryAttachSceneNode(Node sceneNode, out string error)
        {
            var tree = Engine.GetMainLoop() as SceneTree;
            if (tree == null)
            {
                error = "SceneTree is not ready.";
                return false;
            }

            if (SceneMode == SceneLoadMode.Single)
            {
                var currentScene = tree.CurrentScene;
                if (currentScene != null && currentScene != sceneNode && GodotObject.IsInstanceValid(currentScene))
                {
                    currentScene.QueueFree();
                }
            }

            if (sceneNode.GetParent() == null)
            {
                tree.Root.AddChild(sceneNode);
            }

            if (SceneMode == SceneLoadMode.Single)
            {
                tree.CurrentScene = sceneNode;
            }

            SceneNode = sceneNode;
            SceneInfo = new AssetSceneInfo
            {
                Name = SceneName,
                IsLoaded = true,
                IsValidFlag = true
            };

            error = string.Empty;
            return true;
        }

        private static IEnumerable<string> EnumerateSceneCandidates(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                yield break;
            }

            if (path.EndsWith(".tscn", System.StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".scn", System.StringComparison.OrdinalIgnoreCase))
            {
                yield return path;
            }
            else
            {
                yield return $"{path}.tscn";
                yield return $"{path}.scn";
            }
        }

        private void TryRecycleLoadedScene()
        {
            if (_pendingSceneNode != null && GodotObject.IsInstanceValid(_pendingSceneNode))
            {
                _pendingSceneNode.QueueFree();
                _pendingSceneNode = null;
            }

            if (SceneNode != null && GodotObject.IsInstanceValid(SceneNode))
            {
                SceneNode.QueueFree();
                SceneNode = null;
                SceneInfo = default;
                ResourceMgr.UnloadSubScene(SceneName);
            }
        }
    }
}
