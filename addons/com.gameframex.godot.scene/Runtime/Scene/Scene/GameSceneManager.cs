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
//  Any legal disputes and liabilities arising from secondary development based on this project
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
using System.Linq;
using System.Threading.Tasks;
using GameFrameX.Asset.Runtime;
using GameFrameX.AssetSystem;
using GameFrameX.Runtime;

namespace GameFrameX.Scene.Runtime
{
    /// <summary>
    /// 场景管理器。
    /// </summary>
    public sealed class GameSceneManager : GameFrameworkModule, IGameSceneManager
    {
        private sealed class SceneHandleData
        {
            public readonly SceneHandle SceneHandle;
            public readonly object UserData;

            public SceneHandleData(SceneHandle sceneHandle, object userData)
            {
                SceneHandle = sceneHandle;
                UserData = userData;
            }
        }

        private readonly Dictionary<string, SceneHandle> m_LoadedSceneAssetNames;
        private readonly Dictionary<string, SceneHandleData> m_LoadingSceneAssetNames;
        private readonly Dictionary<string, SceneHandle> m_UnloadingSceneAssetNames;
        private IAssetManager m_assetManager;
        private EventHandler<LoadSceneSuccessEventArgs> m_LoadSceneSuccessEventHandler;
        private EventHandler<LoadSceneFailureEventArgs> m_LoadSceneFailureEventHandler;
        private EventHandler<LoadSceneUpdateEventArgs> m_LoadSceneUpdateEventHandler;
        private EventHandler<UnloadSceneSuccessEventArgs> m_UnloadSceneSuccessEventHandler;
        private EventHandler<UnloadSceneFailureEventArgs> m_UnloadSceneFailureEventHandler;

        /// <summary>
        /// 初始化场景管理器的新实例。
        /// </summary>
        public GameSceneManager()
        {
            m_LoadedSceneAssetNames = new Dictionary<string, SceneHandle>();
            m_LoadingSceneAssetNames = new Dictionary<string, SceneHandleData>();
            m_UnloadingSceneAssetNames = new Dictionary<string, SceneHandle>();
            m_assetManager = null;
            m_LoadSceneSuccessEventHandler = null;
            m_LoadSceneFailureEventHandler = null;
            m_LoadSceneUpdateEventHandler = null;
            m_UnloadSceneSuccessEventHandler = null;
            m_UnloadSceneFailureEventHandler = null;
        }

        /// <summary>
        /// 获取游戏框架模块优先级。
        /// </summary>
        /// <remarks>优先级较高的模块会优先轮询，并且关闭操作会后进行。</remarks>
        public override int Priority
        {
            get { return 2; }
        }

        /// <summary>
        /// 加载场景成功事件。
        /// </summary>
        public event EventHandler<LoadSceneSuccessEventArgs> LoadSceneSuccess
        {
            add { m_LoadSceneSuccessEventHandler += value; }
            remove { m_LoadSceneSuccessEventHandler -= value; }
        }

        /// <summary>
        /// 加载场景失败事件。
        /// </summary>
        public event EventHandler<LoadSceneFailureEventArgs> LoadSceneFailure
        {
            add { m_LoadSceneFailureEventHandler += value; }
            remove { m_LoadSceneFailureEventHandler -= value; }
        }

        /// <summary>
        /// 加载场景更新事件。
        /// </summary>
        public event EventHandler<LoadSceneUpdateEventArgs> LoadSceneUpdate
        {
            add { m_LoadSceneUpdateEventHandler += value; }
            remove { m_LoadSceneUpdateEventHandler -= value; }
        }

        /// <summary>
        /// 卸载场景成功事件。
        /// </summary>
        public event EventHandler<UnloadSceneSuccessEventArgs> UnloadSceneSuccess
        {
            add { m_UnloadSceneSuccessEventHandler += value; }
            remove { m_UnloadSceneSuccessEventHandler -= value; }
        }

        /// <summary>
        /// 卸载场景失败事件。
        /// </summary>
        public event EventHandler<UnloadSceneFailureEventArgs> UnloadSceneFailure
        {
            add { m_UnloadSceneFailureEventHandler += value; }
            remove { m_UnloadSceneFailureEventHandler -= value; }
        }

