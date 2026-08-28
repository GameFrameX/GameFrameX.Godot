#if TOOLS
using Godot;

namespace GameFrameX.Sound.Editor
{
    [Tool]
    public partial class GameFrameXSoundPlugin : EditorPlugin
    {
        // 说明：Unity 版 SoundComponentInspector 不迁移；插件占位保持包结构一致。
        public override void _EnterTree()
        {
        }

        public override void _ExitTree()
        {
        }
    }
}
#endif
