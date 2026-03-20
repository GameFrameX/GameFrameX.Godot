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
using System.Threading.Tasks;
using GameFrameX.ObjectPool;
using GameFrameX.Runtime;
using Godot;

namespace GameFrameX.Entity.Runtime
{
    /// <summary>
    /// 实体管理器。
    /// </summary>
    public sealed partial class EntityManager : GameFrameworkModule, IEntityManager
    {
        private readonly Dictionary<int, EntityInfo> m_EntityInfos;
        private readonly Dictionary<string, EntityGroup> m_EntityGroups;
        private readonly Dictionary<int, int> m_EntitiesBeingLoaded;
        private readonly HashSet<int> m_EntitiesToReleaseOnLoad;
        private readonly Queue<EntityInfo> m_RecycleQueue;
        private IObjectPoolManager m_ObjectPoolManager;
        private IEntityHelper m_EntityHelper;
        private int m_Serial;
        private bool m_IsShutdown;
        private EventHandler<ShowEntitySuccessEventArgs> m_ShowEntitySuccessEventHandler;
        private EventHandler<ShowEntityFailureEventArgs> m_ShowEntityFailureEventHandler;
        private EventHandler<ShowEntityUpdateEventArgs> m_ShowEntityUpdateEventHandler;
        private EventHandler<ShowEntityDependencyAssetEventArgs> m_ShowEntityDependencyAssetEventHandler;
        private EventHandler<HideEntityCompleteEventArgs> m_HideEntityCompleteEventHandler;

        /// <summary>
        /// 初始化实体管理器的新实例。
        /// </summary>
        public EntityManager()
        {
            m_EntityInfos = new Dictionary<int, EntityInfo>();
            m_EntityGroups = new Dictionary<string, EntityGroup>(StringComparer.Ordinal);
            m_EntitiesBeingLoaded = new Dictionary<int, int>();
            m_EntitiesToReleaseOnLoad = new HashSet<int>();
            m_RecycleQueue = new Queue<EntityInfo>();
            // m_LoadAssetCallbacks = new LoadAssetCallbacks(LoadAssetSuccessCallback, LoadAssetFailureCallback, LoadAssetUpdateCallback, LoadAssetDependencyAssetCallback);
            m_ObjectPoolManager = null;
            m_EntityHelper = null;
            m_Serial = 0;
            m_IsShutdown = false;
            m_ShowEntitySuccessEventHandler = null;
            m_ShowEntityFailureEventHandler = null;
            m_ShowEntityUpdateEventHandler = null;
            m_ShowEntityDependencyAssetEventHandler = null;
            m_HideEntityCompleteEventHandler = null;
        }

        /// <summary>
        /// 获取实体数量。
        /// </summary>
        public int EntityCount
        {
            get { return m_EntityInfos.Count; }
        }

        /// <summary>
        /// 获取实体组数量。
        /// </summary>
        public int EntityGroupCount
        {
            get { return m_EntityGroups.Count; }
        }

        /// <summary>
        /// 显示实体成功事件。
        /// </summary>
        public event EventHandler<ShowEntitySuccessEventArgs> ShowEntitySuccess
        {
            add { m_ShowEntitySuccessEventHandler += value; }
            remove { m_ShowEntitySuccessEventHandler -= value; }
        }

        /// <summary>
        /// 显示实体失败事件。
        /// </summary>
        public event EventHandler<ShowEntityFailureEventArgs> ShowEntityFailure
        {
            add { m_ShowEntityFailureEventHandler += value; }
            remove { m_ShowEntityFailureEventHandler -= value; }
        }

        /// <summary>
        /// 显示实体更新事件。
        /// </summary>
        public event EventHandler<ShowEntityUpdateEventArgs> ShowEntityUpdate
        {
            add { m_ShowEntityUpdateEventHandler += value; }
            remove { m_ShowEntityUpdateEventHandler -= value; }
        }

