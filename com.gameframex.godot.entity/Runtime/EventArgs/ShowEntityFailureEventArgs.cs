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
using GameFrameX.Event.Runtime;
using GameFrameX.Runtime;

namespace GameFrameX.Entity.Runtime
{
    /// <summary>
    /// 显示实体失败事件。
    /// </summary>
    public sealed class ShowEntityFailureEventArgs : GameEventArgs
    {
        /// <summary>
        /// 显示实体失败事件编号。
        /// </summary>
        public static readonly string EventId = typeof(ShowEntityFailureEventArgs).FullName;

        /// <summary>
        /// 初始化显示实体失败事件的新实例。
        /// </summary>
        public ShowEntityFailureEventArgs()
        {
            EntityId = 0;
            EntityLogicType = null;
            EntityAssetName = null;
            EntityGroupName = null;
            ErrorMessage = null;
            UserData = null;
        }

        /// <summary>
        /// 获取显示实体失败事件编号。
        /// </summary>
        public override string Id
        {
            get
            {
                return EventId;
            }
        }

        /// <summary>
        /// 获取实体编号。
        /// </summary>
        public int EntityId
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取实体逻辑类型。
        /// </summary>
        public Type EntityLogicType
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取实体资源名称。
        /// </summary>
        public string EntityAssetName
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取实体组名称。
        /// </summary>
        public string EntityGroupName
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取错误信息。
        /// </summary>
        public string ErrorMessage
        {
            get;
            private set;
        }

        /// <summary>
        /// 获取用户自定义数据。
        /// </summary>
        public object UserData
        {
            get;
            private set;
        }

        /// <summary>
        /// 创建显示实体失败事件。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <param name="entityGroupName">实体组名称。</param>
        /// <param name="errorMessage">错误信息。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>创建的显示实体失败事件。</returns>
        public static ShowEntityFailureEventArgs Create(int entityId, string entityAssetName, string entityGroupName, string errorMessage, object userData)
        {
            ShowEntityFailureEventArgs showEntityFailureEventArgs = ReferencePool.Acquire<ShowEntityFailureEventArgs>();
            showEntityFailureEventArgs.EntityId = entityId;
            showEntityFailureEventArgs.EntityAssetName = entityAssetName;
            showEntityFailureEventArgs.EntityGroupName = entityGroupName;
            showEntityFailureEventArgs.ErrorMessage = errorMessage;
            showEntityFailureEventArgs.UserData = userData;
            return showEntityFailureEventArgs;
        }

        /// <summary>
        /// 清理显示实体失败事件。
        /// </summary>
        public override void Clear()
        {
            EntityId = 0;
            EntityLogicType = null;
            EntityAssetName = null;
            EntityGroupName = null;
            ErrorMessage = null;
            UserData = null;
        }
    }
}
