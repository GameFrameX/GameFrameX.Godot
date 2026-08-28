# Unity → Godot 全包对比与迁移实施计划

> 生成日期：2026-08-28
> 基准：`Unity/Packages` 下全部 38 个 `com.gameframex.unity*` 包
> 目标：`Godot/addons` 下全部 23 个 `com.gameframex.godot*` 包
> 方法：逐文件清单对比 + 代表性 API 级 diff + 引擎特有接口盘点（6 组并行深度扫描）

---

## 一、执行摘要

- **结构完成度高**：16 组包文件级基本对齐；核心包纯 C# 层（Utility 哈希/加密/校验、对象池、任务池、事件池）与 Unity 逐行一致，`UnityEngine` 残留 0 处。
- **三大结构性缺口**：
  1. `RuntimeHost/` 自动装配体系（22 文件）整体缺失且无替代机制；
  2. `assetsystem` 结构 ~98% 但 Bundled/Database 资源加载是 **stub 占位实现**，真实可用路径只有 RawFile + ResourceLoader + PCK 挂载；
  3. `asset` 组件封装层 0%（空壳包）。
- **完全缺失包 15 个**：startup（启动流程）、sound、scene、asset 组件层、imagecache、systeminfo、operationclipboard、readassets、luban、coroutine、mono + 第三方 UniTask/DOTween/LitJSON/SimpleJSON/UnityWebSocket（后三者已有替代方案，见决策清单）。
- **存量高危缺陷 3 处**（Phase 0 必修）：entity 后台线程调用 `ResourceLoader.Load`（崩溃风险）、network 序列化注册链断裂（只能 JSON）、`NetworkChannelBase` 线程安全降级（回归）。
- **测试基建为零**：无任何测试框架引用，`tests/` 只有空目录；文档提及的 Asset UnitTests 已不存在于磁盘。

---

## 二、38 包总体对账表

### A. 已基本对齐（16 组，含小缺口）

| # | Unity 包 | Godot 包 | 完成度 | 主要缺口 |
|---|---|---|---|---|
| 1 | com.gameframex.unity (199) | com.gameframex.godot (152) | 纯 C# 层 ~100% | RuntimeHost 22 文件、Time 时区 15 文件、29 个平台属性、Extension 5 文件、CameraHelper 空 stub、GameVersion→Version 断裂性重命名 |
| 2 | config (9) | config (10) | ~95% | InvalidateCache 缓存机制删除、DefaultConfigHelper 占位文件、ConfigDefineSymbols 无对应 |
| 3 | entity (29) | entity (29) | 文件 1:1，实现高危 | `EntityManager.cs:344` 用 `Task.Run(() => ResourceLoader.Load)` 后台线程调 Godot API；`SetResourceManager` 注入点缺失；ReleaseEntity 不释放资源句柄；Entity/EntityLogic 节点层级语义差异（attach 表现可能不一致） |
| 4 | event (6) | event (6) | ~99% | 缺 `CheckUnsubscribe(id, handler)` |
| 5 | fsm (8) | fsm (8) | ~99% | 仅容量/可见性微差 |
| 6 | getchannel (1) | getchannel (2) | ~80% | 缺 Android meta-data 渠道读取分支（JNI） |
| 7 | globalconfig (9) | globalconfig (9) | ~98% | 缺 SetGlobalConfig 空参防御、aotCodeLists |
| 8 | google.protobuf (99) | google.protobuf (97) | 纯移植 0 差异 | 缺 `ProtobufMessageSerializer.cs` + `ProtobufSerializerInitializer.cs` 两个 network 粘合文件（⇒ 序列化链断裂） |
| 9 | download (23) | download (21) | ~95% | 缺 `m_CustomDownloadAgentHelper` 注入、IDisposable、证书自定义处理；WWW helper 未迁（合理） |
| 10 | network (56) | network (53) | ~95% | 缺 `IMessageSerializer`/`MessageSerializerRegistry`（硬编码 JSON）；`NetworkChannelBase` 线程安全降级（ConcurrentQueue→LinkedList、去 volatile、锁复用事件委托、遍历中 Close）；RpcState 缺 Reset；HeartBeatInterval setter 缺非负校验；事件参数不回收 |
| 11 | web (14) | web (13) | ~93% | 缺 `DefaultBypassCertificate`（TLS 绕过能力归零）、WebLog 宏切换 Editor 工具 |
| 12 | web.protobuff (6) | web.protobuff (6) | ~100% | 仅 HTTP 层换 HttpClient |
| 13 | ui (43) | ui (43) | ~95% | 缺 `UIDesignResolutionComponent`、`OptionUIAllowMultiInstanceAttribute`/`UseSingletonOpenMode`、`UIComponent.Get` 6 个已加载查询方法、`IUIManager.SetResourceManager`（资源引用链断裂）、`EnableAutoReleaseUIForm` |
| 14 | ui.ugui (12) | ui.gdgui (12) | ~95% | 缺 4 个 Editor 工具（代码生成器/图片替换/Inspector）；无设计分辨率适配 |
| 15 | tuyoogame.yooasset (190) | assetsystem (187) | 结构 ~98%，语义低 | Bundled 资源加载 stub（BundleFile.LoadAsset 返回占位对象）、Database Provider 是 `#if UNITY_EDITOR` 死代码、KuaiShou FS 整目录删、构建端无加密（Encrypted 恒 false）；Editor 118→5 文件（Collector/Reporter/Debugger 原型级，无真机远程调试） |
| 16 | fairygui.unity (167) | fairygui.godot (124) | 渲染层完全重写（Node2D + ArrayMesh） | 缺 Filter 全套（变灰/滤镜上游声明不支持）、TextMeshPro、WebGL 输入、Spine/DragonBones（空分支）、EMRenderSupport/UIPainter/UIPanel、Tree 视图、TypingEffect、全部 Editor 工具 |

