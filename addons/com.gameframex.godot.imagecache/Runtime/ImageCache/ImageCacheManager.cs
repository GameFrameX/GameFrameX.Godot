using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GameFrameX.Download.Runtime;
using GameFrameX.Runtime;
using Godot;

namespace GameFrameX.ImageCache.Runtime
{
    /// <summary>
    /// 图片缓存管理器。
    /// 通过 IDownloadManager 下载远程图片到磁盘，以 URL 的 MD5 哈希作为文件名缓存。
    /// 迁移备注（Unity → Godot）：
    /// 1. Unity 版分 WebGL/非 WebGL 两条链路；Godot 版仅保留磁盘缓存链路（全平台一致）。
    /// 2. Unity 基准的 ImageCacheConfig.MaxDiskSize / ExpireDays 只定义未消费；Godot 版补齐：
    ///    过期文件在 IsCached 时懒删除，下载成功后经 MaintainCache 批量清理并按近似 LRU 淘汰至限额内。
    /// 3. 磁盘 IO 使用 System.IO/FileHelper（操作系统路径）；user:// 虚拟路径由组件层经
    ///    PathHelper.AppHotfixResPath（内部 GlobalizePath）转换为系统绝对路径后灌入配置。
    /// </summary>
    public sealed class ImageCacheManager : GameFrameworkModule, IImageCacheManager
    {
        private readonly ImageCacheConfig m_Config;

        private readonly IImageTextureFactory m_TextureFactory;

        /// <summary>
        /// 正在进行中的下载任务，Key 为下载序列号，Value 为对应的 TaskCompletionSource。
        /// </summary>
        private readonly ConcurrentDictionary<int, TaskCompletionSource<bool>> m_DownloadingTasks = new ConcurrentDictionary<int, TaskCompletionSource<bool>>();

        /// <summary>
        /// 下载管理器实例，用于发起文件下载。
        /// </summary>
        private IDownloadManager m_DownloadManager;

        public ImageCacheManager()
            : this(new GodotImageTextureFactory())
        {
        }

        /// <summary>
        /// 初始化图片缓存管理器的新实例。
        /// 迁移备注：公开注入 IImageTextureFactory 的构造重载供单元测试使用
        /// （Godot native API 在 xunit 宿主会段错误，测试注入 stub 工厂）。
        /// </summary>
        /// <param name="textureFactory">纹理工厂实例。</param>
        public ImageCacheManager(IImageTextureFactory textureFactory)
        {
            m_Config = new ImageCacheConfig();
            m_TextureFactory = textureFactory ?? throw new ArgumentNullException(nameof(textureFactory));
        }

        /// <summary>
        /// 获取图片缓存配置。
        /// </summary>
        public ImageCacheConfig Config
        {
            get { return m_Config; }
        }

        /// <summary>
        /// 设置下载管理器并注册下载成功/失败事件回调。
        /// 迁移备注：Unity 版为 internal（受 asmdef 边界保护）；Godot 单工程全局编译无此边界，改为 public。
        /// </summary>
        /// <param name="downloadManager">下载管理器实例。</param>
        public void SetDownloadManager(IDownloadManager downloadManager)
        {
            m_DownloadManager = downloadManager;
            m_DownloadManager.DownloadSuccess += OnDownloadSuccess;
            m_DownloadManager.DownloadFailure += OnDownloadFailure;
        }

        /// <summary>
        /// 异步加载远程图片。
        /// 优先从磁盘缓存读取，未命中时下载到本地后加载。
        /// </summary>
        /// <param name="url">远程图片地址（必须以 http:// 或 https:// 开头）。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>加载成功返回 ImageTexture，失败返回 null。</returns>
        public async Task<ImageTexture> LoadImageAsync(string url, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(url))
            {
                Log.Warning("ImageCache: url is null or empty.");
                return null;
            }

            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                Log.Warning("ImageCache: url must start with http:// or https://. url: " + url);
                return null;
            }

            var cachePath = GetCachePath(url);
            if (!FileHelper.IsExists(cachePath))
            {
                if (m_DownloadManager == null)
                {
                    Log.Warning("ImageCache: download manager is not set. url: " + url);
                    return null;
                }

                if (!Directory.Exists(m_Config.CachePath))
                {
                    Directory.CreateDirectory(m_Config.CachePath);
                }

                var serialId = m_DownloadManager.AddDownload(cachePath, url);
                var tcs = new TaskCompletionSource<bool>();
                m_DownloadingTasks.TryAdd(serialId, tcs);

                var downloadSuccess = await tcs.Task;
                if (!downloadSuccess)
                {
                    Log.Warning("ImageCache: download failed. url: " + url);
                    return null;
                }

                if (cancellationToken.IsCancellationRequested)
                {
                    return null;
                }

                MaintainCache();
            }

            if (!FileHelper.IsExists(cachePath))
            {
                return null;
            }

