namespace UnityEngine
{
    /// <summary>
    /// UnityEngine.Object 兼容占位类型。
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public class Object
    {
    }
}

namespace UnityEngine.SceneManagement
{
    /// <summary>
    /// 场景加载模式兼容占位枚举。
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public enum LoadSceneMode
    {
        Single = 0,
        Additive = 1
    }
}

namespace YooAsset
{
    using System;

    /// <summary>
    /// 运行模式兼容占位枚举。
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public enum EPlayMode
    {
        EditorSimulateMode = 0,
        OfflinePlayMode = 1,
        HostPlayMode = 2,
        WebPlayMode = 3
    }

    /// <summary>
    /// 文件校验等级兼容占位枚举。
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public enum EFileVerifyLevel
    {
        Low = 0,
        Middle = 1,
        High = 2
    }

    /// <summary>
    /// 资源信息兼容占位类型。
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public sealed class AssetInfo
    {
    }

    /// <summary>
    /// 子资源句柄兼容占位类型。
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public sealed class SubAssetsHandle
    {
    }

    /// <summary>
    /// 原生文件句柄兼容占位类型。
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public sealed class RawFileHandle
    {
    }

    /// <summary>
    /// 资源句柄兼容占位类型。
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public sealed class AssetHandle
    {
    }

    /// <summary>
    /// 全部资源句柄兼容占位类型。
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public sealed class AllAssetsHandle
    {
    }

    /// <summary>
    /// 场景句柄兼容占位类型。
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public sealed class SceneHandle
    {
    }

    /// <summary>
    /// 资源包兼容占位类型。
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public sealed class ResourcePackage
    {
        /// <summary>
        /// 初始化资源包异步操作。
        /// </summary>
        /// <param name="parameters">初始化参数。</param>
        /// <returns>初始化操作句柄。</returns>
        [UnityEngine.Scripting.Preserve]
        public InitializationOperation InitializeAsync(EditorSimulateModeParameters parameters)
        {
            return new InitializationOperation();
        }

        /// <summary>
        /// 初始化资源包异步操作。
        /// </summary>
        /// <param name="parameters">初始化参数。</param>
        /// <returns>初始化操作句柄。</returns>
        [UnityEngine.Scripting.Preserve]
        public InitializationOperation InitializeAsync(OfflinePlayModeParameters parameters)
        {
            return new InitializationOperation();
        }

        /// <summary>
        /// 初始化资源包异步操作。
        /// </summary>
        /// <param name="parameters">初始化参数。</param>
        /// <returns>初始化操作句柄。</returns>
        [UnityEngine.Scripting.Preserve]
        public InitializationOperation InitializeAsync(WebPlayModeParameters parameters)
        {
            return new InitializationOperation();
        }

        /// <summary>
        /// 初始化资源包异步操作。
        /// </summary>
        /// <param name="parameters">初始化参数。</param>
        /// <returns>初始化操作句柄。</returns>
        [UnityEngine.Scripting.Preserve]
        public InitializationOperation InitializeAsync(HostPlayModeParameters parameters)
        {
            return new InitializationOperation();
        }
    }

    /// <summary>
    /// 初始化操作兼容占位类型。
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public sealed class InitializationOperation
    {
    }

    /// <summary>
    /// 文件系统参数兼容占位类型。
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public sealed class FileSystemParameters
    {
        /// <summary>
        /// 创建编辑器文件系统参数。
        /// </summary>
        /// <param name="buildResult">构建结果。</param>
        /// <returns>文件系统参数。</returns>
        [UnityEngine.Scripting.Preserve]
        public static FileSystemParameters CreateDefaultEditorFileSystemParameters(string buildResult)
        {
            return new FileSystemParameters();
        }

        /// <summary>
        /// 创建内置文件系统参数。
        /// </summary>
        /// <returns>文件系统参数。</returns>
        [UnityEngine.Scripting.Preserve]
        public static FileSystemParameters CreateDefaultBuildinFileSystemParameters()
        {
            return new FileSystemParameters();
        }

        /// <summary>
        /// 创建默认Web文件系统参数。
        /// </summary>
        /// <returns>文件系统参数。</returns>
        [UnityEngine.Scripting.Preserve]
        public static FileSystemParameters CreateDefaultWebFileSystemParameters()
        {
            return new FileSystemParameters();
        }

        /// <summary>
        /// 创建默认缓存文件系统参数。
        /// </summary>
        /// <param name="remoteServices">远端服务。</param>
        /// <returns>文件系统参数。</returns>
        [UnityEngine.Scripting.Preserve]
        public static FileSystemParameters CreateDefaultCacheFileSystemParameters(IRemoteServices remoteServices)
        {
            return new FileSystemParameters();
        }
    }