### B. 部分迁移（7 组）

| # | Unity 包 | Godot 包 | 现状 |
|---|---|---|---|
| 17 | ui.fairygui (13) | ui.fairygui (6) | **Godot 版是 gdgui 复制体**，FairyGUI 包管线（FairyGUIPackageComponent/FairyGUILoadAsyncResourceHelper/GObjectHelper 等 7 文件）整体缺失；唯一挂接点是 `UIComponent.EnsureFairyGuiDisplayRootAttached` |
| 18 | timer (6) | timer (4) | 早期精简版：缺 id 索引（同 callback 覆盖）、tag、Pause/Resume、GetRemaining 等 ~12 API、Scaled/Unscaled 模式；缺 TimerAsyncExtensions（可平移）、TimerTimeScale |
| 19 | setting (18) | setting (10) | 缺 Storage 抽象 4 文件（ISettingStorageBackend/工厂/PlayerPrefs 后端）+ MiniGame 4 后端；PlayerPrefsSettingHelper 已用 ConfigFile 替代 |
| 20 | localization (14) | localization (12) | 缺 LanguageChange Before/After 两个事件 |
| 21 | procedure (6) | procedure (5) | 缺 `ProcedureRuntimeOverrideProvider`（UseStartupRunner 开关，startup 前置）+ `DestroyProcedures` |
| 22 | entry (36) | entry (16) | 缺 21 分部：5 个因组件缺失（Asset/Sound/Scene/Mono/Coroutine）、2 个组件已存在可直接迁（UI/FairyGUIPackage）、14 个 Unity-only SDK（登录/广告/统计/内购/XLua/LUGUI） |

### C. 完全缺失（15 个）

