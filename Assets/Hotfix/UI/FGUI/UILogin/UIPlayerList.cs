/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace UILogin
{
    public partial class UIPlayerList : GComponent
    {
        public Controller m_IsSelected;
        public GList m_player_list;
        public GLoader m_selected_icon;
        public GRichTextField m_selected_name;
        public GRichTextField m_selected_level;
        public GButton m_login_button;
        public const string URL = "ui://f011l0h9i3dbs9m";

        public static UIPlayerList CreateInstance()
        {
            return (UIPlayerList)UIPackage.CreateObject("UILogin", "UIPlayerList");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_IsSelected = GetController("IsSelected");
            m_player_list = (GList)GetChild("player_list");
            m_selected_icon = (GLoader)GetChild("selected_icon");
            m_selected_name = (GRichTextField)GetChild("selected_name");
            m_selected_level = (GRichTextField)GetChild("selected_level");
            m_login_button = (GButton)GetChild("login_button");
        }
    }
}