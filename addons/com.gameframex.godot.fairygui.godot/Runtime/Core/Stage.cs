using System;
using Godot;
using System.Collections.Generic;


namespace FairyGUI
{
    /// <summary>
    /// 
    /// </summary>
    public partial class Stage : CanvasLayer
    {
        public float soundVolume { get; set; }

        public event Action beforeUpdate;
        public event Action afterUpdate;

        UpdateContext _updateContext;

        AudioStreamPlayer _audio;

        static List<EventBridge> sHelperChain = new List<EventBridge>();
        TouchInfo[] _touches;
        int _touchCount;
        List<GObject> _rollOutChain;
        List<GObject> _rollOverChain;
        public Vector2 touchPosition { get; private set; }
        bool haveTouchMonitorCount = false;
        public GObject touchTarget { get; private set; }

        /// <summary>
        /// 如果是true，表示使用屏幕上弹出的键盘输入文字。常见于移动设备。
        /// 如果是false，表示是接受按键消息输入文字。常见于PC。
        /// 一般来说，不需要设置，底层会自动根据系统环境设置正确的值。
        /// </summary>
        public static bool virtualKeyboardInput = false;

        GObject _focused;
        GTextInput _lastInput;
        List<GObject> _focusOutChain;
        List<GObject> _focusInChain;
        List<GComponent> _focusHistory;
        GComponent _nextFocus;

        DisplayServer.CursorShape _cursor = DisplayServer.CursorShape.Max;
        DisplayServer.CursorShape _currentCursor = DisplayServer.CursorShape.Max;

        public event System.Action<double> onUpdate;

        static bool _touchScreen = false;
        public static HashSet<GObject> allObject = new HashSet<GObject>();

        internal static int _clickTestThreshold = 10;

        static float _contentScaleFactor = -1;
        static int _contentScaleLevel = -1;

        static Stage _inst;
        /// <summary>
        /// 
        /// </summary>
        public static Stage inst
        {
            get
            {
                if (_inst == null)
                    Instantiate();

                return _inst;
            }
        }