        /// <summary>
        /// 显示实体时加载依赖资源事件。
        /// </summary>
        public event EventHandler<ShowEntityDependencyAssetEventArgs> ShowEntityDependencyAsset
        {
            add { m_ShowEntityDependencyAssetEventHandler += value; }
            remove { m_ShowEntityDependencyAssetEventHandler -= value; }
        }

        /// <summary>
        /// 隐藏实体完成事件。
        /// </summary>
        public event EventHandler<HideEntityCompleteEventArgs> HideEntityComplete
        {
            add { m_HideEntityCompleteEventHandler += value; }
            remove { m_HideEntityCompleteEventHandler -= value; }
        }

        /// <summary>
        /// 实体管理器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public override void Update(float elapseSeconds, float realElapseSeconds)
        {
            while (m_RecycleQueue.Count > 0)
            {
                EntityInfo entityInfo = m_RecycleQueue.Dequeue();
                IEntity entity = entityInfo.Entity;
                EntityGroup entityGroup = (EntityGroup)entity.EntityGroup;
                if (entityGroup == null)
                {
                    throw new GameFrameworkException("Entity group is invalid.");
                }

                entityInfo.Status = EntityStatus.WillRecycle;
                entity.OnRecycle();
                entityInfo.Status = EntityStatus.Recycled;
                entityGroup.UnspawnEntity(entity);
                ReferencePool.Release(entityInfo);
            }

            foreach (KeyValuePair<string, EntityGroup> entityGroup in m_EntityGroups)
            {
                entityGroup.Value.Update(elapseSeconds, realElapseSeconds);
            }
        }

        /// <summary>
        /// 关闭并清理实体管理器。
        /// </summary>
        public override void Shutdown()
        {
            m_IsShutdown = true;
            HideAllLoadedEntities();
            m_EntityGroups.Clear();
            m_EntitiesBeingLoaded.Clear();
            m_EntitiesToReleaseOnLoad.Clear();
            m_RecycleQueue.Clear();
        }

        /// <summary>
        /// 设置对象池管理器。
        /// </summary>
        /// <param name="objectPoolManager">对象池管理器。</param>
        public void SetObjectPoolManager(IObjectPoolManager objectPoolManager)
        {
            if (objectPoolManager == null)
            {
                throw new GameFrameworkException("Object pool manager is invalid.");
            }

            m_ObjectPoolManager = objectPoolManager;
        }

        /// <summary>
        /// 设置实体辅助器。
        /// </summary>
        /// <param name="entityHelper">实体辅助器。</param>
        public void SetEntityHelper(IEntityHelper entityHelper)
        {
            if (entityHelper == null)
            {
                throw new GameFrameworkException("Entity helper is invalid.");
            }

            m_EntityHelper = entityHelper;
        }

        /// <summary>
        /// 增加实体组。
        /// </summary>
        /// <param name="entityGroupName">实体组名称。</param>
        /// <param name="instanceAutoReleaseInterval">实体实例对象池自动释放可释放对象的间隔秒数。</param>
        /// <param name="instanceCapacity">实体实例对象池容量。</param>
        /// <param name="instanceExpireTime">实体实例对象池对象过期秒数。</param>
        /// <param name="instancePriority">实体实例对象池的优先级。</param>
        /// <param name="entityGroupHelper">实体组辅助器。</param>
        /// <returns>是否增加实体组成功。</returns>
        public bool AddEntityGroup(string entityGroupName, float instanceAutoReleaseInterval, int instanceCapacity, float instanceExpireTime, int instancePriority, IEntityGroupHelper entityGroupHelper)
        {
            if (string.IsNullOrEmpty(entityGroupName))
            {
                throw new GameFrameworkException("Entity group name is invalid.");
            }

            if (entityGroupHelper == null)
            {
                throw new GameFrameworkException("Entity group helper is invalid.");
            }

            if (m_ObjectPoolManager == null)
            {
                throw new GameFrameworkException("You must set object pool manager first.");
            }

            if (HasEntityGroup(entityGroupName))
            {
                return false;
            }

            m_EntityGroups.Add(entityGroupName, new EntityGroup(entityGroupName, instanceAutoReleaseInterval, instanceCapacity, instanceExpireTime, instancePriority, entityGroupHelper, m_ObjectPoolManager));

            return true;
        }

