#if TOOLS
using System;
using GameFrameX.Editor;
using Godot;
using Type = System.Type;

namespace GameFrameX.ImageCache.Editor
{
    /// <summary>
    /// 图片缓存组件检查器插件。
    /// 迁移备注（Unity → Godot）：Unity 的 CustomEditor（ImageCacheComponentInspector）负责绘制
    /// 嵌套 Config 三字段与实现类型下拉；Godot 侧配置已拆平为组件 [Export] 字段（Inspector 原生编辑），
    /// 本插件仅保留实现类型下拉（ComponentTypeComponentInspector 基类能力）。
    /// </summary>
    [Tool]
    public partial class ImageCacheComponentInspectorPlugin : ComponentTypeComponentInspector
    {
        protected override Type GetComponentType()
        {
            return typeof(Runtime.ImageCacheComponent);
        }

        protected override Type GetManagerType()
        {
            return typeof(Runtime.IImageCacheManager);
        }
    }
}
#endif