        /// <summary>
        /// 场景管理器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        /// <summary>
        /// 关闭并清理场景管理器。
        /// </summary>
        public override void Shutdown()
        {
            // 迁移备注：Unity 基准此处逐场景走异步 UnloadScene（依赖后续帧驱动 OperationSystem）；
            // Godot 下框架关停由 BaseComponent 的 Predelete/ExitTree 通知触发，引擎已处于退出/释放
            // 流程，异步卸载操作没有后续帧可驱动，其挂起续体会在引擎 teardown 的最后帧踩到半销毁
            // 状态（mutex lock failed SIGABRT，见 tests/EngineTests 引擎测试备案）。引擎退出本身会
            // 释放整棵场景树，此处改为同步释放句柄引用并清空状态字典，不发起任何异步链。
            foreach (var sceneHandle in m_LoadedSceneAssetNames.Values)
            {
                sceneHandle.ReleaseInternal();
            }

            foreach (var sceneHandle in m_UnloadingSceneAssetNames.Values)
            {
                sceneHandle.ReleaseInternal();
            }

            m_LoadedSceneAssetNames.Clear();
            m_UnloadingSceneAssetNames.Clear();
        }

        /// <summary>
        /// 设置资源管理器。
        /// </summary>
        /// <param name="assetManager"></param>
        public void SetResourceManager(IAssetManager assetManager)
        {
            if (assetManager == null)
            {
                throw new GameFrameworkException("Resource manager is invalid.");
            }

            m_assetManager = assetManager;
        }

        private void CheckSceneAssetName(string sceneAssetName)
        {
            if (string.IsNullOrEmpty(sceneAssetName))
            {
                throw new GameFrameworkException("Scene asset name is invalid.");
            }
        }

        private void CheckAssetManager()
        {
            if (m_assetManager == null)
            {
                throw new GameFrameworkException("You must set resource manager first.");
            }
        }

        /// <summary>
        /// 获取场景是否已加载。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <returns>场景是否已加载。</returns>
        public bool SceneIsLoaded(string sceneAssetName)
        {
            CheckSceneAssetName(sceneAssetName);
            return m_LoadedSceneAssetNames.ContainsKey(sceneAssetName);
        }

        /// <summary>
        /// 获取已加载场景的资源名称。
        /// </summary>
        /// <returns>已加载场景的资源名称。</returns>
        public string[] GetLoadedSceneAssetNames()
        {
            return m_LoadedSceneAssetNames.Keys.ToArray();
        }

        /// <summary>
        /// 获取已加载场景的资源名称。
        /// </summary>
        /// <param name="results">已加载场景的资源名称。</param>
        public void GetLoadedSceneAssetNames(List<string> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            results.AddRange(m_LoadedSceneAssetNames.Keys);
        }

        /// <summary>
        /// 获取场景是否正在加载。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <returns>场景是否正在加载。</returns>
        public bool SceneIsLoading(string sceneAssetName)
        {
            CheckSceneAssetName(sceneAssetName);
            return m_LoadingSceneAssetNames.ContainsKey(sceneAssetName);
        }

        /// <summary>
        /// 获取正在加载场景的资源名称。
        /// </summary>
        /// <returns>正在加载场景的资源名称。</returns>
        public string[] GetLoadingSceneAssetNames()
        {
            return m_LoadingSceneAssetNames.Keys.ToArray();
        }

        /// <summary>
        /// 获取正在加载场景的资源名称。
        /// </summary>
        /// <param name="results">正在加载场景的资源名称。</param>
        public void GetLoadingSceneAssetNames(List<string> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            results.AddRange(m_LoadingSceneAssetNames.Keys);
        }

        /// <summary>
        /// 获取场景是否正在卸载。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <returns>场景是否正在卸载。</returns>
        public bool SceneIsUnloading(string sceneAssetName)
        {
            CheckSceneAssetName(sceneAssetName);
            return m_UnloadingSceneAssetNames.ContainsKey(sceneAssetName);
        }

        /// <summary>
        /// 获取正在卸载场景的资源名称。
        /// </summary>
        /// <returns>正在卸载场景的资源名称。</returns>
        public string[] GetUnloadingSceneAssetNames()
        {
            return m_UnloadingSceneAssetNames.Keys.ToArray();
        }

