/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;
using FairyGUI.Utils;

namespace UIBag
{
    public partial class UIBag : GComponent
    {
        public GGraph m_bg;
        public UIBagContent m_content;
        public const string URL = "ui://a3awyna7l50q1";

        public static UIBag CreateInstance()
        {
            return (UIBag)UIPackage.CreateObject("UIBag", "UIBag");
        }

        public override void ConstructFromXML(XML xml)
        {
            base.ConstructFromXML(xml);

            m_bg = (GGraph)GetChild("bg");
            m_content = (UIBagContent)GetChild("content");
        }
    }
}