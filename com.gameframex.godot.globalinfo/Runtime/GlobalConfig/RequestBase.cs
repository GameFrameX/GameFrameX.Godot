namespace GameFrameX.GlobalConfig.Runtime
{
    /// <summary>
    /// 请求基类
    /// </summary>
    public abstract class RequestBase
    {
        /// <summary>
        /// 语言
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// 程序版本
        /// </summary>
        public string AppVersion { get; set; }

        /// <summary>
        /// 运行平台
        /// </summary>
        public string Platform { get; set; }

        /// <summary>
        /// 包名
        /// </summary>
        public string PackageName { get; set; }

        /// <summary>
        /// 渠道
        /// </summary>
        public string Channel { get; set; }

        /// <summary>
        /// 子渠道
        /// </summary>
        public string SubChannel { get; set; }
    }
}