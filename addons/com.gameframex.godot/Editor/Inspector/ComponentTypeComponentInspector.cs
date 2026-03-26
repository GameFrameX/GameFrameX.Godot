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
            GD.Print($"[事件检查器] 开始判断 IsCanHandle，对象类型={@object?.GetType().FullName}");

            if (@object is not Node node)
            {
                GD.Print("[事件检查器] 未命中路径A：对象不是 Node，返回 false");
                return false;
            }

            GD.Print($"[事件检查器] 进入路径B：检查选中节点脚本，节点名={node.Name}");
            var scriptVariant = node.GetScript();
            if (scriptVariant.Obj is not CSharpScript cSharpScript)
            {
                return false;
            }

            var scriptClass = cSharpScript.GetClass();
            var scriptPath = cSharpScript.ResourcePath;
            GD.Print($"[事件检查器] 节点脚本信息：class={scriptClass}, path={scriptPath}");
            GD.Print($"[事件检查器] 组件类型：{componentType.FullName}");
            return IsMatchedEventComponentScript(cSharpScript, componentType, scriptPath);
        }

        /// <summary>
        /// 判断当前脚本是否命中 EventComponent，并打印每个判断分支的结果。
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

            var matchByShortIsClass = cSharpScript.IsClass(componentName);
            GD.Print($"[事件检查器] 路径B-1 IsClass(短类名)={matchByShortIsClass}");
            if (matchByShortIsClass)
            {
                GD.Print("[事件检查器] 命中路径B-1：IsClass(短类名) 命中，返回 true");
                return true;
            }

            var matchByFullIsClass = cSharpScript.IsClass(componentFullName);
            GD.Print($"[事件检查器] 路径B-2 IsClass(全类名:{componentFullName})={matchByFullIsClass}");
            if (matchByFullIsClass)
            {
                GD.Print("[事件检查器] 命中路径B-2：IsClass(全类名) 命中，返回 true");
                return true;
            }

            var matchByShortNameEquals = string.Equals(scriptClass, componentName, System.StringComparison.Ordinal);
            GD.Print($"[事件检查器] 路径B-3 GetClass==短类名={matchByShortNameEquals}");
            if (matchByShortNameEquals)
            {
                GD.Print("[事件检查器] 命中路径B-3：GetClass 与短类名相等，返回 true");
                return true;
            }

            var matchByFullNameEquals = string.Equals(scriptClass, componentFullName, System.StringComparison.Ordinal);
            GD.Print($"[事件检查器] 路径B-4 GetClass==全类名={matchByFullNameEquals}");
            if (matchByFullNameEquals)
            {
                GD.Print("[事件检查器] 命中路径B-4：GetClass 与全类名相等，返回 true");
                return true;
            }

            var matchByPackagePrefixAndClassSuffix =
                safeScriptPath.Contains(safePackagePathPrefix, System.StringComparison.OrdinalIgnoreCase) &&
                safeScriptPath.EndsWith($"/{componentName}.cs", System.StringComparison.OrdinalIgnoreCase);
            GD.Print($"[事件检查器] 路径B-6 包前缀+类名后缀匹配={matchByPackagePrefixAndClassSuffix}");
            if (matchByPackagePrefixAndClassSuffix)
            {
                GD.Print($"[事件检查器] 命中路径B-6：包含 {safePackagePathPrefix} 且以类名结尾，返回 true");
                return true;
            }

            var matchByPathEndsWith = safeScriptPath.EndsWith($"/{componentName}.cs", System.StringComparison.OrdinalIgnoreCase);
            GD.Print($"[事件检查器] 路径B-5 ResourcePath 后缀匹配={matchByPathEndsWith}");
            if (matchByPathEndsWith)
            {
                GD.Print("[事件检查器] 命中路径B-5：脚本路径后缀匹配 EventComponent.cs，返回 true");
                return true;
            }

            return false;
        }

        protected abstract System.Type GetManagerType();
    }
}
#endif