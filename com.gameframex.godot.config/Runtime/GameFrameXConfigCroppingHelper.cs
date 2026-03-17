using Godot;

namespace GameFrameX.Config.Runtime
{
    public partial class GameFrameXConfigCroppingHelper : Node
    {
        public override void _Ready()
        {
            _ = typeof(ConfigManager);
            _ = typeof(IConfigManager);
            _ = typeof(LoadConfigFailureEventArgs);
            _ = typeof(LoadConfigSuccessEventArgs);
            _ = typeof(LoadConfigUpdateEventArgs);
            _ = typeof(IDataTable<>);
            _ = typeof(BaseDataTable<>);
            _ = typeof(ConfigComponent);
        }
    }
}