| # | Unity 包 | 性质 | 评估 |
|---|---|---|---|
| 23 | startup (30) | **核心**：启动补丁状态机（11 状态）+ URL failover + 热更启动 | Failover/结果/Http参数/事件/BM 可直迁（纯 C#）；Patch 五状态需按 AssetSystem 重写；StartupOptions 需 ScriptableObject→Resource；依赖 systeminfo |
| 24 | sound (28) | **核心**：AudioSource 全量映射（组/agent/淡入淡出/3D 绑定） | 需 AudioStreamPlayer 体系重写 |
| 25 | scene (10) | GameFrameX 场景管理层 | assetsystem 已有 SceneHandle 原语（LoadSceneAsync/ActivateScene/UnloadAsync），只缺管理层，迁移成本低 |
| 26 | asset (30) | AssetComponent/IAssetManager/16 组类型化扩展/补丁事件 | Godot asset 包是纯引用壳（0 .cs） |
| 27 | imagecache (5) | 图片缓存（LoadImageAsync/磁盘限额/过期） | 依赖 download（已有），Texture2D→ImageTexture，迁移成本低 |
| 28 | systeminfo (1) | 设备标识（OAID/IDFA/IMEI） | Godot 等价 `OS.GetUniqueID()`（部分平台）；Android 厂商链需原生桥 |
| 29 | operationclipboard (1) | 剪贴板 | `DisplayServer.ClipboardGet/Set` 已有仓库先例，可 1:1 迁移 |
| 30 | readassets (7) | APK 内直读/Zip 流 | Godot res:// 覆盖大部分场景；FileHelper 有死代码钩子 |
| 31 | luban (5) | 配置表运行时（ByteBuf/BeanBase/ITypeId） | 0%；Godot config 未依赖它，需决策是否迁 |
| 32 | coroutine (2) | 协程跟踪封装 | Godot async/await 等价，建议不迁移 |
| 33 | mono (6) | Update/Focus/Pause 分发 | _Process 已替代 Update；Focus/Pause 全局事件缺失（network 包有私有实现未广播） |
| 34 | cysharp.unitask (153) | 第三方异步库 | Task/ToSignal 已验证可替代；无零分配原语 |
| 35 | demigiant.dotween (9) | 第三方补间 | Godot Tween 替代；`DoTweenHelper.cs` 是死代码（宏保护） |
| 36 | litjson (15) | 第三方 JSON | 已被 NewtonsoftJsonHelper 替代（API 等价） |
| 37 | json.simplejson (5) | 第三方 JSON | 同上，无引用需求 |
| 38 | psygames.unitywebsocket (14) | 第三方 WS | 已内联为 `WebSocketPeer` 通道（二进制单通道、poll 驱动）；缺文本帧/多实例/独立抽象 |

---

## 三、存量缺陷清单（Phase 0 修复项）

| 级别 | 问题 | 位置 |
|---|---|---|
| P0 | 后台线程调 `ResourceLoader.Load`（Godot 非线程安全，崩溃风险）+ 绕过 assetsystem | `godot.entity/Runtime/Entity/Entity/EntityManager.cs:344` |
| P0 | 网络消息序列化硬编码 JSON，无注册体系；protobuf 粘合文件被删，protobuf-net 只用于 HTTP body | `godot.network/Runtime/Network/Helper/SerializerHelper.cs`；`google.protobuf` 缺 2 文件 |
| P0 | `NetworkChannelBase` 线程安全回归（非线程安全集合、遍历中 Close、锁复用事件委托） | `godot.network/Runtime/Network/Network/NetworkManager.NetworkChannelBase.cs` |
| P1 | `Start()` 死代码（Godot Node 无 Start 生命周期，strict check 永不执行） | `godot/Runtime/ReferencePool/ReferencePoolComponent.cs:68`、`Base/BaseComponent.cs:194` |
| P1 | 文件名与类名不一致 | `godot/Runtime/Variable/VarUnityObject.cs`（类名 VarGodotObject） |
| P1 | `WebComponent` 注册失败被静默忽略（Unity 版会禁用组件） | `godot/Runtime/Base/GameFrameworkComponent.cs` `_Ready()` |
| P2 | 构建产物入库：`google.protobuf/Runtime/obj/Debug/net8.0/*.cs`、fairygui 的 `UIProject/.objs` | 仓库卫生 |
| P2 | 228 个 Unity `.meta` 残留 | `godot/` 全 addon |
| P2 | README 示例仍是 Unity 代码（引用不存在的扩展方法） | `godot/README.md:58` |
| P2 | `GameVersion`→`Version` 断裂性重命名（引用方需改） | `godot/Runtime/Base/Version/` |

---

## 四、引擎特有接口决策清单（需确认，附推荐方案）

> 以下为"无直接等价或存在多种处理方式"的引擎特有接口。**推荐列为我建议的方案**，请逐项确认或指定其他方式。

### D1. 音频体系（sound 包迁移前提）

| UnityEngine API | 推荐方案 |
|---|---|
| `AudioSource`/`AudioClip` | `AudioStreamPlayer`（2D）/`AudioStreamPlayer3D`（绑定 Entity 时）+ `AudioStream`；agent = 节点池 |
| `AudioMixer`/`AudioMixerGroup` | `AudioServer` Bus 体系：启动时按 SoundGroup 建 Bus，`AudioStreamPlayer.Bus` 指向组 Bus；音量走 `AudioServer.SetBusVolumeDb` |
| 淡入淡出（协程） | Godot `Tween`（`CreateTween().TweenMethod`）改写 |

### D2. 平台与设备

