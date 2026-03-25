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

using GameFrameX.Runtime;
using Godot;

namespace GameFrameX.Entity.Runtime
{
    /// <summary>
    /// 实体逻辑基类。
    /// </summary>
    public abstract partial class EntityLogic : Node
    {
        private bool m_Available = false;
        private bool m_Visible = false;
        private Entity m_Entity = null;
        private Node m_CachedTransform = null;
        private Node m_OriginalTransform = null;

        /// <summary>
        /// 获取实体。
        /// </summary>
        public Entity Entity
        {
            get { return m_Entity; }
        }

        /// <summary>
        /// 获取或设置实体名称。
        /// </summary>
        public new string Name
        {
            get { return base.Name.ToString(); }
            set { base.Name = value; }
        }

        /// <summary>
        /// 获取实体是否可用。
        /// </summary>
        public bool Available
        {
            get { return m_Available; }
        }

        /// <summary>
        /// 获取或设置实体是否可见。
        /// </summary>
        public bool Visible
        {
            get { return m_Available && m_Visible; }
            set
            {
                if (!m_Available)
                {
                    Log.Warning("Entity '{0}' is not available.", Name);
                    return;
                }

                if (m_Visible == value)
                {
                    return;
                }

                m_Visible = value;
                InternalSetVisible(value);
            }
        }

        /// <summary>
        /// 获取已缓存的 Transform。
        /// </summary>
        public Node CachedTransform
        {
            get { return m_CachedTransform; }
        }

        /// <summary>
        /// 实体初始化。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        protected internal virtual void OnInit(object userData)
        {
            if (m_CachedTransform == null)
            {
                m_CachedTransform = this;
            }

            m_Entity = GetParentOrNull<Entity>();
            m_OriginalTransform = CachedTransform.GetParent();
        }

        /// <summary>
        /// 实体回收。
        /// </summary>
        protected internal virtual void OnRecycle()
        {
        }

        /// <summary>
        /// 实体显示。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        protected internal virtual void OnShow(object userData)
        {
            m_Available = true;
            Visible = true;
        }

        /// <summary>
        /// 实体隐藏。
        /// </summary>
        /// <param name="isShutdown">是否是关闭实体管理器时触发。</param>
        /// <param name="userData">用户自定义数据。</param>
        protected internal virtual void OnHide(bool isShutdown, object userData)
        {
            Visible = false;
            m_Available = false;
        }

        /// <summary>
        /// 实体附加子实体。
        /// </summary>
        /// <param name="childEntity">附加的子实体。</param>
        /// <param name="parentTransform">被附加父实体的位置。</param>
        /// <param name="userData">用户自定义数据。</param>
        protected internal virtual void OnAttached(EntityLogic childEntity, Node parentTransform, object userData)
        {
        }

        /// <summary>
        /// 实体解除子实体。
        /// </summary>
        /// <param name="childEntity">解除的子实体。</param>
        /// <param name="userData">用户自定义数据。</param>
        protected internal virtual void OnDetached(EntityLogic childEntity, object userData)
        {
        }

        /// <summary>
        /// 实体附加子实体。
        /// </summary>
        /// <param name="parentEntity">被附加的父实体。</param>
        /// <param name="parentTransform">被附加父实体的位置。</param>
        /// <param name="userData">用户自定义数据。</param>
        protected internal virtual void OnAttachTo(EntityLogic parentEntity, Node parentTransform, object userData)
        {
            CachedTransform.Reparent(parentTransform);
        }

        /// <summary>
        /// 实体解除子实体。
        /// </summary>
        /// <param name="parentEntity">被解除的父实体。</param>
        /// <param name="userData">用户自定义数据。</param>
        protected internal virtual void OnDetachFrom(EntityLogic parentEntity, object userData)
        {
            CachedTransform.Reparent(m_OriginalTransform);
        }

        /// <summary>
        /// 实体轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        protected internal virtual void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
        }

        /// <summary>
        /// 设置实体的可见性。
        /// </summary>
        /// <param name="visible">实体的可见性。</param>
        protected virtual void InternalSetVisible(bool visible)
        {
            Node targetNode = CachedTransform ?? (Node)m_Entity ?? this;
            if (targetNode is CanvasItem canvasItem)
            {
                canvasItem.Visible = visible;
            }
            else if (targetNode is Node3D node3D)
            {
                node3D.Visible = visible;
            }
            ProcessMode = visible ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;
        }
    }
}
