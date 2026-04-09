namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    public class DownloadParam
    {
        public readonly int FailedTryAgain;
        public readonly int Timeout;

        /// <summary>
        /// 导入的本地文件路径
        /// </summary>
        public string ImportFilePath { set; get; }

        /// <summary>
        /// 主资源地址
        /// </summary>
        public string MainURL { set; get; }

        /// <summary>
        /// 备用资源地址
        /// </summary>
        public string FallbackURL { set; get; }

        [AssetSystemPreserve]
        public DownloadParam(int failedTryAgain, int timeout)
        {
            FailedTryAgain = failedTryAgain;
            Timeout = timeout;
        }
    }
}