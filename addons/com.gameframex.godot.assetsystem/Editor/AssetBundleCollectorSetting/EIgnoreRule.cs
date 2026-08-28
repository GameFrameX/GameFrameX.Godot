namespace GameFrameX.AssetSystem.Editor
{
    /// <summary>
    /// 忽略规则(枚举子集,语义对齐 YooAsset Unity 版 DefaultIgnoreRule)
    /// </summary>
    public enum EIgnoreRule
    {
        /// <summary>
        /// 不忽略任何文件
        /// </summary>
        NormalIgnore,

        /// <summary>
        /// 忽略全部文件(调试用)
        /// </summary>
        IgnoreAll,
    }
}
