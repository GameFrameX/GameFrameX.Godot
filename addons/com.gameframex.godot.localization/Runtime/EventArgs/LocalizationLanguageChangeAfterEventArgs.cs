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
//  Any legal disputes and liabilities arising from secondary development based on this project
//  本项目组织与贡献者概不承担。
//  shall be borne solely by the developer; the project organization and contributors assume no responsibility.

using GameFrameX.Event.Runtime;
using GameFrameX.Runtime;

namespace GameFrameX.Localization.Runtime
{
    /// <summary>
    /// 本地化语言改变后事件。
    /// </summary>
    public sealed class LocalizationLanguageChangeAfterEventArgs : GameEventArgs
    {
        /// <summary>
        /// 本地化语言改变后事件编号。
        /// </summary>
        public static readonly string EventId = typeof(LocalizationLanguageChangeAfterEventArgs).FullName;

        /// <summary>
        /// 当前语言。
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// 未知本地化
        /// </summary>
        const string UnknownLocalization = "zxx";

        /// <summary>
        /// 旧的语言。
        /// </summary>
        public string OldLanguage { get; set; }

        /// <summary>
        /// 初始化本地化语言改变后事件的新实例。
        /// </summary>
        public LocalizationLanguageChangeAfterEventArgs()
        {
            OldLanguage = UnknownLocalization;
            Language = UnknownLocalization;
        }

        /// <summary>
        /// 创建本地化语言改变后事件。
        /// </summary>
        /// <param name="oldLanguage">旧的语言。</param>
        /// <param name="language">当前语言。</param>
        /// <returns>创建的本地化语言改变后事件。</returns>
        public static LocalizationLanguageChangeAfterEventArgs Create(string oldLanguage, string language)
        {
            LocalizationLanguageChangeAfterEventArgs localizationLanguageChangeEventArgs = ReferencePool.Acquire<LocalizationLanguageChangeAfterEventArgs>();
            localizationLanguageChangeEventArgs.OldLanguage = oldLanguage;
            localizationLanguageChangeEventArgs.Language = language;
            return localizationLanguageChangeEventArgs;
        }

        /// <summary>
        /// 清除事件参数。
        /// </summary>
        public override void Clear()
        {
            OldLanguage = UnknownLocalization;
            Language = UnknownLocalization;
        }

        /// <summary>
        /// 获取事件编号。
        /// </summary>
        /// <returns>事件编号。</returns>
        public override string Id
        {
            get { return EventId; }
        }
    }
}
