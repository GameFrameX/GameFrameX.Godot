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

using System.Collections.Generic;

namespace GameFrameX.Entity.Runtime
{
    /// <summary>
    /// 实体组接口。
    /// </summary>
    public interface IEntityGroup
    {
        /// <summary>
        /// 获取实体组名称。
        /// </summary>
        string Name
        {
            get;
        }

        /// <summary>
        /// 获取实体组中实体数量。
        /// </summary>
        int EntityCount
        {
            get;
        }

        /// <summary>
        /// 获取或设置实体组实例对象池自动释放可释放对象的间隔秒数。
        /// </summary>
        float InstanceAutoReleaseInterval
        {
            get;
            set;
        }

        /// <summary>
        /// 获取或设置实体组实例对象池的容量。
        /// </summary>
        int InstanceCapacity
        {
            get;
            set;
        }

        /// <summary>
        /// 获取或设置实体组实例对象池对象过期秒数。
        /// </summary>
        float InstanceExpireTime
        {
            get;
            set;
        }

        /// <summary>
        /// 获取或设置实体组实例对象池的优先级。
        /// </summary>
        int InstancePriority
        {
            get;
            set;
        }

        /// <summary>
        /// 获取实体组辅助器。
        /// </summary>
        IEntityGroupHelper Helper
        {
            get;
        }

        /// <summary>
        /// 实体组中是否存在实体。
        /// </summary>
        /// <param name="entityId">实体序列编号。</param>
        /// <returns>实体组中是否存在实体。</returns>
        bool HasEntity(int entityId);

        /// <summary>
        /// 实体组中是否存在实体。
        /// </summary>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <returns>实体组中是否存在实体。</returns>
        bool HasEntity(string entityAssetName);

        /// <summary>
        /// 从实体组中获取实体。
        /// </summary>
        /// <param name="entityId">实体序列编号。</param>
        /// <returns>要获取的实体。</returns>
        IEntity GetEntity(int entityId);

        /// <summary>
        /// 从实体组中获取实体。
        /// </summary>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <returns>要获取的实体。</returns>
        IEntity GetEntity(string entityAssetName);

        /// <summary>
        /// 从实体组中获取实体。
        /// </summary>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <returns>要获取的实体。</returns>
        IEntity[] GetEntities(string entityAssetName);

        /// <summary>
        /// 从实体组中获取实体。
        /// </summary>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <param name="results">要获取的实体。</param>
        void GetEntities(string entityAssetName, List<IEntity> results);

        /// <summary>
        /// 从实体组中获取所有实体。
        /// </summary>
        /// <returns>实体组中的所有实体。</returns>
        IEntity[] GetAllEntities();

        /// <summary>
        /// 从实体组中获取所有实体。
        /// </summary>
        /// <param name="results">实体组中的所有实体。</param>
        void GetAllEntities(List<IEntity> results);

        /// <summary>
        /// 设置实体实例是否被加锁。
        /// </summary>
        /// <param name="entityInstance">实体实例。</param>
        /// <param name="locked">实体实例是否被加锁。</param>
        void SetEntityInstanceLocked(object entityInstance, bool locked);

        /// <summary>
        /// 设置实体实例的优先级。
        /// </summary>
        /// <param name="entityInstance">实体实例。</param>
        /// <param name="priority">实体实例优先级。</param>
        void SetEntityInstancePriority(object entityInstance, int priority);
    }
}
