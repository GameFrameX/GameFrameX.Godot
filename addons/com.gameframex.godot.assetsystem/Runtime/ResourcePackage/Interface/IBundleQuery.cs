namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    internal interface IBundleQuery
    {
        /// <summary>
        /// 获取主资源包信息
        /// </summary>
        [AssetSystemPreserve]
        BundleInfo GetMainBundleInfo(AssetInfo assetInfo);

        /// <summary>
        /// 获取依赖的资源包信息集合
        /// </summary>
        [AssetSystemPreserve]
        BundleInfo[] GetDependBundleInfos(AssetInfo assetPath);

        /// <summary>
        /// 获取主资源包名称
        /// </summary>
        [AssetSystemPreserve]
        string GetMainBundleName(AssetInfo assetInfo);

        /// <summary>
        /// 获取依赖的资源包名称集合
        /// </summary>
        [AssetSystemPreserve]
        string[] GetDependBundleNames(AssetInfo assetInfo);

        /// <summary>
        /// 清单是否有效
        /// </summary>
        [AssetSystemPreserve]
        bool ManifestValid();
    }
}