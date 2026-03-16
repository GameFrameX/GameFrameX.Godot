using Godot;

namespace GameFrameX.Setting.Runtime
{
    public partial class GameFrameXSettingCroppingHelper : Node
    {
        public override void _Ready()
        {
            _ = typeof(DefaultSetting);
            _ = typeof(DefaultSettingHelper);
            _ = typeof(DefaultSettingSerializer);
            _ = typeof(PlayerPrefsSettingHelper);
            _ = typeof(SettingComponent);
            _ = typeof(SettingHelperBase);
            _ = typeof(ISettingHelper);
            _ = typeof(ISettingManager);
            _ = typeof(SettingManager);
        }
    }
}
