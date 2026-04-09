using System;
using System.Collections.Generic;

namespace GameFrameX.AssetSystem
{
    /// <summary>
    /// 内置资源清单目录
    /// </summary>
    [AssetSystemPreserve]
    internal class DefaultBuildinFileCatalog
    {
        [AssetSystemPreserve]
        [Serializable]
        public class FileWrapper
        {
            public string BundleGUID;
            public string FileName;

            [AssetSystemPreserve]
            public FileWrapper(string bundleGUID, string fileName)
            {
                BundleGUID = bundleGUID;
                FileName = fileName;
            }
        }

        /// <summary>
        /// 包裹名称
        /// </summary>
        public string PackageName;

        /// <summary>
        /// 包裹版本
        /// </summary>
        public string PackageVersion;

        /// <summary>
        /// 文件列表
        /// </summary>
        public List<FileWrapper> Wrappers = new();
    }
}
