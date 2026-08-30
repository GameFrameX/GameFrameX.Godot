<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="Game Frame X Logo" width="160" />

# Game Frame X Entry（Godot）

[![License](https://img.shields.io/github/license/GameFrameX/com.gameframex.godot.entry)](https://github.com/GameFrameX/com.gameframex.godot.entry/blob/main/LICENSE.md)
[![Version](https://img.shields.io/github/v/release/GameFrameX/com.gameframex.godot.entry)](https://github.com/GameFrameX/com.gameframex.godot.entry/releases)
[![Godot Version](https://img.shields.io/badge/Godot-4.7-blue?logo=godotengine)](https://godotengine.org/)
[![Documentation](https://img.shields.io/badge/Documentation-docs-blue)](https://gameframex.doc.alianblank.com)

一站式独立游戏开发解决方案 · 为独立开发者的梦想赋能

<br />

[文档](https://gameframex.doc.alianblank.com) · [快速开始](#安装) · QQ 交流群：467608841 / 233840761

<br />

[English](README.md) | **简体中文**

</div>

## 简介

GameFrameX Entry — 应用入口与门面层功能包。提供静态 GameApp 类，聚合所有框架子系统的延迟加载访问器（Procedure、Scene、Network、FSM、Entity、Download、Timer、UI、Audio、Analytics 等），通过条件编译确保只编译已使用的子系统。文档：https://gameframex.doc.alianblank.com

## 快速开始

### 安装

将包复制到 Godot 项目的 `addons/` 目录：

```
addons/com.gameframex.godot.entry/
```

包同时发布在 GameFrameX npm registry，可被包管理工具直接消费：

```
https://npm.cnb.cool/GameFrameX/npm/-/packages/
```

### 依赖

- [`com.gameframex.godot`](https://github.com/GameFrameX/com.gameframex.godot)
- [`com.gameframex.godot.asset`](https://github.com/GameFrameX/com.gameframex.godot.asset)
- [`com.gameframex.godot.config`](https://github.com/GameFrameX/com.gameframex.godot.config)
- [`com.gameframex.godot.download`](https://github.com/GameFrameX/com.gameframex.godot.download)
- [`com.gameframex.godot.entity`](https://github.com/GameFrameX/com.gameframex.godot.entity)
- [`com.gameframex.godot.event`](https://github.com/GameFrameX/com.gameframex.godot.event)
- [`com.gameframex.godot.fsm`](https://github.com/GameFrameX/com.gameframex.godot.fsm)
- [`com.gameframex.godot.globalconfig`](https://github.com/GameFrameX/com.gameframex.godot.globalconfig)
- [`com.gameframex.godot.localization`](https://github.com/GameFrameX/com.gameframex.godot.localization)
- [`com.gameframex.godot.network`](https://github.com/GameFrameX/com.gameframex.godot.network)
- [`com.gameframex.godot.procedure`](https://github.com/GameFrameX/com.gameframex.godot.procedure)
- [`com.gameframex.godot.scene`](https://github.com/GameFrameX/com.gameframex.godot.scene)
- [`com.gameframex.godot.setting`](https://github.com/GameFrameX/com.gameframex.godot.setting)
- [`com.gameframex.godot.sound`](https://github.com/GameFrameX/com.gameframex.godot.sound)
- [`com.gameframex.godot.timer`](https://github.com/GameFrameX/com.gameframex.godot.timer)
- [`com.gameframex.godot.ui`](https://github.com/GameFrameX/com.gameframex.godot.ui)
- [`com.gameframex.godot.ui.fairygui`](https://github.com/GameFrameX/com.gameframex.godot.ui.fairygui)
- [`com.gameframex.godot.web`](https://github.com/GameFrameX/com.gameframex.godot.web)
- [`com.gameframex.godot.web.protobuff`](https://github.com/GameFrameX/com.gameframex.godot.web.protobuff)

## 许可证

Apache-2.0 — 详见 [LICENSE.md](LICENSE.md)。
