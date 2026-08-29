using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using GameFrameX.AssetSystem;
using Godot;

namespace GameFrameX.EngineTests
{
    /// <summary>
    /// 引擎测试资产包 fixture（编辑器/游戏双模式共用同一份构建产物布局）：
    /// 1. 按构建产物布局写入 user://assetsystem_builds/&lt;pkg&gt;/BuiltinBuildPipeline/
    ///    （build_manifest.txt 标记 + PackageManifest_*.version/.hash/.bytes + 每资产一 bundle 物理文件）；
    /// 2. 编辑器上下文走 EditorSimulate（SimulateBuild 定位 + EditorSimulate 初始化）；
    ///    游戏上下文走 HostPlayMode（本地目录 IRemoteServices + 本地文件 IHttpTransport + 缓存目录 + 全量下载）。
    /// </summary>
    public static class TestAssetPackageFixture
    {
        public const string PackageName = "engine_test";
        public const string PackageVersion = "v1";
        public const string SceneAAddress = "test_scene";
        public const string SceneBAddress = "test_scene_b";
        public const string ResourceAddress = "test_resource";
        public const string TextureAddress = "test_png";
        public const string AudioAddress = "test_audio";
        public const string BrokenSceneAddress = "broken_scene";
        public const string BrokenAssetAddress = "broken_asset";
        public const string SceneAPath = "res://tests/engine_test_assets/TestScene.tscn";
        public const string SceneBPath = "res://tests/engine_test_assets/TestSceneB.tscn";
        public const string BrokenScenePath = "res://tests/engine_test_assets/pck_only/never_exists.tscn";

        private static ResourcePackage _package;

        public static async Task<ResourcePackage> GetPackageAsync()
        {
            if (_package != null)
            {
                return _package;
            }

            WriteFixture();
            global::GameFrameX.AssetSystem.AssetSystem.Initialize();
            var package = global::GameFrameX.AssetSystem.AssetSystem.TryGetPackage(PackageName) ?? global::GameFrameX.AssetSystem.AssetSystem.CreatePackage(PackageName);
            global::GameFrameX.AssetSystem.AssetSystem.SetDefaultPackage(package);
            if (Engine.IsEditorHint())
            {
                await InitializeEditorSimulateAsync(package);
            }
            else
            {
                await InitializeHostPlayModeAsync(package);
            }

            _package = package;
            return package;
        }

        public static void RequireEditorSimulateSupport()
        {
            if (EngineTestEnvironment.HasToolsDefine == false)
            {
                throw new EngineTestSkippedException("当前程序集非 TOOLS 构建，EditorSimulate 仅编辑器工具构建可用");
            }

            if (Engine.IsEditorHint() == false)
            {
                throw new EngineTestSkippedException("当前非编辑器运行上下文，ResourcePackage 拒绝 EditorSimulate 初始化");
            }
        }

        private static async Task InitializeEditorSimulateAsync(ResourcePackage package)
        {
            RequireEditorSimulateSupport();
            var simulateBuildResult = EditorSimulateModeHelper.SimulateBuild(nameof(EDefaultBuildPipeline.BuiltinBuildPipeline), PackageName);
            if (string.IsNullOrEmpty(simulateBuildResult.PackageRootDirectory))
            {
                throw new EngineTestFailureException("SimulateBuild 未定位到 fixture 包根目录");
            }

            // 首次失败后重入时包可能已完成初始化，重复 InitializeAsync 会被拒绝
            if (package.InitializeStatus == EOperationStatus.None)
            {
                var createParameters = new EditorSimulateModeParameters();
                createParameters.EditorFileSystemParameters = FileSystemParameters.CreateDefaultEditorFileSystemParameters(simulateBuildResult);
                await EngineTestWait.OperationAsync(package.InitializeAsync(createParameters), "EditorSimulate Initialize");
            }

            var versionOperation = package.RequestPackageVersionAsync(appendTimeTicks: false, timeout: 10);
            await EngineTestWait.OperationAsync(versionOperation, "EditorSimulate RequestPackageVersion");
            var manifestOperation = package.UpdatePackageManifestAsync(versionOperation.PackageVersion, timeout: 10);
            await EngineTestWait.OperationAsync(manifestOperation, "EditorSimulate UpdatePackageManifest");
        }

        private static async Task InitializeHostPlayModeAsync(ResourcePackage package)
        {
            var packageRoot = GetPackageRoot();
            var cacheRoot = ProjectSettings.GlobalizePath("user://engine_tests/host_cache").Replace('\\', '/');
            ResetDirectory(cacheRoot);
            global::GameFrameX.AssetSystem.AssetSystem.SetDownloadSystemHttpTransport(new LocalFileHttpTransport());
            var remoteServices = new LocalDirectoryRemoteServices(packageRoot);
            if (package.InitializeStatus == EOperationStatus.None)
            {
                var createParameters = new HostPlayModeParameters();
                createParameters.CacheFileSystemParameters = FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices, rootDirectory: cacheRoot);
                await EngineTestWait.OperationAsync(package.InitializeAsync(createParameters), "HostPlayMode Initialize");
            }

