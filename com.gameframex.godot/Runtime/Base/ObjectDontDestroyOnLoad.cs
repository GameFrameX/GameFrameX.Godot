using Godot;

namespace GameFrameX.Runtime
{
    /// <summary>
    /// 标记物体对象为不可销毁
    /// In Godot, use AutoLoad singletons instead of DontDestroyOnLoad pattern
    /// </summary>
    public sealed class ObjectDontDestroyOnLoad : Node
    {
        public override void _Ready()
        {
            base._Ready();
            // In Godot, use AutoLoad singletons instead of DontDestroyOnLoad
            // This class is kept for compatibility but the pattern is different in Godot
            // For Godot, register singletons in project settings under AutoLoad
        }
    }
}