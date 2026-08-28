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
//  CNB Repository:  https://cnb.cool/GameFrameX
//  官方文档：https://gameframex.doc.alianblank.com/
//  Official Documentation: https://gameframex.doc.alianblank.com/
// ==========================================================================================

using System;
using Godot;

namespace GameFrameX.SystemInfo.Runtime
{
    /// <summary>
    /// 设备唯一标识符工具类。
    /// </summary>
    public static class BlankDeviceUniqueIdentifier
    {
        private const string CacheFilePath = "user://GameFrameXDeviceUniqueIdentifier.cfg";
        private const string SectionName = "SystemInfo";
        private const string CacheKey = "DeviceUniqueIdentifierID";
        private const string WebStorageKey = "GameFrameX.DeviceUniqueIdentifier";

        private static string m_CachedId;

        /// <summary>
        /// 获取设备的 OAID（开放广告标识符）。
        /// Godot 无广告标识相关 API，降级为设备唯一标识符。
        /// </summary>
        public static string DeviceGetOaid
        {
            get
            {
                return DeviceUniqueIdentifier;
            }
        }

        /// <summary>
        /// 获取设备的 IDFA（苹果广告标识符）。
        /// Godot 无广告标识相关 API，降级为设备唯一标识符。
        /// </summary>
        public static string DeviceGetIdfa
        {
            get
            {
                return DeviceUniqueIdentifier;
            }
        }

        /// <summary>
        /// 获取设备的 IMEI（国际移动设备识别码）。
        /// Godot 无 IMEI 相关 API，降级为设备唯一标识符。
        /// </summary>
        public static string DeviceGetImei
        {
            get
            {
                return DeviceUniqueIdentifier;
            }
        }

        /// <summary>
        /// 获取设备唯一标识符。
        /// 桌面/移动端使用 OS.GetUniqueId()；Web 平台通过 localStorage 持久化；
        /// 结果经本地 user:// 文件缓存，避免重复计算。
        /// </summary>
        public static string DeviceUniqueIdentifier
        {
            get
            {
                if (IsValid(m_CachedId))
                {
                    return m_CachedId;
                }

                string cachedId = LoadCachedId();
                if (IsValid(cachedId))
                {
                    m_CachedId = cachedId;
                    return cachedId;
                }

                string sid = Normalize(ComputeRawId());
                if (!IsValid(sid))
                {
                    sid = Guid.NewGuid().ToString("N");
                }

                m_CachedId = sid;
                SaveCachedId(sid);
                return sid;
            }
        }

        private static string ComputeRawId()
        {
            if (OS.HasFeature("web"))
            {
                return GetWebStorageId();
            }

            return OS.GetUniqueId();
        }

        private static string GetWebStorageId()
        {
            string script = "(function(){var k='" + WebStorageKey + "';"
                + "var v=localStorage.getItem(k);"
                + "if(!v){v=crypto.randomUUID?crypto.randomUUID():(''+Date.now()+Math.random().toString(16).slice(2));"
                + "localStorage.setItem(k,v);}"
                + "return v;})()";
            Variant result = JavaScriptBridge.Eval(script);
            if (result.VariantType == Variant.Type.Nil)
            {
                return string.Empty;
            }

            return result.AsString();
        }

        private static string LoadCachedId()
        {
            ConfigFile configFile = new ConfigFile();
            Error error = configFile.Load(CacheFilePath);
            if (error != Error.Ok && error != Error.FileNotFound)
            {
                return string.Empty;
            }

            return Convert.ToString(configFile.GetValue(SectionName, CacheKey, string.Empty));
        }

        private static void SaveCachedId(string id)
        {
            ConfigFile configFile = new ConfigFile();
            configFile.Load(CacheFilePath);
            configFile.SetValue(SectionName, CacheKey, id);
            configFile.Save(CacheFilePath);
        }

        private static bool IsValid(string id)
        {
            return !string.IsNullOrEmpty(id) && id != "null" && id.Length >= 4;
        }

        private static string Normalize(string sid)
        {
            sid = (sid ?? "").Replace("-", "");
            if (sid.Length > 32)
            {
                sid = sid.Substring(0, 32);
            }

            return sid;
        }
    }
}
