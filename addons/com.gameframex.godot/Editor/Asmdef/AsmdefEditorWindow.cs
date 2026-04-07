#if TOOLS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;

namespace GameFrameX.Editor.Asmdef
{
    [Tool]
    public partial class AsmdefEditorWindow : Window
    {
        private OptionButton m_FileSelector;
        private Label m_FilePathLabel;
        private LineEdit m_NameEdit;
        private LineEdit m_RootNamespaceEdit;
        private TextEdit m_ReferencesEdit;
        private TextEdit m_DefinesEdit;
        private TextEdit m_IncludePlatformsEdit;
        private CheckBox m_EditorOnlyCheck;
        private Label m_StatusLabel;
        private readonly Action<string> m_OnFileSaved;
        private readonly Action m_OnRunSync;

        private List<string> m_CurrentAsmdefFiles = new List<string>();
        private string m_CurrentFilePath = string.Empty;

        public AsmdefEditorWindow(Action<string> onFileSaved, Action onRunSync)
        {
            m_OnFileSaved = onFileSaved;
            m_OnRunSync = onRunSync;
        }

        public override void _Ready()
        {
            BuildUi();
            RefreshFileList();
            CloseRequested += OnCloseRequested;
        }

        public override void _ExitTree()
        {
            CloseRequested -= OnCloseRequested;
        }

        public void OpenAsmdefFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                RefreshFileList();
                return;
            }

            string absolutePath = filePath.StartsWith("res://", StringComparison.OrdinalIgnoreCase)
                ? ProjectSettings.GlobalizePath(filePath)
                : filePath;

