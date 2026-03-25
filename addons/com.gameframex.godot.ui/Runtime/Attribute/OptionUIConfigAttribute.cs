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

namespace GameFrameX.UI.Runtime
{
    /// <summary>
    /// 用于标记 UI 配置的特性类，支持 FairyGUI 和 UGUI 两种 UI 框架。
    /// 通过指定包名或路径，实现 UI 资源的自动定位和加载。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class OptionUIConfigAttribute : Attribute
    {
        /// <summary>
        /// FairyGUI 使用的包名。用于定位 FairyGUI 的 UI 资源包。
        /// </summary>
        public string PackageName { get; private set; }

        /// <summary>
        /// UGUI 使用的资源路径。用于定位 UGUI 的 UI 预制体或资源。
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// 是否为资源路径。若为 true，则 Path 为资源路径；若为 false，则 Path 为 UI 预制体路径。
        /// </summary>
        public bool IsResource { get; private set; }

        /// <summary>
        /// 构造 UI 配置特性。
        /// </summary>
        /// <param name="packageName">FairyGUI 使用的包名，若为 null 则不使用 FairyGUI。</param>
        /// <param name="path">UGUI 使用的资源路径，若为 null 则不使用 UGUI。</param>
        /// <exception cref="Exception">当 packageName 和 path 均为 null 或空字符串时抛出异常。</exception>
        public OptionUIConfigAttribute(string packageName = null, string path = null)
        {
            PackageName = packageName;
            Path = path;
            if (string.IsNullOrEmpty(PackageName) && string.IsNullOrEmpty(Path))
            {
                throw new Exception("PackageName or Path is null");
            }
        }

        /// <summary>
        /// 构造 UI 配置特性。
        /// </summary>
        /// <param name="isResource">是否为资源路径。若为 true，则 Path 为资源路径；若为 false，则 Path 为 UI 预制体路径。</param>
        public OptionUIConfigAttribute(bool isResource = false)
        {
            IsResource = isResource;
        }
    }
}
