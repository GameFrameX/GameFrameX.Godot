namespace GameFrameX.AssetSystem
{
    [AssetSystemPreserve]
    public static class AssetSystemSettingsData
    {
        private static AssetSystemSettings _setting = null;

        public static AssetSystemSettings Setting
        {
            get
            {
                if (_setting == null)
                {
                    LoadSettingData();
                }

                return _setting;
            }
        }

        /// <summary>
        /// 加载配置文件
        /// </summary>
        [AssetSystemPreserve]
        private static void LoadSettingData()
        {
                _setting = AssetSystemResources.Load<AssetSystemSettings>("AssetSystemSettings");
            if (_setting == null)
            {
                AssetSystemLogger.Log("AssetSystem use default settings.");
                _setting = new AssetSystemSettings();
            }
            else
            {
                AssetSystemLogger.Log("AssetSystem use user settings.");
            }
        }

        /// <summary>
        /// 获取构建报告文件名
        /// </summary>
        [AssetSystemPreserve]
        public static string GetReportFileName(string packageName, string packageVersion)
        {
            return $"{AssetSystemSettings.ReportFileName}_{packageName}_{packageVersion}.json";
        }

        /// <summary>
        /// 获取清单文件完整名称
        /// </summary>
        [AssetSystemPreserve]
        public static string GetManifestBinaryFileName(string packageName, string packageVersion)
        {
            return $"{Setting.ManifestFileName}_{packageName}_{packageVersion}.bytes";
        }

        /// <summary>
        /// 获取清单文件完整名称
        /// </summary>
        [AssetSystemPreserve]
        public static string GetManifestJsonFileName(string packageName, string packageVersion)
        {
            return $"{Setting.ManifestFileName}_{packageName}_{packageVersion}.json";
        }

        /// <summary>
        /// 获取包裹的哈希文件完整名称
        /// </summary>
        [AssetSystemPreserve]
        public static string GetPackageHashFileName(string packageName, string packageVersion)
        {
            return $"{Setting.ManifestFileName}_{packageName}_{packageVersion}.hash";
        }

        /// <summary>
        /// 获取包裹的版本文件完整名称
        /// </summary>
        [AssetSystemPreserve]
        public static string GetPackageVersionFileName(string packageName)
        {
            return $"{Setting.ManifestFileName}_{packageName}.version";
        }
    }
}