        /// <summary>
        /// 是否正在加载实体。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <returns>是否正在加载实体。</returns>
        public bool IsLoadingEntity(int entityId)
        {
            return m_EntitiesBeingLoaded.ContainsKey(entityId);
        }

        /// <summary>
        /// 显示实体。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <param name="entityGroupName">实体组名称。</param>
        public Task<IEntity> ShowEntityAsync(int entityId, string entityAssetName, string entityGroupName)
        {
            return ShowEntityAsync(entityId, entityAssetName, entityGroupName, 0, null);
        }

        /// <summary>
        /// 显示实体。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <param name="entityGroupName">实体组名称。</param>
        /// <param name="priority">加载实体资源的优先级。</param>
        public Task<IEntity> ShowEntityAsync(int entityId, string entityAssetName, string entityGroupName, int priority)
        {
            return ShowEntityAsync(entityId, entityAssetName, entityGroupName, priority, null);
        }

        /// <summary>
        /// 显示实体。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <param name="entityGroupName">实体组名称。</param>
        /// <param name="userData">用户自定义数据。</param>
        public Task<IEntity> ShowEntityAsync(int entityId, string entityAssetName, string entityGroupName, object userData)
        {
            return ShowEntityAsync(entityId, entityAssetName, entityGroupName, 0, userData);
        }

        /// <summary>
        /// 显示实体。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <param name="entityGroupName">实体组名称。</param>
        /// <param name="priority">加载实体资源的优先级。</param>
        /// <param name="userData">用户自定义数据。</param>
        public async Task<IEntity> ShowEntityAsync(int entityId, string entityAssetName, string entityGroupName, int priority, object userData)
        {
            if (m_EntityHelper == null)
            {
                throw new GameFrameworkException("You must set entity helper first.");
            }

            if (string.IsNullOrEmpty(entityAssetName))
            {
                throw new GameFrameworkException("Entity asset name is invalid.");
            }

            if (string.IsNullOrEmpty(entityGroupName))
            {
                throw new GameFrameworkException("Entity group name is invalid.");
            }

            if (HasEntity(entityId))
            {
                throw new GameFrameworkException(Utility.Text.Format("Entity id '{0}' is already exist.", entityId));
            }

            if (IsLoadingEntity(entityId))
            {
                throw new GameFrameworkException(Utility.Text.Format("Entity '{0}' is already being loaded.", entityId));
            }

            EntityGroup entityGroup = (EntityGroup)GetEntityGroup(entityGroupName);
            if (entityGroup == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Entity group '{0}' is not exist.", entityGroupName));
            }

            TaskCompletionSource<IEntity> tcs = new TaskCompletionSource<IEntity>();
            EntityInstanceObject entityInstanceObject = entityGroup.SpawnEntityInstanceObject(entityAssetName);
            if (entityInstanceObject == null)
            {
                int serialId = ++m_Serial;
                m_EntitiesBeingLoaded.Add(entityId, serialId);
                object entityAsset = await Task.Run(() => ResourceLoader.Load(entityAssetName));
                var newUserData = ShowEntityInfo.Create(serialId, entityId, entityGroup, userData);
                if (entityAsset != null)
                {
                    LoadAssetSuccessCallback(tcs, entityAssetName, entityAsset, 0f, newUserData);
                }
                else
                {
                    LoadAssetFailureCallback(tcs, entityAssetName, "Failed", $"Can not load resource '{entityAssetName}'.", newUserData);
                }

                return await tcs.Task;
            }

            InternalShowEntity(tcs, entityId, entityAssetName, entityGroup, entityInstanceObject.Target, false, 0f, userData);
            return await tcs.Task;
        }

