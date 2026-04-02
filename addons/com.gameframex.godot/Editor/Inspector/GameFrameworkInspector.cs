#if TOOLS
using System;
using System.Collections.Generic;
using Godot;

namespace GameFrameX.Editor
{
    /// <summary>
    /// 游戏框架 Inspector 抽象类。
    /// </summary>
    [Tool]
    public abstract partial class GameFrameworkInspector : EditorInspectorPlugin
    {
        private bool m_IsCompiling = false;

        /// <summary>
        /// 属性名称到 Helper 接口类型的映射表（不区分大小写）。
        /// 键为属性名称（移除特殊字符后），值为对应的接口类型。
        /// </summary>
        /// <remarks>
        /// Mapping table from property names to Helper interface types (case-insensitive).
        /// Key is the property name (with special characters removed), value is the corresponding interface type.
        /// </remarks>
        private Dictionary<string, System.Type> _helperPropertyTypeMap;

        /// <summary>
        /// 需要隐藏的属性名称集合（不区分大小写）。
        /// </summary>
        /// <remarks>
        /// Set of property names to hide (case-insensitive).
        /// </remarks>
        private HashSet<string> _hiddenPropertyNames;

        /// <summary>
        /// 获取属性名称到 Helper 接口类型的映射表（不区分大小写）。
        /// </summary>
        /// <returns></returns>
        public virtual Dictionary<string, System.Type> GetHelperPropertyTypeMap()
        {
            return new Dictionary<string, System.Type>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 获取需要隐藏的属性名称集合（不区分大小写）。
        /// 子类可以重写此方法来指定要隐藏的父类属性。
        /// </summary>
        /// <remarks>
        /// Gets the set of property names to hide (case-insensitive).
        /// Subclasses can override this method to specify parent class properties to hide.
        /// </remarks>
        /// <returns>需要隐藏的属性名称集合 / Set of property names to hide</returns>
        public virtual HashSet<string> GetHiddenPropertyNames()
        {
            return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 判断此 Inspector 插件是否可以处理指定的对象。
        /// </summary>
        /// <remarks>
        /// Determines whether this Inspector plugin can handle the specified object.
        /// </remarks>
        /// <param name="object">要检查的 Godot 对象 / The Godot object to check</param>
        /// <returns>如果对象不为 null 则返回 <c>true</c>；否则返回 <c>false</c> / <c>true</c> if the object is not null; otherwise <c>false</c></returns>
        public override bool _CanHandle(GodotObject @object)
        {
            return IsCanHandle(@object);
        }

        /// <summary>
        /// 判断此 Inspector 插件是否可以处理指定的对象。
        /// </summary>
        /// <param name="object">要检查的 Godot 对象 / The Godot object to check</param>
        /// <returns>如果可以处理则返回 <c>true</c>；否则返回 <c>false</c> / <c>true</c> if the object can be handled; otherwise <c>false</c></returns>
        protected abstract bool IsCanHandle(GodotObject @object);

        /// <summary>
        /// 解析属性并为匹配的 Helper 类型属性添加自定义编辑器。
        /// </summary>
        /// <remarks>
        /// Parses properties and adds custom editors for matching Helper type properties.
        /// </remarks>
        /// <param name="object">包含属性的对象 / The object containing the property</param>
        /// <param name="type">属性的类型 / The type of the property</param>
        /// <param name="name">属性名称 / The name of the property</param>
        /// <param name="hintType">属性提示类型 / The property hint type</param>
        /// <param name="hintString">属性提示字符串 / The property hint string</param>
        /// <param name="usageFlags">属性使用标志 / The property usage flags</param>
        /// <param name="wide">是否为宽属性 / Whether the property is wide</param>
        /// <returns>如果属性被处理则返回 <c>true</c>；否则返回 <c>false</c> / <c>true</c> if the property was handled; otherwise <c>false</c></returns>
        public override bool _ParseProperty(GodotObject @object, Variant.Type type, string name, PropertyHint hintType, string hintString, PropertyUsageFlags usageFlags, bool wide)
        {
            // 延迟初始化映射表，确保每个实例使用自己的映射
            _helperPropertyTypeMap ??= GetHelperPropertyTypeMap();

            string normalizedPropertyName = NormalizePropertyName(name);


            // 检查是否需要隐藏此属性
            _hiddenPropertyNames ??= GetHiddenPropertyNames();

            if (_hiddenPropertyNames.Contains(normalizedPropertyName))
            {
                return true; // 隐藏属性，不显示任何编辑器
            }

            if (!_helperPropertyTypeMap.TryGetValue(normalizedPropertyName, out System.Type helperInterfaceType))
            {
                return false;
            }

            AddPropertyEditor(name, new HelperTypeEditorProperty(name, helperInterfaceType));
            return true;
        }

        /// <summary>
        /// 规范化属性名称，移除特殊字符（保留下划线前缀的原始大小写）。
        /// </summary>
        /// <remarks>
        /// Normalizes the property name by removing special characters (preserving original case for underscore prefix).
        /// </remarks>
        /// <param name="propertyName">要规范化的属性名称 / The property name to normalize</param>
        /// <returns>规范化后的属性名称 / The normalized property name</returns>
        private static string NormalizePropertyName(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return string.Empty;
            }

            // 移除开头的下划线（如 m_TextHelperTypeName -> mTextHelperTypeName）
            if (propertyName.StartsWith("_"))
            {
                propertyName = propertyName.Substring(1);
            }

            // 移除所有非字母数字字符
            var buffer = new char[propertyName.Length];
            var count = 0;
            foreach (var c in propertyName)
            {
                if (char.IsLetterOrDigit(c))
                {
                    buffer[count++] = c;
                }
            }

            return count == 0 ? string.Empty : new string(buffer, 0, count);
        }
    }
}
#endif