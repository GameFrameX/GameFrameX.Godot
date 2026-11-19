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
    /// Options 贡献者宿主：扫描发现 <see cref="IGameFrameXOptionsContributor"/> 实现并在最早实例化点之前一次性应用。
    /// </summary>
    /// <remarks>
    /// 单入口幂等：自动启动引导在主场景进入前调用 <see cref="EnsureApplied"/>，
    /// 早于场景组件与自动 Host 组件的初始化，因此必然早于任何模块实例化。
    /// 响亮失败：贡献者实例化失败（缺公共无参构造）或 <see cref="IGameFrameXOptionsContributor.ConfigureOptions"/> 抛异常时，
    /// 输出 <c>[GameFrameX]</c> 前缀警告后跳过该贡献者（不中断启动、不静默吞掉）——诊断走 <c>GD</c>，
    /// 因为应用可能早于 Log helper 接线。
    /// </remarks>
    public static class GameFrameXRuntimeContributorHost
    {
        private static bool s_Applied;

        /// <summary>
        /// 确保贡献者已应用（幂等，每个进程一次；重置后可再次应用）。
        /// </summary>
        public static void EnsureApplied()
        {
            if (s_Applied)
            {
                return;
            }

            s_Applied = true;
            Apply(GameFrameXRuntimeScanner.Scan());
        }

        /// <summary>
        /// 重置应用状态（场景重载 / 框架重启后允许重新应用）。
        /// </summary>
        public static void Reset()
        {
            s_Applied = false;
        }

        private static void Apply(GameFrameXRuntimeScanResult scanResult)
        {
            if (scanResult == null)
            {
                return;
            }

            foreach (var contributorType in scanResult.OptionsContributorTypes)
            {
                object instance;
                try
                {
                    instance = Activator.CreateInstance(contributorType);
                }
                catch (Exception exception)
                {
                    GD.PushWarning(Utility.Text.Format(
                        "[GameFrameX] Options contributor '{0}' can not be instantiated (public parameterless constructor required): {1}",
                        contributorType.FullName,
                        exception.Message));
                    continue;
                }

                if (!(instance is IGameFrameXOptionsContributor contributor))
                {
                    continue;
                }

                var context = new GameFrameXOptionsContributorContext();
                try
                {
                    contributor.ConfigureOptions(context);
                }
                catch (Exception exception)
                {
                    GD.PushWarning(Utility.Text.Format(
                        "[GameFrameX] Options contributor '{0}' threw during ConfigureOptions and was skipped: {1}",
                        contributorType.FullName,
                        exception.Message));
                    continue;
                }

                foreach (var skippedType in context.SkippedTypes)
                {
                    GD.Print(Utility.Text.Format(
                        "[GameFrameX] Options contributor '{0}' skipped Options '{1}' (already configured manually).",
                        contributorType.FullName,
                        skippedType.FullName));
                }
            }

            foreach (var error in scanResult.Errors)
            {
                GD.PushWarning("[GameFrameX] Options contributor scan: " + error);
            }
        }
    }
}
