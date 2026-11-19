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
using Godot;

namespace GameFrameX.Runtime
{
    /// <summary>
    /// Manager 实现解析器：按"组件配置类型名 > 运行时覆盖 > 扫描描述符（优先级）> 命名约定"的顺序解析模块接口的实现类型。
    /// </summary>
    /// <remarks>
    /// 迟加载程序集经 <see cref="GameFrameXRuntimeAssemblyLoadHook"/> 增量注册：程序集粒度幂等去重
    /// （同名程序集重复注册 / 热更卸载后同名再加载只生效一次），Editor / Tests 程序集走与全量扫描相同的门直接跳过。
    /// </remarks>
    public static class GameFrameXRuntimeManagerResolver
    {
        private static readonly Dictionary<Type, List<GameFrameXRuntimeManagerDescriptor>> ManagerDescriptors = new Dictionary<Type, List<GameFrameXRuntimeManagerDescriptor>>(32);

        private static readonly Dictionary<Type, GameFrameXManagerRuntimeOverride> ManagerOverrides = new Dictionary<Type, GameFrameXManagerRuntimeOverride>(32);

        private static readonly HashSet<string> RegisteredAssemblyNames = new HashSet<string>(StringComparer.Ordinal);

        private static bool s_DescriptorsReady;

        /// <summary>
        /// 重置解析器全部静态状态。
        /// </summary>
        public static void Reset()
        {
            ManagerDescriptors.Clear();
            ManagerOverrides.Clear();
            RegisteredAssemblyNames.Clear();
            s_DescriptorsReady = false;
        }

        /// <summary>
        /// 批量注册 Manager 描述符。
        /// </summary>
        /// <param name="descriptors">Manager 描述符列表。</param>
        public static void RegisterDescriptors(IEnumerable<GameFrameXRuntimeManagerDescriptor> descriptors)
        {
            RegisterDescriptors(descriptors, true);
        }

        internal static void RegisterDescriptors(IEnumerable<GameFrameXRuntimeManagerDescriptor> descriptors, bool markReady)
        {
            if (descriptors == null)
            {
                return;
            }

            foreach (var descriptor in descriptors)
            {
                if (descriptor == null || descriptor.InterfaceType == null || descriptor.ImplementationType == null)
                {
                    continue;
                }

                if (!ManagerDescriptors.TryGetValue(descriptor.InterfaceType, out var implementations))
                {
                    implementations = new List<GameFrameXRuntimeManagerDescriptor>(32);
                    ManagerDescriptors.Add(descriptor.InterfaceType, implementations);
                }

                implementations.Add(descriptor);
            }

            if (markReady)
            {
                s_DescriptorsReady = true;
            }
        }

        /// <summary>
        /// 增量注册单个程序集内的 Manager 描述：程序集粒度幂等去重，Editor / Tests 程序集直接跳过。
        /// </summary>
        /// <param name="assembly">迟加载的程序集。</param>
        public static void RegisterAssembly(System.Reflection.Assembly assembly)
        {
            RegisterAssembly(assembly, true);
        }

        internal static void RegisterAssembly(System.Reflection.Assembly assembly, bool announce)
        {
            // Reflection.Emit 的 AssemblyBuilder 尚未定义类型即触发 AssemblyLoad，且会污染程序集粒度去重集；
            // 直接忽略（不标记已注册），待真实 LoadFrom/Load 完成时再注册。
            if (assembly is System.Reflection.Emit.AssemblyBuilder)
            {
                return;
            }

            if (assembly == null)
            {
                return;
            }

            string assemblyName = assembly.GetName().Name;
            if (RegisteredAssemblyNames.Contains(assemblyName))
            {
                return;
            }

            RegisteredAssemblyNames.Add(assemblyName);

            if (GameFrameXRuntimeScanner.ShouldSkipAssembly(assembly))
            {
                return;
            }

            var scanResult = GameFrameXRuntimeScanner.Scan(assembly);
            int descriptorCount = scanResult.ManagerDescriptors.Count;
            if (descriptorCount > 0)
            {
                RegisterDescriptors(scanResult.ManagerDescriptors, false);
            }

            if (announce)
            {
                GD.Print(Utility.Text.Format(
                    "[GameFrameX] Late-loaded assembly '{0}' registered {1} manager descriptor(s).",
                    assembly.FullName,
                    descriptorCount.ToString()));
            }
        }

