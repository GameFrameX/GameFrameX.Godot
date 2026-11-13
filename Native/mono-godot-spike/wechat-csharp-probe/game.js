// C1 Spike-W3：微信小游戏容器内 JS↔C# 底层 invoke 桥闭环验证。
// 上轮结论（W2）：容器缺 WebAssembly.Function/Tag/Exception → [JSExport]/[JSImport]
// 启动期绑定全部静默失败（getAssemblyExports 返回空）。本轮绕过绑定层，直接经
// runtime.Module 的 wasm 导出句柄链驱动 C#：
//   mono_wasm_assembly_load(程序集名) → find_class(命名空间, 类名)
//   → find_method(类, 方法名, -1) → mono_wasm_invoke_jsexport(方法句柄, args缓冲)
// args 缓冲布局（由 www/dotnet.runtime.js 反解 + Node 实证）：
//   每槽 32 字节，槽内 +0 原始值(8字节) +8 指针 +12 类型标签(1字节) +13 子类型；
//   slot0=异常 slot1=返回值 slot2..=参数。无参 void 方法只需清零块（type=0=NULL）。
// 能力边界（Node 实证）：void() 可用；带返回值方法可执行但返回不回填；带参方法崩溃。
// 每个阶段结果落盘 USER_DATA_PATH/csharp-probe.json，外层程序化读取。
const fileSystem = wx.getFileSystemManager();
const resultPath = `${wx.env.USER_DATA_PATH}/csharp-probe.json`;
const results = { stage: 'start', startedAt: Date.now() };

function save(stage) {
    if (stage) results.stage = stage;
    results.savedAt = Date.now();
    try {
        fileSystem.writeFileSync(resultPath, JSON.stringify(results, null, 1), 'utf8');
    } catch (error) {
        results.saveError = error.message;
    }
    console.log('CSHARP-PROBE', JSON.stringify(results));
}

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

// 截流运行时日志进结果文件（IDE 控制台不落盘）
results.runtimeLogs = [];
for (const level of ['error', 'warn', 'log', 'info', 'debug']) {
    const original = console[level].bind(console);
    console[level] = (...args) => {
        const line = args.map(item => (typeof item === 'string' ? item : (item && item.message) || String(item))).join(' ');
        if (!line.startsWith('CSHARP-PROBE') && results.runtimeLogs.length < 60) {
            results.runtimeLogs.push(level + ': ' + line.slice(0, 300));
        }
        original(...args);
    };
}

// C# [JSImport("globalThis.__csharpSignal")] 的回调落点（绑定判死，仅留探测）
globalThis.__csharpSignal = message => {
    results.csharpSignal = String(message);
    save('csharp-signal');
};

// 环境身份探针：微信里 globalThis / GameGlobal / window 是否同一对象
results.globalThisIsGameGlobal = (typeof globalThis !== 'undefined') && (globalThis === GameGlobal);
results.wxWasmApi = ['Function', 'Table', 'Memory', 'Module', 'Instance', 'Tag', 'Exception', 'Global']
    .map(name => name + '=' + typeof WXWebAssembly[name]).join(' ');
results.stdWasmApi = typeof WebAssembly === 'undefined' ? 'absent' : 'present';
results.typeofWindow = typeof window;

GameGlobal.WebAssembly = GameGlobal.WXWebAssembly;
GameGlobal.Response = GameGlobal.Response || PackageResponse;

const coreAssemblies = ['System.Private.CoreLib.dll', 'System.Runtime.InteropServices.JavaScript.dll'];
const assemblies = ['System.Runtime.dll', 'WasmLogic.dll'];

function packagePathOf(assemblyName) {
    return 'dotnet/' + assemblyName.replace(/\.dll$/, '.bin');
}

