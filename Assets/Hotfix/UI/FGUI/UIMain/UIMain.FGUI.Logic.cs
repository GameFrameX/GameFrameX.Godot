using FairyGUI;
using GameFrameX.Runtime;
using GameFrameX.AssetSystem;
using Godot;

namespace Godot.Hotfix.FairyGUI
{
    public partial class UIMain
    {
        private const string MainPackageName = "main";
        private const string LogoAssetName = "teamgame_external";

        private GComponent _view;
        private GTextField _playerName;
        private GTextField _playerLevel;
        private GLoader _logo;

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
            _logo = _view.GetChild("logo")?.asLoader;
            ApplyMainPackageLogo();
        }

        public override void OnClose(bool isShutdown, object userData)
        {
            _playerName = null;
            _playerLevel = null;
            _logo = null;
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

        private void ApplyMainPackageLogo()
        {
            if (_logo == null)
            {
                Log.Error("[FGUI.UIMain] logo loader not found.");
                return;
            }

            var texture = global::GameFrameX.AssetSystem.AssetSystem.TryGetPackageAsset<Texture2D>(LogoAssetName, MainPackageName);
            if (texture == null)
            {
                _logo.texture = null;
                Log.Error("[FGUI.UIMain] logo load failed from package '{0}'. assetName={1}", MainPackageName, LogoAssetName);
                return;
            }

            _logo.texture = new NTexture(texture);
            Log.Info("[FGUI.UIMain] logo assigned. source=main package resource={0}", texture.ResourcePath);
        }
    }
}
