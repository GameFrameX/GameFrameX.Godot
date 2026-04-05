using System.Collections.Generic;
using FairyGUI.Utils;
using Godot;

namespace FairyGUI
{
    /// <summary>
    /// GRichTextField class.
    /// </summary>
    public class GRichTextField : GTextField
    {
        /// <summary>
        /// 
        /// </summary>
        public RichTextField richTextField { get; private set; }

        public GRichTextField()
            : base()
        {
        }

        override protected void CreateDisplayObject()
        {
            richTextField = new RichTextField(this);
            richTextField.gOwner = this;
            displayObject = richTextField;
            _textField = richTextField;
        }

        override protected void SetTextFieldText()
        {
            string str = TranslaterStr(_text);
            if (_templateVars != null)
                str = ParseTemplate(str);

            _textField.maxWidth = maxWidth;
            if (_ubbEnabled)
                richTextField.htmlText = UBBParser.inst.Parse(str);
            else
                richTextField.htmlText = str;
        }

        public Dictionary<uint, Emoji> emojies
        {
            get { return richTextField.emojies; }
            set { richTextField.emojies = value; }
        }
    }
}
