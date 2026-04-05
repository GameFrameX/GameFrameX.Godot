using System;
using System.Collections.Generic;
using System.Text;
using Godot;
using FairyGUI.Utils;

namespace FairyGUI
{
    public partial class TextField : Node2D, IDisplayObject
    {
        protected Vector2 _position = Vector2.Zero;
        protected Vector2 _size = Vector2.Zero;
        protected Vector2 _pivot = Vector2.Zero;
        protected Vector2 _scale = Vector2.One;
        protected float _rotation = 0;
        protected Vector2 _skew = Vector2.Zero;
        VertAlignType _verticalAlign;
        protected TextFormat _textFormat;
        protected string _text;
        AutoSizeType _autoSize;
        bool _wordWrap;
        bool _singleLine;
        bool _html;
        RTLSupport.DirectionType _textDirection;
        int _maxWidth;

        List<HtmlElement> _elements;
        List<LineInfo> _lines;
        protected List<CharPosition> _charPositions;
        TextMeshCluster _meshs;

        BaseFont _font;
        float _textWidth;
        float _textHeight;
        protected bool _textChanged;
        protected bool _needUpdateMesh;
        float _yOffset;
        float _fontSizeScale;
        float _renderScale;
        int _fontVersion;
        string _parsedText;
        int _ellipsisCharIndex;
        int _typingEffectPos;
        Vector2 _oldSize;
        bool _inDrawing = false;

        const int GUTTER_X = 2;
        const int GUTTER_Y = 2;
        const float IMAGE_BASELINE = 0.8f;
        const int ELLIPSIS_LENGTH = 2;
        static List<LineCharInfo> sLineChars = new List<LineCharInfo>();

        bool _InUpdateSize = false;
        protected Vector2 _textDrawOffset = Vector2.Zero;
        List<TextHightLightInfo> _textHightLights = new List<TextHightLightInfo>();
        bool _textHightLightChanged = false;

        public GObject gOwner { get; set; }
        public IDisplayObject parent { get { return GetParent() as IDisplayObject; } }
        public CanvasItem node { get { return this; } }
        public bool visible { get { return Visible; } set { Visible = value; } }
        public Vector2 skew
        {
            get { return _skew; }
            set
            {
                if (!_skew.IsEqualApprox(value))
                {
                    _skew = value;
                    UpdateTransform();
                }
            }
        }
        public float skewX
        {
            get { return _skew.X; }
            set
            {
                if (!Mathf.IsEqualApprox(_skew.X, value))
                {
                    _skew.X = value;
                    UpdateTransform();
                }
            }
        }
        public float skewY
        {
            get { return _skew.Y; }
            set
            {
                if (!Mathf.IsEqualApprox(_skew.Y, value))
                {
                    _skew.Y = value;
                    UpdateTransform();
                }
            }
        }
        public Vector2 position
        {
            get { return Position; }
            set { SetPosition(position); }
        }
        public float X
        {
            get { return Position.X; }
            set { SetXY(value, Position.Y); }
        }
        public float Y
        {
            get { return Position.Y; }
            set { SetXY(Position.X, value); }
        }
        public void SetXY(float x, float y)
        {
            if (!Mathf.IsEqualApprox(_position.X, x) || !Mathf.IsEqualApprox(_position.Y, y))
            {
                _position.X = x;
                _position.Y = y;
                UpdatePosition();
            }
        }
        public new void SetPosition(Vector2 pos)
        {
            if (!_position.IsEqualApprox(pos))
            {
                _position = pos;
                UpdatePosition();
            }
        }
        public Vector2 size
        {
            get { return _size; }
            set { SetSize(value); }
        }
        public float width
        {
            get { return _size.X; }
            set
            {
                if (!Mathf.IsEqualApprox(value, _size.X))
                {
                    _size.X = value;
                    UpdateTransform();
                    OnSizeChanged();
                }
            }
        }
        public float height
        {
            get { return _size.Y; }
            set
            {
                if (!Mathf.IsEqualApprox(value, _size.Y))
                {
                    _size.Y = value;
                    UpdateTransform();
                    OnSizeChanged();
                }
            }
        }
        public void SetSize(float w, float h)
        {
            if (!Mathf.IsEqualApprox(w, _size.X) || !Mathf.IsEqualApprox(h, _size.Y))
            {
                _size.X = w;
                _size.Y = h;
                UpdateTransform();
                OnSizeChanged();
            }
        }
        public void SetSize(Vector2 size)
        {
            SetSize(size.X, size.Y);
        }
        public Vector2 pivot
        {
            get { return _pivot; }
            set
            {
                if (!_pivot.IsEqualApprox(value))
                {
                    _pivot = value;
                    UpdatePosition();
                }
            }
        }
        public Vector2 scale
        {
            get { return _scale; }
            set
            {
                if (!_scale.IsEqualApprox(value))
                {
                    _scale = value;
                    UpdateTransform();
                }
            }
        }
        public float rotation
        {
            get { return _rotation; }
            set
            {
                if (!Mathf.IsEqualApprox(_rotation, value))
                {
                    _rotation = value;
                    UpdateTransform();
                }
            }
        }
        void UpdatePosition()
        {
            if (!_pivot.IsZeroApprox())
                UpdateTransform();
            else
                Position = _position;
        }
        void UpdateTransform()
        {
            var transform = Transform2D.Identity;
            if (!Mathf.IsZeroApprox(_rotation) || !_skew.IsZeroApprox() || !_scale.IsEqualApprox(Vector2.One))
            {
                transform.X.X = Mathf.Cos(_rotation + _skew.Y) * _scale.X;
                transform.X.Y = Mathf.Sin(_rotation + _skew.Y) * _scale.X;
                transform.Y.X = -Mathf.Sin(_rotation + _skew.X) * _scale.Y;
                transform.Y.Y = Mathf.Cos(_rotation + _skew.X) * _scale.Y;
            }
            if (Mathf.IsZeroApprox(transform.Determinant()))
            {
                Vector2 xAxis = transform.X;
                float equivalentRotation = Mathf.Atan2(xAxis.Y, xAxis.X);
                float equivalentScaleX = xAxis.Length();
                float equivalentScaleY = transform.Y.Length();
                transform = Transform2D.Identity;
                transform.X.X = Mathf.Cos(equivalentRotation) * equivalentScaleX;
                transform.X.Y = Mathf.Sin(equivalentRotation) * equivalentScaleX;
                transform.Y.X = -Mathf.Sin(equivalentRotation) * equivalentScaleY;
                transform.Y.Y = Mathf.Cos(equivalentRotation) * equivalentScaleY;
            }
            if (!_pivot.IsZeroApprox())
            {
                Vector2 pivotOffset = _pivot * _size;
                transform.Origin = _position + pivotOffset;
                transform = transform * Transform2D.Identity.Translated(-pivotOffset);
            }
            else
                transform.Origin = _position;
            Transform = transform;
            QueueRedraw();
        }
        public BlendMode blendMode { get; set; }