            var versionOperation = package.RequestPackageVersionAsync(appendTimeTicks: false, timeout: 10);
            await EngineTestWait.OperationAsync(versionOperation, "HostPlayMode RequestPackageVersion");
            var manifestOperation = package.UpdatePackageManifestAsync(versionOperation.PackageVersion, timeout: 10);
            await EngineTestWait.OperationAsync(manifestOperation, "HostPlayMode UpdatePackageManifest");
            var downloader = package.CreateResourceDownloader(downloadingMaxNumber: 4, failedTryAgain: 1, timeout: 30);
            downloader.BeginDownload();
            await EngineTestWait.OperationAsync(downloader, "HostPlayMode DownloadRemoteContent");
        }

        private static void WriteFixture()
        {
            var packageRoot = GetPackageRoot();
            ResetDirectory(packageRoot);
            var assets = new List<ManifestFixtureWriter.FixtureAsset>();
            var bundles = new List<ManifestFixtureWriter.FixtureBundle>();
            AddEntry(assets, bundles, SceneAAddress, ResPath("TestScene.tscn"), "scene_test_scene.bundle", packageRoot, false);
            AddEntry(assets, bundles, SceneBAddress, ResPath("TestSceneB.tscn"), "scene_test_scene_b.bundle", packageRoot, false);
            AddEntry(assets, bundles, ResourceAddress, ResPath("test_resource.tres"), "res_test_resource.bundle", packageRoot, false);
            AddEntry(assets, bundles, TextureAddress, ResPath("tiny_4x4.png"), "tex_test_png.bundle", packageRoot, false);
            AddEntry(assets, bundles, AudioAddress, ResPath("test_audio_mp3.tres"), "aud_test_audio.bundle", packageRoot, false);
            AddEntry(assets, bundles, BrokenSceneAddress, ResPath("pck_only/never_exists.tscn"), "broken_scene.bundle", packageRoot, true);
            AddEntry(assets, bundles, BrokenAssetAddress, ResPath("pck_only/never_exists.tres"), "broken_asset.bundle", packageRoot, true);
            var manifestBytes = ManifestFixtureWriter.WriteManifestBinary("BuiltinBuildPipeline", PackageName, PackageVersion, assets, bundles);
            File.WriteAllText(Path.Combine(packageRoot, "build_manifest.txt"), "engine test simulate build marker " + PackageVersion, System.Text.Encoding.UTF8);
            File.WriteAllText(Path.Combine(packageRoot, AssetSystemSettingsData.GetPackageVersionFileName(PackageName)), PackageVersion, System.Text.Encoding.UTF8);
            File.WriteAllText(Path.Combine(packageRoot, AssetSystemSettingsData.GetPackageHashFileName(PackageName, PackageVersion)), ManifestFixtureWriter.ToLowerMd5(manifestBytes), System.Text.Encoding.UTF8);
            File.WriteAllBytes(Path.Combine(packageRoot, AssetSystemSettingsData.GetManifestBinaryFileName(PackageName, PackageVersion)), manifestBytes);
        }

        private static void AddEntry(List<ManifestFixtureWriter.FixtureAsset> assets, List<ManifestFixtureWriter.FixtureBundle> bundles, string address, string resPath, string bundleName, string packageRoot, bool broken)
        {
            var bundleId = bundles.Count;
            string sourceFilePath;
            if (broken)
            {
                // broken 条目：工程内不存在源资源；写一段不可加载的字节数据作为远端 bundle 文件，
                // 清单 hash/crc/size 与物理文件一致（游戏模式可下载成功），但任何资源加载必然失败
                var garbageBytes = Encoding.UTF8.GetBytes("engine-test broken bundle payload: " + bundleName);
                var garbageSourceDir = ProjectSettings.GlobalizePath("user://engine_tests/broken_sources").Replace('\\', '/');
                Directory.CreateDirectory(garbageSourceDir);
                var garbageSourcePath = Path.Combine(garbageSourceDir, bundleName);
                File.WriteAllBytes(garbageSourcePath, garbageBytes);
                File.WriteAllBytes(Path.Combine(packageRoot, bundleName), garbageBytes);
                sourceFilePath = garbageSourcePath;
            }
            else
            {
                sourceFilePath = ProjectSettings.GlobalizePath(resPath);
                if (File.Exists(sourceFilePath) == false)
                {
                    throw new EngineTestFailureException("fixture 源文件不存在：" + sourceFilePath);
                }

                File.Copy(sourceFilePath, Path.Combine(packageRoot, bundleName), true);
            }

            bundles.Add(new ManifestFixtureWriter.FixtureBundle(bundleName, sourceFilePath));
            assets.Add(new ManifestFixtureWriter.FixtureAsset(address, resPath, bundleId));
        }

        private static string GetPackageRoot()
        {
            return Path.Combine(ProjectSettings.GlobalizePath("user://assetsystem_builds"), PackageName, "BuiltinBuildPipeline").Replace('\\', '/');
        }

        private static string ResPath(string fileName)
        {
            return "res://tests/engine_test_assets/" + fileName;
        }

        private static void ResetDirectory(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }

            Directory.CreateDirectory(directoryPath);
        }
    }
}
