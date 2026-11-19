// 微信小游戏 Godot+C# 全链路集成 boot（wechat-integrated）：
// 1) 引擎 Loader（godothub 模板原样）：异步加载 engine 分包并启动 Godot 帧循环，游戏 pck 即 engine/empty-tips.bin
// 2) 并行 boot .NET WASM 运行时：CJS 化 dotnet 模块 + WXWebAssembly 实例化 + wx fs 读程序集资源 + runMain
// 3) 暴露全局 C# 桥 window.__csharpBridge（完整单词命名），供 Godot 经 JavaScriptBridge.eval 驱动：
//    isBridgeReady / noteGodotAlive / invokeTick / readTickCount / readAssemblyVersion / hotSwitchAssembly
// 全部阶段结果写 USER_DATA_PATH/wechat-integrated.json，外层程序化取证（截图仅辅助）。
import './weapp-adapter'
import './glx-config'
import './godot-loader'

const fileSystem = wx.getFileSystemManager();
const resultPath = wx.env.USER_DATA_PATH + '/wechat-integrated.json';
const results = {
    stage: 'start',
    startedAt: Date.now(),
    stages: [],
    verdict: 'PENDING',
};

// 派生判定：每次落盘前依据原始数据重算各阶段结论（判定一律走文件，外层只读不算）
function recomputeVerdicts() {
    const hasPrint = function (keyword) {
        return (results.godotPrints || []).some(function (line) { return line.indexOf(keyword) >= 0; });
    };
    // 阶段1：引擎场景就绪（Godot print）+ C# 桥自检 3 次
    results.engineSceneReady = hasPrint('INTEGRATED-GD: scene ready');
    results.stage1Verdict = (results.engineSceneReady && results.selfTestTickCount === 3) ? 'PASS' : (results.selfTestTickCount === 3 ? 'PARTIAL(引擎未证)' : (results.error ? 'FAIL' : 'PENDING'));
    // 阶段2：Godot 经 print 命令通道驱动过 Tick 且读回有效计数（eval 通道在微信定制构建已证死路，弃用 godotAliveFrames 判据）
    const history = results.godotTickCountHistory || [];
    const godotDrivenTicks = history.filter(function (item) { return String(item.sourceTag || '').indexOf('godot-command-') === 0 && item.count > 0; }).length;
    results.stage2Verdict = (godotDrivenTicks > 0 && hasPrint('INTEGRATED-GD: tick seq=')) ? 'PASS' : 'PENDING';
    // 阶段3：Godot 读到的计数严格递增 ≥30 步
    let ascendingSteps = 0;
    for (let index = 1; index < history.length; index++) {
        if (history[index].count > history[index - 1].count) ascendingSteps += 1;
    }
    results.stage3AscendingSteps = ascendingSteps;
    if (results.stage2Verdict === 'PASS') {
        results.stage3Verdict = ascendingSteps >= 30 ? 'PASS' : 'PENDING(递增' + ascendingSteps + '步)';
    }
    // 阶段4：热切换后新版本计数继续递增且标识落盘
    if (results.hotSwitchHistory && results.hotSwitchHistory.length > 0) {
        const afterSwitch = history.filter(function (item) { return item.file.indexOf('ver=2') >= 0; });
        let ascendingV2 = 0;
        for (let index = 1; index < afterSwitch.length; index++) {
            if (afterSwitch[index].count > afterSwitch[index - 1].count) ascendingV2 += 1;
        }
        results.stage4AscendingV2Steps = ascendingV2;
        results.stage4Verdict = ascendingV2 >= 3 ? 'PASS' : 'PENDING';
    }
    if (!results.error && results.stage3Verdict === 'PASS' && (!results.hotSwitchHistory || results.stage4Verdict === 'PASS')) {
        results.verdict = 'PASS';
    }
}

function save(stage) {
    if (stage && stage !== results.stage) {
        results.stage = stage;
        results.stages.push({ stage: stage, at: Date.now() - results.startedAt });
    }
    results.savedAt = Date.now();
    recomputeVerdicts();
    try {
        fileSystem.writeFileSync(resultPath, JSON.stringify(results), 'utf8');
    } catch (error) {
        results.saveError = error && error.message;
    }
}

// 截流控制台：Godot 引擎 print 与 .NET 运行时日志全部收录进取证文件
results.godotPrints = [];
results.runtimeLogs = [];
const SPIKE_BISECT_DISABLE_ALL = false;  // 全禁（基线：引擎帧循环活）
const SPIKE_BISECT_CONSOLE_ONLY = false; // 仅 console hook
const SPIKE_BISECT_NO_RAF_WRAP = false;   // 二分：禁 rAF 包装
const SPIKE_BISECT_NO_WASM_SWAP = false;  // 二分：禁 WebAssembly 替换
const SPIKE_BISECT_NO_BOOT = false;       // 二分：禁 C# boot（其余全开）
const SPIKE_DEFER_BOOT_UNTIL_ENGINE = true; // 延后 C# boot 到引擎完全就绪（首选修复假设）