    /// <summary>
    /// 编辑器模拟初始化参数兼容占位类型。
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public sealed class EditorSimulateModeParameters
    {
        /// <summary>
        /// 获取或设置编辑器文件系统参数。
        /// </summary>
        public FileSystemParameters EditorFileSystemParameters { get; set; }
    }

    /// <summary>
    /// 单机模式初始化参数兼容占位类型。
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public sealed class OfflinePlayModeParameters
    {
        /// <summary>
        /// 获取或设置内置文件系统参数。
        /// </summary>
        public FileSystemParameters BuildinFileSystemParameters { get; set; }
    }

    /// <summary>
    /// Web模式初始化参数兼容占位类型。
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public sealed class WebPlayModeParameters
    {
        /// <summary>
        /// 获取或设置Web文件系统参数。
        /// </summary>
        public FileSystemParameters WebFileSystemParameters { get; set; }
    }

    /// <summary>
    /// 热更模式初始化参数兼容占位类型。
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public sealed class HostPlayModeParameters
    {
        /// <summary>
        /// 获取或设置内置文件系统参数。
        /// </summary>
        public FileSystemParameters BuildinFileSystemParameters { get; set; }

        /// <summary>
        /// 获取或设置缓存文件系统参数。
        /// </summary>
        public FileSystemParameters CacheFileSystemParameters { get; set; }
    }

    /// <summary>
    /// 构建管线占位枚举。
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public enum EDefaultBuildPipeline
    {
        BuiltinBuildPipeline = 0
    }

    /// <summary>
    /// 编辑器模拟构建工具兼容占位类型。
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public static class EditorSimulateModeHelper
    {
        /// <summary>
        /// 执行模拟构建。
        /// </summary>
        /// <param name="pipelineName">管线名称。</param>
        /// <param name="packageName">包名称。</param>
        /// <returns>模拟构建结果。</returns>
        [UnityEngine.Scripting.Preserve]
        public static string SimulateBuild(string pipelineName, string packageName)
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// 远端服务兼容占位接口。
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public interface IRemoteServices
    {
        /// <summary>
        /// 获取主下载地址。
        /// </summary>
        /// <param name="fileName">文件名。</param>
        /// <param name="packageVersion">包版本。</param>
        /// <returns>主下载地址。</returns>
        [UnityEngine.Scripting.Preserve]
        string GetRemoteMainURL(string fileName, string packageVersion);

        /// <summary>
        /// 获取备用下载地址。
        /// </summary>
        /// <param name="fileName">文件名。</param>
        /// <param name="packageVersion">包版本。</param>
        /// <returns>备用下载地址。</returns>
        [UnityEngine.Scripting.Preserve]
        string GetRemoteFallbackURL(string fileName, string packageVersion);
    }

    /// <summary>
    /// 路径工具兼容占位类型。
    /// </summary>
    [UnityEngine.Scripting.Preserve]
    public static class PathUtility
    {
        /// <summary>
        /// 合并路径。
        /// </summary>
        /// <param name="path1">路径一。</param>
        /// <param name="path2">路径二。</param>
        /// <returns>合并后的路径。</returns>
        [UnityEngine.Scripting.Preserve]
        public static string Combine(string path1, string path2)
        {
            return $"{path1?.TrimEnd('/')}/{path2?.TrimStart('/')}";
        }
    }
}
