<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="Game Frame X Logo" width="160" />

# Game Frame X (Godot Core)

[![License](https://img.shields.io/github/license/GameFrameX/com.gameframex.godot)](https://github.com/GameFrameX/com.gameframex.godot/blob/main/LICENSE.md)
[![Version](https://img.shields.io/github/v/release/GameFrameX/com.gameframex.godot)](https://github.com/GameFrameX/com.gameframex.godot/releases)
[![Godot Version](https://img.shields.io/badge/Godot-4.7-blue?logo=godotengine)](https://godotengine.org/)
[![Documentation](https://img.shields.io/badge/Documentation-docs-blue)](https://gameframex.doc.alianblank.com)

All-in-One Solution for Indie Game Development · Empowering Indie Developers' Dreams

<br />

[Documentation](https://gameframex.doc.alianblank.com) · [Quick Start](#installation) · QQ Group: 467608841 / 233840761

<br />

**English** | [简体中文](README.zh-CN.md)

</div>

## Overview

GameFrameX Godot core package — the essential runtime and editor foundation for the GameFrameX framework on Godot.

- **Modular architecture** — component-based extensible framework design
- **Object pool** — object reuse and memory management
- **Reference pool** — reference-type instance management, GC friendly
- **Helper libraries** — file, network and math helpers
- **Extension methods** — Godot type extensions and conveniences
- **Utility classes** — encryption, compression, hashing and more
- **Editor tooling** — build and asset workflow tools

### Core Modules

| Module | Description |
|--------|-------------|
| **Base** | Framework core: components, events, lifecycle |
| **ObjectPool** | Object reuse, memory optimization |
| **ReferencePool** | Reference-type management, GC optimization |
| **Helper** | File / network / math helpers |
| **Extension** | Godot type extensions |
| **Utility** | Encryption, compression, hashing |

### Editor Tools

| Tool | Description |
|------|-------------|
| **BuildHotfix** | Hotfix assembly build tool |
| **BuildProduct** | Product build helper |
| **PackageManager** | Package manager window |
| **Cropping** | Image cropping tool |
| **Inspector** | Custom inspector panels |

## Quick Start

### Installation

Copy the package into your Godot project's `addons/` directory:

```
addons/com.gameframex.godot/
```

The package is also published to the GameFrameX npm registry, so package-management tooling can consume it directly:

```
https://npm.cnb.cool/GameFrameX/npm/-/packages/
```

### Usage

```csharp
using Godot;
using GameFrameX.Runtime;

public partial class GameManager : Node
{
    public override void _Ready()
    {
        // Get components via the framework entry
        var objectPool = GameEntry.GetComponent<ObjectPoolComponent>();
        var referencePool = GameEntry.GetComponent<ReferencePoolComponent>();
    }
}
```

## License

Apache-2.0 — see [LICENSE.md](LICENSE.md).
