using FairyGUI;

namespace Godot.Hotfix.FairyGUI
{
    public partial class UIMain
    {
        private GComponent _view;
        private GTextField _playerName;
        private GTextField _playerLevel;

        public override void OnOpen(object userData)
        {
            base.OnOpen(userData);

            FairyGuiRuntimeBridge.DisposeView(ref _view);
            _view = FairyGuiRuntimeBridge.CreateFullScreenView("UIMain", "UIMain", UIGroup?.Name);
            if (_view == null)
            {
                return;
            }

            _playerName = _view.GetChild("player_name")?.asTextField;
            _playerLevel = _view.GetChild("player_level")?.asTextField;
        }

        public override void OnClose(bool isShutdown, object userData)
        {
            _playerName = null;
            _playerLevel = null;
            FairyGuiRuntimeBridge.DisposeView(ref _view);
            base.OnClose(isShutdown, userData);
        }

        public void SetPlayerInfo(string playerName, string playerLevel)
        {
            if (_playerName != null)
            {
                _playerName.text = playerName;
            }

            if (_playerLevel != null)
            {
                _playerLevel.text = playerLevel;
            }
        }
    }
}
