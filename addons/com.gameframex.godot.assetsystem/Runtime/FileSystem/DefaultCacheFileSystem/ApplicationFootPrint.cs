using System.IO;

namespace GameFrameX.AssetSystem
{
    /// <summary>
    /// 应用程序水印
    /// </summary>
    [AssetSystemPreserve]
    internal class ApplicationFootPrint
    {
        private readonly DefaultCacheFileSystem _fileSystem;
        private string _footPrint;


        [AssetSystemPreserve]
        public ApplicationFootPrint(DefaultCacheFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        /// <summary>
        /// 读取应用程序水印
        /// </summary>
        [AssetSystemPreserve]
        public void Load(string packageName)
        {
            var footPrintFilePath = _fileSystem.GetSandboxAppFootPrintFilePath();
            if (File.Exists(footPrintFilePath))
            {
                _footPrint = FileUtility.ReadAllText(footPrintFilePath);
            }
            else
            {
                Coverage(packageName);
            }
        }

        /// <summary>
        /// 检测水印是否发生变化
        /// </summary>
        [AssetSystemPreserve]
        public bool IsDirty()
        {
            return _footPrint != GodotBuildIdentity.Current;
        }

        /// <summary>
        /// 覆盖掉水印
        /// </summary>
        [AssetSystemPreserve]
        public void Coverage(string packageName)
        {
            _footPrint = GodotBuildIdentity.Current;
            var footPrintFilePath = _fileSystem.GetSandboxAppFootPrintFilePath();
            FileUtility.WriteAllText(footPrintFilePath, _footPrint);
            AssetSystemLogger.Warning($"Save application foot print : {_footPrint}");
        }
    }
}
