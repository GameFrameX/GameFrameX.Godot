/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace UICommon
{
    public partial class UIDialogMessageBox : GComponent
    {
        public GButton m_enter_button;
        public GButton m_cancel_button;
        public GRichTextField m_content;
        public const string URL = "ui://ats3vms3iopl2l";

        public static UIDialogMessageBox CreateInstance()
        {
            return (UIDialogMessageBox)UIPackage.CreateObject("UICommon", "UIDialogMessageBox");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_enter_button = (GButton)GetChild("enter_button");
            m_cancel_button = (GButton)GetChild("cancel_button");
            m_content = (GRichTextField)GetChild("content");
        }
    }
}