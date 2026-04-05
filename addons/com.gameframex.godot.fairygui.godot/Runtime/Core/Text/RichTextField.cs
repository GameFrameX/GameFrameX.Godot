using System.Collections.Generic;
using FairyGUI.Utils;
using Godot;

namespace FairyGUI
{
    /// <summary>
    /// 
    /// </summary>
    public partial class RichTextField : TextField
    {
        /// <summary>
        /// 
        /// </summary>
        public IHtmlPageContext htmlPageContext { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public HtmlParseOptions htmlParseOptions { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public Dictionary<uint, Emoji> emojies { get; set; }

        public RichTextField(GObject owner)
            : base(owner)
        {
            Name = "RichTextField";

            htmlPageContext = HtmlPageContext.inst;
            htmlParseOptions = new HtmlParseOptions();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public HtmlElement GetHtmlElement(string name)
        {
            List<HtmlElement> elements = htmlElements;
            int count = elements.Count;
            for (int i = 0; i < count; i++)
            {
                HtmlElement element = elements[i];
                if (name.Equals(element.name, System.StringComparison.OrdinalIgnoreCase))
                    return element;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public HtmlElement GetHtmlElementAt(int index)
        {
            return htmlElements[index];
        }

        /// <summary>
        /// 
        /// </summary>
        public int htmlElementCount
        {
            get { return htmlElements.Count; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="show"></param>
        public void ShowHtmlObject(int index, bool show)
        {
            HtmlElement element = htmlElements[index];
            if (element.htmlObject != null && element.type != HtmlElementType.Link)
            {
                //set hidden flag
                if (show)
                    element.status &= 253; //~(1<<1)
                else
                    element.status |= 2;

                if ((element.status & 3) == 0) //not (hidden and clipped)
                {
                    if ((element.status & 4) == 0) //not added
                    {
                        element.status |= 4;
                        element.htmlObject.Add();
                    }
                }
                else
                {
                    if ((element.status & 4) != 0) //added
                    {
                        element.status &= 251;
                        element.htmlObject.Remove();
                    }
                }
            }
        }


        public override void Dispose()
        {
            CleanupObjects();
            base.Dispose();
        }

        internal void CleanupObjects()
        {
            int count = htmlElements.Count;
            for (int i = 0; i < count; i++)
            {
                HtmlElement element = htmlElements[i];
                if (element.htmlObject != null)
                {
                    element.htmlObject.Remove();
                    htmlPageContext.FreeObject(element.htmlObject);
                }
            }
            htmlElements.Clear();
        }

        virtual internal void RefreshObjects()
        {
            int count = htmlElements.Count;
            for (int i = 0; i < count; i++)
            {
                HtmlElement element = htmlElements[i];
                if (element.htmlObject != null)
                {
                    if ((element.status & 3) == 0) //not (hidden and clipped)
                    {
                        if ((element.status & 4) == 0) //not added
                        {
                            element.status |= 4;
                            element.htmlObject.Add();
                        }
                    }
                    else
                    {
                        if ((element.status & 4) != 0) //added
                        {
                            element.status &= 251;
                            element.htmlObject.Remove();
                        }
                    }
                }
            }
        }
    }
}
