/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace UILauncher
{
	public partial class UILauncher : GComponent
	{
		public Controller m_IsUpgrade;
		public Controller m_IsDownload;
		public GLoader m_bg;
		public GTextField m_TipText;
		public GProgressBar m_ProgressBar;
		public UILauncherUpgrade m_upgrade;
		public GTextField m_txtVersion;
		public const string URL = "ui://u7deosq0mw8e0";

		public static UILauncher CreateInstance()
		{
			return (UILauncher)UIPackage.CreateObject("UILauncher", "UILauncher");
		}

		public override void ConstructFromXML(XML xml)
		{
			base.ConstructFromXML(xml);

			m_IsUpgrade = GetController("IsUpgrade");
			m_IsDownload = GetController("IsDownload");
			m_bg = (GLoader)GetChild("bg");
			m_TipText = (GTextField)GetChild("TipText");
			m_ProgressBar = (GProgressBar)GetChild("ProgressBar");
			m_upgrade = (UILauncherUpgrade)GetChild("upgrade");
			m_txtVersion = (GTextField)GetChild("txtVersion");
		}
	}
}
