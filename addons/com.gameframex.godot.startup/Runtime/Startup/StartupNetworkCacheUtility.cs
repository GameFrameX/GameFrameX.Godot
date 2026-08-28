// ==========================================================================================
//  GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//  GameFrameX organization and its derivative projects' copyrights, trademarks, patents, and related rights
//  均受中华人民共和国及相关国际法律法规保护。
//  are protected by the laws of the People's Republic of China and relevant international regulations.
//
//  使用本项目须严格遵守相应法律法规及开源许可证之规定。
//  Usage of this project must strictly comply with applicable laws, regulations, and open-source licenses.
//
//  本项目采用 MIT 许可证与 Apache License 2.0 双许可证分发，
//  This project is dual-licensed under the MIT License and Apache License 2.0,
//  完整许可证文本请参见源代码根目录下的 LICENSE 文件。
//  please refer to the LICENSE file in the root directory of the source code for the full license text.
//
//  禁止利用本项目实施任何危害国家安全、破坏社会秩序、
//  It is prohibited to use this project to engage in any activities that endanger national security, disrupt social order,
//  侵犯他人合法权益等法律法规所禁止的行为！
//  or infringe upon the legitimate rights and interests of others, as prohibited by laws and regulations!
//  因基于本项目二次开发所产生的一切法律纠纷与责任，
//  Any legal disputes or liabilities arising from secondary development based on this project
//  本项目组织与贡献者概不承担。
//  shall be borne solely by the developer; the project organization and contributors assume no responsibility.
//
//  GitHub 仓库：https://github.com/GameFrameX
//  GitHub Repository: https://github.com/GameFrameX
//  Gitee  仓库：https://gitee.com/GameFrameX
//  Gitee Repository:  https://gitee.com/GameFrameX
//  CNB  仓库：https://cnb.cool/GameFrameX
//  CNB Repository: https://cnb.cool/GameFrameX
//  官方文档：https://gameframex.doc.alianblank.com/
//  Official Documentation: https://gameframex.doc.alianblank.com/
// ==========================================================================================

using System;
using GameFrameX.Asset.Runtime;
using GameFrameX.Fsm.Runtime;
using GameFrameX.GlobalConfig.Runtime;
using GameFrameX.Procedure.Runtime;
using GameFrameX.Runtime;
using GameFrameX.Setting.Runtime;
using GameFrameX.Web.Runtime;

namespace GameFrameX.Startup.Runtime
{
    /// <summary>
    /// 启动网络缓存工具类。
    /// </summary>
    /// <remarks>
    /// Startup network cache utility class. Provides methods to save and retrieve cached network response data
    /// during the startup procedure, reducing network requests on subsequent launches.
    /// </remarks>
    internal static class StartupNetworkCacheUtility
    {
        private const string CachePrefix = "GameFrameX.Startup.NetworkCache";
        private const string GlobalInfoKey = CachePrefix + ".GlobalInfo";
        private const string AppVersionKey = CachePrefix + ".AppVersion";
        private const string AssetPackageVersionKey = CachePrefix + ".AssetPackageVersion";

        /// <summary>
        /// 保存全局信息到缓存。
        /// </summary>
        /// <remarks>
        /// Saves the global info response to the local cache.
        /// </remarks>
        /// <param name="responseJson">全局信息响应JSON字符串 / Global info response JSON string</param>
        public static void SaveGlobalInfo(string responseJson)
        {
            SaveString(GlobalInfoKey, responseJson);
        }

        /// <summary>
        /// 尝试应用缓存的全局信息。
        /// </summary>
        /// <remarks>
        /// Attempts to apply the cached global info. Returns true if cache exists and is successfully applied.
        /// </remarks>
        /// <returns>如果缓存存在且成功应用则返回 <c>true</c>；否则返回 <c>false</c> / <c>true</c> if cache exists and is successfully applied; otherwise <c>false</c></returns>
        public static bool TryApplyCachedGlobalInfo()
        {
            if (!TryGetString(GlobalInfoKey, out var responseJson))
            {
                return false;
            }

            try
            {
                var responseGlobalInfo = responseJson.ToHttpJsonResultData<ResponseGlobalInfo>();
                if (!responseGlobalInfo.IsSuccess)
                {
                    return false;
                }

                ApplyGlobalInfo(responseJson, responseGlobalInfo.Data);
                return true;
            }
            catch (Exception exception)
            {
                Log.Error(exception);
                return false;
            }
        }

        /// <summary>
        /// 应用全局信息到游戏配置。
        /// </summary>
        /// <remarks>
        /// Applies the global info to the game configuration, including version check URLs and content data.
        /// </remarks>
        /// <param name="responseJson">全局信息响应JSON字符串 / Global info response JSON string</param>
        /// <param name="data">全局信息数据对象 / Global info data object</param>
        public static void ApplyGlobalInfo(string responseJson, ResponseGlobalInfo data)
        {
            var globalConfig = GameEntry.GetComponent<GlobalConfigComponent>();
            globalConfig.SetOriginalData(responseJson);
            globalConfig.CheckAppVersionUrl = data.CheckAppVersionUrl;
            globalConfig.CheckResourceVersionUrl = data.CheckResourceVersionUrl;
            globalConfig.Content = data.Content;
            globalConfig.SetGlobalConfig(data);
        }

