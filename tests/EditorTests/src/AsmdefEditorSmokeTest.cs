#if TOOLS
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using GameFrameX.Editor.Asmdef;
using Godot;

namespace GameFrameX.EditorTests
{
    /// <summary>
    /// G4 Asmdef 属性编辑器 headless 冒烟验收。
    /// 在真实编辑器进程（--headless -e -s）内驱动窗口全流程：
    /// 构建 UI → 发现现有 asmdef → 新建 → 编辑 → 合法保存 → 非法保存阻止 → 校验 → 同步生成 csproj。
    /// 结果行：=== ASMDEF_SMOKE_RESULT: total=N passed=N failed=N ===（shell 层据此判定）。
    /// </summary>
    public sealed partial class AsmdefEditorSmokeTest : Node
    {
        private int m_Total;
        private int m_Passed;
        private bool m_Ran;
        private bool m_SyncRequested;
        private AsmdefEditorWindow m_Window;

        public override void _Ready()
        {
            // _Ready 传播期间 Root 处于 busy setting up children 状态（同 AssetSystem 驱动节点修复），
            // 窗口经 deferred 挂载，用例在下一帧窗口进树后由 _Process 启动。
            m_Window = new AsmdefEditorWindow(OnFileSaved, OnRunSyncRequested);
            m_Window.Name = "AsmdefEditorWindow";
            GetTree().Root.CallDeferred(Node.MethodName.AddChild, m_Window);
        }

        public override void _Process(double delta)
        {
            if (m_Ran || m_Window == null)
            {
                return;
            }

            if (!m_Window.IsInsideTree())
            {
                return;
            }

            m_Ran = true;
            try
            {
                RunTests();
            }
            catch (Exception exception)
            {
                m_Total++;
                GD.PrintErr($"[AsmdefSmoke] FAIL 异常中断: {exception}");
            }
            finally
            {
                Cleanup();
            }

            int failed = m_Total - m_Passed;
            GD.Print($"=== ASMDEF_SMOKE_RESULT: total={m_Total} passed={m_Passed} failed={failed} ===");
            GetTree().Quit(failed > 0 ? 1 : 0);
        }

        private void RunTests()
        {
            // T1 窗口构建与文件发现
            Check("T1 窗口进树且 UI 构建", m_Window.IsInsideTree() && m_Window.Title.Contains("Asmdef"));
            var fileList = GetField("m_CurrentAsmdefFiles") as List<string>;
            Check("T1 文件列表发现现有 asmdef", fileList != null && fileList.Count >= 1);

            // T2 新建
            InvokePrivate("OnCreateNewAsmdefPressed");
            string created = (string)GetField("m_CurrentFilePath");
            Check("T2 新建 asmdef 落盘", !string.IsNullOrEmpty(created) && File.Exists(created));
            Check("T2 新建后表单加载名称", GetControlText("m_NameEdit") == "NewAssembly");

            // T3 编辑 + 合法保存（defines 与 platformDefines 往返）
            SetControlText("m_DefinesEdit", "GFX_SMOKE_DEFINE");
            SetControlText("m_PlatformDefinesEdit", "macos: GFX_SMOKE_MAC, GFX_DEBUG");
            InvokePrivate("OnSavePressed");
            string savedJson = File.Exists(created) ? File.ReadAllText(created) : string.Empty;
            Check("T3 合法保存落盘 defines", savedJson.Contains("GFX_SMOKE_DEFINE"));
            Check("T3 合法保存落盘 platformDefines", savedJson.Contains("GFX_SMOKE_MAC"));

            // T4 非法保存被校验阻止
            string beforeSave = savedJson;
            SetControlText("m_NameEdit", string.Empty);
            InvokePrivate("OnSavePressed");
            string afterSave = File.Exists(created) ? File.ReadAllText(created) : string.Empty;
            Check("T4 非法名称保存被阻止（文件未变）", afterSave == beforeSave);
            Check("T4 阻止状态提示", GetStatusText().Contains("保存已阻止"));

            // T5 校验按钮
            InvokePrivate("OnValidatePressed");
            Check("T5 校验按钮可执行", GetStatusText().Length > 0);

            // T6 同步生成
            InvokePrivate("OnSyncPressed");
            Check("T6 同步回调触发", m_SyncRequested);
            var syncService = new AsmdefSyncService(AsmdefPathUtility.FindAllAsmdefFiles);
            syncService.RunSync();
            string csprojPath = AsmdefPathUtility.GetCsprojPathForAsmdef(created);
            Check("T6 csproj 已生成", File.Exists(csprojPath));
            if (File.Exists(csprojPath))
            {
                string csprojContent = File.ReadAllText(csprojPath);
                Check("T6 模板含 EnableDefaultCompileItems（NETSDK1022 修复）", csprojContent.Contains("EnableDefaultCompileItems"));
            }

            // T7 OpenAsmdefFile 路径对齐
            m_Window.OpenAsmdefFile(created);
            Check("T7 OpenAsmdefFile 绝对路径对齐", (string)GetField("m_CurrentFilePath") == created);
        }

