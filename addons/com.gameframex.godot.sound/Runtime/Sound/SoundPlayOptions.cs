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
using Godot;

namespace GameFrameX.Sound.Runtime
{
    /// <summary>
    /// 播放声音选项。作为 <see cref="SoundComponent.PlaySound"/> 的参数对象，
    /// 用于替代大量重载方法，简化调用方式。
    /// </summary>
    public sealed class SoundPlayOptions
    {
        /// <summary>
        /// 获取或设置加载声音资源的优先级。默认 <see cref="Constant.DefaultPriority"/>。
        /// </summary>
        public int Priority { get; set; } = Constant.DefaultPriority;

        /// <summary>
        /// 获取或设置播放声音参数。
        /// </summary>
        public PlaySoundParams PlaySoundParams { get; set; } = null;

        /// <summary>
        /// 获取或设置声音绑定的实体。与 <see cref="WorldPosition"/> 互斥，实体绑定优先。
        /// </summary>
        public GameFrameX.Entity.Runtime.Entity BindingEntity { get; set; } = null;

        /// <summary>
        /// 获取或设置声音所在的世界坐标。当 <see cref="BindingEntity"/> 不为 null 时忽略。
        /// </summary>
        public Vector3? WorldPosition { get; set; } = null;

        /// <summary>
        /// 获取或设置用户自定义数据。
        /// </summary>
        public object UserData { get; set; } = null;

        /// <summary>
        /// 获取或设置声音的序列编号。为 null 时自动生成。
        /// </summary>
        public int? SerialId { get; set; } = null;
    }
}
