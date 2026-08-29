using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameFrameX.Asset.Runtime;
using GameFrameX.AssetSystem;
using Godot;

namespace GameFrameX.EngineTests
{
    /// <summary>
    /// 引擎测试专用 IAssetManager 桥：直连真实 ResourcePackage（EditorSimulate 初始化），
    /// 供 GameSceneManager / SoundManager 走真实资产加载链路。
    /// ponytail: 仅实现引擎测试触达的成员（场景/资产/卸载），其余抛 NotSupportedException；
    /// 若后续用例扩展到子资源/原生文件，再补真实转发。
    /// </summary>
    public sealed class EngineTestAssetManager : IAssetManager
    {
        private readonly ResourcePackage _package;
        private readonly Dictionary<string, AssetHandle> _loadedHandles = new Dictionary<string, AssetHandle>();

        public EngineTestAssetManager(ResourcePackage package)
        {
            _package = package;
        }

        public int DownloadingMaxNum { get; set; }

        public int FailedTryAgain { get; set; }

        public string DefaultPackageName { get; set; }

        public EPlayMode PlayMode
        {
            get { return EPlayMode.EditorSimulateMode; }
        }

        public EFileVerifyLevel VerifyLevel
        {
            get { return EFileVerifyLevel.Middle; }
        }

        public long Milliseconds { get; set; }

        public void SetPlayMode(EPlayMode playMode)
        {
        }

        public void Initialize()
        {
        }

        public Task<bool> InitPackageAsync(string packageName, string hostServerURL, string fallbackHostServerURL, bool isDefaultPackage = true)
        {
            throw new NotSupportedException("engine test bridge: InitPackageAsync 未使用");
        }

        public void UnloadAsset(string assetPath)
        {
            AssetHandle handle;
            if (_loadedHandles.TryGetValue(assetPath, out handle))
            {
                _loadedHandles.Remove(assetPath);
                if (handle != null && handle.IsValid)
                {
                    handle.Release();
                }
            }
        }

        public Task<SubAssetsHandle> LoadSubAssetsAsync(AssetInfo assetInfo)
        {
            throw NotSupported();
        }

        public Task<SubAssetsHandle> LoadSubAssetsAsync(string path, Type type)
        {
            throw NotSupported();
        }

        public Task<SubAssetsHandle> LoadSubAssetsAsync<T>(string path) where T : Resource
        {
            throw NotSupported();
        }

        public SubAssetsHandle LoadSubAssetSync(AssetInfo assetInfo)
        {
            throw NotSupported();
        }

        public SubAssetsHandle LoadSubAssetSync(string path, Type type)
        {
            throw NotSupported();
        }

        public SubAssetsHandle LoadSubAssetSync<T>(string path) where T : Resource
        {
            throw NotSupported();
        }

        public Task<RawFileHandle> LoadRawFileAsync(AssetInfo assetInfo)
        {
            throw NotSupported();
        }

        public Task<RawFileHandle> LoadRawFileAsync(string path)
        {
            throw NotSupported();
        }

        public RawFileHandle LoadRawFileSync(AssetInfo assetInfo)
        {
            throw NotSupported();
        }

        public RawFileHandle LoadRawFileSync(string path)
        {
            throw NotSupported();
        }

        public Task<AllAssetsHandle> LoadAllAssetsAsync<T>(string path) where T : Resource
        {
            throw NotSupported();
        }

        public Task<AllAssetsHandle> LoadAllAssetsAsync(string path, Type type)
        {
            throw NotSupported();
        }

        public Task<AllAssetsHandle> LoadAllAssetsAsync(string path)
        {
            throw NotSupported();
        }

        public Task<AllAssetsHandle> LoadAllAssetsAsync(AssetInfo assetInfo)
        {
            throw NotSupported();
        }

        public SubAssetsHandle LoadSubAssetsAsync(string path)
        {
            throw NotSupported();
        }

        public AllAssetsHandle LoadAllAssetsSync(string path)
        {
            throw NotSupported();
        }

        public AllAssetsHandle LoadAllAssetsSync<T>(string path) where T : Resource
        {
            throw NotSupported();
        }

