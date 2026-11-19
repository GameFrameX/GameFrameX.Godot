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
    /// 根据扫描结果和运行时覆盖生成自动启动计划（依赖拓扑排序 + 顺序 + 类型名决胜）。
    /// </summary>
    public static class GameFrameXRuntimePlanner
    {
        /// <summary>
        /// 创建自动运行时启动计划。
        /// </summary>
        /// <param name="scanResult">扫描结果。</param>
        /// <param name="overrideContext">运行时覆盖上下文。</param>
        /// <returns>自动启动计划。</returns>
        public static GameFrameXRuntimePlan CreatePlan(GameFrameXRuntimeScanResult scanResult, GameFrameXRuntimeOverrideContext overrideContext)
        {
            var plan = new GameFrameXRuntimePlan();
            if (scanResult == null)
            {
                return plan;
            }

            var configs = overrideContext != null ? overrideContext.GetComponentConfigs() : null;
            var descriptors = new Dictionary<Type, GameFrameXRuntimeComponentDescriptor>();

            foreach (var componentType in scanResult.ComponentTypes)
            {
                var descriptor = CreateDescriptor(componentType);
                if (configs != null && configs.TryGetValue(componentType, out var config))
                {
                    ApplyConfig(descriptor, config);
                    plan.Diagnostics.Add(Utility.Text.Format(
                        "Apply runtime override to component '{0}': enabled={1}, order={2}, manager={3}.",
                        componentType.FullName,
                        config.Enabled.HasValue ? config.Enabled.Value.ToString() : "default",
                        config.Order.HasValue ? config.Order.Value.ToString() : "default",
                        config.ManagerImplementationType != null ? config.ManagerImplementationType.FullName : config.ManagerImplementationTypeName ?? "default"));
                }

                if (!descriptor.Enabled)
                {
                    plan.Diagnostics.Add(Utility.Text.Format("Skip component '{0}' by runtime override.", componentType.FullName));
                    continue;
                }

                descriptors[componentType] = descriptor;
            }

            foreach (var descriptor in SortByDependency(descriptors.Values, plan.Diagnostics))
            {
                plan.Components.Add(descriptor);
            }

            foreach (var error in scanResult.Errors)
            {
                plan.Diagnostics.Add(error);
            }

            return plan;
        }

        private static GameFrameXRuntimeComponentDescriptor CreateDescriptor(Type componentType)
        {
            var descriptor = new GameFrameXRuntimeComponentDescriptor(componentType, 0);

            var autoAttributes = componentType.GetCustomAttributes(typeof(GameFrameXAutoComponentAttribute), false);
            if (autoAttributes.Length > 0)
            {
                var autoAttribute = (GameFrameXAutoComponentAttribute)autoAttributes[0];
                descriptor.Order = autoAttribute.Order;
                descriptor.Enabled = autoAttribute.EnabledByDefault;
                AddDependencies(descriptor, autoAttribute.Dependencies);
            }

            var dependencyAttributes = componentType.GetCustomAttributes(typeof(GameFrameXComponentDependencyAttribute), false);
            foreach (GameFrameXComponentDependencyAttribute dependencyAttribute in dependencyAttributes)
            {
                if (dependencyAttribute.DependencyType != null)
                {
                    descriptor.Dependencies.Add(dependencyAttribute.DependencyType);
                }
            }

            return descriptor;
        }

        private static void ApplyConfig(GameFrameXRuntimeComponentDescriptor descriptor, GameFrameXComponentRuntimeConfig config)
        {
            descriptor.Config = config;
            if (config.Enabled.HasValue)
            {
                descriptor.Enabled = config.Enabled.Value;
            }

            if (config.Order.HasValue)
            {
                descriptor.Order = config.Order.Value;
            }

            AddDependencies(descriptor, config.Dependencies);
        }

        private static void AddDependencies(GameFrameXRuntimeComponentDescriptor descriptor, IEnumerable<Type> dependencies)
        {
            if (dependencies == null)
            {
                return;
            }

            foreach (var dependency in dependencies)
            {
                if (dependency != null && !descriptor.Dependencies.Contains(dependency))
                {
                    descriptor.Dependencies.Add(dependency);
                }
            }
        }

        private static List<GameFrameXRuntimeComponentDescriptor> SortByDependency(IEnumerable<GameFrameXRuntimeComponentDescriptor> descriptors, IList<string> diagnostics)
        {
            var pending = new List<GameFrameXRuntimeComponentDescriptor>(descriptors);
            pending.Sort(CompareComponentDescriptor);
            var result = new List<GameFrameXRuntimeComponentDescriptor>(pending.Count);

            while (pending.Count > 0)
            {
                bool progressed = false;
                for (int i = 0; i < pending.Count; i++)
                {
                    if (DependenciesSatisfied(pending[i], result, pending))
                    {
                        result.Add(pending[i]);
                        pending.RemoveAt(i);
                        progressed = true;
                        break;
                    }
                }

                if (!progressed)
                {
                    if (diagnostics != null)
                    {
                        diagnostics.Add("Component dependency cycle detected. Falling back to order-only startup for remaining components.");
                    }

                    pending.Sort(CompareComponentDescriptor);
                    result.AddRange(pending);
                    pending.Clear();
                }
            }

            return result;
        }

        private static bool DependenciesSatisfied(GameFrameXRuntimeComponentDescriptor descriptor, List<GameFrameXRuntimeComponentDescriptor> resolved, List<GameFrameXRuntimeComponentDescriptor> pending)
        {
            foreach (var dependency in descriptor.Dependencies)
            {
                if (!ContainsAssignable(pending, dependency))
                {
                    continue;
                }

                if (!ContainsAssignable(resolved, dependency))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool ContainsAssignable(IEnumerable<GameFrameXRuntimeComponentDescriptor> descriptors, Type type)
        {
            foreach (var descriptor in descriptors)
            {
                if (type.IsAssignableFrom(descriptor.ComponentType))
                {
                    return true;
                }
            }

            return false;
        }

        private static int CompareComponentDescriptor(GameFrameXRuntimeComponentDescriptor x, GameFrameXRuntimeComponentDescriptor y)
        {
            int order = x.Order.CompareTo(y.Order);
            if (order != 0)
            {
                return order;
            }

            return string.Compare(x.ComponentType.FullName, y.ComponentType.FullName, StringComparison.Ordinal);
        }
    }
}
