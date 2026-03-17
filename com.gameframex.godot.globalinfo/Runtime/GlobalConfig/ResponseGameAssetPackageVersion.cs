namespace GameFrameX.GlobalConfig.Runtime
{
    /// <summary>
    /// 游戏资源版本信息
    /// </summary>
    public class ResponseGameAssetPackageVersion
    {
        /// <summary>
        /// 语言
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// 资源版本
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 资源包名称
        /// </summary>
        public string AssetPackageName { get; set; }

        /// <summary>
        /// 资源包路径
        /// </summary>
        public string AssetPackagePath { get; set; }

        /// <summary>
        /// 平台
        /// </summary>
        public string Platform { get; set; }

        /// <summary>
        /// 下载根路径
        /// </summary>
        public string RootPath { get; set; }

        /// <summary>
        /// 包名
        /// </summary>
        public string PackageName { get; set; }

        /// <summary>
        /// 游戏版本
        /// </summary>
        public string AppVersion { get; set; }

        /// <summary>
        /// 渠道名称
        /// </summary>
        public string Channel { get; set; }
    }
}