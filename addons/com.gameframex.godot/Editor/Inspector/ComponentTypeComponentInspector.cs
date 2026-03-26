#if TOOLS
using System.Collections.Generic;
using Godot;

namespace GameFrameX.Editor
{
    [Tool]
    public abstract partial class ComponentTypeComponentInspector : GameFrameworkInspector
    {
        public const string DefaultPackagePathPrefix = "com.gameframex.godot";

        /// <summary>
        /// 属性名称到 Helper 接口类型的映射表（不区分大小写）。
        /// 键为属性名称（移除特殊字符后），值为对应的接口类型。
        /// </summary>
        /// <remarks>
        /// Mapping table from property names to Helper interface types (case-insensitive).
        /// Key is the property name (with special characters removed), value is the corresponding interface type.
        /// </remarks>
        public override Dictionary<string, System.Type> GetHelperPropertyTypeMap()
        {
            return new Dictionary<string, System.Type>(System.StringComparer.OrdinalIgnoreCase)
            {
                { "componentType", GetManagerType() },
            };
        }

        protected override bool IsCanHandle(GodotObject @object)
        {
            return IsBindComponent(@object);
        }

        protected abstract System.Type GetComponentType();

        private bool IsBindComponent(GodotObject @object)
        {
            var componentType = GetComponentType();
            // 仅 Node 节点可能挂载 C# 脚本，非 Node 直接不处理。
            if (@object is not Node node)
            {
                return false;
            }

            // 读取节点脚本并要求其为 CSharpScript，其他脚本类型不参与匹配。
            var scriptVariant = node.GetScript();
            if (scriptVariant.Obj is not CSharpScript cSharpScript)
            {
                return false;
            }

            var scriptPath = cSharpScript.ResourcePath;
            // 统一交给脚本匹配方法，按类型名与路径规则判断是否命中目标组件。
            return IsMatchedEventComponentScript(cSharpScript, componentType, scriptPath);
        }

        /// <summary>
        /// 判断当前脚本是否命中目标组件类型。
        /// 匹配按“由强到弱”的顺序进行：类型系统判断优先，字符串判断次之，路径兜底最后。
        /// </summary>
        /// <param name="cSharpScript">当前节点的 CSharpScript 对象。</param>
        /// <param name="typeName">组件类型。</param>
        /// <param name="scriptPath">脚本资源路径。</param>
        /// <param name="packagePathPrefix">脚本路径中要求包含的包前缀。</param>
        /// <returns>命中任意一个判断条件返回 true，否则返回 false。</returns>
        protected bool IsMatchedEventComponentScript(CSharpScript cSharpScript, System.Type typeName, string scriptPath, string packagePathPrefix = DefaultPackagePathPrefix)
        {
            var componentName = typeName.Name;
            var scriptClass = cSharpScript.GetClass();
            var componentFullName = typeName.FullName;
            var safeScriptPath = scriptPath ?? string.Empty;
            var safePackagePathPrefix = packagePathPrefix ?? string.Empty;

            // 规则 1：直接使用 IsClass 与短类名匹配（最常见场景）。
            var matchByShortIsClass = cSharpScript.IsClass(componentName);
            if (matchByShortIsClass)
            {
                return true;
            }

            // 规则 2：使用 IsClass 与全限定类名匹配，兼容命名空间场景。
            var matchByFullIsClass = cSharpScript.IsClass(componentFullName);
            if (matchByFullIsClass)
            {
                return true;
            }

            // 规则 3：比较 GetClass 与短类名，兼容部分编辑器返回值差异。
            var matchByShortNameEquals = string.Equals(scriptClass, componentName, System.StringComparison.Ordinal);
            if (matchByShortNameEquals)
            {
                return true;
            }

            // 规则 4：比较 GetClass 与全限定类名，进一步覆盖命名空间差异。
            var matchByFullNameEquals = string.Equals(scriptClass, componentFullName, System.StringComparison.Ordinal);
            if (matchByFullNameEquals)
            {
                return true;
            }

            // 规则 5：路径同时满足包前缀与类名后缀，避免同名脚本误判。
            var matchByPackagePrefixAndClassSuffix =
                safeScriptPath.Contains(safePackagePathPrefix, System.StringComparison.OrdinalIgnoreCase) &&
                safeScriptPath.EndsWith($"/{componentName}.cs", System.StringComparison.OrdinalIgnoreCase);
            if (matchByPackagePrefixAndClassSuffix)
            {
                return true;
            }

            // 规则 6：仅按脚本文件名后缀兜底匹配，作为最后手段。
            var matchByPathEndsWith = safeScriptPath.EndsWith($"/{componentName}.cs", System.StringComparison.OrdinalIgnoreCase);
            if (matchByPathEndsWith)
            {
                return true;
            }

            return false;
        }

        protected abstract System.Type GetManagerType();
    }
}
#endif
