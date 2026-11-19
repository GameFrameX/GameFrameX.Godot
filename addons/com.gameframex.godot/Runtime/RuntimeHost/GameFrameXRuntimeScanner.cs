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
using System.Reflection;

namespace GameFrameX.Runtime
{
    /// <summary>
    /// 扫描已加载程序集中的 GameFrameX 组件和 Manager。
    /// </summary>
    public static class GameFrameXRuntimeScanner
    {
        /// <summary>
        /// 执行全量运行时扫描。
        /// </summary>
        /// <returns>扫描结果。</returns>
        public static GameFrameXRuntimeScanResult Scan()
        {
            var result = new GameFrameXRuntimeScanResult();
            foreach (var assembly in Utility.Assembly.GetAssemblies())
            {
                ScanAssemblyInto(assembly, result);
            }

            return result;
        }

        /// <summary>
        /// 执行单个程序集的增量扫描：门与全量扫描一致（Editor / Tests 跳过），
        /// Manager 描述符收集口径与全量扫描共用同一条路径。
        /// </summary>
        /// <param name="assembly">要扫描的程序集。</param>
        /// <returns>扫描结果。</returns>
        public static GameFrameXRuntimeScanResult Scan(Assembly assembly)
        {
            var result = new GameFrameXRuntimeScanResult();
            ScanAssemblyInto(assembly, result);
            return result;
        }

        /// <summary>
        /// 判断程序集是否应被扫描门跳过（Editor / Tests），供增量注册与全量扫描共用同一口径。
        /// </summary>
        /// <param name="assembly">要判断的程序集。</param>
        /// <returns>是否跳过。</returns>
        internal static bool ShouldSkipAssembly(Assembly assembly)
        {
            string name = assembly.GetName().Name;
            return IsEditorAssemblyName(name) || IsTestAssemblyName(name);
        }

        private static void ScanAssemblyInto(Assembly assembly, GameFrameXRuntimeScanResult result)
        {
            if (assembly == null || ShouldSkipAssembly(assembly))
            {
                return;
            }

            var types = GetLoadableTypes(assembly, result);
            foreach (var type in types)
            {
                if (type == null || !type.IsClass || type.IsAbstract || type.IsGenericTypeDefinition)
                {
                    continue;
                }

                if (typeof(GameFrameworkComponent).IsAssignableFrom(type))
                {
                    if (type.GetCustomAttributes(typeof(GameFrameXManualStartupAttribute), false).Length <= 0)
                    {
                        result.ComponentTypes.Add(type);
                    }
                }

                if (typeof(IGameFrameworkModule).IsAssignableFrom(type))
                {
                    AddManagerDescriptors(result, type);
                }

                if (typeof(IGameFrameXOptionsContributor).IsAssignableFrom(type))
                {
                    result.OptionsContributorTypes.Add(type);
                }

                if (typeof(IGameFrameXRuntimeOverrideProvider).IsAssignableFrom(type))
                {
                    result.OverrideProviderTypes.Add(type);
                }
            }
        }

        private static bool IsEditorAssemblyName(string name)
        {
            return name.EndsWith(".Editor", StringComparison.Ordinal) ||
                   name.IndexOf("Editor", StringComparison.OrdinalIgnoreCase) >= 0 && name.StartsWith("UnityEditor", StringComparison.Ordinal);
        }

        private static bool IsTestAssemblyName(string name)
        {
            return name.EndsWith(".Tests", StringComparison.OrdinalIgnoreCase) ||
                   name.IndexOf(".Tests.", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static Type[] GetLoadableTypes(Assembly assembly, GameFrameXRuntimeScanResult result)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException exception)
            {
                result.Errors.Add(Utility.Text.Format("Assembly '{0}' contains unloaded types: {1}", assembly.FullName, exception.Message));
                return exception.Types;
            }
            catch (Exception exception)
            {
                result.Errors.Add(Utility.Text.Format("Can not scan assembly '{0}': {1}", assembly.FullName, exception.Message));
                return Array.Empty<Type>();
            }
        }

        private static void AddManagerDescriptors(GameFrameXRuntimeScanResult result, Type implementationType)
        {
            var attributes = implementationType.GetCustomAttributes(typeof(GameFrameXManagerImplementationAttribute), false);
            if (attributes.Length > 0)
            {
                foreach (GameFrameXManagerImplementationAttribute attribute in attributes)
                {
                    if (IsValidManagerInterface(attribute.InterfaceType, implementationType))
                    {
                        result.ManagerDescriptors.Add(new GameFrameXRuntimeManagerDescriptor(attribute.InterfaceType, implementationType, attribute.Priority));
                    }
                }

                return;
            }

            var interfaces = implementationType.GetInterfaces();
            foreach (var interfaceType in interfaces)
            {
                if (IsValidManagerInterface(interfaceType, implementationType))
                {
                    result.ManagerDescriptors.Add(new GameFrameXRuntimeManagerDescriptor(interfaceType, implementationType, 0));
                }
            }
        }

        private static bool IsValidManagerInterface(Type interfaceType, Type implementationType)
        {
            return interfaceType != null &&
                   interfaceType.IsInterface &&
                   interfaceType.FullName != null &&
                   interfaceType.FullName.StartsWith("GameFrameX.", StringComparison.Ordinal) &&
                   !IsFrameworkContractInterface(interfaceType) &&
                   interfaceType.IsAssignableFrom(implementationType);
        }

        private static bool IsFrameworkContractInterface(Type interfaceType)
        {
            return interfaceType == typeof(IGameFrameworkModule) ||
                   interfaceType == typeof(IUpdatableModule) ||
                   interfaceType == typeof(IShutdownableModule) ||
                   interfaceType == typeof(IPrioritizedModule) ||
                   interfaceType == typeof(ILowMemoryAwareModule);
        }
    }
}
