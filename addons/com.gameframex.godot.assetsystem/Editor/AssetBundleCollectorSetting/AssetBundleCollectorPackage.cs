using System;
using System.Collections.Generic;

namespace GameFrameX.AssetSystem.Editor
{
    /// <summary>
    /// 资源收集包裹(语义对齐 GameFrameX Godot 版)
    /// </summary>
    [Serializable]
    public class AssetBundleCollectorPackage
    {
        /// <summary>
        /// 包裹名称
        /// </summary>
        public string PackageName = string.Empty;

        /// <summary>
        /// 包裹描述
        /// </summary>
        public string PackageDesc = string.Empty;

        /// <summary>
        /// 启用可寻址资源定位
        /// </summary>
        public bool EnableAddressable = false;

        /// <summary>
        /// 资源定位地址大小写不敏感
        /// </summary>
        public bool LocationToLower = false;

        /// <summary>
        /// 包含资源 GUID 数据
        /// </summary>
        public bool IncludeAssetGUID = false;

        /// <summary>
        /// 资源忽略规则
        /// </summary>
        public EIgnoreRule IgnoreRule = EIgnoreRule.NormalIgnore;

        /// <summary>
        /// 分组列表
        /// </summary>
        public List<AssetBundleCollectorGroup> Groups = new List<AssetBundleCollectorGroup>();

        /// <summary>
        /// 检测配置错误
        /// </summary>
        public void CheckConfigError()
        {
            foreach (var group in Groups)
            {
                group.CheckConfigError();
            }
        }

        /// <summary>
        /// 获取所有的资源标签
        /// </summary>
        public List<string> GetAllTags()
        {
            var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var group in Groups)
            {
                AppendTags(result, group.AssetTags);
                foreach (var collector in group.Collectors)
                {
                    AppendTags(result, collector.AssetTags);
                }
            }

            return new List<string>(result);
        }

        private static void AppendTags(HashSet<string> result, string tagsText)
        {
            if (string.IsNullOrEmpty(tagsText))
            {
                return;
            }

            var tags = tagsText.Split(';');
            foreach (var tag in tags)
            {
                var trimmed = tag.Trim();
                if (trimmed.Length > 0)
                {
                    result.Add(trimmed);
                }
            }
        }
    }
}