        /// <summary>
        /// 附加子实体。
        /// </summary>
        /// <param name="childEntityId">要附加的子实体的实体编号。</param>
        /// <param name="parentEntityId">被附加的父实体的实体编号。</param>
        public void AttachEntity(int childEntityId, int parentEntityId)
        {
            AttachEntity(childEntityId, parentEntityId, null);
        }

        /// <summary>
        /// 附加子实体。
        /// </summary>
        /// <param name="childEntityId">要附加的子实体的实体编号。</param>
        /// <param name="parentEntityId">被附加的父实体的实体编号。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void AttachEntity(int childEntityId, int parentEntityId, object userData)
        {
            if (childEntityId == parentEntityId)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not attach entity when child entity id equals to parent entity id '{0}'.", parentEntityId));
            }

            EntityInfo childEntityInfo = GetEntityInfo(childEntityId);
            if (childEntityInfo == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find child entity '{0}'.", childEntityId));
            }

            if (childEntityInfo.Status >= EntityStatus.WillHide)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not attach entity when child entity status is '{0}'.", childEntityInfo.Status));
            }

            EntityInfo parentEntityInfo = GetEntityInfo(parentEntityId);
            if (parentEntityInfo == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not find parent entity '{0}'.", parentEntityId));
            }

            if (parentEntityInfo.Status >= EntityStatus.WillHide)
            {
                throw new GameFrameworkException(Utility.Text.Format("Can not attach entity when parent entity status is '{0}'.", parentEntityInfo.Status));
            }

