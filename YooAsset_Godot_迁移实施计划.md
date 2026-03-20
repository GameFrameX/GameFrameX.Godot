# YooAsset 设计到 Godot 的迁移实施计划（递进式）

## 1. 文档目标

本计划基于 YooAsset 源码结构，设计一个 Godot 4.5（C#）可落地的资源系统转换方案。  
执行方式采用“**一次只做一件事**”的递进模式，每个阶段完成后必须通过测试门禁，再进入下一阶段。

---

## 2. 参考源码范围（Unity YooAsset）

本计划直接参考以下核心模块的职责划分与流程编排：

- `Runtime/YooAssets.cs`
- `Runtime/InitializeParameters.cs`
- `Runtime/OperationSystem/OperationSystem.cs`
- `Runtime/ResourcePackage/ResourcePackage.cs`
- `Runtime/ResourcePackage/Operation/*.cs`
- `Runtime/ResourcePackage/PlayMode/*.cs`
- `Runtime/FileSystem/Interface/IFileSystem.cs`
- `Runtime/FileSystem/DefaultCacheFileSystem/DefaultCacheFileSystem.cs`

---

## 3. 设计映射总览

### 3.1 核心对象映射

| YooAsset 概念 | Godot 转换目标 |
|---|---|
| YooAssets（全局入口） | GfAssetSystem（全局入口 + Tick） |
| ResourcePackage | GfResourcePackage（按包隔离） |
| EPlayMode + InitializeParameters | ERunMode + PackageInitOptions |
| IFileSystem（多文件系统组合） | IStorageBackend（Builtin/Cache/Web/Patch） |
| PackageManifest | PackageManifestModel（JSON） |
| OperationSystem（时间片轮询） | OperationScheduler（_Process 时间片驱动） |
| DownloaderOperation | DownloadBatchOperation（并发、重试、暂停） |
| AssetHandle / SceneHandle / RawFileHandle | ResourceHandle / SceneHandle / RawHandle |

### 3.2 Godot 侧关键差异

- Bundle 机制改为 `PCK/ZIP + 资源路径`。
- 资源加载入口改为 `ResourceLoader` 与场景实例化。
- 挂载与覆盖策略通过 `ProjectSettings.load_resource_pack(...)` 实现。
- 运行时轮询由 Godot 节点 `_Process(double delta)` 驱动。

---

## 4. 目标架构

### 4.1 目录规划

建议新增模块目录（仅 Runtime）：

- `com.gameframex.godot.asset/Runtime/Core`
- `com.gameframex.godot.asset/Runtime/Operation`
- `com.gameframex.godot.asset/Runtime/Package`
- `com.gameframex.godot.asset/Runtime/Manifest`
- `com.gameframex.godot.asset/Runtime/Storage`
- `com.gameframex.godot.asset/Runtime/Download`
- `com.gameframex.godot.asset/Runtime/Provider`
- `com.gameframex.godot.asset/Tests/Unit`
- `com.gameframex.godot.asset/Tests/Integration`

### 4.2 分层职责

- **Core 层**：系统入口、包注册、生命周期、全局配置。
- **Operation 层**：统一异步状态机、时间片调度、优先级队列。
- **Package 层**：每个资源包的初始化、版本、清单、更新流程。
- **Storage 层**：本地内置、缓存目录、远端下载、挂载抽象。
- **Manifest 层**：清单模型、校验、依赖解析、增量比较。
- **Provider 层**：资源/场景/原始文件加载与句柄管理。

---

## 5. 递进实施路线（一次一件事）

> 规则：任一阶段未通过“实现 + 单测 + 构建”三道门禁，不允许进入下一阶段。

### 阶段 M0：建立最小骨架与测试跑通

**只做这一件事**
- 建立模块骨架、命名空间、最小入口类与空调度器。

**产出**
- `GfAssetSystem`、`OperationScheduler` 空实现。
- 测试工程可执行。

**测试门禁**
- Smoke Test：初始化入口可调用，调度器可执行一帧。

---

### 阶段 M1：实现 Operation 基础设施

**只做这一件事**
- 对齐 YooAsset 的 `AsyncOperationBase + OperationSystem` 思路，实现统一操作状态机。

**产出**
- `AsyncOperationBase`、`OperationStatus`、`OperationScheduler`。
- 时间片限制与优先级处理。

**测试门禁**
- 状态流转测试（None → Running → Succeed/Failed）。
- 时间片截断测试。
- 优先级执行顺序测试。

---

### 阶段 M2：实现包注册与生命周期

