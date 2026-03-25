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
    /// <summary>
    /// 实体管理器。
    /// </summary>
    public sealed partial class EntityManager
    {
        /// <summary>
        /// 隐藏实体。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        public void HideEntity(int entityId)
        {
            HideEntity(entityId, null);
        }

        /// <summary>
        /// 隐藏实体。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void HideEntity(int entityId, object userData)
        {
            if (IsLoadingEntity(entityId))
            {
                m_EntitiesToReleaseOnLoad.Add(m_EntitiesBeingLoaded[entityId]);
                m_EntitiesBeingLoaded.Remove(entityId);
                return;
            }

            EntityInfo entityInfo = GetEntityInfo(entityId);
            if (entityInfo == null)
            {
                Log.Error(Utility.Text.Format("Can not find entity '{0}'.", entityId));
            }

            InternalHideEntity(entityInfo, userData);
        }

        /// <summary>
        /// 隐藏实体。
        /// </summary>
        /// <param name="entity">实体。</param>
        public void HideEntity(IEntity entity)
        {
            HideEntity(entity, null);
        }

        /// <summary>
        /// 隐藏实体。
        /// </summary>
        /// <param name="entity">实体。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void HideEntity(IEntity entity, object userData)
        {
            if (entity == null)
            {
                throw new GameFrameworkException("Entity is invalid.");
            }

            HideEntity(entity.Id, userData);
        }

        /// <summary>
        /// 隐藏所有已加载的实体。
        /// </summary>
        public void HideAllLoadedEntities()
        {
            HideAllLoadedEntities(null);
        }

        /// <summary>
        /// 隐藏所有已加载的实体。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        public void HideAllLoadedEntities(object userData)
        {
            while (m_EntityInfos.Count > 0)
            {
                foreach (KeyValuePair<int, EntityInfo> entityInfo in m_EntityInfos)
                {
                    InternalHideEntity(entityInfo.Value, userData);
                    break;
                }
            }
        }

        /// <summary>
        /// 隐藏所有正在加载的实体。
        /// </summary>
        public void HideAllLoadingEntities()
        {
            foreach (KeyValuePair<int, int> entityBeingLoaded in m_EntitiesBeingLoaded)
            {
                m_EntitiesToReleaseOnLoad.Add(entityBeingLoaded.Value);
            }

            m_EntitiesBeingLoaded.Clear();
        }


        /// <summary>
        /// 解除子实体。
        /// </summary>
        /// <param name="childEntityId">要解除的子实体的实体编号。</param>
        public void DetachEntity(int childEntityId)
        {
            DetachEntity(childEntityId, null);
        }

        /// <summary>
        /// 解除子实体。
        /// </summary>
        /// <param name="childEntityId">要解除的子实体的实体编号。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void DetachEntity(int childEntityId, object userData)
        {
            EntityInfo childEntityInfo = GetEntityInfo(childEntityId);
            if (childEntityInfo == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find child entity '{0}'.", childEntityId));
            }

            IEntity parentEntity = childEntityInfo.ParentEntity;
            if (parentEntity == null)
            {
                return;
            }

            EntityInfo parentEntityInfo = GetEntityInfo(parentEntity.Id);
            if (parentEntityInfo == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find parent entity '{0}'.", parentEntity.Id));
            }

            IEntity childEntity = childEntityInfo.Entity;
            childEntityInfo.ParentEntity = null;
            parentEntityInfo.RemoveChildEntity(childEntity);
            parentEntity.OnDetached(childEntity, userData);
            childEntity.OnDetachFrom(parentEntity, userData);
        }

        /// <summary>
        /// 解除子实体。
        /// </summary>
        /// <param name="childEntity">要解除的子实体。</param>
        public void DetachEntity(IEntity childEntity)
        {
            DetachEntity(childEntity, null);
        }

        /// <summary>
        /// 解除子实体。
        /// </summary>
        /// <param name="childEntity">要解除的子实体。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void DetachEntity(IEntity childEntity, object userData)
        {
            if (childEntity == null)
            {
                throw new GameFrameworkException("Child entity is invalid.");
            }

            DetachEntity(childEntity.Id, userData);
        }

        /// <summary>
        /// 解除所有子实体。
        /// </summary>
        /// <param name="parentEntityId">被解除的父实体的实体编号。</param>
        public void DetachChildEntities(int parentEntityId)
        {
            DetachChildEntities(parentEntityId, null);
        }

        /// <summary>
        /// 解除所有子实体。
        /// </summary>
        /// <param name="parentEntityId">被解除的父实体的实体编号。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void DetachChildEntities(int parentEntityId, object userData)
        {
            EntityInfo parentEntityInfo = GetEntityInfo(parentEntityId);
            if (parentEntityInfo == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find parent entity '{0}'.", parentEntityId));
            }

            while (parentEntityInfo.ChildEntityCount > 0)
            {
                IEntity childEntity = parentEntityInfo.GetChildEntity();
                DetachEntity(childEntity.Id, userData);
            }
        }

        /// <summary>
        /// 解除所有子实体。
        /// </summary>
        /// <param name="parentEntity">被解除的父实体。</param>
        public void DetachChildEntities(IEntity parentEntity)
        {
            DetachChildEntities(parentEntity, null);
        }

        /// <summary>
        /// 解除所有子实体。
        /// </summary>
        /// <param name="parentEntity">被解除的父实体。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void DetachChildEntities(IEntity parentEntity, object userData)
        {
            if (parentEntity == null)
            {
                throw new GameFrameworkException("Parent entity is invalid.");
            }

            DetachChildEntities(parentEntity.Id, userData);
        }

        private void InternalHideEntity(EntityInfo entityInfo, object userData)
        {
            while (entityInfo.ChildEntityCount > 0)
            {
                IEntity childEntity = entityInfo.GetChildEntity();
                HideEntity(childEntity.Id, userData);
            }

            if (entityInfo.Status == EntityStatus.Hidden)
            {
                return;
            }

            IEntity entity = entityInfo.Entity;
            DetachEntity(entity.Id, userData);
            entityInfo.Status = EntityStatus.WillHide;
            entity.OnHide(m_IsShutdown, userData);
            entityInfo.Status = EntityStatus.Hidden;

            EntityGroup entityGroup = (EntityGroup)entity.EntityGroup;
            if (entityGroup == null)
            {
                Log.Fatal("Entity group is invalid.");
                return;
            }

            entityGroup.RemoveEntity(entity);
            if (!m_EntityInfos.Remove(entity.Id))
            {
                Log.Fatal("Entity info is unmanaged.");
                return;
            }

            if (m_HideEntityCompleteEventHandler != null)
            {
                HideEntityCompleteEventArgs hideEntityCompleteEventArgs = HideEntityCompleteEventArgs.Create(entity.Id, entity.EntityAssetName, entityGroup, userData);
                m_HideEntityCompleteEventHandler(this, hideEntityCompleteEventArgs);
                // ReferencePool.Release(hideEntityCompleteEventArgs);
            }

            m_RecycleQueue.Enqueue(entityInfo);
        }
    }
}