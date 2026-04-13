/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace UIBag
{
    public partial class UIBagItemInfo : GComponent
    {
        public Controller m_IsCanUse;
        public GTextField m_name_text;
        public GRichTextField m_desc_text;
        public GButton m_good_item;
        public GButton m_use_button;
        public GButton m_get_source_button;
        public const string URL = "ui://a3awyna7l50q3";

        public static UIBagItemInfo CreateInstance()
        {
            return (UIBagItemInfo)UIPackage.CreateObject("UIBag", "UIBagItemInfo");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_IsCanUse = GetController("IsCanUse");
            m_name_text = (GTextField)GetChild("name_text");
            m_desc_text = (GRichTextField)GetChild("desc_text");
            m_good_item = (GButton)GetChild("good_item");
            m_use_button = (GButton)GetChild("use_button");
            m_get_source_button = (GButton)GetChild("get_source_button");
        }
    }
}