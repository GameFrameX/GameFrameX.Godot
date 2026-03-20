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
    /// 显示实体时加载依赖资源事件。
    /// </summary>
    public sealed class ShowEntityDependencyAssetEventArgs : GameEventArgs
    {
        /// <summary>
        /// 显示实体时加载依赖资源事件编号。
        /// </summary>
        public static readonly string EventId = typeof(ShowEntityDependencyAssetEventArgs).FullName;

        /// <summary>
        /// 初始化显示实体时加载依赖资源事件的新实例。
        /// </summary>
        public ShowEntityDependencyAssetEventArgs()
        {
            EntityId = 0;
            EntityLogicType = null;
            EntityAssetName = null;
            EntityGroupName = null;
            DependencyAssetName = null;
            LoadedCount = 0;
            TotalCount = 0;
            UserData = null;
        }

        /// <summary>
        /// 获取显示实体时加载依赖资源事件编号。
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
        /// 获取实体逻辑类型。
        /// </summary>
        public Type EntityLogicType { get; private set; }

        /// <summary>
        /// 获取实体资源名称。
        /// </summary>
        public string EntityAssetName { get; private set; }

        /// <summary>
        /// 获取实体组名称。
        /// </summary>
        public string EntityGroupName { get; private set; }

        /// <summary>
        /// 获取被加载的依赖资源名称。
        /// </summary>
        public string DependencyAssetName { get; private set; }

        /// <summary>
        /// 获取当前已加载依赖资源数量。
        /// </summary>
        public int LoadedCount { get; private set; }

        /// <summary>
        /// 获取总共加载依赖资源数量。
        /// </summary>
        public int TotalCount { get; private set; }

        /// <summary>
        /// 获取用户自定义数据。
        /// </summary>
        public object UserData { get; private set; }

        /// <summary>
        /// 创建显示实体时加载依赖资源事件。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <param name="entityGroupName">实体组名称。</param>
        /// <param name="dependencyAssetName">被加载的依赖资源名称。</param>
        /// <param name="loadedCount">当前已加载依赖资源数量。</param>
        /// <param name="totalCount">总共加载依赖资源数量。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>创建的显示实体时加载依赖资源事件。</returns>
        public static ShowEntityDependencyAssetEventArgs Create(int entityId, string entityAssetName, string entityGroupName, string dependencyAssetName, int loadedCount, int totalCount, object userData)
        {
            ShowEntityDependencyAssetEventArgs showEntityDependencyAssetEventArgs = ReferencePool.Acquire<ShowEntityDependencyAssetEventArgs>();
            showEntityDependencyAssetEventArgs.EntityId = entityId;
            showEntityDependencyAssetEventArgs.EntityAssetName = entityAssetName;
            showEntityDependencyAssetEventArgs.EntityGroupName = entityGroupName;
            showEntityDependencyAssetEventArgs.DependencyAssetName = dependencyAssetName;
            showEntityDependencyAssetEventArgs.LoadedCount = loadedCount;
            showEntityDependencyAssetEventArgs.TotalCount = totalCount;
            showEntityDependencyAssetEventArgs.UserData = userData;
            return showEntityDependencyAssetEventArgs;
        }

        /// <summary>
        /// 清理显示实体时加载依赖资源事件。
        /// </summary>
        public override void Clear()
        {
            EntityId = 0;
            EntityLogicType = null;
            EntityAssetName = null;
            EntityGroupName = null;
            DependencyAssetName = null;
            LoadedCount = 0;
            TotalCount = 0;
            UserData = null;
        }
    }
}