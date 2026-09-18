using System;
using System.Collections.Generic;

namespace GameFrameX.AssetSystem.Editor
{
    /// <summary>
    /// Collector 配置根数据类(语义对齐 GameFrameX Godot 版,ScriptableObject 改为普通可序列化类)
    /// </summary>
    [Serializable]
    public class AssetBundleCollectorSetting
    {
        /// <summary>
        /// 显示包裹列表视图
        /// </summary>
        public bool ShowPackageView = false;

        /// <summary>
        /// 是否显示编辑器别名
        /// </summary>
        public bool ShowEditorAlias = false;

        /// <summary>
        /// 资源包名唯一化
        /// </summary>
        public bool UniqueBundleName = false;

        /// <summary>
        /// 包裹列表
        /// </summary>
        public List<AssetBundleCollectorPackage> Packages = new List<AssetBundleCollectorPackage>();

        /// <summary>
        /// 清空所有数据
        /// </summary>
        public void ClearAll()
        {
            ShowPackageView = false;
            UniqueBundleName = false;
            ShowEditorAlias = false;
            Packages.Clear();
        }

        /// <summary>
        /// 检测所有包裹配置错误
        /// </summary>
        public void CheckAllPackageConfigError()
        {
            foreach (var package in Packages)
            {
                package.CheckConfigError();
            }
        }

        /// <summary>
        /// 获取包裹类
        /// </summary>
        public AssetBundleCollectorPackage GetPackage(string packageName)
        {
            foreach (var package in Packages)
            {
                if (package.PackageName == packageName)
                {
                    return package;
                }
            }

            throw new Exception($"Not found package : {packageName}");
        }

        /// <summary>
        /// 获取包裹所有的资源标签
        /// </summary>
        public List<string> GetPackageAllTags(string packageName)
        {
            var package = GetPackage(packageName);
            return package.GetAllTags();
        }
    }
}
