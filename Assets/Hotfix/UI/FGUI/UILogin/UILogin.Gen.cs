/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace UILogin
{
    public partial class UILogin : GComponent
    {
        public GTextField m_ErrorText;
        public GButton m_enter;
        public GTextInput m_UserName;
        public GTextInput m_Password;
        public const string URL = "ui://f011l0h9nmd0c";

        public static UILogin CreateInstance()
        {
            return (UILogin)UIPackage.CreateObject("UILogin", "UILogin");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_ErrorText = (GTextField)GetChild("ErrorText");
            m_enter = (GButton)GetChild("enter");
            m_UserName = (GTextInput)GetChild("UserName");
            m_Password = (GTextInput)GetChild("Password");
        }
    }
}