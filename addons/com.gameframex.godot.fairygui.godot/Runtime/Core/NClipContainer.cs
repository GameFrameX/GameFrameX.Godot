using FairyGUI;
using Godot;
using System;

namespace FairyGUI
{
	public partial class NClipContainer : Control, IDisplayObject
	{
		// Called when the node enters the scene tree for the first time.
		protected CanvasItem _mask;
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
		
		public NClipContainer(GObject owner)
		{
			gOwner = owner;
			MouseFilter = MouseFilterEnum.Ignore;
			ClipContents = true;
		}	

		public CanvasItem mask
		{
			get { return _mask; }
			set
			{
				if (_mask != value)
				{
					if (value == null)
					{
						if (_mask is NImage image)
						{
							image.maskOwner = null;
							image.QueueRedraw();
						}
						else if (_mask is NShape shape)
						{
							shape.maskOwner = null;
							shape.QueueRedraw();
						}
					}
					_mask = value;
					if (_mask != null)
					{
						ClipChildren = ClipChildrenMode.Only;
						TextureRepeat = _mask.TextureRepeat;
						if (_mask is NImage image)
						{
							image.maskOwner = this;
							image.QueueRedraw();
						}
						else if (_mask is NShape shape)
						{
							shape.maskOwner = this;
							shape.QueueRedraw();
						}
					}
					else
					{
						ClipChildren = ClipChildrenMode.Disabled;
					}
					QueueRedraw();
				}
			}
		}
		public bool reversedMask
		{
			get
			{
				if (_mask is NImage image)
				{
					return image.reverseMask;
				}
				else if (_mask is NShape shape)
				{
					return shape.reverseMask;
				}
				return false;
			}
			set
			{
				if (_mask is NImage image)
				{
					image.reverseMask = value;
				}
				else if (_mask is NShape shape)
				{
					shape.reverseMask = value;
				}
			}
		}
		public override void _Draw()
		{
			if (_mask != null)
			{
				if (_mask is NImage image)
				{
					Transform2D trans = image.GetTransform();
					image.UpdateMesh();
					DrawMesh(image.mesh, image.drawTexture, trans);
					if (image.outBoundMesh != null)
						DrawMesh(image.outBoundMesh, null, trans);
				}
				else if (_mask is NShape shape)
				{
					Transform2D trans = shape.GetTransform();
					shape.UpdateMesh();
					DrawMesh(shape.mesh, shape.texture?.nativeTexture, trans);
				}
			}
		}
	}
}