namespace GameFrameX.ImageCache.Runtime
{
    /// <summary>
    /// 图片缓存配置。
    /// 迁移备注（Unity → Godot）：Unity 的 [Serializable]/[SerializeField] 序列化不迁移；
    /// Godot 侧由组件把 [Export] 字段在 _Ready 时灌入本配置（见 ImageCacheComponent）。
    /// MaxDiskSize / ExpireDays 在 Unity 基准中只有字段没有消费逻辑，Godot 版在 ImageCacheManager 中补齐。
    /// </summary>
    public sealed class ImageCacheConfig
    {
        private string m_CachePath;

        private long m_MaxDiskSize;

        private int m_ExpireDays;

        /// <summary>
        /// 初始化图片缓存配置的新实例。
        /// </summary>
        public ImageCacheConfig()
        {
            m_CachePath = string.Empty;
            m_MaxDiskSize = 0;
            m_ExpireDays = 0;
        }

        /// <summary>
        /// 获取或设置缓存文件存放路径。
        /// </summary>
        public string CachePath
        {
            get { return m_CachePath; }
            set { m_CachePath = value; }
        }

        /// <summary>
        /// 获取或设置磁盘缓存最大容量（字节）。0 表示不限制。
        /// </summary>
        public long MaxDiskSize
        {
            get { return m_MaxDiskSize; }
            set { m_MaxDiskSize = value; }
        }

        /// <summary>
        /// 获取或设置缓存过期天数。0 表示永不过期。
        /// </summary>
        public int ExpireDays
        {
            get { return m_ExpireDays; }
            set { m_ExpireDays = value; }
        }
    }
}