| UnityEngine API | 推荐方案 |
|---|---|
| `SystemInfo.deviceUniqueIdentifier` | 桌面/iOS：`OS.GetUniqueID()`；Android：首版用 `OS.GetUniqueID()` 兜底 + 文件缓存（复用 ConfigFile 实践）；OAID/IMEI 厂商链**首版不做**（需 Java 桥，标 ponytail 注明上限） |
| `AndroidJavaClass` JNI（getchannel/systeminfo/clipboard） | Godot 首版只做文件通道（channel.txt）+ `DisplayServer`；Android 原生分支留空 + `// TODO` 标注 |
| `Screen.sleepTimeout`（NeverSleep） | 无直接等价；建议 `OS.LowProcessorUsageMode` 不相关——留原生插件 TODO，不阻塞 |
| `Screen.dpi` | `DisplayServer.ScreenGetDpi()`，赋给 `Utility.Converter.ScreenDpi` |
| `Application.runInBackground` | Godot 桌面默认后台继续跑；Web/移动不适用。建议直接删该配置项 |
| 29 个平台属性（主机/TV/小游戏渠道） | 保留 12 个现有属性；按 Godot `OS.HasFeature` 能力补 `IsAndroid/IsIOS/IsWeb/IsDesktop...` 精简集；小游戏渠道属性（微信/抖音/快手等 20 个）**不迁**，GameFrameX 不支持小游戏平台时无意义 |

### D3. 存储与配置

| UnityEngine API | 推荐方案 |
|---|---|
| `PlayerPrefs` | 已有 `ConfigFile`（user://*.cfg）实践，统一走 `PlayerPrefsSettingHelper`，无需 Unity 版 PlayerPrefs 直通 |
| `ScriptableObject`（StartupOptions） | Godot `Resource` + `[GlobalClass][Export]`（.tres 配置），或 JSON。推荐 Resource（可 Inspector 编辑） |
| `RuntimeInitializeOnLoadMethod` | 改为主场景引导代码显式调用注册（如 `SettingStorageBackend.Register`）；Godot 无等价机制 |

### D4. 异步与补间

| UnityEngine API | 推荐方案 |
|---|---|
| UniTask | 全面 `Task`/`TaskCompletionSource`（entity 已验证）；帧等待统一封装 `await ToSignal(SceneTree, "process_frame")` 为 `TimerAsyncExtensions.WaitForNextFrameAsync` 等价扩展。不移植 UniTask 本体 |
| DOTween | Godot `Node.CreateTween()`/`PropertyTweener`；删除 `DoTweenHelper.cs` 死代码（宏永远不启用） |
| Coroutine/WaitForEndOfFrame | `async/await` + `ToSignal`；`WaitForEndOfFrameFinish` 等价封装放 timer 扩展。coroutine 包不迁移 |

### D5. 资源与场景

| UnityEngine API | 推荐方案 |
|---|---|
| `AssetBundle`（YooAsset Loader） | 保持现有"RawFile + PCK 挂载 + ResourceLoader"路线；Phase 2 把 `BundleFile.LoadAsset` stub 替换为 PCK 内路径 → `ResourceLoader.Load` 真实现 |
| `UnityEditor.AssetDatabase`（EditorSimulate） | 用 `#if TOOLS` + `ResourceLoader`/`DirAccess` 重写 Database Provider（当前是死代码） |
| `SceneManager.LoadSceneMode`/`Camera.main` | 已有 SceneHandle 原语；`CameraHelper` 用 `GetViewport().GetCamera3D()` |
| 证书绕过（CertificateHandler） | `HttpClientHandler.ServerCertificateCustomValidationCallback` 开关（默认关，仅调试用） |
| APK 直读 | res:// 已覆盖；Android 只读路径用 `FileAccess`，readassets 包不迁，FileHelper 死代码钩子删除 |

### D6. UI 特有

| UnityEngine API | 推荐方案 |
|---|---|
| Canvas/CanvasScaler 设计分辨率 | 新建 Godot 版 `UIDesignResolutionComponent`：读 ProjectSettings stretch 配置 + 各组 Control anchors（参考 fairygui `Stage.UpdateContextScale` 思路） |
| FairyGUI Spine/DragonBones/TextMeshPro/Filter/变灰 | 上游声明不支持。**接受现状**：GLoader3D 空分支保留 + 文档声明；若需 Spine 另立任务接 Godot Spine 插件 |
| WebGL 软键盘输入 | `DisplayServer.ShowVirtualKeyboard`/`HideVirtualKeyboard`（fairygui InputTextField 补齐，Web 发布前必须） |
| UGUI Editor 工具（代码生成器） | gdgui 按 Control 体系重做属大工程，Phase 4 按需 |

