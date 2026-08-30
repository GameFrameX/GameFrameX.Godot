<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="Game Frame X Logo" width="160" />

# Game Frame X（Godot 核心包）

[![License](https://img.shields.io/github/license/GameFrameX/com.gameframex.godot)](https://github.com/GameFrameX/com.gameframex.godot/blob/main/LICENSE.md)
[![Version](https://img.shields.io/github/v/release/GameFrameX/com.gameframex.godot)](https://github.com/GameFrameX/com.gameframex.godot/releases)
[![Godot Version](https://img.shields.io/badge/Godot-4.7-blue?logo=godotengine)](https://godotengine.org/)
[![Documentation](https://img.shields.io/badge/Documentation-docs-blue)](https://gameframex.doc.alianblank.com)

一站式独立游戏开发解决方案 · 为独立开发者的梦想赋能

<br />

[文档](https://gameframex.doc.alianblank.com) · [快速开始](#安装) · QQ 交流群：467608841 / 233840761

<br />

[English](README.md) | **简体中文**

</div>

## 简介

GameFrameX Godot 核心包 — GameFrameX 框架在 Godot 上的运行时与编辑器基础。

- **模块化架构** — 基于组件系统的可扩展框架设计
- **对象池** — 对象复用与内存管理
- **引用池** — 引用类型实例管理，减少 GC 压力
- **Helper 库** — 文件、网络、数学等常用助手
- **扩展方法** — Godot 类型扩展与便捷操作
- **实用工具类** — 加密解密、压缩解压、哈希计算等
- **编辑器工具** — 构建与资产工作流工具

### 核心模块

| 模块 | 描述 |
|------|------|
| **Base** | 框架核心：组件、事件、生命周期 |
| **ObjectPool** | 对象复用、内存优化 |
| **ReferencePool** | 引用类型管理、GC 优化 |
| **Helper** | 文件 / 网络 / 数学助手 |
| **Extension** | Godot 类型扩展 |
| **Utility** | 加密、压缩、哈希 |

### 编辑器工具

| 工具 | 描述 |
|------|------|
| **BuildHotfix** | 热更新程序集构建工具 |
| **BuildProduct** | 产品构建助手 |
| **PackageManager** | 包管理器窗口 |
| **Cropping** | 图片裁剪工具 |
| **Inspector** | 自定义检视面板 |

## 快速开始

### 安装

将包复制到 Godot 项目的 `addons/` 目录：

```
addons/com.gameframex.godot/
```

包同时发布在 GameFrameX npm registry，可被包管理工具直接消费：

```
https://npm.cnb.cool/GameFrameX/npm/-/packages/
```

### 使用

```csharp
using Godot;
using GameFrameX.Runtime;

public partial class GameManager : Node
{
    public override void _Ready()
    {
        // 通过框架入口获取组件
        var objectPool = GameEntry.GetComponent<ObjectPoolComponent>();
        var referencePool = GameEntry.GetComponent<ReferencePoolComponent>();
    }
}
```

## 许可证

Apache-2.0 — 详见 [LICENSE.md](LICENSE.md)。
