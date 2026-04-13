/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace UILauncher
{
    public partial class UILauncherUpgrade : GComponent
    {
        public GGraph m_bg;
        public GButton m_EnterButton;
        public GLabel m_TextContent;
        public const string URL = "ui://u7deosq0qew11e";

        public static UILauncherUpgrade CreateInstance()
        {
            return (UILauncherUpgrade)UIPackage.CreateObject("UILauncher", "UILauncherUpgrade");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_bg = (GGraph)GetChild("bg");
            m_EnterButton = (GButton)GetChild("EnterButton");
            m_TextContent = (GLabel)GetChild("TextContent");
        }
    }
}