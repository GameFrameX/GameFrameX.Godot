/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace UIBag
{
    public partial class UIBagContent : GComponent
    {
        public Controller m_IsSelectedItem;
        public GList m_list;
        public UIBagItemInfo m_info;
        public GList m_type_list;
        public const string URL = "ui://a3awyna7l50q2";

        public static UIBagContent CreateInstance()
        {
            return (UIBagContent)UIPackage.CreateObject("UIBag", "UIBagContent");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_IsSelectedItem = GetController("IsSelectedItem");
            m_list = (GList)GetChild("list");
            m_info = (UIBagItemInfo)GetChild("info");
            m_type_list = (GList)GetChild("type_list");
        }
    }
}