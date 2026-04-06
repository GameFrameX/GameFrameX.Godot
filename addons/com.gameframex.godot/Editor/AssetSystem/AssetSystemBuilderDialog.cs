#if TOOLS
using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using YooAsset;

namespace GameFrameX.Editor
{
	/// <summary>
	/// Godot 资源打包窗口（PCK）。
	/// </summary>
	[Tool]
	public partial class AssetSystemBuilderDialog : Window
	{
		private const string BuildBackendPck = "PCK";
		private const string BuildBackendYooAsset = "YooAsset";
		private const string BuildBackendBoth = "Both";
		private const string YooBuildPipelineName = "RawFileBuildPipeline";
		private const string YooDefaultVersionFormat = "yyyyMMddHHmmss";
		private const string YooDefaultBuildManifestFileName = "build_manifest.txt";
		private const string YooDefaultBuildVersionFileName = "build_version.txt";
		private const string DefaultSourcePath = "res://Assets/Probe/teamgame_external.png";
		private const string DefaultMountPath = "probe_runtime/teamgame_external.png";
		private const string DefaultOutputPath = "user://assetsystem_runtime_verify/yoo/runtime_verify/verify_content.pck";
		private const string DefaultBatchConfigSavePath = "user://assetsystem_builder/package_list_config.json";
		private const string DefaultBatchConfigJson = """
{
  "buildBackend": "Both",
  "packages": [
    {
	  "packageName": "runtime_verify",
	  "outputPck": "user://assetsystem_runtime_verify/yoo/runtime_verify/verify_content.pck",
	  "outputYoo": "user://assetsystem_builder/yoo/runtime_verify",
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
	  "outputYoo": "user://assetsystem_builder/yoo/ui_icons",
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
		private LineEdit _packageSearchEdit;
		private ItemList _packageList;
		private Label _currentPackageLabel;
		private OptionButton _buildBackendOptions;
		private LineEdit _packageOutputEdit;
		private LineEdit _packageYooOutputEdit;
		private PackageAssetTree _packageAssetTree;
		private LineEdit _globalSearchEdit;
		private LineEdit _globalPathFilterEdit;
		private OptionButton _globalTypeFilter;
		private GlobalAssetTree _globalAssetTree;
		private ConfirmationDialog _confirmDialog;
		private Action _pendingConfirmAction;
		private readonly List<GlobalAssetItem> _globalAssets = new List<GlobalAssetItem>();
		private readonly List<BatchBuildPackage> _packageMappings = new List<BatchBuildPackage>();
		private readonly List<int> _visiblePackageIndices = new List<int>();
		private int _selectedPackageIndex = -1;
		private bool _packageOutputSyncing;
		private bool _packageYooOutputSyncing;
		private bool _initialized;
		private const string DragAssetPrefix = "asset://";
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
		private static readonly string[] ExcludedDirectoryMarkers =
		{
			"/.godot/",
			"/.mono/",
			"/.vs/",
			"/.git/",
			"/.github/",
			"/.vscode/",
			"/.idea/",
			"/fairyguiproject/",
			"/bin/",
			"/obj/"
		};
		private static readonly string[] AddonsAllowedDirectoryMarkers =
		{
			"/runtime/",
			"/resources/"
		};

		public override void _Ready()
		{
			if (_initialized)
			{
				return;
			}

			_initialized = true;
			BuildUi();
			LoadPackageMappings();
			RefreshGlobalAssets();
			RefreshPackageListView();
			CloseRequested += OnCloseRequested;
		}

		public override void _ExitTree()
		{
			CloseRequested -= OnCloseRequested;
		}

		private void BuildUi()
		{
			Title = "GameFrameX Asset Builder (PCK + YooAsset)";
			MinSize = new Vector2I(1100, 760);
			Size = new Vector2I(1366, 860);

			var root = new VBoxContainer
			{
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
				SizeFlagsVertical = Control.SizeFlags.ExpandFill
			};
			root.AnchorLeft = 0f;
			root.AnchorTop = 0f;
			root.AnchorRight = 1f;
			root.AnchorBottom = 1f;
			AddChild(root);

			var toolbar = new HBoxContainer
			{
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
			};
			toolbar.AddThemeConstantOverride("separation", 8);
			root.AddChild(toolbar);

			var newPackageButton = new Button { Text = "新建包" };
			newPackageButton.Pressed += OnCreatePackagePressed;
			toolbar.AddChild(newPackageButton);

			var deletePackageButton = new Button { Text = "删除包" };
			deletePackageButton.Pressed += OnDeletePackagePressed;
			toolbar.AddChild(deletePackageButton);

			var saveButton = new Button { Text = "保存配置" };
			saveButton.Pressed += OnSaveMappingsPressed;
			toolbar.AddChild(saveButton);

			var buildCurrentButton = new Button { Text = "打包当前包" };
			buildCurrentButton.Pressed += OnBuildCurrentPackagePressed;
			toolbar.AddChild(buildCurrentButton);

			var buildAllButton = new Button { Text = "打包全部" };
			buildAllButton.Pressed += OnBuildAllPackagesPressed;
			toolbar.AddChild(buildAllButton);

			var refreshGlobalButton = new Button { Text = "刷新全局资源" };
			refreshGlobalButton.Pressed += OnRefreshGlobalAssetsPressed;
			toolbar.AddChild(refreshGlobalButton);

			var backendLabel = new Label
			{
				Text = "后端:",
				VerticalAlignment = VerticalAlignment.Center
			};
			toolbar.AddChild(backendLabel);

			_buildBackendOptions = new OptionButton();
			_buildBackendOptions.AddItem(BuildBackendBoth);
			_buildBackendOptions.AddItem(BuildBackendPck);
			_buildBackendOptions.AddItem(BuildBackendYooAsset);
			_buildBackendOptions.ItemSelected += OnBuildBackendSelected;
			toolbar.AddChild(_buildBackendOptions);

			_resultLabel = new Label
			{
				Text = "Build Result: NOT RUN",
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
				HorizontalAlignment = HorizontalAlignment.Right,
				VerticalAlignment = VerticalAlignment.Center
			};
			toolbar.AddChild(_resultLabel);

			var mainRow = new HBoxContainer
			{
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
				SizeFlagsVertical = Control.SizeFlags.ExpandFill
			};
			mainRow.AddThemeConstantOverride("separation", 10);
			root.AddChild(mainRow);

			var leftPanel = BuildSectionPanel("包管理", out var leftBox);
			leftPanel.CustomMinimumSize = new Vector2(180f, 0f);
			leftPanel.SizeFlagsStretchRatio = 2f;
			mainRow.AddChild(leftPanel);

			_packageSearchEdit = new LineEdit { PlaceholderText = "搜索包名..." };
			_packageSearchEdit.TextChanged += OnPackageSearchChanged;
			leftBox.AddChild(_packageSearchEdit);

			_packageList = new ItemList
			{
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
				SizeFlagsVertical = Control.SizeFlags.ExpandFill,
				SelectMode = ItemList.SelectModeEnum.Single
			};
			_packageList.ItemSelected += OnPackageSelected;
			leftBox.AddChild(_packageList);

			var packageButtons = new HBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
			packageButtons.AddThemeConstantOverride("separation", 8);
			leftBox.AddChild(packageButtons);

			var addPackageButton = new Button { Text = "+ 包" };
			addPackageButton.Pressed += OnCreatePackagePressed;
			packageButtons.AddChild(addPackageButton);

			var removePackageButton = new Button { Text = "- 包" };
			removePackageButton.Pressed += OnDeletePackagePressed;
			packageButtons.AddChild(removePackageButton);

			var centerPanel = BuildSectionPanel("当前包资源", out var centerBox);
			centerPanel.CustomMinimumSize = new Vector2(240f, 0f);
			centerPanel.SizeFlagsStretchRatio = 4f;
			mainRow.AddChild(centerPanel);

			_currentPackageLabel = new Label
			{
				Text = "当前包: (未选择)",
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
			};
			_currentPackageLabel.ClipText = true;
			_currentPackageLabel.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
			centerBox.AddChild(_currentPackageLabel);

			_packageOutputEdit = new LineEdit
			{
				PlaceholderText = "当前包 PCK 输出路径（user:// 或 absolute）",
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
			};
			_packageOutputEdit.TextChanged += OnPackageOutputChanged;
			centerBox.AddChild(_packageOutputEdit);

			_packageYooOutputEdit = new LineEdit
			{
				PlaceholderText = "当前包 Yoo 输出目录（user:// 或 absolute）",
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
			};
			_packageYooOutputEdit.TextChanged += OnPackageYooOutputChanged;
			centerBox.AddChild(_packageYooOutputEdit);

			var centerButtons = new HBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
			centerButtons.AddThemeConstantOverride("separation", 8);
			centerBox.AddChild(centerButtons);

			var addFromGlobalButton = new Button { Text = "添加右侧选中资源" };
			addFromGlobalButton.Pressed += OnAddSelectedGlobalAssetPressed;
			centerButtons.AddChild(addFromGlobalButton);

			var removeFromPackageButton = new Button { Text = "删除包内选中资源" };
			removeFromPackageButton.Pressed += OnRemovePackageAssetPressed;
			centerButtons.AddChild(removeFromPackageButton);

			_packageAssetTree = CreatePackageAssetTree();
			_packageAssetTree.AssetDropped += OnAssetDroppedToPackage;
			centerBox.AddChild(_packageAssetTree);

			var centerHint = new Label
			{
				Text = "支持从右侧双击或拖拽添加到当前包。",
				AutowrapMode = TextServer.AutowrapMode.WordSmart,
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
			};
			centerBox.AddChild(centerHint);

			var rightPanel = BuildSectionPanel("全局资源", out var rightBox);
			rightPanel.CustomMinimumSize = new Vector2(180f, 0f);
			rightPanel.SizeFlagsStretchRatio = 4f;
			mainRow.AddChild(rightPanel);

			var globalFilterRow = new HBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
			globalFilterRow.AddThemeConstantOverride("separation", 8);
			rightBox.AddChild(globalFilterRow);

			_globalSearchEdit = new LineEdit
			{
				PlaceholderText = "搜索名称/路径",
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
			};
			_globalSearchEdit.TextChanged += OnGlobalFilterChanged;
			globalFilterRow.AddChild(_globalSearchEdit);

			_globalTypeFilter = new OptionButton();
			_globalTypeFilter.AddItem("全部类型");
			_globalTypeFilter.ItemSelected += OnGlobalTypeFilterSelected;
			globalFilterRow.AddChild(_globalTypeFilter);

			_globalPathFilterEdit = new LineEdit
			{
				PlaceholderText = "路径筛选",
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
			};
			_globalPathFilterEdit.TextChanged += OnGlobalFilterChanged;
			globalFilterRow.AddChild(_globalPathFilterEdit);

			_packableAssetCountLabel = new Label
			{
				Text = "Count: 0",
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
			};
			rightBox.AddChild(_packableAssetCountLabel);

			_globalAssetTree = CreateGlobalAssetTree();
			_globalAssetTree.ItemActivated += OnGlobalAssetTreeItemActivated;
			rightBox.AddChild(_globalAssetTree);

			var logPanel = new PanelContainer
			{
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
				SizeFlagsVertical = Control.SizeFlags.Fill,
				CustomMinimumSize = new Vector2(0f, 180f)
			};
			root.AddChild(logPanel);

			_logText = new RichTextLabel
			{
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
				SizeFlagsVertical = Control.SizeFlags.ExpandFill,
				SelectionEnabled = true,
				ScrollFollowing = true
			};
			logPanel.AddChild(_logText);

			_confirmDialog = new ConfirmationDialog();
			_confirmDialog.Confirmed += OnConfirmDialogConfirmed;
			AddChild(_confirmDialog);
		}

		private static PanelContainer BuildSectionPanel(string title, out VBoxContainer contentBox)
		{
			var panel = new PanelContainer
			{
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
				SizeFlagsVertical = Control.SizeFlags.ExpandFill
			};

			var margin = new MarginContainer
			{
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
				SizeFlagsVertical = Control.SizeFlags.ExpandFill
			};
			margin.AddThemeConstantOverride("margin_top", 10);
			margin.AddThemeConstantOverride("margin_left", 10);
			margin.AddThemeConstantOverride("margin_right", 10);
			margin.AddThemeConstantOverride("margin_bottom", 10);
			panel.AddChild(margin);

			contentBox = new VBoxContainer
			{
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
				SizeFlagsVertical = Control.SizeFlags.ExpandFill
			};
			contentBox.AddThemeConstantOverride("separation", 8);
			margin.AddChild(contentBox);

			var label = new Label
			{
				Text = title,
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
			};
			contentBox.AddChild(label);
			return panel;
		}

		private static GlobalAssetTree CreateGlobalAssetTree()
		{
			var tree = new GlobalAssetTree
			{
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
				SizeFlagsVertical = Control.SizeFlags.ExpandFill,
				HideRoot = true,
				Columns = 3,
				ColumnTitlesVisible = true
			};
			tree.SetColumnTitle(0, "名称");
			tree.SetColumnTitle(1, "类型");
			tree.SetColumnTitle(2, "路径");
			//tree.SetColumnExpand(0, false);
			//tree.SetColumnCustomMinimumWidth(0, 100);
			tree.SetColumnExpandRatio(0, 3);
			tree.SetColumnClipContent(0, true);
			tree.SetColumnExpand(1, false);
			tree.SetColumnCustomMinimumWidth(1, 64);
			tree.SetColumnClipContent(1, true);
			//tree.SetColumnExpandRatio(1, 1);
			tree.SetColumnExpand(2, true);
			tree.SetColumnExpandRatio(2, 6);
			return tree;
		}

		private static PackageAssetTree CreatePackageAssetTree()
		{
			var tree = new PackageAssetTree
			{
				SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
				SizeFlagsVertical = Control.SizeFlags.ExpandFill,
				HideRoot = true,
				Columns = 3,
				ColumnTitlesVisible = true
			};
			tree.SetColumnTitle(0, "名称");
			tree.SetColumnTitle(1, "类型");
			tree.SetColumnTitle(2, "路径");
			tree.SetColumnExpand(0, true);
			tree.SetColumnExpandRatio(0, 5);
			tree.SetColumnCustomMinimumWidth(0, 160);
			tree.SetColumnClipContent(0, true);
			tree.SetColumnExpand(1, true);
			tree.SetColumnExpandRatio(1, 1);
			tree.SetColumnCustomMinimumWidth(1, 64);
			tree.SetColumnClipContent(1, true);
			tree.SetColumnExpand(2, true);
			tree.SetColumnExpandRatio(2, 4);
			tree.SetColumnCustomMinimumWidth(2, 180);
			tree.SetColumnClipContent(2, true);
			return tree;
		}

		private static bool TryParseDragAssetPath(Variant data, out string sourcePath)
		{
			sourcePath = string.Empty;
			if (data.VariantType != Variant.Type.String)
			{
				return false;
			}

			var text = data.AsString();
			if (string.IsNullOrWhiteSpace(text) || !text.StartsWith(DragAssetPrefix, StringComparison.Ordinal))
			{
				return false;
			}

			sourcePath = text[DragAssetPrefix.Length..];
			return !string.IsNullOrWhiteSpace(sourcePath);
		}

		private void LoadPackageMappings()
		{
			_packageMappings.Clear();
			var buildBackend = BuildBackendBoth;
			var configPath = ResolveFilePath(DefaultBatchConfigSavePath);
			if (File.Exists(configPath))
			{
				try
				{
					var content = File.ReadAllText(configPath);
					var config = JsonSerializer.Deserialize<BatchBuildConfig>(content, new JsonSerializerOptions
					{
						PropertyNameCaseInsensitive = true
					});
					if (config?.Packages != null)
					{
						if (!string.IsNullOrWhiteSpace(config.BuildBackend))
						{
							buildBackend = config.BuildBackend.Trim();
						}

						for (var i = 0; i < config.Packages.Count; i++)
						{
							var package = config.Packages[i];
							var normalized = NormalizePackage(package, i + 1);
							_packageMappings.Add(normalized);
						}
					}
				}
				catch (Exception exception)
				{
					AppendLog($"[Config] load failed: {exception.Message}");
				}
			}

			if (_packageMappings.Count == 0)
			{
				_packageMappings.Add(CreateDefaultPackage("runtime_verify"));
			}

			SetBuildBackendSelection(buildBackend);
		}

		private static BatchBuildPackage NormalizePackage(BatchBuildPackage package, int sequence)
		{
			var packageName = string.IsNullOrWhiteSpace(package?.PackageName) ? $"package_{sequence}" : package.PackageName.Trim();
			var normalized = new BatchBuildPackage
			{
				PackageName = packageName,
				OutputPck = string.IsNullOrWhiteSpace(package?.OutputPck) ? BuildDefaultOutputPath(packageName) : package.OutputPck.Trim(),
				OutputYoo = string.IsNullOrWhiteSpace(package?.OutputYoo) ? BuildDefaultYooOutputPath(packageName) : package.OutputYoo.Trim()
			};
			if (package?.Files == null)
			{
				return normalized;
			}

			for (var i = 0; i < package.Files.Count; i++)
			{
				var file = package.Files[i];
				if (string.IsNullOrWhiteSpace(file?.SourcePath))
				{
					continue;
				}

				normalized.Files.Add(new BatchBuildFile
				{
					SourcePath = file.SourcePath.Trim(),
					MountPath = string.IsNullOrWhiteSpace(file.MountPath) ? NormalizeMountPath(file.SourcePath) : NormalizeMountPath(file.MountPath)
				});
			}

			return normalized;
		}

		private static BatchBuildPackage CreateDefaultPackage(string packageName)
		{
			return new BatchBuildPackage
			{
				PackageName = packageName,
				OutputPck = BuildDefaultOutputPath(packageName),
				OutputYoo = BuildDefaultYooOutputPath(packageName),
				Files = new List<BatchBuildFile>()
			};
		}

		private static string BuildDefaultOutputPath(string packageName)
		{
			return $"user://assetsystem_builder/packages/{MakeSafeFileName(packageName)}.pck";
		}

		private static string BuildDefaultYooOutputPath(string packageName)
		{
			return $"user://assetsystem_builder/yoo/{MakeSafeFileName(packageName)}";
		}

		private void RefreshGlobalAssets()
		{
			_globalAssets.Clear();
			var projectRoot = ProjectSettings.GlobalizePath("res://").Replace('\\', '/').TrimEnd('/');
			if (!Directory.Exists(projectRoot))
			{
				RefreshGlobalAssetTypeFilter();
				RefreshGlobalAssetTree();
				return;
			}

			var files = Directory.GetFiles(projectRoot, "*", SearchOption.AllDirectories);
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
				var resourcePath = "res://" + relativePath;
				_globalAssets.Add(new GlobalAssetItem
				{
					Name = Path.GetFileName(resourcePath),
					AssetType = GuessAssetType(resourcePath),
					ResourcePath = resourcePath
				});
			}

			_globalAssets.Sort(static (a, b) => string.Compare(a.ResourcePath, b.ResourcePath, StringComparison.OrdinalIgnoreCase));
			RefreshGlobalAssetTypeFilter();
			RefreshGlobalAssetTree();
		}

		private void RefreshGlobalAssetTypeFilter()
		{
			if (_globalTypeFilter == null)
			{
				return;
			}

			var selectedType = _globalTypeFilter.ItemCount > 0 && _globalTypeFilter.Selected >= 0 && _globalTypeFilter.Selected < _globalTypeFilter.ItemCount
				? _globalTypeFilter.GetItemText(_globalTypeFilter.Selected)
				: "全部类型";
			_globalTypeFilter.Clear();
			_globalTypeFilter.AddItem("全部类型");
			var types = _globalAssets.Select(static x => x.AssetType).Where(static x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).OrderBy(static x => x, StringComparer.OrdinalIgnoreCase).ToList();
			for (var i = 0; i < types.Count; i++)
			{
				_globalTypeFilter.AddItem(types[i]);
			}

			var selectedIndex = 0;
			for (var i = 0; i < _globalTypeFilter.ItemCount; i++)
			{
				if (string.Equals(_globalTypeFilter.GetItemText(i), selectedType, StringComparison.OrdinalIgnoreCase))
				{
					selectedIndex = i;
					break;
				}
			}

			_globalTypeFilter.Select(selectedIndex);
		}

		private void RefreshGlobalAssetTree()
		{
			if (_globalAssetTree == null)
			{
				return;
			}

			_globalAssetTree.Clear();
			var root = _globalAssetTree.CreateItem();
			var search = _globalSearchEdit?.Text?.Trim() ?? string.Empty;
			var pathFilter = _globalPathFilterEdit?.Text?.Trim() ?? string.Empty;
			var selectedType = _globalTypeFilter != null && _globalTypeFilter.ItemCount > 0 && _globalTypeFilter.Selected >= 0 && _globalTypeFilter.Selected < _globalTypeFilter.ItemCount
				? _globalTypeFilter.GetItemText(_globalTypeFilter.Selected)
				: "全部类型";
			var count = 0;
			for (var i = 0; i < _globalAssets.Count; i++)
			{
				var asset = _globalAssets[i];
				if (!string.IsNullOrEmpty(search) &&
					!asset.Name.Contains(search, StringComparison.OrdinalIgnoreCase) &&
					!asset.ResourcePath.Contains(search, StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				if (!string.IsNullOrEmpty(pathFilter) && !asset.ResourcePath.Contains(pathFilter, StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				if (!string.Equals(selectedType, "全部类型", StringComparison.OrdinalIgnoreCase) &&
					!string.Equals(asset.AssetType, selectedType, StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				var item = _globalAssetTree.CreateItem(root);
				item.SetText(0, asset.Name);
				item.SetText(1, asset.AssetType);
				item.SetText(2, asset.ResourcePath);
				item.SetMetadata(0, asset.ResourcePath);
				count++;
			}

			if (_packableAssetCountLabel != null)
			{
				_packableAssetCountLabel.Text = $"Count: {count}";
			}
		}

		private void RefreshPackageListView()
		{
			if (_packageList == null)
			{
				return;
			}

			var previousSelected = _selectedPackageIndex;
			var search = _packageSearchEdit?.Text?.Trim() ?? string.Empty;
			_visiblePackageIndices.Clear();
			_packageList.Clear();
			for (var i = 0; i < _packageMappings.Count; i++)
			{
				var packageName = _packageMappings[i].PackageName;
				if (!string.IsNullOrEmpty(search) && !packageName.Contains(search, StringComparison.OrdinalIgnoreCase))
				{
					continue;
				}

				_visiblePackageIndices.Add(i);
				_packageList.AddItem(packageName);
			}

			if (_visiblePackageIndices.Count == 0)
			{
				_selectedPackageIndex = -1;
				RefreshCurrentPackageAssetsView();
				return;
			}

			var selectedVisibleIndex = _visiblePackageIndices.IndexOf(previousSelected);
			if (selectedVisibleIndex < 0)
			{
				selectedVisibleIndex = 0;
			}

			_packageList.Select(selectedVisibleIndex);
			_selectedPackageIndex = _visiblePackageIndices[selectedVisibleIndex];
			RefreshCurrentPackageAssetsView();
		}

		private void RefreshCurrentPackageAssetsView()
		{
			if (_packageAssetTree == null)
			{
				return;
			}

			_packageAssetTree.Clear();
			var root = _packageAssetTree.CreateItem();
			if (_selectedPackageIndex < 0 || _selectedPackageIndex >= _packageMappings.Count)
			{
				if (_currentPackageLabel != null)
				{
					_currentPackageLabel.Text = "当前包: (未选择)";
				}

				if (_packageOutputEdit != null)
				{
					_packageOutputSyncing = true;
					_packageOutputEdit.Text = string.Empty;
					_packageOutputEdit.Editable = false;
					_packageOutputSyncing = false;
				}

				if (_packageYooOutputEdit != null)
				{
					_packageYooOutputSyncing = true;
					_packageYooOutputEdit.Text = string.Empty;
					_packageYooOutputEdit.Editable = false;
					_packageYooOutputSyncing = false;
				}

				return;
			}

			var package = _packageMappings[_selectedPackageIndex];
			UpdateCurrentPackageSummaryLabel(package);

			if (_packageOutputEdit != null)
			{
				_packageOutputSyncing = true;
				_packageOutputEdit.Text = package.OutputPck ?? string.Empty;
				_packageOutputEdit.Editable = true;
				_packageOutputSyncing = false;
			}

			if (_packageYooOutputEdit != null)
			{
				_packageYooOutputSyncing = true;
				_packageYooOutputEdit.Text = package.OutputYoo ?? string.Empty;
				_packageYooOutputEdit.Editable = true;
				_packageYooOutputSyncing = false;
			}

			for (var i = 0; i < package.Files.Count; i++)
			{
				var file = package.Files[i];
				var sourcePath = file.SourcePath?.Trim() ?? string.Empty;
				if (string.IsNullOrEmpty(sourcePath))
				{
					continue;
				}

				var item = _packageAssetTree.CreateItem(root);
				item.SetText(0, Path.GetFileName(sourcePath));
				item.SetText(1, GuessAssetType(sourcePath));
				item.SetText(2, sourcePath);
				item.SetMetadata(0, sourcePath);
			}
		}

		private void UpdateCurrentPackageSummaryLabel(BatchBuildPackage package)
		{
			if (_currentPackageLabel == null)
			{
				return;
			}

			if (package == null)
			{
				_currentPackageLabel.Text = "当前包: (未选择)";
				return;
			}

			var backend = GetSelectedBuildBackendText();
			_currentPackageLabel.Text = $"当前包: {package.PackageName}  |  后端: {backend}  |  PCK: {package.OutputPck}  |  Yoo: {package.OutputYoo}";
		}

		private void OnCreatePackagePressed()
		{
			var packageName = GenerateUniquePackageName("new_package");
			_packageMappings.Add(CreateDefaultPackage(packageName));
			_selectedPackageIndex = _packageMappings.Count - 1;
			RefreshPackageListView();
			SetBuildResult(true, $"package created: {packageName}");
		}

		private void OnDeletePackagePressed()
		{
			if (!TryGetCurrentPackage(out var package, out var packageIndex))
			{
				SetBuildResult(false, "请先选择一个包。");
				return;
			}

			RequestConfirm($"确认删除包 [{package.PackageName}] 吗？", () =>
			{
				_packageMappings.RemoveAt(packageIndex);
				_selectedPackageIndex = -1;
				RefreshPackageListView();
				SetBuildResult(true, $"package deleted: {package.PackageName}");
			});
		}

		private void OnSaveMappingsPressed()
		{
			var config = new BatchBuildConfig
			{
				BuildBackend = GetSelectedBuildBackendText()
			};
			for (var i = 0; i < _packageMappings.Count; i++)
			{
				var package = _packageMappings[i];
				var cloned = new BatchBuildPackage
				{
					PackageName = package.PackageName,
					OutputPck = package.OutputPck,
					OutputYoo = package.OutputYoo,
					Files = new List<BatchBuildFile>(package.Files.Count)
				};
				for (var j = 0; j < package.Files.Count; j++)
				{
					cloned.Files.Add(new BatchBuildFile
					{
						SourcePath = package.Files[j].SourcePath,
						MountPath = package.Files[j].MountPath
					});
				}

				config.Packages.Add(cloned);
			}

			if (TrySaveBatchConfig(config, out var summary, out var error))
			{
				SetBuildResult(true, $"配置已保存: {summary}");
			}
			else
			{
				SetBuildResult(false, error);
			}
		}

		private void OnBuildCurrentPackagePressed()
		{
			if (!TryGetCurrentPackage(out var package, out _))
			{
				SetBuildResult(false, "请先选择一个包。");
				return;
			}

			var backendMode = GetSelectedBuildBackendMode();
			if (TryBuildMappedPackage(package, backendMode, out var summary, out var error))
			{
				SetBuildResult(true, $"[{package.PackageName}] {summary}");
				return;
			}

			SetBuildResult(false, $"[{package.PackageName}] {error}");
		}

		private void OnBuildAllPackagesPressed()
		{
			if (_packageMappings.Count == 0)
			{
				SetBuildResult(false, "没有可打包的包。");
				return;
			}

			var success = 0;
			var fail = 0;
			for (var i = 0; i < _packageMappings.Count; i++)
			{
				var package = _packageMappings[i];
				if (TryBuildMappedPackage(package, GetSelectedBuildBackendMode(), out var summary, out var error))
				{
					success++;
					AppendLog($"[BuildAll][{package.PackageName}] PASS {summary}");
				}
				else
				{
					fail++;
					AppendLog($"[BuildAll][{package.PackageName}] FAIL {error}");
				}
			}

			SetBuildResult(fail == 0, $"build all complete success={success} fail={fail}");
		}

		private bool TryBuildMappedPackage(BatchBuildPackage package, BuildBackendMode backendMode, out string summary, out string error)
		{
			summary = string.Empty;
			error = string.Empty;
			if (package == null)
			{
				error = "package is null.";
				return false;
			}

			if (package.Files == null || package.Files.Count == 0)
			{
				error = "files is empty.";
				return false;
			}

			var buildFiles = new List<BuildFileItem>(package.Files.Count);
			for (var i = 0; i < package.Files.Count; i++)
			{
				var file = package.Files[i];
				var sourcePath = ResolveFilePath(file.SourcePath);
				if (File.Exists(sourcePath) == false)
				{
					error = $"source missing: {sourcePath}";
					return false;
				}

				var mountPath = NormalizeMountPath(string.IsNullOrWhiteSpace(file.MountPath) ? file.SourcePath : file.MountPath);
				if (string.IsNullOrWhiteSpace(mountPath))
				{
					error = $"mount path is empty: {file.SourcePath}";
					return false;
				}

				buildFiles.Add(new BuildFileItem(sourcePath, mountPath));
			}

			var resultParts = new List<string>(2);
			if (backendMode == BuildBackendMode.Pck || backendMode == BuildBackendMode.Both)
			{
				var output = string.IsNullOrWhiteSpace(package.OutputPck) ? BuildDefaultOutputPath(package.PackageName) : package.OutputPck;
				var outputPath = ResolveFilePath(output);
				if (TryBuildPackage(package.PackageName, outputPath, buildFiles, out var outputLength, out error) == false)
				{
					return false;
				}

				resultParts.Add($"PCK bytes={outputLength}");
			}

			if (backendMode == BuildBackendMode.YooAsset || backendMode == BuildBackendMode.Both)
			{
				if (TryBuildYooPackage(package, buildFiles, out var yooResult, out error) == false)
				{
					return false;
				}

				resultParts.Add($"Yoo files={yooResult.FileCount} version={yooResult.PackageVersion}");
			}

			summary = string.Join(" | ", resultParts);
			return true;
		}

		private void OnRefreshGlobalAssetsPressed()
		{
			RefreshGlobalAssets();
			SetBuildResult(true, $"global assets refreshed: {_globalAssets.Count}");
		}

		private void OnGlobalAssetTreeItemActivated()
		{
			OnAddSelectedGlobalAssetPressed();
		}

		private void OnAssetDroppedToPackage(string sourcePath)
		{
			if (TryAddAssetToCurrentPackage(sourcePath))
			{
				SetBuildResult(true, $"drag added: {sourcePath}");
			}
		}

		private void OnAddSelectedGlobalAssetPressed()
		{
			if (_globalAssetTree == null)
			{
				SetBuildResult(false, "全局资源列表未就绪。");
				return;
			}

			var selectedItem = _globalAssetTree.GetSelected();
			if (selectedItem == null)
			{
				SetBuildResult(false, "请先在右侧选择一个资源。");
				return;
			}

			var sourcePath = selectedItem.GetMetadata(0).ToString();
			if (TryAddAssetToCurrentPackage(sourcePath))
			{
				SetBuildResult(true, $"asset added: {sourcePath}");
			}
		}

		private bool TryAddAssetToCurrentPackage(string sourcePath, bool withResult = true)
		{
			if (!TryGetCurrentPackage(out var package, out _))
			{
				if (withResult)
				{
					SetBuildResult(false, "请先选择一个包。");
				}

				return false;
			}

			if (string.IsNullOrWhiteSpace(sourcePath))
			{
				if (withResult)
				{
					SetBuildResult(false, "选中资源路径无效。");
				}

				return false;
			}

			var exists = package.Files.Any(file => string.Equals(file.SourcePath, sourcePath, StringComparison.OrdinalIgnoreCase));
			if (exists)
			{
				if (withResult)
				{
					SetBuildResult(false, "该资源已在当前包中。");
				}

				return false;
			}

			package.Files.Add(new BatchBuildFile
			{
				SourcePath = sourcePath,
				MountPath = NormalizeMountPath(sourcePath)
			});
			RefreshCurrentPackageAssetsView();
			return true;
		}

		private void OnRemovePackageAssetPressed()
		{
			if (!TryGetCurrentPackage(out var package, out _))
			{
				SetBuildResult(false, "请先选择一个包。");
				return;
			}

			if (_packageAssetTree == null)
			{
				SetBuildResult(false, "包内资源列表未就绪。");
				return;
			}

			var selectedItem = _packageAssetTree.GetSelected();
			if (selectedItem == null)
			{
				SetBuildResult(false, "请先选择一个包内资源。");
				return;
			}

			var sourcePath = selectedItem.GetMetadata(0).ToString();
			if (string.IsNullOrWhiteSpace(sourcePath))
			{
				SetBuildResult(false, "选中资源路径无效。");
				return;
			}

			RequestConfirm($"确认从包中删除资源？\n{sourcePath}", () =>
			{
				var removed = package.Files.RemoveAll(file => string.Equals(file.SourcePath, sourcePath, StringComparison.OrdinalIgnoreCase));
				RefreshCurrentPackageAssetsView();
				SetBuildResult(removed > 0, removed > 0 ? $"asset removed: {sourcePath}" : "未删除任何资源。");
			});
		}

		private void OnPackageSearchChanged(string value)
		{
			RefreshPackageListView();
		}

		private void OnGlobalFilterChanged(string value)
		{
			RefreshGlobalAssetTree();
		}

		private void OnGlobalTypeFilterSelected(long index)
		{
			RefreshGlobalAssetTree();
		}

		private void OnBuildBackendSelected(long index)
		{
			if (TryGetCurrentPackage(out var package, out _))
			{
				UpdateCurrentPackageSummaryLabel(package);
			}
			else
			{
				UpdateCurrentPackageSummaryLabel(null);
			}
		}

		private string GetSelectedBuildBackendText()
		{
			if (_buildBackendOptions == null || _buildBackendOptions.ItemCount <= 0)
			{
				return BuildBackendBoth;
			}

			var selectedIndex = _buildBackendOptions.Selected;
			if (selectedIndex < 0 || selectedIndex >= _buildBackendOptions.ItemCount)
			{
				selectedIndex = 0;
			}

			var selectedText = _buildBackendOptions.GetItemText(selectedIndex);
			if (string.IsNullOrWhiteSpace(selectedText))
			{
				return BuildBackendBoth;
			}

			return selectedText.Trim();
		}

		private BuildBackendMode GetSelectedBuildBackendMode()
		{
			var selectedText = GetSelectedBuildBackendText();
			if (string.Equals(selectedText, BuildBackendPck, StringComparison.OrdinalIgnoreCase))
			{
				return BuildBackendMode.Pck;
			}

			if (string.Equals(selectedText, BuildBackendYooAsset, StringComparison.OrdinalIgnoreCase))
			{
				return BuildBackendMode.YooAsset;
			}

			return BuildBackendMode.Both;
		}

		private void SetBuildBackendSelection(string buildBackend)
		{
			if (_buildBackendOptions == null || _buildBackendOptions.ItemCount <= 0)
			{
				return;
			}

			var selectedIndex = 0;
			for (var i = 0; i < _buildBackendOptions.ItemCount; i++)
			{
				if (string.Equals(_buildBackendOptions.GetItemText(i), buildBackend, StringComparison.OrdinalIgnoreCase))
				{
					selectedIndex = i;
					break;
				}
			}

			_buildBackendOptions.Select(selectedIndex);
		}

		private void OnPackageOutputChanged(string value)
		{
			if (_packageOutputSyncing)
			{
				return;
			}

			if (!TryGetCurrentPackage(out var package, out _))
			{
				return;
			}

			if (string.IsNullOrWhiteSpace(value))
			{
				package.OutputPck = BuildDefaultOutputPath(package.PackageName);
				if (_packageOutputEdit != null)
				{
					_packageOutputSyncing = true;
					_packageOutputEdit.Text = package.OutputPck;
					_packageOutputSyncing = false;
				}
			}
			else
			{
				package.OutputPck = value.Trim();
			}

			UpdateCurrentPackageSummaryLabel(package);
		}

		private void OnPackageYooOutputChanged(string value)
		{
			if (_packageYooOutputSyncing)
			{
				return;
			}

			if (!TryGetCurrentPackage(out var package, out _))
			{
				return;
			}

			if (string.IsNullOrWhiteSpace(value))
			{
				package.OutputYoo = BuildDefaultYooOutputPath(package.PackageName);
				if (_packageYooOutputEdit != null)
				{
					_packageYooOutputSyncing = true;
					_packageYooOutputEdit.Text = package.OutputYoo;
					_packageYooOutputSyncing = false;
				}
			}
			else
			{
				package.OutputYoo = value.Trim();
			}

			UpdateCurrentPackageSummaryLabel(package);
		}

		private void OnPackageSelected(long visibleIndex)
		{
			if (visibleIndex < 0 || visibleIndex >= _visiblePackageIndices.Count)
			{
				return;
			}

			_selectedPackageIndex = _visiblePackageIndices[(int)visibleIndex];
			RefreshCurrentPackageAssetsView();
		}

		private void RequestConfirm(string message, Action action)
		{
			if (_confirmDialog == null)
			{
				action?.Invoke();
				return;
			}

			_pendingConfirmAction = action;
			_confirmDialog.DialogText = message;
			_confirmDialog.PopupCentered(new Vector2I(480, 160));
		}

		private void OnConfirmDialogConfirmed()
		{
			var action = _pendingConfirmAction;
			_pendingConfirmAction = null;
			action?.Invoke();
		}

		private bool TryGetCurrentPackage(out BatchBuildPackage package, out int packageIndex)
		{
			package = null;
			packageIndex = _selectedPackageIndex;
			if (packageIndex < 0 || packageIndex >= _packageMappings.Count)
			{
				return false;
			}

			package = _packageMappings[packageIndex];
			return true;
		}

		private string GenerateUniquePackageName(string baseName)
		{
			var candidate = string.IsNullOrWhiteSpace(baseName) ? "new_package" : baseName.Trim();
			var index = 1;
			while (_packageMappings.Any(pkg => string.Equals(pkg.PackageName, candidate, StringComparison.OrdinalIgnoreCase)))
			{
				candidate = $"{baseName}_{index}";
				index++;
			}

			return candidate;
		}

		private static string GuessAssetType(string resourcePath)
		{
			var extension = Path.GetExtension(resourcePath)?.ToLowerInvariant() ?? string.Empty;
			return extension switch
			{
				".png" or ".jpg" or ".jpeg" or ".webp" or ".bmp" or ".svg" or ".tga" or ".exr" or ".hdr" or ".ktx" or ".ktx2" => "Texture",
				".tscn" or ".scn" => "Scene",
				".tres" or ".res" => "Resource",
				".wav" or ".ogg" or ".mp3" => "Audio",
				".glb" or ".gltf" or ".obj" => "Model",
				".ttf" or ".otf" or ".fnt" => "Font",
				".json" or ".txt" or ".bytes" or ".cfg" or ".csv" => "Data",
				".shader" or ".gdshader" or ".material" => "Shader/Material",
				_ => "Other"
			};
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

			var normalizedPath = fullPath.Replace('\\', '/');
			for (var i = 0; i < ExcludedDirectoryMarkers.Length; i++)
			{
				if (normalizedPath.Contains(ExcludedDirectoryMarkers[i], StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}
			}

			if (normalizedPath.Contains("/addons/", StringComparison.OrdinalIgnoreCase))
			{
				var fileName = Path.GetFileName(normalizedPath);
				if (fileName.Equals("plugin.cfg", StringComparison.OrdinalIgnoreCase) ||
					fileName.Equals("package.json", StringComparison.OrdinalIgnoreCase) ||
					fileName.StartsWith("UPSTREAM_LICENSE", StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}

				var inAllowedAddonsDirectory = false;
				for (var i = 0; i < AddonsAllowedDirectoryMarkers.Length; i++)
				{
					if (normalizedPath.Contains(AddonsAllowedDirectoryMarkers[i], StringComparison.OrdinalIgnoreCase))
					{
						inAllowedAddonsDirectory = true;
						break;
					}
				}

				if (inAllowedAddonsDirectory == false)
				{
					return false;
				}
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
			var buildBackendText = GetSelectedBuildBackendText();

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
			SetBuildBackendSelection(buildBackendText);
			RefreshPackableAssets();
			RefreshPackageListView();
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

		private bool TryBuildYooPackage(BatchBuildPackage package, List<BuildFileItem> buildFiles, out YooBuildResult result, out string error)
		{
			result = default;
			error = string.Empty;
			try
			{
				var packageName = string.IsNullOrWhiteSpace(package.PackageName) ? "default_package" : package.PackageName.Trim();
				var outputRootInput = string.IsNullOrWhiteSpace(package.OutputYoo) ? BuildDefaultYooOutputPath(packageName) : package.OutputYoo.Trim();
				var outputRoot = ResolveFilePath(outputRootInput);
				if (string.IsNullOrWhiteSpace(outputRoot))
				{
					error = "yoo output root is empty.";
					return false;
				}

				var packageVersion = DateTime.Now.ToString(YooDefaultVersionFormat);
				var packageRoot = NormalizeFileSystemPath(Path.Combine(outputRoot, packageName, packageVersion));
				Directory.CreateDirectory(packageRoot);

				var bundleEntries = new List<YooBuildBundleEntry>(buildFiles.Count);
				long totalBytes = 0;
				for (var i = 0; i < buildFiles.Count; i++)
				{
					var buildFile = buildFiles[i];
					var sourceBytes = File.ReadAllBytes(buildFile.SourcePath);
					var bundleName = NormalizeBundleName(buildFile.MountPath);
					var fileHash = HashUtility.BytesMD5(sourceBytes);
					var fileCrc = HashUtility.BytesCRC32(sourceBytes);
					var fileExtension = ManifestTools.GetRemoteBundleFileExtension(bundleName);
					var outputFileName = ManifestTools.GetRemoteBundleFileName(1, bundleName, fileExtension, fileHash);
					var destinationPath = NormalizeFileSystemPath(Path.Combine(packageRoot, outputFileName));
					var destinationDir = Path.GetDirectoryName(destinationPath);
					if (!string.IsNullOrWhiteSpace(destinationDir))
					{
						Directory.CreateDirectory(destinationDir);
					}

					File.WriteAllBytes(destinationPath, sourceBytes);
					totalBytes += sourceBytes.LongLength;
					bundleEntries.Add(new YooBuildBundleEntry
					{
						SourceFilePath = buildFile.SourcePath,
						MountPath = buildFile.MountPath,
						BundleName = bundleName,
						OutputFileName = outputFileName,
						DestinationFilePath = destinationPath,
						FileHash = fileHash,
						FileCRC = fileCrc,
						FileSize = sourceBytes.LongLength,
						Tags = InferYooBundleTags(buildFile)
					});
				}

				var runtimeManifest = BuildYooRuntimeManifest(packageName, packageVersion, bundleEntries);
				var runtimeManifestPath = NormalizeFileSystemPath(Path.Combine(packageRoot, YooAssetSettingsData.GetManifestBinaryFileName(packageName, packageVersion)));
				var runtimeVersionPath = NormalizeFileSystemPath(Path.Combine(packageRoot, YooAssetSettingsData.GetPackageVersionFileName(packageName)));
				var runtimeHashPath = NormalizeFileSystemPath(Path.Combine(packageRoot, YooAssetSettingsData.GetPackageHashFileName(packageName, packageVersion)));
				SerializeRuntimeManifestBinary(runtimeManifestPath, runtimeManifest);
				var runtimeManifestHash = HashUtility.FileMD5(runtimeManifestPath);
				File.WriteAllText(runtimeVersionPath, packageVersion, Encoding.UTF8);
				File.WriteAllText(runtimeHashPath, runtimeManifestHash, Encoding.UTF8);

				var manifestPath = NormalizeFileSystemPath(Path.Combine(packageRoot, YooDefaultBuildManifestFileName));
				var versionPath = NormalizeFileSystemPath(Path.Combine(packageRoot, YooDefaultBuildVersionFileName));
				File.WriteAllText(manifestPath, BuildYooBuildManifestText(packageName, packageVersion, runtimeManifestPath, bundleEntries), Encoding.UTF8);
				File.WriteAllText(versionPath, BuildYooBuildVersionText(packageName, packageVersion, bundleEntries), Encoding.UTF8);

				result = new YooBuildResult
				{
					PackageName = packageName,
					PackageVersion = packageVersion,
					OutputRoot = outputRoot,
					PackageRoot = packageRoot,
					FileCount = bundleEntries.Count,
					TotalBytes = totalBytes
				};
				AppendLog($"[YooAsset][{packageName}] PASS version={packageVersion} files={bundleEntries.Count} output={packageRoot}");
				return true;
			}
			catch (Exception exception)
			{
				error = $"yoo build failed: {exception.Message}";
				return false;
			}
		}

		private static PackageManifest BuildYooRuntimeManifest(string packageName, string packageVersion, List<YooBuildBundleEntry> bundleEntries)
		{
			var manifest = new PackageManifest
			{
				FileVersion = YooAssetSettings.ManifestFileVersion,
				EnableAddressable = false,
				LocationToLower = false,
				IncludeAssetGUID = false,
				OutputNameStyle = 1,
				BuildPipeline = YooBuildPipelineName,
				PackageName = packageName,
				PackageVersion = packageVersion,
				AssetList = new List<PackageAsset>(bundleEntries.Count),
				BundleList = new List<PackageBundle>(bundleEntries.Count)
			};

			for (var i = 0; i < bundleEntries.Count; i++)
			{
				var entry = bundleEntries[i];
				manifest.BundleList.Add(new PackageBundle
				{
					BundleName = entry.BundleName,
					UnityCRC = 0,
					FileHash = entry.FileHash,
					FileCRC = entry.FileCRC,
					FileSize = entry.FileSize,
					Encrypted = false,
					Tags = entry.Tags,
					DependIDs = Array.Empty<int>()
				});

				var sourceAssetPath = ToProjectResourcePath(entry.SourceFilePath);
				manifest.AssetList.Add(new PackageAsset
				{
					Address = sourceAssetPath,
					AssetPath = sourceAssetPath,
					AssetGUID = string.Empty,
					AssetTags = Array.Empty<string>(),
					BundleID = i
				});
			}

			return manifest;
		}

		private static string BuildYooBuildManifestText(string packageName, string packageVersion, string runtimeManifestPath, List<YooBuildBundleEntry> bundleEntries)
		{
			var builder = new StringBuilder();
			builder.AppendLine($"PackageName={packageName}");
			builder.AppendLine($"BuildPipeline={YooBuildPipelineName}");
			builder.AppendLine($"BuildVersion={packageVersion}");
			builder.AppendLine($"BuildTime={DateTime.Now:O}");
			builder.AppendLine($"FileCount={bundleEntries.Count}");
			builder.AppendLine($"RuntimeManifest={NormalizeFileSystemPath(runtimeManifestPath)}");
			builder.AppendLine("Files:");
			for (var i = 0; i < bundleEntries.Count; i++)
			{
				var entry = bundleEntries[i];
				builder.AppendLine(entry.OutputFileName);
				builder.AppendLine(ToProjectResourcePath(entry.SourceFilePath));
			}

			return builder.ToString();
		}

		private static string BuildYooBuildVersionText(string packageName, string packageVersion, List<YooBuildBundleEntry> bundleEntries)
		{
			var builder = new StringBuilder();
			builder.AppendLine($"PackageName={packageName}");
			builder.AppendLine($"BuildPipeline={YooBuildPipelineName}");
			builder.AppendLine($"BuildVersion={packageVersion}");
			builder.AppendLine($"FileCount={bundleEntries.Count}");
			builder.AppendLine($"OutputNameStyle=1");
			return builder.ToString();
		}

		private static string[] InferYooBundleTags(BuildFileItem buildFile)
		{
			var tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			var extension = Path.GetExtension(buildFile.SourcePath)?.TrimStart('.');
			if (!string.IsNullOrWhiteSpace(extension))
			{
				tags.Add(extension.ToLowerInvariant());
			}

			var normalizedMountPath = NormalizeMountPath(buildFile.MountPath);
			var segments = normalizedMountPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
			for (var i = 0; i < segments.Length - 1; i++)
			{
				tags.Add(segments[i].ToLowerInvariant());
			}

			return tags.ToArray();
		}

		private static string NormalizeBundleName(string mountPath)
		{
			var normalized = NormalizeMountPath(mountPath);
			if (string.IsNullOrWhiteSpace(normalized))
			{
				return "unnamed.bundle";
			}

			return normalized;
		}

		private static string NormalizeFileSystemPath(string inputPath)
		{
			return string.IsNullOrWhiteSpace(inputPath) ? string.Empty : inputPath.Replace('\\', '/');
		}

		private static string ToProjectResourcePath(string absolutePath)
		{
			var normalizedAbsolute = NormalizeFileSystemPath(absolutePath);
			var projectRoot = NormalizeFileSystemPath(ProjectSettings.GlobalizePath("res://")).TrimEnd('/');
			if (normalizedAbsolute.StartsWith(projectRoot + "/", StringComparison.OrdinalIgnoreCase))
			{
				return "res://" + normalizedAbsolute[(projectRoot.Length + 1)..];
			}

			return normalizedAbsolute;
		}

		private static void SerializeRuntimeManifestBinary(string savePath, PackageManifest manifest)
		{
			using var fileStream = new FileStream(savePath, FileMode.Create, System.IO.FileAccess.Write);
			using var writer = new BinaryWriter(fileStream, Encoding.UTF8, false);
			writer.Write(YooAssetSettings.ManifestFileSign);
			WriteUtf8(writer, manifest.FileVersion);
			writer.Write(manifest.EnableAddressable);
			writer.Write(manifest.LocationToLower);
			writer.Write(manifest.IncludeAssetGUID);
			writer.Write(manifest.OutputNameStyle);
			WriteUtf8(writer, manifest.BuildPipeline);
			WriteUtf8(writer, manifest.PackageName);
			WriteUtf8(writer, manifest.PackageVersion);

			writer.Write(manifest.AssetList.Count);
			for (var i = 0; i < manifest.AssetList.Count; i++)
			{
				var packageAsset = manifest.AssetList[i];
				WriteUtf8(writer, packageAsset.Address);
				WriteUtf8(writer, packageAsset.AssetPath);
				WriteUtf8(writer, packageAsset.AssetGUID);
				WriteUtf8Array(writer, packageAsset.AssetTags);
				writer.Write(packageAsset.BundleID);
			}

			writer.Write(manifest.BundleList.Count);
			for (var i = 0; i < manifest.BundleList.Count; i++)
			{
				var packageBundle = manifest.BundleList[i];
				WriteUtf8(writer, packageBundle.BundleName);
				writer.Write(packageBundle.UnityCRC);
				WriteUtf8(writer, packageBundle.FileHash);
				WriteUtf8(writer, packageBundle.FileCRC);
				writer.Write(packageBundle.FileSize);
				writer.Write(packageBundle.Encrypted);
				WriteUtf8Array(writer, packageBundle.Tags);
				WriteInt32Array(writer, packageBundle.DependIDs);
			}

			writer.Flush();
		}

		private static void WriteUtf8(BinaryWriter writer, string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				writer.Write((ushort)0);
				return;
			}

			var utf8Bytes = Encoding.UTF8.GetBytes(value);
			if (utf8Bytes.Length > ushort.MaxValue)
			{
				throw new InvalidOperationException("Runtime 清单字段长度超过 UInt16 上限。");
			}

			writer.Write((ushort)utf8Bytes.Length);
			writer.Write(utf8Bytes);
		}

		private static void WriteUtf8Array(BinaryWriter writer, string[] values)
		{
			if (values == null || values.Length == 0)
			{
				writer.Write((ushort)0);
				return;
			}

			if (values.Length > ushort.MaxValue)
			{
				throw new InvalidOperationException("Runtime 清单数组长度超过 UInt16 上限。");
			}

			writer.Write((ushort)values.Length);
			for (var i = 0; i < values.Length; i++)
			{
				WriteUtf8(writer, values[i]);
			}
		}

		private static void WriteInt32Array(BinaryWriter writer, int[] values)
		{
			if (values == null || values.Length == 0)
			{
				writer.Write((ushort)0);
				return;
			}

			if (values.Length > ushort.MaxValue)
			{
				throw new InvalidOperationException("Runtime 清单数组长度超过 UInt16 上限。");
			}

			writer.Write((ushort)values.Length);
			for (var i = 0; i < values.Length; i++)
			{
				writer.Write(values[i]);
			}
		}

		private enum BuildBackendMode
		{
			Pck = 0,
			YooAsset = 1,
			Both = 2
		}

		private readonly struct YooBuildResult
		{
			public string PackageName { get; init; }
			public string PackageVersion { get; init; }
			public string OutputRoot { get; init; }
			public string PackageRoot { get; init; }
			public int FileCount { get; init; }
			public long TotalBytes { get; init; }
		}

		private readonly struct YooBuildBundleEntry
		{
			public string SourceFilePath { get; init; }
			public string MountPath { get; init; }
			public string BundleName { get; init; }
			public string OutputFileName { get; init; }
			public string DestinationFilePath { get; init; }
			public string FileHash { get; init; }
			public string FileCRC { get; init; }
			public long FileSize { get; init; }
			public string[] Tags { get; init; }
		}

		private sealed partial class GlobalAssetTree : Tree
		{
			public override Variant _GetDragData(Vector2 atPosition)
			{
				var item = GetItemAtPosition(atPosition);
				if (item == null)
				{
					return default;
				}

				var sourcePath = item.GetMetadata(0).ToString();
				if (string.IsNullOrWhiteSpace(sourcePath))
				{
					return default;
				}

				var preview = new Label
				{
					Text = $"拖拽资源: {Path.GetFileName(sourcePath)}"
				};
				SetDragPreview(preview);
				return DragAssetPrefix + sourcePath;
			}
		}

		private sealed partial class PackageAssetTree : Tree
		{
			public Action<string> AssetDropped;

			public override bool _CanDropData(Vector2 atPosition, Variant data)
			{
				return TryParseDragAssetPath(data, out _);
			}

			public override void _DropData(Vector2 atPosition, Variant data)
			{
				if (!TryParseDragAssetPath(data, out var sourcePath))
				{
					return;
				}

				AssetDropped?.Invoke(sourcePath);
			}
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

		private sealed class GlobalAssetItem
		{
			public string Name { get; set; } = string.Empty;
			public string AssetType { get; set; } = string.Empty;
			public string ResourcePath { get; set; } = string.Empty;
		}

		private sealed class BatchBuildConfig
		{
			public string BuildBackend { get; set; } = BuildBackendBoth;
			public List<BatchBuildPackage> Packages { get; set; } = new List<BatchBuildPackage>();
		}

		private sealed class BatchBuildPackage
		{
			public string PackageName { get; set; } = string.Empty;
			public string OutputPck { get; set; } = string.Empty;
			public string OutputYoo { get; set; } = string.Empty;
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
