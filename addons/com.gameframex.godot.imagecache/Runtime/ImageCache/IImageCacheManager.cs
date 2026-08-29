using System.Threading;
using System.Threading.Tasks;
using Godot;

namespace GameFrameX.ImageCache.Runtime
{
    /// <summary>
    /// 图片缓存管理器接口。
    /// 迁移备注（Unity → Godot）：
    /// 1. Texture2D → ImageTexture。
    /// 2. WebGL 平台分支（浏览器管理缓存、IsCached 恒 false 等）不迁移：Godot 导出模板无对应平台宏链路，磁盘缓存链路全平台一致。
    /// </summary>
    public interface IImageCacheManager
    {
        /// <summary>
        /// 获取图片缓存配置。
        /// </summary>
        ImageCacheConfig Config { get; }

        /// <summary>
        /// 异步加载远程图片。
        /// 优先从磁盘缓存读取，未命中时下载到本地后加载。
        /// </summary>
        /// <param name="url">远程图片地址（必须以 http:// 或 https:// 开头）。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>加载成功返回 ImageTexture，失败返回 null。</returns>
        Task<ImageTexture> LoadImageAsync(string url, CancellationToken cancellationToken = default);

        /// <summary>
        /// 检查指定 URL 的图片是否已缓存。
        /// </summary>
        /// <param name="url">远程图片地址。</param>
        /// <returns>已缓存返回 true，否则返回 false。</returns>
        bool IsCached(string url);

        /// <summary>
        /// 移除指定 URL 的本地缓存文件。
        /// </summary>
        /// <param name="url">远程图片地址。</param>
        void RemoveCache(string url);

        /// <summary>
        /// 清空所有本地缓存文件。
        /// </summary>
        void ClearCache();
    }
}