**只做这一件事**
- 实现 `GfAssetSystem` 与 `GfResourcePackage` 的创建、获取、移除、销毁流程。

**产出**
- 全局包注册表、重复包名检测、销毁前置校验。

**测试门禁**
- 创建重复包报错。
- 未销毁不可移除。
- 销毁后可再次创建同名包。

---

### 阶段 M3：实现运行模式与初始化参数

**只做这一件事**
- 对齐 `EPlayMode + InitializeParameters`，实现 `ERunMode + PackageInitOptions`。

**产出**
- EditorSimulate/Offline/Host/Web 对应配置对象（Godot 侧可先保留 Host/Offline）。
- 参数合法性校验器。

**测试门禁**
- 缺失关键参数失败。
- 模式与平台不兼容失败。
- 合法参数初始化通过。

---

### 阶段 M4：实现 Storage 抽象与默认后端

**只做这一件事**
- 对齐 `IFileSystem` 思路，落地 `IStorageBackend` 与 `Builtin + Cache` 两个后端。

**产出**
- 后端统一接口：版本读取、清单加载、文件存在性、下载需求判断。
- 默认路径规则（`res://` + `user://`）。

**测试门禁**
- Builtin 与 Cache 的归属判断。
- 缓存命中/未命中判断。
- 文件路径映射正确性。

---

### 阶段 M5：实现 Manifest 模型与校验

**只做这一件事**
- 实现清单模型、反序列化、版本字段和依赖字段校验。

**产出**
- `PackageManifestModel`、`ManifestValidator`、`ManifestResolver`。

**测试门禁**
- 合法清单解析通过。
- 缺字段、错字段、循环依赖识别失败。
- 版本号匹配规则通过。

---

### 阶段 M6：实现版本请求流程

**只做这一件事**
- 对齐 `RequestPackageVersionOperation`，实现本地版本读取和远端版本查询。

**产出**
- `RequestLocalVersionOperation`、`RequestRemoteVersionOperation`。
- 超时、重试策略（最小可用）。

**测试门禁**
- 本地版本读取成功/缺失分支。
- 远端成功、超时、HTTP 异常分支。

---

### 阶段 M7：实现清单更新流程

**只做这一件事**
- 对齐 `UpdatePackageManifestOperation`，实现“检查参数→比较激活清单→加载新清单→激活并持久化版本”。

**产出**
- `UpdateManifestOperation` 与版本持久化器。

**测试门禁**
- 相同版本短路成功。
- 新版本替换成功。
- 清单损坏回退与错误上报。

---

### 阶段 M8：实现下载批处理

**只做这一件事**
- 对齐 `DownloaderOperation`，实现并发下载、失败重试、暂停恢复、进度回调。

**产出**
- `DownloadBatchOperation`、`DownloadItemState`、回调事件模型。

**测试门禁**
- 并发上限生效。
- 失败重试次数生效。
- 暂停恢复后进度连续。

---

### 阶段 M9：实现资源挂载与加载 Provider

**只做这一件事**
- 使用 Godot 资源系统实现 `PCK` 挂载和资源加载入口。

**产出**
- `PackMountService`、`ResourceProvider`、`SceneProvider`。

**测试门禁**
- 挂载成功/失败分支。
- 资源加载成功/路径无效分支。
- 场景异步加载与取消分支。

---

### 阶段 M10：实现句柄与引用管理

**只做这一件事**
- 对齐 Handle 思路，实现句柄生命周期、引用计数、释放策略。

**产出**
- `HandleBase`、`ResourceHandle`、`SceneHandle`、`RawHandle`。

**测试门禁**
- 引用计数增减正确。
- 重复释放安全。
- 无引用对象可被回收。

---

### 阶段 M11：打通端到端更新管线

**只做这一件事**
- 连接初始化、版本、清单、下载、挂载、加载主链路。

**产出**
- `PatchPipelineRunner`（最小可用流程）。

**测试门禁**
- 端到端成功路径测试通过。
- 网络中断恢复后可继续。
- 清单失败时回滚到上个可用版本。

---

### 阶段 M12：验收与性能基线

**只做这一件事**
- 进行稳定性与性能验收，形成上线门槛。

**产出**
- 关键指标基线：冷启动、首次更新、资源加载耗时、失败恢复耗时。

**测试门禁**
- 集成测试全绿。
- 构建与类型检查全绿。

---

## 6. 每阶段固定执行模板

每次迭代必须严格按以下顺序执行：