        /// <summary>
        /// 保存应用版本信息到缓存。
        /// </summary>
        /// <remarks>
        /// Saves the application version info to the local cache.
        /// </remarks>
        /// <param name="dataJson">应用版本信息JSON字符串 / Application version info JSON string</param>
        public static void SaveAppVersionInfo(string dataJson)
        {
            SaveString(AppVersionKey, dataJson);
        }

        /// <summary>
        /// 尝试从缓存获取应用版本信息。
        /// </summary>
        /// <remarks>
        /// Attempts to retrieve the cached application version info.
        /// </remarks>
        /// <param name="gameAppVersion">应用版本信息 / Application version info</param>
        /// <returns>如果缓存存在且成功解析则返回 <c>true</c>；否则返回 <c>false</c> / <c>true</c> if cache exists and is successfully parsed; otherwise <c>false</c></returns>
        public static bool TryGetCachedAppVersionInfo(out ResponseGameAppVersion gameAppVersion)
        {
            gameAppVersion = null;
            if (!TryGetString(AppVersionKey, out var dataJson))
            {
                return false;
            }

            try
            {
                gameAppVersion = Utility.Json.ToObject<ResponseGameAppVersion>(dataJson);
                return gameAppVersion != null;
            }
            catch (Exception exception)
            {
                Log.Error(exception);
                return false;
            }
        }

        /// <summary>
        /// 保存资源包版本信息到缓存。
        /// </summary>
        /// <remarks>
        /// Saves the asset package version info to the local cache.
        /// </remarks>
        /// <param name="dataJson">资源包版本信息JSON字符串 / Asset package version info JSON string</param>
        public static void SaveAssetPackageVersionInfo(string dataJson)
        {
            SaveString(AssetPackageVersionKey, dataJson);
        }

        /// <summary>
        /// 尝试应用缓存的资源包版本信息。
        /// </summary>
        /// <remarks>
        /// Attempts to apply the cached asset package version info to the procedure owner.
        /// </remarks>
        /// <param name="procedureOwner">流程所有者，用于存储资源包信息 / Procedure owner, used to store asset package info</param>
        /// <returns>如果缓存存在且成功应用则返回 <c>true</c>；否则返回 <c>false</c> / <c>true</c> if cache exists and is successfully applied; otherwise <c>false</c></returns>
        public static bool TryApplyCachedAssetPackageVersionInfo(IFsm<IProcedureManager> procedureOwner)
        {
            if (!TryGetString(AssetPackageVersionKey, out var dataJson))
            {
                return false;
            }

            try
            {
                var packageVersion = Utility.Json.ToObject<ResponseGameAssetPackageVersion>(dataJson);
                if (packageVersion == null)
                {
                    return false;
                }

                ApplyAssetPackageVersionInfo(procedureOwner, packageVersion);
                return true;
            }
            catch (Exception exception)
            {
                Log.Error(exception);
                return false;
            }
        }

        /// <summary>
        /// 应用资源包版本信息到流程。
        /// </summary>
        /// <remarks>
        /// Applies the asset package version info to the procedure owner by using the server provided package path,
        /// or constructing one from root path, package name, platform, app version, channel, asset package name, and version.
        /// </remarks>
        /// <param name="procedureOwner">流程所有者，用于存储资源包信息 / Procedure owner, used to store asset package info</param>
        /// <param name="packageVersion">资源包版本信息 / Asset package version info</param>
        public static void ApplyAssetPackageVersionInfo(IFsm<IProcedureManager> procedureOwner, ResponseGameAssetPackageVersion packageVersion)
        {
            var packageUrl = GetAssetPackageUrl(packageVersion);

            var urlValue = ReferencePool.Acquire<VarString>();
            urlValue.SetValue(packageUrl);
            procedureOwner.SetData(AssetComponent.BuildInPackageName, urlValue);

            var versionValue = ReferencePool.Acquire<VarString>();
            versionValue.SetValue(packageVersion.Version);
            procedureOwner.SetData(AssetComponent.BuildInPackageName + "Version", versionValue);
        }

        private static string GetAssetPackageUrl(ResponseGameAssetPackageVersion packageVersion)
        {
            if (!string.IsNullOrWhiteSpace(packageVersion.AssetPackagePath))
            {
                return packageVersion.AssetPackagePath;
            }

            return EnsureTrailingSlash(PathHelper.Combine(
                packageVersion.RootPath,
                packageVersion.PackageName,
                packageVersion.Platform,
                packageVersion.AppVersion,
                packageVersion.Channel,
                packageVersion.AssetPackageName,
                packageVersion.Version));
        }

        private static string EnsureTrailingSlash(string path)
        {
            if (path.EndsWithFast("/") || path.EndsWithFast("\\"))
            {
                return path;
            }

            return path + "/";
        }

        private static bool TryGetString(string key, out string value)
        {
            value = string.Empty;

            // 说明：Godot 主工程将全部 addons 包编译进同一程序集，setting 包必然可用，
            // 无需 Unity 版的 ENABLE_GAME_FRAME_X_SETTING 条件编译，仅保留组件缺失防御。
            var setting = GameEntry.GetComponent<SettingComponent>();
            if (setting == null || !setting.HasSetting(key))
            {
                return false;
            }

            value = setting.GetString(key, string.Empty);
            return !string.IsNullOrEmpty(value);
        }

        private static void SaveString(string key, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            var setting = GameEntry.GetComponent<SettingComponent>();
            if (setting == null)
            {
                return;
            }

            setting.SetString(key, value);
            setting.Save();
        }
    }
}
