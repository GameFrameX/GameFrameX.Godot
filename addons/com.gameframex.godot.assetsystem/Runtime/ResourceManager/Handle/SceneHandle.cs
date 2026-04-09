using System;
using Godot;
namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    public class SceneHandle : HandleBase
    {
        private System.Action<SceneHandle> _callback;
        internal string PackageName { set; get; }

        [AssetSystemPreserve]
        internal SceneHandle(ProviderOperation provider) : base(provider)
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
        public event System.Action<SceneHandle> Completed
        {
            add
            {
                if (IsValidWithWarning == false)
                {
                    throw new System.Exception($"{nameof(SceneHandle)} is invalid !");
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
                    throw new System.Exception($"{nameof(SceneHandle)} is invalid !");
                }

                _callback -= value;
            }
        }

        /// <summary>
        /// 等待异步执行完毕
        /// </summary>
        [AssetSystemPreserve]
        internal void WaitForAsyncComplete()
        {
            if (IsValidWithWarning == false)
            {
                return;
            }

            Provider.WaitForAsyncComplete();
        }

        /// <summary>
        /// 场景名称
        /// </summary>
        public string SceneName
        {
            get
            {
                if (IsValidWithWarning == false)
                {
                    return string.Empty;
                }

                return Provider.SceneName;
            }
        }

        /// <summary>
        /// 场景对象（兼容字段）
        /// </summary>
        public AssetSceneInfo SceneInfo
        {
            get
            {
                if (IsValidWithWarning == false)
                {
                    return default;
                }

                return Provider.SceneInfo;
            }
        }

        /// <summary>
        /// 方案2迁移备注：实际运行的 Godot 场景节点。
        /// </summary>
        public Node SceneNode
        {
            get
            {
                if (IsValidWithWarning == false)
                {
                    return null;
                }

                return Provider.SceneNode;
            }
        }

        /// <summary>
        /// 激活场景（方案2：直接切换 Godot SceneTree.CurrentScene）
        /// </summary>
        [AssetSystemPreserve]
        public bool ActivateScene()
        {
            if (IsValidWithWarning == false)
            {
                return false;
            }

            if (SceneNode != null && GodotObject.IsInstanceValid(SceneNode))
            {
                var tree = SceneNode.GetTree() ?? (Engine.GetMainLoop() as SceneTree);
                if (tree == null)
                {
                    return false;
                }

                if (SceneNode.GetParent() == null)
                {
                    tree.Root.AddChild(SceneNode);
                }

                tree.CurrentScene = SceneNode;
                return true;
            }
            else
            {
                AssetSystemLogger.Warning($"Scene node is invalid or not loaded : {SceneName}");
                return false;
            }
        }

        /// <summary>
        /// 解除场景加载挂起操作
        /// </summary>
        [AssetSystemPreserve]
        public bool UnSuspend()
        {
            if (IsValidWithWarning == false)
            {
                return false;
            }

            if (Provider is ISceneLoadController sceneLoadController)
            {
                sceneLoadController.UnSuspendLoad();
            }
            else
            {
                throw new System.NotImplementedException();
            }

            return true;
        }

        /// <summary>
        /// 是否为主场景
        /// </summary>
        [AssetSystemPreserve]
        public bool IsMainScene()
        {
            if (IsValidWithWarning == false)
            {
                return false;
            }

            if (Provider is ISceneLoadController sceneLoadController)
            {
                return sceneLoadController.SceneMode == SceneLoadMode.Single;
            }
            else
            {
                throw new System.NotImplementedException();
            }
        }

        [AssetSystemPreserve]
        public bool Cancel()
        {
            if (IsValidWithWarning == false)
            {
                return false;
            }

            if (Provider is ISceneLoadController sceneLoadController)
            {
                sceneLoadController.CancelLoad();
                return true;
            }

            throw new System.NotImplementedException();
        }

        /// <summary>
        /// 异步卸载子场景
        /// </summary>
        [AssetSystemPreserve]
        public UnloadSceneOperation UnloadAsync()
        {
            var packageName = GetAssetInfo().PackageName;

            // 如果句柄无效
            if (IsValidWithWarning == false)
            {
                var error = $"{nameof(SceneHandle)} is invalid.";
                var operation = new UnloadSceneOperation(error);
                OperationSystem.StartOperation(packageName, operation);
                return operation;
            }

            // 如果是主场景
            if (IsMainScene())
            {
                var error = $"Cannot unload main scene. Use {nameof(AssetSystem.LoadSceneAsync)} method to change the main scene !";
                AssetSystemLogger.Error(error);
                var operation = new UnloadSceneOperation(error);
                OperationSystem.StartOperation(packageName, operation);
                return operation;
            }

            // 卸载子场景
            // 注意：如果场景正在加载过程，必须等待加载完成后才可以卸载该场景。
            {
                var operation = new UnloadSceneOperation(Provider);
                OperationSystem.StartOperation(packageName, operation);
                return operation;
            }
        }
    }
}
