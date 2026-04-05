using FairyGUI;
using Godot;
using System;

namespace FairyGUI
{
	public partial class NContainer : Node2D, IDisplayObject
	{
		// Called when the node enters the scene tree for the first time.
		protected Vector2 _position = Vector2.Zero;
        protected Vector2 _size = Vector2.Zero;
        protected Vector2 _pivot = Vector2.Zero;
        protected Vector2 _scale = Vector2.One;
        protected float _rotation = 0;
        protected Vector2 _skew = Vector2.Zero;
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
        }
		public BlendMode blendMode { get; set; }
		
		public NContainer(GObject owner)
        {
            gOwner = owner;
		}	
	}
}