using Godot;

namespace GameFrameX.Procedure.Runtime
{
    public partial class GameFrameXProcedureCroppingHelper : Node
    {
        public override void _Ready()
        {
            _ = typeof(IProcedureManager);
            _ = typeof(ProcedureBase);
            _ = typeof(ProcedureManager);
            _ = typeof(ProcedureComponent);
        }
    }
}