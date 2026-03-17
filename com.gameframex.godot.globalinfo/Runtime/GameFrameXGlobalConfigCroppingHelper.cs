using Godot;

namespace GameFrameX.GlobalConfig.Runtime
{
    public partial class GameFrameXGlobalConfigCroppingHelper : Node
    {
        public override void _Ready()
        {
            _ = typeof(GlobalConfigComponent);
            _ = typeof(ResponseGameAppVersion);
            _ = typeof(ResponseGlobalInfo);
            _ = typeof(ResponseGameAssetPackageVersion);
        }
    }
}