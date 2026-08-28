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
    public sealed partial class SoundManager : GameFrameworkModule, ISoundManager
    {
        /// <summary>
        /// 播放声音信息。
        /// </summary>
        private sealed class PlaySoundInfo : IReference
        {
            /// <summary>
            /// 序列编号。
            /// </summary>
            private int m_SerialId;

            /// <summary>
            /// 声音组。
            /// </summary>
            private SoundGroup m_SoundGroup;

            /// <summary>
            /// 播放声音参数。
            /// </summary>
            private PlaySoundParams m_PlaySoundParams;

            /// <summary>
            /// 用户自定义数据。
            /// </summary>
            private object m_UserData;

            /// <summary>
            /// 初始化播放声音信息的新实例。
            /// </summary>
            public PlaySoundInfo()
            {
                m_SerialId = 0;
                m_SoundGroup = null;
                m_PlaySoundParams = null;
                m_UserData = null;
            }

            /// <summary>
            /// 获取序列编号。
            /// </summary>
            public int SerialId
            {
                get { return m_SerialId; }
            }

            /// <summary>
            /// 获取声音组。
            /// </summary>
            public SoundGroup SoundGroup
            {
                get { return m_SoundGroup; }
            }

            /// <summary>
            /// 获取播放声音参数。
            /// </summary>
            public PlaySoundParams PlaySoundParams
            {
                get { return m_PlaySoundParams; }
            }

            /// <summary>
            /// 获取用户自定义数据。
            /// </summary>
            public object UserData
            {
                get { return m_UserData; }
            }

            /// <summary>
            /// 创建播放声音信息。
            /// </summary>
            /// <param name="serialId">序列编号。</param>
            /// <param name="soundGroup">声音组。</param>
            /// <param name="playSoundParams">播放声音参数。</param>
            /// <param name="userData">用户自定义数据。</param>
            /// <returns>创建的播放声音信息。</returns>
            public static PlaySoundInfo Create(int serialId, SoundGroup soundGroup, PlaySoundParams playSoundParams, object userData)
            {
                PlaySoundInfo playSoundInfo = ReferencePool.Acquire<PlaySoundInfo>();
                playSoundInfo.m_SerialId = serialId;
                playSoundInfo.m_SoundGroup = soundGroup;
                playSoundInfo.m_PlaySoundParams = playSoundParams;
                playSoundInfo.m_UserData = userData;
                return playSoundInfo;
            }

            /// <summary>
            /// 清理播放声音信息。
            /// </summary>
            public void Clear()
            {
                m_SerialId = 0;
                m_SoundGroup = null;
                m_PlaySoundParams = null;
                if (m_UserData is SoundPlayContext soundPlayContext)
                {
                    ReferencePool.Release(soundPlayContext);
                }
                m_UserData = null;
            }
        }
    }
}