// C# boot 延后调度状态：引擎就绪信号（"Engine has started!" / 引擎启动 Promise）到达后才启动
let csharpBootStarted = false;
results.bootDeferral = { started: false, reason: '', engineStartedAt: null, bootStartedAt: null };
function recordEngineStarted(signal) {
    if (results.bootDeferral.engineStartedAt === null) {
        results.bootDeferral.engineStartedAt = Date.now() - results.startedAt;
        results.engineStartedSignal = signal;
        save();
    }
}
function startCSharpBoot(reason) {
    if (csharpBootStarted) return;
    csharpBootStarted = true;
    results.bootDeferral.started = true;
    results.bootDeferral.reason = reason;
    results.bootDeferral.bootStartedAt = Date.now() - results.startedAt;
    save('csharp-boot-scheduled');
    bootCSharp().catch(error => {
        results.verdict = 'FAIL';
        results.error = (error && error.name) + ': ' + (error && error.message);
        results.errorStack = error && error.stack ? String(error.stack).slice(0, 1500) : '';
        save('failed');
    });
}
for (const level of SPIKE_BISECT_DISABLE_ALL ? [] : ['error', 'warn', 'log', 'info', 'debug']) {
    const original = console[level].bind(console);
    console[level] = function () {
        const args = Array.prototype.slice.call(arguments);
        const line = args.map(function (item) { return typeof item === 'string' ? item : (item && item.message) || String(item); }).join(' ');
        // 引擎完全就绪信号：godot-sdk startGame Promise 链末尾打印（C# boot 延后调度的主触发）
        if (line.indexOf('Engine has started!') >= 0) {
            recordEngineStarted('console-print');
            if (SPIKE_DEFER_BOOT_UNTIL_ENGINE) startCSharpBoot('engine-started-print');
        }
        if (line.indexOf('INTEGRATED-GD-GDCMD:') === 0) {
            // Godot 命令走 print 通道：console 钩子同步分发（引擎此刻阻塞在 print 内，天然免并发）
            if (results.godotPrints.length < 500) results.godotPrints.push(line.slice(0, 300));
            original.apply(console, args);
            try {
                handleGodotCommand(line.slice('INTEGRATED-GD-GDCMD:'.length));
            } catch (commandError) {
                results.commandHandlerError = (commandError && commandError.name) + ': ' + (commandError && commandError.message);
                results.commandHandlerStack = String((commandError && commandError.stack) || '').slice(0, 800);
                save();
            }
            return;
        }
        if (line.indexOf('INTEGRATED-GD') >= 0 && results.godotPrints.length < 150) {
            results.godotPrints.push(line.slice(0, 300));
            save();
        } else if (line.indexOf('WECHAT-INTEGRATED') < 0 && results.runtimeLogs.length < 80) {
            results.runtimeLogs.push(level + ': ' + line.slice(0, 300));
        }

        original.apply(console, args);
    };
}

