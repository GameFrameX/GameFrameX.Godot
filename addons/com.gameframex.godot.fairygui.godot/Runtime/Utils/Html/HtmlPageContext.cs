using System.Collections.Generic;
using Godot;

namespace FairyGUI.Utils
{
    /// <summary>
    /// 
    /// </summary>
    public class HtmlPageContext : IHtmlPageContext
    {
        Stack<IHtmlObject> _imagePool;
        Stack<IHtmlObject> _inputPool;
        Stack<IHtmlObject> _buttonPool;
        Stack<IHtmlObject> _selectPool;
        Stack<IHtmlObject> _linkPool;

        static HtmlPageContext _inst = null;

        public static HtmlPageContext inst
        {
            get
            {
                if (_inst == null)
                    _inst = new HtmlPageContext();
                return _inst;
            }
        }

        public HtmlPageContext()
        {
            _imagePool = new Stack<IHtmlObject>();
            _inputPool = new Stack<IHtmlObject>();
            _buttonPool = new Stack<IHtmlObject>();
            _selectPool = new Stack<IHtmlObject>();
            _linkPool = new Stack<IHtmlObject>();
        }

        virtual public IHtmlObject CreateObject(RichTextField owner, HtmlElement element)
        {
            IHtmlObject ret = null;
            bool fromPool = false;
            if (element.type == HtmlElementType.Image)
            {
                if (_imagePool.Count > 0)
                {
                    ret = _imagePool.Pop();
                    fromPool = true;
                }
                else
                    ret = new HtmlImage();
            }
            else if (element.type == HtmlElementType.Link)
            {
                if (_linkPool.Count > 0)
                {
                    ret = _linkPool.Pop();
                    fromPool = true;
                }
                else
                    ret = new HtmlLink();
            }
            else if (element.type == HtmlElementType.Input)
            {
                string type = element.GetString("type");
                if (type != null)
                    type = type.ToLower();
                if (type == "button" || type == "submit")
                {
                    if (_buttonPool.Count > 0)
                    {
                        ret = _buttonPool.Pop();
                        fromPool = true;
                    }
                    else
                    {
                        if (HtmlButton.resource != null)
                            ret = new HtmlButton();
                        else
                            GD.PushWarning("FairyGUI: Set HtmlButton.resource first");
                    }
                }
                else
                {
                    if (_inputPool.Count > 0)
                    {
                        ret = _inputPool.Pop();
                        fromPool = true;
                    }
                    else
                        ret = new HtmlInput();
                }
            }
            else if (element.type == HtmlElementType.Select)
            {
                if (_selectPool.Count > 0)
                {
                    ret = _selectPool.Pop();
                    fromPool = true;
                }
                else
                {
                    if (HtmlSelect.resource != null)
                        ret = new HtmlSelect();
                    else
                        GD.PushWarning("FairyGUI: Set HtmlSelect.resource first");
                }
            }

            //Debug.Log("from=" + fromPool);
            if (ret != null)
            {
                //可能已经被销毁了，不再使用
                if (fromPool && ret.displayObject != null && (!GodotObject.IsInstanceValid(ret.displayObject.node) || ret.displayObject.node.IsQueuedForDeletion()))
                {
                    ret.Dispose();
                    return CreateObject(owner, element);

                }
                ret.Create(owner, element);
            }

            return ret;
        }

        virtual public void FreeObject(IHtmlObject obj)
        {
            //可能已经被销毁了，不再回收
            if (obj.displayObject != null && (!GodotObject.IsInstanceValid(obj.displayObject.node) || obj.displayObject.node.IsQueuedForDeletion()))
            {
                obj.Dispose();
                return;
            }

            obj.Release();
            if (obj is HtmlImage)
                _imagePool.Push(obj);
            else if (obj is HtmlInput)
                _inputPool.Push(obj);
            else if (obj is HtmlButton)
                _buttonPool.Push(obj);
            else if (obj is HtmlLink)
                _linkPool.Push(obj);
        }

        virtual public NTexture GetImageTexture(HtmlImage image)
        {
            return null;
        }

        virtual public void FreeImageTexture(HtmlImage image, NTexture texture)
        {
        }
    }
}
