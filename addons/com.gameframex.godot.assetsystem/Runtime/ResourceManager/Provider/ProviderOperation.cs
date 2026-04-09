using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using Godot;

namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    internal abstract class ProviderOperation : AsyncOperationBase
    {
        [AssetSystemPreserve]
        protected enum ESteps
        {
            None = 0,
            CheckBundle,
            Loading,
            Checking,
            Done,
        }

        /// <summary>
        /// 资源提供者唯一标识符
        /// </summary>
        public string ProviderGUID { private set; get; }

        /// <summary>
        /// 所属资源系统
        /// </summary>
        public ResourceManager ResourceMgr { private set; get; }

        /// <summary>
        /// 资源信息
        /// </summary>
        public AssetInfo MainAssetInfo { private set; get; }

        /// <summary>
        /// 获取的资源对象
        /// </summary>
        public object AssetObject { protected set; get; }

        /// <summary>
        /// 获取的资源对象集合
        /// </summary>
        public object[] AllAssetObjects { protected set; get; }

        /// <summary>
        /// 获取的场景对象
        /// </summary>
        public AssetSceneInfo SceneInfo { internal set; get; }

        /// <summary>
        /// 方案2迁移备注：Godot 场景节点实例（运行时主字段）
        /// </summary>
        public Node SceneNode { internal set; get; }

        /// <summary>
        /// 获取的原生对象
        /// </summary>
        public RawBundle RawBundleObject { protected set; get; }

        /// <summary>
        /// 加载的场景名称
        /// </summary>
        public string SceneName { protected set; get; }

        /// <summary>
        /// 引用计数
        /// </summary>
        public int RefCount { private set; get; } = 0;

        /// <summary>
        /// 是否已经销毁
        /// </summary>
        public bool IsDestroyed { private set; get; } = false;


        protected ESteps _steps = ESteps.None;
        protected LoadBundleFileOperation LoadBundleFileOp { private set; get; }
        protected LoadDependBundleFileOperation LoadDependBundleFileOp { private set; get; }
        protected bool IsWaitForAsyncComplete { private set; get; } = false;
        private readonly List<HandleBase> _handles = new();


        [AssetSystemPreserve]
        public ProviderOperation(ResourceManager manager, string providerGUID, AssetInfo assetInfo)
        {
            ResourceMgr = manager;
            ProviderGUID = providerGUID;
            MainAssetInfo = assetInfo;

            if (string.IsNullOrEmpty(providerGUID) == false)
            {
                LoadBundleFileOp = manager.CreateMainBundleFileLoader(assetInfo);
                LoadBundleFileOp.Reference();
                LoadBundleFileOp.AddProvider(this);

                LoadDependBundleFileOp = manager.CreateDependFileLoaders(assetInfo);
                LoadDependBundleFileOp.Reference();
            }
        }

        [AssetSystemPreserve]
        public override void InternalWaitForAsyncComplete()
        {
            IsWaitForAsyncComplete = true;

            while (true)
            {
                if (LoadDependBundleFileOp != null)
                {
                    LoadDependBundleFileOp.WaitForAsyncComplete();
                }

                if (LoadBundleFileOp != null)
                {
                    LoadBundleFileOp.WaitForAsyncComplete();
                }

                if (ExecuteWhileDone())
                {
                    _steps = ESteps.Done;
                    break;
                }
            }
        }

        /// <summary>
        /// 销毁资源提供者
        /// </summary>
        [AssetSystemPreserve]
        public void DestroyProvider()
        {
            IsDestroyed = true;

            // 检测是否为正常销毁
            if (IsDone == false)
            {
                Error = "User abort !";
                Status = EOperationStatus.Failed;
            }

            // 释放资源包加载器
            if (LoadBundleFileOp != null)
            {
                LoadBundleFileOp.Release();
                LoadBundleFileOp = null;
            }

            if (LoadDependBundleFileOp != null)
            {
                LoadDependBundleFileOp.Release();
                LoadDependBundleFileOp = null;
            }
        }

        /// <summary>
        /// 是否可以销毁
        /// </summary>
        [AssetSystemPreserve]
        public bool CanDestroyProvider()
        {
            // 注意：在进行资源加载过程时不可以销毁
            if (_steps == ESteps.Loading || _steps == ESteps.Checking)
            {
                return false;
            }

            return RefCount <= 0;
        }

        /// <summary>
        /// 创建资源句柄
        /// </summary>
        [AssetSystemPreserve]
        public T CreateHandle<T>() where T : HandleBase
        {
            // 引用计数增加
            RefCount++;

            HandleBase handle;
            if (typeof(T) == typeof(AssetHandle))
            {
                handle = new AssetHandle(this);
            }
            else if (typeof(T) == typeof(SceneHandle))
            {
                handle = new SceneHandle(this);
            }
            else if (typeof(T) == typeof(SubAssetsHandle))
            {
                handle = new SubAssetsHandle(this);
            }
            else if (typeof(T) == typeof(AllAssetsHandle))
            {
                handle = new AllAssetsHandle(this);
            }
            else if (typeof(T) == typeof(RawFileHandle))
            {
                handle = new RawFileHandle(this);
            }
            else
            {
                throw new NotImplementedException();
            }

            _handles.Add(handle);
            return handle as T;
        }

        /// <summary>
        /// 释放资源句柄
        /// </summary>
        [AssetSystemPreserve]
        public void ReleaseHandle(HandleBase handle)
        {
            if (RefCount <= 0)
            {
                throw new Exception("Should never get here !");
            }

            if (_handles.Remove(handle) == false)
            {
                throw new Exception("Should never get here !");
            }

            // 引用计数减少
            RefCount--;
        }

        /// <summary>
        /// 释放所有资源句柄
        /// </summary>
        [AssetSystemPreserve]
        public void ReleaseAllHandles()
        {
            for (var i = _handles.Count - 1; i >= 0; i--)
            {
                var handle = _handles[i];
                handle.ReleaseInternal();
            }
        }

        /// <summary>
        /// 处理致命问题
        /// </summary>
        [AssetSystemPreserve]
        protected void ProcessFatalEvent()
        {
            if (LoadBundleFileOp.IsDestroyed)
            {
                throw new Exception("Should never get here !");
            }

            var error = $"The bundle {LoadBundleFileOp.BundleFileInfo.Bundle.BundleName} has been invalidated unexpectedly.";
            AssetSystemLogger.Error(error);
            InvokeCompletion(Error, EOperationStatus.Failed);
        }

        /// <summary>
        /// 结束流程
        /// </summary>
        [AssetSystemPreserve]
        protected void InvokeCompletion(string error, EOperationStatus status)
        {
            DebugEndRecording();
            EndLoadTimeRecord();

            _steps = ESteps.Done;
            Error = error;
            Status = status;

            // 注意：创建临时列表是为了防止外部逻辑在回调函数内创建或者释放资源句柄。
            // 注意：回调方法如果发生异常，会阻断列表里的后续回调方法！
            var tempers = new List<HandleBase>(_handles);
            foreach (var hande in tempers)
            {
                if (hande.IsValid)
                {
                    hande.InvokeCallback();
                }
            }
        }

        /// <summary>
        /// 获取下载报告
        /// </summary>
        [AssetSystemPreserve]
        public DownloadStatus GetDownloadStatus()
        {
            var status = new DownloadStatus();
            status.TotalBytes = LoadBundleFileOp.BundleFileInfo.Bundle.FileSize;
            status.DownloadedBytes = LoadBundleFileOp.DownloadedBytes;
            foreach (var dependBundle in LoadDependBundleFileOp.Depends)
            {
                status.TotalBytes += dependBundle.BundleFileInfo.Bundle.FileSize;
                status.DownloadedBytes += dependBundle.DownloadedBytes;
            }

            if (status.TotalBytes == 0)
            {
                throw new Exception("Should never get here !");
            }

            status.IsDone = status.DownloadedBytes == status.TotalBytes;
            status.Progress = (float)status.DownloadedBytes / status.TotalBytes;
            return status;
        }

        #region 调试信息相关

        /// <summary>
        /// 出生的场景
        /// </summary>
        public string SpawnScene = string.Empty;

        /// <summary>
        /// 出生的时间
        /// </summary>
        public string SpawnTime = string.Empty;

        /// <summary>
        /// 加载耗时（单位：毫秒）
        /// </summary>
        public long LoadingTime { protected set; get; }
        /// <summary>
        /// 加载耗时（单位：毫秒）
        /// </summary>
        public long Duration { private set; get; }

        /// <summary>
        /// 加载开始时间（单位：秒）
        /// 使用 AssetSystemTime.RealtimeSinceStartup 记录，初始值为-1表示未开始
        /// </summary>
        private float _loadingStartTime = -1f;

        /// <summary>
        /// 加载结束时间（单位：秒）
        /// 使用 AssetSystemTime.RealtimeSinceStartup 记录，初始值为-1表示未结束
        /// </summary>
        private float _loadingEndTime = -1f;

        // 加载耗时统计
        private Stopwatch _watch = null;

        [AssetSystemPreserve]
        [Conditional("DEBUG")]
        public void InitSpawnDebugInfo()
        {
            var tree = Engine.GetMainLoop() as SceneTree;
            SpawnScene = tree?.CurrentScene?.Name ?? "UnknownScene";
            SpawnTime = SpawnTimeToString(AssetSystemTime.RealtimeSinceStartup);
        }

        [AssetSystemPreserve]
        private string SpawnTimeToString(float spawnTime)
        {
            float h = (float)System.Math.Floor(spawnTime / 3600f);
            float m = (float)System.Math.Floor(spawnTime / 60f - h * 60f);
            float s = (float)System.Math.Floor(spawnTime - m * 60f - h * 3600f);
            return h.ToString("00") + ":" + m.ToString("00") + ":" + s.ToString("00");
        }

        [AssetSystemPreserve]
        [Conditional("DEBUG")]
        protected void DebugBeginRecording()
        {
            if (_watch == null)
            {
                _watch = Stopwatch.StartNew();
            }
        }

        [AssetSystemPreserve]
        [Conditional("DEBUG")]
        private void DebugEndRecording()
        {
            if (_watch != null)
            {
                LoadingTime = _watch.ElapsedMilliseconds;
                _watch = null;
            }
        }
        /// <summary>
        /// 开始记录加载时间（单位：秒）
        /// 仅当尚未记录时，以 AssetSystemTime.RealtimeSinceStartup 赋值 _loadingStartTime
        /// </summary>
        [AssetSystemPreserve]
        protected void BeginLoadTimeRecord()
        {
            if (_loadingStartTime < 0f)
            {
            _loadingStartTime = AssetSystemTime.RealtimeSinceStartup;
            }
        }

        /// <summary>
        /// 结束加载时间记录并计算总耗时
        /// 1. 若 _loadingEndTime 未记录，则以当前 realtimeSinceStartup 赋值  
        /// 2. 若 _loadingStartTime 仍未记录（异常场景），则将其设为与 _loadingEndTime 相同，避免负值  
        /// 3. 计算耗时（毫秒），负值则取 0，最终写入 Duration 字段  
        /// </summary>
        [AssetSystemPreserve]
        private void EndLoadTimeRecord()
        {
            if (_loadingEndTime < 0f)
            {
            _loadingEndTime = AssetSystemTime.RealtimeSinceStartup;
            }

            if (_loadingStartTime < 0f)
            {
                _loadingStartTime = _loadingEndTime;
            }

            var duration = (_loadingEndTime - _loadingStartTime) * 1000f;
            if (duration < 0f)
            {
                duration = 0f;
            }

            Duration = (long)duration;
        }

        /// <summary>
        /// 获取资源包的调试信息列表
        /// </summary>
        [AssetSystemPreserve]
        internal void GetBundleDebugInfos(List<DebugBundleInfo> output)
        {
            var bundleInfo = new DebugBundleInfo();
            bundleInfo.BundleName = LoadBundleFileOp.BundleFileInfo.Bundle.BundleName;
            bundleInfo.RefCount = LoadBundleFileOp.RefCount;
            bundleInfo.Status = LoadBundleFileOp.Status;
            output.Add(bundleInfo);

            LoadDependBundleFileOp.GetBundleDebugInfos(output);
        }

        #endregion
    }

    internal interface IBundleAssetLoadRequest
    {
        bool IsDone { get; }
        float Progress { get; }
        object AssetObject { get; }
        object[] AllAssetObjects { get; }
    }

    internal interface IBundleAssetLoader
    {
        object LoadAsset(string assetPath, Type assetType);
        IBundleAssetLoadRequest LoadAssetAsync(string assetPath, Type assetType);
        object[] LoadSubAssets(string assetPath, Type assetType);
        IBundleAssetLoadRequest LoadSubAssetsAsync(string assetPath, Type assetType);
        object[] LoadAllAssets(Type assetType);
        IBundleAssetLoadRequest LoadAllAssetsAsync(Type assetType);
    }

    internal interface ISceneLoadController
    {
        SceneLoadMode SceneMode { get; }
        void UnSuspendLoad();
        void CancelLoad();
    }

    internal static class BundleAssetLoaderFactory
    {
        private static IResourceBackend _backend = new GodotResourceBackend();

        public static IResourceBackend Backend
        {
            get { return _backend; }
            set { _backend = value ?? new GodotResourceBackend(); }
        }

        public static bool TryCreate(object bundleResult, out IBundleAssetLoader loader, out string error)
        {
            return Backend.TryCreateBundleAssetLoader(bundleResult, out loader, out error);
        }
    }

    internal sealed class UnityAssetBundleLoader : IBundleAssetLoader
    {
        private readonly BundleFile _assetBundle;

        public UnityAssetBundleLoader(BundleFile assetBundle)
        {
            _assetBundle = assetBundle;
        }

        public object LoadAsset(string assetPath, Type assetType)
        {
            return assetType == null ? _assetBundle.LoadAsset(assetPath) : _assetBundle.LoadAsset(assetPath, assetType);
        }

        public IBundleAssetLoadRequest LoadAssetAsync(string assetPath, Type assetType)
        {
            var request = assetType == null ? _assetBundle.LoadAssetAsync(assetPath) : _assetBundle.LoadAssetAsync(assetPath, assetType);
            return new UnityAssetBundleLoadRequest(request);
        }

        public object[] LoadSubAssets(string assetPath, Type assetType)
        {
            return assetType == null ? _assetBundle.LoadAssetWithSubAssets(assetPath) : _assetBundle.LoadAssetWithSubAssets(assetPath, assetType);
        }

        public IBundleAssetLoadRequest LoadSubAssetsAsync(string assetPath, Type assetType)
        {
            var request = assetType == null ? _assetBundle.LoadAssetWithSubAssetsAsync(assetPath) : _assetBundle.LoadAssetWithSubAssetsAsync(assetPath, assetType);
            return new UnityAssetBundleLoadRequest(request);
        }

        public object[] LoadAllAssets(Type assetType)
        {
            return assetType == null ? _assetBundle.LoadAllAssets() : _assetBundle.LoadAllAssets(assetType);
        }

        public IBundleAssetLoadRequest LoadAllAssetsAsync(Type assetType)
        {
            var request = assetType == null ? _assetBundle.LoadAllAssetsAsync() : _assetBundle.LoadAllAssetsAsync(assetType);
            return new UnityAssetBundleLoadRequest(request);
        }
    }

    internal sealed class UnityAssetBundleLoadRequest : IBundleAssetLoadRequest
    {
        private readonly BundleAssetRequest _request;

        public UnityAssetBundleLoadRequest(BundleAssetRequest request)
        {
            _request = request;
        }

        public bool IsDone => _request.isDone;
        public float Progress => _request.progress;
        public object AssetObject => _request.asset;
        public object[] AllAssetObjects => _request.allAssets;
    }

    internal static class BundleAssetLoadUtility
    {
        public static List<string> GetPathCandidates(AssetInfo assetInfo)
        {
            var candidates = new List<string>(2);
            AddCandidate(candidates, assetInfo.AssetPath);
            AddCandidate(candidates, assetInfo.Address);
            return candidates;
        }

        public static bool IsTypeMatch(object assetObject, Type assetType)
        {
            if (assetObject == null)
            {
                return false;
            }

            if (IsWildcardType(assetType))
            {
                return true;
            }

            return assetType.IsAssignableFrom(assetObject.GetType());
        }

        public static object[] FilterByType(object[] allAssets, Type assetType)
        {
            if (allAssets == null || allAssets.Length == 0)
            {
                return allAssets;
            }

            if (IsWildcardType(assetType))
            {
                return allAssets;
            }

            var results = new List<object>(allAssets.Length);
            foreach (var assetObject in allAssets)
            {
                if (assetObject == null)
                {
                    continue;
                }

                if (assetType.IsAssignableFrom(assetObject.GetType()))
                {
                    results.Add(assetObject);
                }
            }

            return results.ToArray();
        }

        public static bool HasAnyAsset(object[] allAssets)
        {
            return allAssets != null && allAssets.Length > 0;
        }

        private static bool IsWildcardType(Type assetType)
        {
            return assetType == null || assetType == typeof(object);
        }

        private static void AddCandidate(List<string> candidates, string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            if (candidates.Contains(path))
            {
                return;
            }

            candidates.Add(path);
        }
    }
}
