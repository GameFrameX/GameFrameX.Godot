// ==========================================================================================
//  GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//  GameFrameX organization and its derivative projects' copyrights, trademarks, patents, and related rights
//  均受中华人民共和国及相关国际法律法规保护。
//  are protected by the laws of the People's Republic of China and relevant international regulations.
//
//  使用本项目须严格遵守相应法律法规及开源许可证之规定。
//  Usage of this project must strictly comply with applicable laws, regulations, and open-source licenses.
//
//  本项目采用 MIT 许可证与 Apache License 2.0 双许可证分发，
//  This project is dual-licensed under the MIT License and Apache License 2.0,
//  完整许可证文本请参见源代码根目录下的 LICENSE 文件。
//  please refer to the LICENSE file in the root directory of the source code for the full license text.
//
//  禁止利用本项目实施任何危害国家安全、破坏社会秩序、
//  It is prohibited to use this project to engage in any activities that endanger national security, disrupt social order,
//  侵犯他人合法权益等法律法规所禁止的行为！
//  or infringe upon the legitimate rights and interests of others, as prohibited by laws and regulations!
//  因基于本项目二次开发所产生的一切法律纠纷与责任，
//  Any legal disputes or liabilities arising from secondary development based on this project
//  本项目组织与贡献者概不承担。
//  shall be borne solely by the developer; the project organization and contributors assume no responsibility.
//
//  GitHub 仓库：https://github.com/GameFrameX
//  GitHub Repository: https://github.com/GameFrameX
//  Gitee  仓库：https://gitee.com/GameFrameX
//  Gitee Repository:  https://gitee.com/GameFrameX
//  官方文档：https://gameframex.doc.alianblank.com/
//  Official Documentation: https://gameframex.doc.alianblank.com/
// ==========================================================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameFrameX.Asset.Runtime;
using GameFrameX.AssetSystem;
using GameFrameX.Event.Runtime;
using GameFrameX.Runtime;
using Godot;

namespace GameFrameX.Scene.Runtime
{
    /// <summary>
    /// 场景组件。
    /// </summary>
    [GlobalClass]
    public sealed partial class SceneComponent : GameFrameworkComponent
    {
        /// <summary>
        /// 激活场景重试的最大帧数（对应 Unity 版协程等待 60 帧）。
        /// </summary>
        private const int ActivateSceneRetryMaxFrames = 60;

        private IGameSceneManager _gameSceneManager = null;
        private IAssetManager _assetManager = null;
        private EventComponent m_EventComponent = null;

        private readonly SortedDictionary<string, int> m_SceneOrder = new SortedDictionary<string, int>(StringComparer.Ordinal);

        private Camera3D m_MainCamera = null;
        private Camera2D m_MainCamera2D = null;
        private string m_PendingActivateSceneAssetName = null;
        private int m_PendingActivateRetryCount = 0;

        [Export] private bool m_EnableLoadSceneUpdateEvent = true;

        [Export] private bool m_EnableLoadSceneDependencyAssetEvent = true;

        /// <summary>
        /// 获取当前场景主摄像机（3D）。
        /// </summary>
        public Camera3D MainCamera
        {
            get { return m_MainCamera; }
        }

        /// <summary>
        /// 获取当前场景主摄像机（2D）。
        /// </summary>
        public Camera2D MainCamera2D
        {
            get { return m_MainCamera2D; }
        }

        /// <summary>
        /// 游戏框架组件初始化。
        /// </summary>
        public override void _Ready()
        {
            ImplementationComponentType = Utility.Assembly.GetType(componentType);
            InterfaceComponentType = typeof(IGameSceneManager);
            base._Ready();
            _gameSceneManager = GameFrameworkEntry.GetModule<IGameSceneManager>();
            if (_gameSceneManager == null)
            {
                Log.Fatal("Scene manager is invalid.");
                return;
            }

            _gameSceneManager.LoadSceneSuccess += OnLoadGameSceneSuccess;
            _gameSceneManager.LoadSceneFailure += OnLoadGameSceneFailure;

            if (m_EnableLoadSceneUpdateEvent)
            {
                _gameSceneManager.LoadSceneUpdate += OnLoadGameSceneUpdate;
            }

            _gameSceneManager.UnloadSceneSuccess += OnUnloadGameSceneSuccess;
            _gameSceneManager.UnloadSceneFailure += OnUnloadGameSceneFailure;

            // 镜像 Unity 侧 Start 生命周期：初始化推迟到节点进入场景树后执行。
            CallDeferred(nameof(StartInternal));
        }

