#if TOOLS
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using GameFrameX.Editor;
using GameFrameX.UI.Editor;
using Godot;

namespace GameFrameX.EditorTests
{
    /// <summary>
    /// UI 后端宏快捷切换 headless 冒烟验收。
    /// 在真实编辑器进程（--headless -e -s）内驱动 UIScriptingDefineSymbols 全流程：
    /// 切到 GDGUI → 校验各工程（Godot/Hotfix，及存在时的 LeanCLR）互斥写入 → 切回原后端 → 校验还原。
    /// LeanCLR 为可选特性：工程文件不存在时跳过该项校验，且宏同步不得重建该工程。
    /// 结果行：=== UI_DEFINE_SMOKE_RESULT: total=N passed=N failed=N ===（shell 层据此判定）。
    /// </summary>
    public sealed partial class UiDefineSwitchSmokeTest : Node
    {
        private static readonly string[] CsprojPaths =
        {
            "res://Godot.csproj",
            "res://Assets/Hotfix/Hotfix.csproj",
            "res://Assets/LeanCLR/GameFrameX.csproj",
        };

        private int m_Total;
        private int m_Passed;
        private bool m_Ran;
        private string m_OriginalBackend;

        public override void _Ready()
        {
            GD.Print("[UiDefineSmoke] start");
        }

        public override void _Process(double delta)
        {
            if (m_Ran)
            {
                return;
            }

            m_Ran = true;
            RunTests();
            Cleanup();
        }

        private void RunTests()
        {
            // LeanCLR 为可选特性：仅校验磁盘上实际存在的工程，未安装时跳过且不创建。
            string[] csprojPaths = CsprojPaths
                .Where(static path => File.Exists(ProjectSettings.GlobalizePath(path)))
                .ToArray();
            foreach (string skippedPath in CsprojPaths.Except(csprojPaths))
            {
                GD.Print($"[UiDefineSmoke] SKIP optional project absent: {skippedPath}");
            }

            string[] originalSymbols = ScriptingDefineSymbols.GetScriptingDefineSymbols();
            m_OriginalBackend = originalSymbols.FirstOrDefault(static x =>
                string.Equals(x, UIScriptingDefineSymbols.FairyGuiScriptingDefineSymbol, StringComparison.Ordinal) ||
                string.Equals(x, UIScriptingDefineSymbols.GodotGuiScriptingDefineSymbol, StringComparison.Ordinal));

            try
            {
                UIScriptingDefineSymbols.UseGodotGui(triggerRecompile: false);

                Check("use_godot_gui_active_symbol", ScriptingDefineSymbols.HasScriptingDefineSymbol(UIScriptingDefineSymbols.GodotGuiScriptingDefineSymbol));
                Check("use_godot_gui_removed_fairy", !ScriptingDefineSymbols.HasScriptingDefineSymbol(UIScriptingDefineSymbols.FairyGuiScriptingDefineSymbol));
                foreach (string csprojPath in csprojPaths)
                {
                    Check($"use_godot_gui_written:{csprojPath}", ReadDefineConstants(csprojPath).Contains(UIScriptingDefineSymbols.GodotGuiScriptingDefineSymbol));
                    Check($"use_godot_gui_exclusive:{csprojPath}", !ReadDefineConstants(csprojPath).Contains(UIScriptingDefineSymbols.FairyGuiScriptingDefineSymbol));
                }

                RestoreOriginalBackend();

                Check("restore_removed_godot_gui", !ScriptingDefineSymbols.HasScriptingDefineSymbol(UIScriptingDefineSymbols.GodotGuiScriptingDefineSymbol));
                foreach (string csprojPath in csprojPaths)
                {
                    string restored = ReadDefineConstants(csprojPath);
                    Check($"restore_no_godot_gui:{csprojPath}", !restored.Contains(UIScriptingDefineSymbols.GodotGuiScriptingDefineSymbol));
                    if (!string.IsNullOrEmpty(m_OriginalBackend))
                    {
                        Check($"restore_backend_symbol:{csprojPath}", restored.Contains(m_OriginalBackend));
                    }
                }
            }
            catch (Exception exception)
            {
                GD.PushError($"[UiDefineSmoke] exception: {exception}");
                Check("no_exception", false);
                RestoreOriginalBackend();
            }
            finally
            {
                GD.Print($"=== UI_DEFINE_SMOKE_RESULT: total={m_Total} passed={m_Passed} failed={m_Total - m_Passed} ===");
            }
        }

        private void RestoreOriginalBackend()
        {
            if (string.Equals(m_OriginalBackend, UIScriptingDefineSymbols.GodotGuiScriptingDefineSymbol, StringComparison.Ordinal))
            {
                UIScriptingDefineSymbols.UseGodotGui(triggerRecompile: false);
            }
            else
            {
                // 原后端为 FairyGUI（或未配置，默认回 FairyGUI，与主工程默认一致）。
                UIScriptingDefineSymbols.UseFairyGui(triggerRecompile: false);
            }
        }

        private void Cleanup()
        {
            GetTree().Quit(m_Total == m_Passed ? 0 : 1);
        }

        private static string ReadDefineConstants(string resPath)
        {
            string absolutePath = ProjectSettings.GlobalizePath(resPath);
            XDocument doc = XDocument.Load(absolutePath);
            XElement defineNode = doc.Descendants().FirstOrDefault(static e => e.Name.LocalName == "DefineConstants");
            return defineNode?.Value ?? string.Empty;
        }

        private void Check(string name, bool ok)
        {
            m_Total++;
            if (ok)
            {
                m_Passed++;
            }

            GD.Print($"[UiDefineSmoke] {(ok ? "PASS" : "FAIL")} {name}");
        }
    }
}
#endif
