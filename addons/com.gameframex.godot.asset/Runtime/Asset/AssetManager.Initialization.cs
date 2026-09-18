using System;
using GameFrameX.AssetSystem;
using GameFrameX.Runtime;

namespace GameFrameX.Asset.Runtime
{
    public partial class AssetManager
    {
        public const string ConstDefaultPackageName = "DefaultPackage";

        /// <summary>
        /// 根据运行模式创建初始化操作数据
        /// </summary>
        /// <returns></returns>
        private InitializationOperation CreateInitializationOperationHandler(ResourcePackage resourcePackage, string hostServerURL, string fallbackHostServerURL)
        {
            switch (PlayMode)
            {
                case EPlayMode.EditorSimulateMode:
                {
                    // 编辑器下的模拟模式
                    return InitializeEditorSimulateMode(resourcePackage);
                }
                case EPlayMode.OfflinePlayMode:
                {
                    // 单机运行模式
                    return InitializeOfflinePlayMode(resourcePackage);
                }
                case EPlayMode.HostPlayMode:
                {
                    // 联机运行模式
                    return InitializeHostPlayMode(resourcePackage, hostServerURL, fallbackHostServerURL);
                }
                case EPlayMode.WebPlayMode:
                {
                    // WebGL运行模式
                    return InitializeWebPlayMode(resourcePackage, hostServerURL, fallbackHostServerURL);
                }
                default:
                {
                    throw new ArgumentOutOfRangeException(nameof(PlayMode), PlayMode, $"Unsupported play mode: {PlayMode}");
                }
            }
        }

        /// <summary>
        /// 初始化资源系统编辑器模拟运行模式
        /// </summary>
        /// <param name="resourcePackage">资源包</param>
        /// <returns></returns>
        private InitializationOperation InitializeEditorSimulateMode(ResourcePackage resourcePackage)
        {
            var simulateBuildResult = EditorSimulateModeHelper.SimulateBuild(nameof(EDefaultBuildPipeline.BuiltinBuildPipeline), ConstDefaultPackageName);
            var createParameters = new EditorSimulateModeParameters();
            createParameters.EditorFileSystemParameters = FileSystemParameters.CreateDefaultEditorFileSystemParameters(simulateBuildResult);
            return resourcePackage.InitializeAsync(createParameters);
        }

        /// <summary>
        /// 初始化资源系统单机运行模式
        /// </summary>
        /// <param name="resourcePackage">资源包</param>
        /// <returns></returns>
        private InitializationOperation InitializeOfflinePlayMode(ResourcePackage resourcePackage)
        {
            var buildinFileSystem = FileSystemParameters.CreateDefaultBuildinFileSystemParameters();
            var initParameters = new OfflinePlayModeParameters();
            initParameters.BuildinFileSystemParameters = buildinFileSystem;
            return resourcePackage.InitializeAsync(initParameters);
        }

        /// <summary>
        /// 初始化资源系统 WebGL 运行模式
        /// </summary>
        /// <param name="resourcePackage">资源包</param>
        /// <param name="hostServerURL">主机服务器URL</param>
        /// <param name="fallbackHostServerURL">备用主机服务器URL</param>
        /// <returns></returns>
        private InitializationOperation InitializeWebPlayMode(ResourcePackage resourcePackage, string hostServerURL, string fallbackHostServerURL)
        {
            // 说明：Godot 侧无微信/抖音/快手小游戏条件编译分支，统一使用默认 Web 文件系统。
            var initParameters = new WebPlayModeParameters();
            initParameters.WebFileSystemParameters = FileSystemParameters.CreateDefaultWebFileSystemParameters();
            return resourcePackage.InitializeAsync(initParameters);
        }

        /// <summary>
        /// 初始化资源系统热更新运行模式
        /// </summary>
        /// <param name="resourcePackage">资源包</param>
        /// <param name="hostServerURL">主机服务器URL</param>
        /// <param name="fallbackHostServerURL">备用主机服务器URL</param>
        /// <returns></returns>
        private InitializationOperation InitializeHostPlayMode(ResourcePackage resourcePackage, string hostServerURL, string fallbackHostServerURL)
        {
            var remoteServices = new RemoteServices(hostServerURL, fallbackHostServerURL);
            var createParameters = new HostPlayModeParameters
            {
                BuildinFileSystemParameters = FileSystemParameters.CreateDefaultBuildinFileSystemParameters(),
                CacheFileSystemParameters = FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices),
            };
            return resourcePackage.InitializeAsync(createParameters);
        }
    }
}
