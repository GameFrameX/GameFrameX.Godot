namespace GameFrameX.AssetSystem.Editor
{
    /// <summary>
    /// BuildinCatalog 内的单个 bundle 记录。
    /// BundleGUID 对应运行时 PackageBundle.BundleGUID(其值即 FileHash);FileName 为构建输出的物理文件名。
    /// </summary>
    public class BuildinCatalogFileEntry
    {
        public string BundleGUID;

        public string FileName;

        public BuildinCatalogFileEntry()
        {
        }

        public BuildinCatalogFileEntry(string bundleGUID, string fileName)
        {
            BundleGUID = bundleGUID;
            FileName = fileName;
        }
    }
}