// 帧循环活性探针：包装全局 requestAnimationFrame，统计调度/触发次数并落盘（判定引擎帧循环死活）
results.loopProbe = { rafScheduled: 0, rafFired: 0 };
const originalRequestAnimationFrame = (typeof GameGlobal.requestAnimationFrame === 'function') ? GameGlobal.requestAnimationFrame.bind(GameGlobal) : ((typeof requestAnimationFrame === 'function') ? requestAnimationFrame.bind(globalThis) : null);
if (originalRequestAnimationFrame && !SPIKE_BISECT_DISABLE_ALL && !SPIKE_BISECT_CONSOLE_ONLY && !SPIKE_BISECT_NO_RAF_WRAP) {
    const probingRequestAnimationFrame = function (callback) {
        results.loopProbe.rafScheduled += 1;
        return originalRequestAnimationFrame(function (timestamp) {
            results.loopProbe.rafFired += 1;
            // 里程碑自检：rAF 活着但 Godot 主循环死掉时，探查引擎并尝试 resume
            if (results.loopProbe.rafFired === 300 || results.loopProbe.rafFired === 3000) {
                const sdk = GameGlobal.GODOTSDK || (typeof GODOTSDK !== 'undefined' ? GODOTSDK : null);
                const runtimeEnvironment = sdk && sdk.engine && sdk.engine.rtenv;
                const snapshot = { rafFired: results.loopProbe.rafFired, hasSdk: !!sdk, hasRtenv: !!runtimeEnvironment };
                try {
                    if (runtimeEnvironment) {
                        snapshot.loopKeys = Object.keys(runtimeEnvironment).filter(key => /loop|pause|resume/i.test(key));
                        if (typeof runtimeEnvironment.resumeMainLoop === 'function') {
                            runtimeEnvironment.resumeMainLoop();
                            snapshot.resumeCalled = true;
                        }
                    }
                } catch (probeError) { snapshot.probeError = String(probeError && probeError.message); }
                results.engineProbe = results.engineProbe || [];
                results.engineProbe.push(snapshot);
            }
            if (results.loopProbe.rafFired % 30 === 0) save();
            try {
                return callback(timestamp);
            } catch (callbackError) {
                // 捕获 rAF 回调（含引擎主循环）未捕获异常：console 面板的红色错误即此来源
                results.loopProbe.callbackErrors = results.loopProbe.callbackErrors || [];
                if (results.loopProbe.callbackErrors.length < 6) {
                    results.loopProbe.callbackErrors.push(String((callbackError && callbackError.stack) || callbackError).slice(0, 700));
                    save();
                }
                throw callbackError;
            }
        });
    };
    GameGlobal.requestAnimationFrame = probingRequestAnimationFrame;
    if (typeof globalThis !== 'undefined') globalThis.requestAnimationFrame = probingRequestAnimationFrame;
    if (typeof window !== 'undefined' && window) window.requestAnimationFrame = probingRequestAnimationFrame;
}
// 全局错误捕获（wx.onError 小游戏官方入口）
if (typeof wx !== 'undefined' && typeof wx.onError === 'function') {
    wx.onError(function (message) {
        results.wxErrors = results.wxErrors || [];
        if (results.wxErrors.length < 10) { results.wxErrors.push(String(message).slice(0, 400)); save(); }
    });
}
// 生命周期记录：引擎可能 boot 期收到 onHide 暂停主循环且从未 onShow
results.lifecycle = [];
if (typeof wx !== 'undefined') {
    if (typeof wx.onShow === 'function') wx.onShow(() => { results.lifecycle.push({ event: 'onShow', at: Date.now() - results.startedAt }); save(); });
    if (typeof wx.onHide === 'function') wx.onHide(() => { results.lifecycle.push({ event: 'onHide', at: Date.now() - results.startedAt }); save(); });
}

// 环境身份记录（微信里 globalThis / GameGlobal / window 是否同源）
results.globalThisIsGameGlobal = (typeof globalThis !== 'undefined') && (globalThis === GameGlobal);
results.typeofWindow = typeof window;

// ===== Godot↔JS 双向文件通道（user:// 即 WXMEMFS → wx USER_DATA_PATH/godot/app_userdata/<项目名>/）=====
// 引擎启动前预创建 JS→Godot 通道文件，保证 Godot 首次 file_exists 即命中；
// 启动后 JS 更新其内容，验证引擎能否看到内容变化（内容缓存边界由实验 B 落盘判定）
const godotProjectUserDirectory = wx.env.USER_DATA_PATH + '/godot/app_userdata/wechat-godot-csharp-integrated';
results.fsChannel = { preCreated: false, godotWrites: [] };
try {
    try { fileSystem.mkdirSync(godotProjectUserDirectory, true); } catch (mkdirError) { /* 已存在 */ }
    fileSystem.writeFileSync(godotProjectUserDirectory + '/js_to_godot.txt', 'js-precreated at 0', 'utf8');
    results.fsChannel.preCreated = true;
} catch (channelError) {
    results.fsChannel.preCreateError = channelError && channelError.message;
}

function writeJsToGodot(message) {
    try {
        fileSystem.writeFileSync(godotProjectUserDirectory + '/js_to_godot.txt', message, 'utf8');
        return true;
    } catch (error) {
        results.fsChannel.writeError = error && error.message;
        return false;
    }
}


// ===== 阶段1a：引擎 Loader 启动（异步，勿阻塞 C# boot）=====
const engineConfig = {
    textConfig: {
        firstStartText: '首次加载请耐心等待',
        downloadingText: ['正在加载资源', '加载中...', '请稍候...'],
        compilingText: '编译中',
        initText: '引擎初始化中',
        completeText: '开始游戏',
        textDuration: 1500,
        style: { color: '#ffffff', fontSize: 14 },
    },
    barConfig: {
        style: {
            width: 240, height: 25,
            backgroundColor: 'rgba(0, 0, 0, 0.5)',
            foregroundColor: '#4CAF50',
            borderRadius: 20, padding: 2,
        },
    },
    iconConfig: { visible: true, style: { width: 74, height: 30, bottom: 20 } },
    materialConfig: {
        backgroundImage: 'images/background.jpg',
        backgroundVideo: '',
        iconImage: 'images/logo.png',
    },
};
GameGlobal.godotLoader = new GodotLoader(canvas, engineConfig);
results.engineLoaderStartedAt = Date.now() - results.startedAt;
save('engine-loader-started');