            var buffer = FileHelper.ReadAllBytes(cachePath);
            if (buffer == null || buffer.Length == 0)
            {
                return null;
            }

            return m_TextureFactory.Create(buffer);
        }

        /// <summary>
        /// 检查指定 URL 的图片是否已缓存；过期文件视为未缓存并删除。
        /// </summary>
        /// <param name="url">远程图片地址。</param>
        /// <returns>已缓存返回 true，否则返回 false。</returns>
        public bool IsCached(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return false;
            }

            var cachePath = GetCachePath(url);
            if (!FileHelper.IsExists(cachePath))
            {
                return false;
            }

            if (IsExpired(File.GetLastWriteTimeUtc(cachePath)))
            {
                // ponytail: 过期即懒删除，没有周期性后台清理；升级路径：在 Update 中定时批量清理。
                FileHelper.Delete(cachePath);
                return false;
            }

            return true;
        }

        /// <summary>
        /// 移除指定 URL 的本地缓存文件。
        /// </summary>
        /// <param name="url">远程图片地址。</param>
        public void RemoveCache(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return;
            }

            var cachePath = GetCachePath(url);
            if (FileHelper.IsExists(cachePath))
            {
                FileHelper.Delete(cachePath);
            }
        }

        /// <summary>
        /// 清空所有本地缓存文件。
        /// </summary>
        public void ClearCache()
        {
            if (string.IsNullOrEmpty(m_Config.CachePath) || !Directory.Exists(m_Config.CachePath))
            {
                return;
            }

            FileHelper.CleanDirectory(m_Config.CachePath);
        }

        /// <summary>
        /// 维护磁盘缓存：先清理过期文件，再按近似 LRU（最后写入时间从旧到新）淘汰至 MaxDiskSize 以内。
        /// 下载成功后自动调用，也可手动调用。
        /// </summary>
        public void MaintainCache()
        {
            if (string.IsNullOrEmpty(m_Config.CachePath) || !Directory.Exists(m_Config.CachePath))
            {
                return;
            }

            var directory = new DirectoryInfo(m_Config.CachePath);
            if (m_Config.ExpireDays > 0)
            {
                var files = new List<FileInfo>(directory.GetFiles());
                foreach (var file in files)
                {
                    if (IsExpired(file.LastWriteTimeUtc))
                    {
                        file.Delete();
                    }
                }
            }

            if (m_Config.MaxDiskSize <= 0)
            {
                return;
            }

            // ponytail: 近似 LRU——按文件最后写入时间淘汰，缓存命中不更新写入时间；
            // 升级路径：命中时 File.SetLastWriteTimeUtc 触碰即成真 LRU。
            var remaining = new List<FileInfo>(directory.GetFiles());
            remaining.Sort((a, b) => a.LastWriteTimeUtc.CompareTo(b.LastWriteTimeUtc));
            long totalSize = 0;
            foreach (var file in remaining)
            {
                totalSize += file.Length;
            }

            for (int i = 0; i < remaining.Count && totalSize > m_Config.MaxDiskSize; i++)
            {
                totalSize -= remaining[i].Length;
                remaining[i].Delete();
            }
        }

        private string GetCachePath(string url)
        {
            var hash = Utility.Hash.MD5.Hash(url);
            return PathHelper.Combine(m_Config.CachePath, hash + Utility.Const.FileNameSuffix.PNG);
        }

        private bool IsExpired(DateTime lastWriteTimeUtc)
        {
            if (m_Config.ExpireDays <= 0)
            {
                return false;
            }

            return lastWriteTimeUtc < DateTime.UtcNow.AddDays(-m_Config.ExpireDays);
        }

        /// <summary>
        /// 下载成功回调，根据序列号匹配对应的 TaskCompletionSource 并设置结果为 true。
        /// </summary>
        private void OnDownloadSuccess(object sender, DownloadSuccessEventArgs e)
        {
            if (m_DownloadingTasks.TryRemove(e.SerialId, out var tcs))
            {
                tcs.TrySetResult(true);
            }
        }

        /// <summary>
        /// 下载失败回调，根据序列号匹配对应的 TaskCompletionSource 并设置结果为 false。
        /// </summary>
        private void OnDownloadFailure(object sender, DownloadFailureEventArgs e)
        {
            if (m_DownloadingTasks.TryRemove(e.SerialId, out var tcs))
            {
                tcs.TrySetResult(false);
            }
        }

        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        /// <summary>
        /// 关闭并清理资源。
        /// </summary>
        public override void Shutdown()
        {
            if (m_DownloadManager != null)
            {
                m_DownloadManager.DownloadSuccess -= OnDownloadSuccess;
                m_DownloadManager.DownloadFailure -= OnDownloadFailure;
                m_DownloadManager = null;
            }

            m_DownloadingTasks.Clear();
        }
    }
}
