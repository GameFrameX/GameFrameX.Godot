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

namespace GameFrameX.Runtime
{
    /// <summary>
    /// 声明组件参与自动 Host 挂载，并指定挂载顺序与依赖。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class GameFrameXAutoComponentAttribute : Attribute
    {
        /// <summary>
        /// 初始化自动组件特性。
        /// </summary>
        /// <param name="order">挂载顺序，数值小的先挂载。</param>
        /// <param name="dependencies">依赖的组件类型。</param>
        public GameFrameXAutoComponentAttribute(int order = 0, params Type[] dependencies)
        {
            Order = order;
            Dependencies = dependencies ?? Array.Empty<Type>();
            EnabledByDefault = true;
        }

        /// <summary>
        /// 获取挂载顺序。
        /// </summary>
        public int Order { get; private set; }

        /// <summary>
        /// 获取依赖的组件类型列表。
        /// </summary>
        public Type[] Dependencies { get; private set; }

        /// <summary>
        /// 获取或设置是否默认启用。
        /// </summary>
        public bool EnabledByDefault { get; set; }
    }
}
