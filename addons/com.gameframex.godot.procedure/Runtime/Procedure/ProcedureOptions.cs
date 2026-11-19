// ==========================================================================================
//  GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//  GameFrameX organization and its derivative projects' copyrights, trademarks, patents and other related rights
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

namespace GameFrameX.Procedure.Runtime
{
    /// <summary>
    /// 流程模块启动配置（C9 配置终态）。
    /// </summary>
    /// <remarks>
    /// 默认值即开箱值（与原组件 <c>m_EntranceProcedureTypeName</c> / <c>m_UseStartupRunner</c> 序列化默认值同源）；
    /// 游戏在主场景加载前（自动启动引导）经
    /// <c>GameFrameXOptions.Configure(new ProcedureOptions{...})</c> 注入差异值，<see cref="ProcedureComponent"/> 在 _Ready 中一次性消费。
    /// </remarks>
    public sealed class ProcedureOptions
    {
        /// <summary>
        /// 入口流程类型名，默认 null（配合 Inspector 配置的可用流程列表使用）。
        /// </summary>
        /// <remarks>启动期型：流程启动在组件延迟初始化阶段一次性确定。</remarks>
        [GameFrameX.Runtime.GameFrameXStartupOnly]
        public string EntranceProcedureTypeName = null;

        /// <summary>
        /// 是否使用 StartupRunner 接管启动流程，默认 false（走 Inspector 配置的可用流程自动启动）。
        /// </summary>
        /// <remarks>启动期型：为 true 时组件仅在 _Ready 注册管理器，由应用层 StartupRunner 接管初始化与启动。</remarks>
        [GameFrameX.Runtime.GameFrameXStartupOnly]
        public bool UseStartupRunner = false;
    }
}
