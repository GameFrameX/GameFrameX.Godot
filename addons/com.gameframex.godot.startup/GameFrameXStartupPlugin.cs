#if TOOLS
using Godot;

namespace GameFrameX.Startup.Editor
{
    /// <summary>
    /// GameFrameX 启动流程模块插件入口。
    /// </summary>
    [Tool]
    public partial class GameFrameXStartupPlugin : EditorPlugin
    {
        // 说明：Unity 版的 StartupOptionsInspector 编辑器扩展不迁移（决策项），
        /// Godot 侧 StartupOptions 以 [GlobalClass] + [Export] 暴露给 Inspector，无需自定义 Inspector。
        public override void _EnterTree()
        {
        }

        public override void _ExitTree()
        {
        }
    }
}
#endif