        public TextField(GObject owner)
        {
            gOwner = owner;
            _meshs = new TextMeshCluster();
            _textFormat = new TextFormat();
            _fontSizeScale = 1;
            _renderScale = Stage.contentScaleFactor;

            _wordWrap = false;
            _text = string.Empty;
            _parsedText = string.Empty;
            _typingEffectPos = -1;

            _elements = new List<HtmlElement>(0);
            _lines = new List<LineInfo>(1);

            Name = "TextField";
            _oldSize = _size;
        }
        public new virtual void Dispose()
        {
        }

        public virtual TextFormat textFormat
        {
            get { return _textFormat; }
            set
            {
                _textFormat = value;
                ApplyFormat();
            }
        }

        public void ApplyFormat()
        {
            string fontName = _textFormat.font;
            if (string.IsNullOrEmpty(fontName))
                fontName = UIConfig.defaultFont;
            BaseFont newFont = FontManager.inst.GetFont(fontName);
            if (_font != newFont)
            {
                _font = newFont;
                _fontVersion = _font.version;
            }

            if (!string.IsNullOrEmpty(_text))
            {
                _textChanged = true;
                QueueRedraw();
            }
        }

        [Export]
        public string fontName
        {
            get { return _textFormat.font; }
            set
            {
                if (_textFormat.font != value)
                {
                    _textFormat.font = value;
                    if (string.IsNullOrEmpty(_textFormat.font))
                        _textFormat.font = UIConfig.defaultFont;
                    BaseFont newFont = FontManager.inst.GetFont(_textFormat.font);
                    if (_font != newFont)
                    {
                        _font = newFont;
                        _fontVersion = _font.version;
                    }

                    if (!string.IsNullOrEmpty(_text))
                    {
                        _textChanged = true;
                        QueueRedraw();
                    }
                }
            }
        }
        [Export]
        public int _fontSize
        {
            get { return _textFormat.size; }
            set
            {
                if (_textFormat.size != value)
                {
                    _textFormat.size = value;
                    if (!string.IsNullOrEmpty(_text))
                    {
                        _textChanged = true;
                        QueueRedraw();
                    }
                }
            }
        }
        [Export]
        public Color _fontColor
        {
            get { return _textFormat.color; }
            set
            {
                if (_textFormat.color != value)
                {
                    _textFormat.color = value;
                    if (!string.IsNullOrEmpty(_text))
                    {
                        _textChanged = true;
                        QueueRedraw();
                    }
                }
            }
        }

        [Export]
        public AlignType align
        {
            get { return _textFormat.align; }
            set
            {
                if (_textFormat.align != value)
                {
                    _textFormat.align = value;
                    if (!string.IsNullOrEmpty(_text))
                    {
                        _textChanged = true;
                        QueueRedraw();
                    }
                }
            }
        }

        [Export]
        public VertAlignType verticalAlign
        {
            get
            {
                return _verticalAlign;
            }
            set
            {
                if (_verticalAlign != value)
                {
                    _verticalAlign = value;
                    if (!_textChanged)
                        ApplyVertAlign();
                }
            }
        }

        [Export]
        public virtual string text
        {
            get { return _text; }
            set
            {
                if (_text == value && !_html)
                    return;

                _text = value;
                _textChanged = true;
                _html = false;
                QueueRedraw();
            }
        }

        public string htmlText
        {
            get { return _text; }
            set
            {
                if (_text == value && _html)
                    return;

                _text = value;
                _textChanged = true;
                _html = true;
                QueueRedraw();
            }
        }

        public string parsedText
        {
            get { return _parsedText; }
        }

        [Export]
        public AutoSizeType autoSize
        {
            get { return _autoSize; }
            set
            {
                if (_autoSize != value)
                {
                    _autoSize = value;
                    _textChanged = true;
                }
            }
        }

        [Export]
        public bool wordWrap
        {
            get { return _wordWrap; }
            set
            {
                if (_wordWrap != value)
                {
                    _wordWrap = value;
                    _textChanged = true;
                }
            }
        }

        [Export]
        public bool singleLine
        {
            get { return _singleLine; }
            set
            {
                if (_singleLine != value)
                {
                    _singleLine = value;
                    _textChanged = true;
                }
            }
        }

        public float stroke
        {
            get
            {
                return _textFormat.outline;
            }
            set
            {
                if (_textFormat.outline != value)
                {
                    _textFormat.outline = value;
                    _textChanged = true;
                }
            }
        }

        public Color strokeColor
        {
            get
            {
                return _textFormat.outlineColor;
            }
            set
            {
                if (_textFormat.outlineColor != value)
                {
                    _textFormat.outlineColor = value;
                    _textChanged = true;
                }
            }
        }

        public Vector2 shadowOffset
        {
            get
            {
                return _textFormat.shadowOffset;
            }
            set
            {
                _textFormat.shadowOffset = value;
                _textChanged = true;
            }
        }

        public float textWidth
        {
            get
            {
                if (_textChanged)
                    BuildLines();

                return _textWidth;
            }
        }

        public float textHeight
        {
            get
            {
                if (_textChanged)
                    BuildLines();

                return _textHeight;
            }
        }

        public int maxWidth
        {
            get { return _maxWidth; }
            set
            {
                if (_maxWidth != value)
                {
                    _maxWidth = value;
                    _textChanged = true;
                }
            }
        }

        public List<HtmlElement> htmlElements
        {
            get
            {
                if (_textChanged)
                    BuildLines();

                return _elements;
            }
        }

        public List<LineInfo> lines
        {
            get
            {
                if (_textChanged)
                    BuildLines();

                return _lines;
            }
        }

        public List<CharPosition> charPositions
        {
            get
            {
                if (_textChanged)
                    BuildLines();
                return _charPositions;
            }
        }

        public TextField.CharPosition GetCharPosition(int caretIndex)
        {
            if (caretIndex < 0)
                caretIndex = 0;
            else if (caretIndex >= charPositions.Count)
                caretIndex = charPositions.Count - 1;

            return charPositions[caretIndex];
        }

        /// <summary>
        /// 通过本地坐标获得字符索引位置
        /// </summary>
        /// <param name="location">本地坐标</param>
        /// <returns></returns>
        public TextField.CharPosition GetCharPosition(Vector2 location)
        {
            if (charPositions.Count <= 1)
                return charPositions[0];

            location.X -= _textDrawOffset.X;
            location.Y -= _textDrawOffset.Y;

            List<TextField.LineInfo> lines2 = lines;
            int len = lines2.Count;
            TextField.LineInfo line;
            int i;
            for (i = 0; i < len; i++)
            {
                line = lines2[i];
                if (line.y + line.height > location.Y)
                    break;
            }
            if (i == len)
                i = len - 1;

            int lineIndex = i;

            len = charPositions.Count;
            TextField.CharPosition v;
            int firstInLine = -1;
            for (i = 0; i < len; i++)
            {
                v = charPositions[i];
                if (v.lineIndex == lineIndex)
                {
                    if (firstInLine == -1)
                        firstInLine = i;
                    if (v.offsetX + v.width * 0.5f > location.X)
                        return v;
                }
                else if (firstInLine != -1)
                {
                    if (parsedText[i - 1] == '\n')
                        return charPositions[i - 1];
                    else
                        return v;
                }
            }

            return charPositions[i - 1];
        }

