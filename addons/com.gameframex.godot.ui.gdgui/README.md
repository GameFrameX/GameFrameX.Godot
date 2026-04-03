# com.gameframex.godot.ui.gdgui

GameFrameX 在 Godot 下的 `UGUI` 风格适配层，提供 UIManager、UIFormHelper、UIGroupHelper、按钮/图片扩展与裁剪保活辅助。

## 依赖

- `com.gameframex.godot`
- `com.gameframex.godot.ui`

## 已提供内容

- `UIManager`：基于 `BaseUIManager` 的 GDGUI 实现。
- `UGUIFormHelper`：使用 `PackedScene.Instantiate()` 创建 UI，使用 `QueueFree()` 释放。
- `UGUIUIGroupHelper`：按 UIGroup 创建容器并管理深度。
- `UGUI`：UI 基类，适配显示/隐藏处理器。
- `UGUIButtonExtension`：按钮事件 `Add/Remove/Set/Clear`。
- `UGUIImageExtension` + `UIImage`：图片异步设置纹理能力。
- `GameFrameXUIGDGUICroppingHelper`：防裁剪类型引用保活。

## 默认接入行为

项目已支持自动接入：

- `UIComponent` 的默认 `UIFormHelper` 指向 `GameFrameX.UI.GDGUI.Runtime.UGUIFormHelper`。
- `UIComponent` 的默认 `UIGroupHelper` 指向 `GameFrameX.UI.GDGUI.Runtime.UGUIUIGroupHelper`。
- 当 `componentType` 为空时，优先解析 `GameFrameX.UI.GDGUI.Runtime.UIManager`。

## 回退策略

当 GDGUI 类型在运行时不可解析时，会自动回退到基础实现：

- `UIManager` 回退到 `GameFrameX.UI.Runtime.UIManager`。
- `UIFormHelper` 回退到 `GameFrameX.UI.Runtime.DefaultUIFormHelper`。
- `UIGroupHelper` 回退到 `GameFrameX.UI.Runtime.DefaultUIGroupHelper`。

## 资源路径约定

`UIManager.Open` 在加载 `PackedScene` 时会按以下顺序尝试：

1. 原始路径
2. `原始路径.tscn`
3. `原始路径.scn`

并支持将包含 `/Godot/` 的绝对路径规范化为 `res://` 资源路径。

## 最小使用示例

```csharp
using GameFrameX.UI.GDGUI.Runtime;

namespace Demo
{
    /// <summary>
    /// 示例界面。
    /// </summary>
    public partial class DemoMainUI : UGUI
    {
    }
}
```

## 备注

- 插件元信息见 `plugin.cfg` 与 `package.json`。
- 若需要切换自定义 Helper，可在 `UIComponent` 的导出字段中覆盖类型名。
