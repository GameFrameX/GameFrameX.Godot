using GameFrameX.UI.GDGUI.Runtime;
using GameFrameX.UI.Runtime;
using Godot;

namespace Godot.Hotfix.GodotGUI
{
	[OptionUIGroup(UIGroupNameConstants.Normal)]
	public partial class UIMain : GDGUI
	{
		private Label _playerNameLabel;
		private Label _playerLevelLabel;

		public override void OnOpen(object userData)
		{
			base.OnOpen(userData);
			EnsureNodes();
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
		}
	}
}
