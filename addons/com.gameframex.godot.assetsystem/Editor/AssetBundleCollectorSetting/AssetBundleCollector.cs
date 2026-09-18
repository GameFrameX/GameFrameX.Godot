using System;

namespace GameFrameX.AssetSystem.Editor
{
    /// <summary>
    /// 资源收集器(语义对齐 GameFrameX Godot 版,规则字段由类名改为枚举直存)
    /// </summary>
    [Serializable]
    public class AssetBundleCollector
    {
        /// <summary>
        /// 收集路径(支持 res:// 下的文件夹或单个资源文件)
        /// </summary>
        public string CollectPath = string.Empty;

        /// <summary>
        /// 收集器的 GUID
        /// </summary>
        public string CollectorGUID = string.Empty;

        /// <summary>
        /// 收集器类型
        /// </summary>
        public ECollectorType CollectorType = ECollectorType.MainAssetCollector;

        /// <summary>
        /// 寻址规则
        /// </summary>
        public EAddressRule AddressRule = EAddressRule.AddressByFileName;

        /// <summary>
        /// 打包规则
        /// </summary>
        public EPackRule PackRule = EPackRule.PackDirectory;

        /// <summary>
        /// 过滤规则
        /// </summary>
        public EFilterRule FilterRule = EFilterRule.CollectAll;

        /// <summary>
        /// 资源分类标签(分号分隔)
        /// </summary>
        public string AssetTags = string.Empty;

        /// <summary>
        /// 用户自定义数据
        /// </summary>
        public string UserData = string.Empty;

        /// <summary>
        /// 收集器是否有效
        /// </summary>
        public bool IsValid()
        {
            if (string.IsNullOrEmpty(CollectPath))
            {
                return false;
            }

            if (CollectorType == ECollectorType.None)
            {
                return false;
            }

            return true;
        }
    }
}