// ===== 阶段1b：C# boot（与引擎并行）=====
// 最小 Response polyfill：dotnet.js 对下载结果只用 ok/status/url/arrayBuffer()/json()
class PackageResponse {
    constructor(buffer, url) {
        this.ok = true;
        this.status = 200;
        this.url = url;
        this._buffer = buffer;
    }
    arrayBuffer() { return Promise.resolve(this._buffer); }
    json() {
        const text = new TextDecoder('utf-8').decode(new Uint8Array(this._buffer));
        return Promise.resolve(JSON.parse(text));
    }
}

if (!SPIKE_BISECT_DISABLE_ALL && !SPIKE_BISECT_CONSOLE_ONLY && !SPIKE_BISECT_NO_WASM_SWAP) { GameGlobal.WebAssembly = GameGlobal.WXWebAssembly; }
GameGlobal.Response = GameGlobal.Response || PackageResponse;

const coreAssemblies = ['System.Private.CoreLib.dll', 'System.Runtime.InteropServices.JavaScript.dll'];
const assemblies = ['System.Runtime.dll', 'WasmLogic.dll'];
// 阶段4 热更程序集：包内存在 WasmLogicV2.bin 时预置进加载列表（mono_wasm_assembly_load 只能命中已装载程序集）
let hotUpdateAssemblyPresent = false;
try {
    fileSystem.accessSync('dotnetsdk/WasmLogicV2.bin');
    hotUpdateAssemblyPresent = true;
    assemblies.push('WasmLogicV2.dll');
} catch (error) { /* 阶段4 未启用时正常缺席 */ }
results.hotUpdateAssemblyPresent = hotUpdateAssemblyPresent;

function packagePathOf(assemblyName) {
    return 'dotnetsdk/' + assemblyName.replace(/\.dll$/, '.bin');
}

// 程序集名 → (命名空间, 类名) 映射：热切换时按名重建句柄链
const assemblyClassMap = {
    WasmLogic: { namespaceName: 'WasmLogic', className: 'LogicExports', version: 1 },
    WasmLogicV2: { namespaceName: 'WasmLogicV2', className: 'LogicExports', version: 2 },
};

const now = () => (typeof performance !== 'undefined' && performance.now) ? performance.now() : Date.now();

// ===== 场景化热更桥：多方法句柄链 + MEMFS 文件编解码 =====
const bridgeMethodNames = ['Tick', 'WriteSceneSpec', 'WriteUiSpec'];
function invokeVoidExport(methodHandle) {
    heapBytes.fill(0, argsPointer, argsPointer + SLOT_SIZE * SLOT_COUNT);
    invokeExport(methodHandle, argsPointer);
    return heapBytes[argsPointer + 12] === 0 ? 1 : 0;
}
function encodeFileContent(content) {
    return String(content).replace(/\r?\n/g, ';').replace(/ /g, '~').trim();
}
function buildMethodHandles(classHandle) {
    const methodHandles = {};
    for (const methodName of bridgeMethodNames) {
        methodHandles[methodName] = findMethodFunction(classHandle, cstringOf(methodName), -1);
    }
    return methodHandles;
}

