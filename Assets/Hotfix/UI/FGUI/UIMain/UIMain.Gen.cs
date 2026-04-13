/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace UIMain
{
    public partial class UIMain : GComponent
    {
        public GLoader m_bg;
        public GButton m_bag_button;
        public GLoader m_player_icon;
        public GTextField m_player_name;
        public GTextField m_player_level;
        public GLoader m_logo;
        public const string URL = "ui://q9u97yzfxws70";

        public static UIMain CreateInstance()
        {
            return (UIMain)UIPackage.CreateObject("UIMain", "UIMain");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_bg = (GLoader)GetChild("bg");
            m_bag_button = (GButton)GetChild("bag_button");
            m_player_icon = (GLoader)GetChild("player_icon");
            m_player_name = (GTextField)GetChild("player_name");
            m_player_level = (GTextField)GetChild("player_level");
            m_logo = (GLoader)GetChild("logo");
        }
    }
}