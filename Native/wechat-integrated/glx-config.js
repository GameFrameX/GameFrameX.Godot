const gameGlobal = typeof GameGlobal !== 'undefined' ? GameGlobal : globalThis;

if (gameGlobal.__GODOT_DISABLE_WXGLX === undefined) {
    gameGlobal.__GODOT_DISABLE_WXGLX = false;
}
