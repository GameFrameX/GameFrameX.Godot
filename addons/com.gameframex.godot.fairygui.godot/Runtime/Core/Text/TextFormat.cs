using Godot;

namespace FairyGUI
{
    public class TextFormat
    {
        public enum SpecialStyle
        {
            None,
            Superscript,
            Subscript
        }

        public enum TextOutlineType
        {
            Godot,
            FourDir,
            EightDir,
        }

        public static Vector2[] OUTLINE_OFFSETS = new Vector2[8] { new Vector2(-1, 0), new Vector2(1, 0), new Vector2(0, -1), new Vector2(0, 1), new Vector2(-1, -1), new Vector2(1, 1), new Vector2(-1, 1), new Vector2(1, -1) };

        public int size;
        public string font;
        public Color color;
        public int lineSpacing;
        public int letterSpacing;
        public bool bold;
        public bool underline;
        public bool italic;
        public bool strikethrough;
        public Color[] gradientColor;
        public AlignType align;
        public SpecialStyle specialStyle;
        public float outline;
        public Color outlineColor;
        public Vector2 shadowOffset;
        public Color shadowColor;

        public TextFormat()
        {
            color = Colors.Black;
            size = 12;
            lineSpacing = 3;
            outlineColor = shadowColor = Colors.Black;
        }

        public void SetColor(uint value)
        {
            uint rr = (value >> 16) & 0x0000ff;
            uint gg = (value >> 8) & 0x0000ff;
            uint bb = value & 0x0000ff;
            float r = rr / 255.0f;
            float g = gg / 255.0f;
            float b = bb / 255.0f;
            color = new Color(r, g, b, 1);
        }

        public bool EqualStyle(TextFormat aFormat)
        {
            return size == aFormat.size && color == aFormat.color
                && bold == aFormat.bold && underline == aFormat.underline
                && italic == aFormat.italic
                && strikethrough == aFormat.strikethrough
                && gradientColor == aFormat.gradientColor
                && align == aFormat.align
                && specialStyle == aFormat.specialStyle;
        }

        /// <summary>
        /// Only base NOT all formats will be copied
        /// </summary>
        /// <param name="source"></param>
        public void CopyFrom(TextFormat source)
        {
            this.size = source.size;
            this.font = source.font;
            this.color = source.color;
            this.lineSpacing = source.lineSpacing;
            this.letterSpacing = source.letterSpacing;
            this.bold = source.bold;
            this.underline = source.underline;
            this.italic = source.italic;
            this.strikethrough = source.strikethrough;
            if (source.gradientColor != null)
            {
                this.gradientColor = new Color[4];
                source.gradientColor.CopyTo(this.gradientColor, 0);
            }
            else
                this.gradientColor = null;
            this.align = source.align;
            this.specialStyle = source.specialStyle;
            this.shadowColor = source.shadowColor;
            this.shadowOffset = source.shadowOffset;
            this.outline = source.outline;
            this.outlineColor = source.outlineColor;
        }

        public void FillVertexColors(Color[] vertexColors)
        {
            if (gradientColor == null)
                vertexColors[0] = vertexColors[1] = vertexColors[2] = vertexColors[3] = color;
            else
            {
                vertexColors[0] = gradientColor[1];
                vertexColors[1] = gradientColor[0];
                vertexColors[2] = gradientColor[2];
                vertexColors[3] = gradientColor[3];
            }
        }
    }
}
