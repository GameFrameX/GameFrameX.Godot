namespace GameFrameX.AssetSystem.Editor
{
    /// <summary>
    /// 打包规则(枚举子集,语义对齐 YooAsset Unity 版 DefaultPackRule)
    /// </summary>
    public enum EPackRule
    {
        /// <summary>
        /// 以文件路径作为资源包名,每个文件独自打资源包。
        /// </summary>
        PackSeparately,

        /// <summary>
        /// 以资源所在目录路径作为资源包名,目录下所有文件打进同一个资源包。
        /// </summary>
        PackDirectory,

        /// <summary>
        /// 以收集目录的顶层目录作为资源包名(暂未实现,接入收集流程时补齐)。
        /// </summary>
        PackTopDirectory,

        /// <summary>
        /// 原生文件打包,以去扩展名的文件路径作为资源包名,输出后缀 .rawfile。
        /// </summary>
        PackRawFile,
    }
}
