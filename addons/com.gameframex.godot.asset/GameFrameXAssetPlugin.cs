#if TOOLS
using Godot;

namespace GameFrameX.Asset.Editor
{
    /// <summary>
    /// GameFrameX 资源模块插件入口。
    /// </summary>
    /// <remarks>
    /// 说明：资源组件的 Inspector 定制后置（Godot 侧暂无对应需求），当前仅提供最小插件入口用于启用/禁用本模块。
    /// </remarks>
    [Tool]
    public partial class GameFrameXAssetPlugin : EditorPlugin
    {
        public override void _EnterTree()
        {
        }

        public override void _ExitTree()
        {
        }
    }
}
#endif
