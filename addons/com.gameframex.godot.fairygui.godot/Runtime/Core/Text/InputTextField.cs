using System;
using Godot;
using System.Text;
using FairyGUI.Utils;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Transactions;

namespace FairyGUI
{
    public enum FairyGUIVirtualKeyboardType
    {
        Default,
        Character,
        NumberAndInterpunction,
        Url,
        Number,
        Phone,
        EmailAddress

    }
    /// <summary>
    /// 接收用户输入的文本控件。因为支持直接输入表情，所以从RichTextField派生。
    /// </summary>
    public partial class InputTextField : RichTextField
    {
        /// <summary>
        ///
        /// </summary>
        public int maxLength { get; set; }

        /// <summary>
        /// 如果是true，则当文本获得焦点时，弹出键盘进行输入，如果是false则不会。
        /// 默认是使用Stage.keyboardInput的值。
        /// </summary>
        public bool virtualKeyboardInput { get; set; }

        /// <summary>
        ///
        /// </summary>
        public DisplayServer.VirtualKeyboardType keyboardType { get; set; }

        /// <summary>
        ///
        /// </summary>
        public bool hideInput { get; set; }

        /// <summary>
        ///
        /// </summary>
        public bool disableIME { get; set; }

        /// <summary>
        ///
        /// </summary>
        public bool mouseWheelEnabled { get; set; }

        /// <summary>
        ///
        /// </summary>
        public static Action<InputTextField, string> onCopy;

        /// <summary>
        ///
        /// </summary>
        public static Action<InputTextField> onPaste;

        /// <summary>
        ///
        /// </summary>
        public static PopupMenu contextMenu;
        string _restrict;
        Regex _restrictPattern;
        bool _displayAsPassword;
        string _inputText;
        string _promptText;
        string _decodedPromptText;
        string _compositionString = string.Empty;
        int _border;
        int _corner;
        Color _borderColor;
        Color _backgroundColor;
        bool _editable;

        bool _editing;
        int _caretPosition;
        int _selectionStart;
        TextHightLightInfo _selection = new TextHightLightInfo();
        TextHightLightInfo _compositionHightLight = new TextHightLightInfo();
        int _composing;
        char _highSurrogateChar;
        string _textBeforeEdit;
        bool _usingHtmlInput;

        NShape _caret;
        //SelectionShape _selectionShape;
        float _nextBlink;

        NShape _borderShape;

        const int GUTTER_X = 2;
        const int GUTTER_Y = 2;

        public InputTextField(GObject owner)
            : base(owner)
        {
            Name = "InputTextField";

            _inputText = string.Empty;
            maxLength = 0;
            _editable = true;
            _composing = 0;
            virtualKeyboardInput = Stage.virtualKeyboardInput;
            _borderColor = Colors.Black;
            _backgroundColor = Color.Color8(0, 0, 0, 0);
            mouseWheelEnabled = true;
            _charPositions = new List<CharPosition>();

            owner.onFocusIn.Add(__focusIn);
            owner.onFocusOut.Add(__focusOut);
            owner.onKeyDown.Add(__keydown);
            owner.onTouchBegin.AddCapture(__touchBegin);
            owner.onTouchMove.AddCapture(__touchMove);
            owner.onMouseWheel.Add(__mouseWheel);
            owner.onClick.Add(__click);
            owner.onRightClick.Add(__rightClick);

            Stage.inst.onUpdate += OnUpdate;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Stage.inst.onUpdate -= OnUpdate;
        }

        public override string text
        {
            get
            {
                return _inputText;
            }
            set
            {
                _inputText = value;
                ClearSelection();
                UpdateText();
            }
        }