const bridge = {
    // 桥是否完成 boot 并通过自检（Godot 轮询此入口）
    isBridgeReady: function () { return results.bridgeReady ? 1 : 0; },
    // Godot 帧循环心跳：记录引擎侧帧号，证明调用方是 Godot 而非 JS 自驱
    noteGodotAlive: function (frameNumber) {
        results.godotAliveFrames = results.godotAliveFrames || [];
        if (results.godotAliveFrames.length < 600) results.godotAliveFrames.push(frameNumber);
        save();
        return 1;
    },
    // 驱动当前激活程序集的 Tick 一次（记录单次跨桥耗时）
    invokeTick: function () {
        if (!activeHandles || !argsPointer || !activeHandles.methods.Tick) return -1;
        const start = now();
        heapBytes.fill(0, argsPointer, argsPointer + SLOT_SIZE * SLOT_COUNT);
        invokeExport(activeHandles.methods.Tick, argsPointer);
        const elapsed = now() - start;
        results.invokeTimingsMilliseconds = results.invokeTimingsMilliseconds || [];
        if (results.invokeTimingsMilliseconds.length < 600) results.invokeTimingsMilliseconds.push(Number(elapsed.toFixed(4)));
        const exception = heapBytes[argsPointer + 12];
        if (exception !== 0) {
            results.tickExceptionType = exception;
            results.verdict = 'FAIL';
            save('tick-exception');
        }
        return 1;
    },
    // 读回 C# 侧计数（/spike-tick.txt，MEMFS）：记录 Godot 每次读到的值
    readTickCount: function (sourceTag) {
        if (!fsModule) return -1;
        let content = '';
        try {
            content = fsModule.readFile('/spike-tick.txt', { encoding: 'utf8' });
        } catch (error) {
            results.tickFileReadError = error && error.message;
            return -1;
        }
        const match = /ticks=(\d+)/.exec(content);
        const count = match ? parseInt(match[1], 10) : -1;
        results.godotTickCountHistory = results.godotTickCountHistory || [];
        if (results.godotTickCountHistory.length < 600) {
            results.godotTickCountHistory.push({ at: Date.now() - results.startedAt, count: count, file: content.slice(0, 80), sourceTag: sourceTag || 'js' });
        }
        save();
        return count;
    },
    // 当前激活程序集版本（依据 Tick 落盘标记 ver=2 判定）
    readAssemblyVersion: function () {
        if (!fsModule) return -1;
        try {
            const content = fsModule.readFile('/spike-tick.txt', { encoding: 'utf8' });
            return content.indexOf('ver=2') >= 0 ? 2 : 1;
        } catch (error) { return -1; }
    },
    // 阶段4 热切换：按程序集名二次 load + find_class/find_method 重建句柄链
    hotSwitchAssembly: function (assemblyName) {
        const info = assemblyClassMap[assemblyName];
        if (!info) { results.hotSwitchError = 'unknown assembly: ' + assemblyName; save(); return 0; }
        const assemblyHandle = assemblyLoadFunction(cstringOf(assemblyName));
        if (!assemblyHandle) {
            // 能力边界取证：未预置进 config assembly 列表的程序集 load 不到句柄
            results.assemblyLoadBoundary = results.assemblyLoadBoundary || [];
            results.assemblyLoadBoundary.push({ assemblyName: assemblyName, assemblyHandle: 0, note: 'mono_wasm_assembly_load 仅能命中 config 预置程序集' });
            save('hot-switch-unloadable');
            return 0;
        }
        const classHandle = findClassFunction(assemblyHandle, cstringOf(info.namespaceName), cstringOf(info.className));
        const methodHandles = classHandle ? buildMethodHandles(classHandle) : null;
        if (!classHandle || !methodHandles || !methodHandles.Tick || !methodHandles.WriteSceneSpec || !methodHandles.WriteUiSpec) {
            results.hotSwitchError = 'handle chain broken for ' + assemblyName;
            save();
            return 0;
        }
        activeHandles = { assemblyName: assemblyName, assemblyHandle: assemblyHandle, classHandle: classHandle, methods: methodHandles };
        results.hotSwitchHistory = results.hotSwitchHistory || [];
        results.hotSwitchHistory.push({ at: Date.now() - results.startedAt, assemblyName: assemblyName, version: info.version });
        save('hot-switched-' + assemblyName);
        return 1;
    },
    // 程序集句柄探测：返回 mono_wasm_assembly_load 原始句柄（0 即未预置边界证据）
    probeAssemblyHandle: function (assemblyName) {
        if (!assemblyLoadFunction) return -1;
        return assemblyLoadFunction(cstringOf(assemblyName));
    },
};

