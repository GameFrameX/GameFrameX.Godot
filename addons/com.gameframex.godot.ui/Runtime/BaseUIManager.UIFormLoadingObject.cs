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
using System.Threading.Tasks;
using GameFrameX.ObjectPool;
using GameFrameX.Runtime;

namespace GameFrameX.UI.Runtime
{
    public partial class BaseUIManager
    {
        /// <summary>
        /// 界面加载对象。
        /// </summary>
        public sealed class UIFormLoadingObject : IReference
        {
            /// <summary>
            /// 界面资源路径。
            /// </summary>
            public string UIFormAssetPath { get; private set; }

            /// <summary>
            /// 界面资源名称。
            /// </summary>
            public string UIFormAssetName { get; private set; }

            /// <summary>
            /// 界面类型。
            /// </summary>
            public Type UIFormType { get; private set; }

            /// <summary>
            /// 界面加载任务。
            /// </summary>
            public Task<IUIForm> Task { get; private set; }

            /// <summary>
            /// 创建界面实例对象。
            /// </summary>
            /// <param name="uiFormAssetPath">界面资源路径。</param>
            /// <param name="uiFormAssetName">界面资源名称。</param>
            /// <param name="uiFormType">界面类型。</param>
            /// <param name="task">界面加载任务。</param>
            /// <returns>界面实例对象。</returns>
            public static UIFormLoadingObject Create(string uiFormAssetPath, string uiFormAssetName, Type uiFormType, Task<IUIForm> task)
            {
                var uiFormLoadingObject = ReferencePool.Acquire<UIFormLoadingObject>();
                uiFormLoadingObject.UIFormAssetPath = uiFormAssetPath;
                uiFormLoadingObject.UIFormAssetName = uiFormAssetName;
                uiFormLoadingObject.UIFormType = uiFormType;
                uiFormLoadingObject.Task = task;
                return uiFormLoadingObject;
            }

            public void Clear()
            {
                UIFormAssetPath = null;
                UIFormAssetName = null;
                UIFormType = null;
                Task = null;
            }
        }
    }
}
