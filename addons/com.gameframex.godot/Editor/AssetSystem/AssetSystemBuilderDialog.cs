#if TOOLS
using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace GameFrameX.Editor
{
    /// <summary>
    /// Godot 资源打包窗口（PCK）。
    /// </summary>
    [Tool]
    public partial class AssetSystemBuilderDialog : Window
    {
        private const string DefaultSourcePath = "res://Assets/Probe/teamgame_external.png";
        private const string DefaultMountPath = "probe_runtime/teamgame_external.png";
        private const string DefaultOutputPath = "user://assetsystem_runtime_verify/yoo/runtime_verify/verify_content.pck";
        private const string DefaultBatchConfigSavePath = "user://assetsystem_builder/package_list_config.json";
        private const string DefaultBatchConfigJson = """
{
  "packages": [
    {
      "packageName": "runtime_verify",
      "outputPck": "user://assetsystem_runtime_verify/yoo/runtime_verify/verify_content.pck",
      "files": [
        {
          "sourcePath": "res://Assets/Probe/teamgame_external.png",
          "mountPath": "probe_runtime/teamgame_external.png"
        }
      ]
    },
    {
      "packageName": "ui_icons",
      "outputPck": "user://assetsystem_runtime_verify/yoo/runtime_verify/ui_icons.pck",
      "files": [
        {
          "sourcePath": "res://addons/com.gameframex.godot/Resources/gameframex_logo.png",
          "mountPath": "ui/logo.png"
        }
      ]
    }
  ]
}
""";

        private LineEdit _sourcePathEdit;
        private LineEdit _mountPathEdit;
        private LineEdit _outputPathEdit;
        private TextEdit _batchConfigEdit;
        private ItemList _packableAssetList;
        private Label _packableAssetCountLabel;
        private RichTextLabel _logText;
        private Label _resultLabel;
        private bool _initialized;
        private static readonly HashSet<string> PackableExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".png", ".jpg", ".jpeg", ".webp", ".bmp", ".svg", ".tga", ".exr", ".hdr", ".ktx", ".ktx2",
            ".tscn", ".scn", ".tres", ".res",
            ".wav", ".ogg", ".mp3",
            ".glb", ".gltf", ".obj",
            ".ttf", ".otf", ".fnt",
            ".json", ".txt", ".bytes", ".cfg", ".csv",
            ".shader", ".gdshader", ".material"
        };

        public override void _Ready()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
            BuildUi();
            ApplyDefaults();
            CloseRequested += OnCloseRequested;
        }

        public override void _ExitTree()
        {
            CloseRequested -= OnCloseRequested;
        }

        private void BuildUi()
        {
            Title = "GameFrameX Asset Builder (Godot PCK)";
            MinSize = new Vector2I(1120, 640);

            var root = new VBoxContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            root.AnchorLeft = 0f;
            root.AnchorTop = 0f;
            root.AnchorRight = 1f;
            root.AnchorBottom = 1f;
            root.OffsetLeft = 0f;
            root.OffsetTop = 0f;
            root.OffsetRight = 0f;
            root.OffsetBottom = 0f;
            AddChild(root);

            var mainSplit = new HSplitContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill,
                SplitOffsets = new[] { 320 }
            };
            root.AddChild(mainSplit);

            var leftPanel = new PanelContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.Fill,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill,
                CustomMinimumSize = new Vector2(300f, 0f)
            };
            mainSplit.AddChild(leftPanel);

            var leftMargin = new MarginContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            leftMargin.AddThemeConstantOverride("margin_top", 10);
            leftMargin.AddThemeConstantOverride("margin_left", 10);
            leftMargin.AddThemeConstantOverride("margin_right", 10);
            leftMargin.AddThemeConstantOverride("margin_bottom", 10);
            leftPanel.AddChild(leftMargin);

            var leftBox = new VBoxContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            leftBox.AddThemeConstantOverride("separation", 8);
            leftMargin.AddChild(leftBox);

            var assetTitleRow = new HBoxContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            assetTitleRow.AddThemeConstantOverride("separation", 8);
            leftBox.AddChild(assetTitleRow);

            var assetTitle = new Label
            {
                Text = "Packable Assets",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                VerticalAlignment = VerticalAlignment.Center
            };
            assetTitleRow.AddChild(assetTitle);

            var refreshAssetsButton = new Button { Text = "Refresh Assets" };
            refreshAssetsButton.Pressed += OnRefreshPackableAssetsPressed;
            assetTitleRow.AddChild(refreshAssetsButton);

            _packableAssetCountLabel = new Label
            {
                Text = "Count: 0",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            leftBox.AddChild(_packableAssetCountLabel);

            var useAssetButton = new Button
            {
                Text = "Use Selected As Source",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            useAssetButton.Pressed += OnUseSelectedPackableAssetPressed;
            leftBox.AddChild(useAssetButton);

            _packableAssetList = new ItemList
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill,
                SelectMode = ItemList.SelectModeEnum.Single
            };
            _packableAssetList.ItemActivated += OnPackableAssetItemActivated;
            leftBox.AddChild(_packableAssetList);

            var rightBox = new VBoxContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            rightBox.AddThemeConstantOverride("separation", 8);
            mainSplit.AddChild(rightBox);

            var formPanel = new PanelContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            rightBox.AddChild(formPanel);

            var formMargin = new MarginContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            formMargin.AddThemeConstantOverride("margin_top", 10);
            formMargin.AddThemeConstantOverride("margin_left", 10);
            formMargin.AddThemeConstantOverride("margin_right", 10);
            formMargin.AddThemeConstantOverride("margin_bottom", 10);
            formPanel.AddChild(formMargin);

            var form = new VBoxContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            form.AddThemeConstantOverride("separation", 8);
            formMargin.AddChild(form);

            _sourcePathEdit = AddLabeledLineEdit(form, "Source (res:// or absolute):");
            _mountPathEdit = AddLabeledLineEdit(form, "Mount Path (inside pck):");
            _outputPathEdit = AddLabeledLineEdit(form, "Output PCK (user:// or absolute):");

            var buttonRow = new HBoxContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            buttonRow.AddThemeConstantOverride("separation", 8);
            rightBox.AddChild(buttonRow);

            var buildButton = new Button { Text = "Build PCK" };
            buildButton.Pressed += OnBuildPressed;
            buttonRow.AddChild(buildButton);

            var resetButton = new Button { Text = "Reset Defaults" };
            resetButton.Pressed += ApplyDefaults;
            buttonRow.AddChild(resetButton);

            var openButton = new Button { Text = "Open Output Folder" };
            openButton.Pressed += OnOpenOutputFolderPressed;
            buttonRow.AddChild(openButton);

            var clearLogButton = new Button { Text = "Clear Log" };
            clearLogButton.Pressed += OnClearLogPressed;
            buttonRow.AddChild(clearLogButton);

            var refreshLayoutButton = new Button { Text = "Refresh Layout" };
            refreshLayoutButton.Pressed += OnRefreshLayoutPressed;
            buttonRow.AddChild(refreshLayoutButton);

            _resultLabel = new Label
            {
                Text = "Build Result: NOT RUN",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };
            buttonRow.AddChild(_resultLabel);

            var batchPanel = new PanelContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            rightBox.AddChild(batchPanel);

            var batchMargin = new MarginContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            batchMargin.AddThemeConstantOverride("margin_top", 10);
            batchMargin.AddThemeConstantOverride("margin_left", 10);
            batchMargin.AddThemeConstantOverride("margin_right", 10);
            batchMargin.AddThemeConstantOverride("margin_bottom", 10);
            batchPanel.AddChild(batchMargin);

            var batchBox = new VBoxContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            batchBox.AddThemeConstantOverride("separation", 8);
            batchMargin.AddChild(batchBox);

            var batchTitle = new Label
            {
                Text = "Package List Mode (JSON):",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            batchBox.AddChild(batchTitle);

            _batchConfigEdit = new TextEdit
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsVertical = Control.SizeFlags.Fill,
                CustomMinimumSize = new Vector2(0f, 160f),
                WrapMode = TextEdit.LineWrappingMode.Boundary
            };
            batchBox.AddChild(_batchConfigEdit);

            var batchButtonRow = new HBoxContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            batchButtonRow.AddThemeConstantOverride("separation", 8);
            batchBox.AddChild(batchButtonRow);

            var fillTemplateButton = new Button { Text = "Load Batch Template" };
            fillTemplateButton.Pressed += OnLoadBatchTemplatePressed;
            batchButtonRow.AddChild(fillTemplateButton);

            var refreshBatchButton = new Button { Text = "Refresh" };
            refreshBatchButton.Pressed += OnRefreshBatchConfigPressed;
            batchButtonRow.AddChild(refreshBatchButton);

            var saveBatchButton = new Button { Text = "Save" };
            saveBatchButton.Pressed += OnSaveBatchConfigPressed;
            batchButtonRow.AddChild(saveBatchButton);

            var buildBatchButton = new Button { Text = "Build All" };
            buildBatchButton.Pressed += OnBuildBatchPressed;
            batchButtonRow.AddChild(buildBatchButton);

            var hint = new Label
            {
                Text = "Use with [LoadProbe]: default output points to user://assetsystem_runtime_verify/yoo/runtime_verify/verify_content.pck.",
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            rightBox.AddChild(hint);

            var logPanel = new PanelContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            rightBox.AddChild(logPanel);

            _logText = new RichTextLabel
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill,
                CustomMinimumSize = new Vector2(0f, 220f),
                SelectionEnabled = true,
                ScrollFollowing = true
            };
            logPanel.AddChild(_logText);
        }

        private void ApplyDefaults()
        {
            _sourcePathEdit.Text = DefaultSourcePath;
            _mountPathEdit.Text = DefaultMountPath;
            _outputPathEdit.Text = DefaultOutputPath;
            _batchConfigEdit.Text = DefaultBatchConfigJson;
            RefreshPackableAssets();
            AppendLog("Defaults restored.");
        }

        private void OnRefreshPackableAssetsPressed()
        {
            RefreshPackableAssets();
            SetBuildResult(true, $"asset list refreshed, count={_packableAssetList?.ItemCount ?? 0}");
        }

        private void OnUseSelectedPackableAssetPressed()
        {
            if (_packableAssetList == null)
            {
                SetBuildResult(false, "asset list is not ready.");
                return;
            }

            var selectedItems = _packableAssetList.GetSelectedItems();
            if (selectedItems.Length == 0)
            {
                SetBuildResult(false, "please select one asset in left list.");
                return;
            }

            ApplySelectedAssetToInputs(_packableAssetList.GetItemText(selectedItems[0]));
        }

        private void OnPackableAssetItemActivated(long index)
        {
            if (_packableAssetList == null)
            {
                return;
            }

            if (index < 0 || index >= _packableAssetList.ItemCount)
            {
                return;
            }

            ApplySelectedAssetToInputs(_packableAssetList.GetItemText((int)index));
        }

        private void ApplySelectedAssetToInputs(string resourcePath)
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                return;
            }

            _sourcePathEdit.Text = resourcePath;
            var normalized = NormalizeMountPath(resourcePath);
            if (normalized.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
            {
                normalized = normalized["Assets/".Length..];
            }

            _mountPathEdit.Text = normalized;
            AppendLog($"[Assets] selected source={resourcePath} mount={normalized}");
        }

        private void RefreshPackableAssets()
        {
            if (_packableAssetList == null)
            {
                return;
            }

            _packableAssetList.Clear();
            var projectRoot = ProjectSettings.GlobalizePath("res://").Replace('\\', '/').TrimEnd('/');
            if (Directory.Exists(projectRoot) == false)
            {
                if (_packableAssetCountLabel != null)
                {
                    _packableAssetCountLabel.Text = "Count: 0";
                }
                return;
            }

            var files = Directory.GetFiles(projectRoot, "*", SearchOption.AllDirectories);
            var resourcePaths = new List<string>(files.Length);
            for (var i = 0; i < files.Length; i++)
            {
                var fullPath = files[i].Replace('\\', '/');
                if (IsPackableAssetFile(fullPath) == false)
                {
                    continue;
                }

                if (fullPath.StartsWith(projectRoot + "/", StringComparison.OrdinalIgnoreCase) == false)
                {
                    continue;
                }

                var relativePath = fullPath[(projectRoot.Length + 1)..];
                resourcePaths.Add("res://" + relativePath);
            }

            resourcePaths.Sort(StringComparer.OrdinalIgnoreCase);
            for (var i = 0; i < resourcePaths.Count; i++)
            {
                _packableAssetList.AddItem(resourcePaths[i]);
            }

            if (_packableAssetCountLabel != null)
            {
                _packableAssetCountLabel.Text = $"Count: {resourcePaths.Count}";
            }
        }

        private static bool IsPackableAssetFile(string fullPath)
        {
            if (string.IsNullOrWhiteSpace(fullPath))
            {
                return false;
            }

            if (fullPath.EndsWith(".import", StringComparison.OrdinalIgnoreCase) ||
                fullPath.EndsWith(".uid", StringComparison.OrdinalIgnoreCase) ||
                fullPath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) ||
                fullPath.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var extension = Path.GetExtension(fullPath);
            return PackableExtensions.Contains(extension);
        }

        private void OnBuildPressed()
        {
            var sourcePath = ResolveFilePath(_sourcePathEdit.Text);
            if (File.Exists(sourcePath) == false)
            {
                SetBuildResult(false, $"source missing: {sourcePath}");
                return;
            }

            var mountPath = NormalizeMountPath(_mountPathEdit.Text);
            if (string.IsNullOrWhiteSpace(mountPath))
            {
                SetBuildResult(false, "mount path is empty.");
                return;
            }

            var outputPath = ResolveFilePath(_outputPathEdit.Text);
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                SetBuildResult(false, "output path is empty.");
                return;
            }

            var buildFiles = new List<BuildFileItem>(1)
            {
                new BuildFileItem(sourcePath, mountPath)
            };

            if (TryBuildPackage("single", outputPath, buildFiles, out var outputLength, out var error) == false)
            {
                SetBuildResult(false, error);
                return;
            }

            AppendLog($"PASS source={sourcePath}");
            AppendLog($"PASS output={outputPath} bytes={outputLength}");
            AppendLog($"PASS mount={mountPath}");
            SetBuildResult(true, $"output={outputPath} bytes={outputLength}");
        }

        private void OnLoadBatchTemplatePressed()
        {
            _batchConfigEdit.Text = DefaultBatchConfigJson;
            AppendLog("Batch template loaded.");
        }

        private void OnRefreshBatchConfigPressed()
        {
            var configPath = ResolveFilePath(DefaultBatchConfigSavePath);
            if (File.Exists(configPath) == false)
            {
                SetBuildResult(false, $"refresh failed, config not found: {configPath}");
                return;
            }

            try
            {
                var content = File.ReadAllText(configPath);
                if (string.IsNullOrWhiteSpace(content))
                {
                    SetBuildResult(false, $"refresh failed, config is empty: {configPath}");
                    return;
                }

                _batchConfigEdit.Text = content;
                SetBuildResult(true, $"refreshed from local file: {configPath}");
            }
            catch (Exception exception)
            {
                SetBuildResult(false, $"refresh failed: {exception.Message}");
            }
        }

        private void OnSaveBatchConfigPressed()
        {
            if (TryParseBatchConfig(out var config, out var parseError) == false)
            {
                SetBuildResult(false, parseError);
                return;
            }

            if (TrySaveBatchConfig(config, out var saveSummary, out var saveError) == false)
            {
                SetBuildResult(false, saveError);
                return;
            }

            SetBuildResult(true, $"config saved: {saveSummary}");
        }

        private void OnBuildBatchPressed()
        {
            if (TryParseBatchConfig(out var config, out var parseError) == false)
            {
                SetBuildResult(false, parseError);
                return;
            }

            if (TrySaveBatchConfig(config, out var saveSummary, out var saveError) == false)
            {
                SetBuildResult(false, saveError);
                return;
            }

            AppendLog($"[Batch] config saved before build: {saveSummary}");

            var successCount = 0;
            var failCount = 0;
            for (var i = 0; i < config.Packages.Count; i++)
            {
                var package = config.Packages[i];
                var packageName = string.IsNullOrWhiteSpace(package.PackageName) ? $"package_{i + 1}" : package.PackageName.Trim();
                if (string.IsNullOrWhiteSpace(package.OutputPck))
                {
                    failCount++;
                    AppendLog($"[Batch][{packageName}] FAIL outputPck is empty.");
                    continue;
                }

                if (package.Files == null || package.Files.Count == 0)
                {
                    failCount++;
                    AppendLog($"[Batch][{packageName}] FAIL files is empty.");
                    continue;
                }

                var outputPath = ResolveFilePath(package.OutputPck);
                var buildFiles = new List<BuildFileItem>(package.Files.Count);
                var packageInputInvalid = false;
                for (var j = 0; j < package.Files.Count; j++)
                {
                    var file = package.Files[j];
                    var sourcePath = ResolveFilePath(file.SourcePath);
                    if (File.Exists(sourcePath) == false)
                    {
                        packageInputInvalid = true;
                        AppendLog($"[Batch][{packageName}] FAIL source missing: {sourcePath}");
                        continue;
                    }

                    var mountPath = NormalizeMountPath(file.MountPath);
                    if (string.IsNullOrWhiteSpace(mountPath))
                    {
                        packageInputInvalid = true;
                        AppendLog($"[Batch][{packageName}] FAIL mount path is empty for source={sourcePath}");
                        continue;
                    }

                    buildFiles.Add(new BuildFileItem(sourcePath, mountPath));
                }

                if (packageInputInvalid || buildFiles.Count == 0)
                {
                    failCount++;
                    continue;
                }

                if (TryBuildPackage(packageName, outputPath, buildFiles, out var outputLength, out var error) == false)
                {
                    failCount++;
                    AppendLog($"[Batch][{packageName}] FAIL {error}");
                    continue;
                }

                successCount++;
                AppendLog($"[Batch][{packageName}] PASS output={outputPath} bytes={outputLength} files={buildFiles.Count}");
            }

            var summary = $"batch complete success={successCount} fail={failCount}";
            SetBuildResult(failCount == 0, summary);
        }

        private bool TryParseBatchConfig(out BatchBuildConfig config, out string error)
        {
            config = null;
            error = string.Empty;
            var configText = _batchConfigEdit.Text;
            if (string.IsNullOrWhiteSpace(configText))
            {
                error = "batch config is empty.";
                return false;
            }

            try
            {
                config = JsonSerializer.Deserialize<BatchBuildConfig>(configText, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (Exception exception)
            {
                error = $"batch config parse: {exception.Message}";
                return false;
            }

            if (config?.Packages == null || config.Packages.Count == 0)
            {
                error = "batch config has no packages.";
                return false;
            }

            return true;
        }

        private bool TrySaveBatchConfig(BatchBuildConfig config, out string summary, out string error)
        {
            summary = string.Empty;
            error = string.Empty;
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var fullConfigPath = ResolveFilePath(DefaultBatchConfigSavePath);
                var fullConfigDirectory = Path.GetDirectoryName(fullConfigPath);
                if (string.IsNullOrWhiteSpace(fullConfigDirectory))
                {
                    error = $"invalid save path: {fullConfigPath}";
                    return false;
                }

                Directory.CreateDirectory(fullConfigDirectory);
                File.WriteAllText(fullConfigPath, JsonSerializer.Serialize(config, options));

                var packageDirectory = Path.Combine(fullConfigDirectory, "packages").Replace('\\', '/');
                Directory.CreateDirectory(packageDirectory);
                var oldConfigFiles = Directory.GetFiles(packageDirectory, "*.json");
                for (var i = 0; i < oldConfigFiles.Length; i++)
                {
                    File.Delete(oldConfigFiles[i]);
                }

                var packageWriteCount = 0;
                for (var i = 0; i < config.Packages.Count; i++)
                {
                    var package = config.Packages[i];
                    var packageName = string.IsNullOrWhiteSpace(package.PackageName) ? $"package_{i + 1}" : package.PackageName.Trim();
                    var safePackageName = MakeSafeFileName(packageName);
                    var packageConfigPath = Path.Combine(packageDirectory, $"{safePackageName}.json").Replace('\\', '/');
                    File.WriteAllText(packageConfigPath, JsonSerializer.Serialize(package, options));
                    packageWriteCount++;
                }

                summary = $"root={fullConfigPath}, perPackage={packageWriteCount}";
                AppendLog($"[Config] saved {summary}");
                return true;
            }
            catch (Exception exception)
            {
                error = $"save config: {exception.Message}";
                return false;
            }
        }

        private void OnOpenOutputFolderPressed()
        {
            var outputPath = ResolveFilePath(_outputPathEdit.Text);
            var outputDirectory = Path.GetDirectoryName(outputPath);
            if (string.IsNullOrWhiteSpace(outputDirectory) || Directory.Exists(outputDirectory) == false)
            {
                SetBuildResult(false, $"output folder missing: {outputDirectory}");
                return;
            }

            var folderUri = new Uri(outputDirectory + Path.DirectorySeparatorChar).AbsoluteUri;
            OS.ShellOpen(folderUri);
            AppendLog($"OPEN {outputDirectory}");
        }

        private void OnClearLogPressed()
        {
            _logText.Clear();
            AppendLog("Log cleared.");
        }

        private void OnRefreshLayoutPressed()
        {
            var sourceText = _sourcePathEdit?.Text ?? DefaultSourcePath;
            var mountText = _mountPathEdit?.Text ?? DefaultMountPath;
            var outputText = _outputPathEdit?.Text ?? DefaultOutputPath;
            var batchText = _batchConfigEdit?.Text ?? DefaultBatchConfigJson;

            var children = GetChildren();
            for (var i = 0; i < children.Count; i++)
            {
                if (children[i] is Node child)
                {
                    RemoveChild(child);
                    child.QueueFree();
                }
            }

            BuildUi();
            _sourcePathEdit.Text = sourceText;
            _mountPathEdit.Text = mountText;
            _outputPathEdit.Text = outputText;
            _batchConfigEdit.Text = string.IsNullOrWhiteSpace(batchText) ? DefaultBatchConfigJson : batchText;
            RefreshPackableAssets();
            SetBuildResult(true, "layout refreshed.");
        }

        private void OnCloseRequested()
        {
            Hide();
        }

        private static LineEdit AddLabeledLineEdit(VBoxContainer form, string labelText)
        {
            var row = new HBoxContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            row.AddThemeConstantOverride("separation", 10);
            form.AddChild(row);

            var label = new Label
            {
                Text = labelText,
                CustomMinimumSize = new Vector2(300f, 0f),
                SizeFlagsHorizontal = Control.SizeFlags.Fill,
                VerticalAlignment = VerticalAlignment.Center
            };
            row.AddChild(label);

            var lineEdit = new LineEdit
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            row.AddChild(lineEdit);
            return lineEdit;
        }

        private void AppendLog(string message)
        {
            var text = $"[{DateTime.Now:HH:mm:ss}] {message}\n";
            _logText.AppendText(text);
            GD.Print($"[AssetBuilder] {message}");
        }

        private void SetBuildResult(bool succeed, string detail)
        {
            if (_resultLabel != null)
            {
                if (succeed)
                {
                    _resultLabel.Text = "Build Result: SUCCESS";
                    _resultLabel.Modulate = new Color(0.62f, 0.95f, 0.62f);
                }
                else
                {
                    _resultLabel.Text = "Build Result: FAILED";
                    _resultLabel.Modulate = new Color(1f, 0.62f, 0.62f);
                }
            }

            var resultText = succeed ? "[RESULT] SUCCESS" : "[RESULT] FAILED";
            if (string.IsNullOrWhiteSpace(detail))
            {
                AppendLog(resultText);
            }
            else
            {
                AppendLog($"{resultText} {detail}");
            }
        }

        private static string ResolveFilePath(string inputPath)
        {
            if (string.IsNullOrWhiteSpace(inputPath))
            {
                return string.Empty;
            }

            var trimmed = inputPath.Trim();
            if (trimmed.StartsWith("res://", StringComparison.OrdinalIgnoreCase) ||
                trimmed.StartsWith("user://", StringComparison.OrdinalIgnoreCase))
            {
                return ProjectSettings.GlobalizePath(trimmed).Replace('\\', '/');
            }

            if (Path.IsPathRooted(trimmed))
            {
                return trimmed.Replace('\\', '/');
            }

            var projectRoot = ProjectSettings.GlobalizePath("res://").Replace('\\', '/');
            return Path.Combine(projectRoot, trimmed).Replace('\\', '/');
        }

        private static string NormalizeMountPath(string inputMountPath)
        {
            if (string.IsNullOrWhiteSpace(inputMountPath))
            {
                return string.Empty;
            }

            var normalized = inputMountPath.Trim().Replace('\\', '/');
            if (normalized.StartsWith("res://", StringComparison.OrdinalIgnoreCase))
            {
                normalized = normalized["res://".Length..];
            }

            return normalized.TrimStart('/');
        }

        private static string MakeSafeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "package";
            }

            var invalidChars = Path.GetInvalidFileNameChars();
            var chars = value.ToCharArray();
            for (var i = 0; i < chars.Length; i++)
            {
                for (var j = 0; j < invalidChars.Length; j++)
                {
                    if (chars[i] == invalidChars[j])
                    {
                        chars[i] = '_';
                        break;
                    }
                }
            }

            return new string(chars);
        }

        private static bool TryBuildPackage(string packageName, string outputPath, List<BuildFileItem> buildFiles, out long outputLength, out string error)
        {
            outputLength = 0;
            error = string.Empty;

            if (string.IsNullOrWhiteSpace(outputPath))
            {
                error = "output path is empty.";
                return false;
            }

            var outputDirectory = Path.GetDirectoryName(outputPath);
            if (string.IsNullOrWhiteSpace(outputDirectory))
            {
                error = $"invalid output path: {outputPath}";
                return false;
            }

            Directory.CreateDirectory(outputDirectory);

            var packer = new PckPacker();
            var startResult = packer.PckStart(outputPath);
            if (startResult != Error.Ok)
            {
                error = $"pck start: {startResult}";
                return false;
            }

            for (var i = 0; i < buildFiles.Count; i++)
            {
                var file = buildFiles[i];
                var addResult = packer.AddFile(file.MountPath, file.SourcePath);
                if (addResult != Error.Ok)
                {
                    error = $"add file: {addResult} (package={packageName}, mount={file.MountPath})";
                    return false;
                }
            }

            var flushResult = packer.Flush();
            if (flushResult != Error.Ok)
            {
                error = $"flush: {flushResult}";
                return false;
            }

            outputLength = new FileInfo(outputPath).Length;
            return true;
        }

        private readonly struct BuildFileItem
        {
            public BuildFileItem(string sourcePath, string mountPath)
            {
                SourcePath = sourcePath;
                MountPath = mountPath;
            }

            public string SourcePath { get; }
            public string MountPath { get; }
        }

        private sealed class BatchBuildConfig
        {
            public List<BatchBuildPackage> Packages { get; set; } = new List<BatchBuildPackage>();
        }

        private sealed class BatchBuildPackage
        {
            public string PackageName { get; set; } = string.Empty;
            public string OutputPck { get; set; } = string.Empty;
            public List<BatchBuildFile> Files { get; set; } = new List<BatchBuildFile>();
        }

        private sealed class BatchBuildFile
        {
            public string SourcePath { get; set; } = string.Empty;
            public string MountPath { get; set; } = string.Empty;
        }
    }
}
#endif
