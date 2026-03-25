using Godot;

namespace GameFrameX.Web.Runtime
{
    public partial class GameFrameXWebCroppingHelper : Node
    {
        public override void _Ready()
        {
            _ = typeof(WebComponent);
            _ = typeof(IWebManager);
            _ = typeof(WebManager);
            _ = typeof(WebStringResult);
            _ = typeof(WebBufferResult);
        }
    }
}
