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
using System.Collections.Generic;
using System.Text.Json.Serialization;
using GameFrameX.Runtime;

namespace GameFrameX.Startup.Runtime
{
    /// <summary>
    /// 启动HTTP参数，用于启动请求的基础参数。
    /// </summary>
    /// <remarks>
    /// HTTP base parameters used by startup requests.
    /// </remarks>
    public class StartupHttpParams : IStartupHttpParams
    {
        /// <summary>
        /// 语言代码。
        /// </summary>
        /// <remarks>
        /// Language code.
        /// </remarks>
        [JsonInclude]
        public string Language = string.Empty;

        /// <summary>
        /// 用户语言。
        /// </summary>
        /// <remarks>
        /// User language.
        /// </remarks>
        [JsonInclude]
        public string UserLanguage = string.Empty;

        /// <summary>
        /// 应用程序版本。
        /// </summary>
        /// <remarks>
        /// Application version.
        /// </remarks>
        [JsonInclude]
        public string AppVersion = string.Empty;

        /// <summary>
        /// 设备唯一标识符。
        /// </summary>
        /// <remarks>
        /// Device unique identifier.
        /// </remarks>
        [JsonInclude]
        public string DeviceUniqueIdentifier = string.Empty;

        /// <summary>
        /// 平台标识。
        /// </summary>
        /// <remarks>
        /// Platform identifier.
        /// </remarks>
        [JsonInclude]
        public string Platform = string.Empty;

        /// <summary>
        /// 包名。
        /// </summary>
        /// <remarks>
        /// Package name.
        /// </remarks>
        [JsonInclude]
        public string PackageName = string.Empty;

        /// <summary>
        /// 渠道标识。
        /// </summary>
        /// <remarks>
        /// Channel identifier.
        /// </remarks>
        [JsonInclude]
        public string Channel = string.Empty;

        /// <summary>
        /// 子渠道标识。
        /// </summary>
        /// <remarks>
        /// Sub-channel identifier.
        /// </remarks>
        [JsonInclude]
        public string SubChannel = string.Empty;

        /// <summary>
        /// 从启动选项创建HTTP参数实例。
        /// </summary>
        /// <remarks>
        /// Creates HTTP parameters instance from startup options.
        /// </remarks>
        /// <param name="options">启动选项 / Startup options</param>
        /// <returns>HTTP参数实例 / HTTP parameters instance</returns>
        /// <exception cref="ArgumentNullException">当 <paramref name="options"/> 为 null 时抛出 / Thrown when <paramref name="options"/> is null</exception>
        public static StartupHttpParams FromOptions(StartupOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return new StartupHttpParams
            {
                PackageName = options.PackageName ?? string.Empty,
                Channel = options.Channel ?? string.Empty,
                SubChannel = options.SubChannel ?? string.Empty,
            };
        }

        /// <summary>
        /// 将参数序列化为JSON字符串。
        /// </summary>
        /// <remarks>
        /// Serializes parameters to JSON string.
        /// </remarks>
        /// <returns>JSON字符串 / JSON string</returns>
        public virtual string ToJson()
        {
            Language = Language ?? string.Empty;
            UserLanguage = UserLanguage ?? string.Empty;
            AppVersion = AppVersion ?? string.Empty;
            DeviceUniqueIdentifier = DeviceUniqueIdentifier ?? string.Empty;
            Platform = Platform ?? string.Empty;
            PackageName = PackageName ?? string.Empty;
            Channel = Channel ?? string.Empty;
            SubChannel = SubChannel ?? string.Empty;

            return Utility.Json.ToJson(this);
        }

        /// <summary>
        /// 将参数转换为字典。
        /// </summary>
        /// <remarks>
        /// Converts parameters to dictionary.
        /// </remarks>
        /// <returns>参数字典 / Parameters dictionary</returns>
        public virtual Dictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                ["Language"] = Language ?? string.Empty,
                ["UserLanguage"] = UserLanguage ?? string.Empty,
                ["AppVersion"] = AppVersion ?? string.Empty,
                ["DeviceUniqueIdentifier"] = DeviceUniqueIdentifier ?? string.Empty,
                ["Platform"] = Platform ?? string.Empty,
                ["PackageName"] = PackageName ?? string.Empty,
                ["Channel"] = Channel ?? string.Empty,
                ["SubChannel"] = SubChannel ?? string.Empty,
            };
        }
    }
}