            IEntity childEntity = childEntityInfo.Entity;
            IEntity parentEntity = parentEntityInfo.Entity;
            DetachEntity(childEntity.Id, userData);
            childEntityInfo.ParentEntity = parentEntity;
            parentEntityInfo.AddChildEntity(childEntity);
            parentEntity.OnAttached(childEntity, userData);
            childEntity.OnAttachTo(parentEntity, userData);
        }

        /// <summary>
        /// 附加子实体。
        /// </summary>
        /// <param name="childEntityId">要附加的子实体的实体编号。</param>
        /// <param name="parentEntity">被附加的父实体。</param>
        public void AttachEntity(int childEntityId, IEntity parentEntity)
        {
            AttachEntity(childEntityId, parentEntity, null);
        }

        /// <summary>
        /// 附加子实体。
        /// </summary>
        /// <param name="childEntityId">要附加的子实体的实体编号。</param>
        /// <param name="parentEntity">被附加的父实体。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void AttachEntity(int childEntityId, IEntity parentEntity, object userData)
        {
            if (parentEntity == null)
            {
                throw new GameFrameworkException("Parent entity is invalid.");
            }

            AttachEntity(childEntityId, parentEntity.Id, userData);
        }

        /// <summary>
        /// 附加子实体。
        /// </summary>
        /// <param name="childEntity">要附加的子实体。</param>
        /// <param name="parentEntityId">被附加的父实体的实体编号。</param>
        public void AttachEntity(IEntity childEntity, int parentEntityId)
        {
            AttachEntity(childEntity, parentEntityId, null);
        }

        /// <summary>
        /// 附加子实体。
        /// </summary>
        /// <param name="childEntity">要附加的子实体。</param>
        /// <param name="parentEntityId">被附加的父实体的实体编号。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void AttachEntity(IEntity childEntity, int parentEntityId, object userData)
        {
            if (childEntity == null)
            {
                throw new GameFrameworkException("Child entity is invalid.");
            }

            AttachEntity(childEntity.Id, parentEntityId, userData);
        }

        /// <summary>
        /// 附加子实体。
        /// </summary>
        /// <param name="childEntity">要附加的子实体。</param>
        /// <param name="parentEntity">被附加的父实体。</param>
        public void AttachEntity(IEntity childEntity, IEntity parentEntity)
        {
            AttachEntity(childEntity, parentEntity, null);
        }

        /// <summary>
        /// 附加子实体。
        /// </summary>
        /// <param name="childEntity">要附加的子实体。</param>
        /// <param name="parentEntity">被附加的父实体。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void AttachEntity(IEntity childEntity, IEntity parentEntity, object userData)
        {
            if (childEntity == null)
            {
                throw new GameFrameworkException("Child entity is invalid.");
            }

            if (parentEntity == null)
            {
                throw new GameFrameworkException("Parent entity is invalid.");
            }

            AttachEntity(childEntity.Id, parentEntity.Id, userData);
        }

        /// <summary>
        /// 获取实体信息。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <returns>实体信息。</returns>
        private EntityInfo GetEntityInfo(int entityId)
        {
            if (m_EntityInfos.TryGetValue(entityId, out var entityInfo))
            {
                return entityInfo;
            }

            return null;
        }

        private void InternalShowEntity(TaskCompletionSource<IEntity> tcs, int entityId, string entityAssetName, EntityGroup entityGroup, object entityInstance, bool isNewInstance, float duration, object userData)
        {
            try
            {
                IEntity entity = m_EntityHelper.CreateEntity(entityInstance, entityGroup, userData);
                if (entity == null)
                {
                    var exception = new GameFrameworkException("Can not create entity in entity helper.");
                    tcs.TrySetException(exception);
                    throw exception;
                }

                EntityInfo entityInfo = EntityInfo.Create(entity);
                m_EntityInfos.Add(entityId, entityInfo);
                entityInfo.Status = EntityStatus.WillInit;
                entity.OnInit(entityId, entityAssetName, entityGroup, isNewInstance, userData);
                entityInfo.Status = EntityStatus.Inited;
                entityGroup.AddEntity(entity);
                entityInfo.Status = EntityStatus.WillShow;
                entity.OnShow(userData);
                entityInfo.Status = EntityStatus.Showed;

                if (m_ShowEntitySuccessEventHandler != null)
                {
                    ShowEntitySuccessEventArgs showEntitySuccessEventArgs = ShowEntitySuccessEventArgs.Create(entity, duration, userData);
                    m_ShowEntitySuccessEventHandler(this, showEntitySuccessEventArgs);
                    // ReferencePool.Release(showEntitySuccessEventArgs);
                }

                    tcs.TrySetResult(entity);
            }
            catch (Exception exception)
            {
                if (m_ShowEntityFailureEventHandler != null)
                {
                    ShowEntityFailureEventArgs showEntityFailureEventArgs = ShowEntityFailureEventArgs.Create(entityId, entityAssetName, entityGroup.Name, exception.ToString(), userData);
                    m_ShowEntityFailureEventHandler(this, showEntityFailureEventArgs);
                    // ReferencePool.Release(showEntityFailureEventArgs);
                    return;
                }

                tcs.TrySetException(exception);
                throw;
            }
        }

        private void LoadAssetSuccessCallback(TaskCompletionSource<IEntity> tcs, string entityAssetName, object entityAsset, float duration, object userData)
        {
            ShowEntityInfo showEntityInfo = (ShowEntityInfo)userData;
            if (showEntityInfo == null)
            {
                var exception = new GameFrameworkException("Show entity info is invalid.");
                tcs.TrySetException(exception);
                throw exception;
            }

            if (m_EntitiesToReleaseOnLoad.Contains(showEntityInfo.SerialId))
            {
                m_EntitiesToReleaseOnLoad.Remove(showEntityInfo.SerialId);
                m_EntitiesBeingLoaded.Remove(showEntityInfo.EntityId);
                ReferencePool.Release(showEntityInfo);
                return;
            }

            m_EntitiesBeingLoaded.Remove(showEntityInfo.EntityId);
            EntityInstanceObject entityInstanceObject = EntityInstanceObject.Create(entityAssetName, entityAsset, m_EntityHelper.InstantiateEntity(entityAsset), m_EntityHelper);
            showEntityInfo.EntityGroup.RegisterEntityInstanceObject(entityInstanceObject, true);

            InternalShowEntity(tcs, showEntityInfo.EntityId, entityAssetName, showEntityInfo.EntityGroup, entityInstanceObject.Target, true, duration, showEntityInfo.UserData);
            ReferencePool.Release(showEntityInfo);
        }

        private void LoadAssetFailureCallback(TaskCompletionSource<IEntity> tcs, string entityAssetName, string status, string errorMessage, object userData)
        {
            ShowEntityInfo showEntityInfo = (ShowEntityInfo)userData;
            GameFrameworkException exception;
            if (showEntityInfo == null)
            {
                exception = new GameFrameworkException("Show entity info is invalid.");
                tcs.TrySetException(exception);
                throw exception;
            }

            if (m_EntitiesToReleaseOnLoad.Contains(showEntityInfo.SerialId))
            {
                m_EntitiesToReleaseOnLoad.Remove(showEntityInfo.SerialId);
                return;
            }

            m_EntitiesBeingLoaded.Remove(showEntityInfo.EntityId);
            string appendErrorMessage = Utility.Text.Format("Load entity failure, asset name '{0}', status '{1}', error message '{2}'.", entityAssetName, status, errorMessage);
            exception = new GameFrameworkException(appendErrorMessage);
            if (m_ShowEntityFailureEventHandler != null)
            {
                ShowEntityFailureEventArgs showEntityFailureEventArgs = ShowEntityFailureEventArgs.Create(showEntityInfo.EntityId, entityAssetName, showEntityInfo.EntityGroup.Name, appendErrorMessage, showEntityInfo.UserData);
                m_ShowEntityFailureEventHandler(this, showEntityFailureEventArgs);
                // ReferencePool.Release(showEntityFailureEventArgs);
                // return;
            }

            tcs.TrySetException(exception);
            throw exception;
        }

        private void LoadAssetUpdateCallback(string entityAssetName, float progress, object userData)
        {
            ShowEntityInfo showEntityInfo = (ShowEntityInfo)userData;
            if (showEntityInfo == null)
            {
                throw new GameFrameworkException("Show entity info is invalid.");
            }

            if (m_ShowEntityUpdateEventHandler != null)
            {
                ShowEntityUpdateEventArgs showEntityUpdateEventArgs = ShowEntityUpdateEventArgs.Create(showEntityInfo.EntityId, entityAssetName, showEntityInfo.EntityGroup.Name, progress, showEntityInfo.UserData);
                m_ShowEntityUpdateEventHandler(this, showEntityUpdateEventArgs);
                // ReferencePool.Release(showEntityUpdateEventArgs);
            }
        }

        private void LoadAssetDependencyAssetCallback(string entityAssetName, string dependencyAssetName, int loadedCount, int totalCount, object userData)
        {
            ShowEntityInfo showEntityInfo = (ShowEntityInfo)userData;
            if (showEntityInfo == null)
            {
                throw new GameFrameworkException("Show entity info is invalid.");
            }

            if (m_ShowEntityDependencyAssetEventHandler != null)
            {
                ShowEntityDependencyAssetEventArgs showEntityDependencyAssetEventArgs = ShowEntityDependencyAssetEventArgs.Create(showEntityInfo.EntityId, entityAssetName, showEntityInfo.EntityGroup.Name, dependencyAssetName, loadedCount, totalCount, showEntityInfo.UserData);
                m_ShowEntityDependencyAssetEventHandler(this, showEntityDependencyAssetEventArgs);
                // ReferencePool.Release(showEntityDependencyAssetEventArgs);
            }
        }
    }
}
