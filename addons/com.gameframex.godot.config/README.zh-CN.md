<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="Game Frame X Logo" width="160" />

# Game Frame X Config（Godot）

[![License](https://img.shields.io/github/license/GameFrameX/com.gameframex.godot.config)](https://github.com/GameFrameX/com.gameframex.godot.config/blob/main/LICENSE.md)
[![Version](https://img.shields.io/github/v/release/GameFrameX/com.gameframex.godot.config)](https://github.com/GameFrameX/com.gameframex.godot.config/releases)
[![Godot Version](https://img.shields.io/badge/Godot-4.7-blue?logo=godotengine)](https://godotengine.org/)
[![Documentation](https://img.shields.io/badge/Documentation-docs-blue)](https://gameframex.doc.alianblank.com)

一站式独立游戏开发解决方案 · 为独立开发者的梦想赋能

<br />

[文档](https://gameframex.doc.alianblank.com) · [快速开始](#安装) · QQ 交流群：467608841 / 233840761

<br />

[English](README.md) | **简体中文**

</div>

## 简介

GameFrameX Config — Godot 配置与数据表管理功能包。提供命名配置管理与泛型数据表（IDataTable<T>），支持按 int/long/string ID 查找、LINQ 风格查询（Find、FindList、ForEach、Max、Min、Sum），以及异步数据表加载，实现非阻塞初始化。文档：https://gameframex.doc.alianblank.com

## 快速开始

### 安装

将包复制到 Godot 项目的 `addons/` 目录：

```
addons/com.gameframex.godot.config/
```

包同时发布在 GameFrameX npm registry，可被包管理工具直接消费：

```
https://npm.cnb.cool/GameFrameX/npm/-/packages/
```

### 依赖

- [`com.gameframex.godot`](https://github.com/GameFrameX/com.gameframex.godot)
- [`com.gameframex.godot.asset`](https://github.com/GameFrameX/com.gameframex.godot.asset)
- [`com.gameframex.godot.event`](https://github.com/GameFrameX/com.gameframex.godot.event)

## 许可证

Apache-2.0 — 详见 [LICENSE.md](LICENSE.md)。
