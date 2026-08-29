#if TOOLS
using Godot;

namespace GameFrameX.ImageCache.Editor
{
    /// <summary>
    /// 图片缓存模块编辑器插件入口。
    /// </summary>
    [Tool]
    public partial class GameFrameXImageCachePlugin : EditorPlugin
    {
        private ImageCacheComponentInspectorPlugin m_InspectorPlugin;

        public override void _EnterTree()
        {
            m_InspectorPlugin = new ImageCacheComponentInspectorPlugin();
            AddInspectorPlugin(m_InspectorPlugin);
        }

        public override void _ExitTree()
        {
            m_InspectorPlugin = null;
        }
    }
}
#endif
