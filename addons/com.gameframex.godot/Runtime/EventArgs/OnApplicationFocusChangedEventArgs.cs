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

using GameFrameX.Event.Runtime;

namespace GameFrameX.Runtime
{
    /// <summary>
    /// 应用程序获得或失去焦点事件。
    /// 迁移自 Unity com.gameframex.unity.mono；Godot 侧由 BaseComponent 在
    /// NotificationApplicationFocusIn / NotificationApplicationFocusOut 通知时抛出。
    /// </summary>
    public sealed class OnApplicationFocusChangedEventArgs : GameEventArgs
    {
        /// <summary>
        /// 事件编号。
        /// </summary>
        public static readonly string EventId = typeof(OnApplicationFocusChangedEventArgs).FullName;

        /// <summary>
        /// 初始化应用程序获得或失去焦点事件的新实例。
        /// </summary>
        public OnApplicationFocusChangedEventArgs()
        {
            IsFocus = false;
        }

        /// <summary>
        /// 获取事件编号。
        /// </summary>
        public override string Id
        {
            get { return EventId; }
        }

        /// <summary>
        /// 获取应用程序是否获得焦点。
        /// </summary>
        public bool IsFocus { get; private set; }

        /// <summary>
        /// 创建应用程序获得或失去焦点事件。
        /// </summary>
        /// <param name="isFocus">应用程序是否获得焦点。</param>
        /// <returns>创建的应用程序获得或失去焦点事件。</returns>
        public static OnApplicationFocusChangedEventArgs Create(bool isFocus)
        {
            OnApplicationFocusChangedEventArgs eventArgs = ReferencePool.Acquire<OnApplicationFocusChangedEventArgs>();
            eventArgs.IsFocus = isFocus;
            return eventArgs;
        }

        /// <summary>
        /// 清理应用程序获得或失去焦点事件。
        /// </summary>
        public override void Clear()
        {
            IsFocus = false;
        }
    }
}
