#if TOOLS
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Godot;
using IOFile = System.IO.File;

namespace GameFrameX.Editor
{
    /// <summary>
    /// 脚本宏定义编辑窗口。
    /// </summary>
    [Tool]
    public partial class ScriptingDefineSymbolsWindow : Window
    {
        private const string SymbolCatalogPath = "user://gameframex_scripting_define_symbol_catalog.txt";
        private const string SymbolRuleConfigPath = "res://addons/com.gameframex.godot/Editor/Misc/define_symbols.rules.json";
        private const string UngroupedBucketName = "UNGROUPED";

        private readonly List<string> m_CurrentSymbols = new List<string>();
        private readonly HashSet<string> m_CatalogSymbols = new HashSet<string>(StringComparer.Ordinal);
        private readonly Dictionary<string, SymbolRuleItem> m_SymbolRules = new Dictionary<string, SymbolRuleItem>(StringComparer.Ordinal);
        private readonly Dictionary<string, List<string>> m_SymbolGroups = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        private Tree m_SymbolTree;
        private LineEdit m_AddSymbolEdit;
        private Label m_StatusLabel;
        private bool m_Initialized;
        private bool m_SyncingUi;

        public override void _Ready()
        {
            if (m_Initialized)
            {
                return;
            }

            m_Initialized = true;
            BuildUi();
            RefreshFromProject();
        }

        public override void _Notification(int what)
        {
            if (what == NotificationWMCloseRequest)
            {
                OnCloseRequested();
            }
        }

        public void PrepareForDisplay()
        {
            if (!m_Initialized)
            {
                return;
            }

            RefreshFromProject();
        }

        private void BuildUi()
        {
            Title = "Scripting Define Symbols";
            MinSize = new Vector2I(980, 700);
            Size = new Vector2I(1120, 700);

            var root = new VBoxContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            root.AnchorLeft = 0f;
            root.AnchorTop = 0f;
            root.AnchorRight = 1f;
            root.AnchorBottom = 1f;
            root.AddThemeConstantOverride("separation", 8);
            AddChild(root);

            var tipLabel = new Label
            {
                Text = "勾选表示启用宏。新增/删除/开关会自动保存并触发重编译。",
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            root.AddChild(tipLabel);

            var symbolTitleLabel = new Label
            {
                Text = "宏列表（可选择、可滚动）:",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            root.AddChild(symbolTitleLabel);

            var symbolPanel = new PanelContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill,
                CustomMinimumSize = new Vector2(0f, 360f)
            };
            symbolPanel.AddThemeStyleboxOverride("panel", CreateListPanelStyle());
            root.AddChild(symbolPanel);

            var symbolMargin = new MarginContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            symbolMargin.AddThemeConstantOverride("margin_left", 8);
            symbolMargin.AddThemeConstantOverride("margin_right", 8);
            symbolMargin.AddThemeConstantOverride("margin_top", 8);
            symbolMargin.AddThemeConstantOverride("margin_bottom", 8);
            symbolPanel.AddChild(symbolMargin);

            m_SymbolTree = new Tree
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill,
                HideRoot = true,
                Columns = 1,
                ColumnTitlesVisible = false,
                SelectMode = Tree.SelectModeEnum.Single
            };
            m_SymbolTree.SetColumnExpand(0, true);
            m_SymbolTree.SetColumnClipContent(0, false);
            m_SymbolTree.AddThemeStyleboxOverride("panel", CreateListInnerStyle());
            m_SymbolTree.ItemEdited += OnSymbolTreeItemEdited;
            symbolMargin.AddChild(m_SymbolTree);

            var symbolActionRow = new HBoxContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            symbolActionRow.AddThemeConstantOverride("separation", 8);
            root.AddChild(symbolActionRow);

            m_AddSymbolEdit = new LineEdit
            {
                PlaceholderText = "输入新宏名，例如 FAIRY_GUI",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            m_AddSymbolEdit.TextSubmitted += OnAddSymbolSubmitted;
            symbolActionRow.AddChild(m_AddSymbolEdit);

            var addButton = new Button { Text = "添加宏" };
            addButton.Pressed += OnAddSymbolPressed;
            symbolActionRow.AddChild(addButton);

            var removeButton = new Button { Text = "删除选中宏" };
            removeButton.Pressed += OnRemoveSelectedSymbolPressed;
            symbolActionRow.AddChild(removeButton);

            var bottomRow = new HBoxContainer
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            bottomRow.AddThemeConstantOverride("separation", 8);
            root.AddChild(bottomRow);

            var refreshButton = new Button { Text = "刷新" };
            refreshButton.Pressed += RefreshFromProject;
            bottomRow.AddChild(refreshButton);

            var saveButton = new Button { Text = "保存并重编译" };
            saveButton.Pressed += SaveDefineConstants;
            bottomRow.AddChild(saveButton);

            m_StatusLabel = new Label
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };
            bottomRow.AddChild(m_StatusLabel);
        }