1. 明确“本轮只做一件事”目标。
2. 先写该目标的最小测试集。
3. 实现代码，直到该目标测试全部通过。
4. 运行构建、静态检查、测试。
5. 更新任务状态与阶段记录。

---

## 7. 任务列表更新规范

每完成一件事，任务列表立即更新，状态仅允许：

- `pending`
- `in_progress`
- `completed`

约束：

- 同一时刻只能有一个任务是 `in_progress`。
- 未通过测试门禁的任务不得标记 `completed`。
- 新增任务必须说明所属阶段（M0~M12）。

---

## 8. 测试策略

### 8.1 单元测试优先级

1. Operation 状态机与调度器
2. Manifest 解析与校验
3. 版本请求与错误处理
4. 下载器并发与重试
5. 句柄引用计数

### 8.2 集成测试优先级

1. 本地包初始化
2. 远端版本更新 + 清单更新
3. 下载后挂载并加载资源
4. 异常恢复与回滚

---

## 9. DoD（完成定义）

满足以下条件才视为迁移计划执行完成：

- M0~M12 全部完成并有阶段记录。
- 所有阶段门禁测试通过。
- 端到端更新链路可稳定运行。
- 失败场景可恢复且不破坏已有可用资源。
- 构建、静态检查、测试全部通过。

---

## 10. 第一轮执行指令

下一轮从 **M0** 开始，只做：

- 建立 `com.gameframex.godot.asset` 运行时骨架
- 建立最小测试工程
- 运行第一条 Smoke Test

通过门禁后，再进入 M1。

---

## 11. 阶段交付清单（按里程碑验收）

### M0~M3（基础层）

- M0：最小系统骨架与测试可执行环境
- M1：统一异步操作状态机与调度器
- M2：资源包注册表与生命周期管理
- M3：运行模式配置与初始化参数校验

### M4~M7（数据层）

- M4：Storage 抽象与 Builtin/Cache 默认后端
- M5：Manifest 模型、校验器、依赖解析器
- M6：本地/远端版本请求操作链路
- M7：Manifest 更新、激活、持久化与回退

### M8~M12（业务闭环层）

- M8：下载批处理（并发、重试、暂停恢复）
- M9：PCK 挂载与资源/场景加载 Provider
- M10：句柄生命周期与引用计数回收
- M11：端到端补丁管线贯通
- M12：稳定性验收与性能基线固化

---

## 12. 单元测试用例矩阵（按阶段递增）

| 阶段 | 用例编号 | 用例描述 | 通过标准 |
|---|---|---|---|
| M0 | UT-M0-01 | 入口初始化不抛异常 | 单测通过且可重复执行 |
| M0 | UT-M0-02 | 调度器单帧 Tick 可运行 | 单帧后状态更新符合预期 |
| M1 | UT-M1-01 | 操作状态流转完整 | None→Running→Succeed/Failed 正确 |
| M1 | UT-M1-02 | 时间片限制生效 | 单帧执行量不超过阈值 |
| M2 | UT-M2-01 | 重复包名创建失败 | 返回失败且错误信息正确 |
| M3 | UT-M3-01 | 参数缺失拦截 | 缺失关键字段时初始化失败 |
| M4 | UT-M4-01 | 缓存命中判断正确 | 命中与未命中路径均正确 |
| M5 | UT-M5-01 | 清单依赖闭包正确 | 依赖集合与预期一致 |
| M5 | UT-M5-02 | 循环依赖识别 | 返回失败且给出错误原因 |
| M6 | UT-M6-01 | 远端超时重试生效 | 重试次数与间隔符合配置 |
| M7 | UT-M7-01 | 清单损坏触发回退 | 激活清单保持上个可用版本 |
| M8 | UT-M8-01 | 并发下载上限控制 | 活跃任务数不超过限制 |
| M9 | UT-M9-01 | 挂载失败错误透传 | 失败码和上下文可追踪 |
| M10 | UT-M10-01 | 引用计数与释放一致 | 引用归零后可回收 |
| M11 | IT-M11-01 | 端到端补丁成功流程 | 初始化到加载全链路通过 |
| M12 | IT-M12-01 | 回归集全绿 | 单测与集成测试全部通过 |

---

## 13. 执行节奏与协作规则

- 每轮只选择一个阶段中的一个目标，不跨阶段并行开发。
- 每轮先补测试，再实现最小代码，再做重构。
- 每轮完成后固定执行：构建、测试、记录结论。
- 任何失败都先修复当前阶段，不带病进入下一阶段。
- 进入下一轮前，必须明确“本轮唯一目标”和“对应门禁用例”。
