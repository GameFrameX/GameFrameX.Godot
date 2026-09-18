#if TOOLS
using System;
using System.Collections.Generic;
using System.Linq;
using GameFrameX.Editor;

namespace GameFrameX.UI.Editor
{
    /// <summary>
    /// UI 模块脚本宏定义帮助类。
    /// ENABLE_UI_FAIRYGUI 与 ENABLE_UI_GDGUI 为互斥后端宏：切换必须成对原子替换（移除另一个、写入当前个），
    /// 并通过一次 SetScriptingDefineSymbols 调用统一写回 Godot/Hotfix/LeanCLR 三个工程，避免中间态。
    /// </summary>
    public static class UIScriptingDefineSymbols
    {
        public const string FairyGuiScriptingDefineSymbol = "ENABLE_UI_FAIRYGUI";
        public const string GodotGuiScriptingDefineSymbol = "ENABLE_UI_GDGUI";

        /// <summary>
        /// 功能：切换 UI 后端编译宏（互斥成对替换，一次性写回全部工程并触发重编译）。
        /// </summary>
        /// <param name="enableSymbol">要启用的后端宏，必须为本类公开的两个常量之一。</param>
        /// <param name="triggerRecompile">是否立即触发重编译，headless 冒烟验证传 false。</param>
        public static void SetUiBackend(string enableSymbol, bool triggerRecompile = true)
        {
            if (!string.Equals(enableSymbol, FairyGuiScriptingDefineSymbol, StringComparison.Ordinal) &&
                !string.Equals(enableSymbol, GodotGuiScriptingDefineSymbol, StringComparison.Ordinal))
            {
                throw new ArgumentException($"Unknown UI backend define symbol: {enableSymbol}");
            }

            List<string> symbols = new List<string>(ScriptingDefineSymbols.GetScriptingDefineSymbols());
            symbols.RemoveAll(static x =>
                string.Equals(x, FairyGuiScriptingDefineSymbol, StringComparison.Ordinal) ||
                string.Equals(x, GodotGuiScriptingDefineSymbol, StringComparison.Ordinal));
            symbols.Add(enableSymbol);
            ScriptingDefineSymbols.SetScriptingDefineSymbols(symbols.Distinct(StringComparer.Ordinal).ToArray(), triggerRecompile);
        }

        /// <summary>
        /// 功能：切换到 FairyGUI 后端。
        /// </summary>
        /// <param name="triggerRecompile">是否立即触发重编译，headless 冒烟验证传 false。</param>
        public static void UseFairyGui(bool triggerRecompile = true)
        {
            SetUiBackend(FairyGuiScriptingDefineSymbol, triggerRecompile);
        }

        /// <summary>
        /// 功能：切换到 Godot GUI（GDGUI）后端。
        /// </summary>
        /// <param name="triggerRecompile">是否立即触发重编译，headless 冒烟验证传 false。</param>
        public static void UseGodotGui(bool triggerRecompile = true)
        {
            SetUiBackend(GodotGuiScriptingDefineSymbol, triggerRecompile);
        }
    }
}
#endif