        public static float contentScaleFactor
        {
            get
            {
                if (_contentScaleFactor < 0)
                    UpdateContextScale();
                return _contentScaleFactor;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public static int contentScaleLevel
        {
            get
            {
                if (_contentScaleLevel < 0)
                    UpdateContextScale();
                return _contentScaleLevel;
            }
        }

        static void UpdateContextScale()
        {
            int dx = ProjectSettings.GetSetting("display/window/size/viewport_width").AsInt32();
            int dy = ProjectSettings.GetSetting("display/window/size/viewport_height").AsInt32();
            string aspect = ProjectSettings.GetSetting("display/window/stretch/aspect").AsString();
            double scale = ProjectSettings.GetSetting("display/window/stretch/scale").AsDouble();
            string scale_mode = ProjectSettings.GetSetting("display/window/stretch/scale_mode").AsString();
            float screenWidth = Stage.width;
            float screenHeight = Stage.height;

            if (scale_mode == "integer")
                scale = Math.Floor(scale);

            if (screenWidth > screenHeight && dx < dy || screenWidth < screenHeight && dx > dy)
            {
                //scale should not change when orientation change
                int tmp = dx;
                dx = dy;
                dy = tmp;
            }

            if (aspect == "keep")
            {
                float s1 = (float)screenWidth / dx;
                float s2 = (float)screenHeight / dy;
                _contentScaleFactor = Mathf.Min(s1, s2);
            }
            else if (aspect == "keep_width")
                _contentScaleFactor = (float)screenWidth / dx;
            else
                _contentScaleFactor = (float)screenHeight / dy;

            _contentScaleFactor = (float)(_contentScaleFactor * scale);

            if (_contentScaleFactor > 3)
                _contentScaleLevel = 3; //x4
            else if (_contentScaleFactor > 2)
                _contentScaleLevel = 2; //x3
            else if (_contentScaleFactor > 1)
                _contentScaleLevel = 1; //x2
            else
                _contentScaleLevel = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        public static void Instantiate()
        {
            if (_inst == null)
            {
                _inst = new Stage();
                _inst.Layer = UIConfig.canvasLayer;
                GRoot._inst = new GRoot();
                _inst.AddChild(GRoot._inst.displayObject.node);
                _inst.AddChild(TweenManager.inst);
                _inst.AddChild(Timers.inst);
            }
        }

        /// <summary>
        /// As unity does not provide ways to detect this, you should set it by yourself. 
        /// This will effect:
        /// 1. compoistion cursor pos.
        /// 2. mouse wheel speed.
        /// </summary>
        public static float devicePixelRatio
        {
            get; set;
        }

        /// <summary>
        /// The scale of the mouse scroll delta.
        /// </summary>


        public static bool touchScreen { get { return _touchScreen; } }
        public static int width { get { return DisplayServer.WindowGetSize().X; } }
        public static int height { get { return DisplayServer.WindowGetSize().Y; } }
        public Stage() : base()
        {
            _inst = this;
            var root = Engine.GetMainLoop() as SceneTree;
            if (root != null)
            {
                root.Root.CallDeferred(Node.MethodName.AddChild, this);
            }
            else
            {
                GD.PushError("can not get root node");
            }
            soundVolume = 1;

            _updateContext = new UpdateContext();

            _touches = new TouchInfo[5];
            for (int i = 0; i < _touches.Length; i++)
                _touches[i] = new TouchInfo();

            _rollOutChain = new List<GObject>();
            _rollOverChain = new List<GObject>();
            _focusOutChain = new List<GObject>();
            _focusInChain = new List<GObject>();
            _focusHistory = new List<GComponent>();

            // 在PC上，是否retina屏对输入法位置，鼠标滚轮速度都有影响，但现在没发现Unity有获得的方式。仅判断是否Mac可能不够（外接显示器的情况）。所以最好自行设置。
            devicePixelRatio = (OS.GetName() == "macOS" && DisplayServer.ScreenGetDpi() > 96) ? 2 : 1;

            EnableSound();
        }

        public override void _Ready()
        {
            var tree = Engine.GetMainLoop() as SceneTree;
            tree.Root.TreeExiting += OnApplicationQuit;
            GetViewport().SizeChanged += HandleScreenSizeChanged;
            GD.Print("stage inited");
        }

        void OnApplicationQuit()
        {
            GD.Print("stage released");
            GRoot.Release();
            FontManager.inst.Clear();
            foreach (var obj in allObject)
            {
                obj.Dispose();
            }
            QueueFree();
        }

        public void EnableSound()
        {
            if (_audio == null)
            {
                _audio = new AudioStreamPlayer();
                AddChild(_audio);
            }
        }
        public void DisableSound()
        {
            if (_audio != null)
            {
                _audio.QueueFree();
                _audio = null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="volumeScale"></param>
        public void PlayOneShotSound(AudioStream clip, float volumeScale = 1)
        {
            if (_audio != null && this.soundVolume > 0)
            {
                _audio.Stream = clip;
                _audio.VolumeDb = 20f * MathF.Log10(volumeScale);
                _audio.Play();
            }
        }

        public override void _Process(double delta)
        {
            if (_nextFocus != null)
            {
                if (_nextFocus.RootToLocal != null)
                {
                    if (_nextFocus.tabStopChildren)
                    {
                        if (_nextFocus._lastFocus != null && _nextFocus.IsAncestorOf(_nextFocus._lastFocus))
                            SetFocus(_nextFocus._lastFocus);
                        else
                            SetFocus(_nextFocus);
                    }
                    else
                        SetFocus(_nextFocus);
                }
                _nextFocus = null;
            }

            if (beforeUpdate != null)
                beforeUpdate();

            if (onUpdate != null)
                onUpdate(delta);

            _updateContext.Begin();

            _updateContext.End();

            if (afterUpdate != null)
                afterUpdate();
        }

        internal void HandleScreenSizeChanged()
        {
            UpdateContextScale();
            if (!GRoot.inst.DispatchEvent("onStageResized", null))
            {
            }
        }

        public override void _Input(Godot.InputEvent evt)
        {
            if (evt is InputEventMouseButton mb)
            {
                GObject target = GRoot.inst.HitTest(mb.Position);
                if (target != null)
                    HandleMouseButtonEvents(mb, target);
            }
            else if (evt is InputEventMouseMotion mm)
            {
                GObject target = GRoot.inst.HitTest(mm.Position);
                if (target != null)
                    HandleMouseMoveEvents(mm, target);
            }
            else if (evt is InputEventScreenTouch touch)
            {
                _touchScreen = true;
                GObject target = GRoot.inst.HitTest(touch.Position);
                if (target != null)
                    HandleTouchEvents(touch, target);
            }
            else if (evt is InputEventScreenDrag tv)
            {
                _touchScreen = true;
                GObject target = GRoot.inst.HitTest(tv.Position);
                if (target != null)
                    HandleTouchMoveEvents(tv, target);
            }
            else if (evt is InputEventKey)
            {
                HandleKeyEvents(evt as InputEventKey, focus);
            }
        }

        void HandleKeyEvents(InputEventKey evt, GObject sender)
        {
            TouchInfo touch = _touches[0];
            touch.target = sender;
            touch.keyCode = evt.Keycode;
            touch.echo = evt.Echo;
            touch.keyModifiers = evt.GetModifiersMask();
            touch.character = evt.Unicode;
            touch.UpdateEvent();
            if (evt.Pressed)
            {
                if (sender != null)
                    sender.BubbleEvent("onKeyDown", touch.evt);
                else
                    GRoot.inst.DispatchEvent("onKeyDown", touch.evt);
            }
            else
            {
                if (sender != null)
                    sender.BubbleEvent("onKeyUp", touch.evt);
                else
                    GRoot.inst.DispatchEvent("onKeyUp", touch.evt);
            }
        }

        void HandleMouseButtonEvents(InputEventMouseButton evt, GObject sender)
        {
            TouchInfo touch = _touches[0];
            touch.target = sender;
            touchTarget = sender;
            touchPosition = evt.Position;
            touch.x = touchPosition.X;
            touch.y = touchPosition.Y;
            touch.touchId = 0;

            if (evt.Pressed && evt.ButtonIndex != MouseButton.WheelUp && evt.ButtonIndex != MouseButton.WheelDown)
                SetFocus(touch.target);

            switch (evt.ButtonIndex)
            {
                case MouseButton.Left:
                case MouseButton.Right:
                    if (evt.Pressed)
                    {
                        if (!touch.began)
                        {
                            _touchCount = 1;
                            touch.Begin();
                            touch.button = evt.ButtonIndex;
                            touch.UpdateEvent();
                            touch.target.BubbleEvent("onTouchBegin", touch.evt);
                        }
                    }
                    else
                    {
                        if (touch.began)
                        {
                            _touchCount = 0;
                            touch.End();
                            GObject clickTarget = touch.ClickTest();
                            if (clickTarget != null)
                            {
                                touch.UpdateEvent();

                                if (evt.ButtonIndex == MouseButton.Right)
                                    clickTarget.BubbleEvent("onRightClick", touch.evt);
                                else
                                    clickTarget.BubbleEvent("onClick", touch.evt);
                            }

                            touch.button = MouseButton.None;
                        }
                        touch.touchId = -1;
                    }
                    break;
                case MouseButton.WheelUp:
                    if (evt.Pressed)
                    {
                        touch.mouseWheelDelta = -evt.Factor * UIConfig.mouseWheelScale;
                        touch.UpdateEvent();
                        touch.target.BubbleEvent("onMouseWheel", touch.evt);
                        touch.mouseWheelDelta = 0;
                    }
                    else
                    {
                        touch.touchId = -1;
                    }
                    break;
                case MouseButton.WheelDown:
                    if (evt.Pressed)
                    {
                        touch.mouseWheelDelta = evt.Factor * UIConfig.mouseWheelScale;
                        touch.UpdateEvent();
                        touch.target.BubbleEvent("onMouseWheel", touch.evt);
                        touch.mouseWheelDelta = 0;
                    }
                    else
                    {
                        touch.touchId = -1;
                    }
                    break;
                default:
                    touch.touchId = 0;
                    break;
            }
        }

        void HandleMouseMoveEvents(InputEventMouseMotion evt, GObject sender)
        {
            TouchInfo touch = _touches[0];
            touch.target = sender;
            touchTarget = sender;
            touchPosition = evt.Position;
            if (touch.x != touchPosition.X || touch.y != touchPosition.X)
            {
                touch.x = touchPosition.X;
                touch.y = touchPosition.Y;
                touch.Move();
            }

            if (touch.lastRollOver != touch.target)
                HandleRollOver(touch);
        }

        void HandleTouchEvents(InputEventScreenTouch evt, GObject sender)
        {
            if (evt.Index >= 5)
                return;
            TouchInfo touch = _touches[evt.Index];
            touch.target = sender;
            touchTarget = sender;
            touch.x = evt.Position.X;
            touch.y = evt.Position.Y;
            touch.touchId = evt.Index;
            touchPosition = evt.Position;
            if (evt.Pressed)
            {
                SetFocus(touch.target);
                if (!touch.began)
                {
                    touch.Begin();
                    touch.button = 0;
                    touch.UpdateEvent();
                    touch.target.BubbleEvent("onTouchBegin", touch.evt);
                }
            }
            else
            {
                if (touch.began)
                {
                    touch.End();

                    if (!evt.Canceled)
                    {
                        if (sender != null)
                        {
                            //touch.clickCount = evt.DoubleTap ? 2 : 1;
                            touch.UpdateEvent();
                            sender.BubbleEvent("onClick", touch.evt);
                        }
                    }

                    touch.target = null;
                    touch.touchId = -1;
                }
            }
            _touchCount = 0;
            foreach (var info in _touches)
            {
                if (info.began)
                    _touchCount++;
            }
        }

        void HandleTouchMoveEvents(InputEventScreenDrag evt, GObject sender)
        {
            if (evt.Index >= 5)
                return;
            TouchInfo touch = _touches[evt.Index];
            touch.target = sender;
            touchTarget = sender;
            touchPosition = evt.Position;
            if (touch.x != evt.Position.X || touch.y != evt.Position.Y)
            {
                touch.x = evt.Position.X;
                touch.y = evt.Position.Y;
                if (touch.began)
                    touch.Move();
            }
            if (touch.lastRollOver != touch.target)
                HandleRollOver(touch);
        }


        void HandleRollOver(TouchInfo touch)
        {
            GObject element;
            _rollOverChain.Clear();
            _rollOutChain.Clear();

            element = touch.lastRollOver;
            while (element != null)
            {
                _rollOutChain.Add(element);
                element = element.parent;
            }

            touch.lastRollOver = touch.target;

            DisplayServer.CursorShape cursor = _cursor;
            if (cursor == DisplayServer.CursorShape.Max)
            {
                element = touch.target;
                while (element != null)
                {
                    if (element.cursor != DisplayServer.CursorShape.Max && cursor == DisplayServer.CursorShape.Max)
                        cursor = element.cursor;
                    element = element.parent;
                }
            }

            element = touch.target;
            int i;
            while (element != null)
            {
                i = _rollOutChain.IndexOf(element);
                if (i != -1)
                {
                    _rollOutChain.RemoveRange(i, _rollOutChain.Count - i);
                    break;
                }
                _rollOverChain.Add(element);

                element = element.parent;
            }

            int cnt = _rollOutChain.Count;
            if (cnt > 0)
            {
                for (i = 0; i < cnt; i++)
                {
                    element = _rollOutChain[i];
                    if (element.realRoot != null)
                        element.DispatchEvent("onRollOut", touch.evt);
                }
                _rollOutChain.Clear();
            }

            cnt = _rollOverChain.Count;
            if (cnt > 0)
            {
                for (i = 0; i < cnt; i++)
                {
                    element = _rollOverChain[i];
                    if (element.realRoot != null)
                        element.DispatchEvent("onRollOver", touch.evt);
                }
                _rollOverChain.Clear();
            }
            if (cursor != _currentCursor)
                _ChangeCursor(cursor);
        }
        public void AddTouchMonitor(int touchId, EventDispatcher target)
        {
            TouchInfo touch = null;
            for (int j = 0; j < 5; j++)
            {
                touch = _touches[j];
                if (touchId == -1 && touch.touchId != -1
                    || touchId != -1 && touch.touchId == touchId)
                    break;
            }
            if (touch.touchMonitors.IndexOf(target) == -1)
                touch.touchMonitors.Add(target);
            haveTouchMonitorCount = true;
        }

        public void RemoveTouchMonitor(EventDispatcher target)
        {
            haveTouchMonitorCount = false;
            for (int j = 0; j < 5; j++)
            {
                TouchInfo touch = _touches[j];
                int i = touch.touchMonitors.IndexOf(target);
                if (i != -1)
                    touch.touchMonitors[i] = null;
                for (i = 0; i < touch.touchMonitors.Count; i++)
                {
                    if (touch.touchMonitors[i] != null)
                        haveTouchMonitorCount = true;
                }
            }
        }

        public bool IsTouchMonitoring(EventDispatcher target)
        {
            for (int j = 0; j < 5; j++)
            {
                TouchInfo touch = _touches[j];
                int i = touch.touchMonitors.IndexOf(target);
                if (i != -1)
                    return true;
            }

            return false;
        }
        public void ResetInputState()
        {
            for (int j = 0; j < 5; j++)
                _touches[j].Reset();

            if (!_touchScreen)
                _touches[0].touchId = 0;
        }
        public void CancelClick(int touchId)
        {
            for (int j = 0; j < 5; j++)
            {
                TouchInfo touch = _touches[j];
                if (touch.touchId == touchId)
                    touch.clickCancelled = true;
            }
        }
        public Vector2 GetTouchPosition(int touchId)
        {
            if (touchId < 0)
                return touchPosition;

            for (int j = 0; j < 5; j++)
            {
                TouchInfo touch = _touches[j];
                if (touch.touchId == touchId)
                    return new Vector2(touch.x, touch.y);
            }

            return touchPosition;
        }

        public int touchCount
        {
            get { return _touchCount; }
        }

        public int[] GetAllTouch(int[] result)
        {
            if (result == null)
                result = new int[_touchCount];
            int i = 0;
            for (int j = 0; j < 5; j++)
            {
                TouchInfo touch = _touches[j];
                if (touch.touchId != -1)
                {
                    result[i++] = touch.touchId;
                    if (i >= result.Length)
                        break;
                }
            }
            return result;
        }

        public GObject focus
        {
            get
            {
                if (_focused != null && _focused.isDisposed)
                    _focused = null;
                return _focused;
            }
            set
            {
                SetFocus(value);
            }
        }

        public void SetFocus(GObject newFocus, bool byKey = false)
        {
            if (newFocus == GRoot.inst)
                newFocus = null;

            _nextFocus = null;

            if (_focused == newFocus)
                return;

            GComponent navRoot = null;
            GObject element = newFocus;
            while (element != null)
            {
                if (!element.focusable)
                    return;
                else if ((element is GComponent) && ((GComponent)element).tabStopChildren)
                {
                    if (navRoot == null)
                        navRoot = element as GComponent;
                }

                element = element.parent;
            }

            GObject oldFocus = _focused;
            _focused = newFocus;

            if (navRoot != null)
            {
                navRoot._lastFocus = _focused;
                int pos = _focusHistory.IndexOf(navRoot);
                if (pos != -1)
                {
                    if (pos < _focusHistory.Count - 1)
                        _focusHistory.RemoveRange(pos + 1, _focusHistory.Count - pos - 1);
                }
                else
                {
                    _focusHistory.Add(navRoot);
                    if (_focusHistory.Count > 10)
                        _focusHistory.RemoveAt(0);
                }
            }

            _focusInChain.Clear();
            _focusOutChain.Clear();

            element = oldFocus;
            while (element != null)
            {
                if (element.focusable)
                    _focusOutChain.Add(element);
                element = element.parent;
            }

            element = _focused;
            int i;
            while (element != null)
            {
                i = _focusOutChain.IndexOf(element);
                if (i != -1)
                {
                    _focusOutChain.RemoveRange(i, _focusOutChain.Count - i);
                    break;
                }
                if (element.focusable)
                    _focusInChain.Add(element);

                element = element.parent;
            }

            int cnt = _focusOutChain.Count;
            if (cnt > 0)
            {
                for (i = 0; i < cnt; i++)
                {
                    element = _focusOutChain[i];
                    if (element.realRoot != null)
                    {
                        element.DispatchEvent("onFocusOut", null);
                        if (_focused != newFocus) //focus changed in event
                            return;
                    }
                }
                _focusOutChain.Clear();
            }

            cnt = _focusInChain.Count;
            if (cnt > 0)
            {
                for (i = 0; i < cnt; i++)
                {
                    element = _focusInChain[i];
                    if (element.realRoot != null)
                    {
                        element.DispatchEvent("onFocusIn", byKey ? "key" : null);
                        if (_focused != newFocus) //focus changed in event
                            return;
                    }
                }
                _focusInChain.Clear();
            }

            if (_focused is GTextInput)
                _lastInput = (GTextInput)_focused;
        }

        internal void _OnFocusRemoving(GComponent sender)
        {
            _nextFocus = sender;
            if (_focusHistory.Count > 0)
            {
                int i = _focusHistory.Count - 1;
                GObject test = _focusHistory[i];
                GObject element = _focused;
                while (element != null && element != sender)
                {
                    if ((element is GComponent) && ((GComponent)element).tabStopChildren && element == test)
                    {
                        i--;
                        if (i < 0)
                            break;

                        test = _focusHistory[i];
                    }

                    element = element.parent;
                }

                if (i != _focusHistory.Count - 1)
                {
                    _focusHistory.RemoveRange(i + 1, _focusHistory.Count - i - 1);
                    if (_focusHistory.Count > 0)
                        _nextFocus = _focusHistory[_focusHistory.Count - 1];
                }
            }

            if (_focused is GTextInput)
                _lastInput = null;
            _focused = null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="backward"></param>
        public void DoKeyNavigate(bool backward)
        {
            GComponent navBase = null;
            GObject element = _focused;
            while (element != null)
            {
                if ((element is GComponent) && ((GComponent)element).tabStopChildren)
                {
                    navBase = element as GComponent;
                    break;
                }

                element = element.parent;
            }

            if (navBase == null)
                navBase = GRoot.inst;

            var it = navBase.GetDescendants(backward);
            bool started = _focused == null;
            GObject test2 = _focused != null ? _focused.parent : null;

            while (it.MoveNext())
            {
                GObject dobj = it.Current;
                if (started)
                {
                    if (dobj == test2)
                        test2 = test2.parent;
                    else if (dobj._AcceptTab())
                        return;
                }
                else if (dobj == _focused)
                    started = true;
            }

            if (started)
            {
                it.Reset();
                while (it.MoveNext())
                {
                    GObject dobj = it.Current;
                    if (dobj == _focused)
                        break;

                    if (dobj == test2)
                        test2 = test2.parent;
                    else if (dobj._AcceptTab())
                        return;
                }
            }
        }

        public DisplayServer.CursorShape cursor
        {
            get { return _cursor; }
            set { _cursor = value; }
        }
        public DisplayServer.CursorShape activeCursor
        {
            get { return _currentCursor; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cursorName"></param>
        internal void _ChangeCursor(DisplayServer.CursorShape csr)
        {
            if (csr != DisplayServer.CursorShape.Max)
                DisplayServer.CursorSetShape(csr);
            else
                DisplayServer.CursorSetShape(DisplayServer.CursorShape.Arrow);
        }
    }

    class TouchInfo
    {
        public float x;
        public float y;
        public int touchId;
        public int clickCount;
        public Key keyCode;
        public bool echo;
        public long character;
        public KeyModifierMask keyModifiers;
        public MouseButtonMask mouseModifiers;
        public float mouseWheelDelta;
        public MouseButton button;

        public float downX;
        public float downY;
        public float downTime;
        public int downFrame;
        public bool began;
        public bool clickCancelled;
        public float lastClickTime;
        public float lastClickX;
        public float lastClickY;
        public MouseButton lastClickButton;
        public float holdTime;
        public GObject target;
        public List<GObject> downTargets;
        public GObject lastRollOver;
        public List<EventDispatcher> touchMonitors;

        public NInputEvent evt;

        static List<EventBridge> sHelperChain = new List<EventBridge>();

        public TouchInfo()
        {
            evt = new NInputEvent();
            downTargets = new List<GObject>();
            touchMonitors = new List<EventDispatcher>();
            Reset();
        }

        public void Reset()
        {
            touchId = -1;
            x = 0;
            y = 0;
            clickCount = 0;
            button = MouseButton.None;
            keyCode = Key.None;
            echo = false;
            character = '\0';
            keyModifiers = 0;
            mouseModifiers = 0;
            mouseWheelDelta = 0;
            lastClickTime = 0;
            began = false;
            target = null;
            downTargets.Clear();
            lastRollOver = null;
            clickCancelled = false;
            touchMonitors.Clear();
        }

        public void UpdateEvent()
        {
            evt.touchId = this.touchId;
            evt.x = this.x;
            evt.y = this.y;
            evt.clickCount = this.clickCount;
            evt.keyCode = this.keyCode;
            evt.echo = this.echo;
            evt.character = (char)this.character;
            evt.keyModifiers = this.keyModifiers;
            evt.mouseModifiers = this.mouseModifiers;
            evt.mouseWheelDelta = this.mouseWheelDelta;
            evt.button = this.button;
            evt.holdTime = this.holdTime;
        }

        public void Begin()
        {
            began = true;
            clickCancelled = false;
            downX = x;
            downY = y;
            downTime = Time.GetTicksMsec() / 1000.0f;
            downFrame = Engine.GetFramesDrawn();
            holdTime = 0;

            downTargets.Clear();
            if (target != null)
            {
                downTargets.Add(target);
                GObject obj = target;
                while (obj != null)
                {
                    downTargets.Add(obj);
                    obj = obj.parent;
                }
            }
        }

        public void Move()
        {

            if (began)
                holdTime = (Engine.GetFramesDrawn() - downFrame) == 1 ? (1.0f / (float)Engine.GetFramesPerSecond()) : (Time.GetTicksMsec() / 1000.0f - downTime);

            UpdateEvent();

            if (Mathf.Abs(x - downX) > 50 || Mathf.Abs(y - downY) > 50) clickCancelled = true;

            if (touchMonitors.Count > 0)
            {
                int len = touchMonitors.Count;
                for (int i = 0; i < len; i++)
                {
                    EventDispatcher e = touchMonitors[i];
                    if (e != null)
                    {
                        if ((e is GObject) && !((GObject)e).onStage)
                            continue;
                        e.GetChainBridges("onTouchMove", sHelperChain, false);
                    }
                }

                GRoot.inst.BubbleEvent("onTouchMove", evt, sHelperChain);
                sHelperChain.Clear();
            }
            else
                GRoot.inst.DispatchEvent("onTouchMove", evt);
        }

        public void End()
        {
            began = false;
            float CurTime = Time.GetTicksMsec() / 1000.0f;
            if (downTargets.Count == 0
                || clickCancelled
                || Mathf.Abs(x - downX) > Stage._clickTestThreshold
                || Mathf.Abs(y - downY) > Stage._clickTestThreshold)
            {
                clickCancelled = true;
                lastClickTime = 0;
                clickCount = 1;
            }
            else
            {

                if (CurTime - lastClickTime < 0.35f
                    && Mathf.Abs(x - lastClickX) < Stage._clickTestThreshold
                    && Mathf.Abs(y - lastClickY) < Stage._clickTestThreshold
                    && lastClickButton == button)
                {
                    if (clickCount == 2)
                        clickCount = 1;
                    else
                        clickCount++;
                }
                else
                    clickCount = 1;
                lastClickTime = CurTime;
                lastClickX = x;
                lastClickY = y;
                lastClickButton = button;
            }

            //当间隔一帧时，使用帧率计算时间，避免掉帧因素
            holdTime = (Engine.GetFramesDrawn() - downFrame) == 1 ? (1f / (float)Engine.GetFramesPerSecond()) : (CurTime - downTime);
            UpdateEvent();

            if (touchMonitors.Count > 0)
            {
                int len = touchMonitors.Count;
                for (int i = 0; i < len; i++)
                {
                    EventDispatcher e = touchMonitors[i];
                    if (e != null)
                        e.GetChainBridges("onTouchEnd", sHelperChain, false);
                }
                target.BubbleEvent("onTouchEnd", evt, sHelperChain);

                touchMonitors.Clear();
                sHelperChain.Clear();
            }
            else
                target.BubbleEvent("onTouchEnd", evt);
        }

        public GObject ClickTest()
        {
            if (clickCancelled)
            {
                downTargets.Clear();
                return null;
            }

            GObject obj = downTargets[0];
            if (obj.realRoot != null) //依然派发到原来的downTarget，虽然可能它已经偏离当前位置，主要是为了正确处理点击缩放的效果
            {
                downTargets.Clear();
                return obj;
            }

            obj = target;
            while (obj != null)
            {
                int i = downTargets.IndexOf(obj);
                if (i != -1 && obj.realRoot != null)
                    break;

                obj = obj.parent;
            }

            downTargets.Clear();

            return obj;
        }
    }
}
