/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace UICommon
{
    public partial class UIDialogWithoutButton : GComponent
    {
        public GButton m_close_icon;
        public const string URL = "ui://ats3vms3srah1v";

        public static UIDialogWithoutButton CreateInstance()
        {
            return (UIDialogWithoutButton)UIPackage.CreateObject("UICommon", "UIDialogWithoutButton");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_close_icon = (GButton)GetChild("close_icon");
        }
    }
}