        /// <summary>
        /// 获得字符的坐标。
        /// </summary>
        /// <param name="cp"></param>
        /// <returns></returns>
        public Vector2 GetCharLocation(TextField.CharPosition cp)
        {
            TextField.LineInfo line = lines[cp.lineIndex];
            Vector2 pos;
            if (line.charCount == 0 || charPositions.Count == 0)
            {
                if (align == AlignType.Center)
                    pos.X = (int)(width / 2);
                else
                    pos.X = GUTTER_X;
            }
            else
            {
                TextField.CharPosition v = charPositions[Math.Min(cp.charIndex, charPositions.Count - 1)];
                pos.X = v.offsetX;
            }
            pos.Y = line.y;
            return pos;
        }

        public void AddTextHightLight(TextHightLightInfo info)
        {
            if (_textHightLights.IndexOf(info) < 0)
            {
                _textHightLights.Add(info);
            }
            _textHightLightChanged = true;
            QueueRedraw();
        }

        public void RemoveTextHightLight(TextHightLightInfo info)
        {
            if (_textHightLights.Remove(info))
            {
                _textHightLightChanged = true;
                QueueRedraw();
            }
        }

        public void ClearTextHightLight()
        {
            if (_textHightLights.Count > 0)
            {
                _textHightLightChanged = true;
                _textHightLights.Clear();
                QueueRedraw();
            }
        }


        void UpdateTextHightLight()
        {
            if (!_textHightLightChanged)
                return;
            foreach (var info in _textHightLights)
            {
                if (info.startCharIndex == info.endCharIndex)
                    continue;
                if (info.startCharIndex > info.endCharIndex)
                {
                    var tmp = info.startCharIndex;
                    info.startCharIndex = info.endCharIndex;
                    info.endCharIndex = tmp;
                }
                CharPosition start = GetCharPosition(info.startCharIndex);
                CharPosition end = GetCharPosition(info.endCharIndex);
                Vector2 startV = GetCharLocation(start);
                Vector2 endV = GetCharLocation(end);
                if (info.rects == null)
                    info.rects = new List<Rect>();
                else
                    info.rects.Clear();
                GetLinesShape(start.lineIndex, startV.X, end.lineIndex, endV.X, info.clipped, info.rects);
            }
            _textHightLightChanged = true;
        }

        public bool Redraw()
        {
            if (_font == null)
            {
                _font = FontManager.inst.GetFont(UIConfig.defaultFont);
                _fontVersion = _font.version;
                _textChanged = true;
            }

            if (_renderScale != Stage.contentScaleFactor)
                _textChanged = true;

            if (_font.version != _fontVersion)
            {
                _fontVersion = _font.version;
                _textChanged = true;
            }
            if (_textChanged)
                BuildLines();
            if (_needUpdateMesh)
            {
                UpdateMesh();
                QueueRedraw();
                return true;
            }
            return false;
        }

        public override void _Draw()
        {
            _inDrawing = true;
            if (_textChanged)
                BuildLines();
            if (_needUpdateMesh)
                UpdateMesh();
            UpdateTextHightLight();

            Transform2D trans = Transform2D.Identity;
            trans.Origin = _textDrawOffset;
            foreach (var info in _textHightLights)
            {
                if (info.rects == null)
                    continue;
                foreach (var rect in info.rects)
                    DrawRect(rect + _textDrawOffset, info.color);
            }
            for (int i = 0; i < _meshs.meshs.Count; i++)
            {
                TextMeshCluster.TextMeshInfo mesh = _meshs.meshs[i];
                DrawMesh(mesh.mesh, mesh.tex, trans);
            }
            _inDrawing = false;
        }

