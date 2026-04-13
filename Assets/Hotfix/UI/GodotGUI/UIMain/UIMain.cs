using GameFrameX.UI.GDGUI.Runtime;
using GameFrameX.UI.Runtime;
using Godot;
using GameFrameX.AssetSystem;
using GameFrameX.Runtime;

namespace Godot.Hotfix.GodotGUI
{
	[OptionUIGroup(UIGroupNameConstants.Normal)]
	public partial class UIMain : GDGUI
	{
		private const string MainPackageName = "main";
		//private const string BuiltinLogoPath = "res://addons/com.gameframex.godot/Resources/gameframex_logo.png";
		private const string BuiltinLogoPath = "res://Assets/Probe/teamgame_external.png";
		private const string LogoAssetName = "teamgame_external";

		private Label _playerNameLabel;
		private Label _playerLevelLabel;
		private TextureRect _centerLogoTextureRect;

		public override void OnOpen(object userData)
		{
			base.OnOpen(userData);
			EnsureNodes();
			ApplyMainPackageLogo();
		}

		public void SetPlayerInfo(string playerName, string playerLevel)
		{
			EnsureNodes();
			if (_playerNameLabel != null)
			{
				_playerNameLabel.Text = $"Player: {playerName}";
			}

			if (_playerLevelLabel != null)
			{
				_playerLevelLabel.Text = $"Level: {playerLevel}";
			}
		}

		private void EnsureNodes()
		{
			if (_playerNameLabel == null)
			{
				_playerNameLabel = FindChild("PlayerNameLabel", true, false) as Label;
			}

			if (_playerLevelLabel == null)
			{
				_playerLevelLabel = FindChild("PlayerLevelLabel", true, false) as Label;
			}

			if (_centerLogoTextureRect == null)
			{
				_centerLogoTextureRect = ResolveCenterLogoTextureRect();
			}
		}

		private TextureRect ResolveCenterLogoTextureRect()
		{
			// root attach
			var textureRect = GetNodeOrNull<TextureRect>("CenterLogo");
			if (textureRect != null)
			{
				return textureRect;
			}

			// normal attach
			textureRect = GetNodeOrNull<TextureRect>("Center/CenterLogo");
			if (textureRect != null)
			{
				return textureRect;
			}

			// fallback attach: scene root is re-parented to ViewRoot
			textureRect = GetNodeOrNull<TextureRect>("ViewRoot/CenterLogo");
			if (textureRect != null)
			{
				return textureRect;
			}

			// fallback attach: scene root is re-parented to ViewRoot
			textureRect = GetNodeOrNull<TextureRect>("ViewRoot/Center/CenterLogo");
			if (textureRect != null)
			{
				return textureRect;
			}

			var centerNode = GetNodeOrNull<Node>("Center") ?? GetNodeOrNull<Node>("ViewRoot/Center") ?? FindChild("Center", true, false);
			if (centerNode != null)
			{
				textureRect = centerNode.GetNodeOrNull<TextureRect>("CenterLogo") ?? centerNode.FindChild("CenterLogo", true, false) as TextureRect;
			}

			return textureRect;
		}

		private void ApplyMainPackageLogo()
		{
			if (_centerLogoTextureRect == null)
			{
				Log.Warning("[UIMain] Center/CenterLogo TextureRect node not found.");
				return;
			}

			var texture = global::GameFrameX.AssetSystem.AssetSystem.TryGetPackageAsset<Texture2D>(LogoAssetName, MainPackageName);
			var loadedFromMainPackage = texture != null;
			if (texture == null)
			{
				texture = AssetSystemResources.Load<Texture2D>(BuiltinLogoPath);
			}

			if (texture != null)
			{
				_centerLogoTextureRect.Texture = texture;
				var resourcePath = loadedFromMainPackage ? texture.ResourcePath : BuiltinLogoPath;
				Log.Info("[UIMain] Logo assigned. source={0} path={1}", loadedFromMainPackage ? "main package" : "builtin fallback", resourcePath);
            }
            else
			{
				Log.Warning("[UIMain] Logo load failed. package='main' and builtin fallback both missing.");
			}
		}

	}
}