        /// <summary>
        /// 获取正在卸载场景的资源名称。
        /// </summary>
        /// <param name="results">正在卸载场景的资源名称。</param>
        public void GetUnloadingSceneAssetNames(List<string> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            results.AddRange(m_UnloadingSceneAssetNames.Keys);
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
                return false;
            }

            if (m_assetManager == null)
            {
                return false;
            }

            return m_assetManager.HasAssetPath(sceneAssetName);
        }

        /// <summary>
        /// 获取已加载场景的资源句柄。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <returns>已加载场景的资源句柄，未加载时返回 null。</returns>
        public SceneHandle GetSceneHandle(string sceneAssetName)
        {
            CheckSceneAssetName(sceneAssetName);

            SceneHandle sceneHandle;
            return m_LoadedSceneAssetNames.TryGetValue(sceneAssetName, out sceneHandle) ? sceneHandle : null;
        }

        /// <summary>
        /// 加载场景。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        public Task<SceneHandle> LoadScene(string sceneAssetName)
        {
            return LoadScene(sceneAssetName, SceneLoadMode.Single);
        }

        /// <summary>
        /// 加载场景。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <param name="sceneMode">加载场景的方式。</param>
        public Task<SceneHandle> LoadScene(string sceneAssetName, SceneLoadMode sceneMode)
        {
            return LoadScene(sceneAssetName, sceneMode, null);
        }

        /// <summary>
        /// 加载场景。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <param name="userData">用户自定义数据。</param>
        public Task<SceneHandle> LoadScene(string sceneAssetName, object userData)
        {
            return LoadScene(sceneAssetName, SceneLoadMode.Single, userData);
        }

        /// <summary>
        /// 加载场景。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <param name="sceneMode"></param>
        public async Task<SceneHandle> LoadScene(string sceneAssetName, SceneLoadMode sceneMode, object userData)
        {
            CheckSceneAssetName(sceneAssetName);
            CheckAssetManager();

            if (SceneIsUnloading(sceneAssetName))
            {
                throw new GameFrameworkException(Utility.Text.Format("Scene asset '{0}' is being unloaded.", sceneAssetName));
            }

            if (SceneIsLoading(sceneAssetName))
            {
                throw new GameFrameworkException(Utility.Text.Format("Scene asset '{0}' is being loaded.", sceneAssetName));
            }

            if (sceneMode == SceneLoadMode.Single)
            {
                // Single 模式下提前在 m_LoadingSceneAssetNames 占位，防止下方 UnloadAllLoadedScenesInternal
                // 同步触发的 unload 事件订阅者重入 LoadScene(同名场景) 时跳过 SceneIsLoading 检查；
                // await 后用索引器更新为真实 SceneHandle。占位期间 SceneHandle 为 null 是临时的，
                // 因为 OnLoadSceneCompleted/OnLoadSceneUpdate 都从 sceneHandle 参数取 AssetPath，不依赖字典内的 handle。
                m_LoadingSceneAssetNames[sceneAssetName] = new SceneHandleData(null, userData);

                // Godot 迁移说明：Unity 在 Single 模式下由引擎自动销毁其他场景的 GameObject；
                // Godot 侧由 assetsystem 的场景加载流程（DatabaseSceneProvider.TryAttachSceneNode）负责
                // QueueFree 旧的 CurrentScene，框架侧仍需主动释放 YooAsset 资源引用并清理字典残留，
                // 否则后续重新加载（含重启同场景）会报 "already loaded" 异常，故统一清理。
                UnloadAllLoadedScenesInternal();
            }
            else if (SceneIsLoaded(sceneAssetName))
            {
                throw new GameFrameworkException(Utility.Text.Format("Scene asset '{0}' is already loaded.", sceneAssetName));
            }
            else
            {
                // Additive 模式不触发同步 unload 事件，无需提前占位；保持原 Add 行为便于检测重复键。
                m_LoadingSceneAssetNames.Add(sceneAssetName, new SceneHandleData(null, userData));
            }

            SceneHandle sceneOperationHandle;
            try
            {
                sceneOperationHandle = await m_assetManager.LoadSceneAsync(sceneAssetName, sceneMode, true);
            }
            catch
            {
                // await 抛异常时清理占位，避免下次 LoadScene 同名场景被 SceneIsLoading 误判为正在加载。
                m_LoadingSceneAssetNames.Remove(sceneAssetName);
                throw;
            }

            m_LoadingSceneAssetNames[sceneAssetName] = new SceneHandleData(sceneOperationHandle, userData);
            // sceneOperationHandle.Update += OnLoadSceneUpdate;
            sceneOperationHandle.Completed += OnLoadSceneCompleted;
            return sceneOperationHandle;
        }