### D7. 结构性机制

| Unity 机制 | 推荐方案 |
|---|---|
| RuntimeHost 自动装配（22 文件） | **不移植**。Godot 生态用 AutoLoad 单例 + 场景显式挂载；补文档说明手动挂载规范 + 提供启动场景模板。若后续多项目复用痛点明显再评估 |
| Time 时区子系统（15 文件） | 移植（纯 C# 无引擎依赖，成本低）：`SetTimeZone`/`CurrentTimeZone`/`*WithTimeZoneOffset` 全套 |
| Unity 扩展（GameObject/Transform/Vector 5 文件） | 新建 Godot 等价：`Node` 扩展（GetOrAddChild/SetPositionX/Reparent 保持树语义）+ `Vector2/3` 扩展 |
| mono 包 Focus/Pause 全局事件 | 补到核心包：`BaseComponent._Notification` 处理 `NotificationApplicationFocusIn/Out`/`NotificationApplicationPaused` 转发 EventComponent（network 包私有实现抽出） |
| LuBan 配置表运行时 | **需决策**：Godot config 目前不依赖。推荐迁移（5 文件纯 C#，服务端/Unity 共用配置管线时必需）；若 Godot 端配置全走 JSON 直载则不迁 |

---

## 五、迁移路线图

> 每项含验收标准；建议单任务推进（参照 assetsystem 计划的任务状态规范）。

### Phase 0：存量修复（预计 2-3 人日，最高优先）✅ 已完成（2026-08-28，验收：build 0 error / 20 单测全绿）

| 任务 | 验收 |
|---|---|
| 0.1 entity 资源加载改走 assetsystem（主线程 ResourceLoader / AssetSystem.LoadAssetAsync），恢复 `SetResourceManager` 注入与进度/依赖回调 | 单元测试：ShowEntityAsync 走 fake IAssetManager 可注入；引擎测试：headless 场景实例化 .tscn 实体成功 |
| 0.2 network 恢复 `IMessageSerializer`/`MessageSerializerRegistry` + protobuf 粘合（`ProtobufMessageSerializer` + 显式注册替代 RuntimeInitializeOnLoadMethod） | 单元测试：JSON/protobuf 双序列化回环；通道级注册覆盖全局注册 |
| 0.3 NetworkChannelBase 线程安全对齐（ConcurrentQueue/独立锁对象/快照遍历/心跳锁外 Close） | 单元测试：并发 Send/HeartBeat/Close 竞态用例 |
| 0.4 杂项：ReferencePool strict check 移入 `_Ready`；VarUnityObject 改名；GameFrameworkComponent 注册失败处理；obj/UIProject 产物清理 + .gitignore；.meta 清理；README 示例改 Godot | `dotnet build` 0 warning 新增；grep 校验 |

### Phase 1：核心缺失包（预计 3-4 周）

| 顺序 | 任务 | 依赖 | 验收 |
|---|---|---|---|
| 1.1 | **timer 补全**（id 索引/tag/Pause/Resume/TimeScale/Async 扩展，替换精简版） | 无 | 单元测试全 API 矩阵（含同 callback 多定时器） |
| 1.2 | **systeminfo + operationclipboard**（OS.GetUniqueID + DisplayServer.Clipboard，ConfigFile 缓存） | setting | 单元测试缓存回环；引擎测试 clipboard headless 校验 |
| 1.3 | **scene 包**（SceneComponent/IGameSceneManager 复用 assetsystem SceneHandle；Camera 用 Viewport） | assetsystem | 引擎测试：headless 加载/卸载/激活 .tscn 场景全流程 |
| 1.4 | **sound 包**（AudioStreamPlayer 体系 + Bus 组映射 + Tween 淡入淡出 + entity 绑定） | entity、assetsystem | 单元测试管理器状态机；引擎测试：headless 播放/暂停/停止/组音量 |
| 1.5 | **asset 组件层**（AssetComponent + AssetManager 多模式初始化 + typed 扩展 + EPatchStates 6 事件；接 assetsystem） | assetsystem | 单元测试 fake 后端模式下初始化矩阵；引擎测试 EditorSimulate 加载 PackedScene |
| 1.6 | **startup 包**（Failover/结果对象直迁；Patch 五状态按 AssetSystem 重写；StartupOptions→Resource；StartupUIHandler 由业务实现；补 ProcedureComponent 不自动启动开关） | 1.2/1.5、procedure | 引擎测试：headless 跑完 11 状态机（fake Web 响应）到达 HotfixLauncher |
| 1.7 | **entry 补全**（GameApp.Asset/Sound/Scene/UI/FairyGUIPackage 分部 + 各包宏） | 1.3-1.5 | 编译宏开合矩阵验证 |
| 1.8 | 小项补齐：event.CheckUnsubscribe、localization Before/After 事件、setting Storage 抽象（不含小游戏后端）、procedure DestroyProcedures、getchannel 缺口确认 | 各自包 | 对应单元测试 |

