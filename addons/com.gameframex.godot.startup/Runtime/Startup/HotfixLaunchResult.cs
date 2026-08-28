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
//  Any legal disputes or liabilities arising from secondary development based on this project
//  本项目组织与贡献者概不承担。
//  shall be borne solely by the developer; the project organization and contributors assume no responsibility.
//
//  GitHub 仓库：https://github.com/GameFrameX
//  GitHub Repository: https://github.com/GameFrameX
//  Gitee  仓库：https://gitee.com/GameFrameX
//  Gitee Repository:  https://gitee.com/GameFrameX
//  CNB  仓库：https://cnb.cool/GameFrameX
//  CNB Repository: https://cnb.cool/GameFrameX
//  官方文档：https://gameframex.doc.alianblank.com/
//  Official Documentation: https://gameframex.doc.alianblank.com/
// ==========================================================================================

namespace GameFrameX.Startup.Runtime
{
    /// <summary>
    /// 热更加载结果。IHotfixLauncher.StartAsync 的返回类型。
    /// </summary>
    public sealed class HotfixLaunchResult
    {
        /// <summary>
        /// 热更加载是否成功。
        /// </summary>
        public bool Success;

        /// <summary>
        /// 失败时填，错误描述；成功时为空字符串。
        /// </summary>
        public string ErrorMessage;

        /// <summary>
        /// 构造成功结果。
        /// </summary>
        public static HotfixLaunchResult Succeed()
        {
            return new HotfixLaunchResult
            {
                Success = true,
                ErrorMessage = string.Empty,
            };
        }

        /// <summary>
        /// 构造失败结果。
        /// </summary>
        /// <param name="errorMessage">错误描述。</param>
        public static HotfixLaunchResult Fail(string errorMessage)
        {
            return new HotfixLaunchResult
            {
                Success = false,
                ErrorMessage = errorMessage ?? string.Empty,
            };
        }
    }
}
