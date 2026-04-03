#if TOOLS
using Godot;

namespace GameFrameX.Procedure.Editor
{
    [Tool]
    public partial class GameFrameXProcedurePlugin : EditorPlugin
    {
        private ProcedureComponentInspectorPlugin m_InspectorPlugin;

        /// <summary>
        /// 插件进入编辑器树时注册流程检查器插件。
        /// </summary>
        public override void _EnterTree()
        {
            m_InspectorPlugin = new ProcedureComponentInspectorPlugin();
            AddInspectorPlugin(m_InspectorPlugin);
        }

        /// <summary>
        /// 插件退出编辑器树时移除并清理流程检查器插件。
        /// </summary>
        public override void _ExitTree()
        {
            if (m_InspectorPlugin != null)
            {
                RemoveInspectorPlugin(m_InspectorPlugin);
            }
            m_InspectorPlugin = null;
        }
    }
}
#endif
