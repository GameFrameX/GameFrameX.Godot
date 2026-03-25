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

using GameFrameX;
using GameFrameX.Entity;
using System;
using GameFrameX.Runtime;
using Godot;

namespace GameFrameX.Entity.Runtime
{
    /// <summary>
    /// 实体。
    /// </summary>
    public sealed partial class Entity : Node, IEntity
    {
        private int m_Id;
        private string m_EntityAssetName;
        private IEntityGroup m_EntityGroup;
        private EntityLogic m_EntityLogic;

        /// <summary>
        /// 获取实体编号。
        /// </summary>
        public int Id
        {
            get { return m_Id; }
        }

        /// <summary>
        /// 获取实体资源名称。
        /// </summary>
        public string EntityAssetName
        {
            get { return m_EntityAssetName; }
        }

        /// <summary>
        /// 获取实体实例。
        /// </summary>
        public object Handle
        {
            get { return this; }
        }

        /// <summary>
        /// 获取实体所属的实体组。
        /// </summary>
        public IEntityGroup EntityGroup
        {
            get { return m_EntityGroup; }
        }

        /// <summary>
        /// 获取实体逻辑。
        /// </summary>
        public EntityLogic Logic
        {
            get { return m_EntityLogic; }
        }

        /// <summary>
        /// 实体初始化。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <param name="entityAssetName">实体资源名称。</param>
        /// <param name="entityGroup">实体所属的实体组。</param>
        /// <param name="isNewInstance">是否是新实例。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void OnInit(int entityId, string entityAssetName, IEntityGroup entityGroup, bool isNewInstance, object userData)
        {
            m_Id = entityId;
            m_EntityAssetName = entityAssetName;
            if (isNewInstance)
            {
                m_EntityGroup = entityGroup;
            }
            else if (m_EntityGroup != entityGroup)
            {
                Log.Error("Entity group is inconsistent for non-new-instance entity.");
                return;
            }

            ShowEntityInfo showEntityInfo = (ShowEntityInfo)userData;
            Type entityLogicType = showEntityInfo.EntityLogicType;
            if (entityLogicType == null)
            {
                Log.Error("Entity logic type is invalid.");
                return;
            }

            if (m_EntityLogic != null)
            {
                if (m_EntityLogic.GetType() == entityLogicType)
                {
                    m_EntityLogic.ProcessMode = ProcessModeEnum.Inherit;
                }
                else
                {
                    m_EntityLogic.QueueFree();
                    m_EntityLogic = null;
                }
            }

            if (m_EntityLogic == null)
            {
                m_EntityLogic = Activator.CreateInstance(entityLogicType) as EntityLogic;
                if (m_EntityLogic == null)
                {
                    Log.Error("Entity '{0}' can not add entity logic.", entityAssetName);
                    return;
                }

                AddChild(m_EntityLogic);
            }

            try
            {
                m_EntityLogic.OnInit(showEntityInfo.UserData);
            }
            catch (Exception exception)
            {
                Log.Error("Entity '[{0}]{1}' OnInit with exception '{2}'.", m_Id, m_EntityAssetName, exception);
            }
        }

        /// <summary>
        /// 实体回收。
        /// </summary>
        public void OnRecycle()
        {
            try
            {
                m_EntityLogic.OnRecycle();
                m_EntityLogic.ProcessMode = ProcessModeEnum.Disabled;
            }
            catch (Exception exception)
            {
                Log.Error("Entity '[{0}]{1}' OnRecycle with exception '{2}'.", m_Id, m_EntityAssetName, exception);
            }

            m_Id = 0;
        }

        /// <summary>
        /// 实体显示。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        public void OnShow(object userData)
        {
            ShowEntityInfo showEntityInfo = (ShowEntityInfo)userData;
            try
            {
                m_EntityLogic.OnShow(showEntityInfo.UserData);
            }
            catch (Exception exception)
            {
                Log.Error("Entity '[{0}]{1}' OnShow with exception '{2}'.", m_Id, m_EntityAssetName, exception);
            }
        }

        /// <summary>
        /// 实体隐藏。
        /// </summary>
        /// <param name="isShutdown">是否是关闭实体管理器时触发。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void OnHide(bool isShutdown, object userData)
        {
            try
            {
                m_EntityLogic.OnHide(isShutdown, userData);
            }
            catch (Exception exception)
            {
                Log.Error("Entity '[{0}]{1}' OnHide with exception '{2}'.", m_Id, m_EntityAssetName, exception);
            }
        }

        /// <summary>
        /// 实体附加子实体。
        /// </summary>
        /// <param name="childEntity">附加的子实体。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void OnAttached(IEntity childEntity, object userData)
        {
            AttachEntityInfo attachEntityInfo = (AttachEntityInfo)userData;
            try
            {
                m_EntityLogic.OnAttached(((Entity)childEntity).Logic, attachEntityInfo.ParentTransform, attachEntityInfo.UserData);
            }
            catch (Exception exception)
            {
                Log.Error("Entity '[{0}]{1}' OnAttached with exception '{2}'.", m_Id, m_EntityAssetName, exception);
            }
        }

        /// <summary>
        /// 实体解除子实体。
        /// </summary>
        /// <param name="childEntity">解除的子实体。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void OnDetached(IEntity childEntity, object userData)
        {
            try
            {
                m_EntityLogic.OnDetached(((Entity)childEntity).Logic, userData);
            }
            catch (Exception exception)
            {
                Log.Error("Entity '[{0}]{1}' OnDetached with exception '{2}'.", m_Id, m_EntityAssetName, exception);
            }
        }

        /// <summary>
        /// 实体附加子实体。
        /// </summary>
        /// <param name="parentEntity">被附加的父实体。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void OnAttachTo(IEntity parentEntity, object userData)
        {
            AttachEntityInfo attachEntityInfo = (AttachEntityInfo)userData;
            try
            {
                m_EntityLogic.OnAttachTo(((Entity)parentEntity).Logic, attachEntityInfo.ParentTransform, attachEntityInfo.UserData);
            }
            catch (Exception exception)
            {
                Log.Error("Entity '[{0}]{1}' OnAttachTo with exception '{2}'.", m_Id, m_EntityAssetName, exception);
            }

            ReferencePool.Release(attachEntityInfo);
        }

        /// <summary>
        /// 实体解除子实体。
        /// </summary>
        /// <param name="parentEntity">被解除的父实体。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void OnDetachFrom(IEntity parentEntity, object userData)
        {
            try
            {
                m_EntityLogic.OnDetachFrom(((Entity)parentEntity).Logic, userData);
            }
            catch (Exception exception)
            {
                Log.Error("Entity '[{0}]{1}' OnDetachFrom with exception '{2}'.", m_Id, m_EntityAssetName, exception);
            }
        }

        /// <summary>
        /// 实体轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            try
            {
                m_EntityLogic.OnUpdate(elapseSeconds, realElapseSeconds);
            }
            catch (Exception exception)
            {
                Log.Error("Entity '[{0}]{1}' OnUpdate with exception '{2}'.", m_Id, m_EntityAssetName, exception);
            }
        }
    }
}
