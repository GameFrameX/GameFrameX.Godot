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
//
//  GitHub 仓库：https://github.com/GameFrameX
//  GitHub Repository: https://github.com/GameFrameX
//  Gitee  仓库：https://gitee.com/GameFrameX
//  Gitee Repository:  https://gitee.com/GameFrameX
//  官方文档：https://gameframex.doc.alianblank.com/
//  Official Documentation: https://gameframex.doc.alianblank.com/
// ==========================================================================================


namespace GameFrameX.Sound.Runtime
{
    /// <summary>
    /// 声音相关常量。
    /// </summary>
    internal static class Constant
    {
        /// <summary>
        /// 默认播放位置。
        /// </summary>
        internal const float DefaultTime = 0f;

        /// <summary>
        /// 默认是否静音。
        /// </summary>
        internal const bool DefaultMute = false;

        /// <summary>
        /// 默认是否循环播放。
        /// </summary>
        internal const bool DefaultLoop = false;

        /// <summary>
        /// 默认优先级。
        /// </summary>
        internal const int DefaultPriority = 0;

        /// <summary>
        /// 默认音量。
        /// </summary>
        internal const float DefaultVolume = 1f;

        /// <summary>
        /// 默认声音淡入时间，以秒为单位。
        /// </summary>
        internal const float DefaultFadeInSeconds = 0f;

        /// <summary>
        /// 默认声音淡出时间，以秒为单位。
        /// </summary>
        internal const float DefaultFadeOutSeconds = 0f;

        /// <summary>
        /// 默认声音音调。
        /// </summary>
        internal const float DefaultPitch = 1f;

        /// <summary>
        /// 默认声音立体声声相。
        /// </summary>
        internal const float DefaultPanStereo = 0f;

        /// <summary>
        /// 默认声音空间混合量。
        /// </summary>
        internal const float DefaultSpatialBlend = 0f;

        /// <summary>
        /// 默认声音最大距离。
        /// </summary>
        internal const float DefaultMaxDistance = 100f;

        /// <summary>
        /// 默认声音多普勒等级。
        /// </summary>
        internal const float DefaultDopplerLevel = 1f;
    }
}