        /// <summary>
        /// 注册 Manager 运行时覆盖。
        /// </summary>
        /// <param name="overrides">Manager 运行时覆盖列表。</param>
        public static void RegisterOverrides(IEnumerable<GameFrameXManagerRuntimeOverride> overrides)
        {
            if (overrides == null)
            {
                return;
            }

            foreach (var managerOverride in overrides)
            {
                if (managerOverride == null || managerOverride.InterfaceType == null || managerOverride.ImplementationType == null)
                {
                    continue;
                }

                ManagerOverrides[managerOverride.InterfaceType] = managerOverride;
            }
        }

        /// <summary>
        /// 解析模块接口的实现类型。
        /// </summary>
        /// <param name="interfaceType">模块接口类型。</param>
        /// <param name="configuredTypeName">组件配置的实现类型全名（空表示未配置）。</param>
        /// <returns>实现类型；无法解析时为 null。</returns>
        public static Type Resolve(Type interfaceType, string configuredTypeName)
        {
            if (interfaceType == null)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(configuredTypeName))
            {
                Type configuredType = Utility.Assembly.GetType(configuredTypeName);
                if (configuredType == null)
                {
                    GD.PushWarning(Utility.Text.Format(
                        "GameFrameX auto runtime can not resolve configured manager '{0}' for interface '{1}'.",
                        configuredTypeName,
                        interfaceType.FullName));
                    return null;
                }

                if (!interfaceType.IsAssignableFrom(configuredType))
                {
                    GD.PushWarning(Utility.Text.Format(
                        "GameFrameX auto runtime configured manager '{0}' is not assignable to interface '{1}'.",
                        configuredType.FullName,
                        interfaceType.FullName));
                    return null;
                }

                GD.Print(Utility.Text.Format(
                    "GameFrameX auto runtime manager selected by component config: {0} -> {1}.",
                    interfaceType.FullName,
                    configuredType.FullName));
                return configuredType;
            }

            if (ManagerOverrides.TryGetValue(interfaceType, out var managerOverride))
            {
                GD.Print(Utility.Text.Format(
                    "GameFrameX auto runtime manager selected by runtime override: {0} -> {1}.",
                    interfaceType.FullName,
                    managerOverride.ImplementationType.FullName));
                return managerOverride.ImplementationType;
            }

            EnsureDescriptors();

            if (ManagerDescriptors.TryGetValue(interfaceType, out var descriptors) && descriptors.Count > 0)
            {
                descriptors.Sort(CompareManagerDescriptor);
                if (descriptors.Count > 1)
                {
                    GD.PushWarning(Utility.Text.Format(
                        "GameFrameX auto runtime manager conflict for interface '{0}'. Selected '{1}' by priority.",
                        interfaceType.FullName,
                        descriptors[0].ImplementationType.FullName));
                }
                else
                {
                    GD.Print(Utility.Text.Format(
                        "GameFrameX auto runtime manager selected by scan: {0} -> {1}.",
                        interfaceType.FullName,
                        descriptors[0].ImplementationType.FullName));
                }

                return descriptors[0].ImplementationType;
            }

            if (interfaceType.Name.Length > 1 && interfaceType.Name[0] == 'I')
            {
                string conventionName = Utility.Text.Format("{0}.{1}", interfaceType.Namespace, interfaceType.Name.Substring(1));
                Type conventionType = Utility.Assembly.GetType(conventionName);
                if (conventionType != null && interfaceType.IsAssignableFrom(conventionType))
                {
                    GD.Print(Utility.Text.Format(
                        "GameFrameX auto runtime manager selected by naming convention: {0} -> {1}.",
                        interfaceType.FullName,
                        conventionType.FullName));
                    return conventionType;
                }
            }

            GD.PushWarning(Utility.Text.Format(
                "GameFrameX auto runtime can not resolve manager for interface '{0}'.",
                interfaceType.FullName));
            return null;
        }

        private static void EnsureDescriptors()
        {
            if (s_DescriptorsReady)
            {
                return;
            }

            // 全量兜底与增量注册共用同一条按程序集注册路径（静默，避免启动期程序集级日志噪音）。
            foreach (var assembly in Utility.Assembly.GetAssemblies())
            {
                RegisterAssembly(assembly, false);
            }

            s_DescriptorsReady = true;
        }

        private static int CompareManagerDescriptor(GameFrameXRuntimeManagerDescriptor x, GameFrameXRuntimeManagerDescriptor y)
        {
            int priority = y.Priority.CompareTo(x.Priority);
            if (priority != 0)
            {
                return priority;
            }

            return string.Compare(x.ImplementationType.FullName, y.ImplementationType.FullName, StringComparison.Ordinal);
        }
    }
}