        private void Cleanup()
        {
            if (m_Window != null && GodotObject.IsInstanceValid(m_Window))
            {
                m_Window.QueueFree();
            }

            // 测试产物清理：NewAssembly*（asmdef/csproj 及编辑器生成的 uid）
            string addonDir = Path.Combine(AsmdefPathUtility.GetProjectRootPath(), "addons", "com.gameframex.godot");
            try
            {
                foreach (string file in Directory.GetFiles(addonDir, "NewAssembly*"))
                {
                    File.Delete(file);
                }
            }
            catch (Exception exception)
            {
                GD.PrintErr($"[AsmdefSmoke] 清理失败: {exception.Message}");
            }
        }

        private void OnFileSaved(string filePath)
        {
            GD.Print($"[AsmdefSmoke] onFileSaved: {AsmdefPathUtility.ToProjectRelativePath(filePath)}");
        }

        private void OnRunSyncRequested()
        {
            m_SyncRequested = true;
        }

        private void Check(string name, bool ok)
        {
            m_Total++;
            if (ok)
            {
                m_Passed++;
                GD.Print($"[AsmdefSmoke] PASS {name}");
            }
            else
            {
                GD.PrintErr($"[AsmdefSmoke] FAIL {name}");
            }
        }

        private object GetField(string fieldName)
        {
            FieldInfo field = typeof(AsmdefEditorWindow).GetField(fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
            {
                throw new InvalidOperationException($"私有字段不存在: {fieldName}");
            }

            return field.GetValue(m_Window);
        }

        private void InvokePrivate(string methodName)
        {
            MethodInfo method = typeof(AsmdefEditorWindow).GetMethod(methodName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (method == null)
            {
                throw new InvalidOperationException($"私有方法不存在: {methodName}");
            }

            method.Invoke(m_Window, null);
        }

        private string GetControlText(string fieldName)
        {
            object control = GetField(fieldName);
            if (control is LineEdit lineEdit)
            {
                return lineEdit.Text;
            }

            if (control is TextEdit textEdit)
            {
                return textEdit.Text;
            }

            throw new InvalidOperationException($"非文本控件: {fieldName}");
        }

        private void SetControlText(string fieldName, string text)
        {
            object control = GetField(fieldName);
            if (control is LineEdit lineEdit)
            {
                lineEdit.Text = text;
                return;
            }

            if (control is TextEdit textEdit)
            {
                textEdit.Text = text;
                return;
            }

            throw new InvalidOperationException($"非文本控件: {fieldName}");
        }

        private string GetStatusText()
        {
            object label = GetField("m_StatusLabel");
            if (label is Label statusLabel)
            {
                return statusLabel.Text ?? string.Empty;
            }

            return string.Empty;
        }
    }
}
#endif
