#if TOOLS
using Godot;

namespace GameFrameX.GlobalConfig.Editor
{
    [Tool]
    public partial class GameFrameXGlobalConfigPlugin : EditorPlugin
    {
        private GlobalConfigComponentInspectorPlugin m_InspectorPlugin;

        /// <summary>
        /// 插件进入编辑器树时注册组件检查器。
        /// </summary>
        public override void _EnterTree()
        {
            m_InspectorPlugin = new GlobalConfigComponentInspectorPlugin();
            AddInspectorPlugin(m_InspectorPlugin);
        }

        /// <summary>
        /// 插件退出编辑器树时清理引用。
        /// </summary>
        public override void _ExitTree()
        {
            m_InspectorPlugin = null;
        }
    }
}
#endif
