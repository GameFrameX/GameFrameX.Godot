using FairyGUI;
using Godot;

namespace Godot.Hotfix.FairyGUI
{
	public partial class UILauncher
	{
		private GComponent _view;
		private GProgressBar _progressBar;

		public override void OnOpen(object userData)
		{
			base.OnOpen(userData);

			FairyGuiRuntimeBridge.DisposeView(ref _view);
			_progressBar = null;

			_view = FairyGuiRuntimeBridge.CreateFullScreenView("UILauncher", "UILauncher");
			if (_view == null)
			{
				return;
			}

			var isDownload = _view.GetController("IsDownload");
			if (isDownload != null)
			{
				isDownload.selectedPage = "Yes";
			}

			_progressBar = _view.GetChild("ProgressBar")?.asProgress;
			SetProgress(0f);
		}

		public override void OnClose(bool isShutdown, object userData)
		{
			_progressBar = null;
			FairyGuiRuntimeBridge.DisposeView(ref _view);
			base.OnClose(isShutdown, userData);
		}

		public void SetProgress(float value)
		{
			if (_progressBar == null)
			{
				return;
			}

			_progressBar.max = 100;
			_progressBar.value = Mathf.Clamp(value, 0f, 100f);
		}
	}
}
