#if TOOLS
using System.Collections.Generic;
using Godot;

namespace GameFrameX.Editor
{
    [Tool]
    public abstract partial class ComponentTypeComponentInspector : GameFrameworkInspector
    {
       

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

        protected abstract System.Type GetManagerType();
    }
}
#endif
