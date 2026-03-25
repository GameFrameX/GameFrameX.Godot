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

using GameFrameX.ObjectPool;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameFrameX.Event.Runtime;
using GameFrameX.Runtime;
using Godot;

namespace GameFrameX.Entity.Runtime
{
    /// <summary>
    /// 实体组件。
    /// </summary>
    public sealed partial class EntityComponent : GameFrameworkComponent
    {
        private const int DefaultPriority = 0;
        private IEntityManager m_EntityManager = null;
        private EventComponent m_EventComponent = null;

        private readonly List<IEntity> m_InternalEntityResults = new List<IEntity>();

        [Export] private bool m_EnableShowEntityUpdateEvent = false;

        [Export] private bool m_EnableShowEntityDependencyAssetEvent = false;

        [Export] private Node m_InstanceRoot = null;

        [Export] private string m_EntityHelperTypeName = "GameFrameX.Entity.Runtime.DefaultEntityHelper";

        [Export] private EntityHelperBase m_CustomEntityHelper = null;

        [Export] private string m_EntityGroupHelperTypeName = "GameFrameX.Entity.Runtime.DefaultEntityGroupHelper";

        [Export] private EntityGroupHelperBase m_CustomEntityGroupHelper = null;

        private EntityGroup[] m_EntityGroups = null;

        [Export] private int m_DefaultCapacity = 512;

        [Export] private float m_DefaultExpireTime = 3600f;

        /// <summary>
        /// 获取实体数量。
        /// </summary>
        public int EntityCount
        {
            get { return m_EntityManager.EntityCount; }
        }

        /// <summary>
        /// 获取实体组数量。
        /// </summary>
        public int EntityGroupCount
        {
            get { return m_EntityManager.EntityGroupCount; }
        }

        /// <summary>
        /// 获取或设置实体实例对象池默认容量。
        /// </summary>
        public int DefaultCapacity
        {
            get { return m_DefaultCapacity; }
            set { m_DefaultCapacity = value; }
        }

        /// <summary>
        /// 获取或设置实体实例对象池默认过期时间（秒）。
        /// </summary>
        public float DefaultExpireTime
        {
            get { return m_DefaultExpireTime; }
            set { m_DefaultExpireTime = value; }
        }

        /// <summary>
        /// 游戏框架组件初始化。
        /// </summary>
        public override void _Ready()
        {
            ImplementationComponentType = Utility.Assembly.GetType(componentType);
            InterfaceComponentType = typeof(IEntityManager);
            base._Ready();
            m_EntityManager = GameFrameworkEntry.GetModule<IEntityManager>();
            if (m_EntityManager == null)
            {
                Log.Fatal("Entity manager is invalid.");
                return;
            }

            m_EntityManager.ShowEntitySuccess += OnShowEntitySuccess;
            m_EntityManager.ShowEntityFailure += OnShowEntityFailure;

            if (m_EnableShowEntityUpdateEvent)
            {
                m_EntityManager.ShowEntityUpdate += OnShowEntityUpdate;
            }

            if (m_EnableShowEntityDependencyAssetEvent)
            {
                m_EntityManager.ShowEntityDependencyAsset += OnShowEntityDependencyAsset;
            }

            m_EntityManager.HideEntityComplete += OnHideEntityComplete;

            m_EventComponent = GameEntry.GetComponent<EventComponent>();
            if (m_EventComponent == null)
            {
                Log.Fatal("Event component is invalid.");
                return;
            }

            m_EntityManager.SetObjectPoolManager(GameFrameworkEntry.GetModule<IObjectPoolManager>());

            EntityHelperBase entityHelper = Helper.CreateHelper(m_EntityHelperTypeName, m_CustomEntityHelper);
            if (entityHelper == null)
            {
                Log.Error("Can not create entity helper.");
                return;
            }

            entityHelper.Name = "Entity Helper";
            AddChild(entityHelper);

            m_EntityManager.SetEntityHelper(entityHelper);

            if (m_InstanceRoot == null)
            {
                m_InstanceRoot = new Node
                {
                    Name = "Entity Instances"
                };
                AddChild(m_InstanceRoot);
            }

            if (m_EntityGroups == null)
            {
                m_EntityGroups = Array.Empty<EntityGroup>();
            }

            for (int i = 0; i < m_EntityGroups.Length; i++)
            {
                var entityGroup = m_EntityGroups[i];
                if (!AddEntityGroup(entityGroup.Name, entityGroup.InstanceAutoReleaseInterval, entityGroup.InstanceCapacity, entityGroup.InstanceExpireTime, entityGroup.InstancePriority))
                {
                    Log.Warning("Add entity group '{0}' failure.", entityGroup.Name);
                    continue;
                }
            }
        }

