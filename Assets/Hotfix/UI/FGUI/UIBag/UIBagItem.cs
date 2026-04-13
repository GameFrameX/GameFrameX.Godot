/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace UIBag
{
    public partial class UIBagItem : GButton
    {
        public GButton m_good_item;
        public const string URL = "ui://a3awyna7l50q5";

        public static UIBagItem CreateInstance()
        {
            return (UIBagItem)UIPackage.CreateObject("UIBag", "UIBagItem");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_good_item = (GButton)GetChild("good_item");
        }
    }
}