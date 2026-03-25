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
        /// 是否存在实体组。
        /// </summary>
        /// <param name="entityGroupName">实体组名称。</param>
        /// <returns>是否存在实体组。</returns>
        public bool HasEntityGroup(string entityGroupName)
        {
            if (string.IsNullOrEmpty(entityGroupName))
            {
                throw new GameFrameworkException("Entity group name is invalid.");
            }

            return m_EntityGroups.ContainsKey(entityGroupName);
        }

        /// <summary>
        /// 获取实体组。
        /// </summary>
        /// <param name="entityGroupName">实体组名称。</param>
        /// <returns>要获取的实体组。</returns>
        public IEntityGroup GetEntityGroup(string entityGroupName)
        {
            if (string.IsNullOrEmpty(entityGroupName))
            {
                throw new GameFrameworkException("Entity group name is invalid.");
            }

            EntityGroup entityGroup = null;
            if (m_EntityGroups.TryGetValue(entityGroupName, out entityGroup))
            {
                return entityGroup;
            }

            return null;
        }

        /// <summary>
        /// 获取所有实体组。
        /// </summary>
        /// <returns>所有实体组。</returns>
        public IEntityGroup[] GetAllEntityGroups()
        {
            int index = 0;
            IEntityGroup[] results = new IEntityGroup[m_EntityGroups.Count];
            foreach (KeyValuePair<string, EntityGroup> entityGroup in m_EntityGroups)
            {
                results[index++] = entityGroup.Value;
            }

            return results;
        }

        /// <summary>
        /// 获取所有实体组。
        /// </summary>
        /// <param name="results">所有实体组。</param>
        public void GetAllEntityGroups(List<IEntityGroup> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (KeyValuePair<string, EntityGroup> entityGroup in m_EntityGroups)
            {
                results.Add(entityGroup.Value);
            }
        }

        /// <summary>
        /// 是否存在实体。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <returns>是否存在实体。</returns>
        public bool HasEntity(int entityId)
        {
            return m_EntityInfos.ContainsKey(entityId);
        }

        /// <summary>
        /// 是否存在实体。
        /// </summary>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <returns>是否存在实体。</returns>
        public bool HasEntity(string entityAssetName)
        {
            if (string.IsNullOrEmpty(entityAssetName))
            {
                throw new GameFrameworkException("Entity asset name is invalid.");
            }

            foreach (KeyValuePair<int, EntityInfo> entityInfo in m_EntityInfos)
            {
                if (entityInfo.Value.Entity.EntityAssetName == entityAssetName)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 获取实体。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <returns>要获取的实体。</returns>
        public IEntity GetEntity(int entityId)
        {
            EntityInfo entityInfo = GetEntityInfo(entityId);
            if (entityInfo == null)
            {
                return null;
            }

            return entityInfo.Entity;
        }

        /// <summary>
        /// 获取实体。
        /// </summary>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <returns>要获取的实体。</returns>
        public IEntity GetEntity(string entityAssetName)
        {
            if (string.IsNullOrEmpty(entityAssetName))
            {
                throw new GameFrameworkException("Entity asset name is invalid.");
            }

            foreach (KeyValuePair<int, EntityInfo> entityInfo in m_EntityInfos)
            {
                if (entityInfo.Value.Entity.EntityAssetName == entityAssetName)
                {
                    return entityInfo.Value.Entity;
                }
            }

            return null;
        }

        /// <summary>
        /// 获取实体。
        /// </summary>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <returns>要获取的实体。</returns>
        public IEntity[] GetEntities(string entityAssetName)
        {
            if (string.IsNullOrEmpty(entityAssetName))
            {
                throw new GameFrameworkException("Entity asset name is invalid.");
            }

            List<IEntity> results = new List<IEntity>();
            foreach (KeyValuePair<int, EntityInfo> entityInfo in m_EntityInfos)
            {
                if (entityInfo.Value.Entity.EntityAssetName == entityAssetName)
                {
                    results.Add(entityInfo.Value.Entity);
                }
            }

            return results.ToArray();
        }

        /// <summary>
        /// 获取实体。
        /// </summary>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <param name="results">要获取的实体。</param>
        public void GetEntities(string entityAssetName, List<IEntity> results)
        {
            if (string.IsNullOrEmpty(entityAssetName))
            {
                throw new GameFrameworkException("Entity asset name is invalid.");
            }

            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (KeyValuePair<int, EntityInfo> entityInfo in m_EntityInfos)
            {
                if (entityInfo.Value.Entity.EntityAssetName == entityAssetName)
                {
                    results.Add(entityInfo.Value.Entity);
                }
            }
        }

        /// <summary>
        /// 获取所有已加载的实体。
        /// </summary>
        /// <returns>所有已加载的实体。</returns>
        public IEntity[] GetAllLoadedEntities()
        {
            int index = 0;
            IEntity[] results = new IEntity[m_EntityInfos.Count];
            foreach (KeyValuePair<int, EntityInfo> entityInfo in m_EntityInfos)
            {
                results[index++] = entityInfo.Value.Entity;
            }

            return results;
        }

        /// <summary>
        /// 获取所有已加载的实体。
        /// </summary>
        /// <param name="results">所有已加载的实体。</param>
        public void GetAllLoadedEntities(List<IEntity> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (KeyValuePair<int, EntityInfo> entityInfo in m_EntityInfos)
            {
                results.Add(entityInfo.Value.Entity);
            }
        }

        /// <summary>
        /// 获取所有正在加载实体的编号。
        /// </summary>
        /// <returns>所有正在加载实体的编号。</returns>
        public int[] GetAllLoadingEntityIds()
        {
            int index = 0;
            int[] results = new int[m_EntitiesBeingLoaded.Count];
            foreach (KeyValuePair<int, int> entityBeingLoaded in m_EntitiesBeingLoaded)
            {
                results[index++] = entityBeingLoaded.Key;
            }

            return results;
        }

        /// <summary>
        /// 获取所有正在加载实体的编号。
        /// </summary>
        /// <param name="results">所有正在加载实体的编号。</param>
        public void GetAllLoadingEntityIds(List<int> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (var entityBeingLoaded in m_EntitiesBeingLoaded)
            {
                results.Add(entityBeingLoaded.Key);
            }
        }

        /// <summary>
        /// 是否是合法的实体。
        /// </summary>
        /// <param name="entity">实体。</param>
        /// <returns>实体是否合法。</returns>
        public bool IsValidEntity(IEntity entity)
        {
            if (entity == null)
            {
                return false;
            }

            return HasEntity(entity.Id);
        }

        /// <summary>
        /// 获取父实体。
        /// </summary>
        /// <param name="childEntityId">要获取父实体的子实体的实体编号。</param>
        /// <returns>子实体的父实体。</returns>
        public IEntity GetParentEntity(int childEntityId)
        {
            EntityInfo childEntityInfo = GetEntityInfo(childEntityId);
            if (childEntityInfo == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find child entity '{0}'.", childEntityId));
            }

            return childEntityInfo.ParentEntity;
        }

        /// <summary>
        /// 获取父实体。
        /// </summary>
        /// <param name="childEntity">要获取父实体的子实体。</param>
        /// <returns>子实体的父实体。</returns>
        public IEntity GetParentEntity(IEntity childEntity)
        {
            if (childEntity == null)
            {
                throw new GameFrameworkException("Child entity is invalid.");
            }

            return GetParentEntity(childEntity.Id);
        }

        /// <summary>
        /// 获取子实体数量。
        /// </summary>
        /// <param name="parentEntityId">要获取子实体数量的父实体的实体编号。</param>
        /// <returns>子实体数量。</returns>
        public int GetChildEntityCount(int parentEntityId)
        {
            EntityInfo parentEntityInfo = GetEntityInfo(parentEntityId);
            if (parentEntityInfo == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find parent entity '{0}'.", parentEntityId));
            }

            return parentEntityInfo.ChildEntityCount;
        }

        /// <summary>
        /// 获取子实体。
        /// </summary>
        /// <param name="parentEntityId">要获取子实体的父实体的实体编号。</param>
        /// <returns>子实体。</returns>
        public IEntity GetChildEntity(int parentEntityId)
        {
            EntityInfo parentEntityInfo = GetEntityInfo(parentEntityId);
            if (parentEntityInfo == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find parent entity '{0}'.", parentEntityId));
            }

            return parentEntityInfo.GetChildEntity();
        }

        /// <summary>
        /// 获取子实体。
        /// </summary>
        /// <param name="parentEntity">要获取子实体的父实体。</param>
        /// <returns>子实体。</returns>
        public IEntity GetChildEntity(IEntity parentEntity)
        {
            if (parentEntity == null)
            {
                throw new GameFrameworkException("Parent entity is invalid.");
            }

            return GetChildEntity(parentEntity.Id);
        }

        /// <summary>
        /// 获取所有子实体。
        /// </summary>
        /// <param name="parentEntityId">要获取所有子实体的父实体的实体编号。</param>
        /// <returns>所有子实体。</returns>
        public IEntity[] GetChildEntities(int parentEntityId)
        {
            EntityInfo parentEntityInfo = GetEntityInfo(parentEntityId);
            if (parentEntityInfo == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find parent entity '{0}'.", parentEntityId));
            }

            return parentEntityInfo.GetChildEntities();
        }

        /// <summary>
        /// 获取所有子实体。
        /// </summary>
        /// <param name="parentEntityId">要获取所有子实体的父实体的实体编号。</param>
        /// <param name="results">所有子实体。</param>
        public void GetChildEntities(int parentEntityId, List<IEntity> results)
        {
            EntityInfo parentEntityInfo = GetEntityInfo(parentEntityId);
            if (parentEntityInfo == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find parent entity '{0}'.", parentEntityId));
            }

            parentEntityInfo.GetChildEntities(results);
        }

        /// <summary>
        /// 获取所有子实体。
        /// </summary>
        /// <param name="parentEntity">要获取所有子实体的父实体。</param>
        /// <returns>所有子实体。</returns>
        public IEntity[] GetChildEntities(IEntity parentEntity)
        {
            if (parentEntity == null)
            {
                throw new GameFrameworkException("Parent entity is invalid.");
            }

            return GetChildEntities(parentEntity.Id);
        }

        /// <summary>
        /// 获取所有子实体。
        /// </summary>
        /// <param name="parentEntity">要获取所有子实体的父实体。</param>
        /// <param name="results">所有子实体。</param>
        public void GetChildEntities(IEntity parentEntity, List<IEntity> results)
        {
            if (parentEntity == null)
            {
                throw new GameFrameworkException("Parent entity is invalid.");
            }

            GetChildEntities(parentEntity.Id, results);
        }

        /// <summary>
        /// 释放所有实体组。
        /// </summary>
        /// <param name="expireTime">过期时间。默认值为 1 秒。</param>
        public void ReleaseAllEntityGroups(float expireTime = 1f)
        {
            var groupList = new List<IEntityGroup>(32);
            GetAllEntityGroups(groupList);
            foreach (var entityGroup in groupList)
            {
                ReleaseEntityGroup(entityGroup, expireTime);
            }
        }

        /// <summary>
        /// 释放实体组。
        /// </summary>
        /// <param name="entityGroup">实体组。</param>
        /// <param name="expireTime">过期时间。默认值为 1 秒。</param>
        private static void ReleaseEntityGroup(IEntityGroup entityGroup, float expireTime)
        {
            if (entityGroup == null)
            {
                return;
            }

            var currentExpireTime = entityGroup.InstanceExpireTime;
            entityGroup.InstanceExpireTime = expireTime;
            entityGroup.InstanceExpireTime = currentExpireTime;
        }

        /// <summary>
        /// 释放实体组。
        /// </summary>
        /// <param name="entityGroupName">实体组名称。</param>
        /// <param name="expireTime">过期时间。默认值为 1 秒。</param>
        public void ReleaseEntityGroup(string entityGroupName, float expireTime = 1f)
        {
            var entityGroup = GetEntityGroup(entityGroupName);

            ReleaseEntityGroup(entityGroup, expireTime);
        }
    }
}
