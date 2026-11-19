// ==========================================================================================
//  GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//  GameFrameX organization and its derivative projects' copyrights, trademarks, patents and other related rights
//  均受中华人民共和国及相关国际法律法规保护。
//  are protected by the laws of the People's Republic of China and international regulations.
//
//  使用本项目须严格遵守相应法律法规与开源许可证之规定。
//  Usage of this project must strictly comply with applicable laws, regulations, and open-source licenses.
//
//  本项目采用 MIT 许可证与 Apache License 2.0 双许可证分发，
//  This project is dual-licensed under the MIT License and Apache License 2.0,
//  完整许可证文本请参见源代码根目录下的 LICENSE 文件。
//  please refer to the LICENSE file in the source code root directory for the full license text.
//
//  禁止利用本项目实施任何危害国家安全、破坏社会秩序、
//  It is prohibited to use this project to engage in any activities that endanger national security, disrupt social order,
//  侵犯他人合法权益等法律法规所禁止的行为！
//  or infringe upon the legal rights and interests of others, as prohibited by laws and regulations!
//  因基于本项目二次开发所产生的一切法律纠纷与责任，
//  Any legal disputes or liabilities arising from secondary development based on this project
//  本组织与贡献者概不承担。
//  shall be borne solely by the developer; the project organization and contributors assume no responsibility.
//
//  GitHub 仓库：https://github.com/GameFrameX
//  GitHub Repository: https://github.com/GameFrameX
//  Gitee  仓库：https://gitee.com/GameFrameX
//  Gitee Repository: https://gitee.com/GameFrameX
//  CNB 仓库：https://cnb.cool/GameFrameX
//  CNB Repository: https://cnb.cool/GameFrameX
//  官方文档：https://gameframex.doc.alianblank.com/
//  Official Documentation: https://gameframex.doc.alianblank.com/
// ==========================================================================================

using System;
using System.Collections.Generic;
using Godot;

namespace GameFrameX.Runtime
{
    /// <summary>
    /// Options 贡献者上下文：贡献者经本上下文 Configure，手动 Configure 过的类型按类型粒度跳过并诊断。
    /// </summary>
    public sealed class GameFrameXOptionsContributorContext
    {
        private readonly List<Type> m_SkippedTypes = new List<Type>();

        /// <summary>
        /// 获取被跳过（已被手动 Configure）的 Options 类型列表。
        /// </summary>
        public IReadOnlyList<Type> SkippedTypes
        {
            get { return m_SkippedTypes; }
        }

        /// <summary>
        /// 注册某模块的 Options 默认值；该类型已被手动 Configure 时跳过并记录诊断。
        /// </summary>
        /// <typeparam name="TOptions">Options 类型。</typeparam>
        /// <param name="options">Options 实例。</param>
        public void Configure<TOptions>(TOptions options) where TOptions : class
        {
            if (options == null)
            {
                GD.PushWarning("[GameFrameX] Options contributor 提供了 null 的 Options 实例，已忽略。");
                return;
            }

            var optionsType = typeof(TOptions);
            if (GameFrameXOptions.HasConfigured(optionsType))
            {
                // 手动优先：游戏侧显式 Configure 永远胜出，贡献者按类型粒度跳过。
                m_SkippedTypes.Add(optionsType);
                return;
            }

            GameFrameXOptions.Configure(options);
        }
    }
}