### Phase 2：assetsystem 语义补全（预计 2-3 周）

| 任务 | 验收 |
|---|---|
| 2.1 BundledAssetProvider 真实现（BundleFile.LoadAsset → PCK 路径 ResourceLoader；占位资源移除） | 引擎测试：构建产物加载 PackedScene/Texture/AudioStream 三类型 |
| 2.2 Database Provider 重写（`#if TOOLS` + ResourceLoader，替代 UNITY_EDITOR 死代码） | EditorSimulate 模式 headless 加载成功 |
| 2.3 移除 UnityWebRequest stub 家族或改为报错（防"空成功"） | grep 校验 + 全链路回归 |
| 2.4 Collector 规则系统对齐（ECollectorType/Pack/Address 规则） | 构建产物清单与 Unity 版同输入对比 |
| 2.5 Reporter/Debugger 增强（BuildReport 结构对齐；快照调试保留） | 报告字段对齐校验 |

### Phase 3：UI 域完善（预计 2 周，依赖 D6 决策）

| 任务 | 验收 |
|---|---|
| 3.1 ui 主包补全（6 个查询方法、AllowMultiInstance、EnableAutoReleaseUIForm、SetResourceManager 链路恢复） | 单元测试查询矩阵 |
| 3.2 UIDesignResolutionComponent（Godot 版） | 引擎测试：分辨率变化回调 + anchors 布局断言 |
| 3.3 ui.fairygui 真管线（FairyGUIPackageComponent 接 assetsystem UIPackage 加载 + GObjectHelper 映射；决策后定深度） | 引擎测试：UIPackage 创建→GComponent 挂树→关闭释放 |
| 3.4 fairygui Web 输入补齐 | Web 导出构建 + 虚拟键盘冒烟 |

### Phase 4：按需项（不阻塞主线）

- Time 时区子系统迁移（纯 C#，可随时插入）
- Node/Vector 扩展 + mono Focus/Pause 全局事件补齐
- imagecache 包（LoadImageAsync + 磁盘限额）
- LuBan 运行时（待 D7 决策）
- network WebSocket 文本帧/独立 IWebSocket 抽象（有需求再做）

### 明确不做（除非需求变化）

- coroutine 包（async/await 替代）
- readassets 包（res:// 覆盖）
- UniTask/DOTween/LitJSON/SimpleJSON/UnityWebSocket 本体（已有替代）
- entry 的 14 个 Unity-only SDK 分部（XLua/各平台登录/广告/统计/内购）——Godot 侧无 SDK，需要时按平台单独立项
- RuntimeHost 自动装配（AutoLoad + 显式挂载规范替代）
- 小游戏平台（微信/抖音/快手）支持

---

## 六、测试规划

### 6.1 测试基建（Phase 0 同步搭建）

1. **单元测试工程**：`Godot/tests/GameFrameX.UnitTests/`（xUnit + `Microsoft.NET.Test.Sdk`），通过 `ProjectReference` 引用各包 Runtime csproj 或以 `<Compile Include="../addons/**/Runtime/**/*.cs">` 虚拟分组引用（保持主工程 `Compile Remove="tests/**"` 现状）。纯逻辑模块（Utility/Timer/FSM/EventManager/DownloadManager 等）不依赖 Godot 运行时，可全部单测。
2. **引擎测试（Headless）**：`Godot/tests/EngineTests/`——一个专用测试场景 + `Node` 自检脚本（非 GUT，纯 C# `SceneTree` 脚本），命令行入口：
   ```bash
   godot --headless --path . res://tests/EngineTests/run_tests.tscn -- --filter=scene
   ```
   断言失败以非零退出码结束，可挂 CI。适用：需要 ResourceLoader/SceneTree/AudioServer/PackedScene 的行为验证。
