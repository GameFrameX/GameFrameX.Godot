/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace UILogin
{
    public partial class UIAnnouncement : GComponent
    {
        public GGraph m_MaskLayer;
        public UIAnnouncementContent m_TextContent;
        public GTextField m_TextTitle;
        public const string URL = "ui://f011l0h9aneks9g";

        public static UIAnnouncement CreateInstance()
        {
            return (UIAnnouncement)UIPackage.CreateObject("UILogin", "UIAnnouncement");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_MaskLayer = (GGraph)GetChild("MaskLayer");
            m_TextContent = (UIAnnouncementContent)GetChild("TextContent");
            m_TextTitle = (GTextField)GetChild("TextTitle");
        }
    }
}