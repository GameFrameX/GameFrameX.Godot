<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="Game Frame X Logo" width="160" />

# GameFrameX Timer (Godot)

[![License](https://img.shields.io/github/license/GameFrameX/com.gameframex.godot.timer)](https://github.com/GameFrameX/com.gameframex.godot.timer/blob/main/LICENSE.md)
[![Version](https://img.shields.io/github/v/release/GameFrameX/com.gameframex.godot.timer)](https://github.com/GameFrameX/com.gameframex.godot.timer/releases)
[![Godot Version](https://img.shields.io/badge/Godot-4.7-blue?logo=godotengine)](https://godotengine.org/)
[![Documentation](https://img.shields.io/badge/Documentation-docs-blue)](https://gameframex.doc.alianblank.com)

All-in-One Solution for Indie Game Development · Empowering Indie Developers' Dreams

<br />

[Documentation](https://gameframex.doc.alianblank.com) · [Quick Start](#installation) · QQ Group: 467608841 / 233840761

<br />

**English** | [简体中文](README.zh-CN.md)

</div>

## Features

- **Three timer modes** — repeated (`Add`), one-shot (`AddOnce`), per-frame (`AddUpdate`)
- **Pause / Resume** — individually by timer ID, or in bulk by tag
- **Tag-based grouping** — assign string tags for batch pause, resume, and removal
- **Dual time scale** — each timer uses real time (`Unscaled`) or engine time scale-affected (`Scaled`)
- **Thread-safe** — lock-based update loop with out-of-lock callback invocation
- **Object pooling** — `TimerItem` instances are pooled to minimize GC pressure
- **Async/await** — `WaitForSecondsAsync`, `WaitForNextFrameAsync`, `WaitForFramesAsync` with `CancellationToken`
- **Query API** — inspect remaining time, elapsed time, and repeat count
- **OnComplete callback** — fires when a timer finishes naturally or is removed

## Quick Start

### Installation

Copy the package into your Godot project's `addons/` directory:

```
addons/com.gameframex.godot.timer/
```

The package is also published to the GameFrameX npm registry, so package-management tooling can consume it directly:

```
https://npm.cnb.cool/GameFrameX/npm/-/packages/
```

> Requires the [GameFrameX core package](https://github.com/GameFrameX/com.gameframex.godot) (`com.gameframex.godot`).

### Usage

Enable the plugin in **Project → Project Settings → Plugins**, then add a `TimerComponent` to a node and call the manager APIs:

```csharp
// Repeating timer: every 1s, 5 times
int id = timerComponent.Add(1f, 5, param => GD.Print($"tick: {param}"), "hello");

// One-shot timer
timerComponent.AddOnce(3f, param => GD.Print("done"));

// Pause / resume, individually or by tag
timerComponent.Pause(id);
timerComponent.ResumeByTag("hello");

// Async helpers
await timerComponent.WaitForSecondsAsync(2f);
```

## License

Apache-2.0 — see [LICENSE.md](LICENSE.md).
