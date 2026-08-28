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



using System;

namespace GameFrameX.Sound.Runtime
{
    /// <summary>
    /// 声音代理辅助器接口。
    /// </summary>
    public interface ISoundAgentHelper
    {
        /// <summary>
        /// 获取当前是否正在播放。
        /// </summary>
        bool IsPlaying { get; }

        /// <summary>
        /// 获取声音长度。
        /// </summary>
        float Length { get; }

        /// <summary>
        /// 获取或设置播放位置。
        /// </summary>
        float Time { get; set; }

        /// <summary>
        /// 获取或设置是否静音。
        /// </summary>
        bool Mute { get; set; }

        /// <summary>
        /// 获取或设置是否循环播放。
        /// </summary>
        bool Loop { get; set; }

        /// <summary>
        /// 获取或设置声音优先级。
        /// </summary>
        int Priority { get; set; }

        /// <summary>
        /// 获取或设置音量大小。
        /// </summary>
        float Volume { get; set; }

        /// <summary>
        /// 获取或设置声音音调。
        /// </summary>
        float Pitch { get; set; }

        /// <summary>
        /// 获取或设置声音立体声声相。
        /// </summary>
        float PanStereo { get; set; }

        /// <summary>
        /// 获取或设置声音空间混合量。
        /// </summary>
        float SpatialBlend { get; set; }

        /// <summary>
        /// 获取或设置声音最大距离。
        /// </summary>
        float MaxDistance { get; set; }

        /// <summary>
        /// 获取或设置声音多普勒等级。
        /// </summary>
        float DopplerLevel { get; set; }

        /// <summary>
        /// 重置声音代理事件。
        /// </summary>
        event EventHandler<ResetSoundAgentEventArgs> ResetSoundAgent;

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="fadeInSeconds">声音淡入时间，以秒为单位。</param>
        void Play(float fadeInSeconds);

        /// <summary>
        /// 停止播放声音。
        /// </summary>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        void Stop(float fadeOutSeconds);

        /// <summary>
        /// 暂停播放声音。
        /// </summary>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        void Pause(float fadeOutSeconds);

        /// <summary>
        /// 恢复播放声音。
        /// </summary>
        /// <param name="fadeInSeconds">声音淡入时间，以秒为单位。</param>
        void Resume(float fadeInSeconds);

        /// <summary>
        /// 重置声音代理辅助器。
        /// </summary>
        void Reset();

        /// <summary>
        /// 设置声音资源。
        /// </summary>
        /// <param name="soundAsset">声音资源。</param>
        /// <returns>是否设置声音资源成功。</returns>
        bool SetSoundAsset(object soundAsset);
    }
}
