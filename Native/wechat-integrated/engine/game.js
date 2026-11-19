import './godot-sdk'
import './godot'
const exe = '/engine/godot';
const pack = '/engine/empty-tips.bin';
// 引擎启动 Promise 暴露给主包：主包据此把 C# boot 延后到引擎完全就绪之后（修复并发踩 wx fs 杀死主循环）
const engineStartPromise = GODOTSDK.startGame(exe, pack);
const gameGlobalScope = typeof GameGlobal !== 'undefined' ? GameGlobal : globalThis;
gameGlobalScope.__engineStartPromise = engineStartPromise;
engineStartPromise.then(function () {
    gameGlobalScope.__engineStarted = true;
}).catch(function (error) {
    gameGlobalScope.__engineStartError = String(error && error.message);
});
