using GameFrameX.UI.GDGUI.Runtime;
using GameFrameX.UI.Runtime;
using Godot;

namespace Godot.Startup.Demo.GodotUI
{
	[OptionUIGroup(UIGroupNameConstants.Normal)]
	public partial class UILauncher : GDGUI
	{
		private ProgressBar _progressBar;

		public override void OnOpen(object userData)
		{
			base.OnOpen(userData);
			EnsureNodes();
			SetProgress(0f);
		}

		public void SetProgress(float value)
		{
			EnsureNodes();
			if (_progressBar == null)
			{
				return;
			}

			_progressBar.MinValue = 0;
			_progressBar.MaxValue = 100;
			_progressBar.Value = Mathf.Clamp(value, 0f, 100f);
		}

		private void EnsureNodes()
		{
			if (_progressBar != null)
			{
				return;
			}

			_progressBar = FindChild("ProgressBar", true, false) as ProgressBar;
		}
	}
}
