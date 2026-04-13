/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace UICommon
{
    public partial class UIGoodItem : GButton
    {
        public GLoader m_bg;
        public GLoader m_gift;
        public GTextField m_number;
        public const string URL = "ui://ats3vms372ce2u";

        public static UIGoodItem CreateInstance()
        {
            return (UIGoodItem)UIPackage.CreateObject("UICommon", "UIGoodItem");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_bg = (GLoader)GetChild("bg");
            m_gift = (GLoader)GetChild("gift");
            m_number = (GTextField)GetChild("number");
        }
    }
}