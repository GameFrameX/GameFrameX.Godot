<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="Game Frame X Logo" width="160" />

# Game Frame X Startup（Godot）

[![License](https://img.shields.io/github/license/GameFrameX/com.gameframex.godot.startup)](https://github.com/GameFrameX/com.gameframex.godot.startup/blob/main/LICENSE.md)
[![Version](https://img.shields.io/github/v/release/GameFrameX/com.gameframex.godot.startup)](https://github.com/GameFrameX/com.gameframex.godot.startup/releases)
[![Godot Version](https://img.shields.io/badge/Godot-4.7-blue?logo=godotengine)](https://godotengine.org/)
[![Documentation](https://img.shields.io/badge/Documentation-docs-blue)](https://gameframex.doc.alianblank.com)

一站式独立游戏开发解决方案 · 为独立开发者的梦想赋能

<br />

[文档](https://gameframex.doc.alianblank.com) · [快速开始](#安装) · QQ 交流群：467608841 / 233840761

<br />

[English](README.md) | **简体中文**

</div>

## 简介

GameFrameX Startup — Godot 通用启动流程脚手架。封装从游戏启动到热更加载的完整流程：启动 UI → 拉全局信息（URL 主备 failover）→ 拉版本信息 → 拉资源包版本 → YooAsset 补丁 → 启动热更。所有项目可变项（URL 列表、HTTP 公共参数、热更入口、UI 实现）通过 StartupOptions ScriptableObject 配置资产 + 两个接口（IStartupUIHandler / IHotfixLauncher）注入。文档：https://gameframex.doc.alianblank.com

## 快速开始

### 安装

将包复制到 Godot 项目的 `addons/` 目录：

```
addons/com.gameframex.godot.startup/
```

包同时发布在 GameFrameX npm registry，可被包管理工具直接消费：

```
https://npm.cnb.cool/GameFrameX/npm/-/packages/
```

### 依赖

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

## 许可证

Apache-2.0 — 详见 [LICENSE.md](LICENSE.md)。
