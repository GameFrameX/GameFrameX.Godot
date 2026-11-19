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
using System.Collections.Generic;

namespace GameFrameX.Runtime
{
    /// <summary>
    /// 运行时覆盖上下文：OverrideProvider 经本上下文收集组件配置覆盖与 Manager 覆盖。
    /// </summary>
    public sealed class GameFrameXRuntimeOverrideContext
    {
        private readonly Dictionary<Type, GameFrameXComponentRuntimeConfig> m_ComponentConfigs = new Dictionary<Type, GameFrameXComponentRuntimeConfig>(8);

        private readonly List<GameFrameXManagerRuntimeOverride> m_ManagerOverrides = new List<GameFrameXManagerRuntimeOverride>(8);

        /// <summary>
        /// 设置组件是否启用。
        /// </summary>
        /// <param name="componentType">组件类型。</param>
        /// <param name="enabled">是否启用。</param>
        public void SetComponentEnabled(Type componentType, bool enabled)
        {
            GetOrCreateComponentConfig(componentType).Enabled = enabled;
        }

        /// <summary>
        /// 设置组件挂载顺序。
        /// </summary>
        /// <param name="componentType">组件类型。</param>
        /// <param name="order">挂载顺序。</param>
        public void SetComponentOrder(Type componentType, int order)
        {
            GetOrCreateComponentConfig(componentType).Order = order;
        }

        /// <summary>
        /// 设置组件的 Manager 实现类型。
        /// </summary>
        /// <param name="componentType">组件类型。</param>
        /// <param name="managerImplementationType">Manager 实现类型。</param>
        public void SetComponentManager(Type componentType, Type managerImplementationType)
        {
            GetOrCreateComponentConfig(componentType).ManagerImplementationType = managerImplementationType;
        }

        /// <summary>
        /// 添加组件的附加依赖。
        /// </summary>
        /// <param name="componentType">组件类型。</param>
        /// <param name="dependencyType">依赖的组件类型。</param>
        public void AddComponentDependency(Type componentType, Type dependencyType)
        {
            if (dependencyType == null)
            {
                return;
            }

            var config = GetOrCreateComponentConfig(componentType);
            if (!config.Dependencies.Contains(dependencyType))
            {
                config.Dependencies.Add(dependencyType);
            }
        }

        /// <summary>
        /// 添加 Manager 运行时覆盖。
        /// </summary>
        /// <param name="managerOverride">Manager 运行时覆盖。</param>
        public void AddManagerOverride(GameFrameXManagerRuntimeOverride managerOverride)
        {
            if (managerOverride != null)
            {
                m_ManagerOverrides.Add(managerOverride);
            }
        }

        /// <summary>
        /// 获取全部组件配置覆盖。
        /// </summary>
        /// <returns>组件类型到运行时配置的字典。</returns>
        public IDictionary<Type, GameFrameXComponentRuntimeConfig> GetComponentConfigs()
        {
            return m_ComponentConfigs;
        }

        /// <summary>
        /// 获取全部 Manager 覆盖。
        /// </summary>
        /// <returns>Manager 运行时覆盖列表。</returns>
        public IList<GameFrameXManagerRuntimeOverride> GetManagerOverrides()
        {
            return m_ManagerOverrides;
        }

        private GameFrameXComponentRuntimeConfig GetOrCreateComponentConfig(Type componentType)
        {
            if (!m_ComponentConfigs.TryGetValue(componentType, out var config))
            {
                config = new GameFrameXComponentRuntimeConfig(componentType);
                m_ComponentConfigs.Add(componentType, config);
            }

            return config;
        }
    }
}
