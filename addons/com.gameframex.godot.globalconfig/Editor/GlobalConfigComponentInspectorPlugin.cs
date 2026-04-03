#if TOOLS
using System.Collections.Generic;
using GameFrameX.Editor;
using GameFrameX.GlobalConfig.Runtime;
using Godot;

namespace GameFrameX.GlobalConfig.Editor
{
    /// <summary>
    /// 全局配置组件检查器插件。
    /// </summary>
    [Tool]
    public partial class GlobalConfigComponentInspectorPlugin : GameFrameworkInspector
    {
        /// <summary>
        /// 获取需要隐藏的属性名称集合。
        /// </summary>
        /// <returns>隐藏属性名称集合。</returns>
        public override HashSet<string> GetHiddenPropertyNames()
        {
            return new HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
            {
                "componentType"
            };
        }

        /// <summary>
        /// 获取当前检查器绑定的组件类型。
        /// </summary>
        /// <returns>全局配置组件类型。</returns>
        protected override System.Type GetComponentType()
        {
            return typeof(GlobalConfigComponent);
        }

        /// <summary>
        /// 判断是否可处理指定对象。
        /// </summary>
        /// <param name="object">待检查对象。</param>
        /// <returns>可处理返回 true，否则返回 false。</returns>
        protected override bool IsCanHandle(GodotObject @object)
        {
            return IsBindComponent(@object);
        }
    }
}
#endif
