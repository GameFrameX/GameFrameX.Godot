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

using GameFrameX.Event.Runtime;
using GameFrameX.Runtime;

namespace GameFrameX.Entity.Runtime
{
    /// <summary>
    /// 隐藏实体完成事件。
    /// </summary>
    public sealed class HideEntityCompleteEventArgs : GameEventArgs
    {
        /// <summary>
        /// 隐藏实体完成事件编号。
        /// </summary>
        public static readonly string EventId = typeof(HideEntityCompleteEventArgs).FullName;

        /// <summary>
        /// 初始化隐藏实体完成事件的新实例。
        /// </summary>
        public HideEntityCompleteEventArgs()
        {
            EntityId = 0;
            EntityAssetName = null;
            EntityGroup = null;
            UserData = null;
        }

        /// <summary>
        /// 获取隐藏实体完成事件编号。
        /// </summary>
        public override string Id
        {
            get { return EventId; }
        }

        /// <summary>
        /// 获取实体编号。
        /// </summary>
        public int EntityId { get; private set; }

        /// <summary>
        /// 获取实体资源名称。
        /// </summary>
        public string EntityAssetName { get; private set; }

        /// <summary>
        /// 获取实体所属的实体组。
        /// </summary>
        public IEntityGroup EntityGroup { get; private set; }

        /// <summary>
        /// 获取用户自定义数据。
        /// </summary>
        public object UserData { get; private set; }

        /// <summary>
        /// 创建隐藏实体完成事件。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <param name="entityGroup">实体所属的实体组。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>创建的隐藏实体完成事件。</returns>
        public static HideEntityCompleteEventArgs Create(int entityId, string entityAssetName, IEntityGroup entityGroup, object userData)
        {
            HideEntityCompleteEventArgs hideEntityCompleteEventArgs = ReferencePool.Acquire<HideEntityCompleteEventArgs>();
            hideEntityCompleteEventArgs.EntityId = entityId;
            hideEntityCompleteEventArgs.EntityAssetName = entityAssetName;
            hideEntityCompleteEventArgs.EntityGroup = entityGroup;
            hideEntityCompleteEventArgs.UserData = userData;
            return hideEntityCompleteEventArgs;
        }

        /// <summary>
        /// 清理隐藏实体完成事件。
        /// </summary>
        public override void Clear()
        {
            EntityId = 0;
            EntityAssetName = null;
            EntityGroup = null;
            UserData = null;
        }
    }
}