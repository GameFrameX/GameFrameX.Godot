<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="Game Frame X Logo" width="160" />

# Game Frame X Procedure（Godot）

[![License](https://img.shields.io/github/license/GameFrameX/com.gameframex.godot.procedure)](https://github.com/GameFrameX/com.gameframex.godot.procedure/blob/main/LICENSE.md)
[![Version](https://img.shields.io/github/v/release/GameFrameX/com.gameframex.godot.procedure)](https://github.com/GameFrameX/com.gameframex.godot.procedure/releases)
[![Godot Version](https://img.shields.io/badge/Godot-4.7-blue?logo=godotengine)](https://godotengine.org/)
[![Documentation](https://img.shields.io/badge/Documentation-docs-blue)](https://gameframex.doc.alianblank.com)

一站式独立游戏开发解决方案 · 为独立开发者的梦想赋能

<br />

[文档](https://gameframex.doc.alianblank.com) · [快速开始](#安装) · QQ 交流群：467608841 / 233840761

<br />

[English](README.md) | **简体中文**

</div>

## 简介

GameFrameX Procedure — Godot 基于 FSM 的游戏流程管理功能包。通过可切换的过程状态驱动游戏生命周期阶段（闪屏、预加载、登录、主菜单等），提供完整生命周期回调（OnInit、OnEnter、OnUpdate、OnFixedUpdate、OnLeave、OnDestroy），并支持通过 Inspector 配置入口过程。文档：https://gameframex.doc.alianblank.com

## 快速开始

### 安装

将包复制到 Godot 项目的 `addons/` 目录：

```
addons/com.gameframex.godot.procedure/
```

包同时发布在 GameFrameX npm registry，可被包管理工具直接消费：

```
https://npm.cnb.cool/GameFrameX/npm/-/packages/
```

### 依赖

- [`com.gameframex.godot`](https://github.com/GameFrameX/com.gameframex.godot)
- [`com.gameframex.godot.fsm`](https://github.com/GameFrameX/com.gameframex.godot.fsm)

## 许可证

Apache-2.0 — 详见 [LICENSE.md](LICENSE.md)。
