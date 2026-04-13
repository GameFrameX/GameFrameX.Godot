/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace UILogin
{
    public partial class UIPlayerListItem : GComponent
    {
        public GLoader m_icon;
        public GRichTextField m_name_text;
        public GRichTextField m_level_text;
        public UIPlayerListItemLoginButton m_login_button;
        public const string URL = "ui://f011l0h9i3dbs9n";

        public static UIPlayerListItem CreateInstance()
        {
            return (UIPlayerListItem)UIPackage.CreateObject("UILogin", "UIPlayerListItem");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_icon = (GLoader)GetChild("icon");
            m_name_text = (GRichTextField)GetChild("name_text");
            m_level_text = (GRichTextField)GetChild("level_text");
            m_login_button = (UIPlayerListItemLoginButton)GetChild("login_button");
        }
    }
}