        public AllAssetsHandle LoadAllAssetsSync(string path, Type type)
        {
            throw NotSupported();
        }

        public AllAssetsHandle LoadAllAssetsSync(AssetInfo assetInfo)
        {
            throw NotSupported();
        }

        public SubAssetsHandle LoadSubAssetSync(string path)
        {
            throw NotSupported();
        }

        public AssetHandle LoadAssetSync(string path)
        {
            throw NotSupported();
        }

        public AssetHandle LoadAssetSync(string path, Type type)
        {
            throw NotSupported();
        }

        public AssetHandle LoadAssetSync(AssetInfo assetInfo)
        {
            throw NotSupported();
        }

        public AssetHandle LoadAssetSync<T>(string path) where T : Resource
        {
            throw NotSupported();
        }

        public Task<SceneHandle> LoadSceneAsync(string path, SceneLoadMode sceneMode, bool activateOnLoad = true)
        {
            var handle = _package.LoadSceneAsync(path, sceneMode, ScenePhysicsMode.None, activateOnLoad == false, 0);
            return Task.FromResult(handle);
        }

        public Task<SceneHandle> LoadSceneAsync(AssetInfo assetInfo, SceneLoadMode sceneMode, bool activateOnLoad = true)
        {
            var handle = _package.LoadSceneAsync(assetInfo, sceneMode, ScenePhysicsMode.None, activateOnLoad == false, 0);
            return Task.FromResult(handle);
        }

        public ResourcePackage CreateAssetsPackage(string packageName)
        {
            throw NotSupported();
        }

        public ResourcePackage TryGetAssetsPackage(string packageName)
        {
            return packageName == _package.PackageName ? _package : null;
        }

        public ResourcePackage GetAssetsPackage(string packageName)
        {
            return _package;
        }

        public AssetInfo[] GetAssetInfos(string[] assetTags)
        {
            throw NotSupported();
        }

        public AssetInfo[] GetAssetInfos(string assetTag)
        {
            throw NotSupported();
        }

        public AssetInfo GetAssetInfo(string path)
        {
            return _package.GetAssetInfo(path);
        }

        public bool HasAssetsPackage(string packageName)
        {
            return packageName == _package.PackageName;
        }

        public bool IsNeedDownload(AssetInfo assetInfo)
        {
            return false;
        }

        public bool IsNeedDownload(string path)
        {
            return false;
        }

        public bool HasAssetPath(string assetPath)
        {
            return _package.CheckLocationValid(assetPath);
        }

        public void SetDefaultAssetsPackage(ResourcePackage resourcePackage)
        {
        }

        public void ClearUnusedBundleFilesAsync(string packageName = null)
        {
        }

        public void ClearAllBundleFilesAsync(string packageName = null)
        {
        }

        public void UnloadUnusedAssetsAsync(string packageName = null)
        {
        }

        public void UnloadAllAssetsAsync(string packageName = null)
        {
        }

        public void UnloadAsset(string packageName, string assetPath)
        {
            UnloadAsset(assetPath);
        }

        public Task<AssetHandle> LoadAssetAsync(AssetInfo assetInfo)
        {
            throw NotSupported();
        }

        public Task<AssetHandle> LoadAssetAsync(string path, Type type)
        {
            return LoadAssetCoreAsync(path, type);
        }

        public Task<AssetHandle> LoadAssetAsync<T>(string path) where T : Resource
        {
            return LoadAssetCoreAsync(path, typeof(T));
        }

        public Task<AssetHandle> LoadAssetAsync(string path)
        {
            return LoadAssetCoreAsync(path, typeof(object));
        }

        private async Task<AssetHandle> LoadAssetCoreAsync(string path, Type type)
        {
            var handle = _package.LoadAssetAsync(path, type, 0);
            _loadedHandles[path] = handle;
            await EngineTestWait.HandleAsync(handle, "LoadAssetAsync " + path);
            return handle;
        }

        private static NotSupportedException NotSupported()
        {
            return new NotSupportedException("engine test bridge: 该成员未被引擎测试触达");
        }
    }
}