        private void RefreshFromProject()
        {
            try
            {
                var symbols = ScriptingDefineSymbols.GetScriptingDefineSymbols();
                GD.Print($"[ScriptingDefineSymbolsWindow] symbols length={symbols.Length}");
                m_CurrentSymbols.Clear();
                m_CurrentSymbols.AddRange(symbols);
                LoadCatalogSymbols();
                var catalogChanged = LoadSymbolRules();
                for (var i = 0; i < symbols.Length; i++)
                {
                    if (AddSymbolToCatalog(symbols[i]))
                    {
                        catalogChanged = true;
                    }
                }

                if (catalogChanged)
                {
                    PersistCatalogSymbols();
                }
                RefreshSymbolChecklist(m_CurrentSymbols, null);
                SetStatus($"已读取 {m_CurrentSymbols.Count} 个宏定义。");
            }
            catch (Exception exception)
            {
                SetStatus($"读取失败：{exception.Message}", true);
            }
        }

        private void SaveDefineConstants()
        {
            SaveDefineConstantsInternal(null);
        }

        private void SaveDefineConstantsInternal(string preferredSymbol)
        {
            try
            {
                var symbols = m_CurrentSymbols
                    .Where(static x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.Ordinal)
                    .ToArray();
                ScriptingDefineSymbols.SetScriptingDefineSymbols(symbols);
                m_CurrentSymbols.Clear();
                m_CurrentSymbols.AddRange(symbols);
                RefreshSymbolChecklist(m_CurrentSymbols, preferredSymbol);
                SetStatus($"已保存 {symbols.Length} 个宏定义。");
            }
            catch (Exception exception)
            {
                SetStatus($"保存失败：{exception.Message}", true);
            }
        }

        private void OnAddSymbolSubmitted(string text)
        {
            OnAddSymbolPressed();
        }

        private void OnAddSymbolPressed()
        {
            var normalized = NormalizeSymbolName(m_AddSymbolEdit?.Text);
            GD.Print($"[ScriptingDefineSymbolsWindow] OnAddSymbolPressed={normalized}");
            if (string.IsNullOrWhiteSpace(normalized))
            {
                SetStatus("宏名不能为空，且不能包含空格或分号。", true);
                return;
            }

            AddSymbol(m_CurrentSymbols, normalized);
            AddSymbolToCatalog(normalized);
            PersistCatalogSymbols();
            if (m_AddSymbolEdit != null)
            {
                m_AddSymbolEdit.Clear();
            }

            SaveDefineConstantsInternal(normalized);
            SetStatus($"已添加并保存宏：{normalized}");
        }

        private void OnRemoveSelectedSymbolPressed()
        {
            if (m_SymbolTree == null)
            {
                return;
            }

            var selectedItem = m_SymbolTree.GetSelected();
            if (selectedItem == null)
            {
                SetStatus("请先选择要删除的宏。", true);
                return;
            }

            var metadata = selectedItem.GetMetadata(0);
            if (metadata.VariantType == Variant.Type.Nil)
            {
                SetStatus("请先选择具体宏项（不是分组标题）。", true);
                return;
            }

            var symbol = metadata.ToString();
            if (string.IsNullOrWhiteSpace(symbol))
            {
                SetStatus("选中的宏无效。", true);
                return;
            }

            RemoveSymbol(m_CurrentSymbols, symbol);
            RemoveSymbolFromCatalog(symbol);
            PersistCatalogSymbols();
            SaveDefineConstantsInternal(null);
            SetStatus($"已删除并保存宏：{symbol}");
        }