        private void StartInternal()
        {
            BaseComponent baseComponent = GameEntry.GetComponent<BaseComponent>();
            if (baseComponent == null)
            {
                Log.Fatal("Base component is invalid.");
                return;
            }

            m_EventComponent = GameEntry.GetComponent<EventComponent>();
            if (m_EventComponent == null)
            {
                Log.Fatal("Event component is invalid.");
                return;
            }

            _assetManager = GameFrameworkEntry.GetModule<IAssetManager>();
            if (_assetManager == null)
            {
                Log.Fatal("Asset Manager is invalid.");
                return;
            }

            _gameSceneManager.SetResourceManager(_assetManager);
        }

        /// <summary>
        /// 获取场景名称。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <returns>场景名称。</returns>
        public static string GetSceneName(string sceneAssetName)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                Log.Error("Scene asset name is invalid.");
                return null;
            }

            int sceneNamePosition = sceneAssetName.LastIndexOf('/');
            if (sceneNamePosition + 1 >= sceneAssetName.Length)
            {
                Log.Error("Scene asset name '{0}' is invalid.", sceneAssetName);
                return null;
            }

            string sceneName = sceneAssetName.Substring(sceneNamePosition + 1);
            sceneNamePosition = sceneName.LastIndexOf(".tscn");
            if (sceneNamePosition > 0)
            {
                sceneName = sceneName.Substring(0, sceneNamePosition);
            }

