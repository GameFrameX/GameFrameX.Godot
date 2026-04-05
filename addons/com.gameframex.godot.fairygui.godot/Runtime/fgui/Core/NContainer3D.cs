using FairyGUI;
using Godot;
using System;

namespace FairyGUI
{
    public partial class NContainer3D : SubViewportContainer, IDisplayObject
    {
        // Called when the node enters the scene tree for the first time.
        public GObject gOwner { get; set; }
        public IDisplayObject parent { get { return GetParent() as IDisplayObject; } }
        public CanvasItem node { get { return this; } }
        public bool visible { get { return Visible; } set { Visible = value; } }
        public Vector2 skew { get; set; }
        public float skewX { get; set; }
        public float skewY { get; set; }
        public Vector2 position
        {
            get { return Position; }
            set { Position = value; }
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
            Position = new Vector2(x, y);
        }
        public void SetPosition(Vector2 pos)
        {
            Position = pos;
        }
        public Vector2 size
        {
            get { return Size; }
            set { Size = value; }
        }
        public float width
        {
            get { return Size.X; }
            set
            {
                SetSize(value, Size.Y);
            }
        }
        public float height
        {
            get { return Size.Y; }
            set
            {
                SetSize(Size.X, value);
            }
        }
        public void SetSize(float w, float h)
        {
            Size = new Vector2(w, h);
        }
        public void SetSize(Vector2 size)
        {
            Size = size;
        }
        public Vector2 pivot
        {
            get { return PivotOffset / Size; }
            set { PivotOffset = value * Size; }
        }
        public Vector2 scale
        {
            get { return Scale; }
            set { Scale = value; }
        }
        public float rotation
        {
            get { return Rotation; }
            set { Rotation = value; }
        }
        public BlendMode blendMode { get; set; }
        public NContainer3D(GObject owner)
        {
            gOwner = owner;
            MouseFilter = MouseFilterEnum.Ignore;
        }        
    }
}