using System;
using System.Collections.Generic;
using Godot;

namespace FairyGUI
{
    /// <summary>
    /// 
    /// </summary>
    public class FontManager
    {
        static FontManager _inst;
        public static FontManager inst
        {
            get
            {
                if (_inst == null)
                    _inst = new FontManager();
                return _inst;
            }
        }
        public Dictionary<string, BaseFont> _fontFactory = new Dictionary<string, BaseFont>();
        HashSet<BaseFont> _fontUpdateQueue = new HashSet<BaseFont>();
        FontManager()
        {
            RenderingServer.FramePreDraw += OnFramePreDraw;
        }
        public void QueryUpdateFont(BaseFont font)
        {
            _fontUpdateQueue.Add(font);
        }
        void OnFramePreDraw()
        {
            foreach (var font in _fontUpdateQueue)
            {
                font.UpdateCacheTextures();
            }
            _fontUpdateQueue.Clear();
        }
        public void RegisterFont(BaseFont font, string alias = null)
        {
            _fontFactory[font.name] = font;
            if (alias != null)
                _fontFactory[alias] = font;
        }
        public void UnregisterFont(BaseFont font)
        {
            List<string> toDelete = new List<string>();
            foreach (KeyValuePair<string, BaseFont> kv in _fontFactory)
            {
                if (kv.Value == font)
                    toDelete.Add(kv.Key);
            }

            foreach (string key in toDelete)
                _fontFactory.Remove(key);
        }
        public BaseFont GetFont(string fontPath)
        {
            if (string.IsNullOrEmpty(fontPath))
            {
                return fallbackFont;
            }
            BaseFont font;
            if (fontPath.StartsWith(UIPackage.URL_PREFIX))
            {
                font = UIPackage.GetItemAssetByURL(fontPath) as BaseFont;
                if (font != null)
                    return font;
            }

            if (_fontFactory.TryGetValue(fontPath, out font))
                return font;
            font = DynamicFont.LoadFont(fontPath);
            if (font != null)
            {
                RegisterFont(font, fontPath);
                return font;
            }
            else
            {
                return fallbackFont;
            }
        }
        BaseFont fallbackFont
        {
            get
            {
                BaseFont font;
                if (!_fontFactory.TryGetValue("$default_font", out font))
                {
                    font = DynamicFont.LoadFont(UIConfig.defaultFont);
                    RegisterFont(font, "$default_font");
                }
                return font;
            }
        }
        public void Clear()
        {
            foreach (KeyValuePair<string, BaseFont> kv in _fontFactory)
                kv.Value.Dispose();
            _fontFactory.Clear();
        }
    }
}
