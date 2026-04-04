using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace YooAsset.Editor
{
    public static class AssetBundleBuilderHelper
    {
        private static bool UseLegacyUnityPathLayout()
        {
            string key = $"{Application.productName}_{nameof(AssetBundleBuilderHelper)}_UseLegacyUnityPathLayout";
            return EditorPrefs.GetBool(key, false);
        }

        /// <summary>
        /// 获取默认的输出根目录
        /// </summary>
        public static string GetDefaultBuildOutputRoot()
        {
            string projectPath = EditorTools.GetProjectPath();
            if (UseLegacyUnityPathLayout())
            {
                return $"{projectPath}/Bundles";
            }

            return $"{projectPath}/Builds/Godot";
        }

        /// <summary>
        /// 获取流文件夹路径
        /// </summary>
        public static string GetStreamingAssetsRoot()
        {
            if (UseLegacyUnityPathLayout())
            {
                return $"{Application.dataPath}/StreamingAssets/{YooAssetSettingsData.Setting.DefaultYooFolderName}/";
            }

            string projectPath = EditorTools.GetProjectPath();
            return $"{projectPath}/Builds/GodotBuiltin";
        }
    }
}