        /// <summary>
        /// 是否存在实体组。
        /// </summary>
        /// <param name="entityGroupName">实体组名称。</param>
        /// <returns>是否存在实体组。</returns>
        public bool HasEntityGroup(string entityGroupName)
        {
            return m_EntityManager.HasEntityGroup(entityGroupName);
        }

        /// <summary>
        /// 获取实体组。
        /// </summary>
        /// <param name="entityGroupName">实体组名称。</param>
        /// <returns>要获取的实体组。</returns>
        public IEntityGroup GetEntityGroup(string entityGroupName)
        {
            return m_EntityManager.GetEntityGroup(entityGroupName);
        }

        /// <summary>
        /// 获取所有实体组。
        /// </summary>
        /// <returns>所有实体组。</returns>
        public IEntityGroup[] GetAllEntityGroups()
        {
            return m_EntityManager.GetAllEntityGroups();
        }

        /// <summary>
        /// 获取所有实体组。
        /// </summary>
        /// <param name="results">所有实体组。</param>
        public void GetAllEntityGroups(List<IEntityGroup> results)
        {
            m_EntityManager.GetAllEntityGroups(results);
        }

        /// <summary>
        /// 增加实体组。
        /// </summary>
        /// <param name="entityGroupName">实体组名称。</param>
        /// <param name="instanceAutoReleaseInterval">实体实例对象池自动释放可释放对象的间隔秒数。</param>
        /// <returns>是否增加实体组成功。</returns>
        public bool AddEntityGroup(string entityGroupName, float instanceAutoReleaseInterval)
        {
            return AddEntityGroup(entityGroupName, instanceAutoReleaseInterval, m_DefaultCapacity, m_DefaultExpireTime, DefaultPriority);
        }

        /// <summary>
        /// 增加实体组。
        /// </summary>
        /// <param name="entityGroupName">实体组名称。</param>
        /// <param name="instanceAutoReleaseInterval">实体实例对象池自动释放可释放对象的间隔秒数。</param>
        /// <param name="instanceCapacity">实体实例对象池容量。</param>
        /// <param name="instanceExpireTime">实体实例对象池对象过期秒数。</param>
        /// <param name="instancePriority">实体实例对象池的优先级。</param>
        /// <returns>是否增加实体组成功。</returns>
        public bool AddEntityGroup(string entityGroupName, float instanceAutoReleaseInterval, int instanceCapacity,
            float instanceExpireTime, int instancePriority)
        {
            if (m_EntityManager.HasEntityGroup(entityGroupName))
            {
                return false;
            }

            EntityGroupHelperBase entityGroupHelper = Helper.CreateHelper(m_EntityGroupHelperTypeName, m_CustomEntityGroupHelper, EntityGroupCount);
            if (entityGroupHelper == null)
            {
                Log.Error("Can not create entity group helper.");
                return false;
            }

            entityGroupHelper.Name = Utility.Text.Format("Entity Group - {0}", entityGroupName);
            m_InstanceRoot.AddChild(entityGroupHelper);

            return m_EntityManager.AddEntityGroup(entityGroupName, instanceAutoReleaseInterval, instanceCapacity, instanceExpireTime, instancePriority, entityGroupHelper);
        }

        /// <summary>
        /// 是否存在实体。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <returns>是否存在实体。</returns>
        public bool HasEntity(int entityId)
        {
            return m_EntityManager.HasEntity(entityId);
        }

        /// <summary>
        /// 是否存在实体。
        /// </summary>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <returns>是否存在实体。</returns>
        public bool HasEntity(string entityAssetName)
        {
            return m_EntityManager.HasEntity(entityAssetName);
        }

        /// <summary>
        /// 获取实体。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <returns>实体。</returns>
        public Entity GetEntity(int entityId)
        {
            return (Entity)m_EntityManager.GetEntity(entityId);
        }

        /// <summary>
        /// 获取实体。
        /// </summary>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <returns>要获取的实体。</returns>
        public Entity GetEntity(string entityAssetName)
        {
            return (Entity)m_EntityManager.GetEntity(entityAssetName);
        }

        /// <summary>
        /// 获取实体。
        /// </summary>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <returns>要获取的实体。</returns>
        public Entity[] GetEntities(string entityAssetName)
        {
            IEntity[] entities = m_EntityManager.GetEntities(entityAssetName);
            Entity[] entityImpls = new Entity[entities.Length];
            for (int i = 0; i < entities.Length; i++)
            {
                entityImpls[i] = (Entity)entities[i];
            }

            return entityImpls;
        }

        /// <summary>
        /// 获取实体。
        /// </summary>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <param name="results">要获取的实体。</param>
        public void GetEntities(string entityAssetName, List<Entity> results)
        {
            if (results == null)
            {
                Log.Error("Results is invalid.");
                return;
            }

            results.Clear();
            m_EntityManager.GetEntities(entityAssetName, m_InternalEntityResults);
            foreach (IEntity entity in m_InternalEntityResults)
            {
                results.Add((Entity)entity);
            }
        }

