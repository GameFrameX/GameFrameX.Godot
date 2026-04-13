/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace UIBag
{
    public partial class UIBagTypeItem : GButton
    {
        public GImage m_normal;
        public GImage m_select;
        public const string URL = "ui://a3awyna7l50q4";

        public static UIBagTypeItem CreateInstance()
        {
            return (UIBagTypeItem)UIPackage.CreateObject("UIBag", "UIBagTypeItem");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_normal = (GImage)GetChild("normal");
            m_select = (GImage)GetChild("select");
        }
    }
}