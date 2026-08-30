<div align="center">

<img src="https://download.alianblank.com/gameframex/gameframex_logo_320.png" alt="Game Frame X Logo" width="160" />

# GameFrameX Timer（Godot）

[![License](https://img.shields.io/github/license/GameFrameX/com.gameframex.godot.timer)](https://github.com/GameFrameX/com.gameframex.godot.timer/blob/main/LICENSE.md)
[![Version](https://img.shields.io/github/v/release/GameFrameX/com.gameframex.godot.timer)](https://github.com/GameFrameX/com.gameframex.godot.timer/releases)
[![Godot Version](https://img.shields.io/badge/Godot-4.7-blue?logo=godotengine)](https://godotengine.org/)
[![Documentation](https://img.shields.io/badge/Documentation-docs-blue)](https://gameframex.doc.alianblank.com)

一站式独立游戏开发解决方案 · 为独立开发者的梦想赋能

<br />

[文档](https://gameframex.doc.alianblank.com) · [快速开始](#安装) · QQ 交流群：467608841 / 233840761

<br />

[English](README.md) | **简体中文**

</div>

## 功能特性

- **三种计时模式** — 重复（`Add`）、一次性（`AddOnce`）、每帧更新（`AddUpdate`）
- **暂停 / 恢复** — 按定时器 ID 单独操作，或按标签批量操作
- **标签分组** — 为定时器分配字符串标签，支持批量暂停、恢复与移除
- **双时间缩放** — 每个定时器可选择真实时间（`Unscaled`）或受引擎时间缩放影响（`Scaled`）
- **线程安全** — 基于锁的更新循环，回调在锁外执行
- **对象池** — `TimerItem` 实例池化，减少 GC 压力
- **异步支持** — `WaitForSecondsAsync`、`WaitForNextFrameAsync`、`WaitForFramesAsync`，支持 `CancellationToken`
- **查询 API** — 查询剩余时间、已累积时间与剩余重复次数
- **完成回调** — 定时器自然结束或被移除时触发 `OnComplete`

## 快速开始

### 安装

将包复制到 Godot 项目的 `addons/` 目录：

```
addons/com.gameframex.godot.timer/
```

包同时发布在 GameFrameX npm registry，可被包管理工具直接消费：

```
https://npm.cnb.cool/GameFrameX/npm/-/packages/
```

> 依赖 [GameFrameX 核心包](https://github.com/GameFrameX/com.gameframex.godot)（`com.gameframex.godot`）。

### 使用

在 **项目 → 项目设置 → 插件** 中启用插件，为节点添加 `TimerComponent` 后即可调用管理器 API：

```csharp
// 重复定时器：每 1 秒一次，共 5 次
int id = timerComponent.Add(1f, 5, param => GD.Print($"tick: {param}"), "hello");

// 一次性定时器
timerComponent.AddOnce(3f, param => GD.Print("done"));

// 暂停 / 恢复，单个或按标签
timerComponent.Pause(id);
timerComponent.ResumeByTag("hello");

// 异步辅助
await timerComponent.WaitForSecondsAsync(2f);
```

## 许可证

Apache-2.0 — 详见 [LICENSE.md](LICENSE.md)。