        /// <summary>
        /// 获取所有已加载的实体。
        /// </summary>
        /// <returns>所有已加载的实体。</returns>
        public Entity[] GetAllLoadedEntities()
        {
            IEntity[] entities = m_EntityManager.GetAllLoadedEntities();
            Entity[] entityImpls = new Entity[entities.Length];
            for (int i = 0; i < entities.Length; i++)
            {
                entityImpls[i] = (Entity)entities[i];
            }

            return entityImpls;
        }

        /// <summary>
        /// 获取所有已加载的实体。
        /// </summary>
        /// <param name="results">所有已加载的实体。</param>
        public void GetAllLoadedEntities(List<Entity> results)
        {
            if (results == null)
            {
                Log.Error("Results is invalid.");
                return;
            }

            results.Clear();
            m_EntityManager.GetAllLoadedEntities(m_InternalEntityResults);
            foreach (IEntity entity in m_InternalEntityResults)
            {
                results.Add((Entity)entity);
            }
        }

        /// <summary>
        /// 获取所有正在加载实体的编号。
        /// </summary>
        /// <returns>所有正在加载实体的编号。</returns>
        public int[] GetAllLoadingEntityIds()
        {
            return m_EntityManager.GetAllLoadingEntityIds();
        }

        /// <summary>
        /// 获取所有正在加载实体的编号。
        /// </summary>
        /// <param name="results">所有正在加载实体的编号。</param>
        public void GetAllLoadingEntityIds(List<int> results)
        {
            m_EntityManager.GetAllLoadingEntityIds(results);
        }

        /// <summary>
        /// 是否正在加载实体。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <returns>是否正在加载实体。</returns>
        public bool IsLoadingEntity(int entityId)
        {
            return m_EntityManager.IsLoadingEntity(entityId);
        }

        /// <summary>
        /// 是否是合法的实体。
        /// </summary>
        /// <param name="entity">实体。</param>
        /// <returns>实体是否合法。</returns>
        public bool IsValidEntity(Entity entity)
        {
            return m_EntityManager.IsValidEntity(entity);
        }

        /// <summary>
        /// 显示实体。
        /// </summary>
        /// <typeparam name="T">实体逻辑类型。</typeparam>
        /// <param name="entityId">实体编号。</param>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <param name="entityGroupName">实体组名称。</param>
        public Task<IEntity> ShowEntityAsync<T>(int entityId, string entityAssetName, string entityGroupName) where T : EntityLogic
        {
            return ShowEntityAsync(entityId, typeof(T), entityAssetName, entityGroupName, DefaultPriority, null);
        }

        /// <summary>
        /// 显示实体。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <param name="entityLogicType">实体逻辑类型。</param>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <param name="entityGroupName">实体组名称。</param>
        public Task<IEntity> ShowEntityAsync(int entityId, Type entityLogicType, string entityAssetName, string entityGroupName)
        {
            return ShowEntityAsync(entityId, entityLogicType, entityAssetName, entityGroupName, DefaultPriority, null);
        }

        /// <summary>
        /// 显示实体。
        /// </summary>
        /// <typeparam name="T">实体逻辑类型。</typeparam>
        /// <param name="entityId">实体编号。</param>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <param name="entityGroupName">实体组名称。</param>
        /// <param name="priority">加载实体资源的优先级。</param>
        public Task<IEntity> ShowEntityAsync<T>(int entityId, string entityAssetName, string entityGroupName, int priority) where T : EntityLogic
        {
            return ShowEntityAsync(entityId, typeof(T), entityAssetName, entityGroupName, priority, null);
        }

        /// <summary>
        /// 显示实体。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <param name="entityLogicType">实体逻辑类型。</param>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <param name="entityGroupName">实体组名称。</param>
        /// <param name="priority">加载实体资源的优先级。</param>
        public Task<IEntity> ShowEntityAsync(int entityId, Type entityLogicType, string entityAssetName, string entityGroupName, int priority)
        {
            return ShowEntityAsync(entityId, entityLogicType, entityAssetName, entityGroupName, priority, null);
        }

        /// <summary>
        /// 显示实体。
        /// </summary>
        /// <typeparam name="T">实体逻辑类型。</typeparam>
        /// <param name="entityId">实体编号。</param>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <param name="entityGroupName">实体组名称。</param>
        /// <param name="userData">用户自定义数据。</param>
        public Task<IEntity> ShowEntityAsync<T>(int entityId, string entityAssetName, string entityGroupName, object userData) where T : EntityLogic
        {
            return ShowEntityAsync(entityId, typeof(T), entityAssetName, entityGroupName, DefaultPriority, userData);
        }

