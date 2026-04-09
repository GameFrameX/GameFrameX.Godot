namespace GameFrameX.AssetSystem
{
    /// <summary>
    /// 文件校验等级
    /// </summary>
    [AssetSystemPreserve]
    public enum EFileVerifyLevel
    {
        /// <summary>
        /// 验证文件存在
        /// </summary>
        Low = 1,

        /// <summary>
        /// 验证文件大小
        /// </summary>
        Middle = 2,

        /// <summary>
        /// 验证文件大小和CRC
        /// </summary>
        High = 3,
    }
}