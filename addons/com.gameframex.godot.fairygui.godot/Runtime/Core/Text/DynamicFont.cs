using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace FairyGUI
{
    class GlyphInfo
    {
        public int c;
        public Rect rect;
        public Vector2 offset;
        public Vector2 advance;
        public GlyphCacheTex tex;
        public Vector2 sizeR;
        public Rect uvRect;
        public Vector2 offsetR;
        public Vector2 advanceR;
    };
    class GlyphCacheTex
    {
        public Image img;
        public ImageTexture tex;
        public Rect leftArea;
        public Rect lineLeft;
        public bool needUpdate = false;
        public Rect whiteBlockUV;

        public GlyphCacheTex(float height, Image.Format pixcelFormat)
        {
            Init(height, pixcelFormat);
        }

        void Init(float height, Image.Format pixcelFormat)
        {
            img = Image.CreateEmpty(UIConfig.glyphCacheTexSize, UIConfig.glyphCacheTexSize, false, pixcelFormat);
            //右上角3x3填充白色，用于删除线，下划线等
            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                    img.SetPixel(c, r, Colors.White);
            }
            tex = ImageTexture.CreateFromImage(img);
            lineLeft = new Rect(4, 0, UIConfig.glyphCacheTexSize, height);
            leftArea = new Rect(0, height, UIConfig.glyphCacheTexSize, UIConfig.glyphCacheTexSize - height);
            needUpdate = false;
            whiteBlockUV = new Rect(1.5f / UIConfig.glyphCacheTexSize, 1.5f / UIConfig.glyphCacheTexSize, 1.0f / UIConfig.glyphCacheTexSize, 1.0f / UIConfig.glyphCacheTexSize);
        }
    }
    class GlyphCache
    {
        Dictionary<int, GlyphInfo> _glyphDic = new Dictionary<int, GlyphInfo>();
        List<GlyphCacheTex> _cacheTexs = new List<GlyphCacheTex>();
        int _fontSize;
        int _charMaxHeight;

        static int SAMPLE_GAP = 2;


        public GlyphCache(int fontSize, int charMaxHeight)
        {
            _fontSize = fontSize;
            _charMaxHeight = charMaxHeight;
        }


        public GlyphInfo GetGlyphInfo(int c)
        {
            GlyphInfo info;
            if (_glyphDic.TryGetValue(c, out info))
            {
                return info;
            }
            return null;
        }

        public GlyphInfo AddGlyph(int c, Rect rect, Vector2 offset, Vector2 advance, Image glyphImage)
        {
            if (_glyphDic.ContainsKey(c))
                return null;
            float width = rect.width;
            float height = _charMaxHeight;
            if (rect.height > height)
            {
                GD.PushError($"invalid glyph height(charMaxHeight={_charMaxHeight} rect={rect})");
                return null;
            }

            if (width > UIConfig.glyphCacheTexSize || height > UIConfig.glyphCacheTexSize)
            {
                GD.PushError($"too large glyph size({width},{height})");
                return null;
            }

            GlyphCacheTex cacheTex = null;
            if (_cacheTexs.Count > 0)
            {
                cacheTex = _cacheTexs.Last();
                if (width > cacheTex.lineLeft.width || height > cacheTex.lineLeft.height)
                {
                    if (width > cacheTex.leftArea.width || height > cacheTex.leftArea.height)
                    {
                        cacheTex = null;
                        //缓冲区已满，需要新增
                    }
                    else
                    {
                        //行已满，切到下一行
                        cacheTex.lineLeft.position = cacheTex.leftArea.position;
                        cacheTex.lineLeft.yMin += SAMPLE_GAP;
                        cacheTex.lineLeft.width = cacheTex.leftArea.width;
                        cacheTex.lineLeft.height = height;
                        cacheTex.leftArea.height -= cacheTex.lineLeft.height + SAMPLE_GAP;
                        cacheTex.leftArea.yMin += cacheTex.lineLeft.height + SAMPLE_GAP;
                    }
                }
            }
            if (cacheTex == null)
            {
                cacheTex = new GlyphCacheTex(height, glyphImage.GetFormat());
                _cacheTexs.Add(cacheTex);
            }
            GlyphInfo info = new GlyphInfo();
            info.c = c;
            info.rect = new Rect(cacheTex.lineLeft.xMin, cacheTex.lineLeft.yMin, rect.width, rect.height);
            info.offset = offset;
            info.advance = advance;
            info.sizeR = rect.size / _fontSize;
            info.uvRect = new Rect(info.rect.xMin / cacheTex.img.GetWidth(), info.rect.yMin / cacheTex.img.GetHeight(), info.rect.width / cacheTex.img.GetWidth(), info.rect.height / cacheTex.img.GetWidth());
            info.offsetR = offset / _fontSize;
            info.advanceR = advance / _fontSize;
            info.tex = cacheTex;
            _glyphDic.Add(c, info);
            cacheTex.img.BlitRect(glyphImage, new Rect2I((int)rect.xMin, (int)rect.yMin, (int)rect.width, (int)rect.height), new Vector2I((int)info.rect.xMin, (int)info.rect.yMin));
            cacheTex.lineLeft.xMin += width + SAMPLE_GAP;
            cacheTex.needUpdate = true;
            return info;
        }

        public void UpdateTexture()
        {
            for (int i = 0; i < _cacheTexs.Count; i++)
            {
                GlyphCacheTex tex = _cacheTexs[i];
                if (tex.needUpdate)
                {
                    tex.tex.Update(tex.img);
                    tex.needUpdate = false;
                }
            }
        }
    }
    public class DynamicFont : BaseFont
    {
        Font _font;
        Rid _fontRid;
        int _fontSize;
        int _normalizedFontSize;
        int _outlineSize;
        int _charMaxHeight;
        TextFormat _format;
        TextServer.FontStyle _style;
        Dictionary<long, GlyphCache> _glyphCache = new Dictionary<long, GlyphCache>();
        GlyphInfo _glyphInfo;
        TextServer _textSrv;
        static Color[] vertexColors = new Color[4];
        float _underLineOffset = float.NaN;

        public static float LINE_HEIGHT_FACTOR = 1.25f;
        static float DEFAULT_BLOD_STRENGTH = 0.4f;
        static float DEFAULT_ITALIC_ANGLE = Mathf.DegToRad(15);
        static float OUTLINE_SCALE = 4.0f;

        public static DynamicFont LoadFont(string fontPathOrName)
        {
            if (fontPathOrName.StartsWith("res://"))
            {
                Font font = ResourceLoader.Load<Font>(fontPathOrName);
                if (font != null)
                {
                    return new DynamicFont(font);
                }
                else
                {
                    GD.PushWarning($"font {fontPathOrName} not found.");
                }
            }
            else
            {
                SystemFont font = new SystemFont();
                font.FontNames = new string[] { fontPathOrName };
                return new DynamicFont(font);
            }
            return null;
        }

        public DynamicFont()
        {
            this.canTint = true;
            _textSrv = TextServerManager.GetPrimaryInterface();
        }

        public DynamicFont(Font font) : this()
        {
            this.name = font.GetFontName();
            this.nativeFont = font;
        }

        override public void Dispose()
        {
            if (_font != null)
            {
                _font.Dispose();
                _font = null;
            }
        }

        public Font nativeFont
        {
            get { return _font; }
            set
            {
                _font = value;
                if (_font != null)
                {
                    var Rids = _font.GetRids();
                    if (Rids.Count > 0)
                        _fontRid = Rids[0];
                    else
                        _fontRid = new Rid();
                }
                else
                {
                    _fontRid = new Rid();
                }
            }
        }

        int NormalizeFontSize(int fontSize)
        {
            if (UIConfig.fontSizeLevels != null)
            {
                int closest = -1;
                int closestDiff = 0;
                foreach (int level in UIConfig.fontSizeLevels)
                {
                    int diff = Math.Abs(fontSize - level);
                    if (closest < 0 || diff < closestDiff)
                    {
                        closest = level;
                        closestDiff = diff;
                    }
                }
                if (closest > 0)
                    fontSize = closest;
            }
            else
            {
                if (fontSize < UIConfig.minFontSize)
                    fontSize = UIConfig.minFontSize;
                if (UIConfig.maxFontSize > UIConfig.minFontSize && fontSize > UIConfig.maxFontSize)
                    fontSize = UIConfig.maxFontSize;
            }
            return fontSize;
        }

        GlyphCache GetGlyphCache(int fontSize, int outlineSize, TextServer.FontStyle style)
        {
            long key = (((int)style) & 0xFF) << 32 | (outlineSize & 0xFF) << 16 | fontSize & 0xFFFF;
            GlyphCache cache;
            if (_glyphCache.TryGetValue(key, out cache))
                return cache;
            cache = new GlyphCache(fontSize, _charMaxHeight);
            _glyphCache.Add(key, cache);
            return cache;
        }

        GlyphInfo GetGlyphInfo(int ch, float outlineSize)
        {
            int outline = Mathf.RoundToInt(outlineSize * OUTLINE_SCALE);
            GlyphCache cache = GetGlyphCache(_normalizedFontSize, outline, _style);
            if (cache != null)
            {
                GlyphInfo info = cache.GetGlyphInfo(ch);
                if (info == null)
                {
                    //缓冲里不存在需要添加
                    if (_font != null && _fontRid.IsValid)
                    {
                        long glyphIndex = _textSrv.FontGetGlyphIndex(_fontRid, _normalizedFontSize, ch, 0);
                        if (glyphIndex != 0)
                        {
                            Vector2I size = new Vector2I(_normalizedFontSize, outline);
                            var rect = _textSrv.FontGetGlyphUVRect(_fontRid, size, glyphIndex);
                            var advance = _textSrv.FontGetGlyphAdvance(_fontRid, _normalizedFontSize, glyphIndex);
                            var offset = _textSrv.FontGetGlyphOffset(_fontRid, size, glyphIndex);
                            long texIdx = _textSrv.FontGetGlyphTextureIdx(_fontRid, size, glyphIndex);
                            if (texIdx >= 0)
                            {
                                Image img = _textSrv.FontGetTextureImage(_fontRid, size, texIdx);
                                info = cache.AddGlyph(ch, rect, offset, advance, img);
                                FontManager.inst.QueryUpdateFont(this);
                                if (texIdx > 0)
                                {
                                    //godot使用了超过1张缓冲，清除缓冲
                                    _textSrv.FontClearGlyphs(_fontRid, size);
                                    _textSrv.FontClearTextures(_fontRid, size);
                                }
                            }
                        }
                    }
                }
                return info;
            }
            return null;
        }

        override public void SetFormat(TextFormat format, float fontSizeScale)
        {
            _format = format;
            float size = _format.size * fontSizeScale;
            if (_format.specialStyle == TextFormat.SpecialStyle.Subscript || _format.specialStyle == TextFormat.SpecialStyle.Superscript)
                size *= SupScale;
            _fontSize = Mathf.FloorToInt(size);
            if (_fontSize == 0)
                _fontSize = 1;
            _normalizedFontSize = NormalizeFontSize(_fontSize);
            _style = 0;
            if (_format.bold)
            {
                _style |= TextServer.FontStyle.Bold;
                _textSrv.FontSetEmbolden(_fontRid, DEFAULT_BLOD_STRENGTH);
            }
            if (_format.italic)
            {
                _style |= TextServer.FontStyle.Italic;
            }
            _outlineSize = Mathf.RoundToInt(_format.outline);
            _charMaxHeight = Mathf.RoundToInt(_font.GetHeight(_normalizedFontSize) + _outlineSize * 2);
            _format.FillVertexColors(vertexColors);
            _underLineOffset = float.NaN;
        }

        override public void PrepareCharacters(string text, TextFormat format, float fontSizeScale)
        {
            SetFormat(format, fontSizeScale);
        }

        override public bool GetGlyph(char ch, out float width, out float height, out float baseline)
        {
            _glyphInfo = GetGlyphInfo(ch, 0);
            if (_glyphInfo == null)
            {
                width = height = baseline = 0;
                return false;
            }

            width = _glyphInfo.advanceR.X * _fontSize;
            height = _glyphInfo.advanceR.Y * _fontSize * LINE_HEIGHT_FACTOR;
            baseline = _fontSize;

            if (_format.specialStyle == TextFormat.SpecialStyle.Subscript)
            {
                height /= SupScale;
                baseline /= SupScale;
            }
            else if (_format.specialStyle == TextFormat.SpecialStyle.Superscript)
            {
                height = height / SupScale + baseline * SupOffset;
                baseline *= SupOffset + 1 / SupScale;
            }

            height = Mathf.RoundToInt(height);
            baseline = Mathf.RoundToInt(baseline);

            return true;
        }
        override public void DrawGlyph(TextMeshCluster meshCluster, float x, float y)
        {
            if (_glyphInfo == null)
                return;

            if (_format.specialStyle == TextFormat.SpecialStyle.Subscript)
                y = y + Mathf.RoundToInt(_fontSize * SupOffset);
            else if (_format.specialStyle == TextFormat.SpecialStyle.Superscript)
                y = y - Mathf.RoundToInt(_fontSize * (1 / SupScale - 1 + SupOffset));

            Rect drawRect = new Rect(x + _glyphInfo.offsetR.X * _fontSize, y + _glyphInfo.offsetR.Y * _fontSize, _glyphInfo.sizeR.X * _fontSize, _glyphInfo.sizeR.Y * _fontSize);

            if (!Mathf.IsZeroApprox(_format.outline))
            {
                switch (UIConfig.textOutlineType)
                {
                    case TextFormat.TextOutlineType.Godot:
                        {
                            GlyphInfo outlineGlyphInfo = GetGlyphInfo(_glyphInfo.c, _format.outline);
                            if (outlineGlyphInfo != null)
                            {
                                Rect outlineRect = new Rect(x + outlineGlyphInfo.offsetR.X * _fontSize, y + outlineGlyphInfo.offsetR.Y * _fontSize, outlineGlyphInfo.sizeR.X * _fontSize, outlineGlyphInfo.sizeR.Y * _fontSize);
                                meshCluster.AddGlyph(outlineGlyphInfo.tex.tex, outlineRect, outlineGlyphInfo.uvRect, _format.outlineColor, null, (_style & TextServer.FontStyle.Italic) != 0 ? DEFAULT_ITALIC_ANGLE : 0);
                            }
                        }
                        break;
                    case TextFormat.TextOutlineType.FourDir:
                        for (int i = 0; i < 4; i++)
                        {
                            meshCluster.AddGlyph(_glyphInfo.tex.tex, drawRect + TextFormat.OUTLINE_OFFSETS[i] * _format.outline, _glyphInfo.uvRect, _format.outlineColor, null, (_style & TextServer.FontStyle.Italic) != 0 ? DEFAULT_ITALIC_ANGLE : 0);
                        }
                        break;
                    case TextFormat.TextOutlineType.EightDir:
                        for (int i = 0; i < 8; i++)
                        {
                            meshCluster.AddGlyph(_glyphInfo.tex.tex, drawRect + TextFormat.OUTLINE_OFFSETS[i] * _format.outline, _glyphInfo.uvRect, _format.outlineColor, null, (_style & TextServer.FontStyle.Italic) != 0 ? DEFAULT_ITALIC_ANGLE : 0);
                        }
                        break;
                }
            }
            if (!_format.shadowOffset.IsZeroApprox())
                meshCluster.AddGlyph(_glyphInfo.tex.tex, drawRect + _format.shadowOffset, _glyphInfo.uvRect, _format.shadowColor, null, (_style & TextServer.FontStyle.Italic) != 0 ? DEFAULT_ITALIC_ANGLE : 0);
            meshCluster.AddGlyph(_glyphInfo.tex.tex, drawRect, _glyphInfo.uvRect, Colors.White, vertexColors, (_style & TextServer.FontStyle.Italic) != 0 ? DEFAULT_ITALIC_ANGLE : 0);
        }

        override public void DrawLine(TextMeshCluster meshCluster, float x, float y, float width, int fontSize, int type)
        {
            if (_glyphInfo == null)
                return;

            if (_format.specialStyle == TextFormat.SpecialStyle.Subscript)
                y = y - Mathf.RoundToInt(_fontSize * SupOffset);
            else if (_format.specialStyle == TextFormat.SpecialStyle.Superscript)
                y = y + Mathf.RoundToInt(_fontSize * (1 / SupScale - 1 + SupOffset));

            float thickness;
            float offset;

            thickness = Mathf.Max(1, fontSize / 16f);
            if (thickness < 1)
                thickness = 1;

            if (type == 0)
            {
                if (float.IsNaN(_underLineOffset))
                {
                    var underLineGlyphInfo = GetGlyphInfo('_', 0);
                    if (underLineGlyphInfo != null)
                        _underLineOffset = underLineGlyphInfo.offsetR.Y * _fontSize;
                    else
                        _underLineOffset = 0;
                }
                offset = _underLineOffset;
            }
            else
                offset = -Mathf.RoundToInt(fontSize * 0.4f);

            Rect drawRect = new Rect(x, y + offset, width, thickness);

            if (!Mathf.IsZeroApprox(_format.outline))
            {
                switch (UIConfig.textOutlineType)
                {
                    case TextFormat.TextOutlineType.Godot:
                        break;
                    case TextFormat.TextOutlineType.FourDir:
                        for (int i = 0; i < 4; i++)
                        {
                            meshCluster.AddGlyph(_glyphInfo.tex.tex, drawRect + TextFormat.OUTLINE_OFFSETS[i] * _format.outline, _glyphInfo.tex.whiteBlockUV, _format.outlineColor, null, 0);
                        }
                        break;
                    case TextFormat.TextOutlineType.EightDir:
                        for (int i = 0; i < 8; i++)
                        {
                            meshCluster.AddGlyph(_glyphInfo.tex.tex, drawRect + TextFormat.OUTLINE_OFFSETS[i] * _format.outline, _glyphInfo.tex.whiteBlockUV, _format.outlineColor, null, 0);
                        }
                        break;
                }
            }

            meshCluster.AddGlyph(_glyphInfo.tex.tex, drawRect, _glyphInfo.tex.whiteBlockUV, Colors.White, vertexColors, 0);
        }

        override public bool HasCharacter(char ch)
        {
            if (_font != null && _fontRid.IsValid)
            {
                long glyphIndex = _textSrv.FontGetGlyphIndex(_fontRid, _fontSize, ch, 0);
                return glyphIndex != 0;
            }
            return false;
        }

        override public int GetLineHeight(int size)
        {
            return Mathf.RoundToInt(size * LINE_HEIGHT_FACTOR);
        }

        override public void UpdateCacheTextures()
        {
            foreach (var cache in _glyphCache.Values)
            {
                cache.UpdateTexture();
            }
        }
    }
}
