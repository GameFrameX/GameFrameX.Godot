/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace UILogin
{
    public partial class UIPlayerCreate : GComponent
    {
        public GTextInput m_UserName;
        public GButton m_enter;
        public GTextField m_ErrorText;
        public const string URL = "ui://f011l0h9i3dbs9p";

        public static UIPlayerCreate CreateInstance()
        {
            return (UIPlayerCreate)UIPackage.CreateObject("UILogin", "UIPlayerCreate");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_UserName = (GTextInput)GetChild("UserName");
            m_enter = (GButton)GetChild("enter");
            m_ErrorText = (GTextField)GetChild("ErrorText");
        }
    }
}