        /// <summary>
        /// 显示实体。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <param name="entityLogicType">实体逻辑类型。</param>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <param name="entityGroupName">实体组名称。</param>
        /// <param name="userData">用户自定义数据。</param>
        public Task<IEntity> ShowEntityAsync(int entityId, Type entityLogicType, string entityAssetName, string entityGroupName, object userData)
        {
            return ShowEntityAsync(entityId, entityLogicType, entityAssetName, entityGroupName, DefaultPriority, userData);
        }

        /// <summary>
        /// 显示实体。
        /// </summary>
        /// <typeparam name="T">实体逻辑类型。</typeparam>
        /// <param name="entityId">实体编号。</param>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <param name="entityGroupName">实体组名称。</param>
        /// <param name="priority">加载实体资源的优先级。</param>
        /// <param name="userData">用户自定义数据。</param>
        public Task<IEntity> ShowEntityAsync<T>(int entityId, string entityAssetName, string entityGroupName, int priority, object userData) where T : EntityLogic
        {
            return ShowEntityAsync(entityId, typeof(T), entityAssetName, entityGroupName, priority, userData);
        }

        /// <summary>
        /// 显示实体。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <param name="entityLogicType">实体逻辑类型。</param>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <param name="entityGroupName">实体组名称。</param>
        /// <param name="priority">加载实体资源的优先级。</param>
        /// <param name="userData">用户自定义数据。</param>
        public async Task<IEntity> ShowEntityAsync(int entityId, Type entityLogicType, string entityAssetName, string entityGroupName, int priority, object userData)
        {
            if (entityLogicType == null)
            {
                Log.Error("Entity type is invalid.");
                return null;
            }

            return await m_EntityManager.ShowEntityAsync(entityId, entityAssetName, entityGroupName, priority, ShowEntityInfo.Create(entityLogicType, userData));
        }

        /// <summary>
        /// 隐藏实体。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        public void HideEntity(int entityId)
        {
            m_EntityManager.HideEntity(entityId);
        }

        /// <summary>
        /// 隐藏实体。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void HideEntity(int entityId, object userData)
        {
            m_EntityManager.HideEntity(entityId, userData);
        }

        /// <summary>
        /// 隐藏实体。
        /// </summary>
        /// <param name="entity">实体。</param>
        public void HideEntity(Entity entity)
        {
            m_EntityManager.HideEntity(entity);
        }

        /// <summary>
        /// 隐藏实体。
        /// </summary>
        /// <param name="entity">实体。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void HideEntity(Entity entity, object userData)
        {
            m_EntityManager.HideEntity(entity, userData);
        }

        /// <summary>
        /// 释放所有实体组。
        /// </summary>
        public void ReleaseAllEntityGroups()
        {
            m_EntityManager.ReleaseAllEntityGroups();
        }

        /// <summary>
        /// 释放实体组。
        /// </summary>
        /// <param name="entityGroupName">实体组名称。</param>
        public void ReleaseEntityGroup(string entityGroupName)
        {
            m_EntityManager.ReleaseEntityGroup(entityGroupName);
        }

        /// <summary>
        /// 隐藏所有已加载的实体。
        /// </summary>
        public void HideAllLoadedEntities()
        {
            m_EntityManager.HideAllLoadedEntities();
        }

        /// <summary>
        /// 隐藏所有已加载的实体。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        public void HideAllLoadedEntities(object userData)
        {
            m_EntityManager.HideAllLoadedEntities(userData);
        }

        /// <summary>
        /// 隐藏所有正在加载的实体。
        /// </summary>
        public void HideAllLoadingEntities()
        {
            m_EntityManager.HideAllLoadingEntities();
        }

        /// <summary>
        /// 获取父实体。
        /// </summary>
        /// <param name="childEntityId">要获取父实体的子实体的实体编号。</param>
        /// <returns>子实体的父实体。</returns>
        public Entity GetParentEntity(int childEntityId)
        {
            return (Entity)m_EntityManager.GetParentEntity(childEntityId);
        }

        /// <summary>
        /// 获取父实体。
        /// </summary>
        /// <param name="childEntity">要获取父实体的子实体。</param>
        /// <returns>子实体的父实体。</returns>
        public Entity GetParentEntity(Entity childEntity)
        {
            return (Entity)m_EntityManager.GetParentEntity(childEntity);
        }

        /// <summary>
        /// 获取子实体数量。
        /// </summary>
        /// <param name="parentEntityId">要获取子实体数量的父实体的实体编号。</param>
        /// <returns>子实体数量。</returns>
        public int GetChildEntityCount(int parentEntityId)
        {
            return m_EntityManager.GetChildEntityCount(parentEntityId);
        }

