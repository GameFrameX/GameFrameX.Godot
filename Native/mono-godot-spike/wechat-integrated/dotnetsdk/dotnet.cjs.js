var __create = Object.create;
var __defProp = Object.defineProperty;
var __getOwnPropDesc = Object.getOwnPropertyDescriptor;
var __getOwnPropNames = Object.getOwnPropertyNames;
var __getProtoOf = Object.getPrototypeOf;
var __hasOwnProp = Object.prototype.hasOwnProperty;
var __export = (target, all) => {
  for (var name in all)
    __defProp(target, name, { get: all[name], enumerable: true });
};
var __copyProps = (to, from, except, desc) => {
  if (from && typeof from === "object" || typeof from === "function") {
    for (let key of __getOwnPropNames(from))
      if (!__hasOwnProp.call(to, key) && key !== except)
        __defProp(to, key, { get: () => from[key], enumerable: !(desc = __getOwnPropDesc(from, key)) || desc.enumerable });
  }
  return to;
};
var __toESM = (mod, isNodeMode, target) => (target = mod != null ? __create(__getProtoOf(mod)) : {}, __copyProps(
  // If the importer is in node compatibility mode or this is not an ESM
  // file that has been converted to a CommonJS file using a Babel-
  // compatible transform (i.e. "__esModule" has not been set), then set
  // "default" to the CommonJS "module.exports" for node compatibility.
  isNodeMode || !mod || !mod.__esModule ? __defProp(target, "default", { value: mod, enumerable: true }) : target,
  mod
));
var __toCommonJS = (mod) => __copyProps(__defProp({}, "__esModule", { value: true }), mod);
var dotnet_exports = {};
__export(dotnet_exports, {
  default: () => gt,
  dotnet: () => ft,
  exit: () => mt
});
module.exports = __toCommonJS(dotnet_exports);
//! Licensed to the .NET Foundation under one or more agreements.
//! The .NET Foundation licenses this file to you under the MIT license.
/*! bundlerFriendlyImports */
var e = false;
const t = async () => WebAssembly.validate(new Uint8Array([0, 97, 115, 109, 1, 0, 0, 0, 1, 4, 1, 96, 0, 0, 3, 2, 1, 0, 10, 8, 1, 6, 0, 6, 64, 25, 11, 11])), o = async () => WebAssembly.validate(new Uint8Array([0, 97, 115, 109, 1, 0, 0, 0, 1, 5, 1, 96, 0, 1, 123, 3, 2, 1, 0, 10, 15, 1, 13, 0, 65, 1, 253, 15, 65, 2, 253, 15, 253, 128, 2, 11])), n = async () => WebAssembly.validate(new Uint8Array([0, 97, 115, 109, 1, 0, 0, 0, 1, 5, 1, 96, 0, 1, 123, 3, 2, 1, 0, 10, 10, 1, 8, 0, 65, 0, 253, 15, 253, 98, 11])), r = /* @__PURE__ */ Symbol.for("wasm promise_control");
function i(e2, t2) {
  let o2 = null;
  const n2 = new Promise((function(n3, r2) {
    o2 = { isDone: false, promise: null, resolve: (t3) => {
      o2.isDone || (o2.isDone = true, n3(t3), e2 && e2());
    }, reject: (e3) => {
      o2.isDone || (o2.isDone = true, r2(e3), t2 && t2());
    } };
  }));
  o2.promise = n2;
  const i2 = n2;
  return i2[r] = o2, { promise: i2, promise_control: o2 };
}
function s(e2) {
  return e2[r];
}
function a(e2) {
  e2 && (function(e3) {
    return void 0 !== e3[r];
  })(e2) || Be(false, "Promise is not controllable");
}
const l = "__mono_message__", c = ["debug", "log", "trace", "warn", "info", "error"], d = "MONO_WASM: ";
let u, f, m, g, p, h;
function w(e2) {
  g = e2;
}
function b(e2) {
  if (Pe.diagnosticTracing) {
    const t2 = "function" == typeof e2 ? e2() : e2;
    console.debug(d + t2);
  }
}
function y(e2, ...t2) {
  console.info(d + e2, ...t2);
}
function v(e2, ...t2) {
  console.info(e2, ...t2);
}
function E(e2, ...t2) {
  console.warn(d + e2, ...t2);
}
function _(e2, ...t2) {
  if (t2 && t2.length > 0 && t2[0] && "object" == typeof t2[0]) {
    if (t2[0].silent) return;
    if (t2[0].toString) return void console.error(d + e2, t2[0].toString());
  }
  console.error(d + e2, ...t2);
}
function x(e2, t2, o2) {
  return function(...n2) {
    try {
      let r2 = n2[0];
      if (void 0 === r2) r2 = "undefined";
      else if (null === r2) r2 = "null";
      else if ("function" == typeof r2) r2 = r2.toString();
      else if ("string" != typeof r2) try {
        r2 = JSON.stringify(r2);
      } catch (e3) {
        r2 = r2.toString();
      }
      t2(o2 ? JSON.stringify({ method: e2, payload: r2, arguments: n2.slice(1) }) : [e2 + r2, ...n2.slice(1)]);
    } catch (e3) {
      m.error(`proxyConsole failed: ${e3}`);
    }
  };
}
function j(e2, t2, o2) {
  f = t2, g = e2, m = { ...t2 };
  const n2 = `${o2}/console`.replace("https://", "wss://").replace("http://", "ws://");
  u = new WebSocket(n2), u.addEventListener("error", A), u.addEventListener("close", S), (function() {
    for (const e3 of c) f[e3] = x(`console.${e3}`, T, true);
  })();
}
function R(e2) {
  let t2 = 30;
  const o2 = () => {
    u ? 0 == u.bufferedAmount || 0 == t2 ? (e2 && v(e2), (function() {
      for (const e3 of c) f[e3] = x(`console.${e3}`, m.log, false);
    })(), u.removeEventListener("error", A), u.removeEventListener("close", S), u.close(1e3, e2), u = void 0) : (t2--, globalThis.setTimeout(o2, 100)) : e2 && m && m.log(e2);
  };
  o2();
}
function T(e2) {
  u && u.readyState === WebSocket.OPEN ? u.send(e2) : m.log(e2);
}
function A(e2) {
  m.error(`[${g}] proxy console websocket error: ${e2}`, e2);
}
function S(e2) {
  m.debug(`[${g}] proxy console websocket closed: ${e2}`, e2);
}
function D() {
  Pe.preferredIcuAsset = O(Pe.config);
  let e2 = "invariant" == Pe.config.globalizationMode;
  if (!e2) if (Pe.preferredIcuAsset) Pe.diagnosticTracing && b("ICU data archive(s) available, disabling invariant mode");
  else {
    if ("custom" === Pe.config.globalizationMode || "all" === Pe.config.globalizationMode || "sharded" === Pe.config.globalizationMode) {
      const e3 = "invariant globalization mode is inactive and no ICU data archives are available";
      throw _(`ERROR: ${e3}`), new Error(e3);
    }
    Pe.diagnosticTracing && b("ICU data archive(s) not available, using invariant globalization mode"), e2 = true, Pe.preferredIcuAsset = null;
  }
  const t2 = "DOTNET_SYSTEM_GLOBALIZATION_INVARIANT", o2 = Pe.config.environmentVariables;
  if (void 0 === o2[t2] && e2 && (o2[t2] = "1"), void 0 === o2.TZ) try {
    const e3 = Intl.DateTimeFormat().resolvedOptions().timeZone || null;
    e3 && (o2.TZ = e3);
  } catch (e3) {
    y("failed to detect timezone, will fallback to UTC");
  }
}
function O(e2) {
  var t2;
  if ((null === (t2 = e2.resources) || void 0 === t2 ? void 0 : t2.icu) && "invariant" != e2.globalizationMode) {
    const t3 = e2.applicationCulture || (ke ? globalThis.navigator && globalThis.navigator.languages && globalThis.navigator.languages[0] : Intl.DateTimeFormat().resolvedOptions().locale), o2 = e2.resources.icu;
    let n2 = null;
    if ("custom" === e2.globalizationMode) {
      if (o2.length >= 1) return o2[0].name;
    } else t3 && "all" !== e2.globalizationMode ? "sharded" === e2.globalizationMode && (n2 = (function(e3) {
      const t4 = e3.split("-")[0];
      return "en" === t4 || ["fr", "fr-FR", "it", "it-IT", "de", "de-DE", "es", "es-ES"].includes(e3) ? "icudt_EFIGS.dat" : ["zh", "ko", "ja"].includes(t4) ? "icudt_CJK.dat" : "icudt_no_CJK.dat";
    })(t3)) : n2 = "icudt.dat";
    if (n2) for (let e3 = 0; e3 < o2.length; e3++) {
      const t4 = o2[e3];
      if (t4.virtualPath === n2) return t4.name;
    }
  }
  return e2.globalizationMode = "invariant", null;
}
(/* @__PURE__ */ new Date()).valueOf();
const C = class {
  constructor(e2) {
    this.url = e2;
  }
  toString() {
    return this.url;
  }
};
async function k(e2, t2) {
  try {
    const o2 = "function" == typeof globalThis.fetch;
    if (Se) {
      const n2 = e2.startsWith("file://");
      if (!n2 && o2) return globalThis.fetch(e2, t2 || { credentials: "same-origin" });
      p || (h = Ne.require("url"), p = Ne.require("fs")), n2 && (e2 = h.fileURLToPath(e2));
      const r2 = await p.promises.readFile(e2);
      return { ok: true, headers: { length: 0, get: () => null }, url: e2, arrayBuffer: () => r2, json: () => JSON.parse(r2), text: () => {
        throw new Error("NotImplementedException");
      } };
    }
    if (o2) return globalThis.fetch(e2, t2 || { credentials: "same-origin" });
    if ("function" == typeof read) return { ok: true, url: e2, headers: { length: 0, get: () => null }, arrayBuffer: () => new Uint8Array(read(e2, "binary")), json: () => JSON.parse(read(e2, "utf8")), text: () => read(e2, "utf8") };
  } catch (t3) {
    return { ok: false, url: e2, status: 500, headers: { length: 0, get: () => null }, statusText: "ERR28: " + t3, arrayBuffer: () => {
      throw t3;
    }, json: () => {
      throw t3;
    }, text: () => {
      throw t3;
    } };
  }
  throw new Error("No fetch implementation available");
}
function I(e2) {
  return "string" != typeof e2 && Be(false, "url must be a string"), !M(e2) && 0 !== e2.indexOf("./") && 0 !== e2.indexOf("../") && globalThis.URL && globalThis.document && globalThis.document.baseURI && (e2 = new URL(e2, globalThis.document.baseURI).toString()), e2;
}
const U = /^[a-zA-Z][a-zA-Z\d+\-.]*?:\/\//, P = /[a-zA-Z]:[\\/]/;
function M(e2) {
  return Se || Ie ? e2.startsWith("/") || e2.startsWith("\\") || -1 !== e2.indexOf("///") || P.test(e2) : U.test(e2);
}
let L, N = 0;
const $ = [], z = [], W = /* @__PURE__ */ new Map(), F = { "js-module-threads": true, "js-module-runtime": true, "js-module-dotnet": true, "js-module-native": true, "js-module-diagnostics": true }, B = { ...F, "js-module-library-initializer": true }, V = { ...F, dotnetwasm: true, heap: true, manifest: true }, q = { ...B, manifest: true }, H = { ...B, dotnetwasm: true }, J = { dotnetwasm: true, symbols: true }, Z = { ...B, dotnetwasm: true, symbols: true }, Q = { symbols: true };
function G(e2) {
  return !("icu" == e2.behavior && e2.name != Pe.preferredIcuAsset);
}
function K(e2, t2, o2) {
  null != t2 || (t2 = []), Be(1 == t2.length, `Expect to have one ${o2} asset in resources`);
  const n2 = t2[0];
  return n2.behavior = o2, X(n2), e2.push(n2), n2;
}
function X(e2) {
  V[e2.behavior] && W.set(e2.behavior, e2);
}
function Y(e2) {
  Be(V[e2], `Unknown single asset behavior ${e2}`);
  const t2 = W.get(e2);
  if (t2 && !t2.resolvedUrl) {
    if (t2.resolvedUrl = Pe.locateFile(t2.name), F[t2.behavior]) {
      const e3 = ge(t2);
      e3 ? ("string" != typeof e3 && Be(false, "loadBootResource response for 'dotnetjs' type should be a URL string"), t2.resolvedUrl = e3) : t2.resolvedUrl = ce(t2.resolvedUrl, t2.behavior);
    } else if ("dotnetwasm" !== t2.behavior) throw new Error(`Unknown single asset behavior ${e2}`);
  }
  return t2;
}
function ee(e2) {
  const t2 = Y(e2);
  return Be(t2, `Single asset for ${e2} not found`), t2;
}
let te = false;
async function oe() {
  if (!te) {
    te = true, Pe.diagnosticTracing && b("mono_download_assets");
    try {
      const e2 = [], t2 = [], o2 = (e3, t3) => {
        !Z[e3.behavior] && G(e3) && Pe.expected_instantiated_assets_count++, !H[e3.behavior] && G(e3) && (Pe.expected_downloaded_assets_count++, t3.push(se(e3)));
      };
      for (const t3 of $) o2(t3, e2);
      for (const e3 of z) o2(e3, t2);
      Pe.allDownloadsQueued.promise_control.resolve(), Promise.all([...e2, ...t2]).then((() => {
        Pe.allDownloadsFinished.promise_control.resolve();
      })).catch(((e3) => {
        throw Pe.err("Error in mono_download_assets: " + e3), Xe(1, e3), e3;
      })), await Pe.runtimeModuleLoaded.promise;
      const n2 = async (e3) => {
        const t3 = await e3;
        if (t3.buffer) {
          if (!Z[t3.behavior]) {
            t3.buffer && "object" == typeof t3.buffer || Be(false, "asset buffer must be array-like or buffer-like or promise of these"), "string" != typeof t3.resolvedUrl && Be(false, "resolvedUrl must be string");
            const e4 = t3.resolvedUrl, o3 = await t3.buffer, n3 = new Uint8Array(o3);
            pe(t3), await Ue.beforeOnRuntimeInitialized.promise, Ue.instantiate_asset(t3, e4, n3);
          }
        } else J[t3.behavior] ? ("symbols" === t3.behavior && (await Ue.instantiate_symbols_asset(t3), pe(t3)), J[t3.behavior] && ++Pe.actual_downloaded_assets_count) : (t3.isOptional || Be(false, "Expected asset to have the downloaded buffer"), !H[t3.behavior] && G(t3) && Pe.expected_downloaded_assets_count--, !Z[t3.behavior] && G(t3) && Pe.expected_instantiated_assets_count--);
      }, r2 = [], i2 = [];
      for (const t3 of e2) r2.push(n2(t3));
      for (const e3 of t2) i2.push(n2(e3));
      Promise.all(r2).then((() => {
        Ce || Ue.coreAssetsInMemory.promise_control.resolve();
      })).catch(((e3) => {
        throw Pe.err("Error in mono_download_assets: " + e3), Xe(1, e3), e3;
      })), Promise.all(i2).then((async () => {
        Ce || (await Ue.coreAssetsInMemory.promise, Ue.allAssetsInMemory.promise_control.resolve());
      })).catch(((e3) => {
        throw Pe.err("Error in mono_download_assets: " + e3), Xe(1, e3), e3;
      }));
    } catch (e2) {
      throw Pe.err("Error in mono_download_assets: " + e2), e2;
    }
  }
}
let ne = false;
function re() {
  if (ne) return;
  ne = true;
  const e2 = Pe.config, t2 = [];
  if (e2.assets) for (const t3 of e2.assets) "object" != typeof t3 && Be(false, `asset must be object, it was ${typeof t3} : ${t3}`), "string" != typeof t3.behavior && Be(false, "asset behavior must be known string"), "string" != typeof t3.name && Be(false, "asset name must be string"), t3.resolvedUrl && "string" != typeof t3.resolvedUrl && Be(false, "asset resolvedUrl could be string"), t3.hash && "string" != typeof t3.hash && Be(false, "asset resolvedUrl could be string"), t3.pendingDownload && "object" != typeof t3.pendingDownload && Be(false, "asset pendingDownload could be object"), t3.isCore ? $.push(t3) : z.push(t3), X(t3);
  else if (e2.resources) {
    const o2 = e2.resources;
    o2.wasmNative || Be(false, "resources.wasmNative must be defined"), o2.jsModuleNative || Be(false, "resources.jsModuleNative must be defined"), o2.jsModuleRuntime || Be(false, "resources.jsModuleRuntime must be defined"), K(z, o2.wasmNative, "dotnetwasm"), K(t2, o2.jsModuleNative, "js-module-native"), K(t2, o2.jsModuleRuntime, "js-module-runtime"), o2.jsModuleDiagnostics && K(t2, o2.jsModuleDiagnostics, "js-module-diagnostics");
    const n2 = (e3, t3, o3) => {
      const n3 = e3;
      n3.behavior = t3, o3 ? (n3.isCore = true, $.push(n3)) : z.push(n3);
    };
    if (o2.coreAssembly) for (let e3 = 0; e3 < o2.coreAssembly.length; e3++) n2(o2.coreAssembly[e3], "assembly", true);
    if (o2.assembly) for (let e3 = 0; e3 < o2.assembly.length; e3++) n2(o2.assembly[e3], "assembly", !o2.coreAssembly);
    if (0 != e2.debugLevel && Pe.isDebuggingSupported()) {
      if (o2.corePdb) for (let e3 = 0; e3 < o2.corePdb.length; e3++) n2(o2.corePdb[e3], "pdb", true);
      if (o2.pdb) for (let e3 = 0; e3 < o2.pdb.length; e3++) n2(o2.pdb[e3], "pdb", !o2.corePdb);
    }
    if (e2.loadAllSatelliteResources && o2.satelliteResources) for (const e3 in o2.satelliteResources) for (let t3 = 0; t3 < o2.satelliteResources[e3].length; t3++) {
      const r3 = o2.satelliteResources[e3][t3];
      r3.culture = e3, n2(r3, "resource", !o2.coreAssembly);
    }
    if (o2.coreVfs) for (let e3 = 0; e3 < o2.coreVfs.length; e3++) n2(o2.coreVfs[e3], "vfs", true);
    if (o2.vfs) for (let e3 = 0; e3 < o2.vfs.length; e3++) n2(o2.vfs[e3], "vfs", !o2.coreVfs);
    const r2 = O(e2);
    if (r2 && o2.icu) for (let e3 = 0; e3 < o2.icu.length; e3++) {
      const t3 = o2.icu[e3];
      t3.name === r2 && n2(t3, "icu", false);
    }
    if (o2.wasmSymbols) for (let e3 = 0; e3 < o2.wasmSymbols.length; e3++) n2(o2.wasmSymbols[e3], "symbols", false);
  }
  if (e2.appsettings) for (let t3 = 0; t3 < e2.appsettings.length; t3++) {
    const o2 = e2.appsettings[t3], n2 = he(o2);
    "appsettings.json" !== n2 && n2 !== `appsettings.${e2.applicationEnvironment}.json` || z.push({ name: o2, behavior: "vfs", cache: "no-cache", useCredentials: true });
  }
  e2.assets = [...$, ...z, ...t2];
}
async function ie(e2) {
  const t2 = await se(e2);
  return await t2.pendingDownloadInternal.response, t2.buffer;
}
async function se(e2) {
  try {
    return await ae(e2);
  } catch (t2) {
    if (!Pe.enableDownloadRetry) throw t2;
    if (Ie || Se) throw t2;
    if (e2.pendingDownload && e2.pendingDownloadInternal == e2.pendingDownload) throw t2;
    if (e2.resolvedUrl && -1 != e2.resolvedUrl.indexOf("file://")) throw t2;
    if (t2 && 404 == t2.status) throw t2;
    e2.pendingDownloadInternal = void 0, await Pe.allDownloadsQueued.promise;
    try {
      return Pe.diagnosticTracing && b(`Retrying download '${e2.name}'`), await ae(e2);
    } catch (t3) {
      return e2.pendingDownloadInternal = void 0, await new Promise(((e3) => globalThis.setTimeout(e3, 100))), Pe.diagnosticTracing && b(`Retrying download (2) '${e2.name}' after delay`), await ae(e2);
    }
  }
}
async function ae(e2) {
  for (; L; ) await L.promise;
  try {
    ++N, N == Pe.maxParallelDownloads && (Pe.diagnosticTracing && b("Throttling further parallel downloads"), L = i());
    const t2 = await (async function(e3) {
      if (e3.pendingDownload && (e3.pendingDownloadInternal = e3.pendingDownload), e3.pendingDownloadInternal && e3.pendingDownloadInternal.response) return e3.pendingDownloadInternal.response;
      if (e3.buffer) {
        const t4 = await e3.buffer;
        return e3.resolvedUrl || (e3.resolvedUrl = "undefined://" + e3.name), e3.pendingDownloadInternal = { url: e3.resolvedUrl, name: e3.name, response: Promise.resolve({ ok: true, arrayBuffer: () => t4, json: () => JSON.parse(new TextDecoder("utf-8").decode(t4)), text: () => {
          throw new Error("NotImplementedException");
        }, headers: { get: () => {
        } } }) }, e3.pendingDownloadInternal.response;
      }
      const t3 = e3.loadRemote && Pe.config.remoteSources ? Pe.config.remoteSources : [""];
      let o2;
      for (let n3 of t3) {
        n3 = n3.trim(), "./" === n3 && (n3 = "");
        const t4 = le(e3, n3);
        e3.name === t4 ? Pe.diagnosticTracing && b(`Attempting to download '${t4}'`) : Pe.diagnosticTracing && b(`Attempting to download '${t4}' for ${e3.name}`);
        try {
          e3.resolvedUrl = t4;
          const n4 = fe(e3);
          if (e3.pendingDownloadInternal = n4, o2 = await n4.response, !o2 || !o2.ok) continue;
          return o2;
        } catch (e4) {
          o2 || (o2 = { ok: false, url: t4, status: 0, statusText: "" + e4 });
          continue;
        }
      }
      const n2 = e3.isOptional || e3.name.match(/\.pdb$/) && Pe.config.ignorePdbLoadErrors;
      if (o2 || Be(false, `Response undefined ${e3.name}`), !n2) {
        const t4 = new Error(`download '${o2.url}' for ${e3.name} failed ${o2.status} ${o2.statusText}`);
        throw t4.status = o2.status, t4;
      }
      y(`optional download '${o2.url}' for ${e3.name} failed ${o2.status} ${o2.statusText}`);
    })(e2);
    return t2 ? (J[e2.behavior] || (e2.buffer = await t2.arrayBuffer(), ++Pe.actual_downloaded_assets_count), e2) : e2;
  } finally {
    if (--N, L && N == Pe.maxParallelDownloads - 1) {
      Pe.diagnosticTracing && b("Resuming more parallel downloads");
      const e3 = L;
      L = void 0, e3.promise_control.resolve();
    }
  }
}
function le(e2, t2) {
  let o2;
  return null == t2 && Be(false, `sourcePrefix must be provided for ${e2.name}`), e2.resolvedUrl ? o2 = e2.resolvedUrl : (o2 = "" === t2 ? "assembly" === e2.behavior || "pdb" === e2.behavior ? e2.name : "resource" === e2.behavior && e2.culture && "" !== e2.culture ? `${e2.culture}/${e2.name}` : e2.name : t2 + e2.name, o2 = ce(Pe.locateFile(o2), e2.behavior)), o2 && "string" == typeof o2 || Be(false, "attemptUrl need to be path or url string"), o2;
}
function ce(e2, t2) {
  return Pe.modulesUniqueQuery && q[t2] && (e2 += Pe.modulesUniqueQuery), e2;
}
let de = 0;
const ue = /* @__PURE__ */ new Set();
function fe(e2) {
  try {
    e2.resolvedUrl || Be(false, "Request's resolvedUrl must be set");
    const t2 = (function(e3) {
      let t3 = e3.resolvedUrl;
      if (Pe.loadBootResource) {
        const o4 = ge(e3);
        if (o4 instanceof Promise) return o4;
        "string" == typeof o4 && (t3 = o4);
      }
      const o3 = {};
      return e3.cache ? o3.cache = e3.cache : Pe.config.disableNoCacheFetch || (o3.cache = "no-cache"), e3.useCredentials ? o3.credentials = "include" : !Pe.config.disableIntegrityCheck && e3.hash && (o3.integrity = e3.hash), Pe.fetch_like(t3, o3);
    })(e2), o2 = { name: e2.name, url: e2.resolvedUrl, response: t2 };
    return ue.add(e2.name), o2.response.then((() => {
      "assembly" == e2.behavior && Pe.loadedAssemblies.push(e2.name), de++, Pe.onDownloadResourceProgress && Pe.onDownloadResourceProgress(de, ue.size);
    })), o2;
  } catch (t2) {
    const o2 = { ok: false, url: e2.resolvedUrl, status: 500, statusText: "ERR29: " + t2, arrayBuffer: () => {
      throw t2;
    }, json: () => {
      throw t2;
    } };
    return { name: e2.name, url: e2.resolvedUrl, response: Promise.resolve(o2) };
  }
}
const me = { resource: "assembly", assembly: "assembly", pdb: "pdb", icu: "globalization", vfs: "configuration", manifest: "manifest", dotnetwasm: "dotnetwasm", "js-module-dotnet": "dotnetjs", "js-module-native": "dotnetjs", "js-module-runtime": "dotnetjs", "js-module-threads": "dotnetjs" };
function ge(e2) {
  var t2;
  if (Pe.loadBootResource) {
    const o2 = null !== (t2 = e2.hash) && void 0 !== t2 ? t2 : "", n2 = e2.resolvedUrl, r2 = me[e2.behavior];
    if (r2) {
      const t3 = Pe.loadBootResource(r2, e2.name, n2, o2, e2.behavior);
      return "string" == typeof t3 ? I(t3) : t3;
    }
  }
}
function pe(e2) {
  e2.pendingDownloadInternal = null, e2.pendingDownload = null, e2.buffer = null, e2.moduleExports = null;
}
function he(e2) {
  let t2 = e2.lastIndexOf("/");
  return t2 >= 0 && t2++, e2.substring(t2);
}
async function we(e2) {
  e2 && await Promise.all((null != e2 ? e2 : []).map(((e3) => (async function(e4) {
    try {
      const t2 = e4.name;
      if (!e4.moduleExports) {
        const o2 = ce(Pe.locateFile(t2), "js-module-library-initializer");
        Pe.diagnosticTracing && b(`Attempting to import '${o2}' for ${e4}`), e4.moduleExports = await import(
          /*! webpackIgnore: true */
          o2
        );
      }
      Pe.libraryInitializers.push({ scriptName: t2, exports: e4.moduleExports });
    } catch (t2) {
      E(`Failed to import library initializer '${e4}': ${t2}`);
    }
  })(e3))));
}
async function be(e2, t2) {
  if (!Pe.libraryInitializers) return;
  const o2 = [];
  for (let n2 = 0; n2 < Pe.libraryInitializers.length; n2++) {
    const r2 = Pe.libraryInitializers[n2];
    r2.exports[e2] && o2.push(ye(r2.scriptName, e2, (() => r2.exports[e2](...t2))));
  }
  await Promise.all(o2);
}
async function ye(e2, t2, o2) {
  try {
    await o2();
  } catch (o3) {
    throw E(`Failed to invoke '${t2}' on library initializer '${e2}': ${o3}`), Xe(1, o3), o3;
  }
}
function ve(e2, t2) {
  if (e2 === t2) return e2;
  const o2 = { ...t2 };
  return void 0 !== o2.assets && o2.assets !== e2.assets && (o2.assets = [...e2.assets || [], ...o2.assets || []]), void 0 !== o2.resources && (o2.resources = _e(e2.resources || { assembly: [], jsModuleNative: [], jsModuleRuntime: [], wasmNative: [] }, o2.resources)), void 0 !== o2.environmentVariables && (o2.environmentVariables = { ...e2.environmentVariables || {}, ...o2.environmentVariables || {} }), void 0 !== o2.runtimeOptions && o2.runtimeOptions !== e2.runtimeOptions && (o2.runtimeOptions = [...e2.runtimeOptions || [], ...o2.runtimeOptions || []]), Object.assign(e2, o2);
}
function Ee(e2, t2) {
  if (e2 === t2) return e2;
  const o2 = { ...t2 };
  return o2.config && (e2.config || (e2.config = {}), o2.config = ve(e2.config, o2.config)), Object.assign(e2, o2);
}
function _e(e2, t2) {
  if (e2 === t2) return e2;
  const o2 = { ...t2 };
  return void 0 !== o2.coreAssembly && (o2.coreAssembly = [...e2.coreAssembly || [], ...o2.coreAssembly || []]), void 0 !== o2.assembly && (o2.assembly = [...e2.assembly || [], ...o2.assembly || []]), void 0 !== o2.lazyAssembly && (o2.lazyAssembly = [...e2.lazyAssembly || [], ...o2.lazyAssembly || []]), void 0 !== o2.corePdb && (o2.corePdb = [...e2.corePdb || [], ...o2.corePdb || []]), void 0 !== o2.pdb && (o2.pdb = [...e2.pdb || [], ...o2.pdb || []]), void 0 !== o2.jsModuleWorker && (o2.jsModuleWorker = [...e2.jsModuleWorker || [], ...o2.jsModuleWorker || []]), void 0 !== o2.jsModuleNative && (o2.jsModuleNative = [...e2.jsModuleNative || [], ...o2.jsModuleNative || []]), void 0 !== o2.jsModuleDiagnostics && (o2.jsModuleDiagnostics = [...e2.jsModuleDiagnostics || [], ...o2.jsModuleDiagnostics || []]), void 0 !== o2.jsModuleRuntime && (o2.jsModuleRuntime = [...e2.jsModuleRuntime || [], ...o2.jsModuleRuntime || []]), void 0 !== o2.wasmSymbols && (o2.wasmSymbols = [...e2.wasmSymbols || [], ...o2.wasmSymbols || []]), void 0 !== o2.wasmNative && (o2.wasmNative = [...e2.wasmNative || [], ...o2.wasmNative || []]), void 0 !== o2.icu && (o2.icu = [...e2.icu || [], ...o2.icu || []]), void 0 !== o2.satelliteResources && (o2.satelliteResources = (function(e3, t3) {
    if (e3 === t3) return e3;
    for (const o3 in t3) e3[o3] = [...e3[o3] || [], ...t3[o3] || []];
    return e3;
  })(e2.satelliteResources || {}, o2.satelliteResources || {})), void 0 !== o2.modulesAfterConfigLoaded && (o2.modulesAfterConfigLoaded = [...e2.modulesAfterConfigLoaded || [], ...o2.modulesAfterConfigLoaded || []]), void 0 !== o2.modulesAfterRuntimeReady && (o2.modulesAfterRuntimeReady = [...e2.modulesAfterRuntimeReady || [], ...o2.modulesAfterRuntimeReady || []]), void 0 !== o2.extensions && (o2.extensions = { ...e2.extensions || {}, ...o2.extensions || {} }), void 0 !== o2.vfs && (o2.vfs = [...e2.vfs || [], ...o2.vfs || []]), Object.assign(e2, o2);
}
function xe() {
  const e2 = Pe.config;
  if (e2.environmentVariables = e2.environmentVariables || {}, e2.runtimeOptions = e2.runtimeOptions || [], e2.resources = e2.resources || { assembly: [], jsModuleNative: [], jsModuleWorker: [], jsModuleRuntime: [], wasmNative: [], vfs: [], satelliteResources: {} }, e2.assets) {
    Pe.diagnosticTracing && b("config.assets is deprecated, use config.resources instead");
    for (const t2 of e2.assets) {
      const o2 = {};
      switch (t2.behavior) {
        case "assembly":
          o2.assembly = [t2];
          break;
        case "pdb":
          o2.pdb = [t2];
          break;
        case "resource":
          o2.satelliteResources = {}, o2.satelliteResources[t2.culture] = [t2];
          break;
        case "icu":
          o2.icu = [t2];
          break;
        case "symbols":
          o2.wasmSymbols = [t2];
          break;
        case "vfs":
          o2.vfs = [t2];
          break;
        case "dotnetwasm":
          o2.wasmNative = [t2];
          break;
        case "js-module-threads":
          o2.jsModuleWorker = [t2];
          break;
        case "js-module-runtime":
          o2.jsModuleRuntime = [t2];
          break;
        case "js-module-native":
          o2.jsModuleNative = [t2];
          break;
        case "js-module-diagnostics":
          o2.jsModuleDiagnostics = [t2];
          break;
        case "js-module-dotnet":
          break;
        default:
          throw new Error(`Unexpected behavior ${t2.behavior} of asset ${t2.name}`);
      }
      _e(e2.resources, o2);
    }
  }
  e2.debugLevel, e2.applicationEnvironment || (e2.applicationEnvironment = "Production"), e2.applicationCulture && (e2.environmentVariables.LANG = `${e2.applicationCulture}.UTF-8`), Ue.diagnosticTracing = Pe.diagnosticTracing = !!e2.diagnosticTracing, Ue.waitForDebugger = e2.waitForDebugger, Pe.maxParallelDownloads = e2.maxParallelDownloads || Pe.maxParallelDownloads, Pe.enableDownloadRetry = void 0 !== e2.enableDownloadRetry ? e2.enableDownloadRetry : Pe.enableDownloadRetry;
}
let je = false;
async function Re(e2) {
  var t2;
  if (je) return void await Pe.afterConfigLoaded.promise;
  let o2;
  try {
    if (e2.configSrc || Pe.config && 0 !== Object.keys(Pe.config).length && (Pe.config.assets || Pe.config.resources) || (e2.configSrc = "dotnet.boot.js"), o2 = e2.configSrc, je = true, o2 && (Pe.diagnosticTracing && b("mono_wasm_load_config"), await (async function(e3) {
      const t3 = e3.configSrc, o3 = Pe.locateFile(t3);
      let n2 = null;
      void 0 !== Pe.loadBootResource && (n2 = Pe.loadBootResource("manifest", t3, o3, "", "manifest"));
      let r2, i2 = null;
      if (n2) if ("string" == typeof n2) n2.includes(".json") ? (i2 = await s2(I(n2)), r2 = await Ae(i2)) : r2 = (await import(I(n2))).config;
      else {
        const e4 = await n2;
        "function" == typeof e4.json ? (i2 = e4, r2 = await Ae(i2)) : r2 = e4.config;
      }
      else o3.includes(".json") ? (i2 = await s2(ce(o3, "manifest")), r2 = await Ae(i2)) : r2 = (await import(ce(o3, "manifest"))).config;
      function s2(e4) {
        return Pe.fetch_like(e4, { method: "GET", credentials: "include", cache: "no-cache" });
      }
      Pe.config.applicationEnvironment && (r2.applicationEnvironment = Pe.config.applicationEnvironment), ve(Pe.config, r2);
    })(e2)), xe(), await we(null === (t2 = Pe.config.resources) || void 0 === t2 ? void 0 : t2.modulesAfterConfigLoaded), await be("onRuntimeConfigLoaded", [Pe.config]), e2.onConfigLoaded) try {
      await e2.onConfigLoaded(Pe.config, Le), xe();
    } catch (e3) {
      throw _("onConfigLoaded() failed", e3), e3;
    }
    xe(), Pe.afterConfigLoaded.promise_control.resolve(Pe.config);
  } catch (t3) {
    const n2 = `Failed to load config file ${o2} ${t3} ${null == t3 ? void 0 : t3.stack}`;
    throw Pe.config = e2.config = Object.assign(Pe.config, { message: n2, error: t3, isError: true }), Xe(1, new Error(n2)), t3;
  }
}
function Te() {
  return !!globalThis.navigator && (Pe.isChromium || Pe.isFirefox);
}
async function Ae(e2) {
  const t2 = Pe.config, o2 = await e2.json();
  t2.applicationEnvironment || o2.applicationEnvironment || (o2.applicationEnvironment = e2.headers.get("Blazor-Environment") || e2.headers.get("DotNet-Environment") || void 0), o2.environmentVariables || (o2.environmentVariables = {});
  const n2 = e2.headers.get("DOTNET-MODIFIABLE-ASSEMBLIES");
  n2 && (o2.environmentVariables.DOTNET_MODIFIABLE_ASSEMBLIES = n2);
  const r2 = e2.headers.get("ASPNETCORE-BROWSER-TOOLS");
  return r2 && (o2.environmentVariables.__ASPNETCORE_BROWSER_TOOLS = r2), o2;
}
"function" != typeof importScripts || globalThis.onmessage || (globalThis.dotnetSidecar = true);
const Se = "object" == typeof process && "object" == typeof process.versions && "string" == typeof process.versions.node, De = "function" == typeof importScripts, Oe = De && "undefined" != typeof dotnetSidecar, Ce = De && !Oe, ke = "object" == typeof window || De && !Se, Ie = !ke && !Se;
let Ue = {}, Pe = {}, Me = {}, Le = {}, Ne = {}, $e = false;
const ze = {}, We = { config: ze }, Fe = { mono: {}, binding: {}, internal: Ne, module: We, loaderHelpers: Pe, runtimeHelpers: Ue, diagnosticHelpers: Me, api: Le };
function Be(e2, t2) {
  if (e2) return;
  const o2 = "Assert failed: " + ("function" == typeof t2 ? t2() : t2), n2 = new Error(o2);
  _(o2, n2), Ue.nativeAbort(n2);
}
function Ve() {
  return void 0 !== Pe.exitCode;
}
function qe() {
  return Ue.runtimeReady && !Ve();
}
function He() {
  Ve() && Be(false, `.NET runtime already exited with ${Pe.exitCode} ${Pe.exitReason}. You can use runtime.runMain() which doesn't exit the runtime.`), Ue.runtimeReady || Be(false, ".NET runtime didn't start yet. Please call dotnet.create() first.");
}
function Je() {
  ke && (globalThis.addEventListener("unhandledrejection", et), globalThis.addEventListener("error", tt));
}
let Ze, Qe;
function Ge(e2) {
  Qe && Qe(e2), Xe(e2, Pe.exitReason);
}
function Ke(e2) {
  Ze && Ze(e2 || Pe.exitReason), Xe(1, e2 || Pe.exitReason);
}
function Xe(t2, o2) {
  var n2, r2;
  const i2 = o2 && "object" == typeof o2;
  t2 = i2 && "number" == typeof o2.status ? o2.status : void 0 === t2 ? -1 : t2;
  const s2 = i2 && "string" == typeof o2.message ? o2.message : "" + o2;
  (o2 = i2 ? o2 : Ue.ExitStatus ? (function(e2, t3) {
    const o3 = new Ue.ExitStatus(e2);
    return o3.message = t3, o3.toString = () => t3, o3;
  })(t2, s2) : new Error("Exit with code " + t2 + " " + s2)).status = t2, o2.message || (o2.message = s2);
  const a2 = "" + (o2.stack || new Error().stack);
  try {
    Object.defineProperty(o2, "stack", { get: () => a2 });
  } catch (e2) {
  }
  const l2 = !!o2.silent;
  if (o2.silent = true, Ve()) Pe.diagnosticTracing && b("mono_exit called after exit");
  else {
    try {
      We.onAbort == Ke && (We.onAbort = Ze), We.onExit == Ge && (We.onExit = Qe), ke && (globalThis.removeEventListener("unhandledrejection", et), globalThis.removeEventListener("error", tt)), Ue.runtimeReady ? (Ue.jiterpreter_dump_stats && Ue.jiterpreter_dump_stats(false), 0 === t2 && (null === (n2 = Pe.config) || void 0 === n2 ? void 0 : n2.interopCleanupOnExit) && Ue.forceDisposeProxies(true, true), e && 0 !== t2 && (null === (r2 = Pe.config) || void 0 === r2 || r2.dumpThreadsOnNonZeroExit)) : (Pe.diagnosticTracing && b(`abort_startup, reason: ${o2}`), (function(e2) {
        Pe.allDownloadsQueued.promise_control.reject(e2), Pe.allDownloadsFinished.promise_control.reject(e2), Pe.afterConfigLoaded.promise_control.reject(e2), Pe.wasmCompilePromise.promise_control.reject(e2), Pe.runtimeModuleLoaded.promise_control.reject(e2), Ue.dotnetReady && (Ue.dotnetReady.promise_control.reject(e2), Ue.afterInstantiateWasm.promise_control.reject(e2), Ue.beforePreInit.promise_control.reject(e2), Ue.afterPreInit.promise_control.reject(e2), Ue.afterPreRun.promise_control.reject(e2), Ue.beforeOnRuntimeInitialized.promise_control.reject(e2), Ue.afterOnRuntimeInitialized.promise_control.reject(e2), Ue.afterPostRun.promise_control.reject(e2));
      })(o2));
    } catch (e2) {
      E("mono_exit A failed", e2);
    }
    try {
      l2 || ((function(e2, t3) {
        if (0 !== e2 && t3) {
          const e3 = Ue.ExitStatus && t3 instanceof Ue.ExitStatus ? b : _;
          "string" == typeof t3 ? e3(t3) : (void 0 === t3.stack && (t3.stack = new Error().stack + ""), t3.message ? e3(Ue.stringify_as_error_with_stack ? Ue.stringify_as_error_with_stack(t3.message + "\n" + t3.stack) : t3.message + "\n" + t3.stack) : e3(JSON.stringify(t3)));
        }
        !Ce && Pe.config && (Pe.config.logExitCode ? Pe.config.forwardConsoleLogsToWS ? R("WASM EXIT " + e2) : v("WASM EXIT " + e2) : Pe.config.forwardConsoleLogsToWS && R());
      })(t2, o2), (function(e2) {
        if (ke && !Ce && Pe.config && Pe.config.appendElementOnExit && document) {
          const t3 = document.createElement("label");
          t3.id = "tests_done", 0 !== e2 && (t3.style.background = "red"), t3.innerHTML = "" + e2, document.body.appendChild(t3);
        }
      })(t2));
    } catch (e2) {
      E("mono_exit B failed", e2);
    }
    Pe.exitCode = t2, Pe.exitReason || (Pe.exitReason = o2), !Ce && Ue.runtimeReady && We.runtimeKeepalivePop();
  }
  if (Pe.config && Pe.config.asyncFlushOnExit && 0 === t2) throw (async () => {
    try {
      await (async function() {
        try {
          const e2 = await import(
            /*! webpackIgnore: true */
            "process"
          ), t3 = (e3) => new Promise(((t4, o4) => {
            e3.on("error", o4), e3.end("", "utf8", t4);
          })), o3 = t3(e2.stderr), n3 = t3(e2.stdout);
          let r3;
          const i3 = new Promise(((e3) => {
            r3 = setTimeout((() => e3("timeout")), 1e3);
          }));
          await Promise.race([Promise.all([n3, o3]), i3]), clearTimeout(r3);
        } catch (e2) {
          _(`flushing std* streams failed: ${e2}`);
        }
      })();
    } finally {
      Ye(t2, o2);
    }
  })(), o2;
  Ye(t2, o2);
}
function Ye(e2, t2) {
  if (Ue.runtimeReady && Ue.nativeExit) try {
    Ue.nativeExit(e2);
  } catch (e3) {
    !Ue.ExitStatus || e3 instanceof Ue.ExitStatus || E("set_exit_code_and_quit_now failed: " + e3.toString());
  }
  if (0 !== e2 || !ke) throw Se && Ne.process ? Ne.process.exit(e2) : Ue.quit && Ue.quit(e2, t2), t2;
}
function et(e2) {
  ot(e2, e2.reason, "rejection");
}
function tt(e2) {
  ot(e2, e2.error, "error");
}
function ot(e2, t2, o2) {
  e2.preventDefault();
  try {
    t2 || (t2 = new Error("Unhandled " + o2)), void 0 === t2.stack && (t2.stack = new Error().stack), t2.stack = t2.stack + "", t2.silent || (_("Unhandled error:", t2), Xe(1, t2));
  } catch (e3) {
  }
}
!(function(e2) {
  if ($e) throw new Error("Loader module already loaded");
  $e = true, Ue = e2.runtimeHelpers, Pe = e2.loaderHelpers, Me = e2.diagnosticHelpers, Le = e2.api, Ne = e2.internal, Object.assign(Le, { INTERNAL: Ne, invokeLibraryInitializers: be }), Object.assign(e2.module, { config: ve(ze, { environmentVariables: {} }) });
  const r2 = { mono_wasm_bindings_is_ready: false, config: e2.module.config, diagnosticTracing: false, nativeAbort: (e3) => {
    throw e3 || new Error("abort");
  }, nativeExit: (e3) => {
    throw new Error("exit:" + e3);
  } }, l2 = { gitHash: "0427b9abbead3fa64c6655eb4290c537397131fa", config: e2.module.config, diagnosticTracing: false, maxParallelDownloads: 16, enableDownloadRetry: true, _loaded_files: [], loadedFiles: [], loadedAssemblies: [], libraryInitializers: [], workerNextNumber: 1, actual_downloaded_assets_count: 0, actual_instantiated_assets_count: 0, expected_downloaded_assets_count: 0, expected_instantiated_assets_count: 0, afterConfigLoaded: i(), allDownloadsQueued: i(), allDownloadsFinished: i(), wasmCompilePromise: i(), runtimeModuleLoaded: i(), loadingWorkers: i(), is_exited: Ve, is_runtime_running: qe, assert_runtime_running: He, mono_exit: Xe, createPromiseController: i, getPromiseController: s, assertIsControllablePromise: a, mono_download_assets: oe, resolve_single_asset_path: ee, setup_proxy_console: j, set_thread_prefix: w, installUnhandledErrorHandler: Je, retrieve_asset_download: ie, invokeLibraryInitializers: be, isDebuggingSupported: Te, exceptions: t, simd: n, relaxedSimd: o };
  Object.assign(Ue, r2), Object.assign(Pe, l2);
})(Fe);
let nt, rt, it, st = false, at = false;
async function lt(e2) {
  if (!at) {
    if (at = true, ke && Pe.config.forwardConsoleLogsToWS && void 0 !== globalThis.WebSocket && j("main", globalThis.console, globalThis.location.origin), We || Be(false, "Null moduleConfig"), Pe.config || Be(false, "Null moduleConfig.config"), "function" == typeof e2) {
      const t2 = e2(Fe.api);
      if (t2.ready) throw new Error("Module.ready couldn't be redefined.");
      Object.assign(We, t2), Ee(We, t2);
    } else {
      if ("object" != typeof e2) throw new Error("Can't use moduleFactory callback of createDotnetRuntime function.");
      Ee(We, e2);
    }
    await (async function(e3) {
      if (Se) {
        const e4 = await import(
          /*! webpackIgnore: true */
          "process"
        ), t3 = 14;
        if (e4.versions.node.split(".")[0] < t3) throw new Error(`NodeJS at '${e4.execPath}' has too low version '${e4.versions.node}', please use at least ${t3}. See also https://aka.ms/dotnet-wasm-features`);
      }
      const t2 = (
        /*! webpackIgnore: true */
        "https://usr/dotnet/dotnet.js"
      ), o2 = t2.indexOf("?");
      var n2;
      if (o2 > 0 && (Pe.modulesUniqueQuery = t2.substring(o2)), Pe.scriptUrl = t2.replace(/\\/g, "/").replace(/[?#].*/, ""), Pe.scriptDirectory = (n2 = Pe.scriptUrl).slice(0, n2.lastIndexOf("/")) + "/", Pe.locateFile = (e4) => "URL" in globalThis && globalThis.URL !== C ? new URL(e4, Pe.scriptDirectory).toString() : M(e4) ? e4 : Pe.scriptDirectory + e4, Pe.fetch_like = k, Pe.out = console.log, Pe.err = console.error, Pe.onDownloadResourceProgress = e3.onDownloadResourceProgress, ke && globalThis.navigator) {
        const e4 = globalThis.navigator, t3 = e4.userAgentData && e4.userAgentData.brands;
        t3 && t3.length > 0 ? Pe.isChromium = t3.some(((e5) => "Google Chrome" === e5.brand || "Microsoft Edge" === e5.brand || "Chromium" === e5.brand)) : e4.userAgent && (Pe.isChromium = e4.userAgent.includes("Chrome"), Pe.isFirefox = e4.userAgent.includes("Firefox"));
      }
      Ne.require = Se ? await import(
        /*! webpackIgnore: true */
        "module"
      ).then(((e4) => e4.createRequire(
        /*! webpackIgnore: true */
        "https://usr/dotnet/dotnet.js"
      ))) : Promise.resolve((() => {
        throw new Error("require not supported");
      })), void 0 === globalThis.URL && (globalThis.URL = C);
    })(We);
  }
}
async function ct(e2) {
  return await lt(e2), Ze = We.onAbort, Qe = We.onExit, We.onAbort = Ke, We.onExit = Ge, We.ENVIRONMENT_IS_PTHREAD ? (async function() {
    (function() {
      const e4 = new MessageChannel(), t3 = e4.port1, o2 = e4.port2;
      t3.addEventListener("message", ((e5) => {
        var n2, r2;
        n2 = JSON.parse(e5.data.config), r2 = JSON.parse(e5.data.monoThreadInfo), st ? Pe.diagnosticTracing && b("mono config already received") : (ve(Pe.config, n2), Ue.monoThreadInfo = r2, xe(), Pe.diagnosticTracing && b("mono config received"), st = true, Pe.afterConfigLoaded.promise_control.resolve(Pe.config), ke && n2.forwardConsoleLogsToWS && void 0 !== globalThis.WebSocket && Pe.setup_proxy_console("worker-idle", console, globalThis.location.origin)), t3.close(), o2.close();
      }), { once: true }), t3.start(), self.postMessage({ [l]: { monoCmd: "preload", port: o2 } }, [o2]);
    })(), await Pe.afterConfigLoaded.promise, (function() {
      const e4 = Pe.config;
      e4.assets || Be(false, "config.assets must be defined");
      for (const t3 of e4.assets) X(t3), Q[t3.behavior] && z.push(t3);
    })(), setTimeout((async () => {
      try {
        await oe();
      } catch (e4) {
        Xe(1, e4);
      }
    }), 0);
    const e3 = dt(), t2 = await Promise.all(e3);
    return await ut(t2), We;
  })() : (async function() {
    var e3;
    await Re(We), re();
    const t2 = dt();
    (async function() {
      try {
        const e4 = ee("dotnetwasm");
        await se(e4), e4 && e4.pendingDownloadInternal && e4.pendingDownloadInternal.response || Be(false, "Can't load dotnet.native.wasm");
        const t3 = await e4.pendingDownloadInternal.response, o3 = t3.headers && t3.headers.get ? t3.headers.get("Content-Type") : void 0;
        let n2;
        if ("function" == typeof WebAssembly.compileStreaming && "application/wasm" === o3) n2 = await WebAssembly.compileStreaming(t3);
        else {
          ke && "application/wasm" !== o3 && E('WebAssembly resource does not have the expected content type "application/wasm", so falling back to slower ArrayBuffer instantiation.');
          const e5 = await t3.arrayBuffer();
          Pe.diagnosticTracing && b("instantiate_wasm_module buffered"), n2 = Ie ? await Promise.resolve(new WebAssembly.Module(e5)) : await WebAssembly.compile(e5);
        }
        e4.pendingDownloadInternal = null, e4.pendingDownload = null, e4.buffer = null, e4.moduleExports = null, Pe.wasmCompilePromise.promise_control.resolve(n2);
      } catch (e4) {
        Pe.wasmCompilePromise.promise_control.reject(e4);
      }
    })(), setTimeout((async () => {
      try {
        D(), await oe();
      } catch (e4) {
        Xe(1, e4);
      }
    }), 0);
    const o2 = await Promise.all(t2);
    return await ut(o2), await Ue.dotnetReady.promise, await we(null === (e3 = Pe.config.resources) || void 0 === e3 ? void 0 : e3.modulesAfterRuntimeReady), await be("onRuntimeReady", [Fe.api]), Le;
  })();
}
function dt() {
  const e2 = ee("js-module-runtime"), t2 = ee("js-module-native");
  if (nt && rt) return [nt, rt, it];
  "object" == typeof e2.moduleExports ? nt = e2.moduleExports : (Pe.diagnosticTracing && b(`Attempting to import '${e2.resolvedUrl}' for ${e2.name}`), nt = import(
    /*! webpackIgnore: true */
    e2.resolvedUrl
  )), "object" == typeof t2.moduleExports ? rt = t2.moduleExports : (Pe.diagnosticTracing && b(`Attempting to import '${t2.resolvedUrl}' for ${t2.name}`), rt = import(
    /*! webpackIgnore: true */
    t2.resolvedUrl
  ));
  const o2 = Y("js-module-diagnostics");
  return o2 && ("object" == typeof o2.moduleExports ? it = o2.moduleExports : (Pe.diagnosticTracing && b(`Attempting to import '${o2.resolvedUrl}' for ${o2.name}`), it = import(
    /*! webpackIgnore: true */
    o2.resolvedUrl
  ))), [nt, rt, it];
}
async function ut(e2) {
  const { initializeExports: t2, initializeReplacements: o2, configureRuntimeStartup: n2, configureEmscriptenStartup: r2, configureWorkerStartup: i2, setRuntimeGlobals: s2, passEmscriptenInternals: a2 } = e2[0], { default: l2 } = e2[1], c2 = e2[2];
  s2(Fe), t2(Fe), c2 && c2.setRuntimeGlobals(Fe), await n2(We), Pe.runtimeModuleLoaded.promise_control.resolve(), l2(((e3) => (Object.assign(We, { ready: e3.ready, __dotnet_runtime: { initializeReplacements: o2, configureEmscriptenStartup: r2, configureWorkerStartup: i2, passEmscriptenInternals: a2 } }), We))).catch(((e3) => {
    if (e3.message && e3.message.toLowerCase().includes("out of memory")) throw new Error(".NET runtime has failed to start, because too much memory was requested. Please decrease the memory by adjusting EmccMaximumHeapSize. See also https://aka.ms/dotnet-wasm-features");
    throw e3;
  }));
}
const ft = new class {
  withModuleConfig(e2) {
    try {
      return Ee(We, e2), this;
    } catch (e3) {
      throw Xe(1, e3), e3;
    }
  }
  withOnConfigLoaded(e2) {
    try {
      return Ee(We, { onConfigLoaded: e2 }), this;
    } catch (e3) {
      throw Xe(1, e3), e3;
    }
  }
  withConsoleForwarding() {
    try {
      return ve(ze, { forwardConsoleLogsToWS: true }), this;
    } catch (e2) {
      throw Xe(1, e2), e2;
    }
  }
  withExitOnUnhandledError() {
    try {
      return ve(ze, { exitOnUnhandledError: true }), Je(), this;
    } catch (e2) {
      throw Xe(1, e2), e2;
    }
  }
  withAsyncFlushOnExit() {
    try {
      return ve(ze, { asyncFlushOnExit: true }), this;
    } catch (e2) {
      throw Xe(1, e2), e2;
    }
  }
  withExitCodeLogging() {
    try {
      return ve(ze, { logExitCode: true }), this;
    } catch (e2) {
      throw Xe(1, e2), e2;
    }
  }
  withElementOnExit() {
    try {
      return ve(ze, { appendElementOnExit: true }), this;
    } catch (e2) {
      throw Xe(1, e2), e2;
    }
  }
  withInteropCleanupOnExit() {
    try {
      return ve(ze, { interopCleanupOnExit: true }), this;
    } catch (e2) {
      throw Xe(1, e2), e2;
    }
  }
  withDumpThreadsOnNonZeroExit() {
    try {
      return ve(ze, { dumpThreadsOnNonZeroExit: true }), this;
    } catch (e2) {
      throw Xe(1, e2), e2;
    }
  }
  withWaitingForDebugger(e2) {
    try {
      return ve(ze, { waitForDebugger: e2 }), this;
    } catch (e3) {
      throw Xe(1, e3), e3;
    }
  }
  withInterpreterPgo(e2, t2) {
    try {
      return ve(ze, { interpreterPgo: e2, interpreterPgoSaveDelay: t2 }), ze.runtimeOptions ? ze.runtimeOptions.push("--interp-pgo-recording") : ze.runtimeOptions = ["--interp-pgo-recording"], this;
    } catch (e3) {
      throw Xe(1, e3), e3;
    }
  }
  withConfig(e2) {
    try {
      return ve(ze, e2), this;
    } catch (e3) {
      throw Xe(1, e3), e3;
    }
  }
  withConfigSrc(e2) {
    try {
      return e2 && "string" == typeof e2 || Be(false, "must be file path or URL"), Ee(We, { configSrc: e2 }), this;
    } catch (e3) {
      throw Xe(1, e3), e3;
    }
  }
  withVirtualWorkingDirectory(e2) {
    try {
      return e2 && "string" == typeof e2 || Be(false, "must be directory path"), ve(ze, { virtualWorkingDirectory: e2 }), this;
    } catch (e3) {
      throw Xe(1, e3), e3;
    }
  }
  withEnvironmentVariable(e2, t2) {
    try {
      const o2 = {};
      return o2[e2] = t2, ve(ze, { environmentVariables: o2 }), this;
    } catch (e3) {
      throw Xe(1, e3), e3;
    }
  }
  withEnvironmentVariables(e2) {
    try {
      return e2 && "object" == typeof e2 || Be(false, "must be dictionary object"), ve(ze, { environmentVariables: e2 }), this;
    } catch (e3) {
      throw Xe(1, e3), e3;
    }
  }
  withDiagnosticTracing(e2) {
    try {
      return "boolean" != typeof e2 && Be(false, "must be boolean"), ve(ze, { diagnosticTracing: e2 }), this;
    } catch (e3) {
      throw Xe(1, e3), e3;
    }
  }
  withDebugging(e2) {
    try {
      return null != e2 && "number" == typeof e2 || Be(false, "must be number"), ve(ze, { debugLevel: e2 }), this;
    } catch (e3) {
      throw Xe(1, e3), e3;
    }
  }
  withApplicationArguments(...e2) {
    try {
      return e2 && Array.isArray(e2) || Be(false, "must be array of strings"), ve(ze, { applicationArguments: e2 }), this;
    } catch (e3) {
      throw Xe(1, e3), e3;
    }
  }
  withRuntimeOptions(e2) {
    try {
      return e2 && Array.isArray(e2) || Be(false, "must be array of strings"), ze.runtimeOptions ? ze.runtimeOptions.push(...e2) : ze.runtimeOptions = e2, this;
    } catch (e3) {
      throw Xe(1, e3), e3;
    }
  }
  withMainAssembly(e2) {
    try {
      return ve(ze, { mainAssemblyName: e2 }), this;
    } catch (e3) {
      throw Xe(1, e3), e3;
    }
  }
  withApplicationArgumentsFromQuery() {
    try {
      if (!globalThis.window) throw new Error("Missing window to the query parameters from");
      if (void 0 === globalThis.URLSearchParams) throw new Error("URLSearchParams is supported");
      const e2 = new URLSearchParams(globalThis.window.location.search).getAll("arg");
      return this.withApplicationArguments(...e2);
    } catch (e2) {
      throw Xe(1, e2), e2;
    }
  }
  withApplicationEnvironment(e2) {
    try {
      return ve(ze, { applicationEnvironment: e2 }), this;
    } catch (e3) {
      throw Xe(1, e3), e3;
    }
  }
  withApplicationCulture(e2) {
    try {
      return ve(ze, { applicationCulture: e2 }), this;
    } catch (e3) {
      throw Xe(1, e3), e3;
    }
  }
  withResourceLoader(e2) {
    try {
      return Pe.loadBootResource = e2, this;
    } catch (e3) {
      throw Xe(1, e3), e3;
    }
  }
  async download() {
    try {
      await (async function() {
        lt(We), await Re(We), re(), D(), oe(), await Pe.allDownloadsFinished.promise;
      })();
    } catch (e2) {
      throw Xe(1, e2), e2;
    }
  }
  async create() {
    try {
      return this.instance || (this.instance = await (async function() {
        return await ct(We), Fe.api;
      })()), this.instance;
    } catch (e2) {
      throw Xe(1, e2), e2;
    }
  }
  async run() {
    try {
      return We.config || Be(false, "Null moduleConfig.config"), this.instance || await this.create(), this.instance.runMainAndExit();
    } catch (e2) {
      throw Xe(1, e2), e2;
    }
  }
}(), mt = Xe, gt = ct;
Ie || "function" == typeof globalThis.URL || Be(false, "This browser/engine doesn't support URL API. Please use a modern version. See also https://aka.ms/dotnet-wasm-features"), "function" != typeof globalThis.BigInt64Array && Be(false, "This browser/engine doesn't support BigInt64Array API. Please use a modern version. See also https://aka.ms/dotnet-wasm-features"), ft.withConfig(
  /*! dotnetBootConfig */
  {}
);
