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
using GameFrameX.Runtime;

namespace GameFrameX.Entity.Runtime
{
    public sealed partial class EntityManager : GameFrameworkModule, IEntityManager
    {
        /// <summary>
        /// 实体信息。
        /// </summary>
        private sealed class EntityInfo : IReference
        {
            private IEntity m_Entity;
            private EntityStatus m_Status;
            private IEntity m_ParentEntity;
            private List<IEntity> m_ChildEntities;

            public EntityInfo()
            {
                m_Entity = null;
                m_Status = EntityStatus.Unknown;
                m_ParentEntity = null;
                m_ChildEntities = new List<IEntity>();
            }

            public IEntity Entity
            {
                get
                {
                    return m_Entity;
                }
            }

            public EntityStatus Status
            {
                get
                {
                    return m_Status;
                }
                set
                {
                    m_Status = value;
                }
            }

            public IEntity ParentEntity
            {
                get
                {
                    return m_ParentEntity;
                }
                set
                {
                    m_ParentEntity = value;
                }
            }

            public int ChildEntityCount
            {
                get
                {
                    return m_ChildEntities.Count;
                }
            }

            public static EntityInfo Create(IEntity entity)
            {
                if (entity == null)
                {
                    throw new GameFrameworkException("Entity is invalid.");
                }

                EntityInfo entityInfo = ReferencePool.Acquire<EntityInfo>();
                entityInfo.m_Entity = entity;
                entityInfo.m_Status = EntityStatus.WillInit;
                return entityInfo;
            }

            public void Clear()
            {
                m_Entity = null;
                m_Status = EntityStatus.Unknown;
                m_ParentEntity = null;
                m_ChildEntities.Clear();
            }

            public IEntity GetChildEntity()
            {
                return m_ChildEntities.Count > 0 ? m_ChildEntities[0] : null;
            }

            public IEntity[] GetChildEntities()
            {
                return m_ChildEntities.ToArray();
            }

            public void GetChildEntities(List<IEntity> results)
            {
                if (results == null)
                {
                    throw new GameFrameworkException("Results is invalid.");
                }

                results.Clear();
                foreach (IEntity childEntity in m_ChildEntities)
                {
                    results.Add(childEntity);
                }
            }

            public void AddChildEntity(IEntity childEntity)
            {
                if (m_ChildEntities.Contains(childEntity))
                {
                    throw new GameFrameworkException("Can not add child entity which is already exist.");
                }

                m_ChildEntities.Add(childEntity);
            }

            public void RemoveChildEntity(IEntity childEntity)
            {
                if (!m_ChildEntities.Remove(childEntity))
                {
                    throw new GameFrameworkException("Can not remove child entity which is not exist.");
                }
            }
        }
    }
}