        private void UnloadAllLoadedScenesInternal()
        {
            if (m_LoadedSceneAssetNames.Count == 0)
            {
                return;
            }

            var entries = m_LoadedSceneAssetNames.ToArray();
            m_LoadedSceneAssetNames.Clear();

            foreach (var entry in entries)
            {
                var handle = entry.Value;
                var sceneAssetName = entry.Key;

                if (handle != null && !handle.IsMainScene())
                {
                    // Additive 场景：走标准 UnloadAsync 流程，事件由 Completed 回调触发。
                    var unloadOp = handle.UnloadAsync();
                    m_UnloadingSceneAssetNames.Add(sceneAssetName, handle);

                    void OnUnloadCompleted(AsyncOperationBase asyncOperationBase)
                    {
                        if (asyncOperationBase.Error.IsNullOrEmpty())
                        {
                            UnloadSceneSuccessCallback(sceneAssetName, null);
                        }
                        else
                        {
                            UnloadSceneFailureCallback(sceneAssetName, null);
                        }
                    }

                    unloadOp.Completed += OnUnloadCompleted;
                }
                else
                {
                    // Single 模式场景被 assetsystem 标记为 main scene，主动 UnloadAsync 会被拒绝；
                    // Godot 加载新 Single 场景时由 assetsystem 自动销毁旧场景节点，资源释放交由 assetsystem 内部处理。
                    // 但仍需主动触发 UnloadSceneSuccess 事件，让 SceneComponent 等订阅方同步状态（如清理 m_SceneOrder）。
                    UnloadSceneSuccessCallback(sceneAssetName, null);
                }
            }
        }

        private void OnLoadSceneUpdate(SceneHandle sceneHandle)
        {
            if (m_LoadingSceneAssetNames.TryGetValue(sceneHandle.GetAssetInfo().AssetPath, out var value))
            {
                LoadSceneUpdateCallback(sceneHandle.GetAssetInfo().AssetPath, sceneHandle.Progress, value.UserData);
            }
        }

        private void OnLoadSceneCompleted(SceneHandle sceneOperationHandle)
        {
            string assetPath = sceneOperationHandle.GetAssetInfo().AssetPath;

            SceneHandleData value = null;
            if (m_LoadingSceneAssetNames.TryGetValue(assetPath, out value))
            {
                m_LoadingSceneAssetNames.Remove(assetPath);
            }

            if (value != null)
            {
                if (sceneOperationHandle.IsDone && sceneOperationHandle.Status == EOperationStatus.Succeed)
                {
                    // 仅加载成功的场景进入 loaded 字典；失败场景不进，避免被 SceneIsLoaded 误报为已加载。
                    if (!m_LoadedSceneAssetNames.ContainsKey(assetPath))
                    {
                        m_LoadedSceneAssetNames.Add(assetPath, sceneOperationHandle);
                    }

                    LoadSceneSuccessCallback(assetPath, sceneOperationHandle.Duration, value.UserData);
                }
                else
                {
                    LoadSceneFailureCallback(assetPath, sceneOperationHandle.Status, sceneOperationHandle.LastError, value.UserData);
                }
            }
        }

        /// <summary>
        /// 卸载场景。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        public void UnloadScene(string sceneAssetName)
        {
            UnloadScene(sceneAssetName, null);
        }

