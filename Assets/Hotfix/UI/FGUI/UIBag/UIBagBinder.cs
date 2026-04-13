/** This is an automatically generated class by FairyGUI. Please do not modify it. **/

using FairyGUI;

namespace UIBag
{
    public class UIBagBinder
    {
        public static void BindAll()
        {
            UIObjectFactory.SetPackageItemExtension(UIBag.URL, typeof(UIBag));
            UIObjectFactory.SetPackageItemExtension(UIBagContent.URL, typeof(UIBagContent));
            UIObjectFactory.SetPackageItemExtension(UIBagItemInfo.URL, typeof(UIBagItemInfo));
            UIObjectFactory.SetPackageItemExtension(UIBagTypeItem.URL, typeof(UIBagTypeItem));
            UIObjectFactory.SetPackageItemExtension(UIBagItem.URL, typeof(UIBagItem));
        }
    }
}