            m_CurrentFilePath = absolutePath;
            RefreshFileList();
            int index = m_CurrentAsmdefFiles.FindIndex(path => string.Equals(path, absolutePath, StringComparison.OrdinalIgnoreCase));
            if (index >= 0)
            {
                m_FileSelector.Select(index);
                LoadFromFile(absolutePath);
            }
        }

        private void BuildUi()
        {
            Title = "Asmdef 属性编辑器";
            MinSize = new Vector2I(960, 680);
            Size = new Vector2I(1050, 760);

            var root = new VBoxContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            root.AnchorRight = 1f;
            root.AnchorBottom = 1f;
            root.AddThemeConstantOverride("separation", 8);
            AddChild(root);

            var topRow = new HBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
            root.AddChild(topRow);

            m_FileSelector = new OptionButton { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
            m_FileSelector.ItemSelected += OnFileSelectionChanged;
            topRow.AddChild(m_FileSelector);

            var refreshButton = new Button { Text = "刷新列表" };
            refreshButton.Pressed += RefreshFileList;
            topRow.AddChild(refreshButton);

            var newButton = new Button { Text = "新建 asmdef" };
            newButton.Pressed += OnCreateNewAsmdefPressed;
            topRow.AddChild(newButton);

            m_FilePathLabel = new Label
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            root.AddChild(m_FilePathLabel);

            var form = new GridContainer { Columns = 2, SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
            form.AddThemeConstantOverride("h_separation", 8);
            form.AddThemeConstantOverride("v_separation", 8);
            root.AddChild(form);

            form.AddChild(new Label { Text = "程序集名称（name）" });
            m_NameEdit = new LineEdit();
            form.AddChild(m_NameEdit);

            form.AddChild(new Label { Text = "根命名空间（rootNamespace）" });
            m_RootNamespaceEdit = new LineEdit();
            form.AddChild(m_RootNamespaceEdit);

            form.AddChild(new Label { Text = "仅编辑器（editorOnly）" });
            m_EditorOnlyCheck = new CheckBox { Text = "启用" };
            form.AddChild(m_EditorOnlyCheck);

            root.AddChild(CreateTextEditorGroup("引用列表（references，一行一个）", out m_ReferencesEdit));
            root.AddChild(CreateTextEditorGroup("宏定义（defines，一行一个）", out m_DefinesEdit));
            root.AddChild(CreateTextEditorGroup("包含平台（includePlatforms，一行一个）", out m_IncludePlatformsEdit));

            var actionRow = new HBoxContainer();
            actionRow.AddThemeConstantOverride("separation", 8);
            root.AddChild(actionRow);

            var validateButton = new Button { Text = "校验全部 asmdef" };
            validateButton.Pressed += OnValidatePressed;
            actionRow.AddChild(validateButton);

            var saveButton = new Button { Text = "保存当前 asmdef" };
            saveButton.Pressed += OnSavePressed;
            actionRow.AddChild(saveButton);

            var syncButton = new Button { Text = "立即生成 csproj" };
            syncButton.Pressed += OnSyncPressed;
            actionRow.AddChild(syncButton);

            m_StatusLabel = new Label
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            actionRow.AddChild(m_StatusLabel);
        }

        private static VBoxContainer CreateTextEditorGroup(string title, out TextEdit editor)
        {
            var container = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill, SizeFlagsVertical = Control.SizeFlags.ExpandFill };
            container.AddChild(new Label { Text = title });
            editor = new TextEdit
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill,
                CustomMinimumSize = new Vector2(0f, 100f)
            };
            container.AddChild(editor);
            return container;
        }

        private void RefreshFileList()
        {
            m_CurrentAsmdefFiles = AsmdefPathUtility.FindAllAsmdefFiles();
            m_FileSelector.Clear();
            for (int i = 0; i < m_CurrentAsmdefFiles.Count; i++)
            {
                string projectPath = AsmdefPathUtility.ToProjectRelativePath(m_CurrentAsmdefFiles[i]);
                m_FileSelector.AddItem(projectPath, i);
            }

            if (m_CurrentAsmdefFiles.Count == 0)
            {
                ClearForm();
                SetStatus("当前项目尚未发现 asmdef 文件。", false);
                return;
            }

            int index = FindCurrentFileIndex();
            m_FileSelector.Select(index);
            LoadFromFile(m_CurrentAsmdefFiles[index]);
        }

        private int FindCurrentFileIndex()
        {
            if (!string.IsNullOrWhiteSpace(m_CurrentFilePath))
            {
                int currentIndex = m_CurrentAsmdefFiles.FindIndex(path => string.Equals(path, m_CurrentFilePath, StringComparison.OrdinalIgnoreCase));
                if (currentIndex >= 0)
                {
                    return currentIndex;
                }
            }

            return 0;
        }

        private void OnFileSelectionChanged(long index)
        {
            if (index < 0 || index >= m_CurrentAsmdefFiles.Count)
            {
                return;
            }

            LoadFromFile(m_CurrentAsmdefFiles[(int)index]);
        }

        private void LoadFromFile(string filePath)
        {
            try
            {
                AsmdefDocument document = AsmdefIO.LoadDocument(filePath);
                m_CurrentFilePath = filePath;
                m_FilePathLabel.Text = AsmdefPathUtility.ToProjectRelativePath(filePath);
                m_NameEdit.Text = document.Model.Name ?? string.Empty;
                m_RootNamespaceEdit.Text = document.Model.RootNamespace ?? string.Empty;
                m_ReferencesEdit.Text = string.Join("\n", document.Model.References ?? new List<string>());
                m_DefinesEdit.Text = string.Join("\n", document.Model.Defines ?? new List<string>());
                m_IncludePlatformsEdit.Text = string.Join("\n", document.Model.IncludePlatforms ?? new List<string>());
                m_EditorOnlyCheck.ButtonPressed = document.Model.EditorOnly;
                SetStatus("asmdef 已加载。");
            }
            catch (Exception exception)
            {
                SetStatus($"加载失败：{exception.Message}", true);
            }
        }

        private void OnSavePressed()
        {
            if (string.IsNullOrWhiteSpace(m_CurrentFilePath))
            {
                SetStatus("请先选择一个 asmdef 文件。", true);
                return;
            }

            try
            {
                AsmdefModel model = BuildModelFromForm();
                AsmdefIO.SaveDocument(m_CurrentFilePath, model);
                m_OnFileSaved?.Invoke(m_CurrentFilePath);
                SetStatus("asmdef 已保存。");
            }
            catch (Exception exception)
            {
                SetStatus($"保存失败：{exception.Message}", true);
            }
        }

        private void OnValidatePressed()
        {
            var documents = new List<AsmdefDocument>();
            var issues = new List<AsmdefValidationIssue>();
            foreach (string path in m_CurrentAsmdefFiles)
            {
                try
                {
                    documents.Add(AsmdefIO.LoadDocument(path));
                }
                catch (Exception exception)
                {
                    issues.Add(new AsmdefValidationIssue
                    {
                        Severity = AsmdefIssueSeverity.Error,
                        FilePath = path,
                        Message = $"读取失败：{exception.Message}"
                    });
                }
            }

            AsmdefValidationResult result = AsmdefValidator.Validate(documents);
            issues.AddRange(result.Issues);
            if (issues.Count == 0)
            {
                SetStatus("校验通过。");
                return;
            }

            int errorCount = issues.Count(static x => x.Severity == AsmdefIssueSeverity.Error);
            int warningCount = issues.Count - errorCount;
            string firstIssue = issues[0].Message;
            SetStatus($"校验完成：错误 {errorCount}，警告 {warningCount}。首条：{firstIssue}", errorCount > 0);
        }

        private void OnSyncPressed()
        {
            m_OnRunSync?.Invoke();
            SetStatus("已触发生成。");
        }

        private void OnCreateNewAsmdefPressed()
        {
            string projectRoot = AsmdefPathUtility.GetProjectRootPath();
            string defaultDirectory = Path.Combine(projectRoot, "addons", "com.gameframex.godot");
            string newName = "NewAssembly";
            string filePath = Path.Combine(defaultDirectory, $"{newName}.asmdef");
            int suffix = 1;
            while (File.Exists(filePath))
            {
                filePath = Path.Combine(defaultDirectory, $"{newName}{suffix}.asmdef");
                suffix++;
            }

            var model = new AsmdefModel
            {
                Name = Path.GetFileNameWithoutExtension(filePath),
                RootNamespace = Path.GetFileNameWithoutExtension(filePath),
                AutoReferenced = true
            };

            try
            {
                AsmdefIO.SaveDocument(filePath, model);
                m_OnFileSaved?.Invoke(filePath);
                RefreshFileList();
                int index = m_CurrentAsmdefFiles.FindIndex(path => string.Equals(path, filePath, StringComparison.OrdinalIgnoreCase));
                if (index >= 0)
                {
                    m_FileSelector.Select(index);
                    LoadFromFile(filePath);
                }

                SetStatus($"已创建：{AsmdefPathUtility.ToProjectRelativePath(filePath)}");
            }
            catch (Exception exception)
            {
                SetStatus($"创建失败：{exception.Message}", true);
            }
        }

        private AsmdefModel BuildModelFromForm()
        {
            AsmdefDocument current = AsmdefIO.LoadDocument(m_CurrentFilePath);
            AsmdefModel model = current.Model;
            model.Name = (m_NameEdit.Text ?? string.Empty).Trim();
            model.RootNamespace = (m_RootNamespaceEdit.Text ?? string.Empty).Trim();
            model.EditorOnly = m_EditorOnlyCheck.ButtonPressed;
            model.References = ParseLines(m_ReferencesEdit.Text);
            model.Defines = ParseLines(m_DefinesEdit.Text);
            model.IncludePlatforms = ParseLines(m_IncludePlatformsEdit.Text);
            return model;
        }

        private static List<string> ParseLines(string text)
        {
            return (text ?? string.Empty)
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(static x => x.Trim())
                .Where(static x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.Ordinal)
                .ToList();
        }

        private void ClearForm()
        {
            m_CurrentFilePath = string.Empty;
            m_FilePathLabel.Text = string.Empty;
            m_NameEdit.Text = string.Empty;
            m_RootNamespaceEdit.Text = string.Empty;
            m_ReferencesEdit.Text = string.Empty;
            m_DefinesEdit.Text = string.Empty;
            m_IncludePlatformsEdit.Text = string.Empty;
            m_EditorOnlyCheck.ButtonPressed = false;
        }

        private void SetStatus(string message, bool isError = false)
        {
            if (m_StatusLabel == null)
            {
                return;
            }

            m_StatusLabel.Text = message;
            m_StatusLabel.Modulate = isError ? new Color(1f, 0.62f, 0.62f) : new Color(0.62f, 0.95f, 0.62f);
        }

        private void OnCloseRequested()
        {
            Hide();
        }
    }
}
#endif