        /// <summary>
        /// 获取子实体。
        /// </summary>
        /// <param name="parentEntityId">要获取子实体的父实体的实体编号。</param>
        /// <returns>子实体。</returns>
        public Entity GetChildEntity(int parentEntityId)
        {
            return (Entity)m_EntityManager.GetChildEntity(parentEntityId);
        }

        /// <summary>
        /// 获取子实体。
        /// </summary>
        /// <param name="parentEntity">要获取子实体的父实体。</param>
        /// <returns>子实体。</returns>
        public Entity GetChildEntity(IEntity parentEntity)
        {
            return (Entity)m_EntityManager.GetChildEntity(parentEntity);
        }

        /// <summary>
        /// 获取所有子实体。
        /// </summary>
        /// <param name="parentEntityId">要获取所有子实体的父实体的实体编号。</param>
        /// <returns>所有子实体。</returns>
        public Entity[] GetChildEntities(int parentEntityId)
        {
            IEntity[] entities = m_EntityManager.GetChildEntities(parentEntityId);
            Entity[] entityImpls = new Entity[entities.Length];
            for (int i = 0; i < entities.Length; i++)
            {
                entityImpls[i] = (Entity)entities[i];
            }

            return entityImpls;
        }

        /// <summary>
        /// 获取所有子实体。
        /// </summary>
        /// <param name="parentEntityId">要获取所有子实体的父实体的实体编号。</param>
        /// <param name="results">所有子实体。</param>
        public void GetChildEntities(int parentEntityId, List<IEntity> results)
        {
            if (results == null)
            {
                Log.Error("Results is invalid.");
                return;
            }

            results.Clear();
            m_EntityManager.GetChildEntities(parentEntityId, m_InternalEntityResults);
            foreach (var entity in m_InternalEntityResults)
            {
                results.Add((Entity)entity);
            }
        }

        /// <summary>
        /// 获取所有子实体。
        /// </summary>
        /// <param name="parentEntity">要获取所有子实体的父实体。</param>
        /// <returns>所有子实体。</returns>
        public Entity[] GetChildEntities(Entity parentEntity)
        {
            IEntity[] entities = m_EntityManager.GetChildEntities(parentEntity);
            Entity[] entityImpls = new Entity[entities.Length];
            for (int i = 0; i < entities.Length; i++)
            {
                entityImpls[i] = (Entity)entities[i];
            }

            return entityImpls;
        }

        /// <summary>
        /// 获取所有子实体。
        /// </summary>
        /// <param name="parentEntity">要获取所有子实体的父实体。</param>
        /// <param name="results">所有子实体。</param>
        public void GetChildEntities(IEntity parentEntity, List<IEntity> results)
        {
            if (results == null)
            {
                Log.Error("Results is invalid.");
                return;
            }

            results.Clear();
            m_EntityManager.GetChildEntities(parentEntity, m_InternalEntityResults);
            foreach (IEntity entity in m_InternalEntityResults)
            {
                results.Add((Entity)entity);
            }
        }

        /// <summary>
        /// 附加子实体。
        /// </summary>
        /// <param name="childEntityId">要附加的子实体的实体编号。</param>
        /// <param name="parentEntityId">被附加的父实体的实体编号。</param>
        public void AttachEntity(int childEntityId, int parentEntityId)
        {
            AttachEntity(GetEntity(childEntityId), GetEntity(parentEntityId), string.Empty, null);
        }

        /// <summary>
        /// 附加子实体。
        /// </summary>
        /// <param name="childEntityId">要附加的子实体的实体编号。</param>
        /// <param name="parentEntity">被附加的父实体。</param>
        public void AttachEntity(int childEntityId, Entity parentEntity)
        {
            AttachEntity(GetEntity(childEntityId), parentEntity, string.Empty, null);
        }

        /// <summary>
        /// 附加子实体。
        /// </summary>
        /// <param name="childEntity">要附加的子实体。</param>
        /// <param name="parentEntityId">被附加的父实体的实体编号。</param>
        public void AttachEntity(Entity childEntity, int parentEntityId)
        {
            AttachEntity(childEntity, GetEntity(parentEntityId), string.Empty, null);
        }

        /// <summary>
        /// 附加子实体。
        /// </summary>
        /// <param name="childEntity">要附加的子实体。</param>
        /// <param name="parentEntity">被附加的父实体。</param>
        public void AttachEntity(Entity childEntity, Entity parentEntity)
        {
            AttachEntity(childEntity, parentEntity, string.Empty, null);
        }

        /// <summary>
        /// 附加子实体。
        /// </summary>
        /// <param name="childEntityId">要附加的子实体的实体编号。</param>
        /// <param name="parentEntityId">被附加的父实体的实体编号。</param>
        /// <param name="parentTransformPath">相对于被附加父实体的位置。</param>
        public void AttachEntity(int childEntityId, int parentEntityId, string parentTransformPath)
        {
            AttachEntity(GetEntity(childEntityId), GetEntity(parentEntityId), parentTransformPath, null);
        }

