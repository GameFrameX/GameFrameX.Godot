using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using GameFrameX.Download.Runtime;
using GameFrameX.ImageCache.Runtime;
using GameFrameX.Runtime;
using Godot;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// 图片缓存管理器测试（迁移自 Unity com.gameframex.unity.imagecache）。
    /// 迁移备注：
    /// 1. Unity 基准的 Config.MaxDiskSize/ExpireDays 只有字段没有消费逻辑；Godot 版补齐磁盘限额淘汰（近似 LRU）与过期清理，以下用例覆盖补齐语义。
    /// 2. Godot native API（ImageTexture/Image 等）在 xunit 宿主会段错误：纹理解码经 IImageTextureFactory 注入 stub，本测试不构造任何 Godot native 对象。
    /// 3. DownloadSuccessEventArgs.Create 走全局 ReferencePool，按仓库惯例挂 [Collection("ReferencePool")] 避免与其它触碰引用池的用例并行。
    /// 4. 磁盘 IO 用临时目录（Path.GetTempPath + Guid），Dispose 时清理。
    /// </summary>
    [Collection("ReferencePool")]
    public sealed class ImageCacheTests : IDisposable
    {
        private readonly string m_TempDir;

        private readonly StubTextureFactory m_TextureFactory;

        private readonly ImageCacheManager m_Manager;

        public ImageCacheTests()
        {
            m_TempDir = Path.Combine(Path.GetTempPath(), "gfx_imagecache_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(m_TempDir);
            m_TextureFactory = new StubTextureFactory();
            m_Manager = new ImageCacheManager(m_TextureFactory);
            // 迁移备注：CachePath 以尾分隔符结尾贴生产默认形态（AppHotfixResPath + "/cache/images/"），
            // 同时规避 PathHelper.Combine 对无尾分隔符绝对路径的粘连怪癖（文件会落到父目录）。
            m_Manager.Config.CachePath = m_TempDir + "/";
        }

        public void Dispose()
        {
            m_Manager.Shutdown();
            try
            {
                Directory.Delete(m_TempDir, true);
            }
            catch (IOException)
            {
            }
        }

        // ──────────────── 配置 ────────────────

        [Fact]
        public void Config_PropertyRoundTrip()
        {
            var config = new ImageCacheConfig();
            Assert.Equal(string.Empty, config.CachePath);
            Assert.Equal(0L, config.MaxDiskSize);
            Assert.Equal(0, config.ExpireDays);

            config.CachePath = "user://cache/images/";
            config.MaxDiskSize = 1024L;
            config.ExpireDays = 7;
            Assert.Equal("user://cache/images/", config.CachePath);
            Assert.Equal(1024L, config.MaxDiskSize);
            Assert.Equal(7, config.ExpireDays);
        }

        // ──────────────── URL 校验 ────────────────

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void IsCached_NullOrEmptyUrl_ReturnsFalse(string url)
        {
            Assert.False(m_Manager.IsCached(url));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("ftp://example.com/a.png")]
        [InlineData("example.com/a.png")]
        public void LoadImageAsync_InvalidUrl_ReturnsNull(string url)
        {
            var texture = m_Manager.LoadImageAsync(url).GetAwaiter().GetResult();
            Assert.Null(texture);
            Assert.False(m_TextureFactory.Called);
        }

        // ──────────────── 缓存命中 / 未命中 ────────────────

        [Fact]
        public void IsCached_Miss_ReturnsFalse()
        {
            Assert.False(m_Manager.IsCached("https://example.com/miss.png"));
        }

        [Fact]
        public void IsCached_Hit_ReturnsTrue()
        {
            var url = "https://example.com/hit.png";
            WriteCacheFile(url, new byte[] { 1, 2, 3 });
            Assert.True(m_Manager.IsCached(url));
        }

        [Fact]
        public void LoadImageAsync_CacheHit_DecodesFileWithoutDownload()
        {
            var url = "https://example.com/hit.png";
            var bytes = new byte[] { 0x89, 0x50, 0x4E, 0x47 };
            WriteCacheFile(url, bytes);

            var texture = m_Manager.LoadImageAsync(url).GetAwaiter().GetResult();

            // stub 工厂不构造 Godot native 对象（xunit 宿主会段错误），返回 null；命中语义由工厂收到的字节验证
            Assert.Null(texture);
            Assert.True(m_TextureFactory.Called);
            Assert.Equal(bytes, m_TextureFactory.LastBuffer);
        }

        [Fact]
        public void LoadImageAsync_MissWithoutDownloadManager_ReturnsNull()
        {
            var texture = m_Manager.LoadImageAsync("https://example.com/no-manager.png").GetAwaiter().GetResult();
            Assert.Null(texture);
            Assert.False(m_TextureFactory.Called);
        }

        // ──────────────── 下载链路（事件驱动 TCS） ────────────────

        [Fact]
        public async Task LoadImageAsync_DownloadSuccess_CachesAndDecodes()
        {
            var fake = new FakeDownloadManager();
            m_Manager.SetDownloadManager(fake);
            var url = "https://example.com/avatar.png";

            // 迁移备注：LoadImageAsync 的同步段（AddDownload + TryAdd）在返回 task 前已跑完，
            // 此后再手动触发下载事件，TryRemove 必然命中——单线程确定性时序，避免 fake 在
            // AddDownload 内同步触发导致 TryAdd 未执行、TCS 永不完成的死锁（事件先于注册）。
            var task = m_Manager.LoadImageAsync(url);
            fake.CompletePending();
            var texture = await task;

            Assert.Null(texture);
            Assert.True(m_TextureFactory.Called);
            Assert.Equal(fake.Payload, m_TextureFactory.LastBuffer);
            Assert.True(m_Manager.IsCached(url));
        }

        [Fact]
        public async Task LoadImageAsync_DownloadFailure_ReturnsNull()
        {
            var fake = new FakeDownloadManager();
            fake.FailureMode = true;
            m_Manager.SetDownloadManager(fake);
            var url = "https://example.com/broken.png";

            var task = m_Manager.LoadImageAsync(url);
            fake.FailPending();
            var texture = await task;

            Assert.Null(texture);
            Assert.False(m_TextureFactory.Called);
            Assert.False(m_Manager.IsCached(url));
        }

        [Fact]
        public async Task LoadImageAsync_Canceled_AfterDownload_ReturnsNull()
        {
            var fake = new FakeDownloadManager();
            m_Manager.SetDownloadManager(fake);
            var url = "https://example.com/canceled.png";
            var cts = new CancellationTokenSource();
            cts.Cancel();

            var task = m_Manager.LoadImageAsync(url, cts.Token);
            fake.CompletePending();
            var texture = await task;

            Assert.Null(texture);
            Assert.False(m_TextureFactory.Called);
            // 下载本身已完成并落盘，仅取消后续解码
            Assert.True(m_Manager.IsCached(url));
        }

        // ──────────────── 过期清理 ────────────────

        [Fact]
        public void IsCached_ExpiredFile_IsRemovedAndReturnsFalse()
        {
            m_Manager.Config.ExpireDays = 1;
            var url = "https://example.com/old.png";
            var path = WriteCacheFile(url, new byte[] { 1 }, 3);

            Assert.False(m_Manager.IsCached(url));
            Assert.False(File.Exists(path));
        }

        [Fact]
        public void IsCached_ExpireDaysZero_NeverExpires()
        {
            var url = "https://example.com/ancient.png";
            WriteCacheFile(url, new byte[] { 1 }, 3650);

            Assert.True(m_Manager.IsCached(url));
        }

        [Fact]
        public void MaintainCache_RemovesExpiredFiles()
        {
            m_Manager.Config.ExpireDays = 1;
            var keepUrl = "https://example.com/keep.png";
            var dropUrl = "https://example.com/drop.png";
            var keepPath = WriteCacheFile(keepUrl, new byte[] { 1 });
            var dropPath = WriteCacheFile(dropUrl, new byte[] { 1 }, 3);

            m_Manager.MaintainCache();

            Assert.True(File.Exists(keepPath));
            Assert.False(File.Exists(dropPath));
        }

        [Fact]
        public void MaintainCache_MissingDirectory_DoesNotThrow()
        {
            m_Manager.Config.CachePath = Path.Combine(m_TempDir, "not-exists");
            m_Manager.MaintainCache();
        }

        // ──────────────── 磁盘限额淘汰 ────────────────

        [Fact]
        public void MaintainCache_EvictsOldestUntilUnderLimit()
        {
            m_Manager.Config.MaxDiskSize = 200;
            var urlA = "https://example.com/a.png";
            var urlB = "https://example.com/b.png";
            var urlC = "https://example.com/c.png";
            WriteCacheFile(urlA, new byte[100], 3);
            WriteCacheFile(urlB, new byte[100], 2);
            WriteCacheFile(urlC, new byte[100]);

            m_Manager.MaintainCache();

            // 最旧的 a 被淘汰，总量 300B → 200B 恰好满足限额
            Assert.False(m_Manager.IsCached(urlA));
            Assert.True(m_Manager.IsCached(urlB));
            Assert.True(m_Manager.IsCached(urlC));
        }

        [Fact]
        public void MaintainCache_ZeroLimit_KeepsAll()
        {
            var urlA = "https://example.com/a.png";
            var urlB = "https://example.com/b.png";
            WriteCacheFile(urlA, new byte[100], 3);
            WriteCacheFile(urlB, new byte[100]);

            m_Manager.MaintainCache();

            Assert.True(m_Manager.IsCached(urlA));
            Assert.True(m_Manager.IsCached(urlB));
        }

        // ──────────────── 移除 / 清空 ────────────────

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void RemoveCache_NullOrEmptyUrl_DoesNotThrow(string url)
        {
            m_Manager.RemoveCache(url);
        }

        [Fact]
        public void RemoveCache_DeletesCacheFile()
        {
            var url = "https://example.com/remove.png";
            var path = WriteCacheFile(url, new byte[] { 1 });

            m_Manager.RemoveCache(url);

            Assert.False(File.Exists(path));
            Assert.False(m_Manager.IsCached(url));
        }

        [Fact]
        public void RemoveCache_NotCachedUrl_DoesNotThrow()
        {
            m_Manager.RemoveCache("https://example.com/never.png");
        }

        [Fact]
        public void ClearCache_RemovesAllCacheFiles()
        {
            WriteCacheFile("https://example.com/a.png", new byte[] { 1 });
            WriteCacheFile("https://example.com/b.png", new byte[] { 2 });

            m_Manager.ClearCache();

            Assert.Equal(0, Directory.GetFiles(m_TempDir).Length);
        }

        [Fact]
        public void ClearCache_MissingDirectory_DoesNotThrow()
        {
            m_Manager.Config.CachePath = Path.Combine(m_TempDir, "not-exists");
            m_Manager.ClearCache();
        }

        // ──────────────── 辅助 ────────────────

        /// <summary>
        /// 按生产算法（MD5(url) + ".png"）写入缓存文件，可选回拨最后写入时间。
        /// </summary>
        private string WriteCacheFile(string url, byte[] bytes, int ageDays = 0)
        {
            // 迁移备注：必须与生产 GetCachePath 同源使用 PathHelper.Combine——该实现对以 "/" 开头的
            // 绝对路径不追加分隔符（粘连路径怪癖），若测试侧用 Path.Combine 写文件会与 Manager 读取路径不一致。
            var path = PathHelper.Combine(m_Manager.Config.CachePath, Utility.Hash.MD5.Hash(url) + Utility.Const.FileNameSuffix.PNG);
            File.WriteAllBytes(path, bytes);
            if (ageDays > 0)
            {
                File.SetLastWriteTimeUtc(path, DateTime.UtcNow.AddDays(-ageDays));
            }
            return path;
        }

        /// <summary>
        /// 纹理工厂 stub：不构造 Godot native 对象，只记录收到的字节。
        /// </summary>
        private sealed class StubTextureFactory : IImageTextureFactory
        {
            public bool Called { get; private set; }

            public byte[] LastBuffer { get; private set; }

            public ImageTexture Create(byte[] buffer)
            {
                Called = true;
                LastBuffer = buffer;
                return null;
            }
        }

        /// <summary>
        /// 下载管理器 stub：AddDownload 同步落盘假字节并触发成功/失败事件，其余成员不支持。
        /// </summary>
        private sealed class FakeDownloadManager : IDownloadManager
        {
            public byte[] Payload { get; set; } = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A };

            public bool FailureMode { get; set; }

            private int m_SerialId;

            private int m_PendingSerialId;

            private string m_PendingPath;

            private string m_PendingUri;

            private event EventHandler<DownloadStartEventArgs> m_DownloadStart;

            private event EventHandler<DownloadUpdateEventArgs> m_DownloadUpdate;

            private event EventHandler<DownloadSuccessEventArgs> m_DownloadSuccess;

            private event EventHandler<DownloadFailureEventArgs> m_DownloadFailure;

            event EventHandler<DownloadStartEventArgs> IDownloadManager.DownloadStart
            {
                add { m_DownloadStart += value; }
                remove { m_DownloadStart -= value; }
            }

            event EventHandler<DownloadUpdateEventArgs> IDownloadManager.DownloadUpdate
            {
                add { m_DownloadUpdate += value; }
                remove { m_DownloadUpdate -= value; }
            }

            event EventHandler<DownloadSuccessEventArgs> IDownloadManager.DownloadSuccess
            {
                add { m_DownloadSuccess += value; }
                remove { m_DownloadSuccess -= value; }
            }

            event EventHandler<DownloadFailureEventArgs> IDownloadManager.DownloadFailure
            {
                add { m_DownloadFailure += value; }
                remove { m_DownloadFailure -= value; }
            }

            bool IDownloadManager.Paused
            {
                get { throw new NotSupportedException("Test stub."); }
                set { throw new NotSupportedException("Test stub."); }
            }

            int IDownloadManager.TotalAgentCount
            {
                get { throw new NotSupportedException("Test stub."); }
            }

            int IDownloadManager.FreeAgentCount
            {
                get { throw new NotSupportedException("Test stub."); }
            }

            int IDownloadManager.WorkingAgentCount
            {
                get { throw new NotSupportedException("Test stub."); }
            }

            int IDownloadManager.WaitingTaskCount
            {
                get { throw new NotSupportedException("Test stub."); }
            }

            int IDownloadManager.FlushSize
            {
                get { throw new NotSupportedException("Test stub."); }
                set { throw new NotSupportedException("Test stub."); }
            }

            float IDownloadManager.Timeout
            {
                get { throw new NotSupportedException("Test stub."); }
                set { throw new NotSupportedException("Test stub."); }
            }

            float IDownloadManager.CurrentSpeed
            {
                get { throw new NotSupportedException("Test stub."); }
            }

            void IDownloadManager.AddDownloadAgentHelper(IDownloadAgentHelper downloadAgentHelper)
            {
                throw new NotSupportedException("Test stub.");
            }

            TaskInfo IDownloadManager.GetDownloadInfo(int serialId)
            {
                throw new NotSupportedException("Test stub.");
            }

            TaskInfo[] IDownloadManager.GetDownloadInfos(string tag)
            {
                throw new NotSupportedException("Test stub.");
            }

            void IDownloadManager.GetDownloadInfos(string tag, List<TaskInfo> results)
            {
                throw new NotSupportedException("Test stub.");
            }

            TaskInfo[] IDownloadManager.GetAllDownloadInfos()
            {
                throw new NotSupportedException("Test stub.");
            }

            void IDownloadManager.GetAllDownloadInfos(List<TaskInfo> results)
            {
                throw new NotSupportedException("Test stub.");
            }

            int IDownloadManager.AddDownload(string downloadPath, string downloadUri)
            {
                // 迁移备注：真实 DownloadManager 异步触发事件（必然晚于 AddDownload 返回）；
                // 这里只记录 pending，由测试在拿到 LoadImageAsync 返回的 task 后显式触发，
                // 避免 AddDownload 内同步触发造成的 注册晚于事件 死锁。
                m_SerialId++;
                m_PendingSerialId = m_SerialId;
                m_PendingPath = downloadPath;
                m_PendingUri = downloadUri;
                return m_SerialId;
            }

            /// <summary>
            /// 手动触发 pending 下载成功：落盘假字节并广播 DownloadSuccess。
            /// </summary>
            public void CompletePending()
            {
                File.WriteAllBytes(m_PendingPath, Payload);
                m_DownloadSuccess?.Invoke(this, DownloadSuccessEventArgs.Create(m_PendingSerialId, m_PendingPath, m_PendingUri, Payload.Length, null));
            }

            /// <summary>
            /// 手动触发 pending 下载失败：广播 DownloadFailure。
            /// </summary>
            public void FailPending()
            {
                m_DownloadFailure?.Invoke(this, DownloadFailureEventArgs.Create(m_PendingSerialId, m_PendingPath, m_PendingUri, "fake failure", null));
            }

            int IDownloadManager.AddDownload(string downloadPath, string downloadUri, string tag)
            {
                return ((IDownloadManager)this).AddDownload(downloadPath, downloadUri);
            }

            int IDownloadManager.AddDownload(string downloadPath, string downloadUri, int priority)
            {
                return ((IDownloadManager)this).AddDownload(downloadPath, downloadUri);
            }

            int IDownloadManager.AddDownload(string downloadPath, string downloadUri, object userData)
            {
                return ((IDownloadManager)this).AddDownload(downloadPath, downloadUri);
            }

            int IDownloadManager.AddDownload(string downloadPath, string downloadUri, string tag, int priority)
            {
                return ((IDownloadManager)this).AddDownload(downloadPath, downloadUri);
            }

            int IDownloadManager.AddDownload(string downloadPath, string downloadUri, string tag, object userData)
            {
                return ((IDownloadManager)this).AddDownload(downloadPath, downloadUri);
            }

            int IDownloadManager.AddDownload(string downloadPath, string downloadUri, int priority, object userData)
            {
                return ((IDownloadManager)this).AddDownload(downloadPath, downloadUri);
            }

            int IDownloadManager.AddDownload(string downloadPath, string downloadUri, string tag, int priority, object userData)
            {
                return ((IDownloadManager)this).AddDownload(downloadPath, downloadUri);
            }

            bool IDownloadManager.RemoveDownload(int serialId)
            {
                throw new NotSupportedException("Test stub.");
            }

            int IDownloadManager.RemoveDownloads(string tag)
            {
                throw new NotSupportedException("Test stub.");
            }

            int IDownloadManager.RemoveAllDownloads()
            {
                throw new NotSupportedException("Test stub.");
            }
        }
    }
}
