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
using GameFrameX.Runtime;

namespace GameFrameX.Sound.Runtime
{
    /// <summary>
    /// 声音模块启动配置（模块级 Options）。
    /// </summary>
    /// <remarks>
    /// 默认值即开箱值（与原 SoundComponent 辅助器类型默认值同源）；游戏在组件挂载前经 <c>GameFrameXOptions.Configure(new SoundOptions{...})</c> 注入差异值，
    /// 由 <see cref="SoundComponent"/> 在 _Ready 中一次性消费。
    /// Godot 迁移说明：Unity 版辅助器工厂经 GameObject.AddComponent 挂载，Godot 版辅助器为普通 Node，直接 new 后由组件 AddChild。
    /// </remarks>
    public sealed class SoundOptions
    {
        /// <summary>
        /// 声音辅助器工厂（默认 DefaultSoundHelper）。
        /// </summary>
        /// <remarks>启动期型：仅在组件创建辅助器时消费，运行期不可修改。</remarks>
        [GameFrameXStartupOnly]
        public Func<SoundHelperBase> SoundHelperFactory = () => new DefaultSoundHelper();

        /// <summary>
        /// 声音组辅助器工厂（默认 DefaultSoundGroupHelper）。
        /// </summary>
        /// <remarks>启动期型：仅在组件创建声音组时消费，运行期不可修改。</remarks>
        [GameFrameXStartupOnly]
        public Func<SoundGroupHelperBase> SoundGroupHelperFactory = () => new DefaultSoundGroupHelper();

        /// <summary>
        /// 声音代理辅助器工厂（默认 DefaultSoundAgentHelper）。
        /// </summary>
        /// <remarks>启动期型：仅在组件创建声音代理时消费，运行期不可修改。</remarks>
        [GameFrameXStartupOnly]
        public Func<SoundAgentHelperBase> SoundAgentHelperFactory = () => new DefaultSoundAgentHelper();
    }
}
