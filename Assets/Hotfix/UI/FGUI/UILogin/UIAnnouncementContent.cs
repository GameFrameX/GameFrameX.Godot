/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace UILogin
{
    public partial class UIAnnouncementContent : GComponent
    {
        public GRichTextField m_LabelContent;
        public const string URL = "ui://f011l0h9aneks9i";

        public static UIAnnouncementContent CreateInstance()
        {
            return (UIAnnouncementContent)UIPackage.CreateObject("UILogin", "UIAnnouncementContent");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_LabelContent = (GRichTextField)GetChild("LabelContent");
        }
    }
}