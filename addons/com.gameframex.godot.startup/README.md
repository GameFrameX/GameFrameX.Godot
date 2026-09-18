<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="Game Frame X Logo" width="160" />

# Game Frame X Startup (Godot)

[![License](https://img.shields.io/github/license/GameFrameX/com.gameframex.godot.startup)](https://github.com/GameFrameX/com.gameframex.godot.startup/blob/main/LICENSE.md)
[![Version](https://img.shields.io/github/v/release/GameFrameX/com.gameframex.godot.startup)](https://github.com/GameFrameX/com.gameframex.godot.startup/releases)
[![Godot Version](https://img.shields.io/badge/Godot-4.7-blue?logo=godotengine)](https://godotengine.org/)
[![Documentation](https://img.shields.io/badge/Documentation-docs-blue)](https://gameframex.doc.alianblank.com)

All-in-One Solution for Indie Game Development · Empowering Indie Developers' Dreams

<br />

[Documentation](https://gameframex.doc.alianblank.com) · [Quick Start](#installation) · QQ Group: 467608841 / 233840761

<br />

**English** | [简体中文](README.zh-CN.md)

</div>

## Overview

GameFrameX Startup — Godot generic game startup flow scaffold. Encapsulates the common flow from app launch to hotfix loading: launcher UI → fetch global info (primary-backup URL failover) → fetch app version → fetch asset package version → asset system patch → hotfix launch. All project-specific variables (URL list, HTTP params, hotfix entry, UI implementation) are injected via StartupOptions ScriptableObject + two interfaces (IStartupUIHandler / IHotfixLauncher).

## Quick Start

### Installation

Copy the package into your Godot project's `addons/` directory:

```
addons/com.gameframex.godot.startup/
```

The package is also published to the GameFrameX npm registry, so package-management tooling can consume it directly:

```
https://npm.cnb.cool/GameFrameX/npm/-/packages/
```

### Dependencies

- [`com.gameframex.godot`](https://github.com/GameFrameX/com.gameframex.godot)
- [`com.gameframex.godot.asset`](https://github.com/GameFrameX/com.gameframex.godot.asset)
- [`com.gameframex.godot.assetsystem`](https://github.com/GameFrameX/com.gameframex.godot.assetsystem)
- [`com.gameframex.godot.entry`](https://github.com/GameFrameX/com.gameframex.godot.entry)
- [`com.gameframex.godot.event`](https://github.com/GameFrameX/com.gameframex.godot.event)
- [`com.gameframex.godot.fsm`](https://github.com/GameFrameX/com.gameframex.godot.fsm)
- [`com.gameframex.godot.globalconfig`](https://github.com/GameFrameX/com.gameframex.godot.globalconfig)
- [`com.gameframex.godot.localization`](https://github.com/GameFrameX/com.gameframex.godot.localization)
- [`com.gameframex.godot.procedure`](https://github.com/GameFrameX/com.gameframex.godot.procedure)
- [`com.gameframex.godot.setting`](https://github.com/GameFrameX/com.gameframex.godot.setting)
- [`com.gameframex.godot.systeminfo`](https://github.com/GameFrameX/com.gameframex.godot.systeminfo)
- [`com.gameframex.godot.web`](https://github.com/GameFrameX/com.gameframex.godot.web)

## License

Apache-2.0 — see [LICENSE.md](LICENSE.md).