        /// <summary>
        /// 附加子实体。
        /// </summary>
        /// <param name="childEntityId">要附加的子实体的实体编号。</param>
        /// <param name="parentEntity">被附加的父实体。</param>
        /// <param name="parentTransformPath">相对于被附加父实体的位置。</param>
        public void AttachEntity(int childEntityId, Entity parentEntity, string parentTransformPath)
        {
            AttachEntity(GetEntity(childEntityId), parentEntity, parentTransformPath, null);
        }

        /// <summary>
        /// 附加子实体。
        /// </summary>
        /// <param name="childEntity">要附加的子实体。</param>
        /// <param name="parentEntityId">被附加的父实体的实体编号。</param>
        /// <param name="parentTransformPath">相对于被附加父实体的位置。</param>
        public void AttachEntity(Entity childEntity, int parentEntityId, string parentTransformPath)
        {
            AttachEntity(childEntity, GetEntity(parentEntityId), parentTransformPath, null);
        }

        /// <summary>
        /// 附加子实体。
        /// </summary>
        /// <param name="childEntity">要附加的子实体。</param>
        /// <param name="parentEntity">被附加的父实体。</param>
        /// <param name="parentTransformPath">相对于被附加父实体的位置。</param>
        public void AttachEntity(Entity childEntity, Entity parentEntity, string parentTransformPath)
        {
            AttachEntity(childEntity, parentEntity, parentTransformPath, null);
        }

        /// <summary>
        /// 附加子实体。
        /// </summary>
        /// <param name="childEntityId">要附加的子实体的实体编号。</param>
        /// <param name="parentEntityId">被附加的父实体的实体编号。</param>
        /// <param name="parentTransform">相对于被附加父实体的位置。</param>
        public void AttachEntity(int childEntityId, int parentEntityId, Node parentTransform)
        {
            AttachEntity(GetEntity(childEntityId), GetEntity(parentEntityId), parentTransform, null);
        }

        /// <summary>
        /// 附加子实体。
        /// </summary>
        /// <param name="childEntityId">要附加的子实体的实体编号。</param>
        /// <param name="parentEntity">被附加的父实体。</param>
        /// <param name="parentTransform">相对于被附加父实体的位置。</param>
        public void AttachEntity(int childEntityId, Entity parentEntity, Node parentTransform)
        {
            AttachEntity(GetEntity(childEntityId), parentEntity, parentTransform, null);
        }

        /// <summary>
        /// 附加子实体。
        /// </summary>
        /// <param name="childEntity">要附加的子实体。</param>
        /// <param name="parentEntityId">被附加的父实体的实体编号。</param>
        /// <param name="parentTransform">相对于被附加父实体的位置。</param>
        public void AttachEntity(Entity childEntity, int parentEntityId, Node parentTransform)
        {
            AttachEntity(childEntity, GetEntity(parentEntityId), parentTransform, null);
        }

        /// <summary>
        /// 附加子实体。
        /// </summary>
        /// <param name="childEntity">要附加的子实体。</param>
        /// <param name="parentEntity">被附加的父实体。</param>
        /// <param name="parentTransform">相对于被附加父实体的位置。</param>
        public void AttachEntity(Entity childEntity, Entity parentEntity, Node parentTransform)
        {
            AttachEntity(childEntity, parentEntity, parentTransform, null);
        }

        /// <summary>
        /// 附加子实体。
        /// </summary>
        /// <param name="childEntityId">要附加的子实体的实体编号。</param>
        /// <param name="parentEntityId">被附加的父实体的实体编号。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void AttachEntity(int childEntityId, int parentEntityId, object userData)
        {
            AttachEntity(GetEntity(childEntityId), GetEntity(parentEntityId), string.Empty, userData);
        }

        /// <summary>
        /// 附加子实体。
        /// </summary>
        /// <param name="childEntityId">要附加的子实体的实体编号。</param>
        /// <param name="parentEntity">被附加的父实体。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void AttachEntity(int childEntityId, Entity parentEntity, object userData)
        {
            AttachEntity(GetEntity(childEntityId), parentEntity, string.Empty, userData);
        }

        /// <summary>
        /// 附加子实体。
        /// </summary>
        /// <param name="childEntity">要附加的子实体。</param>
        /// <param name="parentEntityId">被附加的父实体的实体编号。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void AttachEntity(Entity childEntity, int parentEntityId, object userData)
        {
            AttachEntity(childEntity, GetEntity(parentEntityId), string.Empty, userData);
        }

        /// <summary>
        /// 附加子实体。
        /// </summary>
        /// <param name="childEntity">要附加的子实体。</param>
        /// <param name="parentEntity">被附加的父实体。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void AttachEntity(Entity childEntity, Entity parentEntity, object userData)
        {
            AttachEntity(childEntity, parentEntity, string.Empty, userData);
        }

