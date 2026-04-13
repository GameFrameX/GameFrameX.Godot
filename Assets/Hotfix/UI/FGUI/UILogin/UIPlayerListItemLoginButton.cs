/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace UILogin
{
    public partial class UIPlayerListItemLoginButton : GComponent
    {
        public GRichTextField m_title;
        public const string URL = "ui://f011l0h9i3dbs9o";

        public static UIPlayerListItemLoginButton CreateInstance()
        {
            return (UIPlayerListItemLoginButton)UIPackage.CreateObject("UILogin", "UIPlayerListItemLoginButton");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_title = (GRichTextField)GetChild("title");
        }
    }
}