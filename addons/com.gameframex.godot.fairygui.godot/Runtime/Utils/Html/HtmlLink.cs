using Godot;
namespace FairyGUI.Utils
{
    /// <summary>
    /// 
    /// </summary>
    public class HtmlLink : IHtmlObject
    {
        RichTextField _owner;
        HtmlElement _element;
        TextField.TextHightLightInfo _bgHightLight = new TextField.TextHightLightInfo();
        bool _bgEnable = false;
        EventCallback1 _clickHandler;
        EventCallback1 _rolloverHandler;
        EventCallback0 _rolloutHandler;
        bool haveRollIn = false;
        bool checkTouchMove = false;
        DisplayServer.CursorShape _savedCursorShape;

        public HtmlLink()
        {
            _clickHandler = (EventContext context) =>
            {
                _owner.gOwner.BubbleEvent("onClickLink", _element.GetString("href"));
            };
            _rolloverHandler = (EventContext context) =>
            {
                _savedCursorShape = _owner.gOwner.cursor;
                _owner.gOwner.cursor = DisplayServer.CursorShape.PointingHand;
                if (_bgEnable)
                {
                    _bgHightLight.color = _owner.htmlParseOptions.linkHoverBgColor;
                    _owner.QueueRedraw();
                }

            };
            _rolloutHandler = () =>
            {
                _owner.gOwner.cursor = _savedCursorShape;
                if (_bgEnable)
                {
                    _bgHightLight.color = _owner.htmlParseOptions.linkBgColor;
                    _owner.QueueRedraw();
                }
            };
        }

        public IDisplayObject displayObject
        {
            get { return null; }
        }

        public HtmlElement element
        {
            get { return _element; }
        }

        public float width
        {
            get { return 0; }
        }

        public float height
        {
            get { return 0; }
        }

        public void Create(RichTextField owner, HtmlElement element)
        {
            _owner = owner;
            _element = element;
            _owner.gOwner.onClick.Add(ClickHandler);
            _owner.gOwner.onRollOver.Add(RolloverHandler);
            GRoot.inst.onTouchMove.Add(TouchMoveHandler);
            _owner.gOwner.onRollOut.Add(RolloutHandler);
            _bgHightLight.color = _owner.htmlParseOptions.linkBgColor;
            _bgEnable = _owner.htmlParseOptions.linkHoverBgColor.A > 0 || _owner.htmlParseOptions.linkBgColor.A > 0;
        }
        void ClickHandler(EventContext context)
        {
            if (_clickHandler != null)
            {
                Vector2 pos = _owner.MakeCanvasPositionLocal(context.inputEvent.position);
                if (_bgHightLight.HitTest(pos))
                    _clickHandler(context);
            }

        }
        void RolloverHandler(EventContext context)
        {
            if (_rolloverHandler != null)
            {
                checkTouchMove = true;
            }
        }
        void TouchMoveHandler(EventContext context)
        {
            if (checkTouchMove)
            {
                Vector2 pos = _owner.MakeCanvasPositionLocal(context.inputEvent.position);
                if (_bgHightLight.HitTest(pos))
                {
                    if (!haveRollIn)
                    {
                        _rolloverHandler(context);
                        haveRollIn = true;
                    }
                }
                else if (haveRollIn)
                {
                    _rolloutHandler();
                    haveRollIn = false;
                }
            }
        }
        void RolloutHandler()
        {
            checkTouchMove = false;
            if (_rolloutHandler != null && haveRollIn)
            {
                _rolloutHandler();
                haveRollIn = false;
            }
        }

        public void SetArea(int startLine, float startCharX, int endLine, float endCharX)
        {
            if (startLine == endLine && startCharX > endCharX)
            {
                float tmp = startCharX;
                startCharX = endCharX;
                endCharX = tmp;
            }
            if (_bgHightLight.rects == null)
                _bgHightLight.rects = new System.Collections.Generic.List<Rect>();
            else
                _bgHightLight.rects.Clear();
            _owner.GetLinesShape(startLine, startCharX, endLine, endCharX, true, _bgHightLight.rects);
            if (_bgEnable)
                _owner.AddTextHightLight(_bgHightLight);
        }

        public void SetPosition(float x, float y)
        {
        }

        public void Add()
        {
        }

        public void Remove()
        {
            _owner.RemoveTextHightLight(_bgHightLight);
        }

        public void Release()
        {
            _owner.gOwner.onClick.Remove(ClickHandler);
            _owner.gOwner.onRollOver.Remove(RolloverHandler);
            GRoot.inst.onTouchMove.Remove(TouchMoveHandler);
            _owner.gOwner.onRollOut.Remove(RolloutHandler);

            _owner = null;
            _element = null;
        }

        public void Dispose()
        {
            _owner.RemoveTextHightLight(_bgHightLight);
        }
    }
}