        private void OnSymbolTreeItemEdited()
        {
            if (m_SyncingUi || m_SymbolTree == null)
            {
                return;
            }

            var editedItem = m_SymbolTree.GetEdited();
            if (editedItem == null)
            {
                return;
            }

            var symbol = editedItem.GetMetadata(0).ToString();
            if (string.IsNullOrWhiteSpace(symbol))
            {
                return;
            }

            var enabled = editedItem.IsChecked(0);
            var catalogChanged = false;
            if (enabled)
            {
                AddSymbol(m_CurrentSymbols, symbol);
                if (AddSymbolToCatalog(symbol))
                {
                    catalogChanged = true;
                }

                if (ApplyEnableRules(symbol))
                {
                    catalogChanged = true;
                }
            }
            else
            {
                RemoveSymbol(m_CurrentSymbols, symbol);
            }

            if (catalogChanged)
            {
                PersistCatalogSymbols();
            }

            SetStatus($"已更新并保存宏：{symbol} = {(enabled ? "ON" : "OFF")}");
            CallDeferred(nameof(SaveDefineConstantsAfterTreeEdited), symbol);
        }

        private void SaveDefineConstantsAfterTreeEdited(string preferredSymbol)
        {
            SaveDefineConstantsInternal(preferredSymbol);
        }

        private void RefreshSymbolChecklist(IReadOnlyCollection<string> enabledSymbols, string preferredSymbol)
        {
            if (m_SymbolTree == null)
            {
                return;
            }

            var activeSymbols = enabledSymbols == null
                ? new HashSet<string>(StringComparer.Ordinal)
                : new HashSet<string>(enabledSymbols, StringComparer.Ordinal);

            var allSymbols = new HashSet<string>(m_CatalogSymbols, StringComparer.Ordinal);
            foreach (var symbol in activeSymbols)
            {
                if (!string.IsNullOrWhiteSpace(symbol))
                {
                    allSymbols.Add(symbol);
                }
            }

            var orderedSymbols = allSymbols
                .Where(static x => !string.IsNullOrWhiteSpace(x))
                .OrderBy(GetSymbolGroupName, StringComparer.Ordinal)
                .ThenBy(static x => x, StringComparer.Ordinal)
                .ToList();

            m_SyncingUi = true;
            m_SymbolTree.Clear();
            var root = m_SymbolTree.CreateItem();
            TreeItem selectedItem = null;

            var groupedSymbols = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            for (var i = 0; i < orderedSymbols.Count; i++)
            {
                var symbol = orderedSymbols[i];
                var groupName = GetSymbolGroupName(symbol);
                if (string.IsNullOrWhiteSpace(groupName))
                {
                    groupName = UngroupedBucketName;
                }

                if (!groupedSymbols.TryGetValue(groupName, out var symbolList))
                {
                    symbolList = new List<string>();
                    groupedSymbols[groupName] = symbolList;
                }

                symbolList.Add(symbol);
            }

            var orderedGroups = groupedSymbols.Keys
                .OrderBy(static x => x == UngroupedBucketName ? string.Empty : x, StringComparer.Ordinal)
                .ToList();

            for (var i = 0; i < orderedGroups.Count; i++)
            {
                var groupName = orderedGroups[i];
                var groupItem = m_SymbolTree.CreateItem(root);
                groupItem.SetSelectable(0, true);
                groupItem.SetText(0, GetGroupDisplayName(groupName));
                groupItem.Collapsed = false;

                var symbolsInGroup = groupedSymbols[groupName];
                for (var j = 0; j < symbolsInGroup.Count; j++)
                {
                    var symbol = symbolsInGroup[j];
                    var item = m_SymbolTree.CreateItem(groupItem);
                    item.SetCellMode(0, TreeItem.TreeCellMode.Check);
                    item.SetEditable(0, true);
                    item.SetSelectable(0, true);
                    item.SetText(0, GetSymbolDisplayName(symbol));
                    item.SetChecked(0, activeSymbols.Contains(symbol));
                    item.SetMetadata(0, symbol);
                    if (!string.IsNullOrWhiteSpace(preferredSymbol) &&
                        string.Equals(preferredSymbol, symbol, StringComparison.Ordinal))
                    {
                        selectedItem = item;
                    }
                }
            }

            if (selectedItem == null && root.GetFirstChild() is TreeItem firstItem)
            {
                selectedItem = firstItem.GetFirstChild() ?? firstItem;
            }

            if (selectedItem != null)
            {
                selectedItem.Select(0);
                m_SymbolTree.ScrollToItem(selectedItem, true);
            }

            m_SyncingUi = false;
        }

