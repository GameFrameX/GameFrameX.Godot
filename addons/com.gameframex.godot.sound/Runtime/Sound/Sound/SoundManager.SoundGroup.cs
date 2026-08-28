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



using System.Collections.Generic;
using GameFrameX.Runtime;

namespace GameFrameX.Sound.Runtime
{
    public sealed partial class SoundManager : GameFrameworkModule, ISoundManager
    {
        /// <summary>
        /// 声音组。
        /// </summary>
        internal sealed class SoundGroup : ISoundGroup
        {
            private readonly string m_Name;
            private readonly ISoundGroupHelper m_SoundGroupHelper;
            private readonly List<SoundAgent> m_SoundAgents;
            private bool m_AvoidBeingReplacedBySamePriority;
            private bool m_Mute;
            private float m_Volume;

            /// <summary>
            /// 初始化声音组的新实例。
            /// </summary>
            /// <param name="name">声音组名称。</param>
            /// <param name="soundGroupHelper">声音组辅助器。</param>
            public SoundGroup(string name, ISoundGroupHelper soundGroupHelper)
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw new GameFrameworkException("Sound group name is invalid.");
                }

                if (soundGroupHelper == null)
                {
                    throw new GameFrameworkException("Sound group helper is invalid.");
                }

                m_Name = name;
                m_SoundGroupHelper = soundGroupHelper;
                m_SoundAgents = new List<SoundAgent>();
            }

            /// <summary>
            /// 获取声音组名称。
            /// </summary>
            public string Name
            {
                get { return m_Name; }
            }

            /// <summary>
            /// 获取声音代理数。
            /// </summary>
            public int SoundAgentCount
            {
                get { return m_SoundAgents.Count; }
            }

            /// <summary>
            /// 获取或设置声音组中的声音是否避免被同优先级声音替换。
            /// </summary>
            public bool AvoidBeingReplacedBySamePriority
            {
                get { return m_AvoidBeingReplacedBySamePriority; }
                set { m_AvoidBeingReplacedBySamePriority = value; }
            }

            /// <summary>
            /// 获取或设置声音组静音。
            /// </summary>
            public bool Mute
            {
                get { return m_Mute; }
                set
                {
                    m_Mute = value;
                    foreach (SoundAgent soundAgent in m_SoundAgents)
                    {
                        soundAgent.RefreshMute();
                    }
                }
            }

            /// <summary>
            /// 获取或设置声音组音量。
            /// </summary>
            public float Volume
            {
                get { return m_Volume; }
                set
                {
                    m_Volume = value;
                    foreach (SoundAgent soundAgent in m_SoundAgents)
                    {
                        soundAgent.RefreshVolume();
                    }
                }
            }

            /// <summary>
            /// 获取声音组辅助器。
            /// </summary>
            public ISoundGroupHelper Helper
            {
                get { return m_SoundGroupHelper; }
            }

            /// <summary>
            /// 增加声音代理辅助器。
            /// </summary>
            /// <param name="soundHelper">声音辅助器接口。</param>
            /// <param name="soundAgentHelper">要增加的声音代理辅助器。</param>
            public void AddSoundAgentHelper(ISoundHelper soundHelper, ISoundAgentHelper soundAgentHelper)
            {
                m_SoundAgents.Add(new SoundAgent(this, soundHelper, soundAgentHelper));
            }

            /// <summary>
            /// 是否正在播放声音。
            /// </summary>
            /// <param name="serialId">声音的序列编号。</param>
            /// <returns>正在播放则返回Ture,否则返回False,找不到指定的序列编号也会返回False</returns>
            public bool IsPlaying(int serialId)
            {
                foreach (SoundAgent soundAgent in m_SoundAgents)
                {
                    if (soundAgent.SerialId == serialId)
                    {
                        return soundAgent.IsPlaying;
                    }
                }

                return false;
            }