        /// <summary>
        /// 附加子实体。
        /// </summary>
        /// <param name="childEntityId">要附加的子实体的实体编号。</param>
        /// <param name="parentEntityId">被附加的父实体的实体编号。</param>
        /// <param name="parentTransformPath">相对于被附加父实体的位置。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void AttachEntity(int childEntityId, int parentEntityId, string parentTransformPath, object userData)
        {
            AttachEntity(GetEntity(childEntityId), GetEntity(parentEntityId), parentTransformPath, userData);
        }

        /// <summary>
        /// 附加子实体。
        /// </summary>
        /// <param name="childEntityId">要附加的子实体的实体编号。</param>
        /// <param name="parentEntity">被附加的父实体。</param>
        /// <param name="parentTransformPath">相对于被附加父实体的位置。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void AttachEntity(int childEntityId, Entity parentEntity, string parentTransformPath, object userData)
        {
            AttachEntity(GetEntity(childEntityId), parentEntity, parentTransformPath, userData);
        }

        /// <summary>
        /// 附加子实体。
        /// </summary>
        /// <param name="childEntity">要附加的子实体。</param>
        /// <param name="parentEntityId">被附加的父实体的实体编号。</param>
        /// <param name="parentTransformPath">相对于被附加父实体的位置。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void AttachEntity(Entity childEntity, int parentEntityId, string parentTransformPath, object userData)
        {
            AttachEntity(childEntity, GetEntity(parentEntityId), parentTransformPath, userData);
        }

        /// <summary>
        /// 附加子实体。
        /// </summary>
        /// <param name="childEntity">要附加的子实体。</param>
        /// <param name="parentEntity">被附加的父实体。</param>
        /// <param name="parentTransformPath">相对于被附加父实体的位置。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void AttachEntity(Entity childEntity, Entity parentEntity, string parentTransformPath, object userData)
        {
            if (childEntity == null)
            {
                Log.Warning("Child entity is invalid.");
                return;
            }

            if (parentEntity == null)
            {
                Log.Warning("Parent entity is invalid.");
                return;
            }

            Node parentTransform = null;
            if (string.IsNullOrEmpty(parentTransformPath))
            {
                parentTransform = parentEntity.Logic.CachedTransform;
            }
            else
            {
                parentTransform = parentEntity.Logic.CachedTransform.GetNodeOrNull(parentTransformPath);
                if (parentTransform == null)
                {
                    Log.Warning("Can not find transform path '{0}' from parent entity '{1}'.", parentTransformPath,
                                parentEntity.Logic.Name);
                    parentTransform = parentEntity.Logic.CachedTransform;
                }
            }

            AttachEntity(childEntity, parentEntity, parentTransform, userData);
        }

        /// <summary>
        /// 附加子实体。
        /// </summary>
        /// <param name="childEntityId">要附加的子实体的实体编号。</param>
        /// <param name="parentEntityId">被附加的父实体的实体编号。</param>
        /// <param name="parentTransform">相对于被附加父实体的位置。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void AttachEntity(int childEntityId, int parentEntityId, Node parentTransform, object userData)
        {
            AttachEntity(GetEntity(childEntityId), GetEntity(parentEntityId), parentTransform, userData);
        }

        /// <summary>
        /// 附加子实体。
        /// </summary>
        /// <param name="childEntityId">要附加的子实体的实体编号。</param>
        /// <param name="parentEntity">被附加的父实体。</param>
        /// <param name="parentTransform">相对于被附加父实体的位置。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void AttachEntity(int childEntityId, Entity parentEntity, Node parentTransform, object userData)
        {
            AttachEntity(GetEntity(childEntityId), parentEntity, parentTransform, userData);
        }

        /// <summary>
        /// 附加子实体。
        /// </summary>
        /// <param name="childEntity">要附加的子实体。</param>
        /// <param name="parentEntityId">被附加的父实体的实体编号。</param>
        /// <param name="parentTransform">相对于被附加父实体的位置。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void AttachEntity(Entity childEntity, int parentEntityId, Node parentTransform, object userData)
        {
            AttachEntity(childEntity, GetEntity(parentEntityId), parentTransform, userData);
        }

        /// <summary>
        /// 附加子实体。
        /// </summary>
        /// <param name="childEntity">要附加的子实体。</param>
        /// <param name="parentEntity">被附加的父实体。</param>
        /// <param name="parentTransform">相对于被附加父实体的位置。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void AttachEntity(Entity childEntity, Entity parentEntity, Node parentTransform, object userData)
        {
            if (childEntity == null)
            {
                Log.Warning("Child entity is invalid.");
                return;
            }

            if (parentEntity == null)
            {
                Log.Warning("Parent entity is invalid.");
                return;
            }

            if (parentTransform == null)
            {
                parentTransform = parentEntity.Logic.CachedTransform;
            }

            m_EntityManager.AttachEntity(childEntity, parentEntity, AttachEntityInfo.Create(parentTransform, userData));
        }