        private bool LoadSymbolRules()
        {
            m_SymbolRules.Clear();
            m_SymbolGroups.Clear();

            var absolutePath = ProjectSettings.GlobalizePath(SymbolRuleConfigPath);
            if (!IOFile.Exists(absolutePath))
            {
                return false;
            }

            try
            {
                var json = IOFile.ReadAllText(absolutePath);
                var config = JsonSerializer.Deserialize<SymbolRuleConfig>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (config?.symbols == null || config.symbols.Length == 0)
                {
                    return false;
                }

                var catalogChanged = false;
                for (var i = 0; i < config.symbols.Length; i++)
                {
                    var rule = config.symbols[i];
                    if (rule == null)
                    {
                        continue;
                    }

                    var normalized = NormalizeSymbolName(rule.symbol);
                    if (string.IsNullOrWhiteSpace(normalized))
                    {
                        continue;
                    }

                    rule.symbol = normalized;
                    rule.group = string.IsNullOrWhiteSpace(rule.group) ? string.Empty : rule.group.Trim();
                    rule.mode = string.IsNullOrWhiteSpace(rule.mode) ? string.Empty : rule.mode.Trim();
                    rule.conflicts = NormalizeSymbolArray(rule.conflicts);
                    rule.implies = NormalizeSymbolArray(rule.implies);
                    m_SymbolRules[normalized] = rule;

                    if (!string.IsNullOrWhiteSpace(rule.group))
                    {
                        if (!m_SymbolGroups.TryGetValue(rule.group, out var list))
                        {
                            list = new List<string>();
                            m_SymbolGroups.Add(rule.group, list);
                        }

                        var exists = false;
                        for (var j = 0; j < list.Count; j++)
                        {
                            if (string.Equals(list[j], normalized, StringComparison.Ordinal))
                            {
                                exists = true;
                                break;
                            }
                        }

                        if (!exists)
                        {
                            list.Add(normalized);
                        }
                    }

                    if (AddSymbolToCatalog(normalized))
                    {
                        catalogChanged = true;
                    }
                }

                return catalogChanged;
            }
            catch (Exception exception)
            {
                GD.PushWarning($"[ScriptingDefineSymbolsWindow] load rules failed: {exception.Message}");
                return false;
            }
        }

        private static string[] NormalizeSymbolArray(string[] symbols)
        {
            if (symbols == null || symbols.Length == 0)
            {
                return Array.Empty<string>();
            }

            var values = new HashSet<string>(StringComparer.Ordinal);
            for (var i = 0; i < symbols.Length; i++)
            {
                var normalized = NormalizeSymbolName(symbols[i]);
                if (!string.IsNullOrWhiteSpace(normalized))
                {
                    values.Add(normalized);
                }
            }

            return values.ToArray();
        }

        private bool ApplyEnableRules(string symbol)
        {
            if (!m_SymbolRules.TryGetValue(symbol, out var rule) || rule == null)
            {
                return false;
            }

            var catalogChanged = false;

            // single + group 表示该组互斥，仅允许一个启用。
            if (string.Equals(rule.mode, "single", StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(rule.group) &&
                m_SymbolGroups.TryGetValue(rule.group, out var groupedSymbols))
            {
                for (var i = 0; i < groupedSymbols.Count; i++)
                {
                    var groupedSymbol = groupedSymbols[i];
                    if (!string.Equals(groupedSymbol, symbol, StringComparison.Ordinal))
                    {
                        RemoveSymbol(m_CurrentSymbols, groupedSymbol);
                    }
                }
            }

            if (rule.conflicts != null)
            {
                for (var i = 0; i < rule.conflicts.Length; i++)
                {
                    RemoveSymbol(m_CurrentSymbols, rule.conflicts[i]);
                }
            }

            if (rule.implies != null)
            {
                for (var i = 0; i < rule.implies.Length; i++)
                {
                    var implied = rule.implies[i];
                    AddSymbol(m_CurrentSymbols, implied);
                    if (AddSymbolToCatalog(implied))
                    {
                        catalogChanged = true;
                    }
                }
            }

            return catalogChanged;
        }

        private string GetSymbolGroupName(string symbol)
        {
            if (m_SymbolRules.TryGetValue(symbol, out var rule) && rule != null && !string.IsNullOrWhiteSpace(rule.group))
            {
                return rule.group;
            }

            return string.Empty;
        }

        private string GetSymbolDisplayName(string symbol)
        {
            if (!m_SymbolRules.TryGetValue(symbol, out var rule) || rule == null)
            {
                return symbol;
            }

            var extras = new List<string>();
            var label = string.IsNullOrWhiteSpace(rule.label) ? string.Empty : rule.label.Trim();
            if (!string.IsNullOrWhiteSpace(label) &&
                !string.Equals(label, symbol, StringComparison.Ordinal))
            {
                extras.Add(label);
            }

            if (extras.Count == 0)
            {
                return symbol;
            }

            return $"{symbol} ({string.Join(", ", extras)})";
        }

        private static string GetGroupDisplayName(string groupName)
        {
            if (string.Equals(groupName, UngroupedBucketName, StringComparison.Ordinal))
            {
                return "GENERAL";
            }

            return $"[{groupName}]";
        }

        private void LoadCatalogSymbols()
        {
            m_CatalogSymbols.Clear();
            if (!FileAccess.FileExists(SymbolCatalogPath))
            {
                return;
            }

            using var file = FileAccess.Open(SymbolCatalogPath, FileAccess.ModeFlags.Read);
            if (file == null)
            {
                return;
            }

            while (!file.EofReached())
            {
                var symbol = NormalizeSymbolName(file.GetLine());
                if (!string.IsNullOrWhiteSpace(symbol))
                {
                    m_CatalogSymbols.Add(symbol);
                }
            }
        }

        private void PersistCatalogSymbols()
        {
            using var file = FileAccess.Open(SymbolCatalogPath, FileAccess.ModeFlags.Write);
            if (file == null)
            {
                return;
            }

            foreach (var symbol in m_CatalogSymbols.OrderBy(static x => x, StringComparer.Ordinal))
            {
                file.StoreLine(symbol);
            }
        }

        private bool AddSymbolToCatalog(string symbol)
        {
            var normalized = NormalizeSymbolName(symbol);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return false;
            }

            return m_CatalogSymbols.Add(normalized);
        }