        /// <summary>
        /// 卸载场景。
        /// </summary>
        /// <param name="sceneAssetName">场景资源名称。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void UnloadScene(string sceneAssetName, object userData)
        {
            CheckSceneAssetName(sceneAssetName);
            CheckAssetManager();

            if (SceneIsUnloading(sceneAssetName))
            {
                throw new GameFrameworkException(Utility.Text.Format("Scene asset '{0}' is being unloaded.", sceneAssetName));
            }

            if (SceneIsLoading(sceneAssetName))
            {
                throw new GameFrameworkException(Utility.Text.Format("Scene asset '{0}' is being loaded.", sceneAssetName));
            }

            if (!SceneIsLoaded(sceneAssetName))
            {
                throw new GameFrameworkException(Utility.Text.Format("Scene asset '{0}' is not loaded yet.", sceneAssetName));
            }

            if (m_LoadedSceneAssetNames.TryGetValue(sceneAssetName, out var sceneOperationHandle))
            {
                var unloadSceneOperationHandle = sceneOperationHandle.UnloadAsync();
                m_LoadedSceneAssetNames.Remove(sceneAssetName);
                m_UnloadingSceneAssetNames.Add(sceneAssetName, sceneOperationHandle);

                void OnUnloadSceneOperationHandleOnCompleted(AsyncOperationBase asyncOperationBase)
                {
                    if (asyncOperationBase.Error.IsNullOrEmpty())
                    {
                        UnloadSceneSuccessCallback(sceneAssetName, userData);
                    }
                    else
                    {
                        UnloadSceneFailureCallback(sceneAssetName, userData);
                    }
                }

                unloadSceneOperationHandle.Completed += OnUnloadSceneOperationHandleOnCompleted;
            }
        }

        private void LoadSceneSuccessCallback(string sceneAssetName, long duration, object userData)
        {
            if (m_LoadSceneSuccessEventHandler != null)
            {
                LoadSceneSuccessEventArgs loadSceneSuccessEventArgs = LoadSceneSuccessEventArgs.Create(sceneAssetName, duration, userData);
                m_LoadSceneSuccessEventHandler(this, loadSceneSuccessEventArgs);
                ReferencePool.Release(loadSceneSuccessEventArgs);
            }
        }

        private void LoadSceneFailureCallback(string sceneAssetName, EOperationStatus status, string errorMessage, object userData)
        {
            string appendErrorMessage = Utility.Text.Format("Load scene failure, scene asset name '{0}', status '{1}', error message '{2}'.", sceneAssetName, status, errorMessage);
            if (m_LoadSceneFailureEventHandler != null)
            {
                LoadSceneFailureEventArgs loadSceneFailureEventArgs = LoadSceneFailureEventArgs.Create(sceneAssetName, status, appendErrorMessage, userData);
                m_LoadSceneFailureEventHandler(this, loadSceneFailureEventArgs);
                ReferencePool.Release(loadSceneFailureEventArgs);
                return;
            }

            throw new GameFrameworkException(appendErrorMessage);
        }

        private void LoadSceneUpdateCallback(string sceneAssetName, float progress, object userData)
        {
            if (m_LoadSceneUpdateEventHandler != null)
            {
                LoadSceneUpdateEventArgs loadSceneUpdateEventArgs = LoadSceneUpdateEventArgs.Create(sceneAssetName, progress, userData);
                m_LoadSceneUpdateEventHandler(this, loadSceneUpdateEventArgs);
                ReferencePool.Release(loadSceneUpdateEventArgs);
            }
        }

        private void UnloadSceneSuccessCallback(string sceneAssetName, object userData)
        {
            m_UnloadingSceneAssetNames.Remove(sceneAssetName);
            m_LoadedSceneAssetNames.Remove(sceneAssetName);
            if (m_UnloadSceneSuccessEventHandler != null)
            {
                UnloadSceneSuccessEventArgs unloadSceneSuccessEventArgs = UnloadSceneSuccessEventArgs.Create(sceneAssetName, userData);
                m_UnloadSceneSuccessEventHandler(this, unloadSceneSuccessEventArgs);
                ReferencePool.Release(unloadSceneSuccessEventArgs);
            }
        }

        private void UnloadSceneFailureCallback(string sceneAssetName, object userData)
        {
            m_UnloadingSceneAssetNames.Remove(sceneAssetName);
            if (m_UnloadSceneFailureEventHandler != null)
            {
                UnloadSceneFailureEventArgs unloadSceneFailureEventArgs = UnloadSceneFailureEventArgs.Create(sceneAssetName, userData);
                m_UnloadSceneFailureEventHandler(this, unloadSceneFailureEventArgs);
                ReferencePool.Release(unloadSceneFailureEventArgs);
                return;
            }

            throw new GameFrameworkException(Utility.Text.Format("Unload scene failure, scene asset name '{0}'.", sceneAssetName));
        }
    }
}