            return sceneName;
        }

        /// <summary>
        /// 获取场景是否已加载。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <returns>场景是否已加载。</returns>
        public bool SceneIsLoaded(string sceneAssetName)
        {
            return _gameSceneManager.SceneIsLoaded(sceneAssetName);
        }

        /// <summary>
        /// 获取已加载场景的资源名称。
        /// </summary>
        /// <returns>已加载场景的资源名称。</returns>
        public string[] GetLoadedSceneAssetNames()
        {
            return _gameSceneManager.GetLoadedSceneAssetNames();
        }

        /// <summary>
        /// 获取已加载场景的资源名称。
        /// </summary>
        /// <param name="results">已加载场景的资源名称。</param>
        public void GetLoadedSceneAssetNames(List<string> results)
        {
            _gameSceneManager.GetLoadedSceneAssetNames(results);
        }

        /// <summary>
        /// 获取场景是否正在加载。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <returns>场景是否正在加载。</returns>
        public bool SceneIsLoading(string sceneAssetName)
        {
            return _gameSceneManager.SceneIsLoading(sceneAssetName);
        }

        /// <summary>
        /// 获取正在加载场景的资源名称。
        /// </summary>
        /// <returns>正在加载场景的资源名称。</returns>
        public string[] GetLoadingSceneAssetNames()
        {
            return _gameSceneManager.GetLoadingSceneAssetNames();
        }

        /// <summary>
        /// 获取正在加载场景的资源名称。
        /// </summary>
        /// <param name="results">正在加载场景的资源名称。</param>
        public void GetLoadingSceneAssetNames(List<string> results)
        {
            _gameSceneManager.GetLoadingSceneAssetNames(results);
        }

        /// <summary>
        /// 获取场景是否正在卸载。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <returns>场景是否正在卸载。</returns>
        public bool SceneIsUnloading(string sceneAssetName)
        {
            return _gameSceneManager.SceneIsUnloading(sceneAssetName);
        }

        /// <summary>
        /// 获取正在卸载场景的资源名称。
        /// </summary>
        /// <returns>正在卸载场景的资源名称。</returns>
        public string[] GetUnloadingSceneAssetNames()
        {
            return _gameSceneManager.GetUnloadingSceneAssetNames();
        }

        /// <summary>
        /// 获取正在卸载场景的资源名称。
        /// </summary>
        /// <param name="results">正在卸载场景的资源名称。</param>
        public void GetUnloadingSceneAssetNames(List<string> results)
        {
            _gameSceneManager.GetUnloadingSceneAssetNames(results);
        }

        /// <summary>
        /// 检查场景资源是否存在。
        /// </summary>
        /// <param name="sceneAssetName">要检查场景资源的名称。</param>
        /// <returns>场景资源是否存在。</returns>
        public bool HasScene(string sceneAssetName)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                Log.Error("Scene asset name is invalid.");
                return false;
            }

            if (!sceneAssetName.StartsWith("res://", StringComparison.Ordinal) ||
                !sceneAssetName.EndsWith(".tscn", StringComparison.Ordinal))
            {
                Log.Error("Scene asset name '{0}' is invalid.", sceneAssetName);
                return false;
            }

            return _gameSceneManager.HasScene(sceneAssetName);
        }

        /// <summary>
        /// 加载场景。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        public async Task<SceneHandle> LoadScene(string sceneAssetName)
        {
            return await LoadScene(sceneAssetName, SceneLoadMode.Single, null);
        }

        /// <summary>
        /// 加载场景。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <param name="sceneMode">加载场景的方式。</param>
        /// <param name="userData">用户自定义数据。</param>
        public async Task<SceneHandle> LoadScene(string sceneAssetName, SceneLoadMode sceneMode, object userData = null)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                Log.Error("Scene asset name is invalid.");
                throw new ArgumentNullException(nameof(sceneAssetName));
            }

            if (!sceneAssetName.StartsWith("res://", StringComparison.Ordinal) ||
                !sceneAssetName.EndsWith(".tscn", StringComparison.Ordinal))
            {
                Log.Error("Scene asset name '{0}' is invalid.", sceneAssetName);
                throw new ArgumentException(nameof(sceneAssetName));
            }

            return await _gameSceneManager.LoadScene(sceneAssetName, sceneMode, userData);
        }

        /// <summary>
        /// 卸载场景。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void UnloadScene(string sceneAssetName, object userData = null)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                throw new ArgumentNullException(nameof(sceneAssetName));
            }

            if (!sceneAssetName.StartsWith("res://", StringComparison.Ordinal) ||
                !sceneAssetName.EndsWith(".tscn", StringComparison.Ordinal))
            {
                throw new ArgumentException(string.Format("Scene asset name '{0}' is invalid.", sceneAssetName), nameof(sceneAssetName));
            }

            _gameSceneManager.UnloadScene(sceneAssetName, userData);
        }

        /// <summary>
        /// 设置场景顺序。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <param name="sceneOrder">要设置的场景顺序。</param>
        public void SetSceneOrder(string sceneAssetName, int sceneOrder)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                Log.Error("Scene asset name is invalid.");
                return;
            }

            if (!sceneAssetName.StartsWith("res://", StringComparison.Ordinal) ||
                !sceneAssetName.EndsWith(".tscn", StringComparison.Ordinal))
            {
                Log.Error("Scene asset name '{0}' is invalid.", sceneAssetName);
                return;
            }

            if (SceneIsLoading(sceneAssetName))
            {
                m_SceneOrder[sceneAssetName] = sceneOrder;
                return;
            }

            if (SceneIsLoaded(sceneAssetName))
            {
                m_SceneOrder[sceneAssetName] = sceneOrder;
                RefreshSceneOrder();
                return;
            }

            Log.Error("Scene '{0}' is not loaded or loading.", sceneAssetName);
        }

        /// <summary>
        /// 刷新当前场景主摄像机。
        /// </summary>
        public void RefreshMainCamera()
        {
            var tree = GetTree();
            if (tree == null || tree.Root == null)
            {
                return;
            }

            m_MainCamera = FindCameraInTree<Camera3D>(tree.Root);
            m_MainCamera2D = FindCameraInTree<Camera2D>(tree.Root);
        }

        public override void _Process(double delta)
        {
            if (m_PendingActivateSceneAssetName == null)
            {
                return;
            }

            // 对应 Unity 版 RefreshSceneOrderWhenLoadedCo 协程：场景句柄尚未就绪时按帧轮询，
            // 最多等待 60 帧（Godot 侧由 _Process 替代协程 yield return null）。
            string sceneAssetName = m_PendingActivateSceneAssetName;
            var handle = _gameSceneManager.GetSceneHandle(sceneAssetName);
            if (handle != null && handle.SceneNode != null && GodotObject.IsInstanceValid(handle.SceneNode))
            {
                m_PendingActivateSceneAssetName = null;
                SetActiveScene(sceneAssetName, handle);
                return;
            }

            m_PendingActivateRetryCount++;
            if (m_PendingActivateRetryCount >= ActivateSceneRetryMaxFrames)
            {
                Log.Warning("Scene '{0}' did not become loaded within timeout, skip activation.", sceneAssetName);
                m_PendingActivateSceneAssetName = null;
            }
        }

        private void RefreshSceneOrder()
        {
            if (m_SceneOrder.Count == 0)
            {
                // Godot 无 Unity 的 GameFrameworkScene 概念：无排序场景时不主动切换 CurrentScene，保持现状。
                return;
            }

            string maxSceneName = null;
            int maxSceneOrder = 0;
            foreach (var sceneOrder in m_SceneOrder)
            {
                if (SceneIsLoading(sceneOrder.Key))
                {
                    continue;
                }

                if (maxSceneName == null)
                {
                    maxSceneName = sceneOrder.Key;
                    maxSceneOrder = sceneOrder.Value;
                    continue;
                }

                if (sceneOrder.Value > maxSceneOrder)
                {
                    maxSceneName = sceneOrder.Key;
                    maxSceneOrder = sceneOrder.Value;
                }
            }

            if (maxSceneName == null)
            {
                return;
            }

            var handle = _gameSceneManager.GetSceneHandle(maxSceneName);
            if (handle == null || handle.SceneNode == null || !GodotObject.IsInstanceValid(handle.SceneNode))
            {
                // Single 模式切换窗口期内场景句柄/节点可能尚未就绪，推迟到 _Process 轮询重试。
                // 多次连续切换时后设置的等待目标直接覆盖旧目标，避免旧重试与新目标争抢激活权。
                m_PendingActivateSceneAssetName = maxSceneName;
                m_PendingActivateRetryCount = 0;
                return;
            }

            SetActiveScene(maxSceneName, handle);
        }

        private void SetActiveScene(string sceneAssetName, SceneHandle handle)
        {
            var tree = GetTree();
            if (tree == null)
            {
                return;
            }

            var sceneNode = handle.SceneNode;
            var lastActiveScene = tree.CurrentScene;
            if (lastActiveScene != sceneNode)
            {
                // Godot 无多活动场景语义：激活即切换 SceneTree.CurrentScene（由 SceneHandle.ActivateScene 完成，
                // 同时处理游离节点的挂树）。
                if (handle.ActivateScene())
                {
                    m_EventComponent.Fire(this, ActiveSceneChangedEventArgs.Create(
                        GetNodeSceneFilePath(lastActiveScene), lastActiveScene,
                        sceneAssetName, sceneNode));
                }
            }

            RefreshMainCamera();
        }

        private static string GetNodeSceneFilePath(Node node)
        {
            if (node == null || !GodotObject.IsInstanceValid(node))
            {
                return string.Empty;
            }

            return node.SceneFilePath;
        }

        private static T FindCameraInTree<T>(Node node) where T : Node
        {
            if (node is T camera)
            {
                return camera;
            }

            foreach (Node child in node.GetChildren())
            {
                var result = FindCameraInTree<T>(child);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private void OnLoadGameSceneSuccess(object sender, LoadSceneSuccessEventArgs eventArgs)
        {
            if (!m_SceneOrder.ContainsKey(eventArgs.SceneAssetName))
            {
                m_SceneOrder.Add(eventArgs.SceneAssetName, 0);
            }

            m_EventComponent.Fire(this, LoadSceneSuccessEventArgs.Create(eventArgs.SceneAssetName, eventArgs.Duration, eventArgs.UserData));
            RefreshSceneOrder();
        }

        private void OnLoadGameSceneFailure(object sender, LoadSceneFailureEventArgs eventArgs)
        {
            Log.Warning("Load scene failure, scene asset name '{0}', error message '{1}'.", eventArgs.SceneAssetName,
                        eventArgs.ErrorMessage);
            m_EventComponent.Fire(this, LoadSceneFailureEventArgs.Create(eventArgs.SceneAssetName, eventArgs.Status, eventArgs.ErrorMessage, eventArgs.UserData));
        }

        private void OnLoadGameSceneUpdate(object sender, LoadSceneUpdateEventArgs eventArgs)
        {
            m_EventComponent.Fire(this, LoadSceneUpdateEventArgs.Create(eventArgs.SceneAssetName, eventArgs.Progress, eventArgs.UserData));
        }

        private void OnUnloadGameSceneSuccess(object sender, UnloadSceneSuccessEventArgs eventArgs)
        {
            m_EventComponent.Fire(this, UnloadSceneSuccessEventArgs.Create(eventArgs.SceneAssetName, eventArgs.UserData));
            m_SceneOrder.Remove(eventArgs.SceneAssetName);
            RefreshSceneOrder();
        }

        private void OnUnloadGameSceneFailure(object sender, UnloadSceneFailureEventArgs eventArgs)
        {
            Log.Warning("Unload scene failure, scene asset name '{0}'.", eventArgs.SceneAssetName);
            m_EventComponent.Fire(this, UnloadSceneFailureEventArgs.Create(eventArgs.SceneAssetName, eventArgs.UserData));
        }
    }
}
