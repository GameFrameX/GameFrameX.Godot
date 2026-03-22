using Godot;

namespace GameFrameX.Timer.Runtime
{
    public partial class GameFrameXTimerCroppingHelper : Node
    {
        public override void _Ready()
        {
            _ = typeof(TimerComponent);
            _ = typeof(ITimerManager);
            _ = typeof(TimerManager);
        }
    }
}