        /// <summary>
        /// 解除子实体。
        /// </summary>
        /// <param name="childEntityId">要解除的子实体的实体编号。</param>
        public void DetachEntity(int childEntityId)
        {
            m_EntityManager.DetachEntity(childEntityId);
        }

        /// <summary>
        /// 解除子实体。
        /// </summary>
        /// <param name="childEntityId">要解除的子实体的实体编号。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void DetachEntity(int childEntityId, object userData)
        {
            m_EntityManager.DetachEntity(childEntityId, userData);
        }

        /// <summary>
        /// 解除子实体。
        /// </summary>
        /// <param name="childEntity">要解除的子实体。</param>
        public void DetachEntity(Entity childEntity)
        {
            m_EntityManager.DetachEntity(childEntity);
        }

        /// <summary>
        /// 解除子实体。
        /// </summary>
        /// <param name="childEntity">要解除的子实体。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void DetachEntity(Entity childEntity, object userData)
        {
            m_EntityManager.DetachEntity(childEntity, userData);
        }

        /// <summary>
        /// 解除所有子实体。
        /// </summary>
        /// <param name="parentEntityId">被解除的父实体的实体编号。</param>
        public void DetachChildEntities(int parentEntityId)
        {
            m_EntityManager.DetachChildEntities(parentEntityId);
        }

        /// <summary>
        /// 解除所有子实体。
        /// </summary>
        /// <param name="parentEntityId">被解除的父实体的实体编号。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void DetachChildEntities(int parentEntityId, object userData)
        {
            m_EntityManager.DetachChildEntities(parentEntityId, userData);
        }

        /// <summary>
        /// 解除所有子实体。
        /// </summary>
        /// <param name="parentEntity">被解除的父实体。</param>
        public void DetachChildEntities(Entity parentEntity)
        {
            m_EntityManager.DetachChildEntities(parentEntity);
        }

        /// <summary>
        /// 解除所有子实体。
        /// </summary>
        /// <param name="parentEntity">被解除的父实体。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void DetachChildEntities(Entity parentEntity, object userData)
        {
            m_EntityManager.DetachChildEntities(parentEntity, userData);
        }

        /// <summary>
        /// 设置实体是否被加锁。
        /// </summary>
        /// <param name="entity">实体。</param>
        /// <param name="locked">实体是否被加锁。</param>
        public void SetEntityInstanceLocked(Entity entity, bool locked)
        {
            if (entity == null)
            {
                Log.Warning("Entity is invalid.");
                return;
            }

            IEntityGroup entityGroup = entity.EntityGroup;
            if (entityGroup == null)
            {
                Log.Warning("Entity group is invalid.");
                return;
            }

            entityGroup.SetEntityInstanceLocked(entity.Handle, locked);
        }

        /// <summary>
        /// 设置实体的优先级。
        /// </summary>
        /// <param name="entity">实体。</param>
        /// <param name="priority">实体优先级。</param>
        public void SetInstancePriority(Entity entity, int priority)
        {
            if (entity == null)
            {
                Log.Warning("Entity is invalid.");
                return;
            }

            IEntityGroup entityGroup = entity.EntityGroup;
            if (entityGroup == null)
            {
                Log.Warning("Entity group is invalid.");
                return;
            }

            entityGroup.SetEntityInstancePriority(entity.Handle, priority);
        }

        private void OnShowEntitySuccess(object sender, ShowEntitySuccessEventArgs eventArgs)
        {
            m_EventComponent.Fire(this, eventArgs);
        }

        private void OnShowEntityFailure(object sender, ShowEntityFailureEventArgs eventArgs)
        {
            Log.Warning("Show entity failure, entity id '{0}', asset name '{1}', entity group name '{2}', error message '{3}'.",
                        eventArgs.EntityId, eventArgs.EntityAssetName, eventArgs.EntityGroupName, eventArgs.ErrorMessage);
            m_EventComponent.Fire(this, eventArgs);
        }

        private void OnShowEntityUpdate(object sender, ShowEntityUpdateEventArgs eventArgs)
        {
            m_EventComponent.Fire(this, eventArgs);
        }

        private void OnShowEntityDependencyAsset(object sender, ShowEntityDependencyAssetEventArgs eventArgs)
        {
            m_EventComponent.Fire(this, eventArgs);
        }

        private void OnHideEntityComplete(object sender, HideEntityCompleteEventArgs eventArgs)
        {
            m_EventComponent.Fire(this, eventArgs);
        }
    }
}