        private void RemoveSymbolFromCatalog(string symbol)
        {
            var normalized = NormalizeSymbolName(symbol);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return;
            }

            m_CatalogSymbols.Remove(normalized);
        }

        private static string NormalizeSymbolName(string rawSymbol)
        {
            if (string.IsNullOrWhiteSpace(rawSymbol))
            {
                return string.Empty;
            }

            var symbol = rawSymbol.Trim();
            if (symbol.Contains(';') || symbol.Contains(' ') || symbol.Contains('\t') || symbol.Contains('\n'))
            {
                return string.Empty;
            }

            return symbol;
        }

        private static StyleBoxFlat CreateListPanelStyle()
        {
            var style = new StyleBoxFlat
            {
                BgColor = new Color(0.12f, 0.12f, 0.13f, 1f),
                BorderColor = new Color(0.22f, 0.22f, 0.24f, 1f)
            };
            style.SetBorderWidthAll(1);
            style.SetCornerRadiusAll(6);
            return style;
        }

        private static StyleBoxFlat CreateListInnerStyle()
        {
            var style = new StyleBoxFlat
            {
                BgColor = new Color(0.09f, 0.09f, 0.10f, 1f),
                BorderColor = new Color(0.18f, 0.18f, 0.20f, 1f)
            };
            style.SetBorderWidthAll(1);
            style.SetCornerRadiusAll(4);
            return style;
        }

        private static void AddSymbol(List<string> symbols, string symbol)
        {
            if (symbols == null || string.IsNullOrWhiteSpace(symbol))
            {
                return;
            }

            for (var i = 0; i < symbols.Count; i++)
            {
                if (string.Equals(symbols[i], symbol, StringComparison.Ordinal))
                {
                    return;
                }
            }

            symbols.Add(symbol);
        }

        private static void RemoveSymbol(List<string> symbols, string symbol)
        {
            if (symbols == null || string.IsNullOrWhiteSpace(symbol))
            {
                return;
            }

            for (var i = symbols.Count - 1; i >= 0; i--)
            {
                if (string.Equals(symbols[i], symbol, StringComparison.Ordinal))
                {
                    symbols.RemoveAt(i);
                }
            }
        }

        private void SetStatus(string message, bool isError = false)
        {
            if (m_StatusLabel == null)
            {
                return;
            }

            m_StatusLabel.Text = message ?? string.Empty;
            m_StatusLabel.Modulate = isError ? new Color(1f, 0.62f, 0.62f) : new Color(0.62f, 0.95f, 0.62f);
        }

        private void OnCloseRequested()
        {
            Hide();
        }

        private sealed class SymbolRuleConfig
        {
            public SymbolRuleItem[] symbols { get; set; } = Array.Empty<SymbolRuleItem>();
        }

        private sealed class SymbolRuleItem
        {
            public string symbol { get; set; }
            public string label { get; set; }
            public string group { get; set; }
            public string mode { get; set; }
            public string[] conflicts { get; set; }
            public string[] implies { get; set; }
        }
    }
}
#endif