        public int SetTypingEffectPos(int pos)
        {
            _typingEffectPos = pos;
            Redraw();
            pos = _typingEffectPos;
            _typingEffectPos = -1;
            return pos;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool HasCharacter(char ch)
        {
            return _font.HasCharacter(ch);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startLine"></param>
        /// <param name="startCharX"></param>
        /// <param name="endLine"></param>
        /// <param name="endCharX"></param>
        /// <param name="clipped"></param>
        /// <param name="resultRects"></param>
        public void GetLinesShape(int startLine, float startCharX, int endLine, float endCharX,
            bool clipped,
            List<Rect> resultRects)
        {
            LineInfo line1 = _lines[startLine];
            LineInfo line2 = _lines[endLine];
            bool leftAlign = _textFormat.align == AlignType.Left;
            Rect rect = new Rect(-_pivot * _size, _size);
            if (startLine == endLine)
            {
                Rect r = Rect.MinMaxRect(startCharX, line1.y, endCharX, line1.y + line1.height);
                if (clipped)
                    resultRects.Add(ToolSet.Intersection(ref r, ref rect));
                else
                    resultRects.Add(r);
            }
            else if (startLine == endLine - 1)
            {
                Rect r = Rect.MinMaxRect(startCharX, line1.y, leftAlign ? (GUTTER_X + line1.width) : rect.xMax, line1.y + line1.height);
                if (clipped)
                    resultRects.Add(ToolSet.Intersection(ref r, ref rect));
                else
                    resultRects.Add(r);
                r = Rect.MinMaxRect(GUTTER_X, line1.y + line1.height, endCharX, line2.y + line2.height);
                if (clipped)
                    resultRects.Add(ToolSet.Intersection(ref r, ref rect));
                else
                    resultRects.Add(r);
            }
            else
            {
                Rect r = Rect.MinMaxRect(startCharX, line1.y, leftAlign ? (GUTTER_X + line1.width) : rect.xMax, line1.y + line1.height);
                if (clipped)
                    resultRects.Add(ToolSet.Intersection(ref r, ref rect));
                else
                    resultRects.Add(r);
                for (int i = startLine + 1; i < endLine; i++)
                {
                    LineInfo line = _lines[i];
                    r = Rect.MinMaxRect(GUTTER_X, r.yMax, leftAlign ? (GUTTER_X + line.width) : rect.xMax, line.y + line.height);
                    if (clipped)
                        resultRects.Add(ToolSet.Intersection(ref r, ref rect));
                    else
                        resultRects.Add(r);
                }
                r = Rect.MinMaxRect(GUTTER_X, r.yMax, endCharX, line2.y + line2.height);
                if (clipped)
                    resultRects.Add(ToolSet.Intersection(ref r, ref rect));
                else
                    resultRects.Add(r);
            }
        }

        protected virtual void OnSizeChanged()
        {
            if (!_InUpdateSize)
            {
                if (_autoSize == AutoSizeType.Shrink || _autoSize == AutoSizeType.Ellipsis || _wordWrap && !Mathf.IsEqualApprox(_oldSize.X, _size.X))
                    _textChanged = true;
                else if (_autoSize != AutoSizeType.None)
                    QueueRedraw();

                if (_verticalAlign != VertAlignType.Top)
                    ApplyVertAlign();
            }
            _oldSize = _size;
            (gOwner as GTextField)?.UpdateSize(_size.X, _size.Y);
        }

        public void EnsureSizeCorrect()
        {
            if (_textChanged && _autoSize != AutoSizeType.None)
                BuildLines();
        }

        /// <summary>
        /// 准备字体纹理
        /// </summary>
        void RequestText()
        {
            if (!_html)
            {
                _font.PrepareCharacters(_parsedText, _textFormat, _fontSizeScale);
                if (_autoSize == AutoSizeType.Ellipsis)
                    _font.PrepareCharacters("…", _textFormat, _fontSizeScale);
            }
            else
            {
                int count = _elements.Count;
                for (int i = 0; i < count; i++)
                {
                    HtmlElement element = _elements[i];
                    if (element.type == HtmlElementType.Text)
                    {
                        _font.SetFormat(element.format, _fontSizeScale);
                        _font.PrepareCharacters(element.text, element.format, _fontSizeScale);
                        if (_autoSize == AutoSizeType.Ellipsis)
                            _font.PrepareCharacters("…", element.format, _fontSizeScale);
                    }
                }
            }
        }

        void BuildLines(bool beForce = false)
        {
            if (!_textChanged && !beForce)
                return;
            if (_font == null)
            {
                _font = FontManager.inst.GetFont(UIConfig.defaultFont);
                _fontVersion = _font.version;
            }

            _textChanged = false;
            _renderScale = Stage.contentScaleFactor;
            _fontSizeScale = 1;
            _ellipsisCharIndex = -1;

            Cleanup();

            if (_text.Length == 0)
            {
                LineInfo emptyLine = LineInfo.Borrow();
                emptyLine.width = 0;
                emptyLine.height = _font.GetLineHeight(_textFormat.size);
                emptyLine.charIndex = emptyLine.charCount = 0;
                emptyLine.y = emptyLine.y2 = GUTTER_Y;
                _lines.Add(emptyLine);

                _textWidth = _textHeight = 0;
            }
            else
            {
                ParseText();

                _font.Prepare(_textFormat);

                BuildLines2();

                if (_autoSize == AutoSizeType.Shrink)
                    DoShrink();
            }

            if (_autoSize == AutoSizeType.Both)
            {
                _InUpdateSize = true;
                if (this is InputTextField)
                {
                    float w = Mathf.Max(_textFormat.size, _textWidth);
                    float h = Mathf.Max(_font.GetLineHeight(_textFormat.size) + GUTTER_Y * 2, _textHeight);
                    SetSize(w, h);
                }
                else
                    SetSize(_textWidth, _textHeight);
                _InUpdateSize = false;
            }
            else if (_autoSize == AutoSizeType.Height)
            {
                _InUpdateSize = true;
                if (this is InputTextField)
                    SetSize(_size.X, Mathf.Max(_font.GetLineHeight(_textFormat.size) + GUTTER_Y * 2, _textHeight));
                else
                    SetSize(_size.X, _textHeight);
                _InUpdateSize = false;
            }

            _yOffset = 0;
            ApplyVertAlign();
            _needUpdateMesh = true;
            QueueRedraw();
        }

        void ParseText()
        {
#if RTL_TEXT_SUPPORT
            _textDirection = RTLSupport.DetectTextDirection(txt);
#endif
            if (_html)
            {
                HtmlParser.inst.Parse(_text, _textFormat, _elements, (this as RichTextField)?.htmlParseOptions);

                _parsedText = string.Empty;
            }
            else
                _parsedText = _text;

            int elementCount = _elements.Count;
            if (elementCount == 0)
            {
                if (_textDirection != RTLSupport.DirectionType.UNKNOW)
                    _parsedText = RTLSupport.DoMapping(_parsedText);

                bool flag = this is InputTextField || (this as RichTextField)?.emojies != null;
                if (!flag)
                {
                    //检查文本中是否有需要转换的字符，如果没有，节省一个new StringBuilder的操作。
                    int cnt = _parsedText.Length;
                    for (int i = 0; i < cnt; i++)
                    {
                        char ch = _parsedText[i];
                        if (ch == '\r' || char.IsHighSurrogate(ch))
                        {
                            flag = true;
                            break;
                        }
                    }
                }

                if (flag)
                {
                    StringBuilder buffer = new StringBuilder();
                    ParseText(buffer, _parsedText, -1);
                    elementCount = _elements.Count;
                    _parsedText = buffer.ToString();
                }
            }
            else
            {
                StringBuilder buffer = new StringBuilder();
                int i = 0;
                while (i < elementCount)
                {
                    HtmlElement element = _elements[i];
                    element.charIndex = buffer.Length;
                    if (element.type == HtmlElementType.Text)
                    {
                        if (_textDirection != RTLSupport.DirectionType.UNKNOW)
                            element.text = RTLSupport.DoMapping(element.text);

                        i = ParseText(buffer, element.text, i);
                        elementCount = _elements.Count;
                    }
                    else if (element.isEntity)
                        buffer.Append(' ');
                    i++;
                }
                _parsedText = buffer.ToString();

#if RTL_TEXT_SUPPORT
                // element.text拼接完后再进行一次判断文本主语序，避免html标签存在把文本变成混合文本 [2018/12/12/ 16:47:42 by aq_1000]
                _textDirection = RTLSupport.DetectTextDirection(_parsedText);
#endif
            }
        }

        void BuildLines2()
        {
            float letterSpacing = _textFormat.letterSpacing * _fontSizeScale;
            float lineSpacing = (_textFormat.lineSpacing - 1) * _fontSizeScale;
            float rectWidth = _size.X - GUTTER_X * 2;
            float rectHeight = _size.Y > 0 ? Mathf.Max(_size.Y, _font.GetLineHeight(_textFormat.size)) : 0;
            float glyphWidth = 0, glyphHeight = 0, baseline = 0;
            short wordLen = 0;
            bool wordPossible = false;
            float posx = 0;
            bool checkEdge = _autoSize == AutoSizeType.Ellipsis;
            bool hasLine = _textFormat.underline || _textFormat.strikethrough;

            TextFormat format = _textFormat;
            _font.SetFormat(format, _fontSizeScale);
            bool wrap = _wordWrap && !_singleLine;
            if (_maxWidth > 0)
            {
                wrap = true;
                rectWidth = _maxWidth - GUTTER_X * 2;
            }
            _textWidth = _textHeight = 0;

            RequestText();

            int elementCount = _elements.Count;
            int elementIndex = 0;
            HtmlElement element = null;
            if (elementCount > 0)
                element = _elements[elementIndex];
            int textLength = _parsedText.Length;

            LineInfo line = LineInfo.Borrow();
            _lines.Add(line);
            line.y = line.y2 = GUTTER_Y;
            sLineChars.Clear();

            for (int charIndex = 0; charIndex < textLength; charIndex++)
            {
                char ch = _parsedText[charIndex];

                glyphWidth = glyphHeight = baseline = 0;

                while (element != null && element.charIndex == charIndex)
                {
                    if (element.type == HtmlElementType.Text)
                    {
                        format = element.format;
                        _font.SetFormat(format, _fontSizeScale);

                        if (format.underline || format.strikethrough)
                            hasLine = true;
                    }
                    else
                    {
                        if (element.type == HtmlElementType.Link)
                            hasLine = true;

                        IHtmlObject htmlObject = element.htmlObject;
                        if (this is RichTextField && htmlObject == null)
                        {
                            var rich = this as RichTextField;
                            element.space = (int)(rectWidth - line.width - 4);
                            htmlObject = rich.htmlPageContext.CreateObject(rich, element);
                            element.htmlObject = htmlObject;
                        }
                        if (htmlObject != null)
                        {
                            glyphWidth = htmlObject.width + 2;
                            glyphHeight = htmlObject.height;
                            baseline = glyphHeight * IMAGE_BASELINE;
                        }

                        if (element.isEntity)
                            ch = '\0'; //indicate it is a place holder
                    }

                    elementIndex++;
                    if (elementIndex < elementCount)
                        element = _elements[elementIndex];
                    else
                        element = null;
                }

                if (ch == '\0' || ch == '\n')
                {
                    wordPossible = false;
                }
                else if (_font.GetGlyph(ch == '\t' ? ' ' : ch, out glyphWidth, out glyphHeight, out baseline))
                {
                    if (ch == '\t')
                        glyphWidth *= 4;

                    if (wordPossible)
                    {
                        if (char.IsWhiteSpace(ch))
                        {
                            wordLen = 0;
                        }
                        else if (ch >= 'a' && ch <= 'z' || ch >= 'A' && ch <= 'Z'
                            || ch >= '0' && ch <= '9'
                            || ch == '.' || ch == '"' || ch == '\''
                            || format.specialStyle == TextFormat.SpecialStyle.Subscript
                            || format.specialStyle == TextFormat.SpecialStyle.Superscript
                            || _textDirection != RTLSupport.DirectionType.UNKNOW && RTLSupport.IsArabicLetter(ch))
                        {
                            wordLen++;
                        }
                        else
                            wordPossible = false;
                    }
                    else if (char.IsWhiteSpace(ch))
                    {
                        wordLen = 0;
                        wordPossible = true;
                    }
                    else if (format.specialStyle == TextFormat.SpecialStyle.Subscript
                        || format.specialStyle == TextFormat.SpecialStyle.Superscript)
                    {
                        if (sLineChars.Count > 0)
                        {
                            wordLen = 2; //避免上标和下标折到下一行
                            wordPossible = true;
                        }
                    }
                    else
                        wordPossible = false;
                }
                else
                    wordPossible = false;

                sLineChars.Add(new LineCharInfo() { width = glyphWidth, height = glyphHeight, baseline = baseline });
                if (glyphWidth != 0)
                {
                    if (posx != 0)
                        posx += letterSpacing;
                    posx += glyphWidth;
                }

                if (ch == '\n' && !_singleLine)
                {
                    UpdateLineInfo(line, letterSpacing, sLineChars.Count);

                    LineInfo newLine = LineInfo.Borrow();
                    _lines.Add(newLine);
                    newLine.y = line.y + (line.height + lineSpacing);
                    if (newLine.y < GUTTER_Y) //lineSpacing maybe negative
                        newLine.y = GUTTER_Y;
                    newLine.y2 = newLine.y;
                    newLine.charIndex = line.charIndex + line.charCount;

                    if (checkEdge && line.y + line.height < rectHeight)
                        _ellipsisCharIndex = line.charIndex + Math.Max(0, line.charCount - ELLIPSIS_LENGTH);

                    sLineChars.Clear();
                    wordPossible = false;
                    posx = 0;
                    line = newLine;
                }
                else if (posx > rectWidth)
                {
                    if (wrap)
                    {
                        int lineCharCount = sLineChars.Count;
                        int toMoveChars;

                        if (wordPossible && wordLen < 20 && lineCharCount > 2) //if word had broken, move word to new line
                        {
                            toMoveChars = wordLen;
                            //we caculate the line width WITHOUT the tailing space
                            UpdateLineInfo(line, letterSpacing, lineCharCount - (toMoveChars + 1));
                            line.charCount++; //but keep it in this line.
                        }
                        else
                        {
                            toMoveChars = lineCharCount > 1 ? 1 : 0; //if only one char here, we cant move it to new line
                            UpdateLineInfo(line, letterSpacing, lineCharCount - toMoveChars);
                        }

                        LineInfo newLine = LineInfo.Borrow();
                        _lines.Add(newLine);
                        newLine.y = line.y + (line.height + lineSpacing);
                        if (newLine.y < GUTTER_Y)
                            newLine.y = GUTTER_Y;
                        newLine.y2 = newLine.y;
                        newLine.charIndex = line.charIndex + line.charCount;

                        posx = 0;
                        if (toMoveChars != 0)
                        {
                            for (int i = line.charCount; i < lineCharCount; i++)
                            {
                                LineCharInfo ci = sLineChars[i];
                                if (posx != 0)
                                    posx += letterSpacing;
                                posx += ci.width;
                            }

                            sLineChars.RemoveRange(0, line.charCount);
                        }
                        else
                            sLineChars.Clear();

                        if (checkEdge && line.y + line.height < rectHeight)
                            _ellipsisCharIndex = line.charIndex + Math.Max(0, line.charCount - ELLIPSIS_LENGTH);

                        wordPossible = false;
                        line = newLine;
                    }
                    else if (checkEdge && _ellipsisCharIndex == -1)
                        _ellipsisCharIndex = line.charIndex + Math.Max(0, sLineChars.Count - ELLIPSIS_LENGTH - 1);
                }
            }

            UpdateLineInfo(line, letterSpacing, sLineChars.Count);

            if (_textWidth > 0)
                _textWidth += GUTTER_X * 2;
            _textHeight = line.y + line.height + GUTTER_Y;

            if (checkEdge && _textWidth <= _size.X && _textHeight <= _size.Y + GUTTER_Y)
                _ellipsisCharIndex = -1;

            if (checkEdge)
                _font.GetGlyph('…', out glyphWidth, out glyphHeight, out baseline);
            if (hasLine)
                _font.GetGlyph('_', out glyphWidth, out glyphHeight, out baseline);

            _textWidth = Mathf.RoundToInt(_textWidth);
            _textHeight = Mathf.RoundToInt(_textHeight);
        }

        void UpdateLineInfo(LineInfo line, float letterSpacing, int cnt)
        {
            for (int i = 0; i < cnt; i++)
            {
                LineCharInfo ci = sLineChars[i];
                if (ci.baseline > line.baseline)
                {
                    line.height += (ci.baseline - line.baseline);
                    line.baseline = ci.baseline;
                }

                if (ci.height - ci.baseline > line.height - line.baseline)
                    line.height += (ci.height - ci.baseline - (line.height - line.baseline));

                if (ci.width > 0)
                {
                    if (line.width != 0)
                        line.width += letterSpacing;
                    line.width += ci.width;
                }
            }

            if (line.height == 0)
            {
                if (_lines.Count == 1)
                    line.height = _textFormat.size;
                else
                    line.height = _lines[_lines.Count - 2].height;
            }

            if (line.width > _textWidth)
                _textWidth = line.width;

            line.charCount = (short)cnt;
        }

        void DoShrink()
        {
            if (_lines.Count > 1 && _textHeight > _size.Y)
            {
                //多行的情况，涉及到自动换行，得用二分法查找最合适的比例，会消耗多一点计算资源
                int low = 0;
                int high = _textFormat.size;

                //先尝试猜测一个比例
                _fontSizeScale = Mathf.Sqrt(_size.Y / _textHeight);
                int cur = Mathf.FloorToInt(_fontSizeScale * _textFormat.size);

                while (true)
                {
                    LineInfo.Return(_lines);
                    BuildLines2();

                    if (_textWidth > _size.X || _textHeight > _size.Y)
                        high = cur;
                    else
                        low = cur;
                    if (high - low > 1 || high != low && cur == high)
                    {
                        cur = low + (high - low) / 2;
                        _fontSizeScale = (float)cur / _textFormat.size;
                    }
                    else
                        break;
                }
            }
            else if (_textWidth > _size.X)
            {
                _fontSizeScale = _size.X / _textWidth;

                LineInfo.Return(_lines);
                BuildLines2();

                if (_textWidth > _size.X) //如果还超出，缩小一点再来一次
                {
                    int size = Mathf.FloorToInt(_textFormat.size * _fontSizeScale);
                    size--;
                    _fontSizeScale = (float)size / _textFormat.size;

                    LineInfo.Return(_lines);
                    BuildLines2();
                }
            }
        }

        int ParseText(StringBuilder buffer, string source, int elementIndex)
        {
            int textLength = source.Length;
            int j = 0;
            int appendPos = 0;
            var rich = this as RichTextField;
            bool hasEmojies = rich?.emojies != null;
            while (j < textLength)
            {
                char ch = source[j];
                if (ch == '\r')
                {
                    buffer.Append(source, appendPos, j - appendPos);
                    if (j != textLength - 1 && source[j + 1] == '\n')
                        j++;
                    appendPos = j + 1;
                    buffer.Append('\n');
                }
                else
                {
                    bool highSurrogate = char.IsHighSurrogate(ch);
                    if (hasEmojies)
                    {
                        uint emojiKey = 0;
                        Emoji emoji;
                        if (highSurrogate)
                            emojiKey = ((uint)source[j + 1] & 0x03FF) + ((((uint)ch & 0x03FF) + 0x40) << 10);
                        else
                            emojiKey = ch;
                        if (rich.emojies.TryGetValue(emojiKey, out emoji))
                        {
                            HtmlElement imageElement = HtmlElement.GetElement(HtmlElementType.Image);
                            imageElement.Set("src", emoji.url);
                            if (emoji.width != 0)
                                imageElement.Set("width", emoji.width);
                            if (emoji.height != 0)
                                imageElement.Set("height", emoji.height);
                            if (highSurrogate)
                                imageElement.text = source.Substring(j, 2);
                            else
                                imageElement.text = source.Substring(j, 1);
                            imageElement.format.align = _textFormat.align;
                            _elements.Insert(++elementIndex, imageElement);

                            buffer.Append(source, appendPos, j - appendPos);
                            appendPos = j;
                            imageElement.charIndex = buffer.Length;
                        }
                    }

                    if (highSurrogate)
                    {
                        buffer.Append(source, appendPos, j - appendPos);
                        appendPos = j + 2;
                        j++;//跳过lowSurrogate
                        buffer.Append(' ');
                    }
                }
                j++;
            }
            if (appendPos < textLength)
                buffer.Append(source, appendPos, j - appendPos);

            return elementIndex;
        }

        public void UpdateMesh(bool beForce = false)
        {
            if (!_needUpdateMesh && !beForce)
                return;
            _meshs.Clear();

            if (_textWidth == 0 && _lines.Count == 1 || _typingEffectPos == 0)
            {
                if (_charPositions != null)
                {
                    _charPositions.Clear();
                    _charPositions.Add(new CharPosition());
                }
                (this as RichTextField)?.RefreshObjects();
                if (_typingEffectPos >= 0 && _textWidth == 0 && _lines.Count == 1)
                    _typingEffectPos = -1;
                _needUpdateMesh = false;
                return;
            }

            float letterSpacing = _textFormat.letterSpacing * _fontSizeScale;
            TextFormat format = _textFormat;
            _font.SetFormat(format, _fontSizeScale);

            float rectWidth = _size.X > 0 ? (_size.X - GUTTER_X * 2) : 0;
            float rectHeight = _size.Y > 0 ? Mathf.Max(_size.Y, _font.GetLineHeight(format.size)) : 0;

            if (_charPositions != null)
                _charPositions.Clear();

            HtmlLink currentLink = null;
            float linkStartX = 0;
            int linkStartLine = 0;

            float posx = 0;
            float indent_x;
            bool clipping = !(this is InputTextField) && (_autoSize == AutoSizeType.None || _autoSize == AutoSizeType.Ellipsis);
            bool lineClipped;
            AlignType lineAlign;
            float glyphWidth, glyphHeight, baseline;
            int charCount = 0;
            float underlineStart;
            float strikethroughStart;
            int minFontSize;
            int maxFontSize;
            string rtlLine = null;

            int elementIndex = 0;
            int elementCount = _elements.Count;
            HtmlElement element = null;
            if (elementCount > 0)
                element = _elements[elementIndex];

            int lineCount = _lines.Count;
            for (int i = 0; i < lineCount; ++i)
            {
                LineInfo line = _lines[i];
                if (line.charCount == 0)
                    continue;

                lineClipped = clipping && i != 0 && line.y + line.height > rectHeight;
                lineAlign = format.align;
                if (element != null && element.charIndex == line.charIndex)
                    lineAlign = element.format.align;
                else
                    lineAlign = format.align;

                if (_textDirection == RTLSupport.DirectionType.RTL)
                {
                    if (lineAlign == AlignType.Center)
                        indent_x = (int)((rectWidth + line.width) / 2);
                    else if (lineAlign == AlignType.Right)
                        indent_x = rectWidth;
                    else
                        indent_x = line.width + GUTTER_X * 2;

                    if (indent_x > rectWidth)
                        indent_x = rectWidth;

                    posx = indent_x - GUTTER_X;
                }
                else
                {
                    if (lineAlign == AlignType.Center)
                        indent_x = (int)((rectWidth - line.width) / 2);
                    else if (lineAlign == AlignType.Right)
                        indent_x = rectWidth - line.width;
                    else
                        indent_x = 0;

                    if (indent_x < 0)
                        indent_x = 0;

                    posx = GUTTER_X + indent_x;
                }

                int lineCharCount = line.charCount;
                underlineStart = posx;
                strikethroughStart = posx;
                minFontSize = maxFontSize = format.size;

                if (_textDirection != RTLSupport.DirectionType.UNKNOW)
                {
                    rtlLine = _parsedText.Substring(line.charIndex, lineCharCount);
                    if (_textDirection == RTLSupport.DirectionType.RTL)
                        rtlLine = RTLSupport.ConvertLineR(rtlLine);
                    else
                        rtlLine = RTLSupport.ConvertLineL(rtlLine);
                    lineCharCount = rtlLine.Length;
                }

                for (int j = 0; j < lineCharCount; j++)
                {
                    int charIndex = line.charIndex + j;
                    char ch = rtlLine != null ? rtlLine[j] : _parsedText[charIndex];
                    bool isEllipsis = charIndex == _ellipsisCharIndex;

                    while (element != null && charIndex == element.charIndex)
                    {
                        if (element.type == HtmlElementType.Text)
                        {
                            if (format.underline != element.format.underline)
                            {
                                if (format.underline)
                                {
                                    if (!lineClipped)
                                    {
                                        float lineWidth;
                                        if (_textDirection == RTLSupport.DirectionType.UNKNOW)
                                            lineWidth = (clipping ? Mathf.Clamp(posx, GUTTER_X, GUTTER_X + rectWidth) : posx) - underlineStart;
                                        else
                                            lineWidth = underlineStart - (clipping ? Mathf.Clamp(posx, GUTTER_X, GUTTER_X + rectWidth) : posx);
                                        if (lineWidth > 0)
                                            _font.DrawLine(_meshs, underlineStart < posx ? underlineStart : posx, line.y + line.baseline, lineWidth, maxFontSize, 0);
                                    }
                                    maxFontSize = 0;
                                }
                                else
                                    underlineStart = posx;
                            }

                            if (format.strikethrough != element.format.strikethrough)
                            {
                                if (format.strikethrough)
                                {
                                    if (!lineClipped)
                                    {
                                        float lineWidth;
                                        if (_textDirection == RTLSupport.DirectionType.UNKNOW)
                                            lineWidth = (clipping ? Mathf.Clamp(posx, GUTTER_X, GUTTER_X + rectWidth) : posx) - strikethroughStart;
                                        else
                                            lineWidth = strikethroughStart - (clipping ? Mathf.Clamp(posx, GUTTER_X, GUTTER_X + rectWidth) : posx);
                                        if (lineWidth > 0)
                                            _font.DrawLine(_meshs, strikethroughStart < posx ? strikethroughStart : posx, line.y + line.baseline, lineWidth, minFontSize, 1);
                                    }
                                    minFontSize = int.MaxValue;
                                }
                                else
                                    strikethroughStart = posx;
                            }

                            format = element.format;
                            minFontSize = Math.Min(minFontSize, format.size);
                            maxFontSize = Math.Max(maxFontSize, format.size);
                            _font.SetFormat(format, _fontSizeScale);
                        }
                        else if (element.type == HtmlElementType.Link)
                        {
                            currentLink = (HtmlLink)element.htmlObject;
                            if (currentLink != null)
                            {
                                element.position = Vector2.Zero;
                                currentLink.SetPosition(0, 0);
                                linkStartX = posx;
                                linkStartLine = i;
                            }
                        }
                        else if (element.type == HtmlElementType.LinkEnd)
                        {
                            if (currentLink != null)
                            {
                                currentLink.SetArea(linkStartLine, linkStartX, i, posx);
                                currentLink = null;
                            }
                        }
                        else
                        {
                            IHtmlObject htmlObj = element.htmlObject;
                            if (htmlObj != null)
                            {
                                if (_textDirection == RTLSupport.DirectionType.RTL)
                                    posx -= htmlObj.width - 2;

                                if (_typingEffectPos > 0 && charCount == _typingEffectPos)
                                    goto out_loop;

                                if (_charPositions != null)
                                {
                                    CharPosition cp = new CharPosition();
                                    cp.lineIndex = (short)i;
                                    cp.charIndex = charCount;
                                    cp.offsetX = posx;
                                    cp.width = (short)htmlObj.width;
                                    _charPositions.Add(cp);
                                }
                                charCount++;

                                if (isEllipsis || lineClipped || clipping && (posx < GUTTER_X || posx > GUTTER_X && posx + htmlObj.width > _size.X - GUTTER_X))
                                    element.status |= 1;
                                else
                                    element.status &= 254;
                                element.status &= 253;

                                element.position = new Vector2(posx + 1, line.y + line.baseline - htmlObj.height * IMAGE_BASELINE);
                                htmlObj.SetPosition(element.position.X, element.position.Y);

                                if (_textDirection == RTLSupport.DirectionType.RTL)
                                    posx -= letterSpacing;
                                else
                                    posx += htmlObj.width + letterSpacing + 2;
                            }
                        }

                        if (element.isEntity)
                            ch = '\0';

                        elementIndex++;
                        if (elementIndex < elementCount)
                            element = _elements[elementIndex];
                        else
                            element = null;
                    }

                    if (isEllipsis)
                        ch = '…';
                    else if (ch == '\0')
                        continue;

                    if (_font.GetGlyph(ch == '\t' ? ' ' : ch, out glyphWidth, out glyphHeight, out baseline))
                    {
                        if (ch == '\t')
                            glyphWidth *= 4;

                        if (!isEllipsis)
                        {
                            if (_textDirection == RTLSupport.DirectionType.RTL)
                            {
                                if (lineClipped || clipping && (rectWidth < 7 || posx != (indent_x - GUTTER_X)) && posx < GUTTER_X - 0.5f) //超出区域，剪裁
                                {
                                    posx -= (letterSpacing + glyphWidth);
                                    continue;
                                }

                                posx -= glyphWidth;
                            }
                            else
                            {
                                if (lineClipped || clipping && (rectWidth < 7 || posx != (GUTTER_X + indent_x)) && posx + glyphWidth > _size.X - GUTTER_X + 0.5f) //超出区域，剪裁
                                {
                                    posx += letterSpacing + glyphWidth;
                                    continue;
                                }
                            }
                        }

                        if (_typingEffectPos > 0 && charCount == _typingEffectPos)
                        {
                            if (char.IsWhiteSpace(ch))
                                _typingEffectPos++;
                            else
                                goto out_loop;
                        }

                        if (_charPositions != null)
                        {
                            CharPosition cp = new CharPosition();
                            cp.lineIndex = (short)i;
                            cp.charIndex = charCount;
                            cp.offsetX = posx;
                            cp.width = (short)glyphWidth;
                            _charPositions.Add(cp);
                        }
                        charCount++;

                        _font.DrawGlyph(_meshs, posx, line.y + line.baseline);

                        if (_textDirection == RTLSupport.DirectionType.RTL)
                            posx -= letterSpacing;
                        else
                            posx += letterSpacing + glyphWidth;
                    }
                    else //if GetGlyph failed
                    {
                        if (_typingEffectPos > 0 && charCount == _typingEffectPos)
                            _typingEffectPos++;

                        if (_charPositions != null)
                        {
                            CharPosition cp = new CharPosition();
                            cp.lineIndex = (short)i;
                            cp.charIndex = charCount;
                            cp.offsetX = posx;
                            _charPositions.Add(cp);
                        }
                        charCount++;

                        if (_textDirection == RTLSupport.DirectionType.RTL)
                            posx -= letterSpacing;
                        else
                            posx += letterSpacing;
                    }

                    if (isEllipsis)
                        lineClipped = true;
                }//text loop

                if (!lineClipped)
                {
                    if (format.underline)
                    {
                        float lineWidth;
                        if (_textDirection == RTLSupport.DirectionType.UNKNOW)
                            lineWidth = (clipping ? Mathf.Clamp(posx, GUTTER_X, GUTTER_X + rectWidth) : posx) - underlineStart;
                        else
                            lineWidth = underlineStart - (clipping ? Mathf.Clamp(posx, GUTTER_X, GUTTER_X + rectWidth) : posx);
                        if (lineWidth > 0)
                            _font.DrawLine(_meshs, underlineStart < posx ? underlineStart : posx, line.y + line.baseline, lineWidth, maxFontSize, 0);
                    }

                    if (format.strikethrough)
                    {
                        float lineWidth;
                        if (_textDirection == RTLSupport.DirectionType.UNKNOW)
                            lineWidth = (clipping ? Mathf.Clamp(posx, GUTTER_X, GUTTER_X + rectWidth) : posx) - strikethroughStart;
                        else
                            lineWidth = strikethroughStart - (clipping ? Mathf.Clamp(posx, GUTTER_X, GUTTER_X + rectWidth) : posx);
                        if (lineWidth > 0)
                            _font.DrawLine(_meshs, strikethroughStart < posx ? strikethroughStart : posx, line.y + line.baseline, lineWidth, minFontSize, 1);
                    }
                }

            }//line loop

            if (element != null && element.type == HtmlElementType.LinkEnd && currentLink != null)
                currentLink.SetArea(linkStartLine, linkStartX, lineCount - 1, posx);

            if (_charPositions != null)
            {
                CharPosition cp = new CharPosition();
                cp.lineIndex = (short)(lineCount - 1);
                cp.charIndex = charCount;
                cp.offsetX = posx;
                _charPositions.Add(cp);
            }
            charCount++;

        out_loop:

            if (_typingEffectPos > 0)
            {
                if (charCount == _typingEffectPos)
                    _typingEffectPos++;
                else
                    _typingEffectPos = -1;
            }

            // if (_font.customOutline)
            // {
            //     if (_textFormat.outline != 0)
            //         vb.GenerateOutline(UIConfig.enhancedTextOutlineEffect ? 8 : 4, _textFormat.outline, _textFormat.outlineColor);
            //     if (_textFormat.shadowOffset.X != 0 || _textFormat.shadowOffset.Y != 0)
            //         vb.GenerateShadow(_textFormat.shadowOffset, _textFormat.shadowColor);
            // }

            (this as RichTextField)?.RefreshObjects();
            _meshs.Finish();
            _needUpdateMesh = false;
        }

        void Cleanup()
        {
            (this as RichTextField)?.CleanupObjects();

            HtmlElement.ReturnElements(_elements);
            LineInfo.Return(_lines);
            _textWidth = 0;
            _textHeight = 0;
            _parsedText = string.Empty;
            _textDirection = RTLSupport.DirectionType.UNKNOW;

            if (_charPositions != null)
                _charPositions.Clear();
        }

        void ApplyVertAlign()
        {
            float oldOffset = _yOffset;
            if (_autoSize == AutoSizeType.Both || _autoSize == AutoSizeType.Height
                || _verticalAlign == VertAlignType.Top)
                _yOffset = 0;
            else
            {
                float dh;
                if (_textHeight == 0 && _lines.Count > 0)
                    dh = _size.Y - _lines[0].height;
                else
                    dh = _size.Y - _textHeight;
                if (dh < 0)
                    dh = 0;
                if (_verticalAlign == VertAlignType.Middle)
                    _yOffset = (int)(dh / 2);
                else
                    _yOffset = dh;
            }

            if (oldOffset != _yOffset)
            {
                int cnt = _lines.Count;
                for (int i = 0; i < cnt; i++)
                    _lines[i].y = _lines[i].y2 + _yOffset;

                _needUpdateMesh = true;
                QueueRedraw();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public class LineInfo
        {
            /// <summary>
            /// 行的宽度
            /// </summary>
            public float width;

            /// <summary>
            /// 行的高度
            /// </summary>
            public float height;

            /// <summary>
            /// 文字渲染基线
            /// </summary>
            public float baseline;

            /// <summary>
            /// 行首的字符索引
            /// </summary>
            public int charIndex;

            /// <summary>
            /// 行包括的字符个数
            /// </summary>
            public short charCount;

            /// <summary>
            /// 行的y轴位置
            /// </summary>
            public float y;

            /// <summary>
            /// 行的y轴位置的备份
            /// </summary>
            internal float y2;

            static Stack<LineInfo> pool = new Stack<LineInfo>();

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            public static LineInfo Borrow()
            {
                if (pool.Count > 0)
                {
                    LineInfo ret = pool.Pop();
                    ret.width = ret.height = ret.baseline = 0;
                    ret.y = ret.y2 = 0;
                    ret.charIndex = ret.charCount = 0;
                    return ret;
                }
                else
                    return new LineInfo();
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="value"></param>
            public static void Return(LineInfo value)
            {
                pool.Push(value);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="values"></param>
            public static void Return(List<LineInfo> values)
            {
                int cnt = values.Count;
                for (int i = 0; i < cnt; i++)
                    pool.Push(values[i]);

                values.Clear();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public struct LineCharInfo
        {
            public float width;
            public float height;
            public float baseline;
        }

        /// <summary>
        /// 
        /// </summary>
        public struct CharPosition
        {
            /// <summary>
            /// 字符索引
            /// </summary>
            public int charIndex;

            /// <summary>
            /// 字符所在的行索引
            /// </summary>
            public short lineIndex;

            /// <summary>
            /// 字符的x偏移
            /// </summary>
            public float offsetX;

            /// <summary>
            /// 字符的宽度
            /// </summary>
            public short width;
        }

        public class TextHightLightInfo
        {
            public int startCharIndex;
            public int endCharIndex;
            public Color color;
            public bool clipped;
            public List<Rect> rects;
            public bool HitTest(Vector2 localPoint)
            {
                if (rects != null)
                {
                    int count = rects.Count;
                    for (int i = 0; i < count; i++)
                        if (rects[i].Contains(localPoint))
                            return true;
                }
                return false;
            }
        }
    }
}
