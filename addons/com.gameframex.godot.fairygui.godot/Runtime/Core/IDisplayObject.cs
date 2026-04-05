using Godot;
using System;

namespace FairyGUI
{
    public interface IDisplayObject
    {
        GObject gOwner { get; set; }
        IDisplayObject parent { get; }
        CanvasItem node { get; }
        bool visible { get; set; }
        Vector2 skew { get; set; }
        float skewX { get; set; }
        float skewY { get; set; }
        Vector2 position { get; set; }
        float X { get; set; }
        float Y { get; set; }
        void SetXY(float x, float y);
        void SetPosition(Vector2 pos);
        Vector2 size { get; set; }
        public float width { get; set; }
        public float height { get; set; }
        void SetSize(float w, float h);
        void SetSize(Vector2 size);
        Vector2 pivot { get; set; }
        Vector2 scale { get; set; }
        float rotation { get; set; }
        BlendMode blendMode { get; set; }
    }
}