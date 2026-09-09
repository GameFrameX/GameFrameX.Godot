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

All-in-One Solution for Indie Game Development · Empowering Indie Developers' Dreams

<br />

[Documentation](https://gameframex.doc.alianblank.com) · [Quick Start](#quick-start) · QQ Group: 467608841 / 233840761

<br />

**English** | [简体中文](README.zh-CN.md) | [繁體中文](README.zh-TW.md) | [日本語](README.ja.md) | [한국어](README.ko.md)

</div>

## Project Overview

GameFrameX Godot is the Godot .NET project and integration sample for the GameFrameX framework. It combines reusable runtime modules, editor plugins, hotfix assemblies, resource-driven UI examples, and verification scenes.

### Features

- Modular components for lifecycle, object pooling, reference pooling, events, FSM, timers, entities, settings, and global configuration.
- Asset and startup workflows for resource packages, version checks, downloading, procedures, and hotfix assembly loading.
- Network, Web, Web Protobuf, localization, configuration-table, and channel utilities.
- Godot GUI and FairyGUI UI adapters with form lifecycle, grouping, depth, and asynchronous loading support.
- Editor plugins and runtime verification scenes for asset, configuration, protocol, UI, and project workflows.

## Quick Start

### Installation

Requirements:

- Godot .NET 4.7.1.
- .NET 8 SDK. Android builds use .NET 9 according to the project configuration.

Clone the repository, open it in the Godot .NET editor, and wait for .NET dependencies to restore. GameFrameX modules are under `addons/` and core modules are enabled through `project.godot`.

Run one of these scenes:

- `Scenes/Launcher.tscn` — scene-configured component chain.
- `Scenes/LauncherAuto.tscn` — code-created component chain.
- `Scenes/Verification/AssetSystemRuntimeVerifier.tscn` — asset-system runtime check.
- `Scenes/Verification/ProtoMessageRuntimeVerifier.tscn` — Protobuf HTTP loopback check.

## Usage Examples

Registered runtime components can be retrieved through `GameEntry`:

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

See the README in the corresponding `addons/com.gameframex.godot.*` directory for module-specific configuration and APIs.

## Architecture

- `addons/` contains reusable GameFrameX Godot modules and editor plugins.
- `Scripts/` contains project startup, procedure, UI flow, and verification scripts.
- `Assets/` contains hotfix assemblies, generated configuration code, UI resources, and runtime data.

`Scenes/Launcher.tscn` serializes the standard component chain. `Scenes/LauncherAuto.tscn` builds an equivalent chain in C# for code-driven startup validation.

## Platform Support

The project targets Godot 4.7 with .NET and declares Mobile rendering features. Project files contain a general .NET 8 target and an Android-specific .NET 9 target. Validate export behavior for each target platform in the Godot .NET editor before shipping.

## Dependencies

- Godot .NET SDK 4.7.1.
- .NET 8 SDK; Android configuration targets .NET 9.
- `SharpZipLib` 1.4.2.
- GameFrameX modules under `addons/`, including Asset, AssetSystem, Config, Download, Entity, Event, FSM, Localization, Network, Procedure, Setting, Timer, UI, Web, and Web Protobuf.
- Optional UI integrations provided by the FairyGUI and GDGUI modules.

## Documentation & Resources

- [GameFrameX documentation](https://gameframex.doc.alianblank.com)
- [GameFrameX GitHub organization](https://github.com/GameFrameX)
- [GameFrameX Gitee organization](https://gitee.com/GameFrameX)
- [Godot documentation](https://docs.godotengine.org/)
- [Repository releases](https://github.com/GameFrameX/GameFrameX.Godot/releases)

## Community & Support

[![Discord](https://img.shields.io/badge/Discord-5865F2?style=for-the-badge&logo=discord&logoColor=white)](https://discord.gg/VDWUjWMDw9)
[![GitHub](https://img.shields.io/badge/GitHub-181717?style=for-the-badge&logo=github&logoColor=white)](https://github.com/GameFrameX/gameframex)
[<img src="https://cdn.jsdelivr.net/npm/devicon@2/icons/linkedin/linkedin-original.svg" height="28" alt="LinkedIn" />](https://www.linkedin.com/in/alianblank)
[![Reddit](https://img.shields.io/badge/Reddit-FF4500?style=for-the-badge&logo=reddit&logoColor=white)](https://www.reddit.com/r/GameFrameX/)
[![X](https://img.shields.io/badge/X-000000?style=for-the-badge&logo=x&logoColor=white)](https://x.com/alian_blank)
[![YouTube](https://img.shields.io/badge/YouTube-FF0000?style=for-the-badge&logo=youtube&logoColor=white)](https://www.youtube.com/channel/UCD9QhSFJ5xZkn5NTSV-DVAw)
[![Bluesky](https://img.shields.io/badge/Bluesky-0285FF?style=for-the-badge&logo=bluesky&logoColor=white)](https://bsky.app/profile/alianblank.bsky.social)
[![Bilibili](https://img.shields.io/badge/Bilibili-00A1D6?style=for-the-badge&logo=bilibili&logoColor=white)](https://www.bilibili.com/video/BV1yrpeepEn7)
[![Gitee](https://img.shields.io/badge/Gitee-C71D23?style=for-the-badge&logo=gitee&logoColor=white)](https://gitee.com/GameFrameX/gameframex)
![QQ](https://img.shields.io/badge/QQ-467608841%2F233840761-EB1923?style=for-the-badge&logo=qq&logoColor=white)

## Changelog

See the [repository releases](https://github.com/GameFrameX/GameFrameX.Godot/releases) and [commit history](https://github.com/GameFrameX/GameFrameX.Godot/commits/main/) for project changes.

## License

See [LICENSE](LICENSE) for license information.

<!--
EN: See [LICENSE](LICENSE) for license information.
zh-CN: 详见 [LICENSE](LICENSE) 文件。
zh-TW: 詳見 [LICENSE](LICENSE) 檔案。
ja: 詳しくは [LICENSE](LICENSE) をご参照ください。
ko: 자세한 내용은 [LICENSE](LICENSE) 파일을 참조하세요.
-->