async function boot() {
    save('require-modules');
    const dotnetModule = require('./dotnet/dotnet.cjs.js');
    const nativeModule = require('./dotnet/dotnet.native.cjs.js');
    const runtimeModule = require('./dotnet/dotnet.runtime.cjs.js');
    results.moduleKeys = {
        dotnet: Object.keys(dotnetModule),
        native: Object.keys(nativeModule).slice(0, 8),
        runtime: Object.keys(runtimeModule).slice(0, 8),
    };

    const config = {
        mainAssemblyName: 'WasmLogic',
        diagnosticTracing: true,
        assembly: assemblies,
        resources: {
            coreAssembly: coreAssemblies.map(name => ({ name, behavior: 'assembly' })),
            assembly: assemblies.map(name => ({ name, behavior: 'assembly' })),
            wasmNative: [{ name: 'dotnet.native.wasm' }],
            jsModuleNative: [{ name: 'dotnet.native.js', moduleExports: nativeModule }],
            jsModuleRuntime: [{ name: 'dotnet.runtime.js', moduleExports: runtimeModule }],
        },
    };

    save('create-runtime');
    const runtime = await dotnetModule.dotnet
        .withConfig(config)
        .withResourceLoader((type, name, defaultUri) => {
            (results.loaderCalls = results.loaderCalls || []).push(type + ':' + name);
            if (type === 'assembly' || type === 'pdb') {
                const buffer = fileSystem.readFileSync(packagePathOf(name));
                return Promise.resolve(new PackageResponse(buffer, defaultUri));
            }
            return defaultUri;
        })
        .withModuleConfig({
            instantiateWasm(imports, receiveInstance) {
                results.instantiateCalled = true;
                WXWebAssembly.instantiate('dotnet/dotnet.native.wasm', imports)
                    .then(output => {
                        results.instantiateShape = Object.keys(output || {}).join(',');
                        receiveInstance(output.instance || output, output.module);
                    })
                    .catch(error => {
                        results.instantiateError = `${error && error.name}: ${error && error.message}`;
                        save('instantiate-failed');
                    });
                return {};
            },
        })
        .create();
    results.runtimeApiKeys = Object.keys(runtime || {}).slice(0, 30);
    results.instantiateCalled = !!results.instantiateCalled;

    // 跑 C# Main：验证运行时完全初始化并留下启动标记
    save('run-main');
    try {
        results.runMainExitCode = await runtime.runMain();
    } catch (error) {
        results.runMainError = String(error && error.message);
    }

    // 经 emscripten FS 验证 C# Main 的落盘标记（数据通道成立）
    save('boot-marker');
    const fsModule = runtime.Module && runtime.Module.FS;
    results.fsPresent = !!fsModule;
    if (fsModule) {
        try {
            results['/spike-boot.txt'] = fsModule.readFile('/spike-boot.txt', { encoding: 'utf8' });
        } catch (error) {
            results['/spike-boot.txt'] = 'read failed: ' + (error && error.message);
        }
    }

    // ===== 低层 invoke 桥：绕过 JSExport 绑定直接驱动 C# =====
    save('invoke-bridge');
    const module = runtime.Module;
    const heapU8 = module.HEAPU8;
    const rawExports = module.wasmExports || {};
    const SLOT_SIZE = 32;
    const SLOT_COUNT = 3;

    // 桥函数解析：优先 wasmExports 原始导出（纯 number 参数），兜底 Module.cwrap
    const bridgeMode = rawExports.mono_wasm_invoke_jsexport ? 'wasmExports+manualCString' : 'cwrap';
    function cstringOf(text) {
        // 写 ASCII C 字符串进 wasm 堆（桥用的名字全是 ASCII）
        const pointer = module._malloc(text.length + 1);
        for (let index = 0; index < text.length; index++) heapU8[pointer + index] = text.charCodeAt(index);
        heapU8[pointer + text.length] = 0;
        return pointer;
    }
    function bridgeFunction(name, returnType, argumentTypes) {
        if (typeof rawExports[name] === 'function') return rawExports[name];
        if (typeof module[name] === 'function') return module[name];
        if (typeof module.cwrap === 'function') return module.cwrap(name, returnType, argumentTypes);
        throw new Error('bridge function missing: ' + name);
    }
    const assemblyLoad = bridgeFunction('mono_wasm_assembly_load', 'number', ['string']);
    const findClass = bridgeFunction('mono_wasm_assembly_find_class', 'number', ['number', 'string', 'string']);
    const findMethod = bridgeFunction('mono_wasm_assembly_find_method', 'number', ['number', 'string', 'number']);
    const methodNameOf = bridgeFunction('mono_wasm_method_get_name', 'number', ['number']);
    const invokeExport = bridgeFunction('mono_wasm_invoke_jsexport', null, ['number', 'number']);
    results.bridgeEntry = 'mono_wasm_invoke_jsexport';
    results.bridgeMode = bridgeMode;

    // 句柄链：assembly → class → method（程序集名为不带扩展名的简单名）
    const stringMode = bridgeMode === 'wasmExports+manualCString';
    const assemblyHandle = assemblyLoad(stringMode ? cstringOf('WasmLogic') : 'WasmLogic');
    const classHandle = findClass(assemblyHandle, stringMode ? cstringOf('WasmLogic') : 'WasmLogic', stringMode ? cstringOf('LogicExports') : 'LogicExports');
    const tickHandle = findMethod(classHandle, stringMode ? cstringOf('Tick') : 'Tick', -1);
    const tickPureHandle = findMethod(classHandle, stringMode ? cstringOf('TickPure') : 'TickPure', -1);
    results.bridgeHandles = { assemblyHandle, classHandle, tickHandle, tickPureHandle };
    if (!assemblyHandle || !classHandle || !tickHandle) throw new Error('handle chain broken: ' + JSON.stringify(results.bridgeHandles));

    // 读回句柄方法名，确认 find_method 按名匹配
    if (stringMode) {
        const namePointer = methodNameOf(tickHandle);
        const bytes = [];
        let cursor = namePointer;
        while (heapU8[cursor] !== 0) { bytes.push(heapU8[cursor]); cursor++; }
        results.bridgeResolvedMethodName = String.fromCharCode.apply(null, bytes);
    } else {
        results.bridgeResolvedMethodName = methodNameOf(tickHandle);
    }

    // args 缓冲：SLOT_COUNT 个 32 字节槽清零（无参 void 方法即合法输入）
    const argsPointer = module._malloc(SLOT_SIZE * SLOT_COUNT);
    heapU8.fill(0, argsPointer, argsPointer + SLOT_SIZE * SLOT_COUNT);
    const exceptionType = () => heapU8[argsPointer + 12];

    const now = () => (typeof performance !== 'undefined' && performance.now) ? performance.now() : Date.now();

    // 1) Tick 连续驱动 3 次（验收：≥3 次，C# 侧计数落盘）
    save('tick-loop');
    const tickTimingsMilliseconds = [];
    for (let index = 0; index < 3; index++) {
        heapU8.fill(0, argsPointer, argsPointer + SLOT_SIZE * SLOT_COUNT);
        const start = now();
        invokeExport(tickHandle, argsPointer);
        tickTimingsMilliseconds.push(now() - start);
        const exception = exceptionType();
        if (exception !== 0) throw new Error('Tick invoke ' + index + ' exception type ' + exception);
    }
    results.bridgeTickTimingsMilliseconds = tickTimingsMilliseconds;
    results.tickFileAfterLoop = fsModule.readFile('/spike-tick.txt', { encoding: 'utf8' });

    // 2) TickPure 性能循环：纯 invoke 开销（无文件 I/O 干扰）
    save('pure-loop');
    const pureLoopCount = 2000;
    const startLoop = now();
    for (let index = 0; index < pureLoopCount; index++) {
        invokeExport(tickPureHandle, argsPointer);
    }
    const pureLoopTotalMilliseconds = now() - startLoop;
    results.bridgePureLoop = {
        count: pureLoopCount,
        totalMilliseconds: pureLoopTotalMilliseconds,
        microsecondsPerCall: (pureLoopTotalMilliseconds * 1000 / pureLoopCount).toFixed(3),
    };

    // 3) 收尾 Tick×1：文件计数应为 3 + pureLoopCount + 1（交叉证明循环真实执行）
    heapU8.fill(0, argsPointer, argsPointer + SLOT_SIZE * SLOT_COUNT);
    invokeExport(tickHandle, argsPointer);
    results.tickFileFinal = fsModule.readFile('/spike-tick.txt', { encoding: 'utf8' });
    const finalCountMatch = /ticks=(\d+)/.exec(results.tickFileFinal);
    const finalTickCount = finalCountMatch ? parseInt(finalCountMatch[1], 10) : -1;
    results.bridgeFinalTickCount = finalTickCount;

    // 能力边界备注（Node 环境实证，微信侧只跑安全路径）
    results.bridgeCapabilityNotes = [
        'void() 无参静态方法：完全可用（本验收路径）',
        '无参带返回值：方法执行但返回值不回填结果槽（slot1 保持 type=0）',
        '带参数方法：invoke 崩溃 NullReferenceException（Node 实证，微信侧未测避免中断）',
        '生产桥形态：C# 侧全部暴露 static void 无参命令入口，数据经共享内存/文件通道往返',
    ].join('；');

    // 验收判定：连续驱动 ≥3 次且计数交叉吻合
    results.verdict = (finalTickCount === 3 + pureLoopCount + 1) ? 'PASS' : 'FAIL';
    save('done');
}

boot().catch(error => {
    results.verdict = 'FAIL';
    results.error = `${error && error.name}: ${error && error.message}`;
    results.errorStack = error && error.stack ? String(error.stack).slice(0, 1500) : '';
    save('failed');
});
