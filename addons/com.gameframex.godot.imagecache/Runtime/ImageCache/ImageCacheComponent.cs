using System.Threading;
using System.Threading.Tasks;
using GameFrameX.Download.Runtime;
using GameFrameX.Runtime;
using Godot;

namespace GameFrameX.ImageCache.Runtime
{
    /// <summary>
    /// 图片缓存组件。
    /// 提供远程图片的异步加载与磁盘缓存能力，可在 Inspector 中配置缓存路径等参数。
    /// 迁移备注（Unity → Godot）：
    /// 1. Unity 的 [SerializeField] 嵌套 ImageCacheConfig 序列化改为拆平的 [Export] 字段，_Ready 时灌入管理器配置。
    /// 2. Unity 的 AddComponentMenu/DisallowMultipleComponent 无 Godot 对应（场景挂载唯一性由场景编辑保证）。
    /// 3. 默认缓存路径沿用 PathHelper.AppHotfixResPath + "/cache/images/"，AppHotfixResPath 内部已把 user:// 映射为系统绝对路径。
    /// 4. componentType 为空时兜底为默认实现类型全名（Unity 侧由 Inspector 下拉写入；新挂节点场景文件尚无该值）。
    /// </summary>
    public sealed partial class ImageCacheComponent : GameFrameworkComponent
    {
        private IImageCacheManager m_ImageCacheManager;

        /// <summary>
        /// 缓存文件存放路径，可在 Inspector 中设置；留空使用默认路径。
        /// </summary>
        [Export] private string m_CachePath = string.Empty;

        /// <summary>
        /// 磁盘缓存最大容量（字节）。0 表示不限制。
        /// </summary>
        [Export] private long m_MaxDiskSize;

        /// <summary>
        /// 缓存过期天数。0 表示永不过期。
        /// </summary>
        [Export] private int m_ExpireDays;

        /// <summary>
        /// 获取图片缓存配置。
        /// </summary>
        public ImageCacheConfig Config
        {
            get { return m_ImageCacheManager != null ? m_ImageCacheManager.Config : null; }
        }

        /// <summary>
        /// 游戏框架组件初始化。
        /// </summary>
        public override void _Ready()
        {
            if (string.IsNullOrEmpty(componentType))
            {
                componentType = typeof(ImageCacheManager).FullName;
            }

            ImplementationComponentType = Utility.Assembly.GetType(componentType);
            InterfaceComponentType = typeof(IImageCacheManager);
            base._Ready();
            m_ImageCacheManager = GameFrameworkEntry.GetModule<IImageCacheManager>();
            if (m_ImageCacheManager == null)
            {
                Log.Fatal("ImageCache manager is invalid.");
                return;
            }

            var cachePath = string.IsNullOrEmpty(m_CachePath)
                ? PathHelper.AppHotfixResPath + "/cache/images/"
                : m_CachePath;
            // 迁移备注：PathHelper.Combine 的绝对路径粘连怪癖已在核心包修复（StartsWith 分支移除）；
            // 此处尾分隔符规范化保留作为信任边界纵深防御，防止外部传入的 CachePath 形态问题写歪缓存位置。
            if (!cachePath.EndsWith("/") && !cachePath.EndsWith("\\"))
            {
                cachePath += "/";
            }

            m_ImageCacheManager.Config.CachePath = cachePath;
            m_ImageCacheManager.Config.MaxDiskSize = m_MaxDiskSize;
            m_ImageCacheManager.Config.ExpireDays = m_ExpireDays;

            var downloadManager = GameFrameworkEntry.GetModule<IDownloadManager>();
            if (downloadManager == null)
            {
                Log.Fatal("Download manager is invalid.");
                return;
            }

            ((ImageCacheManager)m_ImageCacheManager).SetDownloadManager(downloadManager);
        }

        /// <summary>
        /// 异步加载远程图片。
        /// </summary>
        /// <param name="url">远程图片地址（必须以 http:// 或 https:// 开头）。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>加载成功返回 ImageTexture，失败返回 null。</returns>
        public Task<ImageTexture> LoadImageAsync(string url, CancellationToken cancellationToken = default)
        {
            return m_ImageCacheManager.LoadImageAsync(url, cancellationToken);
        }

        /// <summary>
        /// 检查指定 URL 的图片是否已缓存。
        /// </summary>
        /// <param name="url">远程图片地址。</param>
        /// <returns>已缓存返回 true，否则返回 false。</returns>
        public bool IsCached(string url)
        {
            return m_ImageCacheManager.IsCached(url);
        }

        /// <summary>
        /// 移除指定 URL 的本地缓存文件。
        /// </summary>
        /// <param name="url">远程图片地址。</param>
        public void RemoveCache(string url)
        {
            m_ImageCacheManager.RemoveCache(url);
        }

        /// <summary>
        /// 清空所有本地缓存文件。
        /// </summary>
        public void ClearCache()
        {
            m_ImageCacheManager.ClearCache();
        }
    }
}
