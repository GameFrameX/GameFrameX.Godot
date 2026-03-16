using Godot;

namespace GameFrameX.Fsm.Runtime
{
    public partial class GameFrameXFsmCroppingHelper : Node
    {
        public override void _Ready()
        {
            _ = typeof(IFsmManager);
            _ = typeof(IFsm<>);
            _ = typeof(FsmState<>);
            _ = typeof(FsmBase);
            _ = typeof(FsmManager);
            _ = typeof(FsmComponent);
        }
    }
}
