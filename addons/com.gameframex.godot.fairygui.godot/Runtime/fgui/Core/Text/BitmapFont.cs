using System.Collections.Generic;
using FairyGUI.Utils;
using Godot;

namespace FairyGUI
{
    public class BitmapFont : BaseFont
    {
        public class BMGlyph
        {
            public float x;
            public float y;
            public float width;
            public float height;
            public int advance;
            public int lineHeight;
            public Vector2[] uv = new Vector2[4];
            public int channel;//0-n/a, 1-r,2-g,3-b,4-alpha
        }

        public int size;
        public bool resizable;
        public bool hasChannel;
        protected Dictionary<int, BMGlyph> _dict;
        protected BMGlyph _glyph;
        float _scale;

        public BitmapFont()
        {
            this.canTint = true;
            this.hasChannel = false;

            _dict = new Dictionary<int, BMGlyph>();
            _scale = 1;
        }

        public void AddChar(char ch, BMGlyph glyph)
        {
            _dict[ch] = glyph;
        }

        override public void SetFormat(TextFormat format, float fontSizeScale)
        {
            if (resizable)
                _scale = (float)format.size / size * fontSizeScale;
            else
                _scale = fontSizeScale;

            if (canTint)
                format.FillVertexColors(vertexColors);
        }

        override public bool GetGlyph(char ch, out float width, out float height, out float baseline)
        {
            if (ch == ' ')
            {
                width = Mathf.RoundToInt(size * _scale / 2);
                height = Mathf.RoundToInt(size * _scale);
                baseline = height;
                _glyph = null;
                return true;
            }
            else if (_dict.TryGetValue((int)ch, out _glyph))
            {
                width = Mathf.RoundToInt(_glyph.advance * _scale);
                height = Mathf.RoundToInt(_glyph.lineHeight * _scale);
                baseline = height;
                return true;
            }
            else
            {
                width = 0;
                height = 0;
                baseline = 0;
                return false;
            }
        }

        static Vector3[] sVertices = new Vector3[4];
        static Color[] vertexColors = new Color[4];

        override public void DrawGlyph(TextMeshCluster meshCluster, float x, float y)
        {
            if (_glyph == null) //space
                return;

            var mesh = meshCluster.GetMesh(mainTexture.nativeTexture);
            if (mesh == null)
                return;

            sVertices[0].X = x + _glyph.x * _scale;
            sVertices[0].Y = y - (_glyph.lineHeight - _glyph.y) * _scale;
            sVertices[2].X = x + (_glyph.x + _glyph.width) * _scale;
            sVertices[2].Y = sVertices[0].Y + _glyph.height * _scale;

            sVertices[3].X = sVertices[2].X;
            sVertices[3].Y = sVertices[0].Y;
            sVertices[1].X = sVertices[0].X;
            sVertices[1].Y = sVertices[2].Y;

            mesh.AddGlyph(sVertices, _glyph.uv, Colors.White, canTint ? vertexColors : null);
        }

        override public bool HasCharacter(char ch)
        {
            return ch == ' ' || _dict.ContainsKey((int)ch);
        }

        override public int GetLineHeight(int size)
        {
            if (_dict.Count > 0)
            {
                using (var et = _dict.GetEnumerator())
                {
                    et.MoveNext();
                    if (resizable)
                        return Mathf.RoundToInt((float)et.Current.Value.lineHeight * size / this.size);
                    else
                        return et.Current.Value.lineHeight;
                }
            }
            else
                return 0;
        }
    }
}
