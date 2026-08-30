<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="Game Frame X Logo" width="160" />

# Game Frame X Network (Godot)

[![License](https://img.shields.io/github/license/GameFrameX/com.gameframex.godot.network)](https://github.com/GameFrameX/com.gameframex.godot.network/blob/main/LICENSE.md)
[![Version](https://img.shields.io/github/v/release/GameFrameX/com.gameframex.godot.network)](https://github.com/GameFrameX/com.gameframex.godot.network/releases)
[![Godot Version](https://img.shields.io/badge/Godot-4.7-blue?logo=godotengine)](https://godotengine.org/)
[![Documentation](https://img.shields.io/badge/Documentation-docs-blue)](https://gameframex.doc.alianblank.com)

All-in-One Solution for Indie Game Development · Empowering Indie Developers' Dreams

<br />

[Documentation](https://gameframex.doc.alianblank.com) · [Quick Start](#installation) · QQ Group: 467608841 / 233840761

<br />

**English** | [简体中文](README.zh-CN.md)

</div>

## Overview

GameFrameX Network — Godot channel-based network communication package. Supports multiple named connections with configurable packet handling pipelines (send/receive header and body handlers, heartbeat, compression/decompression), RPC timeout, focus-based heartbeat control, and event-driven connection status notifications.

## Quick Start

### Installation

Copy the package into your Godot project's `addons/` directory:

```
addons/com.gameframex.godot.network/
```

The package is also published to the GameFrameX npm registry, so package-management tooling can consume it directly:

```
https://npm.cnb.cool/GameFrameX/npm/-/packages/
```

### Dependencies

- [`com.gameframex.godot`](https://github.com/GameFrameX/com.gameframex.godot)
- [`com.gameframex.godot.event`](https://github.com/GameFrameX/com.gameframex.godot.event)

## License

Apache-2.0 — see [LICENSE.md](LICENSE.md).
