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


using GameFrameX.Runtime;

namespace GameFrameX.Sound.Runtime
{
    /// <summary>
    /// 播放声音参数。
    /// </summary>
    public sealed class PlaySoundParams : IReference
    {
        /// <summary>
        /// 是否被引用。
        /// </summary>
        private bool m_Referenced;

        /// <summary>
        /// 播放位置。
        /// </summary>
        private float m_Time;

        /// <summary>
        /// 在声音组内是否静音。
        /// </summary>
        private bool m_MuteInSoundGroup;

        /// <summary>
        /// 是否循环播放。
        /// </summary>
        private bool m_Loop;

        /// <summary>
        /// 声音优先级。
        /// </summary>
        private int m_Priority;

        /// <summary>
        /// 在声音组内音量大小。
        /// </summary>
        private float m_VolumeInSoundGroup;

        /// <summary>
        /// 声音淡入时间，以秒为单位。
        /// </summary>
        private float m_FadeInSeconds;

        /// <summary>
        /// 声音音调。
        /// </summary>
        private float m_Pitch;

        /// <summary>
        /// 声音立体声声相。
        /// </summary>
        private float m_PanStereo;

        /// <summary>
        /// 声音空间混合量。
        /// </summary>
        private float m_SpatialBlend;

        /// <summary>
        /// 声音最大距离。
        /// </summary>
        private float m_MaxDistance;

        /// <summary>
        /// 声音多普勒等级。
        /// </summary>
        private float m_DopplerLevel;

        /// <summary>
        /// 初始化播放声音参数的新实例。
        /// </summary>
        public PlaySoundParams()
        {
            m_Referenced = false;
            m_Time = Constant.DefaultTime;
            m_MuteInSoundGroup = Constant.DefaultMute;
            m_Loop = Constant.DefaultLoop;
            m_Priority = Constant.DefaultPriority;
            m_VolumeInSoundGroup = Constant.DefaultVolume;
            m_FadeInSeconds = Constant.DefaultFadeInSeconds;
            m_Pitch = Constant.DefaultPitch;
            m_PanStereo = Constant.DefaultPanStereo;
            m_SpatialBlend = Constant.DefaultSpatialBlend;
            m_MaxDistance = Constant.DefaultMaxDistance;
            m_DopplerLevel = Constant.DefaultDopplerLevel;
        }

        /// <summary>
        /// 获取或设置播放位置。
        /// </summary>
        public float Time
        {
            get { return m_Time; }
            set { m_Time = value; }
        }

        /// <summary>
        /// 获取或设置在声音组内是否静音。
        /// </summary>
        public bool MuteInSoundGroup
        {
            get { return m_MuteInSoundGroup; }
            set { m_MuteInSoundGroup = value; }
        }

        /// <summary>
        /// 获取或设置是否循环播放。
        /// </summary>
        public bool Loop
        {
            get { return m_Loop; }
            set { m_Loop = value; }
        }

        /// <summary>
        /// 获取或设置声音优先级。
        /// </summary>
        public int Priority
        {
            get { return m_Priority; }
            set { m_Priority = value; }
        }

        /// <summary>
        /// 获取或设置在声音组内音量大小。
        /// </summary>
        public float VolumeInSoundGroup
        {
            get { return m_VolumeInSoundGroup; }
            set { m_VolumeInSoundGroup = value; }
        }

        /// <summary>
        /// 获取或设置声音淡入时间，以秒为单位。
        /// </summary>
        public float FadeInSeconds
        {
            get { return m_FadeInSeconds; }
            set { m_FadeInSeconds = value; }
        }

        /// <summary>
        /// 获取或设置声音音调。
        /// </summary>
        public float Pitch
        {
            get { return m_Pitch; }
            set { m_Pitch = value; }
        }

        /// <summary>
        /// 获取或设置声音立体声声相。
        /// </summary>
        public float PanStereo
        {
            get { return m_PanStereo; }
            set { m_PanStereo = value; }
        }

        /// <summary>
        /// 获取或设置声音空间混合量。
        /// </summary>
        public float SpatialBlend
        {
            get { return m_SpatialBlend; }
            set { m_SpatialBlend = value; }
        }

        /// <summary>
        /// 获取或设置声音最大距离。
        /// </summary>
        public float MaxDistance
        {
            get { return m_MaxDistance; }
            set { m_MaxDistance = value; }
        }

        /// <summary>
        /// 获取或设置声音多普勒等级。
        /// </summary>
        public float DopplerLevel
        {
            get { return m_DopplerLevel; }
            set { m_DopplerLevel = value; }
        }

        /// <summary>
        /// 获取是否被引用。
        /// </summary>
        internal bool Referenced
        {
            get { return m_Referenced; }
        }

        /// <summary>
        /// 创建播放声音参数。
        /// </summary>
        /// <param name="isLoop">是否循环播放。</param>
        /// <returns>创建的播放声音参数。</returns>
        public static PlaySoundParams Create(bool isLoop = false)
        {
            PlaySoundParams playSoundParams = ReferencePool.Acquire<PlaySoundParams>();
            playSoundParams.m_Referenced = true;
            playSoundParams.m_Loop = isLoop;
            return playSoundParams;
        }

        /// <summary>
        /// 清理播放声音参数。
        /// </summary>
        public void Clear()
        {
            m_Time = Constant.DefaultTime;
            m_MuteInSoundGroup = Constant.DefaultMute;
            m_Loop = Constant.DefaultLoop;
            m_Priority = Constant.DefaultPriority;
            m_VolumeInSoundGroup = Constant.DefaultVolume;
            m_FadeInSeconds = Constant.DefaultFadeInSeconds;
            m_Pitch = Constant.DefaultPitch;
            m_PanStereo = Constant.DefaultPanStereo;
            m_SpatialBlend = Constant.DefaultSpatialBlend;
            m_MaxDistance = Constant.DefaultMaxDistance;
            m_DopplerLevel = Constant.DefaultDopplerLevel;
        }
    }
}
