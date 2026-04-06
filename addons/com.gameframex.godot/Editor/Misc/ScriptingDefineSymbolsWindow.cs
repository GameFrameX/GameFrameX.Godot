#if TOOLS
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace GameFrameX.Editor
{
    /// <summary>
    /// 脚本宏定义编辑窗口。
    /// </summary>
    [Tool]
    public partial class ScriptingDefineSymbolsWindow : Window
    {
        private const string FairyGuiSymbol = "FAIRY_GUI";
        private const string EnableLogSymbol = "ENABLE_LOG";
        private const string IncludeAssetSystemRuntimeSymbol = "INCLUDE_ASSETSYSTEM_RUNTIME";

        private static readonly string[] CommonSymbols =
        {
            FairyGuiSymbol,
            EnableLogSymbol,
            IncludeAssetSystemRuntimeSymbol
        };

        private readonly List<string> m_CurrentSymbols = new List<string>();
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
            CloseRequested += OnCloseRequested;
        }

        public override void _ExitTree()
        {
            CloseRequested -= OnCloseRequested;
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
                Text = "勾选表示启用宏。新增/删除后点击“保存并重编译”生效。",
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
                m_CurrentSymbols.Clear();
                m_CurrentSymbols.AddRange(symbols);
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
            try
            {
                var symbols = m_CurrentSymbols
                    .Where(static x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.Ordinal)
                    .ToArray();
                ScriptingDefineSymbols.SetScriptingDefineSymbols(symbols);
                m_CurrentSymbols.Clear();
                m_CurrentSymbols.AddRange(symbols);
                RefreshSymbolChecklist(m_CurrentSymbols, null);
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
            if (string.IsNullOrWhiteSpace(normalized))
            {
                SetStatus("宏名不能为空，且不能包含空格或分号。", true);
                return;
            }

            UpsertSymbol(m_CurrentSymbols, normalized, true);
            if (m_AddSymbolEdit != null)
            {
                m_AddSymbolEdit.Clear();
            }

            RefreshSymbolChecklist(m_CurrentSymbols, normalized);
            SetStatus($"已添加宏：{normalized}（待保存）");
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

            var symbol = selectedItem.GetMetadata(0).ToString();
            if (string.IsNullOrWhiteSpace(symbol))
            {
                SetStatus("选中的宏无效。", true);
                return;
            }

            UpsertSymbol(m_CurrentSymbols, symbol, false);
            RefreshSymbolChecklist(m_CurrentSymbols, null);
            SetStatus($"已删除宏：{symbol}（待保存）");
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
            UpsertSymbol(m_CurrentSymbols, symbol, enabled);
            SetStatus($"已更新宏：{symbol} = {(enabled ? "ON" : "OFF")}（待保存）");
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

            var allSymbols = new HashSet<string>(CommonSymbols, StringComparer.Ordinal);
            foreach (var symbol in activeSymbols)
            {
                if (!string.IsNullOrWhiteSpace(symbol))
                {
                    allSymbols.Add(symbol);
                }
            }

            var orderedSymbols = allSymbols
                .Where(static x => !string.IsNullOrWhiteSpace(x))
                .OrderBy(static x => x, StringComparer.Ordinal)
                .ToList();

            m_SyncingUi = true;
            m_SymbolTree.Clear();
            var root = m_SymbolTree.CreateItem();
            TreeItem selectedItem = null;
            for (var i = 0; i < orderedSymbols.Count; i++)
            {
                var symbol = orderedSymbols[i];
                var item = m_SymbolTree.CreateItem(root);
                item.SetCellMode(0, TreeItem.TreeCellMode.Check);
                item.SetEditable(0, true);
                item.SetSelectable(0, true);
                item.SetText(0, symbol);
                item.SetChecked(0, activeSymbols.Contains(symbol));
                item.SetMetadata(0, symbol);
                if (!string.IsNullOrWhiteSpace(preferredSymbol) &&
                    string.Equals(preferredSymbol, symbol, StringComparison.Ordinal))
                {
                    selectedItem = item;
                }
            }

            if (selectedItem == null && root.GetFirstChild() is TreeItem firstItem)
            {
                selectedItem = firstItem;
            }

            if (selectedItem != null)
            {
                selectedItem.Select(0);
                m_SymbolTree.ScrollToItem(selectedItem, true);
            }

            m_SyncingUi = false;
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

        private static void UpsertSymbol(List<string> symbols, string symbol, bool enabled)
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

            if (enabled)
            {
                symbols.Add(symbol);
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
    }
}
#endif
