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


using GameFrameX.Entity.Runtime;
using GameFrameX.Runtime;
using Godot;

namespace GameFrameX.Sound.Runtime
{
    /// <summary>
    /// 播放声音信息。
    /// </summary>
    public sealed class SoundPlayContext : IReference
    {
        /// <summary>
        /// 绑定的实体。
        /// </summary>
        private GameFrameX.Entity.Runtime.Entity m_BindingEntity;

        /// <summary>
        /// 声音在世界空间中的位置。
        /// </summary>
        private Vector3 m_WorldPosition;

        /// <summary>
        /// 用户自定义数据。
        /// </summary>
        private object m_UserData;

        /// <summary>
        /// 初始化播放声音信息的新实例。
        /// </summary>
        public SoundPlayContext()
        {
            m_BindingEntity = null;
            m_WorldPosition = Vector3.Zero;
            m_UserData = null;
        }

        /// <summary>
        /// 获取绑定的实体。
        /// </summary>
        public GameFrameX.Entity.Runtime.Entity BindingEntity
        {
            get { return m_BindingEntity; }
        }

        /// <summary>
        /// 获取声音在世界空间中的位置。
        /// </summary>
        public Vector3 WorldPosition
        {
            get { return m_WorldPosition; }
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
        /// <param name="bindingEntity">绑定的实体。</param>
        /// <param name="worldPosition">声音在世界空间中的位置。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>创建的播放声音信息。</returns>
        public static SoundPlayContext Create(GameFrameX.Entity.Runtime.Entity bindingEntity, Vector3 worldPosition, object userData)
        {
            SoundPlayContext playSoundInfo = ReferencePool.Acquire<SoundPlayContext>();
            playSoundInfo.m_BindingEntity = bindingEntity;
            playSoundInfo.m_WorldPosition = worldPosition;
            playSoundInfo.m_UserData = userData;
            return playSoundInfo;
        }

        /// <summary>
        /// 清理播放声音信息。
        /// </summary>
        public void Clear()
        {
            m_BindingEntity = null;
            m_WorldPosition = Vector3.Zero;
            m_UserData = null;
        }
    }
}
