using FairyGUI;
using GameFrameX.Runtime;
using GameFrameX.AssetSystem;
using Godot;

namespace Godot.Hotfix.FairyGUI
{
    public partial class UIMain
    {
        /// <summary>头像所在 FairyGUI 包名（与 Unity 侧 FUIPackage.UICommonAvatar 对应）。</summary>
        private const string AvatarPackageName = "UICommonAvatar";
        private const string FallbackLogoPath = "res://icon.svg";
        private const int LogoMaxSize = 512;
        private const string MainPackageName = "main";
        private const string LogoAssetName = "teamgame_external";

        private GComponent _view;
        private GTextField _playerName;
        private GTextField _playerLevel;
        private GLoader _playerIcon;
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
            _playerIcon = _view.GetChild("player_icon")?.asLoader;
            _logo = _view.GetChild("logo")?.asLoader;
            ApplyLogo();
        }

        public override void OnClose(bool isShutdown, object userData)
        {
            _playerName = null;
            _playerLevel = null;
            _playerIcon = null;
            _logo = null;
            FairyGuiRuntimeBridge.DisposeView(ref _view);
            base.OnClose(isShutdown, userData);
        }


        /// <summary>
        /// 设置玩家信息。头像从 UICommonAvatar 包按精灵区域加载（对齐 Unity 侧
        /// m_player_icon.icon = UIPackage.GetItemURL(FUIPackage.UICommonAvatar, avatar)）。
        /// </summary>
        /// <param name="playerName">玩家名。</param>
        /// <param name="playerLevel">等级文本（Unity 侧格式为 “当前等级:1”）。</param>
        /// <param name="avatar">头像 ID（UICommonAvatar 包内精灵名）。</param>
        public void SetPlayerInfo(string playerName, string playerLevel, string avatar)
        {
            if (_playerName != null)
            {
                _playerName.text = playerName;
            }

            if (_playerLevel != null)
            {
                _playerLevel.text = playerLevel;
            }

            if (_playerIcon != null)
            {
                var iconUrl = UIPackage.GetItemURL(AvatarPackageName, avatar);
                _playerIcon.icon = iconUrl;
                if (string.IsNullOrEmpty(iconUrl))
                {
                    Log.Warning("[FGUI.UIMain] avatar not found. package={0} avatar={1}", AvatarPackageName, avatar);
                }
            }
        }

        private void ApplyLogo()
        {
            if (_logo == null)
            {
                return;
            }

            // 资源包可能给出外链图（如运行时下载的团队标识）。若拿到的是截图级大图
            // （例如 runtime_verify 测试包里的 2560x1920 探针截图），整张塞进 logo 框
            // 会表现为“渲染整图集”，此时回退到单图资源，避免异常表现。
            var texture = global::GameFrameX.AssetSystem.AssetSystem.TryGetPackageAsset<Texture2D>(LogoAssetName, MainPackageName);
            if (texture == null || texture.GetWidth() > LogoMaxSize || texture.GetHeight() > LogoMaxSize)
            {
                texture = ResourceLoader.Load<Texture2D>(FallbackLogoPath);
            }

            if (texture == null)
            {
                _logo.texture = null;
                Log.Error("[FGUI.UIMain] logo load failed. fallback={0}", FallbackLogoPath);
                return;
            }

            _logo.texture = new NTexture(texture);
            Log.Info("[FGUI.UIMain] logo assigned. source={0}", texture.ResourcePath);
        }
    }
}
