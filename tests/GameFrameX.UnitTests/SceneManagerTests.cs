using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GameFrameX.Asset.Runtime;
using GameFrameX.AssetSystem;
using GameFrameX.Runtime;
using GameFrameX.Scene.Runtime;
using Godot;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// 场景管理器行为测试（迁移自 Unity com.gameframex.unity.scene v2.3.1，Unity 侧 Tests 为空壳，此处自建）。
    /// 覆盖 GameSceneManager 三字典状态机：loaded/loading/unloading 的转换与查询、事件派发。
    /// 引擎依赖说明：
    /// 1. SceneHandle/ProviderOperation 构造函数为 internal，测试通过反射构造 DatabaseSceneProvider
    ///    （空 providerGUID + null ResourceManager 时构造函数不触碰 Godot 原生 API），全程无引擎调用。
    /// 2. 卸载操作的完成回调由 OperationSystem 驱动（纯 C# 静态循环），测试中手动 Initialize+Update 步进。
    /// 3. SceneComponent 的节点操作（CurrentScene 切换/摄像机遍历）依赖 Godot 运行时，归引擎测试，此处不覆盖。
    /// </summary>
    public sealed class SceneManagerTests : IDisposable
    {
        private const string SceneA = "res://Scenes/SceneA.tscn";
        private const string SceneB = "res://Scenes/SceneB.tscn";

        private readonly GameSceneManager _manager;
        private readonly FakeAssetManager _assetManager;

        public SceneManagerTests()
        {
            _manager = new GameSceneManager();
            _assetManager = new FakeAssetManager();
            _manager.SetResourceManager(_assetManager);
            // 屏蔽 assetsystem 的 Godot 原生日志兜底（GD.PushError/PushWarning）：
            // 测试宿主无 Godot 原生运行时，主场景 UnloadAsync 等路径触发时会直接段错误。
            _previousLogger = AssetSystemLogger.Logger;
            AssetSystemLogger.Logger = new NoOpLogger();
        }

        public void Dispose()
        {
            _manager.Shutdown();
            AssetSystemLogger.Logger = _previousLogger;
        }

        private readonly GameFrameX.AssetSystem.ILogger _previousLogger;

        // ──────────────── LoadScene：成功 ────────────────

        [Fact]
        public async Task LoadScene_Additive_Success_LoadedAndEventFired()
        {
            var userData = new object();
            string eventSceneName = null;
            object eventUserData = null;
            var eventFired = false;
            _manager.LoadSceneSuccess += (sender, args) =>
            {
                eventFired = true;
                eventSceneName = args.SceneAssetName;
                eventUserData = args.UserData;
            };

            var handle = CreateSceneHandle(SceneA, SceneLoadMode.Additive, EOperationStatus.Succeed, null);
            _assetManager.OnLoadSceneAsync = (path, mode, activate) => Task.FromResult(handle);

            var returned = await _manager.LoadScene(SceneA, SceneLoadMode.Additive, userData);

            Assert.Same(handle, returned);
            Assert.True(_manager.SceneIsLoaded(SceneA), "Scene should be loaded after success");
            Assert.False(_manager.SceneIsLoading(SceneA), "Scene should not stay in loading after success");
            Assert.True(eventFired, "LoadSceneSuccess event should fire");
            Assert.Equal(SceneA, eventSceneName);
            Assert.Same(userData, eventUserData);
            Assert.Contains(SceneA, _manager.GetLoadedSceneAssetNames());
        }

        // ──────────────── LoadScene：失败 ────────────────

        [Fact]
        public async Task LoadScene_FailedStatus_FiresFailureEvent_AndNotLoaded()
        {
            EOperationStatus? eventStatus = null;
            _manager.LoadSceneFailure += (sender, args) => { eventStatus = args.Status; };

            var handle = CreateSceneHandle(SceneA, SceneLoadMode.Additive, EOperationStatus.Failed, "load failed");
            _assetManager.OnLoadSceneAsync = (path, mode, activate) => Task.FromResult(handle);

            await _manager.LoadScene(SceneA, SceneLoadMode.Additive, null);

            Assert.Equal(EOperationStatus.Failed, eventStatus);
            Assert.False(_manager.SceneIsLoaded(SceneA), "Failed scene must not be marked loaded");
            Assert.False(_manager.SceneIsLoading(SceneA), "Failed scene must be removed from loading");
        }

        [Fact]
        public async Task LoadScene_AwaitThrows_CleansLoadingPlaceholder()
        {
            _assetManager.OnLoadSceneAsync = (path, mode, activate) => throw new InvalidOperationException("boom");

            await Assert.ThrowsAsync<InvalidOperationException>(() => _manager.LoadScene(SceneA, SceneLoadMode.Single, null));

            Assert.False(_manager.SceneIsLoading(SceneA), "Placeholder must be cleaned when await throws");
        }

        [Fact]
        public async Task LoadScene_Failure_WithoutSubscriber_ThrowsFrameworkException()
        {
            var handle = CreateSceneHandle(SceneA, SceneLoadMode.Additive, EOperationStatus.Failed, "load failed");
            _assetManager.OnLoadSceneAsync = (path, mode, activate) => Task.FromResult(handle);

            // Unity 版对齐行为：无订阅者时加载失败直接抛 GameFrameworkException。
            await Assert.ThrowsAsync<GameFrameworkException>(() => _manager.LoadScene(SceneA, SceneLoadMode.Additive, null));
        }

        // ──────────────── LoadScene：重复加载 ────────────────

        [Fact]
        public async Task LoadScene_AlreadyLoaded_Throws()
        {
            var handle = CreateSceneHandle(SceneA, SceneLoadMode.Additive, EOperationStatus.Succeed, null);
            _assetManager.OnLoadSceneAsync = (path, mode, activate) => Task.FromResult(handle);
            await _manager.LoadScene(SceneA, SceneLoadMode.Additive, null);

            await Assert.ThrowsAsync<GameFrameworkException>(() => _manager.LoadScene(SceneA, SceneLoadMode.Additive, null));
        }

        [Fact]
        public async Task LoadScene_InFlight_Additive_Duplicate_Throws()
        {
            var tcs = new TaskCompletionSource<SceneHandle>();
            _assetManager.OnLoadSceneAsync = (path, mode, activate) => tcs.Task;

            // 提前准备一个合法句柄，用于 finally 里释放挂起的 Task，避免悬挂 Task 跨用例累积导致 host 崩溃。
            var deferredHandle = CreateSceneHandle(SceneA, SceneLoadMode.Additive, EOperationStatus.Succeed, null);

            var loadTask = _manager.LoadScene(SceneA, SceneLoadMode.Additive, null);
            Assert.True(_manager.SceneIsLoading(SceneA), "In-flight scene should be marked loading");

            try
            {
                await Assert.ThrowsAsync<GameFrameworkException>(() => _manager.LoadScene(SceneA, SceneLoadMode.Additive, null));
            }
            finally
            {
                tcs.TrySetResult(deferredHandle);
            }

            // 在用例内等待续体结束，避免其与 Dispose->Shutdown 竞争操作管理器状态。
            await loadTask;
        }

        [Fact]
        public async Task LoadScene_InFlight_Single_Placeholder_BlocksReentry()
        {
            // Single 模式在 await 前先占位，即使句柄尚未返回，同名场景重入也应被拒绝。
            var tcs = new TaskCompletionSource<SceneHandle>();
            _assetManager.OnLoadSceneAsync = (path, mode, activate) => tcs.Task;
            var deferredHandle = CreateSceneHandle(SceneA, SceneLoadMode.Single, EOperationStatus.Succeed, null);

            var loadTask = _manager.LoadScene(SceneA, SceneLoadMode.Single, null);
            Assert.True(_manager.SceneIsLoading(SceneA), "Single-mode placeholder should mark scene loading");

            try
            {
                await Assert.ThrowsAsync<GameFrameworkException>(() => _manager.LoadScene(SceneA, SceneLoadMode.Single, null));
            }
            finally
            {
                tcs.TrySetResult(deferredHandle);
            }

            await loadTask;
        }

        // ──────────────── LoadScene：Single 模式自动卸载旧场景 ────────────────

        [Fact]
        public async Task LoadScene_Single_ReleasesLoadedAdditiveScenes()
        {
            var handleA = CreateSceneHandle(SceneA, SceneLoadMode.Additive, EOperationStatus.Succeed, null);
            var handleB = CreateSceneHandle(SceneB, SceneLoadMode.Single, EOperationStatus.Succeed, null);
            _assetManager.OnLoadSceneAsync = (path, mode, activate) => Task.FromResult(mode == SceneLoadMode.Single ? handleB : handleA);
            await _manager.LoadScene(SceneA, SceneLoadMode.Additive, null);

            await _manager.LoadScene(SceneB, SceneLoadMode.Single, null);

            // 旧 Additive 场景：立即移出 loaded，进入 unloading（完成回调由 OperationSystem 驱动）。
            Assert.False(_manager.SceneIsLoaded(SceneA), "Additive scene must leave loaded on Single load");
            Assert.True(_manager.SceneIsUnloading(SceneA), "Additive scene must be unloading after Single load");
            Assert.Contains(SceneA, _manager.GetUnloadingSceneAssetNames());
            Assert.True(_manager.SceneIsLoaded(SceneB), "New Single scene should be loaded");
        }

        [Fact]
        public async Task LoadScene_Single_ReloadSameScene_FiresUnloadSuccessThenReloads()
        {
            var handle1 = CreateSceneHandle(SceneA, SceneLoadMode.Single, EOperationStatus.Succeed, null);
            var handle2 = CreateSceneHandle(SceneA, SceneLoadMode.Single, EOperationStatus.Succeed, null);
            var loadCount = 0;
            _assetManager.OnLoadSceneAsync = (path, mode, activate) =>
            {
                loadCount++;
                return Task.FromResult(loadCount == 1 ? handle1 : handle2);
            };
            await _manager.LoadScene(SceneA, SceneLoadMode.Single, null);

            var unloadEvents = new List<string>();
            _manager.UnloadSceneSuccess += (sender, args) => unloadEvents.Add(args.SceneAssetName);

            await _manager.LoadScene(SceneA, SceneLoadMode.Single, null);

            // Single 场景被 assetsystem 视为主场景，UnloadAsync 会被拒绝，
            // 框架直接触发 UnloadSceneSuccess 事件让订阅方同步状态，然后重新加载。
            Assert.Contains(SceneA, unloadEvents);
            Assert.True(_manager.SceneIsLoaded(SceneA), "Scene should be reloaded after Single reload");
        }

        // ──────────────── UnloadScene ────────────────

        [Fact]
        public void UnloadScene_NotLoaded_Throws()
        {
            Assert.Throws<GameFrameworkException>(() => _manager.UnloadScene(SceneA));
        }

        [Fact]
        public async Task UnloadScene_WhileLoading_Throws()
        {
            var tcs = new TaskCompletionSource<SceneHandle>();
            _assetManager.OnLoadSceneAsync = (path, mode, activate) => tcs.Task;
            var deferredHandle = CreateSceneHandle(SceneA, SceneLoadMode.Additive, EOperationStatus.Succeed, null);
            var loadTask = _manager.LoadScene(SceneA, SceneLoadMode.Additive, null);

            try
            {
                Assert.Throws<GameFrameworkException>(() => _manager.UnloadScene(SceneA));
            }
            finally
            {
                tcs.TrySetResult(deferredHandle);
            }

            await loadTask;
        }

        [Fact]
        public async Task UnloadScene_Loaded_MovesToUnloading()
        {
            var handle = CreateSceneHandle(SceneA, SceneLoadMode.Additive, EOperationStatus.Succeed, null);
            _assetManager.OnLoadSceneAsync = (path, mode, activate) => Task.FromResult(handle);
            await _manager.LoadScene(SceneA, SceneLoadMode.Additive, null);

            var unloadSuccessFired = false;
            _manager.UnloadSceneSuccess += (sender, args) => unloadSuccessFired = true;

            _manager.UnloadScene(SceneA);

            // 卸载是异步操作：同步阶段仅完成 loaded -> unloading 的迁移。
            Assert.False(_manager.SceneIsLoaded(SceneA), "Scene must leave loaded immediately on unload");
            Assert.True(_manager.SceneIsUnloading(SceneA), "Scene must be unloading");
            Assert.False(unloadSuccessFired, "Unload success event fires after operation completes, not synchronously");
            Assert.Contains(SceneA, _manager.GetUnloadingSceneAssetNames());
        }

        [Fact]
        public async Task UnloadScene_OperationCompletes_RemovesFromUnloadingAndFiresEvent()
        {
            var handle = CreateSceneHandle(SceneA, SceneLoadMode.Additive, EOperationStatus.Succeed, null);
            _assetManager.OnLoadSceneAsync = (path, mode, activate) => Task.FromResult(handle);
            await _manager.LoadScene(SceneA, SceneLoadMode.Additive, null);

            string failureEventSceneName = null;
            _manager.UnloadSceneFailure += (sender, args) => failureEventSceneName = args.SceneAssetName;

            // OperationSystem 为 internal 类，用反射驱动其纯 C# 的初始化与单帧更新。
            // 先 DestroyAll 清空静态队列：前序用例 Shutdown 遗留的卸载操作若被本次 Update 一并驱动，
            // 其失败回调在各自管理器上无订阅者会抛 GameFrameworkException，污染本用例。
            // 必须在本用例 UnloadScene 之前清，否则会连本用例自己的卸载操作一起清掉。
            var operationSystemType = typeof(SceneHandle).Assembly.GetType("GameFrameX.AssetSystem.OperationSystem");
            Assert.NotNull(operationSystemType);
            operationSystemType.GetMethod("DestroyAll", BindingFlags.Public | BindingFlags.Static).Invoke(null, null);
            operationSystemType.GetMethod("Initialize", BindingFlags.Public | BindingFlags.Static).Invoke(null, null);

            _manager.UnloadScene(SceneA);

            // 测试环境无场景节点，UnloadSceneOperation 完成时会判定失败并回调（引擎内为成功路径）。
            // 这里验证的是：unloading 条目在操作完成后被清理，事件被派发。
            operationSystemType.GetMethod("Update", BindingFlags.Public | BindingFlags.Static).Invoke(null, null);

            Assert.False(_manager.SceneIsUnloading(SceneA), "Unloading entry must be cleaned after operation completes");
            Assert.Equal(SceneA, failureEventSceneName);
        }

        [Fact]
        public async Task UnloadScene_Unloading_Duplicate_Throws()
        {
            var handle = CreateSceneHandle(SceneA, SceneLoadMode.Additive, EOperationStatus.Succeed, null);
            _assetManager.OnLoadSceneAsync = (path, mode, activate) => Task.FromResult(handle);
            // 直接驱动：加载成功后手动卸载一次进入 unloading 状态
            await _manager.LoadScene(SceneA, SceneLoadMode.Additive, null);
            _manager.UnloadScene(SceneA);

            Assert.Throws<GameFrameworkException>(() => _manager.UnloadScene(SceneA));
        }

        // ──────────────── Shutdown ────────────────

        [Fact]
        public async Task Shutdown_UnloadsLoadedScenes()
        {
            var handleA = CreateSceneHandle(SceneA, SceneLoadMode.Additive, EOperationStatus.Succeed, null);
            var handleB = CreateSceneHandle(SceneB, SceneLoadMode.Additive, EOperationStatus.Succeed, null);
            _assetManager.OnLoadSceneAsync = (path, mode, activate) => Task.FromResult(path == SceneA ? handleA : handleB);
            await _manager.LoadScene(SceneA, SceneLoadMode.Additive, null);
            await _manager.LoadScene(SceneB, SceneLoadMode.Additive, null);

            _manager.Shutdown();

            Assert.False(_manager.SceneIsLoaded(SceneA));
            Assert.False(_manager.SceneIsLoaded(SceneB));
            Assert.True(_manager.SceneIsUnloading(SceneA), "Shutdown should move loaded scenes into unloading");
            Assert.True(_manager.SceneIsUnloading(SceneB), "Shutdown should move loaded scenes into unloading");
        }

        // ──────────────── 前置校验 ────────────────

        [Fact]
        public async Task LoadScene_WithoutResourceManager_Throws()
        {
            var manager = new GameSceneManager();
            await Assert.ThrowsAsync<GameFrameworkException>(() => manager.LoadScene(SceneA));
            manager.Shutdown();
        }

        [Fact]
        public void SetResourceManager_Null_Throws()
        {
            var manager = new GameSceneManager();
            Assert.Throws<GameFrameworkException>(() => manager.SetResourceManager(null));
            manager.Shutdown();
        }

        [Fact]
        public void SceneIsLoaded_NullName_Throws()
        {
            Assert.Throws<GameFrameworkException>(() => _manager.SceneIsLoaded(null));
        }

        // ──────────────── HasScene / GetSceneHandle / 查询 ────────────────

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void HasScene_EmptyName_ReturnsFalse(string sceneName)
        {
            Assert.False(_manager.HasScene(sceneName));
        }

        [Fact]
        public void HasScene_NoResourceManager_ReturnsFalse()
        {
            var manager = new GameSceneManager();
            Assert.False(manager.HasScene(SceneA));
            manager.Shutdown();
        }

        [Fact]
        public void HasScene_ThroughAssetManager()
        {
            _assetManager.OnHasAssetPath = path => path == SceneA;

            Assert.True(_manager.HasScene(SceneA));
            Assert.False(_manager.HasScene(SceneB));
        }

        [Fact]
        public async Task GetSceneHandle_ReturnsHandleForLoadedScene()
        {
            var handle = CreateSceneHandle(SceneA, SceneLoadMode.Additive, EOperationStatus.Succeed, null);
            _assetManager.OnLoadSceneAsync = (path, mode, activate) => Task.FromResult(handle);
            await _manager.LoadScene(SceneA, SceneLoadMode.Additive, null);

            Assert.Same(handle, _manager.GetSceneHandle(SceneA));
            Assert.Null(_manager.GetSceneHandle(SceneB));
        }

        [Fact]
        public async Task GetNames_ListOverloads_ReturnCurrentState()
        {
            var handleA = CreateSceneHandle(SceneA, SceneLoadMode.Additive, EOperationStatus.Succeed, null);
            _assetManager.OnLoadSceneAsync = (path, mode, activate) => Task.FromResult(handleA);
            await _manager.LoadScene(SceneA, SceneLoadMode.Additive, null);

            var tcs = new TaskCompletionSource<SceneHandle>();
            _assetManager.OnLoadSceneAsync = (path, mode, activate) => tcs.Task;
            var loadTask = _manager.LoadScene(SceneB, SceneLoadMode.Additive, null);

            var loaded = new List<string>();
            _manager.GetLoadedSceneAssetNames(loaded);
            var loadingNames = new List<string>();
            _manager.GetLoadingSceneAssetNames(loadingNames);

            Assert.Equal(new List<string> { SceneA }, loaded);
            Assert.Equal(new List<string> { SceneB }, loadingNames);
            Assert.Empty(_manager.GetUnloadingSceneAssetNames());

            // 断言完成后释放挂起的加载，避免永不完成的 Task 跨用例泄漏。
            tcs.TrySetResult(CreateSceneHandle(SceneB, SceneLoadMode.Additive, EOperationStatus.Succeed, null));
            await loadTask;
        }

        #region 测试辅助

        /// <summary>
        /// 空实现日志器：阻止 assetsystem 在无 Godot 原生运行时的测试宿主里兜底调用 GD.PushError/PushWarning 导致段错误。
        /// </summary>
        private sealed class NoOpLogger : GameFrameX.AssetSystem.ILogger
        {
            public void Log(string message)
            {
            }

            public void Warning(string message)
            {
            }

            public void Error(string message)
            {
            }

            public void Exception(Exception exception)
            {
            }
        }

        /// <summary>
        /// 通过反射构造 SceneHandle：
        /// AssetInfo(internal ctor) + DatabaseSceneProvider(internal class, 空 providerGUID 时不触碰引擎) + SceneHandle(internal ctor)。
        /// </summary>
        private static SceneHandle CreateSceneHandle(string assetPath, SceneLoadMode mode, EOperationStatus status, string error)
        {
            var packageAsset = new PackageAsset { AssetPath = assetPath };
            var assetInfo = (AssetInfo)InvokeConstructor(typeof(AssetInfo), "TestPackage", packageAsset, null);

            var providerType = typeof(SceneHandle).Assembly.GetType("GameFrameX.AssetSystem.DatabaseSceneProvider");
            Assert.NotNull(providerType);
            var provider = InvokeConstructor(providerType, null, string.Empty, assetInfo, new SceneLoadParameters(mode), false);

            if (status != EOperationStatus.None)
            {
                SetProtectedProperty(provider, "Status", status);
            }

            if (string.IsNullOrEmpty(error) == false)
            {
                SetProtectedProperty(provider, "Error", error);
            }

            return (SceneHandle)InvokeConstructor(typeof(SceneHandle), provider);
        }

        private static object InvokeConstructor(Type type, params object[] args)
        {
            var constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var constructor = constructors.Single(c => c.GetParameters().Length == args.Length);
            return constructor.Invoke(args);
        }

        private static void SetProtectedProperty(object target, string propertyName, object value)
        {
            var property = typeof(AsyncOperationBase).GetProperty(propertyName);
            property.GetSetMethod(true).Invoke(target, new object[] { value });
        }

        /// <summary>
        /// IAssetManager 测试替身：仅实现 LoadSceneAsync / HasAssetPath，其余成员抛 NotSupportedException。
        /// </summary>
        private sealed class FakeAssetManager : IAssetManager
        {
            public Func<string, SceneLoadMode, bool, Task<SceneHandle>> OnLoadSceneAsync;
            public Func<string, bool> OnHasAssetPath;

            public int DownloadingMaxNum { get; set; }

            public int FailedTryAgain { get; set; }

            public string DefaultPackageName { get; set; }

            public EPlayMode PlayMode
            {
                get { throw Unsupported(); }
            }

            public EFileVerifyLevel VerifyLevel
            {
                get { throw Unsupported(); }
            }

            public long Milliseconds { get; set; }

            public void SetPlayMode(EPlayMode playMode)
            {
                throw Unsupported();
            }

            public void Initialize()
            {
                throw Unsupported();
            }

            public Task<bool> InitPackageAsync(string packageName, string hostServerURL, string fallbackHostServerURL, bool isDefaultPackage = true)
            {
                throw Unsupported();
            }

            public void UnloadAsset(string assetPath)
            {
                throw Unsupported();
            }

            public Task<SubAssetsHandle> LoadSubAssetsAsync(AssetInfo assetInfo)
            {
                throw Unsupported();
            }

            public Task<SubAssetsHandle> LoadSubAssetsAsync(string path, Type type)
            {
                throw Unsupported();
            }

            public Task<SubAssetsHandle> LoadSubAssetsAsync<T>(string path) where T : Resource
            {
                throw Unsupported();
            }

            public SubAssetsHandle LoadSubAssetSync(AssetInfo assetInfo)
            {
                throw Unsupported();
            }

            public SubAssetsHandle LoadSubAssetSync(string path, Type type)
            {
                throw Unsupported();
            }

            public SubAssetsHandle LoadSubAssetSync<T>(string path) where T : Resource
            {
                throw Unsupported();
            }

            public Task<RawFileHandle> LoadRawFileAsync(AssetInfo assetInfo)
            {
                throw Unsupported();
            }

            public Task<RawFileHandle> LoadRawFileAsync(string path)
            {
                throw Unsupported();
            }

            public RawFileHandle LoadRawFileSync(AssetInfo assetInfo)
            {
                throw Unsupported();
            }

            public RawFileHandle LoadRawFileSync(string path)
            {
                throw Unsupported();
            }

            public Task<AssetHandle> LoadAssetAsync(AssetInfo assetInfo)
            {
                throw Unsupported();
            }

            public Task<AssetHandle> LoadAssetAsync(string path, Type type)
            {
                throw Unsupported();
            }

            public Task<AssetHandle> LoadAssetAsync<T>(string path) where T : Resource
            {
                throw Unsupported();
            }

            public Task<AllAssetsHandle> LoadAllAssetsAsync<T>(string path) where T : Resource
            {
                throw Unsupported();
            }

            public Task<AllAssetsHandle> LoadAllAssetsAsync(string path, Type type)
            {
                throw Unsupported();
            }

            public Task<AllAssetsHandle> LoadAllAssetsAsync(string path)
            {
                throw Unsupported();
            }

            public Task<AllAssetsHandle> LoadAllAssetsAsync(AssetInfo assetInfo)
            {
                throw Unsupported();
            }

            public Task<AssetHandle> LoadAssetAsync(string path)
            {
                throw Unsupported();
            }

            public SubAssetsHandle LoadSubAssetsAsync(string path)
            {
                throw Unsupported();
            }

            public AllAssetsHandle LoadAllAssetsSync(string path)
            {
                throw Unsupported();
            }

            public AllAssetsHandle LoadAllAssetsSync<T>(string path) where T : Resource
            {
                throw Unsupported();
            }

            public AllAssetsHandle LoadAllAssetsSync(string path, Type type)
            {
                throw Unsupported();
            }

            public AllAssetsHandle LoadAllAssetsSync(AssetInfo assetInfo)
            {
                throw Unsupported();
            }

            public SubAssetsHandle LoadSubAssetSync(string path)
            {
                throw Unsupported();
            }

            public AssetHandle LoadAssetSync(string path)
            {
                throw Unsupported();
            }

            public AssetHandle LoadAssetSync(string path, Type type)
            {
                throw Unsupported();
            }

            public AssetHandle LoadAssetSync(AssetInfo assetInfo)
            {
                throw Unsupported();
            }

            public AssetHandle LoadAssetSync<T>(string path) where T : Resource
            {
                throw Unsupported();
            }

            public Task<SceneHandle> LoadSceneAsync(string path, SceneLoadMode sceneMode, bool activateOnLoad = true)
            {
                if (OnLoadSceneAsync == null)
                {
                    throw Unsupported();
                }

                return OnLoadSceneAsync(path, sceneMode, activateOnLoad);
            }

            public Task<SceneHandle> LoadSceneAsync(AssetInfo assetInfo, SceneLoadMode sceneMode, bool activateOnLoad = true)
            {
                throw Unsupported();
            }

            public ResourcePackage CreateAssetsPackage(string packageName)
            {
                throw Unsupported();
            }

            public ResourcePackage TryGetAssetsPackage(string packageName)
            {
                throw Unsupported();
            }

            public bool HasAssetsPackage(string packageName)
            {
                throw Unsupported();
            }

            public ResourcePackage GetAssetsPackage(string packageName)
            {
                throw Unsupported();
            }

            public bool IsNeedDownload(AssetInfo assetInfo)
            {
                throw Unsupported();
            }

            public bool IsNeedDownload(string path)
            {
                throw Unsupported();
            }

            public AssetInfo[] GetAssetInfos(string[] assetTags)
            {
                throw Unsupported();
            }

            public AssetInfo[] GetAssetInfos(string assetTag)
            {
                throw Unsupported();
            }

            public AssetInfo GetAssetInfo(string path)
            {
                throw Unsupported();
            }

            public bool HasAssetPath(string assetPath)
            {
                return OnHasAssetPath != null && OnHasAssetPath(assetPath);
            }

            public void SetDefaultAssetsPackage(ResourcePackage resourcePackage)
            {
                throw Unsupported();
            }

            public void ClearUnusedBundleFilesAsync(string packageName = null)
            {
                throw Unsupported();
            }

            public void ClearAllBundleFilesAsync(string packageName = null)
            {
                throw Unsupported();
            }

            public void UnloadUnusedAssetsAsync(string packageName = null)
            {
                throw Unsupported();
            }

            public void UnloadAllAssetsAsync(string packageName = null)
            {
                throw Unsupported();
            }

            public void UnloadAsset(string packageName, string assetPath)
            {
                throw Unsupported();
            }

            private static NotSupportedException Unsupported()
            {
                return new NotSupportedException("FakeAssetManager only supports LoadSceneAsync/HasAssetPath.");
            }
        }

        #endregion
    }
}
