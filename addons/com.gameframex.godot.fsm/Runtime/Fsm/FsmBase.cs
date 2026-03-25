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
//  CNB  仓库：https://cnb.cool/GameFrameX
//  CNB Repository:  https://cnb.cool/GameFrameX
//  官方文档：https://gameframex.doc.alianblank.com/
//  Official Documentation: https://gameframex.doc.alianblank.com/
// ==========================================================================================

using System;
using GameFrameX.Runtime;

namespace GameFrameX.Fsm.Runtime
{
    /// <summary>
    /// 有限状态机基类。
    /// </summary>
    public abstract class FsmBase
    {
        private string m_Name;

        /// <summary>
        /// 初始化有限状态机基类的新实例。
        /// </summary>
        public FsmBase()
        {
            m_Name = string.Empty;
        }

        /// <summary>
        /// 获取有限状态机名称。
        /// </summary>
        public string Name
        {
            get { return m_Name; }
            protected set { m_Name = value ?? string.Empty; }
        }

        /// <summary>
        /// 获取有限状态机完整名称。
        /// </summary>
        public string FullName
        {
            get { return new TypeNamePair(OwnerType, m_Name).ToString(); }
        }

        /// <summary>
        /// 获取有限状态机持有者类型。
        /// </summary>
        public abstract Type OwnerType { get; }

        /// <summary>
        /// 获取有限状态机中状态的数量。
        /// </summary>
        public abstract int FsmStateCount { get; }

        /// <summary>
        /// 获取有限状态机是否正在运行。
        /// </summary>
        public abstract bool IsRunning { get; }

        /// <summary>
        /// 获取有限状态机是否被销毁。
        /// </summary>
        public abstract bool IsDestroyed { get; }

        /// <summary>
        /// 获取当前有限状态机状态名称。
        /// </summary>
        public abstract string CurrentStateName { get; }

        /// <summary>
        /// 获取当前有限状态机状态持续时间。
        /// </summary>
        public abstract float CurrentStateTime { get; }

        /// <summary>
        /// 有限状态机轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">当前已流逝时间，以秒为单位。</param>
        internal abstract void Update(float elapseSeconds, float realElapseSeconds);

        /// <summary>
        /// 有限状态机固定轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">当前已流逝时间，以秒为单位。</param>
        internal abstract void FixedUpdate(float elapseSeconds, float realElapseSeconds);

        /// <summary>
        /// 关闭并清理有限状态机。
        /// </summary>
        internal abstract void Shutdown();
    }
}