        public override TextFormat textFormat
        {
            get
            {
                return base.textFormat;
            }
            set
            {
                base.textFormat = value;
                if (_editing)
                {
                    _caret.Y = _textFormat.size;
                    _caret.DrawRect(0, Colors.Transparent, _textFormat.color);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        public string restrict
        {
            get { return _restrict; }
            set
            {
                _restrict = value;
                if (string.IsNullOrEmpty(_restrict))
                    _restrictPattern = null;
                else
                    _restrictPattern = new Regex(value);
            }
        }

        /// <summary>
        ///
        /// </summary>
        public int caretPosition
        {
            get
            {
                Redraw();
                return _caretPosition;
            }
            set
            {
                SetSelection(value, 0);
            }
        }

        public int selectionBeginIndex
        {
            get { return _selectionStart < _caretPosition ? _selectionStart : _caretPosition; }
        }

        public int selectionEndIndex
        {
            get { return _selectionStart < _caretPosition ? _caretPosition : _selectionStart; }
        }

        /// <summary>
        ///
        /// </summary>
        public string promptText
        {
            get
            {
                return _promptText;
            }
            set
            {
                _promptText = value;
                if (!string.IsNullOrEmpty(_promptText))
                    _decodedPromptText = UBBParser.inst.Parse(XMLUtils.EncodeString(_promptText));
                else
                    _decodedPromptText = null;
                UpdateText();
            }
        }

        /// <summary>
        ///
        /// </summary>
        public bool displayAsPassword
        {
            get { return _displayAsPassword; }
            set
            {
                if (_displayAsPassword != value)
                {
                    _displayAsPassword = value;
                    UpdateText();
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        public bool editable
        {
            get { return _editable; }
            set
            {
                _editable = value;
                if (_caret != null)
                    _caret.visible = _editable;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public int border
        {
            get { return _border; }
            set
            {
                _border = value;
                UpdateShape();
            }
        }

        /// <summary>
        ///
        /// </summary>
        public int corner
        {
            get { return _corner; }
            set
            {
                _corner = value;
                UpdateShape();
            }
        }

        /// <summary>
        ///
        /// </summary>
        public Color borderColor
        {
            get { return _borderColor; }
            set
            {
                _borderColor = value;
                UpdateShape();
            }
        }

        /// <summary>
        ///
        /// </summary>
        public Color backgroundColor
        {
            get { return _backgroundColor; }
            set
            {
                _backgroundColor = value;
                UpdateShape();
            }
        }

        void UpdateShape()
        {
            if (_border > 0 || _backgroundColor.A > 0)
            {
                _borderShape = new NShape(gOwner);
                AddChild(_borderShape);
                _borderShape.SetXY(0, 0);
                _borderShape.size = _size;
                _borderShape.DrawRect(_border, _borderColor, _backgroundColor);
            }
            else
            {
                if (_borderShape != null)
                {
                    _borderShape.QueueFree();
                    _borderShape = null;
                }

            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="start"></param>
        /// <param name="length">-1 means the rest count from start</param>
        public void SetSelection(int start, int length)
        {
            if (!_editing)
                gOwner.RequestFocus();

            _selectionStart = start;
            _caretPosition = length < 0 ? int.MaxValue : (start + length);
            Redraw();
            int cnt = charPositions.Count;
            if (_caretPosition >= cnt)
                _caretPosition = cnt - 1;
            if (_selectionStart >= cnt)
                _selectionStart = cnt - 1;
            UpdateCaret();

        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        public void ReplaceSelection(string value)
        {
            if (virtualKeyboardInput && Stage.virtualKeyboardInput)
            {
                this.text = _inputText + value;
                OnChanged();
                return;
            }

            if (!_editing)
                gOwner.RequestFocus();

            Redraw();

            int t0, t1;
            if (_selectionStart != _caretPosition)
            {
                if (_selectionStart < _caretPosition)
                {
                    t0 = _selectionStart;
                    t1 = _caretPosition;
                    _caretPosition = _selectionStart;
                }
                else
                {
                    t0 = _caretPosition;
                    t1 = _selectionStart;
                    _selectionStart = _caretPosition;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(value))
                    return;

                t0 = t1 = _caretPosition;
            }

            StringBuilder buffer = new StringBuilder();
            GetPartialText(0, t0, buffer);
            if (!string.IsNullOrEmpty(value))
            {
                value = ValidateInput(value);
                buffer.Append(value);

                _caretPosition += GetTextlength(value);
            }
            GetPartialText(t1 + _composing, -1, buffer);

            string newText = buffer.ToString();
            if (maxLength > 0)
            {
                string newText2 = TruncateText(newText, maxLength);
                if (newText2.Length != newText.Length)
                    _caretPosition += (newText2.Length - newText.Length);
                newText = newText2;
            }

            this.text = newText;
            OnChanged();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        public void ReplaceText(string value)
        {
            if (value == _inputText)
                return;

            if (value == null)
                value = string.Empty;

            value = ValidateInput(value);

            if (maxLength > 0)
                value = TruncateText(value, maxLength);

            _caretPosition = value.Length;

            this.text = value;
            OnChanged();
        }

        void GetPartialText(int startIndex, int endIndex, StringBuilder buffer)
        {
            int elementCount = htmlElements.Count;
            int lastIndex = startIndex;
            string tt;
            if (_displayAsPassword)
                tt = _inputText;
            else
                tt = parsedText;
            if (endIndex < 0)
                endIndex = tt.Length;
            for (int i = 0; i < elementCount; i++)
            {
                HtmlElement element = htmlElements[i];
                if (element.htmlObject != null && element.text != null)
                {
                    if (element.charIndex >= startIndex && element.charIndex < endIndex)
                    {
                        buffer.Append(tt.Substring(lastIndex, element.charIndex - lastIndex));
                        buffer.Append(element.text);
                        lastIndex = element.charIndex + 1;
                    }
                }
            }
            if (lastIndex < tt.Length)
                buffer.Append(tt.Substring(lastIndex, endIndex - lastIndex));
        }

        int GetTextlength(string value)
        {
            int textLen = value.Length;
            int ret = textLen;
            for (int i = 0; i < textLen; i++)
            {
                if (char.IsHighSurrogate(value[i]))
                    ret--;
            }
            return ret;
        }

        string TruncateText(string value, int length)
        {
            int textLen = value.Length;
            int len = 0;
            int i = 0;
            while (i < textLen)
            {
                if (len == length)
                    return value.Substring(0, i);

                if (char.IsHighSurrogate(value[i]))
                    i++;
                i++;
                len++;
            }
            return value;
        }

        string ValidateInput(string source)
        {
            if (_restrict != null)
            {
                StringBuilder sb = new StringBuilder();
                Match mc = _restrictPattern.Match(source);
                int lastPos = 0;
                string s;
                while (mc != Match.Empty)
                {
                    if (mc.Index != lastPos)
                    {
                        //保留tab和回车
                        for (int i = lastPos; i < mc.Index; i++)
                        {
                            if (source[i] == '\n' || source[i] == '\t')
                                sb.Append(source[i]);
                        }
                    }

                    s = mc.ToString();
                    lastPos = mc.Index + s.Length;
                    sb.Append(s);

                    mc = mc.NextMatch();
                }
                for (int i = lastPos; i < source.Length; i++)
                {
                    if (source[i] == '\n' || source[i] == '\t')
                        sb.Append(source[i]);
                }

                return sb.ToString();
            }
            else
                return source;
        }

        void UpdateText()
        {
            if (!_editing && _inputText.Length == 0 && !string.IsNullOrEmpty(_decodedPromptText))
            {
                htmlText = _decodedPromptText;
                return;
            }

            if (_displayAsPassword)
                base.text = EncodePasswordText(_inputText);
            else
                base.text = _inputText;

            _composing = compositionString.Length;
            if (_composing > 0)
            {
                StringBuilder buffer = new StringBuilder();
                GetPartialText(0, _caretPosition, buffer);
                buffer.Append(compositionString);
                GetPartialText(_caretPosition, -1, buffer);

                base.text = buffer.ToString();
            }
        }

        string EncodePasswordText(string value)
        {
            int textLen = value.Length;
            StringBuilder tmp = new StringBuilder(textLen);
            int i = 0;
            while (i < textLen)
            {
                char c = value[i];
                if (c == '\n')
                    tmp.Append(c);
                else
                {
                    if (char.IsHighSurrogate(c))
                        i++;
                    tmp.Append("*");
                }
                i++;
            }
            return tmp.ToString();
        }

        void ClearSelection()
        {
            if (_selectionStart != _caretPosition)
            {
                RemoveTextHightLight(_selection);
                _selectionStart = _caretPosition;
            }
        }

        public string GetSelection()
        {
            if (_selectionStart == _caretPosition)
                return string.Empty;

            StringBuilder buffer = new StringBuilder();
            if (_selectionStart < _caretPosition)
                GetPartialText(_selectionStart, _caretPosition, buffer);
            else
                GetPartialText(_caretPosition, _selectionStart, buffer);
            return buffer.ToString();
        }

        void Scroll(int hScroll, int vScroll)
        {
            vScroll = Mathf.Clamp(vScroll, 0, lines.Count - 1);
            TextField.LineInfo line = lines[vScroll];
            hScroll = Mathf.Clamp(hScroll, 0, line.charCount - 1);

            TextField.CharPosition cp = GetCharPosition(line.charIndex + hScroll);
            Vector2 pt = GetCharLocation(cp);
            MoveContent(new Vector2(GUTTER_X - pt.X, GUTTER_Y - pt.Y), false);
        }

        void AdjustCaret(TextField.CharPosition cp, bool moveSelectionHeader = false)
        {
            _caretPosition = cp.charIndex;
            if (moveSelectionHeader)
                _selectionStart = _caretPosition;

            UpdateCaret();
        }

        void UpdateCaret(bool forceUpdate = false)
        {
            TextField.CharPosition cp;
            if (_editing)
                cp = GetCharPosition(_caretPosition + compositionString.Length);
            else
                cp = GetCharPosition(_caretPosition);

            Vector2 pos = GetCharLocation(cp);
            TextField.LineInfo line = lines[cp.lineIndex];
            Vector2 offset = pos + _textDrawOffset;

            if (offset.X < textFormat.size)
                offset.X += Mathf.Min(50, _size.X * 0.5f);
            else if (offset.X > _size.X - GUTTER_X - textFormat.size)
                offset.X -= Mathf.Min(50, _size.X * 0.5f);

            if (offset.X < GUTTER_X)
                offset.X = GUTTER_X;
            else if (offset.X > _size.X - GUTTER_X)
                offset.X = Mathf.Max(GUTTER_X, _size.X - GUTTER_X);

            if (offset.Y < GUTTER_Y)
                offset.Y = GUTTER_Y;
            else if (offset.Y + line.height >= _size.Y - GUTTER_Y)
                offset.Y = Mathf.Max(GUTTER_Y, _size.Y - line.height - GUTTER_Y);

            MoveContent(offset - pos, forceUpdate);

            if (_editing)
            {
                _caret.SetPosition(_textDrawOffset + pos);
                _caret.height = line.height > 0 ? line.height : textFormat.size;

                if (_editable)
                {
                    Vector2 cursorPos = _caret.GetGlobalTransform() * (new Vector2(0, _caret.height));
                    DisplayServer.WindowSetImePosition(new Vector2I((int)cursorPos.X, (int)cursorPos.Y));
                }

                _nextBlink = Time.GetTicksMsec() / 1000.0f + 0.5f;
                _caret.visible = true;

                UpdateSelection(cp.charIndex);
            }
        }

        void MoveContent(Vector2 pos, bool forceUpdate)
        {
            float ox = _textDrawOffset.X;
            float oy = _textDrawOffset.Y;
            float nx = pos.X;
            float ny = pos.Y;
            float rectWidth = width - 1; //-1 to avoid cursor be clipped
            if (rectWidth - nx > textWidth)
                nx = rectWidth - textWidth;
            if (height - ny > textHeight)
                ny = height - textHeight;
            if (nx > 0)
                nx = 0;
            if (ny > 0)
                ny = 0;
            nx = (int)nx;
            ny = (int)ny;

            if (nx != ox || ny != oy || forceUpdate)
            {
                if (_caret != null)
                {
                    _caret.SetXY(nx + _caret.Position.X - ox, ny + _caret.Position.Y - oy);
                    //_selectionShape.Position = new Vector2(nx, ny);
                }
                _textDrawOffset.X = nx;
                _textDrawOffset.Y = ny;
                QueueRedraw();

                List<HtmlElement> elements = htmlElements;
                int count = elements.Count;
                for (int i = 0; i < count; i++)
                {
                    HtmlElement element = elements[i];
                    if (element.htmlObject != null)
                        element.htmlObject.SetPosition(element.position.X + nx, element.position.Y + ny);
                }
            }
        }

        void UpdateSelection(int cp)
        {
            if (_selectionStart == _caretPosition)
            {
                RemoveTextHightLight(_selection);
                return;
            }

            int start;
            if (_editing && compositionString.Length > 0)
            {
                if (_selectionStart < _caretPosition)
                {
                    cp = _caretPosition;
                    start = _selectionStart;
                }
                else
                    start = _selectionStart + compositionString.Length;
            }
            else
                start = _selectionStart;
            if (start > cp)
            {
                int tmp = start;
                start = cp;
                cp = tmp;
            }
            _selection.startCharIndex = start;
            _selection.endCharIndex = cp;
            _selection.clipped = false;
            _selection.color = UIConfig.inputHighlightColor;
            AddTextHightLight(_selection);
        }



        override internal void RefreshObjects()
        {
            base.RefreshObjects();

            if (_editing)
            {
                //MoveChild(_selectionShape, 0);
                _caret.MoveToFront();
            }

            int cnt = charPositions.Count;
            if (_caretPosition >= cnt)
                _caretPosition = cnt - 1;
            if (_selectionStart >= cnt)
                _selectionStart = cnt - 1;

            UpdateCaret(true);
        }

        protected void OnChanged()
        {
            gOwner.DispatchEvent("onChanged", null);

            TextInputHistory.inst.MarkChanged(this);
        }

        // protected override void OnSizeChanged()
        // {
        //     base.OnSizeChanged();

        //     Rect rect = _contentRect;
        //     rect.X += GUTTER_X;
        //     rect.Y += GUTTER_Y;
        //     rect.width -= GUTTER_X * 2;
        //     rect.height -= GUTTER_Y * 2;
        //     this.clipRect = rect;
        //     ((RectHitTest)this.hitArea).rect = _contentRect;
        // }

        void OnUpdate(double delta)
        {
            if (_editing)
            {
                if (_nextBlink < Time.GetTicksMsec() / 1000.0f)
                {
                    _nextBlink = Time.GetTicksMsec() / 1000.0f + 0.5f;
                    _caret.visible = !_caret.visible;
                }
            }
        }

        public override void Dispose()
        {
            _editing = false;
            if (_caret != null)
            {
                _caret.Dispose();
                RemoveTextHightLight(_selection);
                _caret = null;
                _selection = null;
            }
            base.Dispose();
        }

        void DoCopy(string value)
        {
            if (onCopy != null)
            {
                onCopy(this, value);
                return;
            }
            DisplayServer.ClipboardSet(value);
        }

        void DoPaste()
        {
            if (onPaste != null)
            {
                onPaste(this);
                return;
            }
            string value = DisplayServer.ClipboardGet();
            if (!string.IsNullOrEmpty(value))
                ReplaceSelection(value);
        }

        void CreateCaret()
        {
            _caret = new NShape(gOwner);
            AddChild(_caret);
            _caret.Name = "Caret";
            _caret.SetPosition(_textDrawOffset);
        }

        void __touchBegin(EventContext context)
        {
            if (!_editing || charPositions.Count <= 1
                || virtualKeyboardInput && Stage.virtualKeyboardInput
                || context.inputEvent.button != MouseButton.Left)
                return;

            ClearSelection();

            Vector2 v = Stage.inst.touchPosition;
            v = gOwner.GlobalToLocal(v);
            TextField.CharPosition cp = GetCharPosition(v);

            AdjustCaret(cp, true);

            context.CaptureTouch();
        }

        void __touchMove(EventContext context)
        {
            if (!_editing)
                return;

            Vector2 v = Stage.inst.touchPosition;
            v = gOwner.GlobalToLocal(v);
            if (float.IsNaN(v.X))
                return;

            TextField.CharPosition cp = GetCharPosition(v);
            if (cp.charIndex != _caretPosition)
                AdjustCaret(cp);
        }

        void __mouseWheel(EventContext context)
        {
            if (_editing && mouseWheelEnabled)
            {
                context.StopPropagation();

                TextField.CharPosition cp = GetCharPosition(new Vector2(GUTTER_X, GUTTER_Y));
                int vScroll = cp.lineIndex;
                int hScroll = cp.charIndex - lines[cp.lineIndex].charIndex;
                if (context.inputEvent.mouseWheelDelta < 0)
                    vScroll--;
                else
                    vScroll++;
                Scroll(hScroll, vScroll);
            }
        }

        void __focusIn(EventContext context)
        {
            _editing = true;
            _textBeforeEdit = _inputText;

            if (_caret == null)
                CreateCaret();

            if (!string.IsNullOrEmpty(_promptText))
                UpdateText();

            float caretSize;
            //如果界面缩小过，光标很容易看不见，这里放大一下
            if (UIConfig.inputCaretSize == 1 && Stage.contentScaleFactor < 1)
                caretSize = UIConfig.inputCaretSize / Stage.contentScaleFactor;
            else
                caretSize = UIConfig.inputCaretSize;
            _caret.SetSize(caretSize, textFormat.size);
            _caret.DrawRect(0, Colors.Transparent, textFormat.color);
            _caret.visible = _editable;
            _caret.visible = true;

            RemoveTextHightLight(_selection);

            Redraw();
            TextField.CharPosition cp = GetCharPosition(_caretPosition);
            AdjustCaret(cp);

            if (Stage.virtualKeyboardInput)
            {
                if (virtualKeyboardInput)
                {
                    if (_editable)
                        DisplayServer.VirtualKeyboardShow(_inputText, null, _displayAsPassword ? DisplayServer.VirtualKeyboardType.Default : keyboardType);
                    SetSelection(0, -1);
                }
            }
            else
            {
                DisplayServer.WindowSetImeActive(!disableIME && !_displayAsPassword);
                DisplayServer.WindowSetImeActive(!disableIME && !_displayAsPassword);
                _composing = 0;

                if ((string)context.data == "key") //select all if got focus by tab key
                    SetSelection(0, -1);

                TextInputHistory.inst.StartRecord(this);
            }
        }

        void __focusOut()
        {
            if (!_editing)
                return;

            _editing = false;
            if (_usingHtmlInput)
            {
                _usingHtmlInput = false;
                visible = true;
                _caret.visible = true;
            }
            else if (Stage.virtualKeyboardInput)
            {
                if (virtualKeyboardInput)
                    DisplayServer.VirtualKeyboardHide();
            }
            else
            {
                DisplayServer.WindowSetImeActive(true);
                TextInputHistory.inst.StopRecord(this);
            }

            if (!string.IsNullOrEmpty(_promptText))
                UpdateText();

            _caret.visible = false;
            RemoveTextHightLight(_selection);

            if (contextMenu != null && contextMenu.contentPane.onStage)
                contextMenu.Hide();
        }

        void __keydown(EventContext context)
        {
            if (!_editing)
                return;

            if (HandleKey(context.inputEvent))
                context.StopPropagation();
        }

        bool HandleKey(NInputEvent evt)
        {
            bool keyCodeHandled = true;
            switch (evt.keyCode)
            {
                case Key.Backspace:
                    {
                        if (evt.command)
                        {
                            //for mac:CMD+Backspace=Delete
                            if (_selectionStart == _caretPosition && _caretPosition < charPositions.Count - 1)
                                _selectionStart = _caretPosition + 1;
                        }
                        else
                        {
                            if (_selectionStart == _caretPosition && _caretPosition > 0)
                                _selectionStart = _caretPosition - 1;
                        }
                        if (_editable)
                            ReplaceSelection(null);
                        break;
                    }

                case Key.Delete:
                    {
                        if (_selectionStart == _caretPosition && _caretPosition < charPositions.Count - 1)
                            _selectionStart = _caretPosition + 1;
                        if (_editable)
                            ReplaceSelection(null);
                        break;
                    }

                case Key.Left:
                    {
                        if (!evt.shift)
                            ClearSelection();
                        if (_caretPosition > 0)
                        {
                            if (evt.command) //mac keyboard
                            {
                                TextField.CharPosition cp = GetCharPosition(_caretPosition);
                                TextField.LineInfo line = lines[cp.lineIndex];
                                cp = GetCharPosition(new Vector2(int.MinValue, line.y + Y));
                                AdjustCaret(cp, !evt.shift);
                            }
                            else
                            {
                                TextField.CharPosition cp = GetCharPosition(_caretPosition - 1);
                                AdjustCaret(cp, !evt.shift);
                            }
                        }
                        break;
                    }

                case Key.Right:
                    {
                        if (!evt.shift)
                            ClearSelection();
                        if (_caretPosition < charPositions.Count - 1)
                        {
                            if (evt.command)
                            {
                                TextField.CharPosition cp = GetCharPosition(_caretPosition);
                                TextField.LineInfo line = lines[cp.lineIndex];
                                cp = GetCharPosition(new Vector2(int.MaxValue, line.y + Y));
                                AdjustCaret(cp, !evt.shift);
                            }
                            else
                            {
                                TextField.CharPosition cp = GetCharPosition(_caretPosition + 1);
                                AdjustCaret(cp, !evt.shift);
                            }
                        }
                        break;
                    }

                case Key.Up:
                    {
                        if (!evt.shift)
                            ClearSelection();

                        TextField.CharPosition cp = GetCharPosition(_caretPosition);
                        if (cp.lineIndex > 0)
                        {
                            TextField.LineInfo line = lines[cp.lineIndex - 1];
                            cp = GetCharPosition(new Vector2(_caret.X, line.y + Y));
                            AdjustCaret(cp, !evt.shift);
                        }
                        break;
                    }

                case Key.Down:
                    {
                        if (!evt.shift)
                            ClearSelection();

                        TextField.CharPosition cp = GetCharPosition(_caretPosition);
                        if (cp.lineIndex == lines.Count - 1)
                            cp.charIndex = charPositions.Count - 1;
                        else
                        {
                            TextField.LineInfo line = lines[cp.lineIndex + 1];
                            cp = GetCharPosition(new Vector2(_caret.X, line.y + Y));
                        }
                        AdjustCaret(cp, !evt.shift);
                        break;
                    }

                case Key.Pageup:
                    {
                        ClearSelection();
                        break;
                    }

                case Key.Pagedown:
                    {
                        ClearSelection();
                        break;
                    }

                case Key.Home:
                    {
                        if (!evt.shift)
                            ClearSelection();

                        TextField.CharPosition cp = GetCharPosition(_caretPosition);
                        TextField.LineInfo line = lines[cp.lineIndex];
                        cp = GetCharPosition(new Vector2(int.MinValue, line.y + Y));
                        AdjustCaret(cp, !evt.shift);
                        break;
                    }

                case Key.End:
                    {
                        if (!evt.shift)
                            ClearSelection();

                        TextField.CharPosition cp = GetCharPosition(_caretPosition);
                        TextField.LineInfo line = lines[cp.lineIndex];
                        cp = GetCharPosition(new Vector2(int.MaxValue, line.y + Y));
                        AdjustCaret(cp, !evt.shift);

                        break;
                    }

                //Select All
                case Key.A:
                    {
                        if (evt.ctrlOrCmd)
                        {
                            _selectionStart = 0;
                            AdjustCaret(GetCharPosition(int.MaxValue));
                        }
                        break;
                    }

                //Copy
                case Key.C:
                    {
                        if (evt.ctrlOrCmd && !_displayAsPassword)
                        {
                            string s = GetSelection();
                            if (!string.IsNullOrEmpty(s))
                                DoCopy(s);
                        }
                        break;
                    }

                //Paste
                case Key.V:
                    {
                        if (evt.ctrlOrCmd && _editable)
                            DoPaste();
                        break;
                    }

                //Cut
                case Key.X:
                    {
                        if (evt.ctrlOrCmd && !_displayAsPassword)
                        {
                            string s = GetSelection();
                            if (!string.IsNullOrEmpty(s))
                            {
                                DoCopy(s);
                                if (_editable)
                                    ReplaceSelection(null);
                            }
                        }
                        break;
                    }

                case Key.Z:
                    {
                        if (evt.ctrlOrCmd && _editable)
                        {
                            if (evt.shift)
                                TextInputHistory.inst.Redo(this);
                            else
                                TextInputHistory.inst.Undo(this);
                        }
                        break;
                    }

                case Key.Y:
                    {
                        if (evt.ctrlOrCmd && _editable)
                            TextInputHistory.inst.Redo(this);
                        break;
                    }

                case Key.Enter:
                case Key.KpEnter:
                    {
                        if (singleLine)
                        {
                            Stage.inst.focus = gOwner.parent;
                            gOwner.DispatchEvent("onSubmit", null);
                            gOwner.DispatchEvent("onKeyDown", null); //for backward compatibility
                        }
                        break;
                    }

                case Key.Tab:
                    {
                        if (singleLine)
                        {
                            Stage.inst.DoKeyNavigate(evt.shift);
                            keyCodeHandled = false;
                        }
                        break;
                    }

                case Key.Escape:
                    {
                        this.text = _textBeforeEdit;
                        Stage.inst.focus = gOwner.parent;
                        break;
                    }

                default:
                    keyCodeHandled = (int)evt.keyCode <= 272 && !evt.ctrlOrCmd;
                    break;
            }

            char c = evt.character;
            if (c != 0)
            {
                if (!evt.ctrlOrCmd)
                    HandleTextInput(c);
                return true;
            }
            else
                return keyCodeHandled;
        }

        void HandleTextInput(char c)
        {
            if (c == '\r' || c == 3)
                c = '\n';

            if (c == 25)/*shift+tab*/
                c = '\t';

            if (c == 27/*escape*/ || singleLine && (c == '\n' || c == '\t'))
                return;

            if (_editable)
            {
                if (char.IsHighSurrogate(c))
                {
                    _highSurrogateChar = c;
                    return;
                }

                if (char.IsLowSurrogate(c))
                    ReplaceSelection(char.ConvertFromUtf32(((int)c & 0x03FF) + ((((int)_highSurrogateChar & 0x03FF) + 0x40) << 10)));
                else
                    ReplaceSelection(c.ToString());
            }
        }

        void CheckComposition()
        {
            if (!_editable || virtualKeyboardInput)
                return;

            if (compositionString.Length == 0)
            {
                RemoveTextHightLight(_compositionHightLight);
                UpdateText();
            }
            else
            {
                int composing = _composing;
                _composing = compositionString.Length;

                StringBuilder buffer = new StringBuilder();
                GetPartialText(0, _caretPosition, buffer);
                buffer.Append(compositionString);
                GetPartialText(_caretPosition + composing, -1, buffer);

                base.text = buffer.ToString();

                _compositionHightLight.startCharIndex = _caretPosition;
                _compositionHightLight.endCharIndex = _caretPosition + compositionString.Length;
                _compositionHightLight.clipped = false;
                _compositionHightLight.color = UIConfig.imeCompositionHighlightColor;
                AddTextHightLight(_compositionHightLight);
            }
        }

        void __click(EventContext context)
        {
            if (_editing && context.inputEvent.isDoubleClick)
            {
                context.StopPropagation();
                _selectionStart = 0;
                AdjustCaret(GetCharPosition(int.MaxValue));
            }
        }

        void __rightClick(EventContext context)
        {
            if (contextMenu != null)
            {
                context.StopPropagation();
                contextMenu.Show();
            }
        }

        public string compositionString
        {
            get
            {
                if (Stage.virtualKeyboardInput)
                    return String.Empty;
                return _compositionString;
            }
        }

        public static DisplayServer.VirtualKeyboardType TranslateKeyboardType(FairyGUIVirtualKeyboardType type)
        {
            switch (type)
            {
                case FairyGUIVirtualKeyboardType.Character:
                    return DisplayServer.VirtualKeyboardType.Default;
                case FairyGUIVirtualKeyboardType.NumberAndInterpunction:
                    return DisplayServer.VirtualKeyboardType.NumberDecimal;
                case FairyGUIVirtualKeyboardType.Url:
                    return DisplayServer.VirtualKeyboardType.Url;
                case FairyGUIVirtualKeyboardType.Number:
                    return DisplayServer.VirtualKeyboardType.Number;
                case FairyGUIVirtualKeyboardType.Phone:
                    return DisplayServer.VirtualKeyboardType.Phone;
                case FairyGUIVirtualKeyboardType.EmailAddress:
                    return DisplayServer.VirtualKeyboardType.EmailAddress;
                default:
                    return DisplayServer.VirtualKeyboardType.Default;
            }
        }
        public override void _Notification(int what)
        {
            if (what == (int)MainLoop.NotificationOsImeUpdate && _editing)
            {
                _compositionString = DisplayServer.ImeGetText();
                CheckComposition();
            }
        }
    }

    class TextInputHistory
    {
        static TextInputHistory _inst;
        public static TextInputHistory inst
        {
            get
            {
                if (_inst == null)
                    _inst = new TextInputHistory();
                return _inst;
            }
        }

        List<string> _undoBuffer;
        List<string> _redoBuffer;
        string _currentText;
        InputTextField _textField;
        bool _lock;
        int _changedFrame;

        public const int maxHistoryLength = 5;

        public TextInputHistory()
        {
            _undoBuffer = new List<string>();
            _redoBuffer = new List<string>();
        }

        public void StartRecord(InputTextField textField)
        {
            _undoBuffer.Clear();
            _redoBuffer.Clear();
            _textField = textField;
            _lock = false;
            _currentText = _textField.text;
            _changedFrame = 0;
        }

        public void MarkChanged(InputTextField textField)
        {
            if (_textField != textField)
                return;

            if (_lock)
                return;

            string newText = _textField.text;
            if (_currentText == newText)
                return;

            if (_changedFrame != Engine.GetFramesDrawn())
            {
                _changedFrame = Engine.GetFramesDrawn();
                _undoBuffer.Add(_currentText);
                if (_undoBuffer.Count > maxHistoryLength)
                    _undoBuffer.RemoveAt(0);
            }
            else
            {
                int cnt = _undoBuffer.Count;
                if (cnt > 0 && newText == _undoBuffer[cnt - 1])
                    _undoBuffer.RemoveAt(cnt - 1);
            }
            _currentText = newText;
        }

        public void StopRecord(InputTextField textField)
        {
            if (_textField != textField)
                return;

            _undoBuffer.Clear();
            _redoBuffer.Clear();
            _textField = null;
            _currentText = null;
        }

        public void Undo(InputTextField textField)
        {
            if (_textField != textField)
                return;

            if (_undoBuffer.Count == 0)
                return;

            string text = _undoBuffer[_undoBuffer.Count - 1];
            _undoBuffer.RemoveAt(_undoBuffer.Count - 1);
            _redoBuffer.Add(_currentText);
            _lock = true;
            int caretPos = _textField.caretPosition;
            _textField.text = text;
            int dlen = text.Length - _currentText.Length;
            if (dlen < 0)
                _textField.caretPosition = caretPos + dlen;
            _currentText = text;
            _lock = false;
        }

        public void Redo(InputTextField textField)
        {
            if (_textField != textField)
                return;

            if (_redoBuffer.Count == 0)
                return;

            string text = _redoBuffer[_redoBuffer.Count - 1];
            _redoBuffer.RemoveAt(_redoBuffer.Count - 1);
            _undoBuffer.Add(_currentText);
            _lock = true;
            int caretPos = _textField.caretPosition;
            _textField.text = text;
            int dlen = text.Length - _currentText.Length;
            if (dlen > 0)
                _textField.caretPosition = caretPos + dlen;
            _currentText = text;
            _lock = false;
        }
    }
}