// ===== Godot 命令处理器：命令来自 console 钩子的 print 通道分发，响应写 js_to_godot.txt =====
// 命令格式 "<命令> <序号> msec=<引擎毫秒>"；响应格式 "OK <序号> key=value ..."（Godot 按序号去重）
function handleGodotCommand(rawLine) {
    results.handlerEntered = (results.handlerEntered || 0) + 1; // 入口计数：区分"代码没跑到"vs"代码是旧版"
    // 逐语句隔离：任一句抛错时落盘步骤名 + 原始入参形态（微信环境 String/parseInt 可能被改写）
    const stepFail = function (step, error) {
        results.stepFail = step + ': ' + (error && error.message) + ' | typeofRaw=' + (typeof rawLine) + ' raw=' + String(rawLine).slice(0, 60) + ' String=' + (typeof String) + ' parseInt=' + (typeof parseInt);
        save();
        throw error;
    };
    let line = '';
    try { line = String(rawLine).trim(); } catch (stepError) { stepFail('trim', stepError); }
    let parts = [];
    try { parts = line.split(/\s+/); } catch (stepError) { stepFail('split', stepError); }
    let sequence = 0;
    try { sequence = parseInt(parts[1], 10) || 0; } catch (stepError) { stepFail('parse-seq', stepError); }
    let commandName = '';
    try { commandName = parts[0]; } catch (stepError) { stepFail('cmd', stepError); }
    // 修复：SWITCH/PROBE/SPEC/UIEV 带目标参数，格式为 "<命令> <目标> <序号> msec="（目标在 parts[1]，序号在 parts[2]）
    if ((commandName === 'SWITCH' || commandName === 'PROBE' || commandName === 'SPEC' || commandName === 'UIEV') && parts.length >= 4) {
        sequence = parseInt(parts[2], 10) || 0;
    }
    try { godotMsecMatch = /msec=(\d+)/.exec(line); } catch (stepError) { stepFail('exec', stepError); }
    let godotMsec = 0;
    try { godotMsec = godotMsecMatch ? parseInt(godotMsecMatch[1], 10) : 0; } catch (stepError) { stepFail('msec', stepError); }
    if (!results.bridgeReady) {
        // 桥未就绪：回应 WAIT，Godot 侧重发（不进命令日志，避免污染证据）
        writeJsToGodot('WAIT ' + sequence + ' msec=' + godotMsec);
        return;
    }
    const processStart = now();
    // 命令分步追踪：定位 "reading '1'" 抛点（每步落盘，最后一步即抛点）
    const commandTrace = function (step) {
        results.commandTrace = results.commandTrace || [];
        if (results.commandTrace.length < 60) results.commandTrace.push(step);
        save();
    };
    let response = 'ERR ' + sequence + ' unknown ' + commandName;
    if (commandName === 'TICK') {
        commandTrace('tick-begin seq=' + sequence);
        const tickResult = bridge.invokeTick();
        commandTrace('invoke-ok ret=' + tickResult);
        const count = bridge.readTickCount('godot-command-' + sequence);
        commandTrace('read-ok count=' + count);
        const version = bridge.readAssemblyVersion();
        commandTrace('version-ok ver=' + version);
        let stateEncoded = '';
        try { stateEncoded = encodeFileContent(fsModule.readFile('/ui-state.txt', { encoding: 'utf8' })); } catch (stateError) { stateEncoded = ''; }
        commandTrace('state-ok len=' + stateEncoded.length);
        response = 'OK ' + sequence + ' count=' + count + ' ver=' + version + ' state=' + stateEncoded + ' msec=' + godotMsec;
    } else if (commandName === 'SWITCH') {
        const target = parts[1] || ''; // 目标参数在 parts[1]（SWITCH <目标> <序号> msec=）
        const switched = bridge.hotSwitchAssembly(target);
        response = 'OK ' + sequence + ' switched=' + switched + ' target=' + target + ' msec=' + godotMsec;
    } else if (commandName === 'PROBE') {
        const target = parts[1] || ''; // 目标参数在 parts[1]（PROBE <目标> <序号> msec=）
        const handle = bridge.probeAssemblyHandle(target);
        response = 'OK ' + sequence + ' handle=' + handle + ' target=' + target + ' msec=' + godotMsec;
    } else if (commandName === 'SPEC') {
        // 场景化热更：驱动当前程序集下发场景与 UI 规格（MEMFS 文件 → 编码进响应）
        let sceneEncoded = '';
        let uiEncoded = '';
        let specError = '';
        try {
            const sceneResult = invokeVoidExport(activeHandles.methods.WriteSceneSpec);
            sceneEncoded = sceneResult ? encodeFileContent(fsModule.readFile('/scene-spec.txt', { encoding: 'utf8' })) : '';
            const uiResult = invokeVoidExport(activeHandles.methods.WriteUiSpec);
            uiEncoded = uiResult ? encodeFileContent(fsModule.readFile('/ui-spec.txt', { encoding: 'utf8' })) : '';
            if (!sceneResult) { specError = 'scene-invoke-exception'; }
            else if (!uiResult) { specError = 'ui-invoke-exception'; }
            else if (!sceneEncoded || !uiEncoded) { specError = 'spec-file-empty'; }
        } catch (specFailure) {
            specError = 'spec:' + (specFailure && specFailure.message);
        }
        results.specDelivered = (results.specDelivered || 0) + 1;
        results.specAssemblyName = activeHandles.assemblyName;
        save();
        response = 'OK ' + sequence + ' assembly=' + activeHandles.assemblyName + ' scene=' + sceneEncoded + ' ui=' + uiEncoded + ' specError=' + specError + ' msec=' + godotMsec;
    } else if (commandName === 'UIEV') {
        // 交互事件转发：写入 C# MEMFS 事件文件，下一次 TICK 由 C# 消费
        const eventId = parts[1] || '';
        let eventError = '';
        try {
            let existingEvents = '';
            try { existingEvents = fsModule.readFile('/ui-events.txt', { encoding: 'utf8' }); } catch (readError) { existingEvents = ''; }
            fsModule.writeFile('/ui-events.txt', existingEvents + 'button:' + eventId + '\n');
            results.uiEventForwarded = (results.uiEventForwarded || 0) + 1;
            save();
        } catch (eventFailure) {
            eventError = (eventFailure && eventFailure.message) || 'write-failed';
        }
        response = 'OK ' + sequence + ' event=' + eventId + ' eventError=' + eventError + ' msec=' + godotMsec;
    }
    const processingMilliseconds = now() - processStart;
    results.commandLog = results.commandLog || [];
    if (results.commandLog.length < 400) {
        results.commandLog.push({ at: Date.now() - results.startedAt, command: line.slice(0, 90), response: response.slice(0, 90), processingMilliseconds: Number(processingMilliseconds.toFixed(3)) });
    }
    writeJsToGodot(response);
    save();
}
// 桥同时挂 GameGlobal / globalThis / window（Godot eval 用 window.__csharpBridge 访问）
GameGlobal.__csharpBridge = bridge;
// 通道结论（实证）：JS 读引擎写的文件会死锁 wx fs → JS 不再轮询任何引擎文件；
// Godot→JS 命令一律走 print（console 钩子同步分发），JS→Godot 响应写 js_to_godot.txt（引擎读）。
// 不可 unlink 引擎 userfs 内文件（WXMEMFS 惰性节点表会卡死引擎帧循环）。
if (typeof globalThis !== 'undefined') globalThis.__csharpBridge = bridge;
if (typeof window !== 'undefined' && window) window.__csharpBridge = bridge;