3. **CI 门槛**：`dotnet build`（0 error）+ `dotnet test`（单元全绿）+ headless 引擎测试（退出码 0）。

### 6.2 各包测试用例矩阵（核心项）

| 包 | 单元测试 | 引擎测试（headless） |
|---|---|---|
| core | Utility.Hash/Encryption/Verifier 已知向量回环；ReferencePool 严格模式；ObjectPool 容量/过期；EventPool 订阅/退订/CheckUnsubscribe；TaskPool；TimerHelper 时区迁移后 163 方法抽样 30 个对拍 Unity 输出 | BaseComponent 驱动注册模块 Update；AutoLoad 单例挂载 |
| timer | id/tag CRUD、Pause/Resume、Scaled/Unscaled、同 callback 多定时器、WaitForFramesAsync 帧数精确性 | —（逻辑纯） |
| fsm/procedure | 状态迁移链、BlackBoard 注入、不自动启动开关 | ProcedureComponent CallDeferred 启动 |
| event | Fire/Subscribe/CheckUnsubscribe/空事件 | — |
| network | 序列化注册表（JSON/protobuf 回环、通道级覆盖）；心跳超时判定；RpcState TryAdd/Reset；NetworkChannelBase 并发竞态（Task 并行 Send×Close） | WebSocket 本地 echo 服务器连接/收发/关闭（127.0.0.1） |
| web/web.protobuff | HttpClient 打 localhost 测试服务器（HttpListener）GET/POST/超时/头注入 | — |
| download | 断点续传（Range 头断言）、失败重试、计数器 | 本地文件服务下载全流程 |
| config | Get/TryGet/Find/聚合函数；SortedDictionary 等价性 | — |
| asset/assetsystem | 初始化参数矩阵（fake backend）；包注册/默认包校验；Unload 调用链 | EditorSimulate 构建产物加载 PackedScene/Texture/Audio；PCK 挂载/卸载 |
| entity | ShowEntityInfo/状态机；fake IAssetManager 注入后 Show/Hide/Attach 流程 | .tscn 实体实例化→OnShow→Attach→Hide→QueueFree 全生命周期 |
| scene | —（薄封装） | LoadScene Single/Additive、Unload、SceneOrder、激活回调 |
| sound | 组/agent 注册、serialId、优先级抢占、StopAll 状态机 | headless 播放/暂停/恢复/淡入淡出进度/组音量（AudioServer Bus 断言） |
| ui | UIComponent.Get 查询矩阵、UIGroup 层级/暂停/覆盖、事件序（Before/本体/After 类比 localization） | Open/Close 全流程（PackedScene）、初始化重试、DesignResolution 变更 |
| localization | 三事件顺序、GetString 格式化回退 | — |
| setting | ConfigFile 后端回环、Storage 抽象注入 | 持久化跨重启（重开 headless 进程校验文件） |
| globalconfig | Request/Response 序列化 | — |
| startup | UrlFailoverRunner（本地 HttpListener 多 URL/重试/延迟）、StartupNetworkCacheUtility、StartupHttpParams | headless 状态机 11 状态全跑（fake Web + EditorSimulate asset）到 HotfixLauncher |
| systeminfo/clipboard | 缓存回环、Normalize | clipboard 读写（DisplayServer headless 支持） |
| getchannel | channel.txt 解析 | streaming_assets 渠道文件读取 |

### 6.3 回归基线

- 每个 Phase 结束输出对比快照（文件数/API 数），同步更新本计划文档附录。
- 与 Unity 版行为对拍项：TimerHelper 30 方法、Utility 哈希向量、protobuf 回环字节一致性。

---

## 七、附录：对比原始数据来源

- 包清单：Unity 38 包（199+30+9+2+9+23+29+36+6+167+5+8+1+9+99+5+5+15+14+6+56+1+6+14+7+10+18+28+30+1+6+190+43+13+12+14+6）Runtime .cs
- 深度对比报告（6 组）：核心框架 / 资源下载 / 网络 Web / UI / 场景表现时序 / 启动外围
- 已有文档：`Unity2Godot_迁移标准模板.md`（SOP）、`AssetSystem_Godot_迁移实施计划.md`（进行中，其 L1.2-RT/L2.1 待办并入本计划 Phase 2）
