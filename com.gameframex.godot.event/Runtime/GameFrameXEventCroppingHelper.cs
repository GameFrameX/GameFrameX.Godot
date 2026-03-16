using Godot;

namespace GameFrameX.Event.Runtime
{
    public partial class GameFrameXEventCroppingHelper : Node
    {
        public override void _Ready()
        {
            _ = typeof(EventManager);
            _ = typeof(EventComponent);
            _ = typeof(GameEventArgs);
            _ = typeof(EmptyEventArgs);
            _ = typeof(IEventManager);
        }
    }
}
