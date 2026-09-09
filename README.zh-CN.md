<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="Game Frame X Logo" width="160" />

# GameFrameX Godot

[![License](https://img.shields.io/badge/license-blue.svg)](LICENSE)
[![Version](https://img.shields.io/github/v/release/GameFrameX/GameFrameX.Godot)](https://github.com/GameFrameX/GameFrameX.Godot/releases)
[![Godot Version](https://img.shields.io/badge/Godot-4.7-blue?logo=godotengine)](https://godotengine.org/)
[![Documentation](https://img.shields.io/badge/Documentation-docs-blue)](https://gameframex.doc.alianblank.com)

[![Discord](https://img.shields.io/badge/-5865F2?logo=discord&logoColor=white)](https://discord.gg/VDWUjWMDw9)
[![GitHub](https://img.shields.io/badge/-181717?logo=github&logoColor=white)](https://github.com/GameFrameX/gameframex)
[![Bilibili](https://img.shields.io/badge/-00A1D6?logo=bilibili&logoColor=white)](https://www.bilibili.com/video/BV1yrpeepEn7)
[![Gitee](https://img.shields.io/badge/-C71D23?logo=gitee&logoColor=white)](https://gitee.com/GameFrameX/gameframex)

独立游戏前后端一体化解决方案 · 独立游戏开发者的圆梦大使

<br />

[文档](https://gameframex.doc.alianblank.com) · [快速开始](#快速开始) · QQ群: 467608841 / 233840761

<br />

[English](README.md) | **简体中文** | [繁體中文](README.zh-TW.md) | [日本語](README.ja.md) | [한국어](README.ko.md)

</div>

## 项目简介

GameFrameX Godot 是 GameFrameX 框架的 Godot .NET 项目与集成示例，整合可复用运行时模块、编辑器插件、热更新程序集、资源驱动的 UI 示例和验证场景。

### 功能特性

- 提供生命周期、对象池、引用池、事件、有限状态机、定时器、实体、设置和全局配置等模块化运行时组件。
- 提供资源包、版本与更新检查、下载流程、流程状态和热更新程序集加载能力。
- 提供网络、Web、Web Protobuf、本地化、配置表和渠道工具。
- 提供 Godot GUI 与 FairyGUI UI 适配和示例，支持界面生命周期、分组、层级及异步加载。
- 提供编辑器插件，以及用于资源、配置、协议、UI 和项目工作流检查的验证场景。

## 快速开始

### 安装

环境要求：

- Godot .NET 4.7.1。
- .NET 8 SDK；Android 构建根据项目配置使用 .NET 9。

克隆仓库，在 Godot .NET 编辑器中打开项目目录，并等待 .NET 依赖还原。GameFrameX 模块位于 `addons/`，核心模块通过 `project.godot` 启用。

可运行以下场景：

- `Scenes/Launcher.tscn`：使用场景配置的组件链。
- `Scenes/LauncherAuto.tscn`：使用代码创建的组件链。
- `Scenes/Verification/AssetSystemRuntimeVerifier.tscn`：资源系统运行时检查。
- `Scenes/Verification/ProtoMessageRuntimeVerifier.tscn`：Protobuf HTTP 回环检查。

## 使用示例

已注册的运行时组件可以通过 `GameEntry` 获取：

```csharp
using Godot;
using GameFrameX.Runtime;

public partial class GameManager : Node
{
    public override void _Ready()
    {
        var objectPool = GameEntry.GetComponent<ObjectPoolComponent>();
        var ui = GameEntry.GetComponent<GameFrameX.UI.Runtime.UIComponent>();
    }
}
```

模块配置和 API 请参阅对应 `addons/com.gameframex.godot.*` 目录中的 README。

## 架构概览

- `addons/`：可复用的 GameFrameX Godot 模块和编辑器插件。
- `Scripts/`：项目级启动、流程、UI 流程和验证脚本。
- `Assets/`：热更新程序集、生成的配置代码、UI 资源和运行时数据。

`Scenes/Launcher.tscn` 序列化标准组件链；`Scenes/LauncherAuto.tscn` 使用 C# 构建等价组件链，用于代码驱动的启动验证。

## 平台支持

项目目标为 Godot 4.7 .NET，并声明使用 Mobile 渲染特性。项目文件包含通用 .NET 8 目标和 Android 专用的 .NET 9 目标。发布前请在 Godot .NET 编辑器中分别验证各目标平台的导出行为。

## 依赖

- Godot .NET SDK 4.7.1。
- .NET 8 SDK；Android 配置目标为 .NET 9。
- `SharpZipLib` 1.4.2。
- `addons/` 中的 GameFrameX 模块，包括 Asset、AssetSystem、Config、Download、Entity、Event、FSM、Localization、Network、Procedure、Setting、Timer、UI、Web 和 Web Protobuf。
- FairyGUI 和 GDGUI 模块提供可选的 UI 集成。

## 文档与资源

- [GameFrameX 文档](https://gameframex.doc.alianblank.com)
- [GameFrameX GitHub 组织](https://github.com/GameFrameX)
- [GameFrameX Gitee 组织](https://gitee.com/GameFrameX)
- [Godot 文档](https://docs.godotengine.org/)
- [仓库发布版本](https://github.com/GameFrameX/GameFrameX.Godot/releases)

## 社区与支持

![QQ](https://img.shields.io/badge/QQ-467608841%2F233840761-EB1923?style=for-the-badge&logo=qq&logoColor=white)
[![Bilibili](https://img.shields.io/badge/Bilibili-00A1D6?style=for-the-badge&logo=bilibili&logoColor=white)](https://www.bilibili.com/video/BV1yrpeepEn7)
[![Gitee](https://img.shields.io/badge/Gitee-C71D23?style=for-the-badge&logo=gitee&logoColor=white)](https://gitee.com/GameFrameX/gameframex)
[![Discord](https://img.shields.io/badge/Discord-5865F2?style=for-the-badge&logo=discord&logoColor=white)](https://discord.gg/VDWUjWMDw9)
[![GitHub](https://img.shields.io/badge/GitHub-181717?style=for-the-badge&logo=github&logoColor=white)](https://github.com/GameFrameX/gameframex)
[<img src="https://cdn.jsdelivr.net/npm/devicon@2/icons/linkedin/linkedin-original.svg" height="28" alt="LinkedIn" />](https://www.linkedin.com/in/alianblank)
[![Reddit](https://img.shields.io/badge/Reddit-FF4500?style=for-the-badge&logo=reddit&logoColor=white)](https://www.reddit.com/r/GameFrameX/)
[![X](https://img.shields.io/badge/X-000000?style=for-the-badge&logo=x&logoColor=white)](https://x.com/alian_blank)
[![YouTube](https://img.shields.io/badge/YouTube-FF0000?style=for-the-badge&logo=youtube&logoColor=white)](https://www.youtube.com/channel/UCD9QhSFJ5xZkn5NTSV-DVAw)
[![Bluesky](https://img.shields.io/badge/Bluesky-0285FF?style=for-the-badge&logo=bluesky&logoColor=white)](https://bsky.app/profile/alianblank.bsky.social)

## 更新日志

项目变更请参阅[仓库发布版本](https://github.com/GameFrameX/GameFrameX.Godot/releases)和[提交历史](https://github.com/GameFrameX/GameFrameX.Godot/commits/main/)。

## 开源协议

详见 [LICENSE](LICENSE) 文件。

<!--
EN: See [LICENSE](LICENSE) for license information.
zh-CN: 详见 [LICENSE](LICENSE) 文件。
zh-TW: 詳見 [LICENSE](LICENSE) 檔案。
ja: 詳しくは [LICENSE](LICENSE) をご参照ください。
ko: 자세한 내용은 [LICENSE](LICENSE) 파일을 참조하세요.
-->