// 低层 invoke 桥共享状态（boot 完成后填充）
let runtimeModule = null;
let heapBytes = null;
let fsModule = null;
let invokeExport = null;
let assemblyLoadFunction = null;
let findClassFunction = null;
let findMethodFunction = null;
let cstringOf = null;
let activeHandles = null;
const SLOT_SIZE = 32;
const SLOT_COUNT = 3;
let argsPointer = 0;

async function bootCSharp() {
    // dotnetsdk 分包就绪门：主包瘦身到 4MB 内，.NET 运行时全部移入分包，就绪后方可 require
    save('subpackage-load-started');
    await new Promise((resolve, reject) => {
        wx.loadSubpackage({
            name: 'dotnetsdk',
            success: () => resolve(),
            fail: (error) => {
                results.subpackageLoadError = error && (error.errMsg || error.message);
                save('subpackage-load-failed');
                reject(new Error('dotnetsdk subpackage load failed: ' + JSON.stringify(error)));
            },
        });
    });
    save('require-modules');
    const dotnetModule = require('./dotnetsdk/dotnet.cjs.js');
    const nativeModule = require('./dotnetsdk/dotnet.native.cjs.js');
    const runtimeJsModule = require('./dotnetsdk/dotnet.runtime.cjs.js');
    const config = {
        mainAssemblyName: 'WasmLogic',
        assembly: assemblies,
        resources: {
            coreAssembly: coreAssemblies.map(name => ({ name: name, behavior: 'assembly' })),
            assembly: assemblies.map(name => ({ name: name, behavior: 'assembly' })),
            wasmNative: [{ name: 'dotnet.native.wasm' }],
            jsModuleNative: [{ name: 'dotnet.native.js', moduleExports: nativeModule }],
            jsModuleRuntime: [{ name: 'dotnet.runtime.js', moduleExports: runtimeJsModule }],
        },
    };

    save('create-runtime');
    const runtime = await dotnetModule.dotnet
        .withConfig(config)
        .withResourceLoader((type, name, defaultUri) => {
            if (type === 'assembly' || type === 'pdb') {
                const buffer = fileSystem.readFileSync(packagePathOf(name));
                return Promise.resolve(new PackageResponse(buffer, defaultUri));
            }
            return defaultUri;
        })
        .withModuleConfig({
            instantiateWasm(imports, receiveInstance) {
                WXWebAssembly.instantiate('dotnetsdk/dotnet.native.wasm', imports)
                    .then(output => { receiveInstance(output.instance || output, output.module); })
                    .catch(error => {
                        results.instantiateError = (error && error.name) + ': ' + (error && error.message);
                        results.verdict = 'FAIL';
                        save('instantiate-failed');
                    });
                return {};
            },
        })
        .create();

    runtimeModule = runtime.Module;
    fsModule = runtimeModule && runtimeModule.FS;
    heapBytes = runtimeModule.HEAPU8;

    // 跑 C# Main：运行时完全初始化并留下启动标记
    save('run-main');
    results.runMainExitCode = await runtime.runMain();
    if (fsModule) {
        try { results.bootMarker = fsModule.readFile('/spike-boot.txt', { encoding: 'utf8' }); } catch (error) { results.bootMarker = 'missing'; }
    }

    // ===== 句柄链：assembly → class → method（绕过 JSExport 绑定层）=====
    save('invoke-bridge');
    const rawExports = runtimeModule.wasmExports || {};
    function bridgeFunction(name) {
        if (typeof rawExports[name] === 'function') return rawExports[name];
        if (typeof runtimeModule[name] === 'function') return runtimeModule[name];
        throw new Error('bridge function missing: ' + name);
    }
    assemblyLoadFunction = bridgeFunction('mono_wasm_assembly_load');
    findClassFunction = bridgeFunction('mono_wasm_assembly_find_class');
    findMethodFunction = bridgeFunction('mono_wasm_assembly_find_method');
    invokeExport = bridgeFunction('mono_wasm_invoke_jsexport');

    cstringOf = function (text) {
        const pointer = runtimeModule._malloc(text.length + 1);
        for (let index = 0; index < text.length; index++) heapBytes[pointer + index] = text.charCodeAt(index);
        heapBytes[pointer + text.length] = 0;
        return pointer;
    };

    activeHandles = {
        assemblyName: 'WasmLogic',
        assemblyHandle: assemblyLoadFunction(cstringOf('WasmLogic')),
        classHandle: null,
        methods: null,
    };
    const v1Info = assemblyClassMap.WasmLogic;
    activeHandles.classHandle = findClassFunction(activeHandles.assemblyHandle, cstringOf(v1Info.namespaceName), cstringOf(v1Info.className));
    activeHandles.methods = activeHandles.classHandle ? buildMethodHandles(activeHandles.classHandle) : null;
    if (!activeHandles.assemblyHandle || !activeHandles.classHandle
        || !activeHandles.methods || !activeHandles.methods.Tick
        || !activeHandles.methods.WriteSceneSpec || !activeHandles.methods.WriteUiSpec) {
        throw new Error('handle chain broken: ' + JSON.stringify(activeHandles));
    }
    results.bridgeHandles = { assemblyHandle: activeHandles.assemblyHandle, classHandle: activeHandles.classHandle, methods: bridgeMethodNames.join(',') };

    argsPointer = runtimeModule._malloc(SLOT_SIZE * SLOT_COUNT);
    heapBytes.fill(0, argsPointer, argsPointer + SLOT_SIZE * SLOT_COUNT);

    // 阶段1 自检：JS 侧连续驱动 3 次 Tick 并读回（证明桥功能完整，不依赖 Godot）
    for (let index = 0; index < 3; index++) bridge.invokeTick();
    results.selfTestTickCount = bridge.readTickCount();
    results.bridgeReadyAt = Date.now() - results.startedAt;
    results.bridgeReady = true;
    results.stage1Verdict = (results.selfTestTickCount === 3) ? 'PASS' : 'FAIL';
    save('csharp-bridge-ready');

    // 实验B续：桥就绪后更新通道文件内容（验证 Godot 能否看到 JS 侧内容更新）
    results.fsChannel.jsUpdateAfterReady = writeJsToGodot('csharp bridge ready, selfTestTicks=' + results.selfTestTickCount + ' at ' + results.bridgeReadyAt);

    // 阶段4 预置程序集句柄探测（若启用）：确认 V2 已可 load
    if (hotUpdateAssemblyPresent) {
        const v2Handle = assemblyLoadFunction(cstringOf('WasmLogicV2'));
        results.hotUpdateAssemblyHandle = v2Handle;
        save('hot-update-assembly-probed');
    }
}

// ===== C# boot 调度（首选修复假设：并发踩 wx fs / WebAssembly 杀死引擎主循环 → 延后到引擎完全就绪）=====
// 就绪信号优先级：console 钩子捕获 "Engine has started!" > 引擎启动 Promise 轮询 > 兜底定时器
if (SPIKE_BISECT_DISABLE_ALL || SPIKE_BISECT_CONSOLE_ONLY || SPIKE_BISECT_NO_BOOT) {
    // 二分基线：不启动 C# boot
} else if (SPIKE_DEFER_BOOT_UNTIL_ENGINE) {
    const enginePromiseWatcher = setInterval(() => {
        if (GameGlobal.__engineStarted) {
            clearInterval(enginePromiseWatcher);
            startCSharpBoot('engine-promise');
        }
    }, 250);
    setTimeout(() => {
        clearInterval(enginePromiseWatcher);
        // 兜底延到 60s：15s 会在引擎冷缓存慢加载时提前触发并发 boot，重新引入主循环死锁
        startCSharpBoot('fallback-timer-60s');
    }, 60000);
} else {
    startCSharpBoot('immediate');
}
