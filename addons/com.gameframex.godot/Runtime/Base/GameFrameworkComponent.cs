// ==========================================================================================
//  GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//  GameFrameX organization and its derivative projects' copyrights, trademarks, patents and other related rights
//  均受中华人民共和国及相关国际法律法规保护。
//  are protected by the laws of the People's Republic of China and international regulations.
//
//  使用本项目须严格遵守相应法律法规与开源许可证之规定。
//  Usage of this project must strictly comply with applicable laws, regulations, and open-source licenses.
//
//  本项目采用 MIT 许可证与 Apache License 2.0 双许可证分发，
//  This project is dual-licensed under the MIT License and Apache License 2.0,
//  完整许可证文本请参见源代码根目录下的 LICENSE 文件。
//  please refer to the LICENSE file in the source code root directory for the full license text.
//
//  禁止利用本项目实施任何危害国家安全、破坏社会秩序、
//  It is prohibited to use this project to engage in any activities that endanger national security, disrupt social order,
//  侵犯他人合法权益等法律法规所禁止的行为！
//  or infringe upon the legal rights and interests of others, as prohibited by laws and regulations!
//  因基于本项目二次开发所产生的一切法律纠纷与责任，
//  Any legal disputes or liabilities arising from secondary development based on this project
//  本组织与贡献者概不承担。
//  shall be borne solely by the developer; the project organization and contributors assume no responsibility.
//
//  GitHub 仓库：https://github.com/GameFrameX
//  GitHub Repository: https://github.com/GameFrameX
//  Gitee  仓库：https://gitee.com/GameFrameX
//  Gitee Repository: https://gitee.com/GameFrameX
//  CNB 仓库：https://cnb.cool/GameFrameX
//  CNB Repository: https://cnb.cool/GameFrameX
//  官方文档：https://gameframex.doc.alianblank.com/
//  Official Documentation: https://gameframex.doc.alianblank.com/
// ==========================================================================================

using System;
using Godot;

namespace GameFrameX.Runtime
{
    /// <summary>
    /// 游戏框架组件抽象类。
    /// </summary>
    /// <remarks>
    /// 薄入口形态：组件只承载转发与生命周期接线；Manager 实现优先由组件显式指定，
    /// 未指定时经 <see cref="GameFrameXRuntimeManagerResolver"/> 按"配置类型名 &gt; 运行时覆盖 &gt; 扫描优先级 &gt; 命名约定"解析。
    /// 自动 Host 接管期间（<see cref="GameFrameXRuntimeHost.IsAutoInjectionClaimed"/> 为真）_Ready 的组件视为场景残留，
    /// 自我抑制：不注册、不初始化、不驱动。
    /// </remarks>
    public abstract partial class GameFrameworkComponent : Node, IGameFrameXComponentRuntimeConfigurable
    {
        /// <summary>
        /// 是否自动注册模块。
        /// </summary>
        protected bool IsAutoRegister { get; set; } = true;

        /// <summary>
        /// 实现类的类型。
        /// </summary>
        protected Type ImplementationComponentType = null;

        /// <summary>
        /// 接口类的类型。
        /// </summary>
        protected Type InterfaceComponentType = null;

        /// <summary>
        /// 游戏框架组件类型（Manager 实现类型全名；空表示经解析器自动解析）。
        /// </summary>
        [Export] protected string componentType = string.Empty;

        /// <summary>
        /// 是否被自动 Host 抑制（场景残留组件标记）。
        /// </summary>
        protected bool m_SuppressedByAutoHost;

        private bool m_RuntimeConfigApplied;

        /// <summary>
        /// 获取组件是否完成运行时初始化（注册 + 模块接线完成）。
        /// </summary>
        protected bool IsRuntimeComponentReady { get; private set; }

        /// <summary>
        /// 游戏框架组件初始化。
        /// </summary>
        public override void _Ready()
        {
            base._Ready();
            IsRuntimeComponentReady = false;

            // 自动注入接管期间 _Ready 的组件一律视为场景残留：自我抑制，不注册、不初始化、不驱动。
            if (GameFrameXRuntimeHost.IsAutoInjectionClaimed)
            {
                m_SuppressedByAutoHost = true;
                GameFrameXRuntimeHost.SuppressSceneEntry(this);
                return;
            }

            ApplyPendingRuntimeConfig();

            if (GameEntry.RegisterComponent(this) == false)
            {
                // 与 Unity 版行为对齐：注册失败时禁用组件，避免重复组件继续参与框架循环
                SetProcess(false);
                return;
            }

            if (IsAutoRegister)
            {
                if (ImplementationComponentType == null && InterfaceComponentType != null)
                {
                    ImplementationComponentType = GameFrameXRuntimeManagerResolver.Resolve(InterfaceComponentType, componentType);
                    if (ImplementationComponentType != null && string.IsNullOrEmpty(componentType))
                    {
                        componentType = ImplementationComponentType.FullName;
                    }
                }

                if (InterfaceComponentType == null || ImplementationComponentType == null)
                {
                    Log.Warning("GameFrameX component '{0}' can not resolve manager. Component has been disabled.", GetType().FullName);
                    SetProcess(false);
                    return;
                }

                GameFrameworkEntry.RegisterModule(InterfaceComponentType, ImplementationComponentType);
            }

            IsRuntimeComponentReady = true;
        }

        /// <summary>
        /// 应用组件运行时配置（Host 挂载时传入；重复调用只生效一次）。
        /// </summary>
        /// <param name="config">组件运行时配置。</param>
        public void ApplyRuntimeConfig(GameFrameXComponentRuntimeConfig config)
        {
            if (config == null || m_RuntimeConfigApplied)
            {
                return;
            }

            m_RuntimeConfigApplied = true;
            if (config.ManagerImplementationType != null)
            {
                ImplementationComponentType = config.ManagerImplementationType;
                componentType = config.ManagerImplementationType.FullName;
                return;
            }

            if (!string.IsNullOrEmpty(config.ManagerImplementationTypeName))
            {
                componentType = config.ManagerImplementationTypeName;
            }
        }

        private void ApplyPendingRuntimeConfig()
        {
            var config = GameFrameXRuntimeHost.GetPendingConfig(GetType());
            if (config != null)
            {
                ApplyRuntimeConfig(config);
            }
        }
    }
}
