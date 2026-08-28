using System;
using System.Collections.Generic;

namespace GameFrameX.AssetSystem.Editor
{
    /// <summary>
    /// 资源收集分组(语义对齐 YooAsset Unity 版)
    /// </summary>
    [Serializable]
    public class AssetBundleCollectorGroup
    {
        /// <summary>
        /// 分组名称
        /// </summary>
        public string GroupName = string.Empty;

        /// <summary>
        /// 分组描述
        /// </summary>
        public string GroupDesc = string.Empty;

        /// <summary>
        /// 资源分类标签(分号分隔)
        /// </summary>
        public string AssetTags = string.Empty;

        /// <summary>
        /// 分组激活规则
        /// </summary>
        public EActiveRule ActiveRule = EActiveRule.EnableGroup;

        /// <summary>
        /// 分组的收集器列表
        /// </summary>
        public List<AssetBundleCollector> Collectors = new List<AssetBundleCollector>();

        /// <summary>
        /// 分组是否激活
        /// </summary>
        public bool IsActive()
        {
            return CollectorRuleStrategy.IsActiveGroup(ActiveRule);
        }

        /// <summary>
        /// 检测配置错误
        /// </summary>
        public void CheckConfigError()
        {
            if (IsActive() == false)
            {
                return;
            }

            foreach (var collector in Collectors)
            {
                if (collector.IsValid() == false)
                {
                    throw new Exception($"Invalid collector in group : {GroupName}, CollectPath : {collector.CollectPath}");
                }
            }
        }
    }
}
