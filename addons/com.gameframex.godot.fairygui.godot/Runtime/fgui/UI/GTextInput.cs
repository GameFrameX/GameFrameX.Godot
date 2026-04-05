using System.Collections.Generic;
using FairyGUI.Utils;
using Godot;

namespace FairyGUI
{
    /// <summary>
    /// 
    /// </summary>
    public class GTextInput : GTextField
    {
        public InputTextField inputTextField { get; private set; }
        NClipContainer _container;

        EventListener _onChanged;
        EventListener _onSubmit;

        public GTextInput()
        {
            _textField.autoSize = AutoSizeType.None;
            _textField.wordWrap = false;
            cursor = DisplayServer.CursorShape.Ibeam;
            touchable = true;
            focusable = true;
            tabStop = true;
        }

        public EventListener onChanged
        {
            get { return _onChanged ?? (_onChanged = new EventListener(this, "onChanged")); }
        }

        public EventListener onSubmit
        {
            get { return _onSubmit ?? (_onSubmit = new EventListener(this, "onSubmit")); }
        }

        public bool editable
        {
            get { return inputTextField.editable; }
            set { inputTextField.editable = value; }
        }

        public bool hideInput
        {
            get { return inputTextField.hideInput; }
            set { inputTextField.hideInput = value; }
        }

        public int maxLength
        {
            get { return inputTextField.maxLength; }
            set { inputTextField.maxLength = value; }
        }

        public string restrict
        {
            get { return inputTextField.restrict; }
            set { inputTextField.restrict = value; }
        }

        public bool displayAsPassword
        {
            get { return inputTextField.displayAsPassword; }
            set { inputTextField.displayAsPassword = value; }
        }

        public int caretPosition
        {
            get { return inputTextField.caretPosition; }
            set { inputTextField.caretPosition = value; }
        }

        public string promptText
        {
            get { return inputTextField.promptText; }
            set { inputTextField.promptText = value; }
        }

        /// <summary>
        /// 在移动设备上是否使用键盘输入。如果false，则文本在获得焦点后不会弹出键盘。
        /// </summary>
        public bool keyboardInput
        {
            get { return inputTextField.virtualKeyboardInput; }
            set { inputTextField.virtualKeyboardInput = value; }
        }

        public DisplayServer.VirtualKeyboardType keyboardType
        {
            get { return inputTextField.keyboardType; }
            set { inputTextField.keyboardType = value; }
        }

        public bool disableIME
        {
            get { return inputTextField.disableIME; }
            set { inputTextField.disableIME = value; }
        }

        public Dictionary<uint, Emoji> emojies
        {
            get { return inputTextField.emojies; }
            set { inputTextField.emojies = value; }
        }

        public int border
        {
            get { return inputTextField.border; }
            set { inputTextField.border = value; }
        }

        public int corner
        {
            get { return inputTextField.corner; }
            set { inputTextField.corner = value; }
        }

        public Color borderColor
        {
            get { return inputTextField.borderColor; }
            set { inputTextField.borderColor = value; }
        }

        public Color backgroundColor
        {
            get { return inputTextField.backgroundColor; }
            set { inputTextField.backgroundColor = value; }
        }

        public bool mouseWheelEnabled
        {
            get { return inputTextField.mouseWheelEnabled; }
            set { inputTextField.mouseWheelEnabled = value; }
        }

        public void SetSelection(int start, int length)
        {
            inputTextField.SetSelection(start, length);
        }

        public void ReplaceSelection(string value)
        {
            inputTextField.ReplaceSelection(value);
        }

        override protected void SetTextFieldText()
        {
            inputTextField.text = _text;
        }

        override protected void CreateDisplayObject()
        {            
            inputTextField = new InputTextField(this);
            _container = new NClipContainer(this);
            displayObject = _container;
            _textField = inputTextField;
            _container.AddChild(inputTextField);
        }

        public override void UpdateSize(float X, float Y)
        {
            base.UpdateSize(X, Y);
            _container.SetSize(X, Y);
        }

        public override void Setup_BeforeAdd(ByteBuffer buffer, int beginPos)
        {
            base.Setup_BeforeAdd(buffer, beginPos);

            buffer.Seek(beginPos, 4);

            string str = buffer.ReadS();
            if (str != null)
                inputTextField.promptText = GTextField.TranslaterStr(str);

            str = buffer.ReadS();
            if (str != null)
                inputTextField.restrict = str;

            int iv = buffer.ReadInt();
            if (iv != 0)
                inputTextField.maxLength = iv;
            iv = buffer.ReadInt();
            if (iv != 0)
                inputTextField.keyboardType = InputTextField.TranslateKeyboardType((FairyGUIVirtualKeyboardType)iv);
            if (buffer.ReadBool())
                inputTextField.displayAsPassword = true;
        }
    }
}