            /// <summary>
            /// 播放声音。代理选择算法：优先空闲代理；无空闲时按优先级抢占（选最低优先级者），同优先级按设置声音资源时间戳抢占（选最旧者）。
            /// </summary>
            /// <param name="serialId">声音的序列编号。</param>
            /// <param name="soundAsset">声音资源。</param>
            /// <param name="playSoundParams">播放声音参数。</param>
            /// <param name="errorCode">错误码。</param>
            /// <returns>用于播放的声音代理。</returns>
            public ISoundAgent PlaySound(int serialId, object soundAsset, PlaySoundParams playSoundParams, out PlaySoundErrorCode? errorCode)
            {
                errorCode = null;
                SoundAgent candidateAgent = null;
                foreach (SoundAgent soundAgent in m_SoundAgents)
                {
                    if (!soundAgent.IsPlaying)
                    {
                        candidateAgent = soundAgent;
                        break;
                    }

                    if (soundAgent.Priority < playSoundParams.Priority)
                    {
                        if (candidateAgent == null || soundAgent.Priority < candidateAgent.Priority)
                        {
                            candidateAgent = soundAgent;
                        }
                    }
                    else if (!m_AvoidBeingReplacedBySamePriority && soundAgent.Priority == playSoundParams.Priority)
                    {
                        if (candidateAgent == null || soundAgent.SetSoundAssetTime < candidateAgent.SetSoundAssetTime)
                        {
                            candidateAgent = soundAgent;
                        }
                    }
                }

                if (candidateAgent == null)
                {
                    errorCode = PlaySoundErrorCode.IgnoredDueToLowPriority;
                    return null;
                }

                if (!candidateAgent.SetSoundAsset(soundAsset))
                {
                    errorCode = PlaySoundErrorCode.SetSoundAssetFailure;
                    return null;
                }

                candidateAgent.SerialId = serialId;
                candidateAgent.Time = playSoundParams.Time;
                candidateAgent.MuteInSoundGroup = playSoundParams.MuteInSoundGroup;
                candidateAgent.Loop = playSoundParams.Loop;
                candidateAgent.Priority = playSoundParams.Priority;
                candidateAgent.VolumeInSoundGroup = playSoundParams.VolumeInSoundGroup;
                candidateAgent.Pitch = playSoundParams.Pitch;
                candidateAgent.PanStereo = playSoundParams.PanStereo;
                candidateAgent.SpatialBlend = playSoundParams.SpatialBlend;
                candidateAgent.MaxDistance = playSoundParams.MaxDistance;
                candidateAgent.DopplerLevel = playSoundParams.DopplerLevel;
                candidateAgent.Play(playSoundParams.FadeInSeconds);
                return candidateAgent;
            }

            /// <summary>
            /// 停止播放声音。
            /// </summary>
            /// <param name="serialId">要停止播放声音的序列编号。</param>
            /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
            /// <returns>是否停止播放声音成功。</returns>
            public bool StopSound(int serialId, float fadeOutSeconds)
            {
                foreach (SoundAgent soundAgent in m_SoundAgents)
                {
                    if (soundAgent.SerialId != serialId)
                    {
                        continue;
                    }

                    soundAgent.Stop(fadeOutSeconds);
                    return true;
                }

                return false;
            }

            /// <summary>
            /// 暂停播放声音。
            /// </summary>
            /// <param name="serialId">要暂停播放声音的序列编号。</param>
            /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
            /// <returns>是否暂停播放声音成功。</returns>
            public bool PauseSound(int serialId, float fadeOutSeconds)
            {
                foreach (SoundAgent soundAgent in m_SoundAgents)
                {
                    if (soundAgent.SerialId != serialId)
                    {
                        continue;
                    }

                    soundAgent.Pause(fadeOutSeconds);
                    return true;
                }

                return false;
            }

            /// <summary>
            /// 恢复播放声音。
            /// </summary>
            /// <param name="serialId">要恢复播放声音的序列编号。</param>
            /// <param name="fadeInSeconds">声音淡入时间，以秒为单位。</param>
            /// <returns>是否恢复播放声音成功。</returns>
            public bool ResumeSound(int serialId, float fadeInSeconds)
            {
                foreach (SoundAgent soundAgent in m_SoundAgents)
                {
                    if (soundAgent.SerialId != serialId)
                    {
                        continue;
                    }

                    soundAgent.Resume(fadeInSeconds);
                    return true;
                }

                return false;
            }

            /// <summary>
            /// 停止所有已加载的声音。
            /// </summary>
            public void StopAllLoadedSounds()
            {
                foreach (SoundAgent soundAgent in m_SoundAgents)
                {
                    if (soundAgent.IsPlaying)
                    {
                        soundAgent.Stop();
                    }
                }
            }

            /// <summary>
            /// 停止所有已加载的声音。
            /// </summary>
            /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
            public void StopAllLoadedSounds(float fadeOutSeconds)
            {
                foreach (SoundAgent soundAgent in m_SoundAgents)
                {
                    if (soundAgent.IsPlaying)
                    {
                        soundAgent.Stop(fadeOutSeconds);
                    }
                }
            }
        }
    }
}
