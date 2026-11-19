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
var __copyProps = (to2, from, except, desc) => {
  if (from && typeof from === "object" || typeof from === "function") {
    for (let key of __getOwnPropNames(from))
      if (!__hasOwnProp.call(to2, key) && key !== except)
        __defProp(to2, key, { get: () => from[key], enumerable: !(desc = __getOwnPropDesc(from, key)) || desc.enumerable });
  }
  return to2;
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
var dotnet_runtime_exports = {};
__export(dotnet_runtime_exports, {
  configureEmscriptenStartup: () => Yc,
  configureRuntimeStartup: () => Qc,
  configureWorkerStartup: () => el,
  initializeExports: () => nl,
  initializeReplacements: () => Ao,
  passEmscriptenInternals: () => dt,
  runtimeList: () => tl,
  setRuntimeGlobals: () => ft
});
module.exports = __toCommonJS(dotnet_runtime_exports);
//! Licensed to the .NET Foundation under one or more agreements.
//! The .NET Foundation licenses this file to you under the MIT license.
var e = "10.0.14", t = "Release", n = false;
const r = [[true, "mono_wasm_register_root", "number", ["number", "number", "string"]], [true, "mono_wasm_deregister_root", null, ["number"]], [true, "mono_wasm_string_get_data_ref", null, ["number", "number", "number", "number"]], [true, "mono_wasm_set_is_debugger_attached", "void", ["bool"]], [true, "mono_wasm_send_dbg_command", "bool", ["number", "number", "number", "number", "number"]], [true, "mono_wasm_send_dbg_command_with_parms", "bool", ["number", "number", "number", "number", "number", "number", "string"]], [true, "mono_wasm_setenv", null, ["string", "string"]], [true, "mono_wasm_parse_runtime_options", null, ["number", "number"]], [true, "mono_wasm_strdup", "number", ["string"]], [true, "mono_background_exec", null, []], [true, "mono_wasm_ds_exec", null, []], [true, "mono_wasm_execute_timer", null, []], [true, "mono_wasm_load_icu_data", "number", ["number"]], [false, "mono_wasm_add_assembly", "number", ["string", "number", "number"]], [true, "mono_wasm_add_satellite_assembly", "void", ["string", "string", "number", "number"]], [false, "mono_wasm_load_runtime", null, ["number", "number", "number", "number"]], [true, "mono_wasm_change_debugger_log_level", "void", ["number"]], [true, "mono_wasm_assembly_load", "number", ["string"]], [true, "mono_wasm_assembly_find_class", "number", ["number", "string", "string"]], [true, "mono_wasm_assembly_find_method", "number", ["number", "string", "number"]], [true, "mono_wasm_string_from_utf16_ref", "void", ["number", "number", "number"]], [true, "mono_wasm_intern_string_ref", "void", ["number"]], [false, "mono_wasm_exit", "void", ["number"]], [true, "mono_wasm_getenv", "number", ["string"]], [true, "mono_wasm_set_main_args", "void", ["number", "number"]], [() => !ct.emscriptenBuildOptions.enableAotProfiler, "mono_wasm_profiler_init_aot", "void", ["string"]], [() => !ct.emscriptenBuildOptions.enableDevToolsProfiler, "mono_wasm_profiler_init_browser_devtools", "void", ["string"]], [() => !ct.emscriptenBuildOptions.enableLogProfiler, "mono_wasm_profiler_init_log", "void", ["string"]], [false, "mono_wasm_exec_regression", "number", ["number", "string"]], [false, "mono_wasm_invoke_jsexport", "void", ["number", "number"]], [true, "mono_wasm_write_managed_pointer_unsafe", "void", ["number", "number"]], [true, "mono_wasm_copy_managed_pointer", "void", ["number", "number"]], [true, "mono_wasm_i52_to_f64", "number", ["number", "number"]], [true, "mono_wasm_u52_to_f64", "number", ["number", "number"]], [true, "mono_wasm_f64_to_i52", "number", ["number", "number"]], [true, "mono_wasm_f64_to_u52", "number", ["number", "number"]], [true, "mono_wasm_method_get_name", "number", ["number"]], [true, "mono_wasm_method_get_name_ex", "number", ["number"]], [true, "mono_wasm_method_get_full_name", "number", ["number"]], [true, "mono_wasm_gc_lock", "void", []], [true, "mono_wasm_gc_unlock", "void", []], [true, "mono_wasm_get_i32_unaligned", "number", ["number"]], [true, "mono_wasm_get_f32_unaligned", "number", ["number"]], [true, "mono_wasm_get_f64_unaligned", "number", ["number"]], [true, "mono_wasm_read_as_bool_or_null_unsafe", "number", ["number"]], [true, "mono_jiterp_trace_bailout", "void", ["number"]], [true, "mono_jiterp_get_trace_bailout_count", "number", ["number"]], [true, "mono_jiterp_value_copy", "void", ["number", "number", "number"]], [true, "mono_jiterp_get_member_offset", "number", ["number"]], [true, "mono_jiterp_encode_leb52", "number", ["number", "number", "number"]], [true, "mono_jiterp_encode_leb64_ref", "number", ["number", "number", "number"]], [true, "mono_jiterp_encode_leb_signed_boundary", "number", ["number", "number", "number"]], [true, "mono_jiterp_write_number_unaligned", "void", ["number", "number", "number"]], [true, "mono_jiterp_type_is_byref", "number", ["number"]], [true, "mono_jiterp_get_size_of_stackval", "number", []], [true, "mono_jiterp_parse_option", "number", ["string"]], [true, "mono_jiterp_get_options_as_json", "number", []], [true, "mono_jiterp_get_option_as_int", "number", ["string"]], [true, "mono_jiterp_get_options_version", "number", []], [true, "mono_jiterp_adjust_abort_count", "number", ["number", "number"]], [true, "mono_jiterp_register_jit_call_thunk", "void", ["number", "number"]], [true, "mono_jiterp_type_get_raw_value_size", "number", ["number"]], [true, "mono_jiterp_get_signature_has_this", "number", ["number"]], [true, "mono_jiterp_get_signature_return_type", "number", ["number"]], [true, "mono_jiterp_get_signature_param_count", "number", ["number"]], [true, "mono_jiterp_get_signature_params", "number", ["number"]], [true, "mono_jiterp_type_to_ldind", "number", ["number"]], [true, "mono_jiterp_type_to_stind", "number", ["number"]], [true, "mono_jiterp_imethod_to_ftnptr", "number", ["number"]], [true, "mono_jiterp_debug_count", "number", []], [true, "mono_jiterp_get_trace_hit_count", "number", ["number"]], [true, "mono_jiterp_get_polling_required_address", "number", []], [true, "mono_jiterp_get_rejected_trace_count", "number", []], [true, "mono_jiterp_boost_back_branch_target", "void", ["number"]], [true, "mono_jiterp_is_imethod_var_address_taken", "number", ["number", "number"]], [true, "mono_jiterp_get_opcode_value_table_entry", "number", ["number"]], [true, "mono_jiterp_get_simd_intrinsic", "number", ["number", "number"]], [true, "mono_jiterp_get_simd_opcode", "number", ["number", "number"]], [true, "mono_jiterp_get_arg_offset", "number", ["number", "number", "number"]], [true, "mono_jiterp_get_opcode_info", "number", ["number", "number"]], [true, "mono_wasm_is_zero_page_reserved", "number", []], [true, "mono_jiterp_is_special_interface", "number", ["number"]], [true, "mono_jiterp_initialize_table", "void", ["number", "number", "number"]], [true, "mono_jiterp_allocate_table_entry", "number", ["number"]], [true, "mono_jiterp_get_interp_entry_func", "number", ["number"]], [true, "mono_jiterp_get_counter", "number", ["number"]], [true, "mono_jiterp_modify_counter", "number", ["number", "number"]], [true, "mono_jiterp_tlqueue_next", "number", ["number"]], [true, "mono_jiterp_tlqueue_add", "number", ["number", "number"]], [true, "mono_jiterp_tlqueue_clear", "void", ["number"]], [true, "mono_jiterp_begin_catch", "void", ["number"]], [true, "mono_jiterp_end_catch", "void", []], [true, "mono_interp_pgo_load_table", "number", ["number", "number"]], [true, "mono_interp_pgo_save_table", "number", ["number", "number"]], [() => !ct.emscriptenBuildOptions.enableEventPipe && !ct.emscriptenBuildOptions.enableDevToolsProfiler, "mono_jiterp_prof_enter", "void", ["number", "number"]], [() => !ct.emscriptenBuildOptions.enableEventPipe && !ct.emscriptenBuildOptions.enableDevToolsProfiler, "mono_jiterp_prof_samplepoint", "void", ["number", "number"]], [() => !ct.emscriptenBuildOptions.enableEventPipe && !ct.emscriptenBuildOptions.enableDevToolsProfiler, "mono_jiterp_prof_leave", "void", ["number", "number"]]], o = {}, s = o, a = o, i = ["void", "number", null];
function c(e2, t2, n2, r2) {
  let o2 = void 0 === r2 && i.indexOf(t2) >= 0 && (!n2 || n2.every(((e3) => i.indexOf(e3) >= 0))) && Ke.wasmExports ? Ke.wasmExports[e2] : void 0;
  if (o2 && n2 && o2.length !== n2.length && (He(`argument count mismatch for cwrap ${e2}`), o2 = void 0), "function" != typeof o2 && (o2 = Ke.cwrap(e2, t2, n2, r2)), "function" != typeof o2) throw new Error(`cwrap ${e2} not found or not a function`);
  return o2;
}
const l = 0, p = 0, u = 0, d = BigInt("9223372036854775807"), f = BigInt("-9223372036854775808");
function _(e2) {
  return Ke._malloc(e2) >>> 0;
}
function m(e2) {
  Ke._free(e2);
}
function h(e2, t2, n2) {
  if (!Number.isSafeInteger(e2)) throw new Error(`Assert failed: Value is not an integer: ${e2} (${typeof e2})`);
  if (!(e2 >= t2 && e2 <= n2)) throw new Error(`Assert failed: Overflow: value ${e2} is out of ${t2} ${n2} range`);
}
function g(e2, t2) {
  K().fill(0, e2, e2 + t2);
}
function b(e2, t2) {
  const n2 = !!t2;
  "number" == typeof t2 && h(t2, 0, 1), Ke.HEAP32[e2 >>> 2] = n2 ? 1 : 0;
}
function y(e2, t2) {
  const n2 = !!t2;
  "number" == typeof t2 && h(t2, 0, 1), Ke.HEAPU8[e2] = n2 ? 1 : 0;
}
function w(e2, t2) {
  h(t2, 0, 255), Ke.HEAPU8[e2] = t2;
}
function k(e2, t2) {
  h(t2, 0, 65535), Ke.HEAPU16[e2 >>> 1] = t2;
}
function S(e2, t2) {
  h(t2, 0, 4294967295), Ke.HEAPU32[e2 >>> 2] = t2;
}
function v(e2, t2) {
  h(t2, -128, 127), Ke.HEAP8[e2] = t2;
}
function U(e2, t2) {
  h(t2, -32768, 32767), Ke.HEAP16[e2 >>> 1] = t2;
}
function T(e2, t2) {
  h(t2, -2147483648, 2147483647), Ke.HEAP32[e2 >>> 2] = t2;
}
function E(e2) {
  if (0 !== e2) switch (e2) {
    case 1:
      throw new Error("value was not an integer");
    case 2:
      throw new Error("value out of range");
    default:
      throw new Error("unknown internal error");
  }
}
function x(e2, t2) {
  if (!Number.isSafeInteger(t2)) throw new Error(`Assert failed: Value is not a safe integer: ${t2} (${typeof t2})`);
  E(o.mono_wasm_f64_to_i52(e2, t2));
}
function I(e2, t2) {
  if (!Number.isSafeInteger(t2)) throw new Error(`Assert failed: Value is not a safe integer: ${t2} (${typeof t2})`);
  if (!(t2 >= 0)) throw new Error("Assert failed: Can't convert negative Number into UInt64");
  E(o.mono_wasm_f64_to_u52(e2, t2));
}
function A(e2, t2) {
  if ("bigint" != typeof t2) throw new Error(`Assert failed: Value is not an bigint: ${t2} (${typeof t2})`);
  if (!(t2 >= f && t2 <= d)) throw new Error(`Assert failed: Overflow: value ${t2} is out of ${f} ${d} range`);
  Ke.HEAP64[e2 >>> 3] = t2;
}
function j(e2, t2) {
  if ("number" != typeof t2) throw new Error(`Assert failed: Value is not a Number: ${t2} (${typeof t2})`);
  Ke.HEAPF32[e2 >>> 2] = t2;
}
function $(e2, t2) {
  if ("number" != typeof t2) throw new Error(`Assert failed: Value is not a Number: ${t2} (${typeof t2})`);
  Ke.HEAPF64[e2 >>> 3] = t2;
}
let L = true;
function R(e2) {
  const t2 = Ke.HEAPU32[e2 >>> 2];
  return t2 > 1 && L && (L = false, We(`getB32: value at ${e2} is not a boolean, but a number: ${t2}`)), !!t2;
}
function B(e2) {
  return !!Ke.HEAPU8[e2];
}
function N(e2) {
  return Ke.HEAPU8[e2];
}
function O(e2) {
  return Ke.HEAPU16[e2 >>> 1];
}
function C(e2) {
  return Ke.HEAPU32[e2 >>> 2];
}
function D(e2, t2) {
  return e2[t2 >>> 2];
}
function F(e2) {
  return o.mono_wasm_get_i32_unaligned(e2);
}
function P(e2) {
  return o.mono_wasm_get_i32_unaligned(e2) >>> 0;
}
function M(e2) {
  return Ke.HEAP8[e2];
}
function z(e2) {
  return Ke.HEAP16[e2 >>> 1];
}
function V(e2) {
  return Ke.HEAP32[e2 >>> 2];
}
function W(e2) {
  const t2 = o.mono_wasm_i52_to_f64(e2, ct._i52_error_scratch_buffer);
  return E(V(ct._i52_error_scratch_buffer)), t2;
}
function H(e2) {
  const t2 = o.mono_wasm_u52_to_f64(e2, ct._i52_error_scratch_buffer);
  return E(V(ct._i52_error_scratch_buffer)), t2;
}
function q(e2) {
  return Ke.HEAP64[e2 >>> 3];
}
function G(e2) {
  return Ke.HEAPF32[e2 >>> 2];
}
function J(e2) {
  return Ke.HEAPF64[e2 >>> 3];
}
function X() {
  return Ke.HEAP8;
}
function Q() {
  return Ke.HEAP16;
}
function Y() {
  return Ke.HEAP32;
}
function Z() {
  return Ke.HEAP64;
}
function K() {
  return Ke.HEAPU8;
}
function ee() {
  return Ke.HEAPU16;
}
function te() {
  return Ke.HEAPU32;
}
function ne() {
  return Ke.HEAPF32;
}
function re() {
  return Ke.HEAPF64;
}
function oe(e2, t2) {
  return e2 >>> t2;
}
let se = false;
function ae() {
  if (se) throw new Error("GC is already locked");
  se = true;
}
function ie() {
  if (!se) throw new Error("GC is not locked");
  se = false;
}
const ce = 8192;
let le = null, pe = null, ue = 0;
const de = [], fe = [];
function _e(e2, t2) {
  if (e2 <= 0) throw new Error("capacity >= 1");
  const n2 = 4 * (e2 |= 0), r2 = _(n2);
  if (r2 % 4 != 0) throw new Error("Malloc returned an unaligned offset");
  return g(r2, n2), new me(r2, e2, true, t2);
}
class me {
  constructor(e2, t2, n2, r2) {
    const s2 = 4 * t2;
    this.__offset = e2, this.__offset32 = e2 >>> 2, this.__count = t2, this.length = t2, this.__handle = o.mono_wasm_register_root(e2, s2, r2 || "noname"), this.__ownsAllocation = n2;
  }
  _throw_index_out_of_range() {
    throw new Error("index out of range");
  }
  _check_in_range(e2) {
    (e2 >= this.__count || e2 < 0) && this._throw_index_out_of_range();
  }
  get_address(e2) {
    return this._check_in_range(e2), this.__offset + 4 * e2;
  }
  get_address_32(e2) {
    return this._check_in_range(e2), this.__offset32 + e2;
  }
  get(e2) {
    this._check_in_range(e2);
    const t2 = this.get_address_32(e2);
    return te()[t2];
  }
  set(e2, t2) {
    const n2 = this.get_address(e2);
    return o.mono_wasm_write_managed_pointer_unsafe(n2, t2), t2;
  }
  copy_value_from_address(e2, t2) {
    const n2 = this.get_address(e2);
    o.mono_wasm_copy_managed_pointer(n2, t2);
  }
  _unsafe_get(e2) {
    return te()[this.__offset32 + e2];
  }
  _unsafe_set(e2, t2) {
    const n2 = this.__offset + e2;
    o.mono_wasm_write_managed_pointer_unsafe(n2, t2);
  }
  clear() {
    this.__offset && g(this.__offset, 4 * this.__count);
  }
  release() {
    this.__offset && this.__ownsAllocation && (o.mono_wasm_deregister_root(this.__offset), g(this.__offset, 4 * this.__count), m(this.__offset)), this.__handle = this.__offset = this.__count = this.__offset32 = 0;
  }
  toString() {
    return `[root buffer @${this.get_address(0)}, size ${this.__count} ]`;
  }
}
class he {
  constructor(e2, t2) {
    this.__buffer = e2, this.__index = t2;
  }
  get_address() {
    return this.__buffer.get_address(this.__index);
  }
  get_address_32() {
    return this.__buffer.get_address_32(this.__index);
  }
  get address() {
    return this.__buffer.get_address(this.__index);
  }
  get() {
    return this.__buffer._unsafe_get(this.__index);
  }
  set(e2) {
    const t2 = this.__buffer.get_address(this.__index);
    return o.mono_wasm_write_managed_pointer_unsafe(t2, e2), e2;
  }
  copy_from(e2) {
    const t2 = e2.address, n2 = this.address;
    o.mono_wasm_copy_managed_pointer(n2, t2);
  }
  copy_to(e2) {
    const t2 = this.address, n2 = e2.address;
    o.mono_wasm_copy_managed_pointer(n2, t2);
  }
  copy_from_address(e2) {
    const t2 = this.address;
    o.mono_wasm_copy_managed_pointer(t2, e2);
  }
  copy_to_address(e2) {
    const t2 = this.address;
    o.mono_wasm_copy_managed_pointer(e2, t2);
  }
  get value() {
    return this.get();
  }
  set value(e2) {
    this.set(e2);
  }
  valueOf() {
    throw new Error("Implicit conversion of roots to pointers is no longer supported. Use .value or .address as appropriate");
  }
  clear() {
    const e2 = this.__buffer.get_address_32(this.__index);
    te()[e2] = 0;
  }
  release() {
    if (!this.__buffer) throw new Error("No buffer");
    var e2;
    de.length > 128 ? (void 0 !== (e2 = this.__index) && (le.set(e2, 0), pe[ue] = e2, ue++), this.__buffer = null, this.__index = 0) : (this.set(0), de.push(this));
  }
  toString() {
    return `[root @${this.address}]`;
  }
}
class ge {
  constructor(e2) {
    this.__external_address = 0, this.__external_address_32 = 0, this._set_address(e2);
  }
  _set_address(e2) {
    this.__external_address = e2, this.__external_address_32 = e2 >>> 2;
  }
  get address() {
    return this.__external_address;
  }
  get_address() {
    return this.__external_address;
  }
  get_address_32() {
    return this.__external_address_32;
  }
  get() {
    return te()[this.__external_address_32];
  }
  set(e2) {
    return o.mono_wasm_write_managed_pointer_unsafe(this.__external_address, e2), e2;
  }
  copy_from(e2) {
    const t2 = e2.address, n2 = this.__external_address;
    o.mono_wasm_copy_managed_pointer(n2, t2);
  }
  copy_to(e2) {
    const t2 = this.__external_address, n2 = e2.address;
    o.mono_wasm_copy_managed_pointer(n2, t2);
  }
  copy_from_address(e2) {
    const t2 = this.__external_address;
    o.mono_wasm_copy_managed_pointer(t2, e2);
  }
  copy_to_address(e2) {
    const t2 = this.__external_address;
    o.mono_wasm_copy_managed_pointer(e2, t2);
  }
  get value() {
    return this.get();
  }
  set value(e2) {
    this.set(e2);
  }
  valueOf() {
    throw new Error("Implicit conversion of roots to pointers is no longer supported. Use .value or .address as appropriate");
  }
  clear() {
    te()[this.__external_address >>> 2] = 0;
  }
  release() {
    fe.length < 128 && fe.push(this);
  }
  toString() {
    return `[external root @${this.address}]`;
  }
}
const be = /* @__PURE__ */ new Map(), ye = "";
let we;
const ke = /* @__PURE__ */ new Map();
let Se, ve, Ue, Te, Ee, xe = 0, Ie = null, Ae = 0;
function je(e2) {
  if (void 0 === Te) {
    const t2 = Ke.lengthBytesUTF8(e2), n2 = new Uint8Array(t2);
    return Ke.stringToUTF8Array(e2, n2, 0, t2), n2;
  }
  return Te.encode(e2);
}
function $e(e2) {
  const t2 = Ke.lengthBytesUTF8(e2) + 1, n2 = _(t2), r2 = K().subarray(n2, n2 + t2);
  return Ke.stringToUTF8Array(e2, r2, 0, t2), r2[t2 - 1] = 0, n2;
}
function Le(e2) {
  const t2 = K();
  return (function(e3, t3, n2) {
    const r2 = t3 + n2;
    let o2 = t3;
    for (; e3[o2] && !(o2 >= r2); ) ++o2;
    if (o2 - t3 <= 16) return Ke.UTF8ArrayToString(e3, t3, n2);
    if (void 0 === Ue) return Ke.UTF8ArrayToString(e3, t3, n2);
    const s2 = Fe(e3, t3, o2);
    return Ue.decode(s2);
  })(t2, e2, t2.length - e2);
}
function Re(e2, t2) {
  if (Se) {
    const n2 = Fe(K(), e2, t2);
    return Se.decode(n2);
  }
  return (function(e3, t3) {
    let n2 = "";
    const r2 = ee();
    for (let o2 = e3; o2 < t3; o2 += 2) {
      const e4 = r2[o2 >>> 1];
      n2 += String.fromCharCode(e4);
    }
    return n2;
  })(e2, t2);
}
function Be(e2, t2, n2) {
  const r2 = ee(), o2 = n2.length;
  for (let c2 = 0; c2 < o2 && (s2 = r2, a2 = e2, h(i2 = n2.charCodeAt(c2), 0, 65535), s2[a2 >>> 1] = i2, !((e2 += 2) >= t2)); c2++) ;
  var s2, a2, i2;
}
function Ne(e2) {
  const t2 = 2 * (e2.length + 1), n2 = _(t2);
  return g(n2, 2 * e2.length), Be(n2, n2 + t2, e2), n2;
}
function Oe(e2) {
  if (e2.value === p) return null;
  const t2 = we + 0, n2 = we + 4, r2 = we + 8;
  let s2;
  o.mono_wasm_string_get_data_ref(e2.address, t2, n2, r2);
  const a2 = te(), i2 = D(a2, n2), c2 = D(a2, t2), l2 = D(a2, r2);
  if (l2 && (s2 = ke.get(e2.value)), void 0 === s2 && (i2 && c2 ? (s2 = Re(c2, c2 + i2), l2 && ke.set(e2.value, s2)) : s2 = ye), void 0 === s2) throw new Error(`internal error when decoding string at location ${e2.value}`);
  return s2;
}
function Ce(e2, t2) {
  let n2;
  if ("symbol" == typeof e2 ? (n2 = e2.description, "string" != typeof n2 && (n2 = Symbol.keyFor(e2)), "string" != typeof n2 && (n2 = "<unknown Symbol>")) : "string" == typeof e2 && (n2 = e2), "string" != typeof n2) throw new Error(`Argument to stringToInternedMonoStringRoot must be a string but was ${e2}`);
  if (0 === n2.length && xe) return void t2.set(xe);
  const r2 = be.get(n2);
  r2 ? t2.set(r2) : (De(n2, t2), (function(e3, t3, n3) {
    if (!t3.value) throw new Error("null pointer passed to _store_string_in_intern_table");
    Ae >= 8192 && (Ie = null), Ie || (Ie = _e(8192, "interned strings"), Ae = 0);
    const r3 = Ie, s2 = Ae++;
    if (o.mono_wasm_intern_string_ref(t3.address), !t3.value) throw new Error("mono_wasm_intern_string_ref produced a null pointer");
    be.set(e3, t3.value), ke.set(t3.value, e3), 0 !== e3.length || xe || (xe = t3.value), r3.copy_value_from_address(s2, t3.address);
  })(n2, t2));
}
function De(e2, t2) {
  const n2 = 2 * (e2.length + 1), r2 = _(n2);
  Be(r2, r2 + n2, e2), o.mono_wasm_string_from_utf16_ref(r2, e2.length, t2.address), m(r2);
}
function Fe(e2, t2, n2) {
  return e2.buffer, e2.subarray(t2, n2);
}
function Pe(e2) {
  if (e2 === p) return null;
  Ee.value = e2;
  const t2 = Oe(Ee);
  return Ee.value = p, t2;
}
let Me = "MONO_WASM: ";
function ze(e2) {
  if (ct.diagnosticTracing) {
    const t2 = "function" == typeof e2 ? e2() : e2;
    console.debug(Me + t2);
  }
}
function Ve(e2, ...t2) {
  console.info(Me + e2, ...t2);
}
function We(e2, ...t2) {
  console.warn(Me + e2, ...t2);
}
function He(e2, ...t2) {
  if (t2 && t2.length > 0 && t2[0] && "object" == typeof t2[0]) {
    if (t2[0].silent) return;
    if (t2[0].toString) return void console.error(Me + e2, t2[0].toString());
  }
  console.error(Me + e2, ...t2);
}
const qe = /* @__PURE__ */ new Map();
let Ge;
const Je = [];
function Xe(e2) {
  try {
    if (Ye(), 0 == qe.size) return e2;
    const t2 = e2;
    for (let n2 = 0; n2 < Je.length; n2++) {
      const r2 = e2.replace(new RegExp(Je[n2], "g"), ((e3, ...t3) => {
        const n3 = t3.find(((e4) => "object" == typeof e4 && void 0 !== e4.replaceSection));
        if (void 0 === n3) return e3;
        const r3 = n3.funcNum, o2 = n3.replaceSection, s2 = qe.get(Number(r3));
        return void 0 === s2 ? e3 : e3.replace(o2, `${s2} (${o2})`);
      }));
      if (r2 !== t2) return r2;
    }
    return t2;
  } catch (t2) {
    return console.debug(`failed to symbolicate: ${t2}`), e2;
  }
}
function Qe(e2) {
  let t2;
  return t2 = "string" == typeof e2 ? e2 : null == e2 || void 0 === e2.stack ? new Error().stack + "" : e2.stack + "", Xe(t2);
}
function Ye() {
  if (!Ge) return;
  Je.push(/at (?<replaceSection>[^:()]+:wasm-function\[(?<funcNum>\d+)\]:0x[a-fA-F\d]+)((?![^)a-fA-F\d])|$)/), Je.push(/(?:WASM \[[\da-zA-Z]+\], (?<replaceSection>function #(?<funcNum>[\d]+) \(''\)))/), Je.push(/(?<replaceSection>[a-z]+:\/\/[^ )]*:wasm-function\[(?<funcNum>\d+)\]:0x[a-fA-F\d]+)/), Je.push(/(?<replaceSection><[^ >]+>[.:]wasm-function\[(?<funcNum>[0-9]+)\])/);
  const e2 = Ge;
  Ge = void 0;
  try {
    e2.split(/[\r\n]/).forEach(((e3) => {
      const t2 = e3.split(/:/);
      t2.length < 2 || (t2[1] = t2.splice(1).join(":"), qe.set(Number(t2[0]), t2[1]));
    })), lt.diagnosticTracing && ze(`Loaded ${qe.size} symbols`);
  } catch (e3) {
    We(`Failed to load symbol map: ${e3}`);
  }
}
function Ze() {
  return Ye(), [...qe.values()];
}
let Ke, et;
const tt = "object" == typeof process && "object" == typeof process.versions && "string" == typeof process.versions.node, nt = "function" == typeof importScripts, rt = nt && "undefined" != typeof dotnetSidecar, ot = nt && !rt, st = "object" == typeof window || nt && !tt, at = !st && !tt;
let it = null, ct = null, lt = null, pt = null, ut = false;
function dt(e2, t2) {
  ct.emscriptenBuildOptions = t2, e2.isPThread, ct.quit = e2.quit_, ct.ExitStatus = e2.ExitStatus, ct.getMemory = e2.getMemory, ct.getWasmIndirectFunctionTable = e2.getWasmIndirectFunctionTable, ct.updateMemoryViews = e2.updateMemoryViews;
}
function ft(e2) {
  if (ut) throw new Error("Runtime module already loaded");
  ut = true, Ke = e2.module, et = e2.internal, ct = e2.runtimeHelpers, lt = e2.loaderHelpers, pt = e2.diagnosticHelpers, it = e2.api;
  const t2 = { gitHash: "0427b9abbead3fa64c6655eb4290c537397131fa", coreAssetsInMemory: _t(), allAssetsInMemory: _t(), dotnetReady: _t(), afterInstantiateWasm: _t(), beforePreInit: _t(), afterPreInit: _t(), afterPreRun: _t(), beforeOnRuntimeInitialized: _t(), afterMonoStarted: _t(), afterDeputyReady: _t(), afterIOStarted: _t(), afterOnRuntimeInitialized: _t(), afterPostRun: _t(), nativeAbort: (e3) => {
    throw e3 || new Error("abort");
  }, nativeExit: (e3) => {
    throw new Error("exit:" + e3);
  } };
  Object.assign(ct, t2), Object.assign(e2.module.config, {}), Object.assign(e2.api, { Module: e2.module, ...e2.module }), Object.assign(e2.api, { INTERNAL: e2.internal });
}
function _t(e2, t2) {
  return lt.createPromiseController(e2, t2);
}
function mt(e2, t2) {
  if (e2) return;
  const n2 = "Assert failed: " + ("function" == typeof t2 ? t2() : t2), r2 = new Error(n2);
  He(n2, r2), ct.nativeAbort(r2);
}
function ht(e2, t2, n2) {
  const r2 = (function(e3, t3, n3) {
    let r3, o3 = 0;
    r3 = e3.length - o3;
    const s3 = { read: function() {
      if (o3 >= r3) return null;
      const t4 = e3[o3];
      return o3 += 1, t4;
    } };
    return Object.defineProperty(s3, "eof", { get: function() {
      return o3 >= r3;
    }, configurable: true, enumerable: true }), s3;
  })(e2);
  let o2 = "", s2 = 0, a2 = 0, i2 = 0, c2 = 0, l2 = 0, p2 = 0;
  for (; s2 = r2.read(), a2 = r2.read(), i2 = r2.read(), null !== s2; ) null === a2 && (a2 = 0, l2 += 1), null === i2 && (i2 = 0, l2 += 1), p2 = s2 << 16 | a2 << 8 | i2, c2 = (16777215 & p2) >> 18, o2 += gt[c2], c2 = (262143 & p2) >> 12, o2 += gt[c2], l2 < 2 && (c2 = (4095 & p2) >> 6, o2 += gt[c2]), 2 === l2 ? o2 += "==" : 1 === l2 ? o2 += "=" : (c2 = 63 & p2, o2 += gt[c2]);
  return o2;
}
const gt = ["A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "+", "/"], bt = /* @__PURE__ */ new Map();
bt.remove = function(e2) {
  const t2 = this.get(e2);
  return this.delete(e2), t2;
};
let yt, wt, kt, St = {}, vt = 0, Ut = -1;
function mono_wasm_fire_debugger_agent_message_with_data_to_pause(e2) {
  console.assert(true, `mono_wasm_fire_debugger_agent_message_with_data ${e2}`);
  debugger;
}
function Tt(e2) {
  e2.length > Ut && (yt && m(yt), Ut = Math.max(e2.length, Ut, 256), yt = _(Ut));
  const t2 = atob(e2), n2 = K();
  for (let e3 = 0; e3 < t2.length; e3++) n2[yt + e3] = t2.charCodeAt(e3);
}
function Et(e2, t2, n2, r2, s2, a2, i2) {
  Tt(r2), o.mono_wasm_send_dbg_command_with_parms(e2, t2, n2, yt, s2, a2, i2.toString());
  const { res_ok: c2, res: l2 } = bt.remove(e2);
  if (!c2) throw new Error("Failed on mono_wasm_send_dbg_command_with_parms");
  return l2;
}
function xt(e2, t2, n2, r2) {
  Tt(r2), o.mono_wasm_send_dbg_command(e2, t2, n2, yt, r2.length);
  const { res_ok: s2, res: a2 } = bt.remove(e2);
  if (!s2) throw new Error("Failed on mono_wasm_send_dbg_command");
  return a2;
}
function It() {
  const { res_ok: e2, res: t2 } = bt.remove(0);
  if (!e2) throw new Error("Failed on mono_wasm_get_dbg_command_info");
  return t2;
}
function At() {
}
function jt() {
  o.mono_wasm_set_is_debugger_attached(false);
}
function $t(e2) {
  o.mono_wasm_change_debugger_log_level(e2);
}
function Lt(e2, t2 = {}) {
  if ("object" != typeof e2) throw new Error(`event must be an object, but got ${JSON.stringify(e2)}`);
  if (void 0 === e2.eventName) throw new Error(`event.eventName is a required parameter, in event: ${JSON.stringify(e2)}`);
  if ("object" != typeof t2) throw new Error(`args must be an object, but got ${JSON.stringify(t2)}`);
  console.debug("mono_wasm_debug_event_raised:aef14bca-5519-4dfe-b35a-f867abc123ae", JSON.stringify(e2), JSON.stringify(t2));
}
function Rt() {
  -1 == ct.waitForDebugger && (ct.waitForDebugger = 1), o.mono_wasm_set_is_debugger_attached(true);
}
function Bt(e2) {
  if (null != e2.arguments && !Array.isArray(e2.arguments)) throw new Error(`"arguments" should be an array, but was ${e2.arguments}`);
  const t2 = e2.objectId, n2 = e2.details;
  let r2 = {};
  if (t2.startsWith("dotnet:cfo_res:")) {
    if (!(t2 in St)) throw new Error(`Unknown object id ${t2}`);
    r2 = St[t2];
  } else r2 = (function(e3, t3) {
    if (e3.startsWith("dotnet:array:")) {
      let e4;
      if (void 0 === t3.items) return e4 = t3.map(((e5) => e5.value)), e4;
      if (void 0 === t3.dimensionsDetails || 1 === t3.dimensionsDetails.length) return e4 = t3.items.map(((e5) => e5.value)), e4;
    }
    const n3 = {};
    return Object.keys(t3).forEach(((e4) => {
      const r3 = t3[e4];
      void 0 !== r3.get ? Object.defineProperty(n3, r3.name, { get: () => xt(r3.get.id, r3.get.commandSet, r3.get.command, r3.get.buffer), set: function(e5) {
        return Et(r3.set.id, r3.set.commandSet, r3.set.command, r3.set.buffer, r3.set.length, r3.set.valtype, e5), true;
      } }) : void 0 !== r3.set ? Object.defineProperty(n3, r3.name, { get: () => r3.value, set: function(e5) {
        return Et(r3.set.id, r3.set.commandSet, r3.set.command, r3.set.buffer, r3.set.length, r3.set.valtype, e5), true;
      } }) : n3[r3.name] = r3.value;
    })), n3;
  })(t2, n2);
  const o2 = null != e2.arguments ? e2.arguments.map(((e3) => JSON.stringify(e3.value))) : [], s2 = `const fn = ${e2.functionDeclaration}; return fn.apply(proxy, [${o2}]);`, a2 = new Function("proxy", s2)(r2);
  if (void 0 === a2) return { type: "undefined" };
  if (Object(a2) !== a2) return "object" == typeof a2 && null == a2 ? { type: typeof a2, subtype: `${a2}`, value: null } : { type: typeof a2, description: `${a2}`, value: `${a2}` };
  if (e2.returnByValue && null == a2.subtype) return { type: "object", value: a2 };
  if (Object.getPrototypeOf(a2) == Array.prototype) {
    const e3 = Ot(a2);
    return { type: "object", subtype: "array", className: "Array", description: `Array(${a2.length})`, objectId: e3 };
  }
  return void 0 !== a2.value || void 0 !== a2.subtype ? a2 : a2 == r2 ? { type: "object", className: "Object", description: "Object", objectId: t2 } : { type: "object", className: "Object", description: "Object", objectId: Ot(a2) };
}
function Nt(e2, t2 = {}) {
  return (function(e3, t3) {
    if (!(e3 in St)) throw new Error(`Could not find any object with id ${e3}`);
    const n2 = St[e3], r2 = Object.getOwnPropertyDescriptors(n2);
    t3.accessorPropertiesOnly && Object.keys(r2).forEach(((e4) => {
      void 0 === r2[e4].get && Reflect.deleteProperty(r2, e4);
    }));
    const o2 = [];
    return Object.keys(r2).forEach(((e4) => {
      let t4;
      const n3 = r2[e4];
      t4 = "object" == typeof n3.value ? Object.assign({ name: e4 }, n3) : void 0 !== n3.value ? { name: e4, value: Object.assign({ type: typeof n3.value, description: "" + n3.value }, n3) } : void 0 !== n3.get ? { name: e4, get: { className: "Function", description: `get ${e4} () {}`, type: "function" } } : { name: e4, value: { type: "symbol", value: "<Unknown>", description: "<Unknown>" } }, o2.push(t4);
    })), { __value_as_json_string__: JSON.stringify(o2) };
  })(`dotnet:cfo_res:${e2}`, t2);
}
function Ot(e2) {
  const t2 = "dotnet:cfo_res:" + vt++;
  return St[t2] = e2, t2;
}
function Ct(e2) {
  e2 in St && delete St[e2];
}
let Dt = false;
function Ft() {
  if (Dt) return globalThis.performance.now();
}
function Pt(e2, t2, n2) {
  if (Dt && e2) {
    const r2 = st ? { start: e2 } : { startTime: e2 }, o2 = n2 ? `${t2}${n2} ` : t2;
    globalThis.performance.measure(o2, r2);
  }
}
const Mt = /* @__PURE__ */ new Map();
function zt(e2, t2, n2) {
  if (0 === t2 || 1 === t2 || 2 === t2 || 26 === t2) return;
  let r2, o2, s2, a2;
  o2 = to(Cn(e2)), s2 = to(Dn(e2)), a2 = to(Fn(e2));
  const i2 = On(e2);
  r2 = Vt(i2), 19 === t2 && (t2 = i2);
  const c2 = Vt(t2), l2 = Cn(e2), p2 = n2 * In;
  return (e3) => c2(e3 + p2, l2, r2, o2, s2, a2);
}
function Vt(e2) {
  if (0 === e2 || 1 === e2) return;
  const t2 = vn.get(e2);
  return t2 && "function" == typeof t2 || mt(false, `ERR41: Unknown converter for type ${e2}. ${Kr}`), t2;
}
function Wt(e2) {
  return 0 == zn(e2) ? null : (function(e3) {
    return e3 || mt(false, "Null arg"), B(e3);
  })(e2);
}
function Ht(e2) {
  return 0 == zn(e2) ? null : (function(e3) {
    return e3 || mt(false, "Null arg"), N(e3);
  })(e2);
}
function qt(e2) {
  return 0 == zn(e2) ? null : (function(e3) {
    return e3 || mt(false, "Null arg"), O(e3);
  })(e2);
}
function Gt(e2) {
  return 0 == zn(e2) ? null : (function(e3) {
    return e3 || mt(false, "Null arg"), z(e3);
  })(e2);
}
function Jt(e2) {
  return 0 == zn(e2) ? null : (function(e3) {
    return e3 || mt(false, "Null arg"), V(e3);
  })(e2);
}
function Xt(e2) {
  return 0 == zn(e2) ? null : (function(e3) {
    return e3 || mt(false, "Null arg"), J(e3);
  })(e2);
}
function Qt(e2) {
  return 0 == zn(e2) ? null : (function(e3) {
    return e3 || mt(false, "Null arg"), q(e3);
  })(e2);
}
function Yt(e2) {
  return 0 == zn(e2) ? null : (function(e3) {
    return e3 || mt(false, "Null arg"), G(e3);
  })(e2);
}
function Zt(e2) {
  return 0 == zn(e2) ? null : (function(e3) {
    return e3 || mt(false, "Null arg"), J(e3);
  })(e2);
}
function Kt(e2) {
  return 0 == zn(e2) ? null : Hn(e2);
}
function en() {
  return null;
}
function tn(e2) {
  return 0 === zn(e2) ? null : (function(e3) {
    e3 || mt(false, "Null arg");
    const t2 = J(e3);
    return new Date(t2);
  })(e2);
}
function nn(e2, t2, n2, r2, o2, s2) {
  if (0 === zn(e2)) return null;
  const a2 = Zn(e2);
  let i2 = qr(a2);
  return null == i2 && (i2 = (e3, t3, i3) => (function(e4, t4, n3, r3, o3, s3, a3, i4) {
    lt.assert_runtime_running();
    const c2 = Ke.stackSave();
    try {
      const c3 = $n(6), l2 = Ln(c3, 2);
      if (Wn(l2, 14), Kn(l2, e4), s3 && s3(Ln(c3, 3), t4), a3 && a3(Ln(c3, 4), n3), i4 && i4(Ln(c3, 5), r3), kn(yn.CallDelegate, c3), o3) return o3(Ln(c3, 1));
    } finally {
      lt.is_runtime_running() && Ke.stackRestore(c2);
    }
  })(a2, e3, t3, i3, n2, r2, o2, s2), i2.dispose = () => {
    i2.isDisposed || (i2.isDisposed = true, Vr(i2, a2));
  }, i2.isDisposed = false, zr(i2, a2)), i2;
}
class rn {
  constructor(e2, t2) {
    this.promise = e2, this.resolve_or_reject = t2;
  }
}
function on(e2, t2, n2) {
  const r2 = zn(e2);
  30 == r2 && mt(false, "Unexpected Task type: TaskPreCreated");
  const o2 = cn(e2, r2, n2);
  if (false !== o2) return o2;
  const s2 = Qn(e2), a2 = ln(n2);
  return (function(e3, t3) {
    hr(), xr[0 - t3] = e3, Object.isExtensible(e3) && (e3[Cr] = t3);
  })(a2, s2), a2.promise;
}
function sn(e2, t2, n2) {
  const r2 = ln(n2);
  return Yn(e2, Pr(r2)), Wn(e2, 30), r2.promise;
}
function an(e2, t2, n2) {
  const r2 = Ln(e2, 1), o2 = zn(r2);
  if (30 === o2) return n2;
  Mr(Pr(n2));
  const s2 = cn(r2, o2, t2);
  return false === s2 && mt(false, `Expected synchronous result, got: ${o2}`), s2;
}
function cn(e2, t2, n2) {
  if (0 === t2) return null;
  if (29 === t2) return Promise.reject(un(e2));
  if (28 === t2) {
    const t3 = Vn(e2);
    if (1 === t3) return Promise.resolve();
    Wn(e2, t3), n2 || (n2 = vn.get(t3)), n2 || mt(false, `Unknown sub_converter for type ${t3}. ${Kr}`);
    const r2 = n2(e2);
    return Promise.resolve(r2);
  }
  return false;
}
function ln(e2) {
  const { promise: t2, promise_control: n2 } = lt.createPromiseController();
  return new rn(t2, ((t3, r2, o2) => {
    if (29 === t3) {
      const e3 = un(o2);
      n2.reject(e3);
    } else if (28 === t3) {
      const t4 = zn(o2);
      if (1 === t4) n2.resolve(void 0);
      else {
        e2 || (e2 = vn.get(t4)), e2 || mt(false, `Unknown sub_converter for type ${t4}. ${Kr}`);
        const r3 = e2(o2);
        n2.resolve(r3);
      }
    } else mt(false, `Unexpected type ${t3}`);
    Mr(r2);
  }));
}
function pn(e2) {
  if (0 == zn(e2)) return null;
  {
    const t2 = er(e2);
    try {
      return Oe(t2);
    } finally {
      t2.release();
    }
  }
}
function un(e2) {
  const t2 = zn(e2);
  if (0 == t2) return null;
  if (27 == t2) return Fr(Qn(e2));
  const n2 = Zn(e2);
  let r2 = qr(n2);
  if (null == r2) {
    const t3 = pn(e2);
    r2 = new ManagedError(t3), zr(r2, n2);
  }
  return r2;
}
function dn(e2) {
  if (0 == zn(e2)) return null;
  const t2 = Qn(e2), n2 = Fr(t2);
  return void 0 === n2 && mt(false, `JS object JSHandle ${t2} was not found`), n2;
}
function fn(e2) {
  const t2 = zn(e2);
  if (0 == t2) return null;
  if (13 == t2) return Fr(Qn(e2));
  if (21 == t2) return mn(e2, Vn(e2));
  if (14 == t2) {
    const t3 = Zn(e2);
    if (t3 === u) return null;
    let n3 = qr(t3);
    return n3 || (n3 = new ManagedObject(), zr(n3, t3)), n3;
  }
  const n2 = vn.get(t2);
  return n2 || mt(false, `Unknown converter for type ${t2}. ${Kr}`), n2(e2);
}
function _n(e2, t2) {
  return t2 || mt(false, "Expected valid element_type parameter"), mn(e2, t2);
}
function mn(e2, t2) {
  if (0 == zn(e2)) return null;
  -1 == rr(t2) && mt(false, `Element type ${t2} not supported`);
  const n2 = Hn(e2), r2 = tr(e2);
  let s2 = null;
  if (15 == t2) {
    s2 = new Array(r2);
    for (let e3 = 0; e3 < r2; e3++) {
      const t3 = Ln(n2, e3);
      s2[e3] = pn(t3);
    }
    o.mono_wasm_deregister_root(n2);
  } else if (14 == t2) {
    s2 = new Array(r2);
    for (let e3 = 0; e3 < r2; e3++) {
      const t3 = Ln(n2, e3);
      s2[e3] = fn(t3);
    }
    o.mono_wasm_deregister_root(n2);
  } else if (13 == t2) {
    s2 = new Array(r2);
    for (let e3 = 0; e3 < r2; e3++) {
      const t3 = Ln(n2, e3);
      s2[e3] = dn(t3);
    }
  } else if (4 == t2) {
    const e3 = oe(n2, 0);
    s2 = K().subarray(e3, e3 + r2).slice();
  } else if (7 == t2) {
    const e3 = oe(n2, 2);
    s2 = Y().subarray(e3, e3 + r2).slice();
  } else {
    if (10 != t2) throw new Error(`NotImplementedException ${t2}. ${Kr}`);
    {
      const e3 = oe(n2, 3);
      s2 = re().subarray(e3, e3 + r2).slice();
    }
  }
  return m(n2), s2;
}
function hn(e2, t2) {
  t2 || mt(false, "Expected valid element_type parameter");
  const n2 = Hn(e2), r2 = tr(e2);
  let o2 = null;
  if (4 == t2) o2 = new Span(n2, r2, 0);
  else if (7 == t2) o2 = new Span(n2, r2, 1);
  else {
    if (10 != t2) throw new Error(`NotImplementedException ${t2}. ${Kr}`);
    o2 = new Span(n2, r2, 2);
  }
  return o2;
}
function gn(e2, t2) {
  t2 || mt(false, "Expected valid element_type parameter");
  const n2 = Hn(e2), r2 = tr(e2);
  let o2 = null;
  if (4 == t2) o2 = new ArraySegment(n2, r2, 0);
  else if (7 == t2) o2 = new ArraySegment(n2, r2, 1);
  else {
    if (10 != t2) throw new Error(`NotImplementedException ${t2}. ${Kr}`);
    o2 = new ArraySegment(n2, r2, 2);
  }
  return zr(o2, Zn(e2)), o2;
}
const bn = { pthreadId: 0, reuseCount: 0, updateCount: 0, threadPrefix: "          -    ", threadName: "emscripten-loaded" }, yn = {};
function wn(e2, t2, n2, r2) {
  if (hr(), o.mono_wasm_invoke_jsexport(t2, n2), Rn(n2)) throw un(Ln(n2, 0));
}
function kn(e2, t2) {
  if (hr(), o.mono_wasm_invoke_jsexport(e2, t2), Rn(t2)) throw un(Ln(t2, 0));
}
function Sn(e2) {
  const t2 = o.mono_wasm_assembly_find_method(ct.runtime_interop_exports_class, e2, -1);
  if (!t2) throw "Can't find method " + ct.runtime_interop_namespace + "." + ct.runtime_interop_exports_classname + "." + e2;
  return t2;
}
const vn = /* @__PURE__ */ new Map(), Un = /* @__PURE__ */ new Map(), Tn = /* @__PURE__ */ Symbol.for("wasm bound_cs_function"), En = /* @__PURE__ */ Symbol.for("wasm bound_js_function"), xn = /* @__PURE__ */ Symbol.for("wasm imported_js_function"), In = 32, An = 32, jn = 32;
function $n(e2) {
  const t2 = In * e2, n2 = Ke.stackAlloc(t2);
  return g(n2, t2), n2;
}
function Ln(e2, t2) {
  return e2 || mt(false, "Null args"), e2 + t2 * In;
}
function Rn(e2) {
  return e2 || mt(false, "Null args"), 0 !== zn(e2);
}
function Bn(e2, t2) {
  return e2 || mt(false, "Null signatures"), e2 + t2 * An + jn;
}
function Nn(e2) {
  return e2 || mt(false, "Null sig"), N(e2 + 0);
}
function On(e2) {
  return e2 || mt(false, "Null sig"), N(e2 + 16);
}
function Cn(e2) {
  return e2 || mt(false, "Null sig"), N(e2 + 20);
}
function Dn(e2) {
  return e2 || mt(false, "Null sig"), N(e2 + 24);
}
function Fn(e2) {
  return e2 || mt(false, "Null sig"), N(e2 + 28);
}
function Pn(e2) {
  return e2 || mt(false, "Null signatures"), V(e2 + 4);
}
function Mn(e2) {
  return e2 || mt(false, "Null signatures"), V(e2 + 0);
}
function zn(e2) {
  return e2 || mt(false, "Null arg"), N(e2 + 12);
}
function Vn(e2) {
  return e2 || mt(false, "Null arg"), N(e2 + 13);
}
function Wn(e2, t2) {
  e2 || mt(false, "Null arg"), w(e2 + 12, t2);
}
function Hn(e2) {
  return e2 || mt(false, "Null arg"), C(e2);
}
function qn(e2, t2) {
  if (e2 || mt(false, "Null arg"), "boolean" != typeof t2) throw new Error(`Assert failed: Value is not a Boolean: ${t2} (${typeof t2})`);
  y(e2, t2);
}
function Gn(e2, t2) {
  e2 || mt(false, "Null arg"), S(e2, t2);
}
function Jn(e2, t2) {
  e2 || mt(false, "Null arg"), $(e2, t2.getTime());
}
function Xn(e2, t2) {
  e2 || mt(false, "Null arg"), $(e2, t2);
}
function Qn(e2) {
  return e2 || mt(false, "Null arg"), V(e2 + 4);
}
function Yn(e2, t2) {
  e2 || mt(false, "Null arg"), T(e2 + 4, t2);
}
function Zn(e2) {
  return e2 || mt(false, "Null arg"), V(e2 + 4);
}
function Kn(e2, t2) {
  e2 || mt(false, "Null arg"), T(e2 + 4, t2);
}
function er(e2) {
  return e2 || mt(false, "Null arg"), (function(e3) {
    let t2;
    if (!e3) throw new Error("address must be a location in the native heap");
    return fe.length > 0 ? (t2 = fe.pop(), t2._set_address(e3)) : t2 = new ge(e3), t2;
  })(e2);
}
function tr(e2) {
  return e2 || mt(false, "Null arg"), V(e2 + 8);
}
function nr(e2, t2) {
  e2 || mt(false, "Null arg"), T(e2 + 8, t2);
}
class ManagedObject {
  dispose() {
    Vr(this, u);
  }
  get isDisposed() {
    return this[Or] === u;
  }
  toString() {
    return `CsObject(gc_handle: ${this[Or]})`;
  }
}
class ManagedError extends Error {
  constructor(e2) {
    super(e2), this.superStack = Object.getOwnPropertyDescriptor(this, "stack"), Object.defineProperty(this, "stack", { get: this.getManageStack });
  }
  getSuperStack() {
    if (this.superStack) {
      if (void 0 !== this.superStack.value) return this.superStack.value;
      if (void 0 !== this.superStack.get) return this.superStack.get.call(this);
    }
    return super.stack;
  }
  getManageStack() {
    if (this.managed_stack) return this.managed_stack;
    if (!lt.is_runtime_running()) return this.managed_stack = "... omitted managed stack trace.\n" + this.getSuperStack(), this.managed_stack;
    {
      const e2 = this[Or];
      if (e2 !== u) {
        const t2 = (function(e3) {
          lt.assert_runtime_running();
          const t3 = Ke.stackSave();
          try {
            const t4 = $n(3), n2 = Ln(t4, 2);
            return Wn(n2, 16), Kn(n2, e3), kn(yn.GetManagedStackTrace, t4), pn(Ln(t4, 1));
          } finally {
            lt.is_runtime_running() && Ke.stackRestore(t3);
          }
        })(e2);
        if (t2) return this.managed_stack = t2 + "\n" + this.getSuperStack(), this.managed_stack;
      }
    }
    return this.getSuperStack();
  }
  dispose() {
    Vr(this, u);
  }
  get isDisposed() {
    return this[Or] === u;
  }
}
function rr(e2) {
  return 4 == e2 ? 1 : 7 == e2 ? 4 : 8 == e2 || 10 == e2 ? 8 : 15 == e2 || 14 == e2 || 13 == e2 ? In : -1;
}
class or {
  constructor(e2, t2, n2) {
    this._pointer = e2, this._length = t2, this._viewType = n2;
  }
  _unsafe_create_view() {
    const e2 = 0 == this._viewType ? new Uint8Array(K().buffer, this._pointer, this._length) : 1 == this._viewType ? new Int32Array(Y().buffer, this._pointer, this._length) : 2 == this._viewType ? new Float64Array(re().buffer, this._pointer, this._length) : null;
    if (!e2) throw new Error("NotImplementedException");
    return e2;
  }
  set(e2, t2) {
    if (this.isDisposed) throw new Error("Assert failed: ObjectDisposedException");
    const n2 = this._unsafe_create_view();
    if (!e2 || !n2 || e2.constructor !== n2.constructor) throw new Error(`Assert failed: Expected ${n2.constructor}`);
    n2.set(e2, t2);
  }
  copyTo(e2, t2) {
    if (this.isDisposed) throw new Error("Assert failed: ObjectDisposedException");
    const n2 = this._unsafe_create_view();
    if (!e2 || !n2 || e2.constructor !== n2.constructor) throw new Error(`Assert failed: Expected ${n2.constructor}`);
    const r2 = n2.subarray(t2);
    e2.set(r2);
  }
  slice(e2, t2) {
    if (this.isDisposed) throw new Error("Assert failed: ObjectDisposedException");
    return this._unsafe_create_view().slice(e2, t2);
  }
  get length() {
    if (this.isDisposed) throw new Error("Assert failed: ObjectDisposedException");
    return this._length;
  }
  get byteLength() {
    if (this.isDisposed) throw new Error("Assert failed: ObjectDisposedException");
    return 0 == this._viewType ? this._length : 1 == this._viewType ? this._length << 2 : 2 == this._viewType ? this._length << 3 : 0;
  }
}
class Span extends or {
  constructor(e2, t2, n2) {
    super(e2, t2, n2), this.is_disposed = false;
  }
  dispose() {
    this.is_disposed = true;
  }
  get isDisposed() {
    return this.is_disposed;
  }
}
class ArraySegment extends or {
  constructor(e2, t2, n2) {
    super(e2, t2, n2);
  }
  dispose() {
    Vr(this, u);
  }
  get isDisposed() {
    return this[Or] === u;
  }
}
const sr = [null];
function ar(e2) {
  const t2 = e2.args_count, r2 = e2.arg_marshalers, o2 = e2.res_converter, s2 = e2.arg_cleanup, a2 = e2.has_cleanup, i2 = e2.fn, c2 = e2.fqn;
  return e2 = null, function(l2) {
    const p2 = Ft();
    try {
      n && e2.isDisposed;
      const c3 = new Array(t2);
      for (let e3 = 0; e3 < t2; e3++) {
        const t3 = (0, r2[e3])(l2);
        c3[e3] = t3;
      }
      const p3 = i2(...c3);
      if (o2 && o2(l2, p3), a2) for (let e3 = 0; e3 < t2; e3++) {
        const t3 = s2[e3];
        t3 && t3(c3[e3]);
      }
    } catch (e3) {
      wo(l2, e3);
    } finally {
      Pt(p2, "mono.callCsFunction:", c2);
    }
  };
}
function ir(e2, t2) {
  _r.set(e2, t2), lt.diagnosticTracing && ze(`added module imports '${e2}'`);
}
function cr(e2, t2, n2) {
  if (!e2) throw new Error("Assert failed: Null reference");
  e2[t2] = n2;
}
function lr(e2, t2) {
  if (!e2) throw new Error("Assert failed: Null reference");
  return e2[t2];
}
function pr(e2, t2) {
  if (!e2) throw new Error("Assert failed: Null reference");
  return t2 in e2;
}
function ur(e2, t2) {
  if (!e2) throw new Error("Assert failed: Null reference");
  return typeof e2[t2];
}
function dr() {
  return globalThis;
}
const fr = /* @__PURE__ */ new Map(), _r = /* @__PURE__ */ new Map();
function mr(e2, t2) {
  hr(), e2 && "string" == typeof e2 || mt(false, "module_name must be string"), t2 && "string" == typeof t2 || mt(false, "module_url must be string");
  let n2 = fr.get(e2);
  const r2 = !n2;
  return r2 && (lt.diagnosticTracing && ze(`importing ES6 module '${e2}' from '${t2}'`), n2 = import(
    /*! webpackIgnore: true */
    t2
  ), fr.set(e2, n2)), Qr((async () => {
    const o2 = await n2;
    return r2 && (_r.set(e2, o2), lt.diagnosticTracing && ze(`imported ES6 module '${e2}' from '${t2}'`)), o2;
  }));
}
function hr() {
  lt.assert_runtime_running(), ct.mono_wasm_bindings_is_ready || mt(false, "The runtime must be initialized.");
}
function gr(e2) {
  e2();
}
const br = "function" == typeof globalThis.WeakRef;
function yr(e2) {
  return br ? new WeakRef(e2) : /* @__PURE__ */ (function(e3) {
    return { deref: () => e3, dispose: () => {
      e3 = null;
    } };
  })(e2);
}
function wr(e2, t2, n2, r2, o2, s2, a2) {
  const i2 = `[${t2}] ${n2}.${r2}:${o2}`, c2 = Ft();
  lt.diagnosticTracing && ze(`Binding [JSExport] ${n2}.${r2}:${o2} from ${t2} assembly`);
  const l2 = Mn(a2);
  2 !== l2 && mt(false, `Signature version ${l2} mismatch.`);
  const p2 = Pn(a2), u2 = new Array(p2);
  for (let e3 = 0; e3 < p2; e3++) {
    const t3 = Bn(a2, e3 + 2), n3 = eo(t3, Nn(t3), e3 + 2);
    n3 || mt(false, "ERR43: argument marshaler must be resolved"), u2[e3] = n3;
  }
  const d2 = Bn(a2, 1);
  let f2 = Nn(d2);
  const _2 = 20 == f2, m2 = 26 == f2;
  _2 && (f2 = 30);
  const h2 = zt(d2, f2, 1), g2 = { method: e2, fullyQualifiedName: i2, args_count: p2, arg_marshalers: u2, res_converter: h2, is_async: _2, is_discard_no_wait: m2, isDisposed: false };
  let b2;
  b2 = _2 ? 1 == p2 && h2 ? (function(e3) {
    const t3 = e3.method, n3 = e3.arg_marshalers[0], r3 = e3.res_converter, o3 = e3.fullyQualifiedName;
    return e3 = null, function(e4) {
      const s3 = Ft();
      lt.assert_runtime_running();
      const a3 = Ke.stackSave();
      try {
        const o4 = $n(3);
        n3(o4, e4);
        let s4 = r3(o4);
        return wn(ct.managedThreadTID, t3, o4), s4 = an(o4, void 0, s4), s4;
      } finally {
        lt.is_runtime_running() && Ke.stackRestore(a3), Pt(s3, "mono.callCsFunction:", o3);
      }
    };
  })(g2) : 2 == p2 && h2 ? (function(e3) {
    const t3 = e3.method, n3 = e3.arg_marshalers[0], r3 = e3.arg_marshalers[1], o3 = e3.res_converter, s3 = e3.fullyQualifiedName;
    return e3 = null, function(e4, a3) {
      const i3 = Ft();
      lt.assert_runtime_running();
      const c3 = Ke.stackSave();
      try {
        const s4 = $n(4);
        n3(s4, e4), r3(s4, a3);
        let i4 = o3(s4);
        return wn(ct.managedThreadTID, t3, s4), i4 = an(s4, void 0, i4), i4;
      } finally {
        lt.is_runtime_running() && Ke.stackRestore(c3), Pt(i3, "mono.callCsFunction:", s3);
      }
    };
  })(g2) : kr(g2) : m2 ? kr(g2) : 0 != p2 || h2 ? 1 != p2 || h2 ? 1 == p2 && h2 ? (function(e3) {
    const t3 = e3.method, n3 = e3.arg_marshalers[0], r3 = e3.res_converter, o3 = e3.fullyQualifiedName;
    return e3 = null, function(e4) {
      const s3 = Ft();
      lt.assert_runtime_running();
      const a3 = Ke.stackSave();
      try {
        const o4 = $n(3);
        return n3(o4, e4), kn(t3, o4), r3(o4);
      } finally {
        lt.is_runtime_running() && Ke.stackRestore(a3), Pt(s3, "mono.callCsFunction:", o3);
      }
    };
  })(g2) : 2 == p2 && h2 ? (function(e3) {
    const t3 = e3.method, n3 = e3.arg_marshalers[0], r3 = e3.arg_marshalers[1], o3 = e3.res_converter, s3 = e3.fullyQualifiedName;
    return e3 = null, function(e4, a3) {
      const i3 = Ft();
      lt.assert_runtime_running();
      const c3 = Ke.stackSave();
      try {
        const s4 = $n(4);
        return n3(s4, e4), r3(s4, a3), kn(t3, s4), o3(s4);
      } finally {
        lt.is_runtime_running() && Ke.stackRestore(c3), Pt(i3, "mono.callCsFunction:", s3);
      }
    };
  })(g2) : kr(g2) : (function(e3) {
    const t3 = e3.method, n3 = e3.arg_marshalers[0], r3 = e3.fullyQualifiedName;
    return e3 = null, function(e4) {
      const o3 = Ft();
      lt.assert_runtime_running();
      const s3 = Ke.stackSave();
      try {
        const r4 = $n(3);
        n3(r4, e4), kn(t3, r4);
      } finally {
        lt.is_runtime_running() && Ke.stackRestore(s3), Pt(o3, "mono.callCsFunction:", r3);
      }
    };
  })(g2) : (function(e3) {
    const t3 = e3.method, n3 = e3.fullyQualifiedName;
    return e3 = null, function() {
      const e4 = Ft();
      lt.assert_runtime_running();
      const r3 = Ke.stackSave();
      try {
        const e5 = $n(2);
        kn(t3, e5);
      } finally {
        lt.is_runtime_running() && Ke.stackRestore(r3), Pt(e4, "mono.callCsFunction:", n3);
      }
    };
  })(g2), b2[Tn] = g2, (function(e3, t3, n3, r3, o3, s3) {
    const a3 = `${t3}.${n3}`.replace(/\//g, ".").split(".");
    let i3, c3 = Sr.get(e3);
    c3 || (c3 = {}, Sr.set(e3, c3), Sr.set(e3 + ".dll", c3)), i3 = c3;
    for (let e4 = 0; e4 < a3.length; e4++) {
      const t4 = a3[e4];
      if ("" != t4) {
        let e5 = i3[t4];
        void 0 === e5 && (e5 = {}, i3[t4] = e5), e5 || mt(false, `${t4} not found while looking up ${n3}`), i3 = e5;
      }
    }
    i3[r3] || (i3[r3] = s3), i3[`${r3}.${o3}`] = s3;
  })(t2, n2, r2, o2, s2, b2), Pt(c2, "mono.bindCsFunction:", i2);
}
function kr(e2) {
  const t2 = e2.args_count, n2 = e2.arg_marshalers, r2 = e2.res_converter, o2 = e2.method, s2 = e2.fullyQualifiedName, a2 = e2.is_async, i2 = e2.is_discard_no_wait;
  return e2 = null, function(...e3) {
    const c2 = Ft();
    lt.assert_runtime_running();
    const l2 = Ke.stackSave();
    try {
      const s3 = $n(2 + t2);
      for (let r3 = 0; r3 < t2; r3++) {
        const t3 = n2[r3];
        t3 && t3(s3, e3[r3]);
      }
      let c3;
      return a2 && (c3 = r2(s3)), a2 ? (wn(ct.managedThreadTID, o2, s3), c3 = an(s3, void 0, c3)) : i2 ? wn(ct.managedThreadTID, o2, s3) : (kn(o2, s3), r2 && (c3 = r2(s3))), c3;
    } finally {
      lt.is_runtime_running() && Ke.stackRestore(l2), Pt(c2, "mono.callCsFunction:", s2);
    }
  };
}
const Sr = /* @__PURE__ */ new Map();
async function vr(e2) {
  return hr(), Sr.get(e2) || await (function(e3) {
    lt.assert_runtime_running();
    const t2 = Ke.stackSave();
    try {
      const t3 = $n(3), n2 = Ln(t3, 1);
      mo(Ln(t3, 2), e3);
      let r2 = sn(n2);
      return wn(ct.managedThreadTID, yn.BindAssemblyExports, t3), r2 = an(t3, Jt, r2), null == r2 && (r2 = Promise.resolve()), r2;
    } finally {
      lt.is_runtime_running() && Ke.stackRestore(t2);
    }
  })(e2), Sr.get(e2) || {};
}
const Ur = "function" == typeof globalThis.FinalizationRegistry;
let Tr;
const Er = [null], xr = [null], Ir = [];
let Ar = 1;
const jr = /* @__PURE__ */ new Map(), $r = [];
let Lr = -2;
function Rr(e2) {
  return e2 < -1;
}
function Br(e2) {
  return e2 > 0;
}
function Nr(e2) {
  return e2 < -1;
}
Ur && (Tr = new globalThis.FinalizationRegistry(Hr));
const Or = /* @__PURE__ */ Symbol.for("wasm js_owned_gc_handle"), Cr = /* @__PURE__ */ Symbol.for("wasm cs_owned_js_handle"), Dr = /* @__PURE__ */ Symbol.for("wasm do_not_force_dispose");
function Fr(e2) {
  return Br(e2) ? Er[e2] : Rr(e2) ? xr[0 - e2] : null;
}
function Pr(e2) {
  if (hr(), e2[Cr]) return e2[Cr];
  const t2 = Ir.length ? Ir.pop() : Ar++;
  return Er[t2] = e2, Object.isExtensible(e2) && ("function" == typeof e2 && Object.prototype.hasOwnProperty.call(e2, "prototype") || (e2[Cr] = t2)), t2;
}
function Mr(e2) {
  let t2;
  Br(e2) ? (t2 = Er[e2], Er[e2] = void 0, Ir.push(e2)) : Rr(e2) && (t2 = xr[0 - e2], xr[0 - e2] = void 0), null == t2 && mt(false, "ObjectDisposedException"), void 0 !== t2[Cr] && (t2[Cr] = void 0);
}
function zr(e2, t2) {
  hr(), e2[Or] = t2, Ur && Tr.register(e2, t2, e2);
  const n2 = yr(e2);
  jr.set(t2, n2);
}
function Vr(e2, t2, r2) {
  var o2;
  hr(), e2 && (t2 = e2[Or], e2[Or] = u, Ur && Tr.unregister(e2)), t2 !== u && jr.delete(t2) && !r2 && lt.is_runtime_running() && !Gr && (function(e3) {
    e3 || mt(false, "Must be valid gc_handle"), lt.assert_runtime_running();
    const t3 = Ke.stackSave();
    try {
      const t4 = $n(3), r3 = Ln(t4, 2);
      Wn(r3, 14), Kn(r3, e3), n && !Nr(e3) && bn.isUI || kn(yn.ReleaseJSOwnedObjectByGCHandle, t4);
    } finally {
      lt.is_runtime_running() && Ke.stackRestore(t3);
    }
  })(t2), Nr(t2) && (o2 = t2, $r.push(o2));
}
function Wr(e2) {
  const t2 = e2[Or];
  if (t2 == u) throw new Error("Assert failed: ObjectDisposedException");
  return t2;
}
function Hr(e2) {
  lt.is_runtime_running() && Vr(null, e2);
}
function qr(e2) {
  if (!e2) return null;
  const t2 = jr.get(e2);
  return t2 ? t2.deref() : null;
}
let Gr = false;
function Jr(e2, t2) {
  let n2 = false, r2 = false;
  Gr = true;
  let o2 = 0, s2 = 0, a2 = 0, i2 = 0;
  const c2 = [...jr.keys()];
  for (const e3 of c2) {
    const r3 = jr.get(e3), o3 = r3 && r3.deref();
    if (Ur && o3 && Tr.unregister(o3), o3) {
      const s3 = "boolean" == typeof o3[Dr] && o3[Dr];
      if (t2 && We(`Proxy of C# ${typeof o3} with GCHandle ${e3} was still alive. ${s3 ? "keeping" : "disposing"}.`), s3) n2 = true;
      else {
        const t3 = lt.getPromiseController(o3);
        t3 && t3.reject(new Error("WebWorker which is origin of the Task is being terminated.")), "function" == typeof o3.dispose && o3.dispose(), o3[Or] === e3 && (o3[Or] = u), !br && r3 && r3.dispose(), a2++;
      }
    }
  }
  n2 || (jr.clear(), Ur && (Tr = new globalThis.FinalizationRegistry(Hr)));
  const l2 = (e3, n3) => {
    const o3 = n3[e3], s3 = o3 && "boolean" == typeof o3[Dr] && o3[Dr];
    if (s3 || (n3[e3] = void 0), o3) if (t2 && We(`Proxy of JS ${typeof o3} with JSHandle ${e3} was still alive. ${s3 ? "keeping" : "disposing"}.`), s3) r2 = true;
    else {
      const t3 = lt.getPromiseController(o3);
      t3 && t3.reject(new Error("WebWorker which is origin of the Task is being terminated.")), "function" == typeof o3.dispose && o3.dispose(), o3[Cr] === e3 && (o3[Cr] = void 0), i2++;
    }
  };
  for (let e3 = 0; e3 < Er.length; e3++) l2(e3, Er);
  for (let e3 = 0; e3 < xr.length; e3++) l2(e3, xr);
  if (r2 || (Er.length = 1, xr.length = 1, Ar = 1, Ir.length = 0), $r.length = 0, Lr = -2, e2) {
    for (const e4 of sr) if (e4) {
      const t3 = e4[xn];
      t3 && (t3.disposed = true, o2++);
    }
    sr.length = 1;
    const e3 = [...Sr.values()];
    for (const t3 of e3) for (const e4 in t3) {
      const n3 = t3[e4][Tn];
      n3 && (n3.disposed = true, s2++);
    }
    Sr.clear();
  }
  Ve(`forceDisposeProxies done: ${o2} imports, ${s2} exports, ${a2} GCHandles, ${i2} JSHandles.`);
}
function Xr(e2) {
  return Promise.resolve(e2) === e2 || ("object" == typeof e2 || "function" == typeof e2) && "function" == typeof e2.then;
}
function Qr(e2) {
  const { promise: t2, promise_control: n2 } = _t();
  return e2().then(((e3) => n2.resolve(e3))).catch(((e3) => n2.reject(e3))), t2;
}
const Yr = /* @__PURE__ */ Symbol.for("wasm promise_holder");
class Zr extends ManagedObject {
  constructor(e2, t2, n2, r2) {
    super(), this.promise = e2, this.gc_handle = t2, this.promiseHolderPtr = n2, this.res_converter = r2, this.isResolved = false, this.isPosted = false, this.isPostponed = false, this.data = null, this.reason = void 0;
  }
  setIsResolving() {
    return true;
  }
  resolve(e2) {
    lt.is_runtime_running() ? (this.isResolved && mt(false, "resolve could be called only once"), this.isDisposed && mt(false, "resolve is already disposed."), this.isResolved = true, this.complete_task_wrapper(e2, null)) : lt.diagnosticTracing && ze("This promise resolution can't be propagated to managed code, mono runtime already exited.");
  }
  reject(e2) {
    lt.is_runtime_running() ? (e2 || (e2 = new Error()), this.isResolved && mt(false, "reject could be called only once"), this.isDisposed && mt(false, "resolve is already disposed."), e2[Yr], this.isResolved = true, this.complete_task_wrapper(null, e2)) : lt.diagnosticTracing && ze("This promise rejection can't be propagated to managed code, mono runtime already exited.");
  }
  cancel() {
    if (lt.is_runtime_running()) if (this.isResolved && mt(false, "cancel could be called only once"), this.isDisposed && mt(false, "resolve is already disposed."), this.isPostponed) this.isResolved = true, void 0 !== this.reason ? this.complete_task_wrapper(null, this.reason) : this.complete_task_wrapper(this.data, null);
    else {
      const e2 = this.promise;
      lt.assertIsControllablePromise(e2);
      const t2 = lt.getPromiseController(e2), n2 = new Error("OperationCanceledException");
      n2[Yr] = this, t2.reject(n2);
    }
    else lt.diagnosticTracing && ze("This promise cancelation can't be propagated to managed code, mono runtime already exited.");
  }
  complete_task_wrapper(e2, t2) {
    try {
      this.isPosted && mt(false, "Promise is already posted to managed."), this.isPosted = true, Vr(this, this.gc_handle, true), (function(e3, t3, n2, r2) {
        lt.assert_runtime_running();
        const o2 = Ke.stackSave();
        try {
          const o3 = $n(5), s2 = Ln(o3, 2);
          Wn(s2, 14), Kn(s2, e3);
          const a2 = Ln(o3, 3);
          if (t3) wo(a2, t3);
          else {
            Wn(a2, 0);
            const e4 = Ln(o3, 4);
            r2 || mt(false, "res_converter missing"), r2(e4, n2);
          }
          wn(ct.ioThreadTID, yn.CompleteTask, o3);
        } finally {
          lt.is_runtime_running() && Ke.stackRestore(o2);
        }
      })(this.gc_handle, t2, e2, this.res_converter || So);
    } catch (e3) {
      try {
        lt.mono_exit(1, e3);
      } catch (e4) {
      }
    }
  }
}
const Kr = "For more information see https://aka.ms/dotnet-wasm-jsinterop";
function eo(e2, t2, n2) {
  if (0 === t2 || 1 === t2 || 2 === t2 || 26 === t2) return;
  let r2, o2, s2, a2;
  o2 = Vt(Cn(e2)), s2 = Vt(Dn(e2)), a2 = Vt(Fn(e2));
  const i2 = On(e2);
  r2 = to(i2), 19 === t2 && (t2 = i2);
  const c2 = to(t2), l2 = Cn(e2), p2 = n2 * In;
  return (e3, t3) => {
    c2(e3 + p2, t3, l2, r2, o2, s2, a2);
  };
}
function to(e2) {
  if (0 === e2 || 1 === e2) return;
  const t2 = Un.get(e2);
  return t2 && "function" == typeof t2 || mt(false, `ERR30: Unknown converter for type ${e2}`), t2;
}
function no(e2, t2) {
  null == t2 ? Wn(e2, 0) : (Wn(e2, 3), qn(e2, t2));
}
function ro(e2, t2) {
  null == t2 ? Wn(e2, 0) : (Wn(e2, 4), (function(e3, t3) {
    e3 || mt(false, "Null arg"), w(e3, t3);
  })(e2, t2));
}
function oo(e2, t2) {
  null == t2 ? Wn(e2, 0) : (Wn(e2, 5), (function(e3, t3) {
    e3 || mt(false, "Null arg"), k(e3, t3);
  })(e2, t2));
}
function so(e2, t2) {
  null == t2 ? Wn(e2, 0) : (Wn(e2, 6), (function(e3, t3) {
    e3 || mt(false, "Null arg"), U(e3, t3);
  })(e2, t2));
}
function ao(e2, t2) {
  null == t2 ? Wn(e2, 0) : (Wn(e2, 7), (function(e3, t3) {
    e3 || mt(false, "Null arg"), T(e3, t3);
  })(e2, t2));
}
function io(e2, t2) {
  null == t2 ? Wn(e2, 0) : (Wn(e2, 8), (function(e3, t3) {
    if (e3 || mt(false, "Null arg"), !Number.isSafeInteger(t3)) throw new Error(`Assert failed: Value is not an integer: ${t3} (${typeof t3})`);
    $(e3, t3);
  })(e2, t2));
}
function co(e2, t2) {
  null == t2 ? Wn(e2, 0) : (Wn(e2, 9), (function(e3, t3) {
    e3 || mt(false, "Null arg"), A(e3, t3);
  })(e2, t2));
}
function lo(e2, t2) {
  null == t2 ? Wn(e2, 0) : (Wn(e2, 10), Xn(e2, t2));
}
function po(e2, t2) {
  null == t2 ? Wn(e2, 0) : (Wn(e2, 11), (function(e3, t3) {
    e3 || mt(false, "Null arg"), j(e3, t3);
  })(e2, t2));
}
function uo(e2, t2) {
  null == t2 ? Wn(e2, 0) : (Wn(e2, 12), Gn(e2, t2));
}
function fo(e2, t2) {
  if (null == t2) Wn(e2, 0);
  else {
    if (!(t2 instanceof Date)) throw new Error("Assert failed: Value is not a Date");
    Wn(e2, 17), Jn(e2, t2);
  }
}
function _o(e2, t2) {
  if (null == t2) Wn(e2, 0);
  else {
    if (!(t2 instanceof Date)) throw new Error("Assert failed: Value is not a Date");
    Wn(e2, 18), Jn(e2, t2);
  }
}
function mo(e2, t2) {
  if (null == t2) Wn(e2, 0);
  else {
    if (Wn(e2, 15), "string" != typeof t2) throw new Error("Assert failed: Value is not a String");
    ho(e2, t2);
  }
}
function ho(e2, t2) {
  {
    const n2 = er(e2);
    try {
      !(function(e3, t3) {
        if (t3.clear(), null !== e3) if ("symbol" == typeof e3) Ce(e3, t3);
        else {
          if ("string" != typeof e3) throw new Error("Expected string argument, got " + typeof e3);
          if (0 === e3.length) Ce(e3, t3);
          else {
            if (e3.length <= 256) {
              const n3 = be.get(e3);
              if (n3) return void t3.set(n3);
            }
            De(e3, t3);
          }
        }
      })(t2, n2);
    } finally {
      n2.release();
    }
  }
}
function go(e2) {
  Wn(e2, 0);
}
function bo(e2, t2, r2, o2, s2, a2, i2) {
  if (null == t2) return void Wn(e2, 0);
  if (!(t2 && t2 instanceof Function)) throw new Error("Assert failed: Value is not a Function");
  const c2 = function(e3) {
    const r3 = Ln(e3, 0), l2 = Ln(e3, 1), p2 = Ln(e3, 2), u2 = Ln(e3, 3), d2 = Ln(e3, 4), f2 = ct.isPendingSynchronousCall;
    try {
      let e4, r4, f3;
      n && c2.isDisposed, s2 && (e4 = s2(p2)), a2 && (r4 = a2(u2)), i2 && (f3 = i2(d2)), ct.isPendingSynchronousCall = true;
      const _2 = t2(e4, r4, f3);
      o2 && o2(l2, _2);
    } catch (e4) {
      wo(r3, e4);
    } finally {
      ct.isPendingSynchronousCall = f2;
    }
  };
  c2[En] = true, c2.isDisposed = false, c2.dispose = () => {
    c2.isDisposed = true;
  }, Yn(e2, Pr(c2)), Wn(e2, 25);
}
function yo(e2, t2, n2, r2) {
  const o2 = 30 == zn(e2);
  if (null == t2) return void Wn(e2, 0);
  if (!Xr(t2)) throw new Error("Assert failed: Value is not a Promise");
  const s2 = o2 ? Zn(e2) : $r.length ? $r.pop() : Lr--;
  o2 || (Kn(e2, s2), Wn(e2, 20));
  const a2 = new Zr(t2, s2, 0, r2);
  zr(a2, s2), t2.then(((e3) => a2.resolve(e3)), ((e3) => a2.reject(e3)));
}
function wo(e2, t2) {
  if (null == t2) Wn(e2, 0);
  else if (t2 instanceof ManagedError) Wn(e2, 16), Kn(e2, Wr(t2));
  else {
    if ("object" != typeof t2 && "string" != typeof t2) throw new Error("Assert failed: Value is not an Error " + typeof t2);
    Wn(e2, 27), ho(e2, t2.toString());
    const n2 = t2[Cr];
    Yn(e2, n2 || Pr(t2));
  }
}
function ko(e2, t2) {
  if (null == t2) Wn(e2, 0);
  else {
    if (void 0 !== t2[Or]) throw new Error(`Assert failed: JSObject proxy of ManagedObject proxy is not supported. ${Kr}`);
    if ("function" != typeof t2 && "object" != typeof t2) throw new Error(`Assert failed: JSObject proxy of ${typeof t2} is not supported`);
    Wn(e2, 13), Yn(e2, Pr(t2));
  }
}
function So(e2, t2) {
  if (null == t2) Wn(e2, 0);
  else {
    const n2 = t2[Or], r2 = typeof t2;
    if (void 0 === n2) if ("string" === r2 || "symbol" === r2) Wn(e2, 15), ho(e2, t2);
    else if ("number" === r2) Wn(e2, 10), Xn(e2, t2);
    else {
      if ("bigint" === r2) throw new Error("NotImplementedException: bigint");
      if ("boolean" === r2) Wn(e2, 3), qn(e2, t2);
      else if (t2 instanceof Date) Wn(e2, 17), Jn(e2, t2);
      else if (t2 instanceof Error) wo(e2, t2);
      else if (t2 instanceof Uint8Array) Uo(e2, t2, 4);
      else if (t2 instanceof Float64Array) Uo(e2, t2, 10);
      else if (t2 instanceof Int32Array) Uo(e2, t2, 7);
      else if (Array.isArray(t2)) Uo(e2, t2, 14);
      else {
        if (t2 instanceof Int16Array || t2 instanceof Int8Array || t2 instanceof Uint8ClampedArray || t2 instanceof Uint16Array || t2 instanceof Uint32Array || t2 instanceof Float32Array) throw new Error("NotImplementedException: TypedArray");
        if (Xr(t2)) yo(e2, t2);
        else {
          if (t2 instanceof Span) throw new Error("NotImplementedException: Span");
          if ("object" != r2) throw new Error(`JSObject proxy is not supported for ${r2} ${t2}`);
          {
            const n3 = Pr(t2);
            Wn(e2, 13), Yn(e2, n3);
          }
        }
      }
    }
    else {
      if (Wr(t2), t2 instanceof ArraySegment) throw new Error("NotImplementedException: ArraySegment. " + Kr);
      if (t2 instanceof ManagedError) Wn(e2, 16), Kn(e2, n2);
      else {
        if (!(t2 instanceof ManagedObject)) throw new Error("NotImplementedException " + r2 + ". " + Kr);
        Wn(e2, 14), Kn(e2, n2);
      }
    }
  }
}
function vo(e2, t2, n2) {
  n2 || mt(false, "Expected valid element_type parameter"), Uo(e2, t2, n2);
}
function Uo(e2, t2, n2) {
  if (null == t2) Wn(e2, 0);
  else {
    const r2 = rr(n2);
    -1 == r2 && mt(false, `Element type ${n2} not supported`);
    const s2 = t2.length, a2 = r2 * s2, i2 = _(a2);
    if (15 == n2) {
      if (!Array.isArray(t2)) throw new Error("Assert failed: Value is not an Array");
      g(i2, a2), o.mono_wasm_register_root(i2, a2, "marshal_array_to_cs");
      for (let e3 = 0; e3 < s2; e3++) mo(Ln(i2, e3), t2[e3]);
    } else if (14 == n2) {
      if (!Array.isArray(t2)) throw new Error("Assert failed: Value is not an Array");
      g(i2, a2), o.mono_wasm_register_root(i2, a2, "marshal_array_to_cs");
      for (let e3 = 0; e3 < s2; e3++) So(Ln(i2, e3), t2[e3]);
    } else if (13 == n2) {
      if (!Array.isArray(t2)) throw new Error("Assert failed: Value is not an Array");
      g(i2, a2);
      for (let e3 = 0; e3 < s2; e3++) ko(Ln(i2, e3), t2[e3]);
    } else if (4 == n2) {
      if (!(Array.isArray(t2) || t2 instanceof Uint8Array)) throw new Error("Assert failed: Value is not an Array or Uint8Array");
      K().subarray(i2, i2 + s2).set(t2);
    } else if (7 == n2) {
      if (!(Array.isArray(t2) || t2 instanceof Int32Array)) throw new Error("Assert failed: Value is not an Array or Int32Array");
      const e3 = oe(i2, 2);
      Y().subarray(e3, e3 + s2).set(t2);
    } else {
      if (10 != n2) throw new Error("not implemented");
      {
        if (!(Array.isArray(t2) || t2 instanceof Float64Array)) throw new Error("Assert failed: Value is not an Array or Float64Array");
        const e3 = oe(i2, 3);
        re().subarray(e3, e3 + s2).set(t2);
      }
    }
    Gn(e2, i2), Wn(e2, 21), (function(e3, t3) {
      e3 || mt(false, "Null arg"), w(e3 + 13, t3);
    })(e2, n2), nr(e2, t2.length);
  }
}
function To(e2, t2, n2) {
  if (n2 || mt(false, "Expected valid element_type parameter"), t2.isDisposed) throw new Error("Assert failed: ObjectDisposedException");
  xo(n2, t2._viewType), Wn(e2, 23), Gn(e2, t2._pointer), nr(e2, t2.length);
}
function Eo(e2, t2, n2) {
  n2 || mt(false, "Expected valid element_type parameter");
  const r2 = Wr(t2);
  r2 || mt(false, "Only roundtrip of ArraySegment instance created by C#"), xo(n2, t2._viewType), Wn(e2, 22), Gn(e2, t2._pointer), nr(e2, t2.length), Kn(e2, r2);
}
function xo(e2, t2) {
  if (4 == e2) {
    if (0 != t2) throw new Error("Assert failed: Expected MemoryViewType.Byte");
  } else if (7 == e2) {
    if (1 != t2) throw new Error("Assert failed: Expected MemoryViewType.Int32");
  } else {
    if (10 != e2) throw new Error(`NotImplementedException ${e2} `);
    if (2 != t2) throw new Error("Assert failed: Expected MemoryViewType.Double");
  }
}
const Io = { now: function() {
  return Date.now();
} };
function Ao(e2) {
  void 0 === globalThis.performance && (globalThis.performance = Io), e2.require = et.require, e2.scriptDirectory = lt.scriptDirectory, Ke.locateFile === Ke.__locateFile && (Ke.locateFile = lt.locateFile), e2.fetch = lt.fetch_like, e2.ENVIRONMENT_IS_WORKER = ot;
}
function jo() {
  if ("function" != typeof globalThis.fetch || "function" != typeof globalThis.AbortController) throw new Error(tt ? "Please install `node-fetch` and `node-abort-controller` npm packages to enable HTTP client support. See also https://aka.ms/dotnet-wasm-features" : "This browser doesn't support fetch API. Please use a modern browser. See also https://aka.ms/dotnet-wasm-features");
}
function $o() {
  if ("undefined" != typeof Request && "body" in Request.prototype && "function" == typeof ReadableStream && "function" == typeof TransformStream) {
    let e2 = false;
    const t2 = new Request("", { body: new ReadableStream(), method: "POST", get duplex() {
      return e2 = true, "half";
    } }).headers.has("Content-Type");
    return e2 && !t2;
  }
  return false;
}
function Lo() {
  return "undefined" != typeof Response && "body" in Response.prototype && "function" == typeof ReadableStream;
}
function Ro() {
  return jo(), hr(), { abortController: new AbortController() };
}
function Bo(e2) {
  e2.catch(((e3) => {
    e3 && "AbortError" !== e3 && "AbortError" !== e3.name && ze("http muted: " + e3);
  }));
}
function No(e2) {
  try {
    e2.isAborted || (e2.streamWriter && (Bo(e2.streamWriter.abort()), e2.isAborted = true), e2.streamReader && (Bo(e2.streamReader.cancel()), e2.isAborted = true)), e2.isAborted || e2.abortController.signal.aborted || e2.abortController.abort("AbortError");
  } catch (e3) {
  }
}
function Oo(e2, t2, n2) {
  n2 > 0 || mt(false, "expected bufferLength > 0");
  const r2 = new Span(t2, n2, 0).slice();
  return Qr((async () => {
    e2.streamWriter || mt(false, "expected streamWriter"), e2.responsePromise || mt(false, "expected fetch promise");
    try {
      await e2.streamWriter.ready, await e2.streamWriter.write(r2);
    } catch (e3) {
      throw new Error("BrowserHttpWriteStream.Rejected");
    }
  }));
}
function Co(e2) {
  return e2 || mt(false, "expected controller"), Qr((async () => {
    e2.streamWriter || mt(false, "expected streamWriter"), e2.responsePromise || mt(false, "expected fetch promise");
    try {
      await e2.streamWriter.ready, await e2.streamWriter.close();
    } catch (e3) {
      throw new Error("BrowserHttpWriteStream.Rejected");
    }
  }));
}
function Do(e2, t2, n2, r2, o2, s2) {
  const a2 = new TransformStream();
  return e2.streamWriter = a2.writable.getWriter(), Bo(e2.streamWriter.closed), Bo(e2.streamWriter.ready), Po(e2, t2, n2, r2, o2, s2, a2.readable);
}
function Fo(e2, t2, n2, r2, o2, s2, a2, i2) {
  return Po(e2, t2, n2, r2, o2, s2, new Span(a2, i2, 0).slice());
}
function Po(e2, t2, n2, r2, o2, s2, a2) {
  jo(), hr(), t2 && "string" == typeof t2 || mt(false, "expected url string"), n2 && r2 && Array.isArray(n2) && Array.isArray(r2) && n2.length === r2.length || mt(false, "expected headerNames and headerValues arrays"), o2 && s2 && Array.isArray(o2) && Array.isArray(s2) && o2.length === s2.length || mt(false, "expected headerNames and headerValues arrays");
  const i2 = new Headers();
  for (let e3 = 0; e3 < n2.length; e3++) i2.append(n2[e3], r2[e3]);
  const c2 = { body: a2, headers: i2, signal: e2.abortController.signal };
  "undefined" != typeof ReadableStream && a2 instanceof ReadableStream && (c2.duplex = "half");
  for (let e3 = 0; e3 < o2.length; e3++) c2[o2[e3]] = s2[e3];
  return e2.responsePromise = Qr((() => lt.fetch_like(t2, c2).then(((t3) => (e2.response = t3, null))))), e2.responsePromise.then((() => {
    if (e2.response || mt(false, "expected response"), e2.responseHeaderNames = [], e2.responseHeaderValues = [], e2.response.headers && e2.response.headers.entries) {
      const t3 = e2.response.headers.entries();
      for (const n3 of t3) e2.responseHeaderNames.push(n3[0]), e2.responseHeaderValues.push(n3[1]);
    }
  })).catch((() => {
  })), e2.responsePromise;
}
function Mo(e2) {
  var t2;
  return null === (t2 = e2.response) || void 0 === t2 ? void 0 : t2.type;
}
function zo(e2) {
  var t2, n2;
  return null !== (n2 = null === (t2 = e2.response) || void 0 === t2 ? void 0 : t2.status) && void 0 !== n2 ? n2 : 0;
}
function Vo(e2) {
  return e2.responseHeaderNames || mt(false, "expected responseHeaderNames"), e2.responseHeaderNames;
}
function Wo(e2) {
  return e2.responseHeaderValues || mt(false, "expected responseHeaderValues"), e2.responseHeaderValues;
}
function Ho(e2) {
  return Qr((async () => {
    const t2 = await e2.response.arrayBuffer();
    return e2.responseBuffer = t2, e2.currentBufferOffset = 0, t2.byteLength;
  }));
}
function qo(e2, t2) {
  if (e2 || mt(false, "expected controller"), e2.responseBuffer || mt(false, "expected resoved arrayBuffer"), null == e2.currentBufferOffset && mt(false, "expected currentBufferOffset"), e2.currentBufferOffset == e2.responseBuffer.byteLength) return 0;
  const n2 = new Uint8Array(e2.responseBuffer, e2.currentBufferOffset);
  t2.set(n2, 0);
  const r2 = Math.min(t2.byteLength, n2.byteLength);
  return e2.currentBufferOffset += r2, r2;
}
function Go(e2, t2, n2) {
  const r2 = new Span(t2, n2, 0);
  return Qr((async () => {
    if (await e2.responsePromise, e2.response || mt(false, "expected response"), !e2.response.body) return 0;
    if (e2.streamReader || (e2.streamReader = e2.response.body.getReader(), Bo(e2.streamReader.closed)), e2.currentStreamReaderChunk && void 0 !== e2.currentBufferOffset || (e2.currentStreamReaderChunk = await e2.streamReader.read(), e2.currentBufferOffset = 0), e2.currentStreamReaderChunk.done) {
      if (e2.isAborted) throw new Error("OperationCanceledException");
      return 0;
    }
    const t3 = e2.currentStreamReaderChunk.value.byteLength - e2.currentBufferOffset;
    t3 > 0 || mt(false, "expected remaining_source to be greater than 0");
    const n3 = Math.min(t3, r2.byteLength), o2 = e2.currentStreamReaderChunk.value.subarray(e2.currentBufferOffset, e2.currentBufferOffset + n3);
    return r2.set(o2, 0), e2.currentBufferOffset += n3, t3 == n3 && (e2.currentStreamReaderChunk = void 0), n3;
  }));
}
let Jo, Xo, Qo = 0;
function Yo() {
  if (!lt.isChromium) return;
  const e2 = (/* @__PURE__ */ new Date()).valueOf(), t2 = e2 + 36e4;
  for (let n2 = Math.max(e2 + 1e3, Qo); n2 < t2; n2 += 1e3) {
    const t3 = n2 - e2;
    globalThis.setTimeout(Zo, t3);
  }
  Qo = t2;
}
function Zo() {
  if (Ke.maybeExit(), lt.is_runtime_running()) {
    try {
      o.mono_wasm_execute_timer();
    } catch (e2) {
      lt.mono_exit(1, e2);
    }
    Ko();
  }
}
function Ko() {
  if (Jo = void 0, Ke.maybeExit(), lt.is_runtime_running()) try {
    o.mono_background_exec();
  } catch (e2) {
    lt.mono_exit(1, e2);
  }
}
function mono_wasm_schedule_timer_tick() {
  if (Ke.maybeExit(), lt.is_runtime_running()) {
    Xo = void 0;
    try {
      o.mono_wasm_execute_timer();
    } catch (e2) {
      lt.mono_exit(1, e2);
    }
  }
}
class es {
  constructor() {
    this.queue = [], this.offset = 0;
  }
  getLength() {
    return this.queue.length - this.offset;
  }
  isEmpty() {
    return 0 == this.queue.length;
  }
  enqueue(e2) {
    this.queue.push(e2);
  }
  dequeue() {
    if (0 === this.queue.length) return;
    const e2 = this.queue[this.offset];
    return this.queue[this.offset] = null, 2 * ++this.offset >= this.queue.length && (this.queue = this.queue.slice(this.offset), this.offset = 0), e2;
  }
  peek() {
    return this.queue.length > 0 ? this.queue[this.offset] : void 0;
  }
  drain(e2) {
    for (; this.getLength(); ) e2(this.dequeue());
  }
}
const ts = /* @__PURE__ */ Symbol.for("wasm ws_pending_send_buffer"), ns = /* @__PURE__ */ Symbol.for("wasm ws_pending_send_buffer_offset"), rs = /* @__PURE__ */ Symbol.for("wasm ws_pending_send_buffer_type"), os = /* @__PURE__ */ Symbol.for("wasm ws_pending_receive_event_queue"), ss = /* @__PURE__ */ Symbol.for("wasm ws_pending_receive_promise_queue"), as = /* @__PURE__ */ Symbol.for("wasm ws_pending_open_promise"), is = /* @__PURE__ */ Symbol.for("wasm wasm_ws_pending_open_promise_used"), cs = /* @__PURE__ */ Symbol.for("wasm wasm_ws_pending_error"), ls = /* @__PURE__ */ Symbol.for("wasm ws_pending_close_promises"), ps = /* @__PURE__ */ Symbol.for("wasm ws_pending_send_promises"), us = /* @__PURE__ */ Symbol.for("wasm ws_is_aborted"), ds = /* @__PURE__ */ Symbol.for("wasm wasm_ws_close_sent"), fs = /* @__PURE__ */ Symbol.for("wasm wasm_ws_close_received"), _s = /* @__PURE__ */ Symbol.for("wasm ws_receive_status_ptr"), ms = 65536, hs = new Uint8Array();
function gs(e2) {
  var t2, n2;
  return e2.readyState != WebSocket.CLOSED ? null !== (t2 = e2.readyState) && void 0 !== t2 ? t2 : -1 : 0 == e2[os].getLength() ? null !== (n2 = e2.readyState) && void 0 !== n2 ? n2 : -1 : e2[ds] ? WebSocket.CLOSING : WebSocket.OPEN;
}
function bs(e2, t2, n2) {
  let r2;
  !(function() {
    if (at) throw new Error("WebSockets are not supported in shell JS engine.");
    if ("function" != typeof globalThis.WebSocket) throw new Error(tt ? "Please install `ws` npm package to enable networking support. See also https://aka.ms/dotnet-wasm-features" : "This browser doesn't support WebSocket API. Please use a modern browser. See also https://aka.ms/dotnet-wasm-features");
  })(), hr(), e2 && "string" == typeof e2 || mt(false, "ERR12: Invalid uri " + typeof e2);
  try {
    r2 = new globalThis.WebSocket(e2, t2 || void 0);
  } catch (e3) {
    throw We("WebSocket error in ws_wasm_create: " + e3.toString()), e3;
  }
  const { promise_control: o2 } = _t();
  r2[os] = new es(), r2[ss] = new es(), r2[as] = o2, r2[ps] = [], r2[ls] = [], r2[_s] = n2, r2.binaryType = "arraybuffer";
  const s2 = () => {
    try {
      if (r2[us]) return;
      if (!lt.is_runtime_running()) return;
      o2.resolve(r2), Yo();
    } catch (e3) {
      We("failed to propagate WebSocket open event: " + e3.toString());
    }
  }, a2 = (e3) => {
    try {
      if (r2[us]) return;
      if (!lt.is_runtime_running()) return;
      !(function(e4, t3) {
        const n3 = e4[os], r3 = e4[ss];
        if ("string" == typeof t3.data) n3.enqueue({ type: 0, data: je(t3.data), offset: 0 });
        else {
          if ("ArrayBuffer" !== t3.data.constructor.name) throw new Error("ERR19: WebSocket receive expected ArrayBuffer");
          n3.enqueue({ type: 1, data: new Uint8Array(t3.data), offset: 0 });
        }
        if (r3.getLength() && n3.getLength() > 1) throw new Error("ERR21: Invalid WS state");
        for (; r3.getLength() && n3.getLength(); ) {
          const t4 = r3.dequeue();
          Ts(e4, n3, t4.buffer_ptr, t4.buffer_length), t4.resolve();
        }
        Yo();
      })(r2, e3), Yo();
    } catch (e4) {
      We("failed to propagate WebSocket message event: " + e4.toString());
    }
  }, i2 = (e3) => {
    try {
      if (r2.removeEventListener("message", a2), r2[us]) return;
      if (!lt.is_runtime_running()) return;
      r2[fs] = true, r2.close_status = e3.code, r2.close_status_description = e3.reason, r2[is] && o2.reject(new Error(e3.reason));
      for (const e4 of r2[ls]) e4.resolve();
      Ke.safeSetTimeout((() => {
        r2[ss].drain(((e4) => {
          T(n2, 0), T(n2 + 4, 2), T(n2 + 8, 1), e4.resolve();
        }));
      }), 0);
    } catch (e4) {
      We("failed to propagate WebSocket close event: " + e4.toString());
    }
  }, c2 = (e3) => {
    try {
      if (r2[us]) return;
      if (!lt.is_runtime_running()) return;
      r2.removeEventListener("message", a2);
      const t3 = e3.message ? "WebSocket error: " + e3.message : "WebSocket error";
      We(t3), r2[cs] = t3, Us(r2, new Error(t3));
    } catch (e4) {
      We("failed to propagate WebSocket error event: " + e4.toString());
    }
  };
  return r2.addEventListener("message", a2), r2.addEventListener("open", s2, { once: true }), r2.addEventListener("close", i2, { once: true }), r2.addEventListener("error", c2, { once: true }), r2.dispose = () => {
    r2.removeEventListener("message", a2), r2.removeEventListener("open", s2), r2.removeEventListener("close", i2), r2.removeEventListener("error", c2), vs(r2);
  }, r2;
}
function ys(e2) {
  if (e2 || mt(false, "ERR17: expected ws instance"), e2[cs]) return Es(e2[cs]);
  const t2 = e2[as];
  return e2[is] = true, t2.promise;
}
function ws(e2, t2, n2, r2, o2) {
  if (e2 || mt(false, "ERR17: expected ws instance"), e2[cs]) return Es(e2[cs]);
  if (e2[us] || e2[ds]) return Es("InvalidState: The WebSocket is not connected.");
  if (e2.readyState == WebSocket.CLOSED) return null;
  const s2 = (function(e3, t3, n3, r3) {
    let o3 = e3[ts], s3 = 0;
    const a2 = t3.byteLength;
    if (o3) {
      if (s3 = e3[ns], n3 = e3[rs], 0 !== a2) {
        if (s3 + a2 > o3.length) {
          const n4 = new Uint8Array(1.5 * (s3 + a2 + 50));
          n4.set(o3, 0), n4.subarray(s3).set(t3), e3[ts] = o3 = n4;
        } else o3.subarray(s3).set(t3);
        s3 += a2, e3[ns] = s3;
      }
    } else r3 ? 0 !== a2 && (o3 = t3, s3 = a2) : (0 !== a2 && (o3 = t3.slice(), s3 = a2, e3[ns] = s3, e3[ts] = o3), e3[rs] = n3);
    return r3 ? 0 == s3 || null == o3 ? hs : 0 === n3 ? (function(e4) {
      return void 0 === ve ? Ke.UTF8ArrayToString(e4, 0, e4.byteLength) : ve.decode(e4);
    })(Fe(o3, 0, s3)) : o3.subarray(0, s3) : null;
  })(e2, new Uint8Array(K().buffer, t2, n2), r2, o2);
  return o2 && s2 ? (function(e3, t3) {
    if (e3.send(t3), e3[ts] = null, e3.bufferedAmount < ms) return null;
    const { promise: n3, promise_control: r3 } = _t(), o3 = e3[ps];
    o3.push(r3);
    let s3 = 1;
    const a2 = () => {
      try {
        if (0 === e3.bufferedAmount) r3.resolve();
        else {
          const t5 = e3.readyState;
          if (t5 != WebSocket.OPEN && t5 != WebSocket.CLOSING) r3.reject(new Error(`InvalidState: ${t5} The WebSocket is not connected.`));
          else if (!r3.isDone) return globalThis.setTimeout(a2, s3), void (s3 = Math.min(1.5 * s3, 1e3));
        }
        const t4 = o3.indexOf(r3);
        t4 > -1 && o3.splice(t4, 1);
      } catch (e4) {
        We("WebSocket error in web_socket_send_and_wait: " + e4.toString()), r3.reject(e4);
      }
    };
    return globalThis.setTimeout(a2, 0), n3;
  })(e2, s2) : null;
}
function ks(e2, t2, n2) {
  if (e2 || mt(false, "ERR18: expected ws instance"), e2[cs]) return Es(e2[cs]);
  if (e2[us]) {
    const t3 = e2[_s];
    return T(t3, 0), T(t3 + 4, 2), T(t3 + 8, 1), null;
  }
  const r2 = e2[os], o2 = e2[ss];
  if (r2.getLength()) return 0 != o2.getLength() && mt(false, "ERR20: Invalid WS state"), Ts(e2, r2, t2, n2), null;
  if (e2[fs]) {
    const t3 = e2[_s];
    return T(t3, 0), T(t3 + 4, 2), T(t3 + 8, 1), null;
  }
  const { promise: s2, promise_control: a2 } = _t(), i2 = a2;
  return i2.buffer_ptr = t2, i2.buffer_length = n2, o2.enqueue(i2), s2;
}
function Ss(e2, t2, n2, r2) {
  if (e2 || mt(false, "ERR19: expected ws instance"), e2[us] || e2[ds] || e2.readyState == WebSocket.CLOSED) return null;
  if (e2[cs]) return Es(e2[cs]);
  if (e2[ds] = true, r2) {
    const { promise: r3, promise_control: o2 } = _t();
    return e2[ls].push(o2), "string" == typeof n2 ? e2.close(t2, n2) : e2.close(t2), r3;
  }
  return "string" == typeof n2 ? e2.close(t2, n2) : e2.close(t2), null;
}
function vs(e2) {
  if (e2 || mt(false, "ERR18: expected ws instance"), !e2[us] && !e2[ds]) {
    e2[us] = true, Us(e2, new Error("OperationCanceledException"));
    try {
      e2.close(1e3, "Connection was aborted.");
    } catch (e3) {
      We("WebSocket error in ws_wasm_abort: " + e3.toString());
    }
  }
}
function Us(e2, t2) {
  const n2 = e2[as], r2 = e2[is];
  n2 && r2 && n2.reject(t2);
  for (const n3 of e2[ls]) n3.reject(t2);
  for (const n3 of e2[ps]) n3.reject(t2);
  e2[ss].drain(((e3) => {
    e3.reject(t2);
  }));
}
function Ts(e2, t2, n2, r2) {
  const o2 = t2.peek(), s2 = Math.min(r2, o2.data.length - o2.offset);
  if (s2 > 0) {
    const e3 = o2.data.subarray(o2.offset, o2.offset + s2);
    new Uint8Array(K().buffer, n2, r2).set(e3, 0), o2.offset += s2;
  }
  const a2 = o2.data.length === o2.offset ? 1 : 0;
  a2 && t2.dequeue();
  const i2 = e2[_s];
  T(i2, s2), T(i2 + 4, o2.type), T(i2 + 8, a2);
}
function Es(e2) {
  return (function(e3) {
    const { promise: t2, promise_control: n2 } = _t();
    return e3.then(((e4) => n2.resolve(e4))).catch(((e4) => n2.reject(e4))), t2;
  })(Promise.reject(new Error(e2)));
}
function xs(e2, t2, n2) {
  lt.diagnosticTracing && ze(`Loaded:${e2.name} as ${e2.behavior} size ${n2.length} from ${t2}`);
  const r2 = Ft(), s2 = "string" == typeof e2.virtualPath ? e2.virtualPath : e2.name;
  let a2 = null;
  switch (e2.behavior) {
    case "dotnetwasm":
    case "js-module-threads":
    case "js-module-diagnostics":
    case "symbols":
      break;
    case "resource":
    case "assembly":
    case "pdb":
      lt._loaded_files.push({ url: t2, file: s2 });
    case "heap":
    case "icu":
      a2 = (function(e3) {
        const t3 = e3.length + 16;
        let n3 = Ke._sbrk(t3);
        if (n3 <= 0) {
          if (n3 = Ke._sbrk(t3), n3 <= 0) throw He(`sbrk failed to allocate ${t3} bytes, and failed upon retry.`), new Error("Out of memory");
          We(`sbrk failed to allocate ${t3} bytes, but succeeded upon retry!`);
        }
        return new Uint8Array(K().buffer, n3, e3.length).set(e3), n3;
      })(n2);
      break;
    case "vfs": {
      const e3 = s2.lastIndexOf("/");
      let t3 = e3 > 0 ? s2.substring(0, e3) : null, r3 = e3 > 0 ? s2.substring(e3 + 1) : s2;
      r3.startsWith("/") && (r3 = r3.substring(1)), t3 ? (t3.startsWith("/") || (t3 = "/" + t3), ze(`Creating directory '${t3}'`), Ke.FS_createPath("/", t3, true, true)) : t3 = "/", lt.diagnosticTracing && ze(`Creating file '${r3}' in directory '${t3}'`), Ke.FS_createDataFile(t3, r3, n2, true, true, true);
      break;
    }
    default:
      throw new Error(`Unrecognized asset behavior:${e2.behavior}, for asset ${e2.name}`);
  }
  if ("assembly" === e2.behavior) {
    if (!o.mono_wasm_add_assembly(s2, a2, n2.length)) {
      const e3 = lt._loaded_files.findIndex(((e4) => e4.file == s2));
      lt._loaded_files.splice(e3, 1);
    }
  } else "pdb" === e2.behavior ? o.mono_wasm_add_assembly(s2, a2, n2.length) : "icu" === e2.behavior ? (function(e3) {
    if (!o.mono_wasm_load_icu_data(e3)) throw new Error("Failed to load ICU data");
  })(a2) : "resource" === e2.behavior && o.mono_wasm_add_satellite_assembly(s2, e2.culture || "", a2, n2.length);
  Pt(r2, "mono.instantiateAsset:", e2.name), ++lt.actual_instantiated_assets_count;
}
async function Is(e2) {
  try {
    const n2 = await e2.pendingDownloadInternal.response;
    t2 = await n2.text(), Ge && mt(false, "Another symbol map was already loaded"), Ge = t2, lt.diagnosticTracing && ze(`Deferred loading of ${t2.length}ch symbol map`);
  } catch (t3) {
    Ve(`Error loading symbol file ${e2.name}: ${JSON.stringify(t3)}`);
  }
  var t2;
}
function As() {
  return lt.loadedFiles;
}
const js = {};
function $s(e2) {
  let t2 = js[e2];
  if ("string" != typeof t2) {
    const n2 = o.mono_jiterp_get_opcode_info(e2, 0);
    js[e2] = t2 = Le(n2);
  }
  return t2;
}
const Ls = 2, Rs = 64, Bs = 64, Ns = -2147483648, Os = {};
class Cs {
  constructor(e2) {
    this.locals = /* @__PURE__ */ new Map(), this.permanentFunctionTypeCount = 0, this.permanentFunctionTypes = {}, this.permanentFunctionTypesByShape = {}, this.permanentFunctionTypesByIndex = {}, this.functionTypesByIndex = {}, this.permanentImportedFunctionCount = 0, this.permanentImportedFunctions = {}, this.nextImportIndex = 0, this.functions = [], this.estimatedExportBytes = 0, this.frame = 0, this.traceBuf = [], this.branchTargets = /* @__PURE__ */ new Set(), this.constantSlots = [], this.backBranchOffsets = [], this.callHandlerReturnAddresses = [], this.nextConstantSlot = 0, this.backBranchTraceLevel = 0, this.compressImportNames = false, this.lockImports = false, this._assignParameterIndices = (e3) => {
      let t2 = 0;
      for (const n2 in e3) this.locals.set(n2, t2), t2++;
      return t2;
    }, this.stack = [new Ds()], this.clear(e2), this.cfg = new Fs(this), this.defineType("__cpp_exception", { ptr: 127 }, 64, true);
  }
  clear(e2) {
    if (this.options = fa(), this.options.maxModuleSize >= 24576) throw new Error(`blobBuilderCapacity 24576 is not large enough for jiterpreter-max-module-size of ${this.options.maxModuleSize}`);
    this.stackSize = 1, this.inSection = false, this.inFunction = false, this.lockImports = false, this.locals.clear(), this.functionTypeCount = this.permanentFunctionTypeCount, this.functionTypes = Object.create(this.permanentFunctionTypes), this.functionTypesByShape = Object.create(this.permanentFunctionTypesByShape), this.functionTypesByIndex = Object.create(this.permanentFunctionTypesByIndex), this.nextImportIndex = 0, this.importedFunctionCount = 0, this.importedFunctions = Object.create(this.permanentImportedFunctions);
    for (const e3 in this.importedFunctions) this.importedFunctions[e3].index = void 0;
    this.functions.length = 0, this.estimatedExportBytes = 0, this.argumentCount = 0, this.current.clear(), this.traceBuf.length = 0, this.branchTargets.clear(), this.activeBlocks = 0, this.nextConstantSlot = 0, this.constantSlots.length = this.options.useConstants ? e2 : 0;
    for (let e3 = 0; e3 < this.constantSlots.length; e3++) this.constantSlots[e3] = 0;
    this.backBranchOffsets.length = 0, this.callHandlerReturnAddresses.length = 0, this.allowNullCheckOptimization = this.options.eliminateNullChecks, this.containsSimd = false, this.containsAtomics = false;
  }
  _push() {
    this.stackSize++, this.stackSize >= this.stack.length && this.stack.push(new Ds()), this.current.clear();
  }
  _pop(e2) {
    if (this.stackSize <= 1) throw new Error("Stack empty");
    const t2 = this.current;
    return this.stackSize--, e2 ? (this.appendULeb(t2.size), t2.copyTo(this.current), null) : t2.getArrayView(false).slice(0, t2.size);
  }
  setImportFunction(e2, t2) {
    const n2 = this.importedFunctions[e2];
    if (!n2) throw new Error("No import named " + e2);
    n2.func = t2;
  }
  getExceptionTag() {
    const e2 = Ke.wasmExports.__cpp_exception;
    return void 0 !== e2 && (e2 instanceof WebAssembly.Tag || mt(false, `expected __cpp_exception export from dotnet.wasm to be WebAssembly.Tag but was ${e2}`)), e2;
  }
  getWasmImports() {
    const e2 = ct.getMemory();
    e2 instanceof WebAssembly.Memory || mt(false, `expected heap import to be WebAssembly.Memory but was ${e2}`);
    const t2 = this.getExceptionTag(), n2 = { c: this.getConstants(), m: { h: e2 } };
    t2 && (n2.x = { e: t2 });
    const r2 = this.getImportsToEmit();
    for (let e3 = 0; e3 < r2.length; e3++) {
      const t3 = r2[e3];
      if ("function" != typeof t3.func) throw new Error(`Import '${t3.name}' not found or not a function`);
      const o2 = this.getCompressedName(t3);
      let s2 = n2[t3.module];
      s2 || (s2 = n2[t3.module] = {}), s2[o2] = t3.func;
    }
    return n2;
  }
  get bytesGeneratedSoFar() {
    const e2 = this.compressImportNames ? 8 : 20;
    return this.stack[0].size + 32 + this.importedFunctionCount * e2 + 2 * this.functions.length + this.estimatedExportBytes;
  }
  get current() {
    return this.stack[this.stackSize - 1];
  }
  get size() {
    return this.current.size;
  }
  appendU8(e2) {
    if (e2 != e2 >>> 0 || e2 > 255) throw new Error(`Byte out of range: ${e2}`);
    return this.current.appendU8(e2);
  }
  appendSimd(e2, t2) {
    return this.current.appendU8(253), 0 | e2 || 0 === e2 && true === t2 || mt(false, "Expected non-v128_load simd opcode or allowLoad==true"), this.current.appendULeb(e2);
  }
  appendAtomic(e2, t2) {
    return this.current.appendU8(254), 0 | e2 || 0 === e2 && true === t2 || mt(false, "Expected non-notify atomic opcode or allowNotify==true"), this.current.appendU8(e2);
  }
  appendU32(e2) {
    return this.current.appendU32(e2);
  }
  appendF32(e2) {
    return this.current.appendF32(e2);
  }
  appendF64(e2) {
    return this.current.appendF64(e2);
  }
  appendBoundaryValue(e2, t2) {
    return this.current.appendBoundaryValue(e2, t2);
  }
  appendULeb(e2) {
    return this.current.appendULeb(e2);
  }
  appendLeb(e2) {
    return this.current.appendLeb(e2);
  }
  appendLebRef(e2, t2) {
    return this.current.appendLebRef(e2, t2);
  }
  appendBytes(e2) {
    return this.current.appendBytes(e2);
  }
  appendName(e2) {
    return this.current.appendName(e2);
  }
  ret(e2) {
    this.ip_const(e2), this.appendU8(15);
  }
  i32_const(e2) {
    this.appendU8(65), this.appendLeb(e2);
  }
  ptr_const(e2) {
    let t2 = this.options.useConstants ? this.constantSlots.indexOf(e2) : -1;
    this.options.useConstants && t2 < 0 && this.nextConstantSlot < this.constantSlots.length && (t2 = this.nextConstantSlot++, this.constantSlots[t2] = e2), t2 >= 0 ? (this.appendU8(35), this.appendLeb(t2)) : this.i32_const(e2);
  }
  ip_const(e2) {
    this.appendU8(65), this.appendLeb(e2 - this.base);
  }
  i52_const(e2) {
    this.appendU8(66), this.appendLeb(e2);
  }
  v128_const(e2) {
    if (0 === e2) this.local("v128_zero");
    else {
      if ("object" != typeof e2) throw new Error("Expected v128_const arg to be 0 or a Uint8Array");
      {
        16 !== e2.byteLength && mt(false, "Expected v128_const arg to be 16 bytes in size");
        let t2 = true;
        for (let n2 = 0; n2 < 16; n2++) 0 !== e2[n2] && (t2 = false);
        t2 ? this.local("v128_zero") : (this.appendSimd(12), this.appendBytes(e2));
      }
    }
  }
  defineType(e2, t2, n2, r2) {
    if (this.functionTypes[e2]) throw new Error(`Function type ${e2} already defined`);
    if (r2 && this.functionTypeCount > this.permanentFunctionTypeCount) throw new Error("New permanent function types cannot be defined after non-permanent ones");
    let o2 = "";
    for (const e3 in t2) o2 += t2[e3] + ",";
    o2 += n2;
    let s2 = this.functionTypesByShape[o2];
    "number" != typeof s2 && (s2 = this.functionTypeCount++, r2 ? (this.permanentFunctionTypeCount++, this.permanentFunctionTypesByShape[o2] = s2, this.permanentFunctionTypesByIndex[s2] = [t2, Object.values(t2).length, n2]) : (this.functionTypesByShape[o2] = s2, this.functionTypesByIndex[s2] = [t2, Object.values(t2).length, n2]));
    const a2 = [s2, t2, n2, `(${JSON.stringify(t2)}) -> ${n2}`, r2];
    return r2 ? this.permanentFunctionTypes[e2] = a2 : this.functionTypes[e2] = a2, s2;
  }
  generateTypeSection() {
    this.beginSection(1), this.appendULeb(this.functionTypeCount);
    for (let e2 = 0; e2 < this.functionTypeCount; e2++) {
      const t2 = this.functionTypesByIndex[e2][0], n2 = this.functionTypesByIndex[e2][1], r2 = this.functionTypesByIndex[e2][2];
      this.appendU8(96), this.appendULeb(n2);
      for (const e3 in t2) this.appendU8(t2[e3]);
      64 !== r2 ? (this.appendULeb(1), this.appendU8(r2)) : this.appendULeb(0);
    }
    this.endSection();
  }
  getImportedFunctionTable() {
    const e2 = {};
    for (const t2 in this.importedFunctions) {
      const n2 = this.importedFunctions[t2];
      e2[this.getCompressedName(n2)] = n2.func;
    }
    return e2;
  }
  getCompressedName(e2) {
    if (!this.compressImportNames || "number" != typeof e2.index) return e2.name;
    let t2 = Os[e2.index];
    return "string" != typeof t2 && (Os[e2.index] = t2 = e2.index.toString(36)), t2;
  }
  getImportsToEmit() {
    const e2 = [];
    for (const t2 in this.importedFunctions) {
      const n2 = this.importedFunctions[t2];
      "number" == typeof n2.index && e2.push(n2);
    }
    return e2.sort(((e3, t2) => e3.index - t2.index)), e2;
  }
  _generateImportSection(e2) {
    const t2 = this.getImportsToEmit();
    if (this.lockImports = true, false !== e2) throw new Error("function table imports are disabled");
    const n2 = void 0 !== this.getExceptionTag();
    this.beginSection(2), this.appendULeb(1 + (n2 ? 1 : 0) + t2.length + this.constantSlots.length + (false !== e2 ? 1 : 0));
    for (let e3 = 0; e3 < t2.length; e3++) {
      const n3 = t2[e3];
      this.appendName(n3.module), this.appendName(this.getCompressedName(n3)), this.appendU8(0), this.appendU8(n3.typeIndex);
    }
    for (let e3 = 0; e3 < this.constantSlots.length; e3++) this.appendName("c"), this.appendName(e3.toString(36)), this.appendU8(3), this.appendU8(127), this.appendU8(0);
    this.appendName("m"), this.appendName("h"), this.appendU8(2), this.appendU8(0), this.appendULeb(1), n2 && (this.appendName("x"), this.appendName("e"), this.appendU8(4), this.appendU8(0), this.appendULeb(this.getTypeIndex("__cpp_exception"))), false !== e2 && (this.appendName("f"), this.appendName("f"), this.appendU8(1), this.appendU8(112), this.appendU8(0), this.appendULeb(1));
  }
  defineImportedFunction(e2, t2, n2, r2, o2) {
    if (this.lockImports) throw new Error("Import section already generated");
    if (r2 && this.importedFunctionCount > 0) throw new Error("New permanent imports cannot be defined after any indexes have been assigned");
    const s2 = this.functionTypes[n2];
    if (!s2) throw new Error("No function type named " + n2);
    if (r2 && !s2[4]) throw new Error("A permanent import must have a permanent function type");
    const a2 = s2[0], i2 = r2 ? this.permanentImportedFunctions : this.importedFunctions;
    if ("number" == typeof o2 && (o2 = qs().get(o2)), "function" != typeof o2 && void 0 !== o2) throw new Error(`Value passed for imported function ${t2} was not a function or valid function pointer or undefined`);
    return i2[t2] = { index: void 0, typeIndex: a2, module: e2, name: t2, func: o2 };
  }
  markImportAsUsed(e2) {
    const t2 = this.importedFunctions[e2];
    if (!t2) throw new Error("No imported function named " + e2);
    "number" != typeof t2.index && (t2.index = this.importedFunctionCount++);
  }
  getTypeIndex(e2) {
    const t2 = this.functionTypes[e2];
    if (!t2) throw new Error("No type named " + e2);
    return t2[0];
  }
  defineFunction(e2, t2) {
    const n2 = { index: this.functions.length, name: e2.name, typeName: e2.type, typeIndex: this.getTypeIndex(e2.type), export: e2.export, locals: e2.locals, generator: t2, error: null, blob: null };
    return this.functions.push(n2), n2.export && (this.estimatedExportBytes += n2.name.length + 8), n2;
  }
  emitImportsAndFunctions(e2) {
    let t2 = 0;
    for (let e3 = 0; e3 < this.functions.length; e3++) {
      const n2 = this.functions[e3];
      n2.export && t2++, this.beginFunction(n2.typeName, n2.locals);
      try {
        n2.blob = n2.generator();
      } finally {
        try {
          n2.blob || (n2.blob = this.endFunction(false));
        } catch (e4) {
        }
      }
    }
    this._generateImportSection(e2), this.beginSection(3), this.appendULeb(this.functions.length);
    for (let e3 = 0; e3 < this.functions.length; e3++) this.appendULeb(this.functions[e3].typeIndex);
    this.beginSection(7), this.appendULeb(t2);
    for (let e3 = 0; e3 < this.functions.length; e3++) {
      const t3 = this.functions[e3];
      t3.export && (this.appendName(t3.name), this.appendU8(0), this.appendULeb(this.importedFunctionCount + e3));
    }
    this.beginSection(10), this.appendULeb(this.functions.length);
    for (let e3 = 0; e3 < this.functions.length; e3++) {
      const t3 = this.functions[e3];
      t3.blob || mt(false, `expected function ${t3.name} to have a body`), this.appendULeb(t3.blob.length), this.appendBytes(t3.blob);
    }
    this.endSection();
  }
  call_indirect() {
    throw new Error("call_indirect unavailable");
  }
  callImport(e2) {
    const t2 = this.importedFunctions[e2];
    if (!t2) throw new Error("No imported function named " + e2);
    if ("number" != typeof t2.index) {
      if (this.lockImports) throw new Error("Import section was emitted before assigning an index to import named " + e2);
      t2.index = this.importedFunctionCount++;
    }
    this.appendU8(16), this.appendULeb(t2.index);
  }
  beginSection(e2) {
    this.inSection && this._pop(true), this.appendU8(e2), this._push(), this.inSection = true;
  }
  endSection() {
    if (!this.inSection) throw new Error("Not in section");
    this.inFunction && this.endFunction(true), this._pop(true), this.inSection = false;
  }
  _assignLocalIndices(e2, t2, n2, r2) {
    e2[127] = 0, e2[126] = 0, e2[125] = 0, e2[124] = 0, e2[123] = 0;
    for (const n3 in t2) {
      const o3 = t2[n3];
      e2[o3] <= 0 && r2++, e2[o3]++;
    }
    const o2 = e2[127], s2 = o2 + e2[126], a2 = s2 + e2[125], i2 = a2 + e2[124];
    e2[127] = 0, e2[126] = 0, e2[125] = 0, e2[124] = 0, e2[123] = 0;
    for (const r3 in t2) {
      const c2 = t2[r3];
      let l2, p2 = 0;
      switch (c2) {
        case 127:
          l2 = 0;
          break;
        case 126:
          l2 = o2;
          break;
        case 125:
          l2 = s2;
          break;
        case 124:
          l2 = a2;
          break;
        case 123:
          l2 = i2;
          break;
        default:
          throw new Error(`Unimplemented valtype: ${c2}`);
      }
      p2 = e2[c2]++ + l2 + n2, this.locals.set(r3, p2);
    }
    return r2;
  }
  beginFunction(e2, t2) {
    if (this.inFunction) throw new Error("Already in function");
    this._push();
    const n2 = this.functionTypes[e2];
    this.locals.clear(), this.branchTargets.clear();
    let r2 = {};
    const o2 = [127, 126, 125, 124, 123];
    let s2 = 0;
    const a2 = this._assignParameterIndices(n2[1]);
    t2 ? s2 = this._assignLocalIndices(r2, t2, a2, s2) : r2 = {}, this.appendULeb(s2);
    for (let e3 = 0; e3 < o2.length; e3++) {
      const t3 = o2[e3], n3 = r2[t3];
      n3 && (this.appendULeb(n3), this.appendU8(t3));
    }
    this.inFunction = true;
  }
  endFunction(e2) {
    if (!this.inFunction) throw new Error("Not in function");
    if (this.activeBlocks > 0) throw new Error(`${this.activeBlocks} unclosed block(s) at end of function`);
    const t2 = this._pop(e2);
    return this.inFunction = false, t2;
  }
  block(e2, t2) {
    const n2 = this.appendU8(t2 || 2);
    return e2 ? this.appendU8(e2) : this.appendU8(64), this.activeBlocks++, n2;
  }
  endBlock() {
    if (this.activeBlocks <= 0) throw new Error("No blocks active");
    this.activeBlocks--, this.appendU8(11);
  }
  arg(e2, t2) {
    const n2 = "string" == typeof e2 ? this.locals.has(e2) ? this.locals.get(e2) : void 0 : e2;
    if ("number" != typeof n2) throw new Error("No local named " + e2);
    t2 && this.appendU8(t2), this.appendULeb(n2);
  }
  local(e2, t2) {
    const n2 = "string" == typeof e2 ? this.locals.has(e2) ? this.locals.get(e2) : void 0 : e2 + this.argumentCount;
    if ("number" != typeof n2) throw new Error("No local named " + e2);
    t2 ? this.appendU8(t2) : this.appendU8(32), this.appendULeb(n2);
  }
  appendMemarg(e2, t2) {
    this.appendULeb(t2), this.appendULeb(e2);
  }
  lea(e2, t2) {
    "string" == typeof e2 ? this.local(e2) : this.i32_const(e2), this.i32_const(t2), this.appendU8(106);
  }
  getArrayView(e2, t2) {
    if (true !== t2 && this.stackSize > 1) throw new Error("Jiterpreter block stack not empty");
    return this.stack[0].getArrayView(e2);
  }
  getConstants() {
    const e2 = {};
    for (let t2 = 0; t2 < this.constantSlots.length; t2++) e2[t2.toString(36)] = this.constantSlots[t2];
    return e2;
  }
}
class Ds {
  constructor() {
    this.textBuf = new Uint8Array(1024), this.capacity = 24576, this.buffer = _(this.capacity), this.buffer || mt(false, "Failed to allocate 24576b buffer for BlobBuilder"), K().fill(0, this.buffer, this.buffer + this.capacity), this.size = 0, this.clear(), "function" == typeof TextEncoder && (this.encoder = new TextEncoder());
  }
  clear() {
    this.size = 0;
  }
  appendU8(e2) {
    if (this.size >= this.capacity) throw new Error("Buffer full");
    const t2 = this.size;
    return K()[this.buffer + this.size++] = e2, t2;
  }
  appendU32(e2) {
    const t2 = this.size;
    return o.mono_jiterp_write_number_unaligned(this.buffer + this.size, e2, 0), this.size += 4, t2;
  }
  appendI32(e2) {
    const t2 = this.size;
    return o.mono_jiterp_write_number_unaligned(this.buffer + this.size, e2, 1), this.size += 4, t2;
  }
  appendF32(e2) {
    const t2 = this.size;
    return o.mono_jiterp_write_number_unaligned(this.buffer + this.size, e2, 2), this.size += 4, t2;
  }
  appendF64(e2) {
    const t2 = this.size;
    return o.mono_jiterp_write_number_unaligned(this.buffer + this.size, e2, 3), this.size += 8, t2;
  }
  appendBoundaryValue(e2, t2) {
    if (this.size + 8 >= this.capacity) throw new Error("Buffer full");
    const n2 = o.mono_jiterp_encode_leb_signed_boundary(this.buffer + this.size, e2, t2);
    if (n2 < 1) throw new Error(`Failed to encode ${e2} bit boundary value with sign ${t2}`);
    return this.size += n2, n2;
  }
  appendULeb(e2) {
    if ("number" != typeof e2 && mt(false, `appendULeb expected number but got ${e2}`), e2 >= 0 || mt(false, "cannot pass negative value to appendULeb"), e2 < 127) {
      if (this.size + 1 >= this.capacity) throw new Error("Buffer full");
      return this.appendU8(e2), 1;
    }
    if (this.size + 8 >= this.capacity) throw new Error("Buffer full");
    const t2 = o.mono_jiterp_encode_leb52(this.buffer + this.size, e2, 0);
    if (t2 < 1) throw new Error(`Failed to encode value '${e2}' as unsigned leb`);
    return this.size += t2, t2;
  }
  appendLeb(e2) {
    if ("number" != typeof e2 && mt(false, `appendLeb expected number but got ${e2}`), this.size + 8 >= this.capacity) throw new Error("Buffer full");
    const t2 = o.mono_jiterp_encode_leb52(this.buffer + this.size, e2, 1);
    if (t2 < 1) throw new Error(`Failed to encode value '${e2}' as signed leb`);
    return this.size += t2, t2;
  }
  appendLebRef(e2, t2) {
    if (this.size + 8 >= this.capacity) throw new Error("Buffer full");
    const n2 = o.mono_jiterp_encode_leb64_ref(this.buffer + this.size, e2, t2 ? 1 : 0);
    if (n2 < 1) throw new Error("Failed to encode value as leb");
    return this.size += n2, n2;
  }
  copyTo(e2, t2) {
    if ("number" != typeof t2 && (t2 = this.size), e2.size + t2 >= e2.capacity) throw new Error("Destination buffer full");
    K().copyWithin(e2.buffer + e2.size, this.buffer, this.buffer + t2), e2.size += t2;
  }
  appendBytes(e2, t2) {
    const n2 = this.size, r2 = K(), o2 = "number" != typeof t2 ? e2.length : t2;
    if (this.size + o2 >= this.capacity) throw new Error("Buffer full");
    return e2.buffer === r2.buffer ? (r2.copyWithin(this.buffer + n2, e2.byteOffset, e2.byteOffset + o2), this.size += o2) : ("number" == typeof t2 && (e2 = new Uint8Array(e2.buffer, e2.byteOffset, t2)), this.getArrayView(true).set(e2, this.size), this.size += e2.length), n2;
  }
  appendName(e2) {
    let t2 = e2.length, n2 = 1 === e2.length ? e2.charCodeAt(0) : -1;
    if (n2 > 127 && (n2 = -1), t2 && n2 < 0) if (this.encoder) t2 = this.encoder.encodeInto(e2, this.textBuf).written || 0;
    else for (let n3 = 0; n3 < t2; n3++) {
      const t3 = e2.charCodeAt(n3);
      if (t3 > 127) throw new Error("Out of range character and no TextEncoder available");
      this.textBuf[n3] = t3;
    }
    this.appendULeb(t2), n2 >= 0 ? this.appendU8(n2) : t2 > 1 && this.appendBytes(this.textBuf, t2);
  }
  getArrayView(e2) {
    return new Uint8Array(K().buffer, this.buffer, e2 ? this.capacity : this.size);
  }
}
class Fs {
  constructor(e2) {
    this.segments = [], this.backBranchTargets = null, this.lastSegmentEnd = 0, this.overheadBytes = 0, this.blockStack = [], this.backDispatchOffsets = [], this.dispatchTable = /* @__PURE__ */ new Map(), this.observedBackBranchTargets = /* @__PURE__ */ new Set(), this.trace = 0, this.builder = e2;
  }
  initialize(e2, t2, n2) {
    this.segments.length = 0, this.blockStack.length = 0, this.startOfBody = e2, this.backBranchTargets = t2, this.base = this.builder.base, this.ip = this.lastSegmentStartIp = this.firstOpcodeIp = this.builder.base, this.lastSegmentEnd = 0, this.overheadBytes = 10, this.dispatchTable.clear(), this.observedBackBranchTargets.clear(), this.trace = n2, this.backDispatchOffsets.length = 0;
  }
  entry(e2) {
    this.entryIp = e2;
    const t2 = o.mono_jiterp_get_opcode_info(676, 1);
    return this.firstOpcodeIp = e2 + 2 * t2, this.appendBlob(), 1 !== this.segments.length && mt(false, "expected 1 segment"), "blob" !== this.segments[0].type && mt(false, "expected blob"), this.entryBlob = this.segments[0], this.segments.length = 0, this.overheadBytes += 9, this.backBranchTargets && (this.overheadBytes += 20, this.overheadBytes += this.backBranchTargets.length), this.firstOpcodeIp;
  }
  appendBlob() {
    this.builder.current.size !== this.lastSegmentEnd && (this.segments.push({ type: "blob", ip: this.lastSegmentStartIp, start: this.lastSegmentEnd, length: this.builder.current.size - this.lastSegmentEnd }), this.lastSegmentStartIp = this.ip, this.lastSegmentEnd = this.builder.current.size, this.overheadBytes += 2);
  }
  startBranchBlock(e2, t2) {
    this.appendBlob(), this.segments.push({ type: "branch-block-header", ip: e2, isBackBranchTarget: t2 }), this.overheadBytes += 1;
  }
  branch(e2, t2, n2) {
    t2 && this.observedBackBranchTargets.add(e2), this.appendBlob(), this.segments.push({ type: "branch", from: this.ip, target: e2, isBackward: t2, branchType: n2 }), this.overheadBytes += 4, t2 && (this.overheadBytes += 4);
  }
  jumpTable(e2, t2) {
    this.appendBlob(), this.segments.push({ type: "jump-table", from: this.ip, targets: e2, fallthrough: t2 }), this.overheadBytes += 4, this.overheadBytes += e2.length, this.overheadBytes += 24;
  }
  emitBlob(e2, t2) {
    const n2 = t2.subarray(e2.start, e2.start + e2.length);
    this.builder.appendBytes(n2);
  }
  generate() {
    this.appendBlob();
    const e2 = this.builder.endFunction(false);
    this.builder._push(), this.builder.base = this.base, this.emitBlob(this.entryBlob, e2), this.backBranchTargets && this.builder.block(64, 3);
    for (let e3 = 0; e3 < this.segments.length; e3++) {
      const t2 = this.segments[e3];
      "branch-block-header" === t2.type && this.blockStack.push(t2.ip);
    }
    this.blockStack.sort(((e3, t2) => e3 - t2));
    for (let e3 = 0; e3 < this.blockStack.length; e3++) this.builder.block(64);
    if (this.backBranchTargets) {
      this.backDispatchOffsets.length = 0;
      for (let e3 = 0; e3 < this.backBranchTargets.length; e3++) {
        const t2 = 2 * this.backBranchTargets[e3] + this.startOfBody;
        this.blockStack.indexOf(t2) < 0 || this.observedBackBranchTargets.has(t2) && (this.dispatchTable.set(t2, this.backDispatchOffsets.length + 1), this.backDispatchOffsets.push(t2));
      }
      if (0 === this.backDispatchOffsets.length) this.trace > 0 && Ve("No back branch targets were reachable after filtering");
      else if (1 === this.backDispatchOffsets.length) this.trace > 0 && (this.backDispatchOffsets[0] === this.entryIp ? Ve(`Exactly one back dispatch offset and it was the entry point 0x${this.entryIp.toString(16)}`) : Ve(`Exactly one back dispatch offset and it was 0x${this.backDispatchOffsets[0].toString(16)}`)), this.builder.local("disp"), this.builder.appendU8(13), this.builder.appendULeb(this.blockStack.indexOf(this.backDispatchOffsets[0]));
      else {
        this.trace > 0 && Ve(`${this.backDispatchOffsets.length} back branch offsets after filtering.`), this.builder.block(64), this.builder.block(64), this.builder.local("disp"), this.builder.appendU8(14), this.builder.appendULeb(this.backDispatchOffsets.length + 1), this.builder.appendULeb(1);
        for (let e3 = 0; e3 < this.backDispatchOffsets.length; e3++) this.builder.appendULeb(this.blockStack.indexOf(this.backDispatchOffsets[e3]) + 2);
        this.builder.appendULeb(0), this.builder.endBlock(), this.builder.appendU8(0), this.builder.endBlock();
      }
      this.backDispatchOffsets.length > 0 && this.blockStack.push(0);
    }
    this.trace > 1 && Ve(`blockStack=${this.blockStack}`);
    for (let t2 = 0; t2 < this.segments.length; t2++) {
      const n2 = this.segments[t2];
      switch (n2.type) {
        case "blob":
          this.emitBlob(n2, e2);
          break;
        case "branch-block-header": {
          const e3 = this.blockStack.indexOf(n2.ip);
          0 !== e3 && mt(false, `expected ${n2.ip} on top of blockStack but found it at index ${e3}, top is ${this.blockStack[0]}`), this.builder.endBlock(), this.blockStack.shift();
          break;
        }
        case "jump-table": {
          const e3 = 1;
          this.builder.appendU8(14), this.builder.appendULeb(n2.targets.length);
          for (const t4 of n2.targets) {
            const n3 = this.blockStack.indexOf(t4);
            n3 >= 0 ? (da(13, 1), this.builder.appendULeb(n3 + e3)) : (da(14, 1), this.trace > 0 && Ve(`Switch target ${t4} not found in block stack ${this.blockStack}`), this.builder.appendULeb(0));
          }
          const t3 = this.blockStack.indexOf(n2.fallthrough);
          t3 >= 0 ? (da(13, 1), this.builder.appendULeb(t3 + e3)) : (da(14, 1), this.trace > 0 && Ve(`Switch fallthrough ${n2.fallthrough} not found in block stack ${this.blockStack}`), this.builder.appendULeb(0)), this.builder.appendU8(0);
          break;
        }
        case "branch": {
          const e3 = n2.isBackward ? 0 : n2.target;
          let t3, r2 = this.blockStack.indexOf(e3), o2 = false;
          if (n2.isBackward && (this.dispatchTable.has(n2.target) ? (t3 = this.dispatchTable.get(n2.target), this.trace > 1 && Ve(`backward br from ${n2.from.toString(16)} to ${n2.target.toString(16)}: disp=${t3}`), o2 = true) : (this.trace > 0 && Ve(`br from ${n2.from.toString(16)} to ${n2.target.toString(16)} failed: back branch target not in dispatch table`), r2 = -1)), r2 >= 0 || o2) {
            let e4 = 0;
            switch (n2.branchType) {
              case 2:
                this.builder, n2.from, void 0 !== t3 && (this.builder.i32_const(t3), this.builder.local("disp", 33)), this.builder.appendU8(12);
                break;
              case 3:
                this.builder.block(64, 4), this.builder, n2.from, void 0 !== t3 && (this.builder.i32_const(t3), this.builder.local("disp", 33)), this.builder.appendU8(12), e4 = 1;
                break;
              case 0:
                void 0 !== t3 && (this.builder.i32_const(t3), this.builder.local("disp", 33)), this.builder.appendU8(12);
                break;
              case 1:
                void 0 !== t3 ? (this.builder.block(64, 4), this.builder.i32_const(t3), this.builder.local("disp", 33), e4 = 1, this.builder.appendU8(12)) : this.builder.appendU8(13);
                break;
              default:
                throw new Error("Unimplemented branch type");
            }
            this.builder.appendULeb(e4 + r2), e4 && this.builder.endBlock(), this.trace > 1 && Ve(`br from ${n2.from.toString(16)} to ${n2.target.toString(16)} breaking out ${e4 + r2 + 1} level(s)`);
          } else {
            if (this.trace > 0) {
              const e5 = this.base;
              n2.target >= e5 && n2.target < this.exitIp ? Ve(`br from ${n2.from.toString(16)} to ${n2.target.toString(16)} failed (inside of trace!)`) : this.trace > 1 && Ve(`br from ${n2.from.toString(16)} to ${n2.target.toString(16)} failed (outside of trace 0x${e5.toString(16)} - 0x${this.exitIp.toString(16)})`);
            }
            const e4 = 1 === n2.branchType || 3 === n2.branchType;
            e4 && this.builder.block(64, 4), Ws(this.builder, n2.target, 4), e4 && this.builder.endBlock();
          }
          break;
        }
        default:
          throw new Error("unreachable");
      }
    }
    return this.backBranchTargets && (this.blockStack.length <= 1 || mt(false, "expected one or zero entries in the block stack at the end"), this.blockStack.length && this.blockStack.shift(), this.builder.endBlock()), 0 !== this.blockStack.length && mt(false, `expected block stack to be empty at end of function but it was ${this.blockStack}`), this.builder.ip_const(this.exitIp), this.builder.appendU8(15), this.builder.appendU8(11), this.builder._pop(false);
  }
}
let Ps;
const Ms = {}, zs = globalThis.performance && globalThis.performance.now ? globalThis.performance.now.bind(globalThis.performance) : Date.now;
function Vs(e2, t2, n2) {
  let r2;
  switch (n2) {
    case 633:
      r2 = "prof_enter";
      break;
    case 634:
      r2 = "prof_samplepoint";
      break;
    case 635:
    case 636:
      r2 = "prof_leave";
      break;
    default:
      throw new Error(`Unimplemented profiler event ${n2}`);
  }
  e2.local("frame"), e2.i32_const(t2), e2.callImport(r2);
}
function Ws(e2, t2, n2) {
  e2.ip_const(t2), e2.options.countBailouts && (e2.i32_const(e2.traceIndex), e2.i32_const(n2), e2.callImport("bailout")), e2.appendU8(15);
}
function Hs(e2, t2, n2, r2) {
  e2.local("cinfo"), e2.block(64, 4), e2.local("cinfo"), e2.local("disp"), e2.appendU8(54), e2.appendMemarg(ea(19), 0), n2 <= e2.options.monitoringLongDistance + 2 && (e2.local("cinfo"), e2.i32_const(n2), e2.appendU8(54), e2.appendMemarg(ea(20), 0)), e2.endBlock(), e2.ip_const(t2), e2.options.countBailouts && (e2.i32_const(e2.traceIndex), e2.i32_const(r2), e2.callImport("bailout")), e2.appendU8(15);
}
function qs() {
  if (Ps || (Ps = ct.getWasmIndirectFunctionTable()), !Ps) throw new Error("Module did not export the indirect function table");
  return Ps;
}
function Gs(e2, t2) {
  t2 || mt(false, "Attempting to set null function into table");
  const n2 = o.mono_jiterp_allocate_table_entry(e2);
  return n2 > 0 && qs().set(n2, t2), n2;
}
function Js(e2, t2, n2, r2, o2) {
  if (r2 <= 0) return o2 && e2.appendU8(26), true;
  if (r2 >= Rs) return false;
  const s2 = o2 ? "memop_dest" : "pLocals";
  o2 && e2.local(s2, 33);
  let a2 = o2 ? 0 : t2;
  if (e2.options.enableSimd) {
    const t3 = 16;
    for (; r2 >= t3; ) e2.local(s2), e2.v128_const(0), e2.appendSimd(11), e2.appendMemarg(a2, 0), a2 += t3, r2 -= t3;
  }
  for (; r2 >= 8; ) e2.local(s2), e2.i52_const(0), e2.appendU8(55), e2.appendMemarg(a2, 0), a2 += 8, r2 -= 8;
  for (; r2 >= 1; ) {
    e2.local(s2), e2.i32_const(0);
    let t3 = r2 % 4;
    switch (t3) {
      case 0:
        t3 = 4, e2.appendU8(54);
        break;
      case 1:
        e2.appendU8(58);
        break;
      case 3:
      case 2:
        t3 = 2, e2.appendU8(59);
    }
    e2.appendMemarg(a2, 0), a2 += t3, r2 -= t3;
  }
  return true;
}
function Xs(e2, t2, n2) {
  Js(e2, 0, 0, n2, true) || (e2.i32_const(t2), e2.i32_const(n2), e2.appendU8(252), e2.appendU8(11), e2.appendU8(0));
}
function Qs(e2, t2, n2, r2, o2, s2, a2) {
  if (r2 <= 0) return o2 && (e2.appendU8(26), e2.appendU8(26)), true;
  if (r2 >= Bs) return false;
  o2 ? (s2 = s2 || "memop_dest", a2 = a2 || "memop_src", e2.local(a2, 33), e2.local(s2, 33)) : s2 && a2 || (s2 = a2 = "pLocals");
  let i2 = o2 ? 0 : t2, c2 = o2 ? 0 : n2;
  if (e2.options.enableSimd) {
    const t3 = 16;
    for (; r2 >= t3; ) e2.local(s2), e2.local(a2), e2.appendSimd(0, true), e2.appendMemarg(c2, 0), e2.appendSimd(11), e2.appendMemarg(i2, 0), i2 += t3, c2 += t3, r2 -= t3;
  }
  for (; r2 >= 8; ) e2.local(s2), e2.local(a2), e2.appendU8(41), e2.appendMemarg(c2, 0), e2.appendU8(55), e2.appendMemarg(i2, 0), i2 += 8, c2 += 8, r2 -= 8;
  for (; r2 >= 1; ) {
    let t3, n3, o3 = r2 % 4;
    switch (o3) {
      case 0:
        o3 = 4, t3 = 40, n3 = 54;
        break;
      default:
      case 1:
        o3 = 1, t3 = 44, n3 = 58;
        break;
      case 3:
      case 2:
        o3 = 2, t3 = 46, n3 = 59;
    }
    e2.local(s2), e2.local(a2), e2.appendU8(t3), e2.appendMemarg(c2, 0), e2.appendU8(n3), e2.appendMemarg(i2, 0), c2 += o3, i2 += o3, r2 -= o3;
  }
  return true;
}
function Ys(e2, t2) {
  return Qs(e2, 0, 0, t2, true) || (e2.i32_const(t2), e2.appendU8(252), e2.appendU8(10), e2.appendU8(0), e2.appendU8(0)), true;
}
function Zs() {
  const e2 = da(5, 1);
  e2 >= Ls && (Ve(`Disabling jiterpreter after ${e2} failures`), pa({ enableTraces: false, enableInterpEntry: false, enableJitCall: false }));
}
const Ks = {};
function ea(e2) {
  const t2 = Ks[e2];
  return void 0 === t2 ? Ks[e2] = o.mono_jiterp_get_member_offset(e2) : t2;
}
function ta(e2) {
  const t2 = Ke.wasmExports[e2];
  if ("function" != typeof t2) throw new Error(`raw cwrap ${e2} not found`);
  return t2;
}
const na = {};
function ra(e2) {
  let t2 = na[e2];
  return "number" != typeof t2 && (t2 = na[e2] = o.mono_jiterp_get_opcode_value_table_entry(e2)), t2;
}
function oa(e2, t2) {
  return [e2, e2, t2];
}
let sa;
function aa() {
  if (!o.mono_wasm_is_zero_page_reserved()) return false;
  if (true === sa) return false;
  const e2 = te();
  for (let t2 = 0; t2 < 8; t2++) if (0 !== e2[t2]) return false === sa && He(`Zero page optimizations are enabled but garbage appeared in memory at address ${4 * t2}: ${e2[t2]}`), sa = true, false;
  return sa = false, true;
}
const ia = { enableTraces: "jiterpreter-traces-enabled", enableInterpEntry: "jiterpreter-interp-entry-enabled", enableJitCall: "jiterpreter-jit-call-enabled", enableBackwardBranches: "jiterpreter-backward-branch-entries-enabled", enableCallResume: "jiterpreter-call-resume-enabled", enableWasmEh: "jiterpreter-wasm-eh-enabled", enableSimd: "jiterpreter-simd-enabled", enableAtomics: "jiterpreter-atomics-enabled", zeroPageOptimization: "jiterpreter-zero-page-optimization", cprop: "jiterpreter-constant-propagation", enableStats: "jiterpreter-stats-enabled", disableHeuristic: "jiterpreter-disable-heuristic", estimateHeat: "jiterpreter-estimate-heat", countBailouts: "jiterpreter-count-bailouts", dumpTraces: "jiterpreter-dump-traces", useConstants: "jiterpreter-use-constants", eliminateNullChecks: "jiterpreter-eliminate-null-checks", noExitBackwardBranches: "jiterpreter-backward-branches-enabled", directJitCalls: "jiterpreter-direct-jit-calls", minimumTraceValue: "jiterpreter-minimum-trace-value", minimumTraceHitCount: "jiterpreter-minimum-trace-hit-count", monitoringPeriod: "jiterpreter-trace-monitoring-period", monitoringShortDistance: "jiterpreter-trace-monitoring-short-distance", monitoringLongDistance: "jiterpreter-trace-monitoring-long-distance", monitoringMaxAveragePenalty: "jiterpreter-trace-monitoring-max-average-penalty", backBranchBoost: "jiterpreter-back-branch-boost", jitCallHitCount: "jiterpreter-jit-call-hit-count", jitCallFlushThreshold: "jiterpreter-jit-call-queue-flush-threshold", interpEntryHitCount: "jiterpreter-interp-entry-hit-count", interpEntryFlushThreshold: "jiterpreter-interp-entry-queue-flush-threshold", wasmBytesLimit: "jiterpreter-wasm-bytes-limit", tableSize: "jiterpreter-table-size", aotTableSize: "jiterpreter-aot-table-size", maxModuleSize: "jiterpreter-max-module-size", maxSwitchSize: "jiterpreter-max-switch-size" };
let ca = -1, la = {};
function pa(e2) {
  for (const t2 in e2) {
    const n2 = ia[t2];
    if (!n2) {
      He(`Unrecognized jiterpreter option: ${t2}`);
      continue;
    }
    const r2 = e2[t2];
    "boolean" == typeof r2 ? o.mono_jiterp_parse_option((r2 ? "--" : "--no-") + n2) : "number" == typeof r2 ? o.mono_jiterp_parse_option(`--${n2}=${r2}`) : He(`Jiterpreter option must be a boolean or a number but was ${typeof r2} '${r2}'`);
  }
}
function ua(e2) {
  return o.mono_jiterp_get_counter(e2);
}
function da(e2, t2) {
  return o.mono_jiterp_modify_counter(e2, t2);
}
function fa() {
  const e2 = o.mono_jiterp_get_options_version();
  return e2 !== ca && ((function() {
    la = {};
    for (const e3 in ia) {
      const t2 = o.mono_jiterp_get_option_as_int(ia[e3]);
      t2 !== Ns ? la[e3] = t2 : Ve(`Failed to retrieve value of option ${ia[e3]}`);
    }
  })(), ca = e2), la;
}
function _a(e2, t2, n2, r2) {
  const s2 = qs(), a2 = t2, i2 = a2 + n2 - 1;
  return i2 < s2.length || mt(false, `Last index out of range: ${i2} >= ${s2.length}`), s2.set(a2, r2), o.mono_jiterp_initialize_table(e2, a2, i2), t2 + n2;
}
let ma = false;
const ha = ["Unknown", "InterpreterTiering", "NullCheck", "VtableNotInitialized", "Branch", "BackwardBranch", "ConditionalBranch", "ConditionalBackwardBranch", "ComplexBranch", "ArrayLoadFailed", "ArrayStoreFailed", "StringOperationFailed", "DivideByZero", "Overflow", "Return", "Call", "Throw", "AllocFailed", "SpanOperationFailed", "CastFailed", "SafepointBranchTaken", "UnboxFailed", "CallDelegate", "Debugging", "Icall", "UnexpectedRetIp", "LeaveCheck", "SwitchSize", "SwitchTarget"], ga = { 2: ["V128_I1_NEGATION", "V128_I2_NEGATION", "V128_I4_NEGATION", "V128_ONES_COMPLEMENT", "V128_U2_WIDEN_LOWER", "V128_U2_WIDEN_UPPER", "V128_I1_CREATE_SCALAR", "V128_I2_CREATE_SCALAR", "V128_I4_CREATE_SCALAR", "V128_I8_CREATE_SCALAR", "V128_I1_EXTRACT_MSB", "V128_I2_EXTRACT_MSB", "V128_I4_EXTRACT_MSB", "V128_I8_EXTRACT_MSB", "V128_I1_CREATE", "V128_I2_CREATE", "V128_I4_CREATE", "V128_I8_CREATE", "SplatX1", "SplatX2", "SplatX4", "SplatX8", "NegateD1", "NegateD2", "NegateD4", "NegateD8", "NegateR4", "NegateR8", "SqrtR4", "SqrtR8", "CeilingR4", "CeilingR8", "FloorR4", "FloorR8", "TruncateR4", "TruncateR8", "RoundToNearestR4", "RoundToNearestR8", "NotANY", "AnyTrueANY", "AllTrueD1", "AllTrueD2", "AllTrueD4", "AllTrueD8", "PopCountU1", "BitmaskD1", "BitmaskD2", "BitmaskD4", "BitmaskD8", "AddPairwiseWideningI1", "AddPairwiseWideningU1", "AddPairwiseWideningI2", "AddPairwiseWideningU2", "AbsI1", "AbsI2", "AbsI4", "AbsI8", "AbsR4", "AbsR8", "ConvertToSingleI4", "ConvertToSingleU4", "ConvertToSingleR8", "ConvertToDoubleLowerI4", "ConvertToDoubleLowerU4", "ConvertToDoubleLowerR4", "ConvertToInt32SaturateR4", "ConvertToUInt32SaturateR4", "ConvertToInt32SaturateR8", "ConvertToUInt32SaturateR8", "SignExtendWideningLowerD1", "SignExtendWideningLowerD2", "SignExtendWideningLowerD4", "SignExtendWideningUpperD1", "SignExtendWideningUpperD2", "SignExtendWideningUpperD4", "ZeroExtendWideningLowerD1", "ZeroExtendWideningLowerD2", "ZeroExtendWideningLowerD4", "ZeroExtendWideningUpperD1", "ZeroExtendWideningUpperD2", "ZeroExtendWideningUpperD4", "LoadVector128ANY", "LoadScalarVector128X4", "LoadScalarVector128X8", "LoadScalarAndSplatVector128X1", "LoadScalarAndSplatVector128X2", "LoadScalarAndSplatVector128X4", "LoadScalarAndSplatVector128X8", "LoadWideningVector128I1", "LoadWideningVector128U1", "LoadWideningVector128I2", "LoadWideningVector128U2", "LoadWideningVector128I4", "LoadWideningVector128U4"], 3: ["V128_I1_ADD", "V128_I2_ADD", "V128_I4_ADD", "V128_R4_ADD", "V128_I1_SUB", "V128_I2_SUB", "V128_I4_SUB", "V128_R4_SUB", "V128_BITWISE_AND", "V128_BITWISE_OR", "V128_BITWISE_EQUALITY", "V128_BITWISE_INEQUALITY", "V128_R4_FLOAT_EQUALITY", "V128_R8_FLOAT_EQUALITY", "V128_EXCLUSIVE_OR", "V128_I1_MULTIPLY", "V128_I2_MULTIPLY", "V128_I4_MULTIPLY", "V128_R4_MULTIPLY", "V128_R4_DIVISION", "V128_I1_LEFT_SHIFT", "V128_I2_LEFT_SHIFT", "V128_I4_LEFT_SHIFT", "V128_I8_LEFT_SHIFT", "V128_I1_RIGHT_SHIFT", "V128_I2_RIGHT_SHIFT", "V128_I4_RIGHT_SHIFT", "V128_I1_URIGHT_SHIFT", "V128_I2_URIGHT_SHIFT", "V128_I4_URIGHT_SHIFT", "V128_I8_URIGHT_SHIFT", "V128_U1_NARROW", "V128_U1_GREATER_THAN", "V128_I1_LESS_THAN", "V128_U1_LESS_THAN", "V128_I2_LESS_THAN", "V128_I1_EQUALS", "V128_I2_EQUALS", "V128_I4_EQUALS", "V128_R4_EQUALS", "V128_I8_EQUALS", "V128_I1_EQUALS_ANY", "V128_I2_EQUALS_ANY", "V128_I4_EQUALS_ANY", "V128_I8_EQUALS_ANY", "V128_AND_NOT", "V128_U2_LESS_THAN_EQUAL", "V128_I1_SHUFFLE", "V128_I2_SHUFFLE", "V128_I4_SHUFFLE", "V128_I8_SHUFFLE", "ExtractScalarI1", "ExtractScalarU1", "ExtractScalarI2", "ExtractScalarU2", "ExtractScalarD4", "ExtractScalarD8", "ExtractScalarR4", "ExtractScalarR8", "SwizzleD1", "AddD1", "AddD2", "AddD4", "AddD8", "AddR4", "AddR8", "SubtractD1", "SubtractD2", "SubtractD4", "SubtractD8", "SubtractR4", "SubtractR8", "MultiplyD2", "MultiplyD4", "MultiplyD8", "MultiplyR4", "MultiplyR8", "DivideR4", "DivideR8", "DotI2", "ShiftLeftD1", "ShiftLeftD2", "ShiftLeftD4", "ShiftLeftD8", "ShiftRightArithmeticD1", "ShiftRightArithmeticD2", "ShiftRightArithmeticD4", "ShiftRightArithmeticD8", "ShiftRightLogicalD1", "ShiftRightLogicalD2", "ShiftRightLogicalD4", "ShiftRightLogicalD8", "AndANY", "AndNotANY", "OrANY", "XorANY", "CompareEqualD1", "CompareEqualD2", "CompareEqualD4", "CompareEqualD8", "CompareEqualR4", "CompareEqualR8", "CompareNotEqualD1", "CompareNotEqualD2", "CompareNotEqualD4", "CompareNotEqualD8", "CompareNotEqualR4", "CompareNotEqualR8", "CompareLessThanI1", "CompareLessThanU1", "CompareLessThanI2", "CompareLessThanU2", "CompareLessThanI4", "CompareLessThanU4", "CompareLessThanI8", "CompareLessThanR4", "CompareLessThanR8", "CompareLessThanOrEqualI1", "CompareLessThanOrEqualU1", "CompareLessThanOrEqualI2", "CompareLessThanOrEqualU2", "CompareLessThanOrEqualI4", "CompareLessThanOrEqualU4", "CompareLessThanOrEqualI8", "CompareLessThanOrEqualR4", "CompareLessThanOrEqualR8", "CompareGreaterThanI1", "CompareGreaterThanU1", "CompareGreaterThanI2", "CompareGreaterThanU2", "CompareGreaterThanI4", "CompareGreaterThanU4", "CompareGreaterThanI8", "CompareGreaterThanR4", "CompareGreaterThanR8", "CompareGreaterThanOrEqualI1", "CompareGreaterThanOrEqualU1", "CompareGreaterThanOrEqualI2", "CompareGreaterThanOrEqualU2", "CompareGreaterThanOrEqualI4", "CompareGreaterThanOrEqualU4", "CompareGreaterThanOrEqualI8", "CompareGreaterThanOrEqualR4", "CompareGreaterThanOrEqualR8", "ConvertNarrowingSaturateSignedI2", "ConvertNarrowingSaturateSignedI4", "ConvertNarrowingSaturateUnsignedI2", "ConvertNarrowingSaturateUnsignedI4", "MultiplyWideningLowerI1", "MultiplyWideningLowerI2", "MultiplyWideningLowerI4", "MultiplyWideningLowerU1", "MultiplyWideningLowerU2", "MultiplyWideningLowerU4", "MultiplyWideningUpperI1", "MultiplyWideningUpperI2", "MultiplyWideningUpperI4", "MultiplyWideningUpperU1", "MultiplyWideningUpperU2", "MultiplyWideningUpperU4", "AddSaturateI1", "AddSaturateU1", "AddSaturateI2", "AddSaturateU2", "SubtractSaturateI1", "SubtractSaturateU1", "SubtractSaturateI2", "SubtractSaturateU2", "MultiplyRoundedSaturateQ15I2", "MinI1", "MinI2", "MinI4", "MinU1", "MinU2", "MinU4", "MaxI1", "MaxI2", "MaxI4", "MaxU1", "MaxU2", "MaxU4", "AverageRoundedU1", "AverageRoundedU2", "MinR4", "MinR8", "MaxR4", "MaxR8", "PseudoMinR4", "PseudoMinR8", "PseudoMaxR4", "PseudoMaxR8", "StoreANY"], 4: ["V128_CONDITIONAL_SELECT", "ReplaceScalarD1", "ReplaceScalarD2", "ReplaceScalarD4", "ReplaceScalarD8", "ReplaceScalarR4", "ReplaceScalarR8", "ShuffleD1", "BitwiseSelectANY", "LoadScalarAndInsertX1", "LoadScalarAndInsertX2", "LoadScalarAndInsertX4", "LoadScalarAndInsertX8", "StoreSelectedScalarX1", "StoreSelectedScalarX2", "StoreSelectedScalarX4", "StoreSelectedScalarX8"] }, ba = { 13: [65, 0], 14: [65, 1] }, ya = { 456: 168, 462: 174, 457: 170, 463: 176 }, wa = { 508: [69, 40, 54], 428: [106, 40, 54], 430: [107, 40, 54], 432: [107, 40, 54], 436: [115, 40, 54], 429: [124, 41, 55], 431: [125, 41, 55], 433: [125, 41, 55], 437: [133, 41, 55], 511: [106, 40, 54], 515: [108, 40, 54], 513: [124, 41, 55], 517: [126, 41, 55], 434: [140, 42, 56], 435: [154, 43, 57], 464: [178, 40, 56], 467: [183, 40, 57], 438: [184, 40, 57], 465: [180, 41, 56], 468: [185, 41, 57], 439: [186, 41, 57], 469: [187, 42, 57], 466: [182, 43, 56], 460: [1, 52, 55], 461: [1, 53, 55], 444: [113, 40, 54], 452: [113, 40, 54], 440: [117, 40, 54], 448: [117, 40, 54], 445: [113, 41, 54], 453: [113, 41, 54], 441: [117, 41, 54], 449: [117, 41, 54], 525: [116, 40, 54], 526: [134, 41, 55], 527: [117, 40, 54], 528: [135, 41, 55], 523: [118, 40, 54], 524: [136, 41, 55], 640: [119, 40, 54], 641: [137, 41, 55], 642: [120, 40, 54], 643: [138, 41, 55], 644: [103, 40, 54], 646: [104, 40, 54], 648: [105, 40, 54], 645: [121, 41, 55], 647: [122, 41, 55], 649: [123, 41, 55], 512: [106, 40, 54], 516: [108, 40, 54], 514: [124, 41, 55], 518: [126, 41, 55], 519: [113, 40, 54], 520: [113, 40, 54], 521: [114, 40, 54], 522: [114, 40, 54] }, ka = { 394: 187, 395: 1, 398: 187, 399: 1, 402: 187, 403: 1, 406: 187, 407: 1, 412: 187, 413: 1, 416: 187, 417: 1, 426: 187, 427: 1, 420: 187, 421: 1, 65536: 187, 65537: 187, 65535: 187, 65539: 1, 65540: 1, 65538: 1 }, Sa = { 344: [106, 40, 54], 362: [106, 40, 54], 364: [106, 40, 54], 348: [107, 40, 54], 352: [108, 40, 54], 366: [108, 40, 54], 368: [108, 40, 54], 356: [109, 40, 54], 360: [110, 40, 54], 380: [111, 40, 54], 384: [112, 40, 54], 374: [113, 40, 54], 376: [114, 40, 54], 378: [115, 40, 54], 388: [116, 40, 54], 390: [117, 40, 54], 386: [118, 40, 54], 345: [124, 41, 55], 349: [125, 41, 55], 353: [126, 41, 55], 357: [127, 41, 55], 381: [129, 41, 55], 361: [128, 41, 55], 385: [130, 41, 55], 375: [131, 41, 55], 377: [132, 41, 55], 379: [133, 41, 55], 389: [134, 41, 55], 391: [135, 41, 55], 387: [136, 41, 55], 346: [146, 42, 56], 350: [147, 42, 56], 354: [148, 42, 56], 358: [149, 42, 56], 347: [160, 43, 57], 351: [161, 43, 57], 355: [162, 43, 57], 359: [163, 43, 57], 392: [70, 40, 54], 396: [71, 40, 54], 414: [72, 40, 54], 400: [74, 40, 54], 418: [76, 40, 54], 404: [78, 40, 54], 424: [73, 40, 54], 410: [75, 40, 54], 422: [77, 40, 54], 408: [79, 40, 54], 393: [81, 41, 54], 397: [82, 41, 54], 415: [83, 41, 54], 401: [85, 41, 54], 419: [87, 41, 54], 405: [89, 41, 54], 425: [84, 41, 54], 411: [86, 41, 54], 423: [88, 41, 54], 409: [90, 41, 54] }, va = { 187: 392, 207: 396, 195: 400, 215: 410, 199: 414, 223: 424, 191: 404, 211: 408, 203: 418, 219: 422, 231: [392, false, true], 241: [396, false, true], 235: [400, false, true], 245: [410, false, true], 237: [414, false, true], 249: [424, false, true], 233: [404, false, true], 243: [408, false, true], 239: [418, false, true], 247: [422, false, true], 251: [392, 65, true], 261: [396, 65, true], 255: [400, 65, true], 265: [410, 65, true], 257: [414, 65, true], 269: [424, 65, true], 253: [404, 65, true], 263: [408, 65, true], 259: [418, 65, true], 267: [422, 65, true], 188: 393, 208: 397, 196: 401, 216: 411, 200: 415, 224: 425, 192: 405, 212: 409, 204: 419, 220: 423, 252: [393, 66, true], 256: [401, 66, true], 266: [411, 66, true], 258: [415, 66, true], 270: [425, 66, true], 254: [405, 66, true], 264: [409, 66, true], 260: [419, 66, true], 268: [423, 66, true], 189: 394, 209: 65535, 197: 402, 217: 412, 201: 416, 225: 426, 193: 406, 213: 65536, 205: 420, 221: 65537, 190: 395, 210: 65538, 198: 403, 218: 413, 202: 417, 226: 427, 194: 407, 214: 65539, 206: 421, 222: 65540 }, Ua = { 599: [true, false, 159], 626: [true, true, 145], 586: [true, false, 155], 613: [true, true, 141], 592: [true, false, 156], 619: [true, true, 142], 603: [true, false, 153], 630: [true, true, 139], 581: [true, false, "acos"], 608: [true, true, "acosf"], 582: [true, false, "acosh"], 609: [true, true, "acoshf"], 587: [true, false, "cos"], 614: [true, true, "cosf"], 579: [true, false, "asin"], 606: [true, true, "asinf"], 580: [true, false, "asinh"], 607: [true, true, "asinhf"], 598: [true, false, "sin"], 625: [true, true, "sinf"], 583: [true, false, "atan"], 610: [true, true, "atanf"], 584: [true, false, "atanh"], 611: [true, true, "atanhf"], 601: [true, false, "tan"], 628: [true, true, "tanf"], 588: [true, false, "cbrt"], 615: [true, true, "cbrtf"], 590: [true, false, "exp"], 617: [true, true, "expf"], 593: [true, false, "log"], 620: [true, true, "logf"], 594: [true, false, "log2"], 621: [true, true, "log2f"], 595: [true, false, "log10"], 622: [true, true, "log10f"], 604: [false, false, 164], 631: [false, true, 150], 605: [false, false, 165], 632: [false, true, 151], 585: [false, false, "atan2"], 612: [false, true, "atan2f"], 596: [false, false, "pow"], 623: [false, true, "powf"], 383: [false, false, "fmod"], 382: [false, true, "fmodf"] }, Ta = { 560: [67, 0, 0], 561: [67, 192, 0], 562: [68, 0, 1], 563: [68, 193, 1], 564: [65, 0, 2], 565: [66, 0, 3] }, Ea = { 566: [74, 0, 0], 567: [74, 192, 0], 568: [75, 0, 1], 569: [75, 193, 1], 570: [72, 0, 2], 571: [73, 0, 3] }, xa = { 653: 1, 654: 2, 655: 4, 656: 8 }, Ia = { 653: 44, 654: 46, 655: 40, 656: 41 }, Aa = { 653: 58, 654: 59, 655: 54, 656: 55 }, ja = /* @__PURE__ */ new Set([20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 80, 81, 82, 83, 84, 85, 86, 87, 88, 89, 90, 91]), $a = { 51: [16, 54], 52: [16, 54], 53: [8, 54], 54: [8, 54], 55: [4, 54], 57: [4, 56], 56: [2, 55], 58: [2, 57] }, La = { 1: [16, 40], 2: [8, 40], 3: [4, 40], 5: [4, 42], 4: [2, 41], 6: [2, 43] }, Ra = /* @__PURE__ */ new Set([81, 84, 85, 86, 87, 82, 83, 88, 89, 90, 91, 92, 93]), Ba = { 13: [16], 14: [8], 15: [4], 16: [2] }, Na = { 10: 100, 11: 132, 12: 164, 13: 196, 45: 100, 46: 132, 47: 164, 48: 196 }, Oa = { 6: [44, 23], 7: [46, 26], 8: [40, 28], 9: [41, 30] };
function Ca(e2, t2) {
  return O(e2 + 2 * t2);
}
function Da(e2, t2) {
  return z(e2 + 2 * t2);
}
function Fa(e2, t2) {
  return P(e2 + 2 * t2);
}
function Pa(e2, t2) {
  return F(e2 + 2 * t2);
}
function Ma(e2) {
  return P(e2 + ea(4));
}
function za(e2, t2) {
  const n2 = P(Ma(e2) + ea(5));
  return P(n2 + t2 * gc);
}
function Va(e2, t2) {
  const n2 = P(Ma(e2) + ea(12));
  return P(n2 + t2 * gc);
}
function Wa(e2, t2, n2) {
  if (!n2) return false;
  for (let r2 = 0; r2 < n2.length; r2++) if (2 * n2[r2] + t2 === e2) return true;
  return false;
}
const Ha = /* @__PURE__ */ new Map();
function qa(e2, t2) {
  if (!ci(e2, t2)) return Ha.get(t2);
}
function Ga(e2, t2) {
  const n2 = qa(e2, t2);
  if (void 0 !== n2) switch (n2.type) {
    case "i32":
    case "v128":
      return n2.value;
  }
}
function Ja(e2, t2) {
  try {
    let n2 = o.mono_jiterp_get_opcode_info(t2, 1);
    return 271 === t2 && (n2 = 4 + 2 * Fa(e2, 2)), n2;
  } catch (n2) {
    throw He(`Found invalid opcode ${t2} at ip ${e2}`), n2;
  }
}
const Xa = /* @__PURE__ */ new Map();
let Qa = -1;
function Ya() {
  Qa = -1, Xa.clear(), Ha.clear();
}
function Za(e2) {
  Qa === e2 && (Qa = -1), Xa.delete(e2), Ha.delete(e2);
}
function Ka(e2, t2) {
  for (let n2 = 0; n2 < t2; n2 += 1) Za(e2 + n2);
}
function ei(e2, t2, n2) {
  e2.cfg.startBranchBlock(t2, n2);
}
function ti(e2, t2, n2) {
  let r2 = 0;
  switch (e2 % 16 == 0 ? r2 = 4 : e2 % 8 == 0 ? r2 = 3 : e2 % 4 == 0 ? r2 = 2 : e2 % 2 == 0 && (r2 = 1), t2) {
    case 253:
      r2 = 0 === n2 || 11 === n2 ? Math.min(r2, 4) : 0;
      break;
    case 41:
    case 43:
    case 55:
    case 57:
      r2 = Math.min(r2, 3);
      break;
    case 52:
    case 53:
    case 62:
    case 40:
    case 42:
    case 54:
    case 56:
      r2 = Math.min(r2, 2);
      break;
    case 50:
    case 51:
    case 46:
    case 47:
    case 61:
    case 59:
      r2 = Math.min(r2, 1);
      break;
    default:
      r2 = 0;
  }
  return r2;
}
function ni(e2, t2, n2, r2, o2) {
  if (e2.options.cprop && 40 === n2) {
    const n3 = qa(e2, t2);
    if (n3) switch (n3.type) {
      case "i32":
        return !(o2 && 0 === n3.value || (r2 || e2.i32_const(n3.value), 0));
      case "ldloca":
        return r2 || si(e2, n3.offset, 0), true;
    }
  }
  return false;
}
function ri(e2, t2, n2, r2) {
  if (ni(e2, t2, n2, false)) return;
  if (e2.local("pLocals"), n2 >= 40 || mt(false, `Expected load opcode but got ${n2}`), e2.appendU8(n2), void 0 !== r2) e2.appendULeb(r2);
  else if (253 === n2) throw new Error("PREFIX_simd ldloc without a simdOpcode");
  const o2 = ti(t2, n2, r2);
  e2.appendMemarg(t2, o2);
}
function oi(e2, t2, n2, r2) {
  n2 >= 54 || mt(false, `Expected store opcode but got ${n2}`), e2.appendU8(n2), void 0 !== r2 && e2.appendULeb(r2);
  const o2 = ti(t2, n2, r2);
  e2.appendMemarg(t2, o2), Za(t2), void 0 !== r2 && Za(t2 + 8);
}
function si(e2, t2, n2) {
  n2 > 0 && Ka(t2, n2), e2.lea("pLocals", t2);
}
function ai(e2, t2, n2, r2) {
  Ka(t2, r2), Js(e2, t2, 0, r2, false) || (si(e2, t2, r2), Xs(e2, n2, r2));
}
function ii(e2, t2, n2, r2) {
  if (Ka(t2, r2), Qs(e2, t2, n2, r2, false)) return true;
  si(e2, t2, r2), si(e2, n2, 0), Ys(e2, r2);
}
function ci(e2, t2) {
  return 0 !== o.mono_jiterp_is_imethod_var_address_taken(Ma(e2.frame), t2);
}
function li(e2, t2, n2, r2) {
  if (e2.allowNullCheckOptimization && Xa.has(t2) && !ci(e2, t2)) return da(7, 1), void (Qa === t2 ? r2 && e2.local("cknull_ptr") : (ri(e2, t2, 40), e2.local("cknull_ptr", r2 ? 34 : 33), Qa = t2));
  ri(e2, t2, 40), e2.local("cknull_ptr", 34), e2.appendU8(69), e2.block(64, 4), Ws(e2, n2, 2), e2.endBlock(), r2 && e2.local("cknull_ptr"), e2.allowNullCheckOptimization && !ci(e2, t2) ? (Xa.set(t2, n2), Qa = t2) : Qa = -1;
}
function pi(e2, t2, n2) {
  let r2, s2 = 54;
  const a2 = ba[n2];
  if (a2) e2.local("pLocals"), e2.appendU8(a2[0]), r2 = a2[1], e2.appendLeb(r2);
  else switch (n2) {
    case 15:
      e2.local("pLocals"), r2 = Da(t2, 2), e2.i32_const(r2);
      break;
    case 16:
      e2.local("pLocals"), r2 = Pa(t2, 2), e2.i32_const(r2);
      break;
    case 17:
      e2.local("pLocals"), e2.i52_const(0), s2 = 55;
      break;
    case 19:
      e2.local("pLocals"), e2.appendU8(66), e2.appendLebRef(t2 + 4, true), s2 = 55;
      break;
    case 18:
      e2.local("pLocals"), e2.i52_const(Da(t2, 2)), s2 = 55;
      break;
    case 20:
      e2.local("pLocals"), e2.appendU8(67), e2.appendF32((function(e3, t3) {
        return n3 = e3 + 2 * t3, o.mono_wasm_get_f32_unaligned(n3);
        var n3;
      })(t2, 2)), s2 = 56;
      break;
    case 21:
      e2.local("pLocals"), e2.appendU8(68), e2.appendF64((function(e3, t3) {
        return n3 = e3 + 2 * t3, o.mono_wasm_get_f64_unaligned(n3);
        var n3;
      })(t2, 2)), s2 = 57;
      break;
    default:
      return false;
  }
  e2.appendU8(s2);
  const i2 = Ca(t2, 1);
  return e2.appendMemarg(i2, 2), Za(i2), "number" == typeof r2 ? Ha.set(i2, { type: "i32", value: r2 }) : Ha.delete(i2), true;
}
function ui(e2, t2, n2) {
  let r2 = 40, o2 = 54;
  switch (n2) {
    case 74:
      r2 = 44;
      break;
    case 75:
      r2 = 45;
      break;
    case 76:
      r2 = 46;
      break;
    case 77:
      r2 = 47;
      break;
    case 78:
      r2 = 45, o2 = 58;
      break;
    case 79:
      r2 = 47, o2 = 59;
      break;
    case 80:
      break;
    case 81:
      r2 = 41, o2 = 55;
      break;
    case 82: {
      const n3 = Ca(t2, 3);
      return ii(e2, Ca(t2, 1), Ca(t2, 2), n3), true;
    }
    case 83:
      return ii(e2, Ca(t2, 1), Ca(t2, 2), 8), ii(e2, Ca(t2, 3), Ca(t2, 4), 8), true;
    case 84:
      return ii(e2, Ca(t2, 1), Ca(t2, 2), 8), ii(e2, Ca(t2, 3), Ca(t2, 4), 8), ii(e2, Ca(t2, 5), Ca(t2, 6), 8), true;
    case 85:
      return ii(e2, Ca(t2, 1), Ca(t2, 2), 8), ii(e2, Ca(t2, 3), Ca(t2, 4), 8), ii(e2, Ca(t2, 5), Ca(t2, 6), 8), ii(e2, Ca(t2, 7), Ca(t2, 8), 8), true;
    default:
      return false;
  }
  return e2.local("pLocals"), ri(e2, Ca(t2, 2), r2), oi(e2, Ca(t2, 1), o2), true;
}
function di(e2, t2, n2, r2) {
  const o2 = r2 >= 23 && r2 <= 36 || r2 >= 50 && r2 <= 60, s2 = Ca(n2, o2 ? 2 : 1), a2 = Ca(n2, 3), i2 = Ca(n2, o2 ? 1 : 2), c2 = e2.allowNullCheckOptimization && Xa.has(s2) && !ci(e2, s2);
  36 !== r2 && 45 !== r2 && li(e2, s2, n2, false);
  let l2 = 54, p2 = 40;
  switch (r2) {
    case 23:
      p2 = 44;
      break;
    case 24:
      p2 = 45;
      break;
    case 25:
      p2 = 46;
      break;
    case 26:
      p2 = 47;
      break;
    case 31:
    case 41:
    case 27:
      break;
    case 43:
    case 29:
      p2 = 42, l2 = 56;
      break;
    case 44:
    case 30:
      p2 = 43, l2 = 57;
      break;
    case 37:
    case 38:
      l2 = 58;
      break;
    case 39:
    case 40:
      l2 = 59;
      break;
    case 28:
    case 42:
      p2 = 41, l2 = 55;
      break;
    case 45:
      return c2 || e2.block(), e2.local("pLocals"), e2.i32_const(a2), e2.i32_const(s2), e2.i32_const(i2), e2.callImport("stfld_o"), c2 ? (e2.appendU8(26), da(7, 1)) : (e2.appendU8(13), e2.appendULeb(0), Ws(e2, n2, 2), e2.endBlock()), true;
    case 32: {
      const t3 = Ca(n2, 4);
      return si(e2, i2, t3), e2.local("cknull_ptr"), 0 !== a2 && (e2.i32_const(a2), e2.appendU8(106)), Ys(e2, t3), true;
    }
    case 46: {
      const r3 = za(t2, Ca(n2, 4));
      return e2.local("cknull_ptr"), 0 !== a2 && (e2.i32_const(a2), e2.appendU8(106)), si(e2, i2, 0), e2.ptr_const(r3), e2.callImport("value_copy"), true;
    }
    case 47: {
      const t3 = Ca(n2, 4);
      return e2.local("cknull_ptr"), 0 !== a2 && (e2.i32_const(a2), e2.appendU8(106)), si(e2, i2, 0), Ys(e2, t3), true;
    }
    case 36:
    case 35:
      return e2.local("pLocals"), ri(e2, s2, 40), 0 !== a2 && (e2.i32_const(a2), e2.appendU8(106)), oi(e2, i2, l2), true;
    default:
      return false;
  }
  return o2 && e2.local("pLocals"), e2.local("cknull_ptr"), o2 ? (e2.appendU8(p2), e2.appendMemarg(a2, 0), oi(e2, i2, l2), true) : (ri(e2, i2, p2), e2.appendU8(l2), e2.appendMemarg(a2, 0), true);
}
function fi(e2, t2, n2, r2) {
  const o2 = r2 >= 23 && r2 <= 36 || r2 >= 50 && r2 <= 60, s2 = Ca(n2, 1), a2 = za(t2, Ca(n2, 2)), i2 = za(t2, Ca(n2, 3));
  !(function(e3, t3, n3) {
    e3.block(), e3.ptr_const(t3), e3.appendU8(45), e3.appendMemarg(ea(0), 0), e3.appendU8(13), e3.appendULeb(0), Ws(e3, n3, 3), e3.endBlock();
  })(e2, a2, n2);
  let c2 = 54, l2 = 40;
  switch (r2) {
    case 50:
      l2 = 44;
      break;
    case 51:
      l2 = 45;
      break;
    case 52:
      l2 = 46;
      break;
    case 53:
      l2 = 47;
      break;
    case 58:
    case 65:
    case 54:
      break;
    case 67:
    case 56:
      l2 = 42, c2 = 56;
      break;
    case 68:
    case 57:
      l2 = 43, c2 = 57;
      break;
    case 61:
    case 62:
      c2 = 58;
      break;
    case 63:
    case 64:
      c2 = 59;
      break;
    case 55:
    case 66:
      l2 = 41, c2 = 55;
      break;
    case 69:
      return e2.ptr_const(i2), si(e2, s2, 0), e2.callImport("copy_ptr"), true;
    case 59: {
      const t3 = Ca(n2, 4);
      return si(e2, s2, t3), e2.ptr_const(i2), Ys(e2, t3), true;
    }
    case 72:
      return e2.local("pLocals"), e2.ptr_const(i2), oi(e2, s2, c2), true;
    default:
      return false;
  }
  return o2 ? (e2.local("pLocals"), e2.ptr_const(i2), e2.appendU8(l2), e2.appendMemarg(0, 0), oi(e2, s2, c2), true) : (e2.ptr_const(i2), ri(e2, s2, l2), e2.appendU8(c2), e2.appendMemarg(0, 0), true);
}
function _i(e2, t2, n2) {
  let r2, o2, s2, a2, i2 = "math_lhs32", c2 = "math_rhs32", l2 = false;
  const p2 = ka[n2];
  if (p2) {
    e2.local("pLocals");
    const r3 = 1 == p2;
    return ri(e2, Ca(t2, 2), r3 ? 43 : 42), r3 || e2.appendU8(p2), ri(e2, Ca(t2, 3), r3 ? 43 : 42), r3 || e2.appendU8(p2), e2.i32_const(n2), e2.callImport("relop_fp"), oi(e2, Ca(t2, 1), 54), true;
  }
  switch (n2) {
    case 382:
    case 383:
      return wi(e2, t2, n2);
    default:
      if (a2 = Sa[n2], !a2) return false;
      a2.length > 3 ? (r2 = a2[1], o2 = a2[2], s2 = a2[3]) : (r2 = o2 = a2[1], s2 = a2[2]);
  }
  switch (n2) {
    case 356:
    case 357:
    case 360:
    case 361:
    case 380:
    case 381:
    case 384:
    case 385: {
      const s3 = 361 === n2 || 385 === n2 || 357 === n2 || 381 === n2;
      i2 = s3 ? "math_lhs64" : "math_lhs32", c2 = s3 ? "math_rhs64" : "math_rhs32", e2.block(), ri(e2, Ca(t2, 2), r2), e2.local(i2, 33), ri(e2, Ca(t2, 3), o2), e2.local(c2, 34), l2 = true, s3 && (e2.appendU8(80), e2.appendU8(69)), e2.appendU8(13), e2.appendULeb(0), Ws(e2, t2, 12), e2.endBlock(), 356 !== n2 && 380 !== n2 && 357 !== n2 && 381 !== n2 || (e2.block(), e2.local(c2), s3 ? e2.i52_const(-1) : e2.i32_const(-1), e2.appendU8(s3 ? 82 : 71), e2.appendU8(13), e2.appendULeb(0), e2.local(i2), e2.appendU8(s3 ? 66 : 65), e2.appendBoundaryValue(s3 ? 64 : 32, -1), e2.appendU8(s3 ? 82 : 71), e2.appendU8(13), e2.appendULeb(0), Ws(e2, t2, 13), e2.endBlock());
      break;
    }
    case 362:
    case 364:
    case 366:
    case 368:
      ri(e2, Ca(t2, 2), r2), e2.local(i2, 34), ri(e2, Ca(t2, 3), o2), e2.local(c2, 34), e2.i32_const(n2), e2.callImport(364 === n2 || 368 === n2 ? "ckovr_u4" : "ckovr_i4"), e2.block(64, 4), Ws(e2, t2, 13), e2.endBlock(), l2 = true;
  }
  return e2.local("pLocals"), l2 ? (e2.local(i2), e2.local(c2)) : (ri(e2, Ca(t2, 2), r2), ri(e2, Ca(t2, 3), o2)), e2.appendU8(a2[0]), oi(e2, Ca(t2, 1), s2), true;
}
function mi(e2, t2, n2) {
  const r2 = wa[n2];
  if (!r2) return false;
  const o2 = r2[1], s2 = r2[2];
  switch ((n2 < 472 || n2 > 507) && e2.local("pLocals"), n2) {
    case 428:
    case 430:
      ri(e2, Ca(t2, 2), o2), e2.i32_const(1);
      break;
    case 432:
      e2.i32_const(0), ri(e2, Ca(t2, 2), o2);
      break;
    case 436:
      ri(e2, Ca(t2, 2), o2), e2.i32_const(-1);
      break;
    case 444:
    case 445:
      ri(e2, Ca(t2, 2), o2), 41 === o2 && e2.appendU8(167), e2.i32_const(255);
      break;
    case 452:
    case 453:
      ri(e2, Ca(t2, 2), o2), 41 === o2 && e2.appendU8(167), e2.i32_const(65535);
      break;
    case 440:
    case 441:
      ri(e2, Ca(t2, 2), o2), 41 === o2 && e2.appendU8(167), e2.i32_const(24), e2.appendU8(116), e2.i32_const(24);
      break;
    case 448:
    case 449:
      ri(e2, Ca(t2, 2), o2), 41 === o2 && e2.appendU8(167), e2.i32_const(16), e2.appendU8(116), e2.i32_const(16);
      break;
    case 429:
    case 431:
      ri(e2, Ca(t2, 2), o2), e2.i52_const(1);
      break;
    case 433:
      e2.i52_const(0), ri(e2, Ca(t2, 2), o2);
      break;
    case 437:
      ri(e2, Ca(t2, 2), o2), e2.i52_const(-1);
      break;
    case 511:
    case 515:
    case 519:
    case 521:
    case 525:
    case 527:
    case 523:
    case 640:
    case 642:
      ri(e2, Ca(t2, 2), o2), e2.i32_const(Da(t2, 3));
      break;
    case 512:
    case 516:
    case 520:
    case 522:
      ri(e2, Ca(t2, 2), o2), e2.i32_const(Pa(t2, 3));
      break;
    case 513:
    case 517:
    case 526:
    case 528:
    case 524:
    case 641:
    case 643:
      ri(e2, Ca(t2, 2), o2), e2.i52_const(Da(t2, 3));
      break;
    case 514:
    case 518:
      ri(e2, Ca(t2, 2), o2), e2.i52_const(Pa(t2, 3));
      break;
    default:
      ri(e2, Ca(t2, 2), o2);
  }
  return 1 !== r2[0] && e2.appendU8(r2[0]), oi(e2, Ca(t2, 1), s2), true;
}
function hi(e2, t2, n2, r2) {
  const o2 = 133 === r2 ? t2 + 6 : t2 + 8, s2 = Va(n2, O(o2 - 2));
  e2.local("pLocals"), e2.ptr_const(o2), e2.appendU8(54), e2.appendMemarg(s2, 0), e2.callHandlerReturnAddresses.push(o2);
}
function gi(e2, t2) {
  const n2 = o.mono_jiterp_get_opcode_info(t2, 4), r2 = e2 + 2 + 2 * o.mono_jiterp_get_opcode_info(t2, 2);
  let s2;
  switch (n2) {
    case 7:
      s2 = F(r2);
      break;
    case 8:
      s2 = z(r2);
      break;
    case 17:
      s2 = z(r2 + 2);
      break;
    default:
      return;
  }
  return s2;
}
function bi(e2, t2, n2, r2) {
  const s2 = r2 >= 227 && r2 <= 270, a2 = gi(t2, r2);
  if ("number" != typeof a2) return false;
  switch (r2) {
    case 132:
    case 133:
    case 128:
    case 129: {
      const s3 = 132 === r2 || 133 === r2, i3 = t2 + 2 * a2;
      return a2 <= 0 ? e2.backBranchOffsets.indexOf(i3) >= 0 ? (e2.backBranchTraceLevel > 1 && Ve(`0x${t2.toString(16)} performing backward branch to 0x${i3.toString(16)}`), s3 && hi(e2, t2, n2, r2), e2.cfg.branch(i3, true, 0), da(9, 1), true) : (i3 < e2.cfg.entryIp ? (e2.backBranchTraceLevel > 1 || e2.cfg.trace > 1) && Ve(`0x${t2.toString(16)} ${$s(r2)} target 0x${i3.toString(16)} before start of trace`) : (e2.backBranchTraceLevel > 0 || e2.cfg.trace > 0) && Ve(`0x${t2.toString(16)} ${$s(r2)} target 0x${i3.toString(16)} not found in list ` + e2.backBranchOffsets.map(((e3) => "0x" + e3.toString(16))).join(", ")), o.mono_jiterp_boost_back_branch_target(i3), Ws(e2, i3, 5), da(10, 1), true) : (e2.branchTargets.add(i3), s3 && hi(e2, t2, n2, r2), e2.cfg.branch(i3, false, 0), true);
    }
    case 145:
    case 143:
    case 229:
    case 227:
    case 146:
    case 144: {
      const n3 = 146 === r2 || 144 === r2;
      ri(e2, Ca(t2, 1), n3 ? 41 : 40), 143 === r2 || 227 === r2 ? e2.appendU8(69) : 144 === r2 ? e2.appendU8(80) : 146 === r2 && (e2.appendU8(80), e2.appendU8(69));
      break;
    }
    default:
      if (void 0 === va[r2]) throw new Error(`Unsupported relop branch opcode: ${$s(r2)}`);
      if (4 !== o.mono_jiterp_get_opcode_info(r2, 1)) throw new Error(`Unsupported long branch opcode: ${$s(r2)}`);
  }
  const i2 = t2 + 2 * a2;
  return a2 < 0 ? e2.backBranchOffsets.indexOf(i2) >= 0 ? (e2.backBranchTraceLevel > 1 && Ve(`0x${t2.toString(16)} performing conditional backward branch to 0x${i2.toString(16)}`), e2.cfg.branch(i2, true, s2 ? 3 : 1), da(9, 1)) : (i2 < e2.cfg.entryIp ? (e2.backBranchTraceLevel > 1 || e2.cfg.trace > 1) && Ve(`0x${t2.toString(16)} ${$s(r2)} target 0x${i2.toString(16)} before start of trace`) : (e2.backBranchTraceLevel > 0 || e2.cfg.trace > 0) && Ve(`0x${t2.toString(16)} ${$s(r2)} target 0x${i2.toString(16)} not found in list ` + e2.backBranchOffsets.map(((e3) => "0x" + e3.toString(16))).join(", ")), o.mono_jiterp_boost_back_branch_target(i2), e2.block(64, 4), Ws(e2, i2, 5), e2.endBlock(), da(10, 1)) : (e2.branchTargets.add(i2), e2.cfg.branch(i2, false, s2 ? 3 : 1)), true;
}
function yi(e2, t2, n2, r2) {
  const o2 = va[r2];
  if (!o2) return false;
  const s2 = Array.isArray(o2) ? o2[0] : o2, a2 = Sa[s2], i2 = ka[s2];
  if (!a2 && !i2) return false;
  const c2 = a2 ? a2[1] : 1 === i2 ? 43 : 42;
  return ri(e2, Ca(t2, 1), c2), a2 || 1 === i2 || e2.appendU8(i2), Array.isArray(o2) && o2[1] ? (e2.appendU8(o2[1]), e2.appendLeb(Da(t2, 2))) : ri(e2, Ca(t2, 2), c2), a2 || 1 == i2 || e2.appendU8(i2), a2 ? e2.appendU8(a2[0]) : (e2.i32_const(s2), e2.callImport("relop_fp")), bi(e2, t2, n2, r2);
}
function wi(e2, t2, n2) {
  let r2, o2, s2, a2;
  const i2 = Ca(t2, 1), c2 = Ca(t2, 2), l2 = Ca(t2, 3), p2 = Ua[n2];
  if (!p2) return false;
  if (r2 = p2[0], o2 = p2[1], "string" == typeof p2[2] ? s2 = p2[2] : a2 = p2[2], e2.local("pLocals"), r2) {
    if (ri(e2, c2, o2 ? 42 : 43), a2) e2.appendU8(a2);
    else {
      if (!s2) throw new Error("internal error");
      e2.callImport(s2);
    }
    return oi(e2, i2, o2 ? 56 : 57), true;
  }
  if (ri(e2, c2, o2 ? 42 : 43), ri(e2, l2, o2 ? 42 : 43), a2) e2.appendU8(a2);
  else {
    if (!s2) throw new Error("internal error");
    e2.callImport(s2);
  }
  return oi(e2, i2, o2 ? 56 : 57), true;
}
function ki(e2, t2, n2) {
  const r2 = n2 >= 87 && n2 <= 112, o2 = n2 >= 107 && n2 <= 112, s2 = n2 >= 95 && n2 <= 106 || n2 >= 120 && n2 <= 127 || o2, a2 = n2 >= 101 && n2 <= 106 || n2 >= 124 && n2 <= 127 || o2;
  let i2, c2, l2 = -1, p2 = 0, u2 = 1;
  o2 ? (i2 = Ca(t2, 1), c2 = Ca(t2, 2), l2 = Ca(t2, 3), p2 = Da(t2, 4), u2 = Da(t2, 5)) : s2 ? a2 ? r2 ? (i2 = Ca(t2, 1), c2 = Ca(t2, 2), p2 = Da(t2, 3)) : (i2 = Ca(t2, 2), c2 = Ca(t2, 1), p2 = Da(t2, 3)) : r2 ? (i2 = Ca(t2, 1), c2 = Ca(t2, 2), l2 = Ca(t2, 3)) : (i2 = Ca(t2, 3), c2 = Ca(t2, 1), l2 = Ca(t2, 2)) : r2 ? (c2 = Ca(t2, 2), i2 = Ca(t2, 1)) : (c2 = Ca(t2, 1), i2 = Ca(t2, 2));
  let d2, f2 = 54;
  switch (n2) {
    case 87:
    case 95:
    case 101:
    case 107:
      d2 = 44;
      break;
    case 88:
    case 96:
    case 102:
    case 108:
      d2 = 45;
      break;
    case 89:
    case 97:
    case 103:
    case 109:
      d2 = 46;
      break;
    case 90:
    case 98:
    case 104:
    case 110:
      d2 = 47;
      break;
    case 113:
    case 120:
    case 124:
      d2 = 40, f2 = 58;
      break;
    case 114:
    case 121:
    case 125:
      d2 = 40, f2 = 59;
      break;
    case 91:
    case 99:
    case 105:
    case 111:
    case 115:
    case 122:
    case 126:
    case 119:
      d2 = 40;
      break;
    case 93:
    case 117:
      d2 = 42, f2 = 56;
      break;
    case 94:
    case 118:
      d2 = 43, f2 = 57;
      break;
    case 92:
    case 100:
    case 106:
    case 112:
    case 116:
    case 123:
    case 127:
      d2 = 41, f2 = 55;
      break;
    default:
      return false;
  }
  const _2 = ni(e2, c2, 40, true, true);
  return _2 || li(e2, c2, t2, false), r2 ? (e2.local("pLocals"), _2 ? mt(ni(e2, c2, 40, false, true), "Unknown jiterpreter cprop failure") : e2.local("cknull_ptr"), o2 ? (ri(e2, l2, 40), 0 !== p2 && (e2.i32_const(p2), e2.appendU8(106), p2 = 0), 1 !== u2 && (e2.i32_const(u2), e2.appendU8(108)), e2.appendU8(106)) : s2 && l2 >= 0 ? (ri(e2, l2, 40), e2.appendU8(106)) : p2 < 0 && (e2.i32_const(p2), e2.appendU8(106), p2 = 0), e2.appendU8(d2), e2.appendMemarg(p2, 0), oi(e2, i2, f2)) : 119 === n2 ? (_2 ? mt(ni(e2, c2, 40, false, true), "Unknown jiterpreter cprop failure") : e2.local("cknull_ptr"), si(e2, i2, 0), e2.callImport("copy_ptr")) : (_2 ? mt(ni(e2, c2, 40, false, true), "Unknown jiterpreter cprop failure") : e2.local("cknull_ptr"), s2 && l2 >= 0 ? (ri(e2, l2, 40), e2.appendU8(106)) : p2 < 0 && (e2.i32_const(p2), e2.appendU8(106), p2 = 0), ri(e2, i2, d2), e2.appendU8(f2), e2.appendMemarg(p2, 0)), true;
}
function Si(e2, t2, n2, r2, o2) {
  e2.block(), ri(e2, r2, 40), e2.local("index", 34);
  let s2 = "cknull_ptr";
  e2.options.zeroPageOptimization && aa() ? (da(8, 1), ri(e2, n2, 40), s2 = "src_ptr", e2.local(s2, 34)) : li(e2, n2, t2, true), e2.appendU8(40), e2.appendMemarg(ea(9), 2), e2.appendU8(73), e2.appendU8(13), e2.appendULeb(0), Ws(e2, t2, 9), e2.endBlock(), e2.local(s2), e2.i32_const(ea(1)), e2.appendU8(106), e2.local("index"), 1 != o2 && (e2.i32_const(o2), e2.appendU8(108)), e2.appendU8(106);
}
function vi(e2, t2, n2, r2) {
  const o2 = r2 <= 328 && r2 >= 315 || 341 === r2, s2 = Ca(n2, o2 ? 2 : 1), a2 = Ca(n2, o2 ? 1 : 3), i2 = Ca(n2, o2 ? 3 : 2);
  let c2, l2, p2 = 54;
  switch (r2) {
    case 341:
      return e2.local("pLocals"), li(e2, s2, n2, true), e2.appendU8(40), e2.appendMemarg(ea(9), 2), oi(e2, a2, 54), true;
    case 326:
      return e2.local("pLocals"), l2 = Ca(n2, 4), Si(e2, n2, s2, i2, l2), oi(e2, a2, 54), true;
    case 337:
      return e2.block(), ri(e2, Ca(n2, 1), 40), ri(e2, Ca(n2, 2), 40), ri(e2, Ca(n2, 3), 40), e2.callImport("stelemr_tc"), e2.appendU8(13), e2.appendULeb(0), Ws(e2, n2, 10), e2.endBlock(), true;
    case 340:
      return Si(e2, n2, s2, i2, 4), si(e2, a2, 0), e2.callImport("copy_ptr"), true;
    case 324:
    case 320:
    case 319:
    case 333:
      l2 = 4, c2 = 40;
      break;
    case 315:
      l2 = 1, c2 = 44;
      break;
    case 316:
      l2 = 1, c2 = 45;
      break;
    case 330:
    case 329:
      l2 = 1, c2 = 40, p2 = 58;
      break;
    case 317:
      l2 = 2, c2 = 46;
      break;
    case 318:
      l2 = 2, c2 = 47;
      break;
    case 332:
    case 331:
      l2 = 2, c2 = 40, p2 = 59;
      break;
    case 322:
    case 335:
      l2 = 4, c2 = 42, p2 = 56;
      break;
    case 321:
    case 334:
      l2 = 8, c2 = 41, p2 = 55;
      break;
    case 323:
    case 336:
      l2 = 8, c2 = 43, p2 = 57;
      break;
    case 325: {
      const t3 = Ca(n2, 4);
      return e2.local("pLocals"), e2.i32_const(Ca(n2, 1)), e2.appendU8(106), Si(e2, n2, s2, i2, t3), Ys(e2, t3), Ka(Ca(n2, 1), t3), true;
    }
    case 338: {
      const r3 = Ca(n2, 5), o3 = za(t2, Ca(n2, 4));
      return Si(e2, n2, s2, i2, r3), si(e2, a2, 0), e2.ptr_const(o3), e2.callImport("value_copy"), true;
    }
    case 339: {
      const t3 = Ca(n2, 5);
      return Si(e2, n2, s2, i2, t3), si(e2, a2, 0), Ys(e2, t3), true;
    }
    default:
      return false;
  }
  return o2 ? (e2.local("pLocals"), Si(e2, n2, s2, i2, l2), e2.appendU8(c2), e2.appendMemarg(0, 0), oi(e2, a2, p2)) : (Si(e2, n2, s2, i2, l2), ri(e2, a2, c2), e2.appendU8(p2), e2.appendMemarg(0, 0)), true;
}
function Ui(e2, t2, n2) {
  const r2 = `${t2}_${n2.toString(16)}`;
  return "object" != typeof e2.importedFunctions[r2] && e2.defineImportedFunction("s", r2, t2, false, n2), r2;
}
function Ti(e2, t2, n2, r2, s2, a2) {
  if (e2.options.enableSimd && ct.featureWasmSimd) switch (s2) {
    case 2:
      if ((function(e3, t3, n3) {
        const r3 = o.mono_jiterp_get_simd_opcode(1, n3), s3 = Na[n3];
        if (s3) return xi(e3, t3), e3.appendSimd(s3), oi(e3, Ca(t3, 1), 54), true;
        if (r3 >= 0) return Ra.has(n3) ? (e3.local("pLocals"), ri(e3, Ca(t3, 2), 40), e3.appendSimd(r3, true), e3.appendMemarg(0, 0), Ei(e3, t3)) : (xi(e3, t3), e3.appendSimd(r3), Ei(e3, t3)), true;
        switch (n3) {
          case 6:
          case 7:
          case 8:
          case 9: {
            const r4 = Oa[n3];
            return e3.local("pLocals"), e3.v128_const(0), ri(e3, Ca(t3, 2), r4[0]), e3.appendSimd(r4[1]), e3.appendU8(0), oi(e3, Ca(t3, 1), 253, 11), true;
          }
          case 14:
            return xi(e3, t3, 7), Ei(e3, t3), true;
          case 15:
            return xi(e3, t3, 8), Ei(e3, t3), true;
          case 16:
            return xi(e3, t3, 9), Ei(e3, t3), true;
          case 17:
            return xi(e3, t3, 10), Ei(e3, t3), true;
          default:
            return false;
        }
      })(e2, t2, a2)) return true;
      break;
    case 3:
      if ((function(e3, t3, n3) {
        const r3 = o.mono_jiterp_get_simd_opcode(2, n3);
        if (r3 >= 0) {
          const o2 = ja.has(n3), s3 = $a[n3];
          if (o2) e3.local("pLocals"), ri(e3, Ca(t3, 2), 253, 0), ri(e3, Ca(t3, 3), 40), e3.appendSimd(r3), Ei(e3, t3);
          else if (Array.isArray(s3)) {
            const n4 = Ga(e3, Ca(t3, 3)), o3 = s3[0];
            if ("number" != typeof n4) return He(`${e3.functions[0].name}: Non-constant lane index passed to ExtractScalar`), false;
            if (n4 >= o3 || n4 < 0) return He(`${e3.functions[0].name}: ExtractScalar index ${n4} out of range (0 - ${o3 - 1})`), false;
            e3.local("pLocals"), ri(e3, Ca(t3, 2), 253, 0), e3.appendSimd(r3), e3.appendU8(n4), oi(e3, Ca(t3, 1), s3[1]);
          } else Ii(e3, t3), e3.appendSimd(r3), Ei(e3, t3);
          return true;
        }
        switch (n3) {
          case 191:
            return ri(e3, Ca(t3, 2), 40), ri(e3, Ca(t3, 3), 253, 0), e3.appendSimd(11), e3.appendMemarg(0, 0), true;
          case 10:
          case 11:
            return Ii(e3, t3), e3.appendSimd(214), e3.appendSimd(195), 11 === n3 && e3.appendU8(69), oi(e3, Ca(t3, 1), 54), true;
          case 12:
          case 13: {
            const r4 = 13 === n3, o2 = r4 ? 71 : 65;
            return e3.local("pLocals"), ri(e3, Ca(t3, 2), 253, 0), e3.local("math_lhs128", 34), ri(e3, Ca(t3, 3), 253, 0), e3.local("math_rhs128", 34), e3.appendSimd(o2), e3.local("math_lhs128"), e3.local("math_lhs128"), e3.appendSimd(o2), e3.local("math_rhs128"), e3.local("math_rhs128"), e3.appendSimd(o2), e3.appendSimd(80), e3.appendSimd(77), e3.appendSimd(80), e3.appendSimd(r4 ? 195 : 163), oi(e3, Ca(t3, 1), 54), true;
          }
          case 47: {
            const n4 = Ca(t3, 3), r4 = Ga(e3, n4);
            return e3.local("pLocals"), ri(e3, Ca(t3, 2), 253, 0), "object" == typeof r4 ? (e3.appendSimd(12), e3.appendBytes(r4)) : ri(e3, n4, 253, 0), e3.appendSimd(14), Ei(e3, t3), true;
          }
          case 48:
          case 49:
            return (function(e4, t4, n4) {
              const r4 = 16 / n4, o2 = Ca(t4, 3), s3 = Ga(e4, o2);
              if (2 !== r4 && 4 !== r4 && mt(false, "Unsupported shuffle element size"), e4.local("pLocals"), ri(e4, Ca(t4, 2), 253, 0), "object" == typeof s3) {
                const t5 = new Uint8Array(bc), o3 = 2 === r4 ? new Uint16Array(s3.buffer, s3.byteOffset, n4) : new Uint32Array(s3.buffer, s3.byteOffset, n4);
                for (let e5 = 0, s4 = 0; e5 < n4; e5++, s4 += r4) {
                  const n5 = o3[e5];
                  for (let e6 = 0; e6 < r4; e6++) t5[s4 + e6] = n5 * r4 + e6;
                }
                e4.appendSimd(12), e4.appendBytes(t5);
              } else {
                ri(e4, o2, 253, 0), 4 === n4 && (e4.v128_const(0), e4.appendSimd(134)), e4.v128_const(0), e4.appendSimd(102), e4.appendSimd(12);
                for (let t5 = 0; t5 < n4; t5++) for (let n5 = 0; n5 < r4; n5++) e4.appendU8(t5);
                ct.featureWasmRelaxedSimd ? e4.appendSimd(256) : e4.appendSimd(14), e4.i32_const(4 === n4 ? 2 : 1), e4.appendSimd(107), e4.appendSimd(12);
                for (let t5 = 0; t5 < n4; t5++) for (let t6 = 0; t6 < r4; t6++) e4.appendU8(t6);
                e4.appendSimd(80);
              }
              return e4.appendSimd(14), Ei(e4, t4), true;
            })(e3, t3, 48 === n3 ? 8 : 4);
          default:
            return false;
        }
        return false;
      })(e2, t2, a2)) return true;
      break;
    case 4:
      if ((function(e3, t3, n3) {
        const r3 = o.mono_jiterp_get_simd_opcode(3, n3);
        if (r3 >= 0) {
          const o2 = La[n3], s3 = Ba[n3];
          if (Array.isArray(o2)) {
            const n4 = o2[0], s4 = Ga(e3, Ca(t3, 3));
            if ("number" != typeof s4) return He(`${e3.functions[0].name}: Non-constant lane index passed to ReplaceScalar`), false;
            if (s4 >= n4 || s4 < 0) return He(`${e3.functions[0].name}: ReplaceScalar index ${s4} out of range (0 - ${n4 - 1})`), false;
            e3.local("pLocals"), ri(e3, Ca(t3, 2), 253, 0), ri(e3, Ca(t3, 4), o2[1]), e3.appendSimd(r3), e3.appendU8(s4), Ei(e3, t3);
          } else if (Array.isArray(s3)) {
            const n4 = s3[0], o3 = Ga(e3, Ca(t3, 4));
            if ("number" != typeof o3) return He(`${e3.functions[0].name}: Non-constant lane index passed to store method`), false;
            if (o3 >= n4 || o3 < 0) return He(`${e3.functions[0].name}: Store lane ${o3} out of range (0 - ${n4 - 1})`), false;
            ri(e3, Ca(t3, 2), 40), ri(e3, Ca(t3, 3), 253, 0), e3.appendSimd(r3), e3.appendMemarg(0, 0), e3.appendU8(o3);
          } else !(function(e4, t4) {
            e4.local("pLocals"), ri(e4, Ca(t4, 2), 253, 0), ri(e4, Ca(t4, 3), 253, 0), ri(e4, Ca(t4, 4), 253, 0);
          })(e3, t3), e3.appendSimd(r3), Ei(e3, t3);
          return true;
        }
        switch (n3) {
          case 0:
            return e3.local("pLocals"), ri(e3, Ca(t3, 3), 253, 0), ri(e3, Ca(t3, 4), 253, 0), ri(e3, Ca(t3, 2), 253, 0), e3.appendSimd(82), Ei(e3, t3), true;
          case 7: {
            const n4 = Ga(e3, Ca(t3, 4));
            if ("object" != typeof n4) return He(`${e3.functions[0].name}: Non-constant indices passed to PackedSimd.Shuffle`), false;
            for (let t4 = 0; t4 < 32; t4++) {
              const r4 = n4[t4];
              if (r4 < 0 || r4 > 31) return He(`${e3.functions[0].name}: Shuffle lane index #${t4} (${r4}) out of range (0 - 31)`), false;
            }
            return e3.local("pLocals"), ri(e3, Ca(t3, 2), 253, 0), ri(e3, Ca(t3, 3), 253, 0), e3.appendSimd(13), e3.appendBytes(n4), Ei(e3, t3), true;
          }
          default:
            return false;
        }
      })(e2, t2, a2)) return true;
  }
  switch (n2) {
    case 652:
      if (e2.options.enableSimd && ct.featureWasmSimd) {
        e2.local("pLocals");
        const n3 = K().slice(t2 + 4, t2 + 4 + bc);
        e2.v128_const(n3), Ei(e2, t2), Ha.set(Ca(t2, 1), { type: "v128", value: n3 });
      } else si(e2, Ca(t2, 1), bc), e2.ptr_const(t2 + 4), Ys(e2, bc);
      return true;
    case 653:
    case 654:
    case 655:
    case 656: {
      const r3 = xa[n2], o2 = bc / r3, s3 = Ca(t2, 1), a3 = Ca(t2, 2), i2 = Ia[n2], c2 = Aa[n2];
      for (let t3 = 0; t3 < o2; t3++) e2.local("pLocals"), ri(e2, a3 + t3 * yc, i2), oi(e2, s3 + t3 * r3, c2);
      return true;
    }
    case 657: {
      Ms[r2] = (Ms[r2] || 0) + 1, si(e2, Ca(t2, 1), bc), si(e2, Ca(t2, 2), 0);
      const n3 = Ui(e2, "simd_p_p", o.mono_jiterp_get_simd_intrinsic(1, a2));
      return e2.callImport(n3), true;
    }
    case 658: {
      Ms[r2] = (Ms[r2] || 0) + 1, si(e2, Ca(t2, 1), bc), si(e2, Ca(t2, 2), 0), si(e2, Ca(t2, 3), 0);
      const n3 = Ui(e2, "simd_p_pp", o.mono_jiterp_get_simd_intrinsic(2, a2));
      return e2.callImport(n3), true;
    }
    case 659: {
      Ms[r2] = (Ms[r2] || 0) + 1, si(e2, Ca(t2, 1), bc), si(e2, Ca(t2, 2), 0), si(e2, Ca(t2, 3), 0), si(e2, Ca(t2, 4), 0);
      const n3 = Ui(e2, "simd_p_ppp", o.mono_jiterp_get_simd_intrinsic(3, a2));
      return e2.callImport(n3), true;
    }
    default:
      return Ve(`jiterpreter emit_simd failed for ${r2}`), false;
  }
}
function Ei(e2, t2) {
  oi(e2, Ca(t2, 1), 253, 11);
}
function xi(e2, t2, n2) {
  e2.local("pLocals"), ri(e2, Ca(t2, 2), 253, n2 || 0);
}
function Ii(e2, t2) {
  e2.local("pLocals"), ri(e2, Ca(t2, 2), 253, 0), ri(e2, Ca(t2, 3), 253, 0);
}
function Ai(e2, t2, n2) {
  if (559 === n2) return true;
  if (!e2.options.enableAtomics) return false;
  const r2 = Ta[n2];
  if (r2) {
    const n3 = r2[2] > 2;
    return e2.local("pLocals"), li(e2, Ca(t2, 2), t2, true), ri(e2, Ca(t2, 3), n3 ? 41 : 40), e2.appendAtomic(r2[0], false), e2.appendMemarg(0, r2[2]), 0 !== r2[1] && e2.appendU8(r2[1]), oi(e2, Ca(t2, 1), n3 ? 55 : 54), true;
  }
  const o2 = Ea[n2];
  if (o2) {
    const n3 = o2[2] > 2;
    return e2.local("pLocals"), li(e2, Ca(t2, 2), t2, true), ri(e2, Ca(t2, 4), n3 ? 41 : 40), ri(e2, Ca(t2, 3), n3 ? 41 : 40), e2.appendAtomic(o2[0], false), e2.appendMemarg(0, o2[2]), 0 !== o2[1] && e2.appendU8(o2[1]), oi(e2, Ca(t2, 1), n3 ? 55 : 54), true;
  }
  return false;
}
function ji(e2, t2, n2) {
  const r2 = Ja(t2, 271), o2 = (function(e3) {
    271 !== O(e3) && mt(false, "decodeSwitch called on a non-switch");
    const t3 = Fa(e3, 2), n3 = [];
    for (let r3 = 0; r3 < t3; r3++) {
      const t4 = e3 + 8 + 4 * r3, o3 = t4 + 2 * P(t4);
      n3.push(o3);
    }
    return n3;
  })(t2);
  let s2 = false;
  if (o2.length > e2.options.maxSwitchSize) s2 = true;
  else for (const n3 of o2) n3 > t2 && e2.branchTargets.add(n3);
  if (s2) return da(14, o2.length), Ws(e2, t2, 27), true;
  const a2 = t2 + 2 * r2;
  return e2.branchTargets.add(a2), e2.block(), ri(e2, Ca(t2, 1), 40), e2.cfg.jumpTable(o2, a2), e2.endBlock(), Hs(e2, t2, n2, 28), true;
}
const $i = 64;
let Li, Ri, Bi, Ni = 0;
const Oi = {};
function Ci() {
  return Ri || (Ri = [oa("interp_entry_prologue", ta("mono_jiterp_interp_entry_prologue")), oa("interp_entry", ta("mono_jiterp_interp_entry")), oa("unbox", ta("mono_jiterp_object_unbox")), oa("stackval_from_data", ta("mono_jiterp_stackval_from_data"))], Ri);
}
let Di, Fi = class {
  constructor(e2, t2, n2, r2, o2, s2, a2, i2) {
    this.imethod = e2, this.method = t2, this.argumentCount = n2, this.unbox = o2, this.hasThisReference = s2, this.hasReturnValue = a2, this.paramTypes = new Array(n2);
    for (let e3 = 0; e3 < n2; e3++) this.paramTypes[e3] = P(r2 + 4 * e3);
    this.defaultImplementation = i2, this.result = 0, this.hitCount = 0;
  }
  generateName() {
    const e2 = o.mono_wasm_method_get_full_name(this.method);
    try {
      const t2 = Le(e2);
      this.name = t2;
      let n2 = t2;
      if (n2) {
        const e3 = 24;
        n2.length > e3 && (n2 = n2.substring(n2.length - e3, n2.length)), n2 = `${this.imethod.toString(16)}_${n2}`;
      } else n2 = `${this.imethod.toString(16)}_${this.hasThisReference ? "i" : "s"}${this.hasReturnValue ? "_r" : ""}_${this.argumentCount}`;
      this.traceName = n2;
    } finally {
      e2 && m(e2);
    }
  }
  getTraceName() {
    return this.traceName || this.generateName(), this.traceName || "unknown";
  }
  getName() {
    return this.name || this.generateName(), this.name || "unknown";
  }
};
function Pi() {
  const e2 = [];
  let t2 = 0;
  for (; 0 != (t2 = o.mono_jiterp_tlqueue_next(1)); ) {
    const n3 = Oi[t2];
    n3 ? e2.push(n3) : Ve(`Failed to find corresponding info for method ptr ${t2} from jit queue!`);
  }
  if (!e2.length) return;
  const n2 = 4 * e2.length + 1;
  let r2 = Li;
  if (r2 ? r2.clear(n2) : (Li = r2 = new Cs(n2), r2.defineType("unbox", { pMonoObject: 127 }, 127, true), r2.defineType("interp_entry_prologue", { pData: 127, this_arg: 127 }, 127, true), r2.defineType("interp_entry", { pData: 127, res: 127 }, 64, true), r2.defineType("stackval_from_data", { type: 127, result: 127, value: 127 }, 64, true)), r2.options.wasmBytesLimit <= ua(6)) return;
  const s2 = zs();
  let a2 = 0, i2 = true, c2 = false;
  try {
    r2.appendU32(1836278016), r2.appendU32(1);
    for (let t4 = 0; t4 < e2.length; t4++) {
      const n4 = e2[t4], o3 = {};
      n4.hasThisReference && (o3.this_arg = 127), n4.hasReturnValue && (o3.res = 127);
      for (let e3 = 0; e3 < n4.argumentCount; e3++) o3[`arg${e3}`] = 127;
      o3.rmethod = 127, r2.defineType(n4.getTraceName(), o3, 64, false);
    }
    r2.generateTypeSection();
    const t3 = Ci();
    r2.compressImportNames = true;
    for (let e3 = 0; e3 < t3.length; e3++) t3[e3] || mt(false, `trace #${e3} missing`), r2.defineImportedFunction("i", t3[e3][0], t3[e3][1], true, t3[e3][2]);
    for (let e3 = 0; e3 < t3.length; e3++) r2.markImportAsUsed(t3[e3][0]);
    r2._generateImportSection(false), r2.beginSection(3), r2.appendULeb(e2.length);
    for (let t4 = 0; t4 < e2.length; t4++) {
      const n4 = e2[t4].getTraceName();
      r2.functionTypes[n4] || mt(false, "func type missing"), r2.appendULeb(r2.functionTypes[n4][0]);
    }
    r2.beginSection(7), r2.appendULeb(e2.length);
    for (let t4 = 0; t4 < e2.length; t4++) {
      const n4 = e2[t4].getTraceName();
      r2.appendName(n4), r2.appendU8(0), r2.appendULeb(r2.importedFunctionCount + t4);
    }
    r2.beginSection(10), r2.appendULeb(e2.length);
    for (let t4 = 0; t4 < e2.length; t4++) {
      const n4 = e2[t4], o3 = n4.getTraceName();
      r2.beginFunction(o3, { sp_args: 127, need_unbox: 127, scratchBuffer: 127 }), zi(r2, n4), r2.appendU8(11), r2.endFunction(true);
    }
    r2.endSection(), a2 = zs();
    const n3 = r2.getArrayView();
    da(6, n3.length);
    const o2 = new WebAssembly.Module(n3), s3 = r2.getWasmImports(), c3 = new WebAssembly.Instance(o2, s3);
    for (let t4 = 0; t4 < e2.length; t4++) {
      const n4 = e2[t4], r3 = n4.getTraceName(), o3 = c3.exports[r3];
      Bi.set(n4.result, o3), i2 = false;
    }
    da(2, e2.length);
  } catch (e3) {
    c2 = true, i2 = false, He(`interp_entry code generation failed: ${e3}`), Zs();
  } finally {
    const t3 = zs();
    if (a2 ? (da(11, a2 - s2), da(12, t3 - a2)) : da(11, t3 - s2), c2) {
      Ve(`// ${e2.length} trampolines generated, blob follows //`);
      let t4 = "", n3 = 0;
      try {
        r2.inSection && r2.endSection();
      } catch (e3) {
      }
      const o2 = r2.getArrayView(false, true);
      for (let e3 = 0; e3 < o2.length; e3++) {
        const r3 = o2[e3];
        r3 < 16 && (t4 += "0"), t4 += r3.toString(16), t4 += " ", t4.length % 10 == 0 && (Ve(`${n3}	${t4}`), t4 = "", n3 = e3 + 1);
      }
      Ve(`${n3}	${t4}`), Ve("// end blob //");
    } else i2 && !c2 && He("failed to generate trampoline for unknown reason");
  }
}
function Mi(e2, t2, n2, r2, s2) {
  const a2 = o.mono_jiterp_type_get_raw_value_size(n2), i2 = o.mono_jiterp_get_arg_offset(t2, 0, s2);
  switch (a2) {
    case 256:
      e2.local("sp_args"), e2.local(r2), e2.appendU8(54), e2.appendMemarg(i2, 2);
      break;
    case -1:
    case -2:
    case 1:
    case 2:
    case 4:
      switch (e2.local("sp_args"), e2.local(r2), a2) {
        case -1:
          e2.appendU8(45), e2.appendMemarg(0, 0);
          break;
        case 1:
          e2.appendU8(44), e2.appendMemarg(0, 0);
          break;
        case -2:
          e2.appendU8(47), e2.appendMemarg(0, 0);
          break;
        case 2:
          e2.appendU8(46), e2.appendMemarg(0, 0);
          break;
        case 4:
          e2.appendU8(40), e2.appendMemarg(0, 2);
      }
      e2.appendU8(54), e2.appendMemarg(i2, 2);
      break;
    default:
      e2.ptr_const(n2), e2.local("sp_args"), e2.i32_const(i2), e2.appendU8(106), e2.local(r2), e2.callImport("stackval_from_data");
  }
}
function zi(e2, t2) {
  const n2 = _($i);
  g(n2, $i), T(n2 + ea(13), t2.paramTypes.length + (t2.hasThisReference ? 1 : 0)), t2.hasThisReference && (e2.block(), e2.local("rmethod"), e2.i32_const(1), e2.appendU8(113), e2.appendU8(69), e2.appendU8(13), e2.appendULeb(0), e2.local("this_arg"), e2.callImport("unbox"), e2.local("this_arg", 33), e2.endBlock()), e2.ptr_const(n2), e2.local("scratchBuffer", 34), e2.local("rmethod"), e2.i32_const(-2), e2.appendU8(113), e2.appendU8(54), e2.appendMemarg(ea(6), 0), e2.local("scratchBuffer"), t2.hasThisReference ? e2.local("this_arg") : e2.i32_const(0), e2.callImport("interp_entry_prologue"), e2.local("sp_args", 33), t2.hasThisReference && Mi(e2, t2.imethod, 0, "this_arg", 0);
  for (let n3 = 0; n3 < t2.paramTypes.length; n3++) {
    const r2 = t2.paramTypes[n3];
    Mi(e2, t2.imethod, r2, `arg${n3}`, n3 + (t2.hasThisReference ? 1 : 0));
  }
  return e2.local("scratchBuffer"), t2.hasReturnValue ? e2.local("res") : e2.i32_const(0), e2.callImport("interp_entry"), e2.appendU8(15), true;
}
const Vi = 16, Wi = 0;
let Hi, qi, Gi = 0;
const Ji = [], Xi = {}, Qi = {};
class Yi {
  constructor(e2, t2, n2, r2, s2) {
    this.queue = [], r2 || mt(false, "Expected nonzero arg_offsets pointer"), this.method = e2, this.rmethod = t2, this.catchExceptions = s2, this.cinfo = n2, this.addr = P(n2 + 0), this.wrapper = P(n2 + 8), this.signature = P(n2 + 12), this.noWrapper = 0 !== N(n2 + 28), this.hasReturnValue = -1 !== F(n2 + 24), this.returnType = o.mono_jiterp_get_signature_return_type(this.signature), this.paramCount = o.mono_jiterp_get_signature_param_count(this.signature), this.hasThisReference = 0 !== o.mono_jiterp_get_signature_has_this(this.signature);
    const a2 = o.mono_jiterp_get_signature_params(this.signature);
    this.paramTypes = new Array(this.paramCount);
    for (let e3 = 0; e3 < this.paramCount; e3++) this.paramTypes[e3] = P(a2 + 4 * e3);
    const i2 = this.paramCount + (this.hasThisReference ? 1 : 0);
    this.argOffsets = new Array(this.paramCount);
    for (let e3 = 0; e3 < i2; e3++) this.argOffsets[e3] = P(r2 + 4 * e3);
    this.target = this.noWrapper ? this.addr : this.wrapper, this.result = 0, this.wasmNativeReturnType = this.returnType && this.hasReturnValue ? ec[o.mono_jiterp_type_to_stind(this.returnType)] : 64, this.wasmNativeSignature = this.paramTypes.map(((e3) => ec[o.mono_jiterp_type_to_ldind(e3)])), this.enableDirect = fa().directJitCalls && !this.noWrapper && this.wasmNativeReturnType && (0 === this.wasmNativeSignature.length || this.wasmNativeSignature.every(((e3) => e3))), this.enableDirect && (this.target = this.addr);
    let c2 = this.target.toString(16);
    const l2 = Gi++;
    this.name = `${this.enableDirect ? "jcp" : "jcw"}_${c2}_${l2.toString(16)}`;
  }
}
function Zi(e2) {
  let t2 = Ji[e2];
  return t2 || (e2 >= Ji.length && (Ji.length = e2 + 1), qi || (qi = qs()), Ji[e2] = t2 = qi.get(e2)), t2;
}
function Ki() {
  const e2 = [];
  let t2 = 0;
  for (; 0 != (t2 = o.mono_jiterp_tlqueue_next(0)); ) {
    const n3 = Qi[t2];
    if (n3) for (let t3 = 0; t3 < n3.length; t3++) 0 === n3[t3].result && e2.push(n3[t3]);
    else Ve(`Failed to find corresponding info list for method ptr ${t2} from jit queue!`);
  }
  if (!e2.length) return;
  let n2 = Hi;
  if (n2 ? n2.clear(0) : (Hi = n2 = new Cs(0), n2.defineType("trampoline", { ret_sp: 127, sp: 127, ftndesc: 127, thrown: 127 }, 64, true), n2.defineType("begin_catch", { ptr: 127 }, 64, true), n2.defineType("end_catch", {}, 64, true), n2.defineImportedFunction("i", "begin_catch", "begin_catch", true, ta("mono_jiterp_begin_catch")), n2.defineImportedFunction("i", "end_catch", "end_catch", true, ta("mono_jiterp_end_catch"))), n2.options.wasmBytesLimit <= ua(6)) return void o.mono_jiterp_tlqueue_clear(0);
  n2.options.enableWasmEh && (ct.featureWasmEh || (pa({ enableWasmEh: false }), n2.options.enableWasmEh = false));
  const r2 = zs();
  let s2 = 0, a2 = true, i2 = false;
  const c2 = [];
  try {
    qi || (qi = qs()), n2.appendU32(1836278016), n2.appendU32(1);
    for (let t4 = 0; t4 < e2.length; t4++) {
      const r4 = e2[t4], o2 = {};
      if (r4.enableDirect) {
        r4.hasThisReference && (o2.this = 127);
        for (let e3 = 0; e3 < r4.wasmNativeSignature.length; e3++) o2[`arg${e3}`] = r4.wasmNativeSignature[e3];
        o2.rgctx = 127;
      } else {
        const e3 = (r4.hasThisReference ? 1 : 0) + (r4.hasReturnValue ? 1 : 0) + r4.paramCount;
        for (let t5 = 0; t5 < e3; t5++) o2[`arg${t5}`] = 127;
        o2.ftndesc = 127;
      }
      n2.defineType(r4.name, o2, r4.enableDirect ? r4.wasmNativeReturnType : 64, false);
      const s3 = Zi(r4.target);
      "function" != typeof s3 && mt(false, `expected call target to be function but was ${s3}`), c2.push([r4.name, r4.name, s3]);
    }
    n2.generateTypeSection(), n2.compressImportNames = true;
    for (let e3 = 0; e3 < c2.length; e3++) n2.defineImportedFunction("i", c2[e3][0], c2[e3][1], false, c2[e3][2]);
    for (let e3 = 0; e3 < c2.length; e3++) n2.markImportAsUsed(c2[e3][0]);
    n2.markImportAsUsed("begin_catch"), n2.markImportAsUsed("end_catch"), n2._generateImportSection(false), n2.beginSection(3), n2.appendULeb(e2.length), n2.functionTypes.trampoline || mt(false, "func type missing");
    for (let t4 = 0; t4 < e2.length; t4++) n2.appendULeb(n2.functionTypes.trampoline[0]);
    n2.beginSection(7), n2.appendULeb(e2.length);
    for (let t4 = 0; t4 < e2.length; t4++) {
      const r4 = e2[t4];
      n2.appendName(r4.name), n2.appendU8(0), n2.appendULeb(n2.importedFunctionCount + t4);
    }
    n2.beginSection(10), n2.appendULeb(e2.length);
    for (let t4 = 0; t4 < e2.length; t4++) {
      const r4 = e2[t4];
      if (n2.beginFunction("trampoline", { old_sp: 127 }), !oc(n2, r4)) throw new Error(`Failed to generate ${r4.name}`);
      n2.appendU8(11), n2.endFunction(true);
    }
    n2.endSection(), s2 = zs();
    const t3 = n2.getArrayView();
    da(6, t3.length);
    const r3 = new WebAssembly.Module(t3), i3 = n2.getWasmImports(), l2 = new WebAssembly.Instance(r3, i3);
    for (let t4 = 0; t4 < e2.length; t4++) {
      const n3 = e2[t4], r4 = Gs(1, l2.exports[n3.name]);
      if (n3.result = r4, r4 > 0) {
        o.mono_jiterp_register_jit_call_thunk(n3.cinfo, r4);
        for (let e3 = 0; e3 < n3.queue.length; e3++) o.mono_jiterp_register_jit_call_thunk(n3.queue[e3], r4);
        n3.enableDirect && da(4, 1), da(3, 1);
      }
      n3.queue.length = 0, a2 = false;
    }
  } catch (e3) {
    i2 = true, a2 = false, He(`jit_call code generation failed: ${e3}`), Zs();
  } finally {
    const t3 = zs();
    if (s2 ? (da(11, s2 - r2), da(12, t3 - s2)) : da(11, t3 - r2), i2 || a2) for (let t4 = 0; t4 < e2.length; t4++) e2[t4].result = -1;
    if (i2) {
      Ve(`// ${e2.length} jit call wrappers generated, blob follows //`);
      for (let t5 = 0; t5 < e2.length; t5++) Ve(`// #${t5} === ${e2[t5].name} hasThis=${e2[t5].hasThisReference} hasRet=${e2[t5].hasReturnValue} wasmArgTypes=${e2[t5].wasmNativeSignature}`);
      let t4 = "", r3 = 0;
      try {
        n2.inSection && n2.endSection();
      } catch (e3) {
      }
      const o2 = n2.getArrayView(false, true);
      for (let e3 = 0; e3 < o2.length; e3++) {
        const n3 = o2[e3];
        n3 < 16 && (t4 += "0"), t4 += n3.toString(16), t4 += " ", t4.length % 10 == 0 && (Ve(`${r3}	${t4}`), t4 = "", r3 = e3 + 1);
      }
      Ve(`${r3}	${t4}`), Ve("// end blob //");
    } else a2 && !i2 && He("failed to generate trampoline for unknown reason");
  }
}
const ec = { 65535: 127, 70: 127, 71: 127, 72: 127, 73: 127, 74: 127, 75: 127, 76: 126, 77: 127, 78: 125, 79: 124, 80: 127, 81: 127, 82: 127, 83: 127, 84: 127, 85: 126, 86: 125, 87: 124, 223: 127 }, tc = { 70: 44, 71: 45, 72: 46, 73: 47, 74: 40, 75: 40, 76: 41, 77: 40, 78: 42, 79: 43, 80: 40, 81: 54, 82: 58, 83: 59, 84: 54, 85: 55, 86: 56, 87: 57, 223: 54 };
function nc(e2, t2, n2) {
  e2.local("sp"), e2.appendU8(n2), e2.appendMemarg(t2, 0);
}
function rc(e2, t2) {
  e2.local("sp"), e2.i32_const(t2), e2.appendU8(106);
}
function oc(e2, t2) {
  let n2 = 0;
  e2.options.enableWasmEh && e2.block(64, 6), t2.hasReturnValue && t2.enableDirect && e2.local("ret_sp"), t2.hasThisReference && (nc(e2, t2.argOffsets[0], 40), n2++), t2.hasReturnValue && !t2.enableDirect && e2.local("ret_sp");
  for (let r2 = 0; r2 < t2.paramCount; r2++) {
    const s2 = t2.argOffsets[n2 + r2];
    if (N(P(t2.cinfo + Vi) + r2) == Wi) nc(e2, s2, 40);
    else if (t2.enableDirect) {
      const n3 = o.mono_jiterp_type_to_ldind(t2.paramTypes[r2]);
      if (n3 || mt(false, `No load opcode for ${t2.paramTypes[r2]}`), 65535 === n3) rc(e2, s2);
      else {
        const o2 = tc[n3];
        if (!o2) return He(`No wasm load op for arg #${r2} type ${t2.paramTypes[r2]} cil opcode ${n3}`), false;
        nc(e2, s2, o2);
      }
    } else rc(e2, s2);
  }
  if (e2.local("ftndesc"), (t2.enableDirect || t2.noWrapper) && (e2.appendU8(40), e2.appendMemarg(4, 0)), e2.callImport(t2.name), t2.hasReturnValue && t2.enableDirect) {
    const n3 = o.mono_jiterp_type_to_stind(t2.returnType), r2 = tc[n3];
    if (!r2) return He(`No wasm store op for return type ${t2.returnType} cil opcode ${n3}`), false;
    e2.appendU8(r2), e2.appendMemarg(0, 0);
  }
  return e2.options.enableWasmEh && (e2.appendU8(7), e2.appendULeb(e2.getTypeIndex("__cpp_exception")), e2.callImport("begin_catch"), e2.callImport("end_catch"), e2.local("thrown"), e2.i32_const(1), e2.appendU8(54), e2.appendMemarg(0, 2), e2.endBlock()), e2.appendU8(15), true;
}
const sc = 30;
let ac, ic, cc = false;
const lc = [], pc = [];
class uc {
  constructor(e2) {
    this.name = e2, this.eip = 0;
  }
}
class dc {
  constructor(e2, t2, n2) {
    this.ip = e2, this.index = t2, this.isVerbose = !!n2;
  }
  get hitCount() {
    return o.mono_jiterp_get_trace_hit_count(this.index);
  }
}
const fc = {};
let _c = 1;
const mc = {}, hc = {}, gc = 4, bc = 16, yc = 8;
let wc, kc;
const Sc = ["asin", "acos", "atan", "asinh", "acosh", "atanh", "cos", "sin", "tan", "cosh", "sinh", "tanh", "exp", "log", "log2", "log10", "cbrt"], vc = ["fmod", "atan2", "pow"], Uc = ["asinf", "acosf", "atanf", "asinhf", "acoshf", "atanhf", "cosf", "sinf", "tanf", "coshf", "sinhf", "tanhf", "expf", "logf", "log2f", "log10f", "cbrtf"], Tc = ["fmodf", "atan2f", "powf"];
function Ec(e2, t2, n2) {
  if (o.mono_jiterp_trace_bailout(n2), 14 === n2) return e2;
  const r2 = hc[t2];
  if (!r2) return void He(`trace info not found for ${t2}`);
  let s2 = r2.bailoutCounts;
  s2 || (r2.bailoutCounts = s2 = {});
  const a2 = s2[n2];
  return s2[n2] = a2 ? a2 + 1 : 1, r2.bailoutCount ? r2.bailoutCount++ : r2.bailoutCount = 1, e2;
}
function xc() {
  if (kc) return kc;
  kc = [oa("bailout", Ec), oa("copy_ptr", ta("mono_wasm_copy_managed_pointer")), oa("entry", ta("mono_jiterp_increase_entry_count")), oa("value_copy", ta("mono_jiterp_value_copy")), oa("gettype", ta("mono_jiterp_gettype_ref")), oa("castv2", ta("mono_jiterp_cast_v2")), oa("hasparent", ta("mono_jiterp_has_parent_fast")), oa("imp_iface", ta("mono_jiterp_implements_interface")), oa("imp_iface_s", ta("mono_jiterp_implements_special_interface")), oa("box", ta("mono_jiterp_box_ref")), oa("localloc", ta("mono_jiterp_localloc")), ["ckovr_i4", "overflow_check_i4", ta("mono_jiterp_overflow_check_i4")], ["ckovr_u4", "overflow_check_i4", ta("mono_jiterp_overflow_check_u4")], oa("newobj_i", ta("mono_jiterp_try_newobj_inlined")), oa("newstr", ta("mono_jiterp_try_newstr")), oa("newarr", ta("mono_jiterp_try_newarr")), oa("ld_del_ptr", ta("mono_jiterp_ld_delegate_method_ptr")), oa("ldtsflda", ta("mono_jiterp_ldtsflda")), oa("conv", ta("mono_jiterp_conv")), oa("relop_fp", ta("mono_jiterp_relop_fp")), oa("safepoint", ta("mono_jiterp_do_safepoint")), oa("hashcode", ta("mono_jiterp_get_hashcode")), oa("try_hash", ta("mono_jiterp_try_get_hashcode")), oa("hascsize", ta("mono_jiterp_object_has_component_size")), oa("hasflag", ta("mono_jiterp_enum_hasflag")), oa("array_rank", ta("mono_jiterp_get_array_rank")), ["a_elesize", "array_rank", ta("mono_jiterp_get_array_element_size")], oa("stfld_o", ta("mono_jiterp_set_object_field")), ["stelemr_tc", "stelemr", ta("mono_jiterp_stelem_ref")], oa("fma", ta("fma")), oa("fmaf", ta("fmaf"))], pc.length > 0 && (kc.push(["trace_eip", "trace_eip", Ic]), kc.push(["trace_args", "trace_eip", Ac])), (ct.emscriptenBuildOptions.enableEventPipe || ct.emscriptenBuildOptions.enableDevToolsProfiler) && (kc.push(oa("prof_enter", ta("mono_jiterp_prof_enter"))), kc.push(oa("prof_samplepoint", ta("mono_jiterp_prof_samplepoint"))), kc.push(oa("prof_leave", ta("mono_jiterp_prof_leave"))));
  const e2 = (e3, t2) => {
    for (let n2 = 0; n2 < e3.length; n2++) {
      const r2 = e3[n2];
      kc.push([r2, t2, ta(r2)]);
    }
  };
  return e2(Uc, "mathop_f_f"), e2(Tc, "mathop_ff_f"), e2(Sc, "mathop_d_d"), e2(vc, "mathop_dd_d"), kc;
}
function Ic(e2, t2) {
  const n2 = fc[e2];
  if (!n2) throw new Error(`Unrecognized instrumented trace id ${e2}`);
  n2.eip = t2, ac = n2;
}
function Ac(e2, t2) {
  if (!ac) throw new Error("No trace active");
  ac.operand1 = e2 >>> 0, ac.operand2 = t2 >>> 0;
}
function jc(e2, t2, n2, r2) {
  if ("number" == typeof r2) o.mono_jiterp_adjust_abort_count(r2, 1), r2 = $s(r2);
  else {
    let e3 = mc[r2];
    "number" != typeof e3 ? e3 = 1 : e3++, mc[r2] = e3;
  }
  hc[e2].abortReason = r2;
}
function $c(e2) {
  if (!ct.runtimeReady) return;
  if (ic || (ic = fa()), !ic.enableStats) return;
  const t2 = ua(9), n2 = ua(10), r2 = ua(7), s2 = ua(8), a2 = ua(3), i2 = ua(4), c2 = ua(2), l2 = ua(1), p2 = ua(0), u2 = ua(6), d2 = ua(11), f2 = ua(12), _2 = ua(13), m2 = ua(14), h2 = t2 / (t2 + n2) * 100, g2 = o.mono_jiterp_get_rejected_trace_count(), b2 = ic.eliminateNullChecks ? r2.toString() : "off", y2 = ic.zeroPageOptimization ? s2.toString() + (aa() ? "" : " (disabled)") : "off", w2 = ic.enableBackwardBranches ? `emitted: ${t2}, failed: ${n2} (${h2.toFixed(1)}%)` : ": off", k2 = a2 ? ic.directJitCalls ? `direct jit calls: ${i2} (${(i2 / a2 * 100).toFixed(1)}%)` : "direct jit calls: off" : "";
  if (Ve(`// jitted ${u2}b; ${l2} traces (${(l2 / p2 * 100).toFixed(1)}%) (${g2} rejected); ${a2} jit_calls; ${c2} interp_entries`), Ve(`// cknulls pruned: ${b2}, fused: ${y2}; back-brs ${w2}; switch tgts ${_2}/${m2 + _2}; ${k2}`), Ve(`// time: ${0 | d2}ms generating, ${0 | f2}ms compiling wasm.`), !e2) {
    if (ic.countBailouts) {
      const e3 = Object.values(hc);
      e3.sort(((e4, t3) => (t3.bailoutCount || 0) - (e4.bailoutCount || 0)));
      for (let e4 = 0; e4 < ha.length; e4++) {
        const t3 = o.mono_jiterp_get_trace_bailout_count(e4);
        t3 && Ve(`// traces bailed out ${t3} time(s) due to ${ha[e4]}`);
      }
      for (let t3 = 0, n3 = 0; t3 < e3.length && n3 < sc; t3++) {
        const r3 = e3[t3];
        if (r3.bailoutCount) {
          n3++, Ve(`${r3.name}: ${r3.bailoutCount} bailout(s)`);
          for (const e4 in r3.bailoutCounts) Ve(`  ${ha[e4]} x${r3.bailoutCounts[e4]}`);
        }
      }
    }
    if (ic.estimateHeat) {
      const e3 = {}, t3 = Object.values(hc);
      for (let n4 = 0; n4 < t3.length; n4++) {
        const r3 = t3[n4];
        r3.abortReason && "end-of-body" !== r3.abortReason && (e3[r3.abortReason] ? e3[r3.abortReason] += r3.hitCount : e3[r3.abortReason] = r3.hitCount);
      }
      t3.sort(((e4, t4) => t4.hitCount - e4.hitCount)), Ve("// hottest failed traces:");
      for (let e4 = 0, n4 = 0; e4 < t3.length && n4 < sc; e4++) if (t3[e4].name && !(t3[e4].fnPtr || t3[e4].name.indexOf("Xunit.") >= 0)) {
        if (t3[e4].abortReason) {
          if (t3[e4].abortReason.startsWith("mono_icall_") || t3[e4].abortReason.startsWith("ret.")) continue;
          switch (t3[e4].abortReason) {
            case "trace-too-small":
            case "trace-too-big":
            case "call":
            case "callvirt.fast":
            case "calli.nat.fast":
            case "calli.nat":
            case "call.delegate":
            case "newobj":
            case "newobj_vt":
            case "newobj_slow":
            case "switch":
            case "rethrow":
            case "end-of-body":
            case "ret":
            case "intrins_marvin_block":
            case "intrins_ascii_chars_to_uppercase":
              continue;
          }
        }
        n4++, Ve(`${t3[e4].name} @${t3[e4].ip} (${t3[e4].hitCount} hits) ${t3[e4].abortReason}`);
      }
      const n3 = [];
      for (const t4 in e3) n3.push([t4, e3[t4]]);
      n3.sort(((e4, t4) => t4[1] - e4[1])), Ve("// heat:");
      for (let e4 = 0; e4 < n3.length; e4++) Ve(`// ${n3[e4][0]}: ${n3[e4][1]}`);
    } else {
      for (let e4 = 0; e4 < 691; e4++) {
        const t3 = $s(e4), n3 = o.mono_jiterp_adjust_abort_count(e4, 0);
        n3 > 0 ? mc[t3] = n3 : delete mc[t3];
      }
      const e3 = Object.keys(mc);
      e3.sort(((e4, t3) => mc[t3] - mc[e4]));
      for (let t3 = 0; t3 < e3.length; t3++) Ve(`// ${e3[t3]}: ${mc[e3[t3]]} abort(s)`);
    }
    for (const e3 in Ms) Ve(`// simd ${e3}: ${Ms[e3]} fallback insn(s)`);
  }
}
const Lc = "https://dotnet.generated.invalid/interp_pgo";
async function Rc() {
  if (!lt.is_runtime_running()) return void Ve("Skipped saving interp_pgo table (already exited)");
  const e2 = await Oc(Lc);
  if (e2) try {
    const t2 = o.mono_interp_pgo_save_table(0, 0);
    if (t2 <= 0) return void Ve("Failed to save interp_pgo table (No data to save)");
    const r2 = _(t2);
    if (0 !== o.mono_interp_pgo_save_table(r2, t2)) return void He("Failed to save interp_pgo table (Unknown error)");
    const s2 = K().slice(r2, r2 + t2);
    await (async function(e3, t3, r3) {
      try {
        const r4 = await Nc();
        if (!r4) return false;
        const o2 = n ? new Uint8Array(t3).slice(0) : t3, s3 = new Response(o2, { headers: { "content-type": "application/octet-stream", "content-length": t3.byteLength.toString() } });
        return await r4.put(e3, s3), true;
      } catch (t4) {
        return We("Failed to store entry to the cache: " + e3, t4), false;
      }
    })(e2, s2) && Ve("Saved interp_pgo table to cache"), (async function(e3, t3) {
      try {
        const n2 = await Nc();
        if (!n2) return;
        const r3 = await n2.keys();
        for (const o2 of r3) o2.url && o2.url !== t3 && o2.url.startsWith(e3) && await n2.delete(o2);
      } catch (e4) {
        return;
      }
    })(Lc, e2), m(r2);
  } catch (e3) {
    He(`Failed to save interp_pgo table: ${e3}`);
  }
  else He("Failed to save interp_pgo table (No cache key)");
}
async function Bc() {
  const e2 = await Oc(Lc);
  if (!e2) return void He("Failed to create cache key for interp_pgo table");
  const t2 = await (async function(e3) {
    try {
      const t3 = await Nc();
      if (!t3) return;
      const n3 = await t3.match(e3);
      if (!n3) return;
      return n3.arrayBuffer();
    } catch (t3) {
      return void We("Failed to load entry from the cache: " + e3, t3);
    }
  })(e2);
  if (!t2) return void Ve("Failed to load interp_pgo table (No table found in cache)");
  const n2 = _(t2.byteLength);
  K().set(new Uint8Array(t2), n2), o.mono_interp_pgo_load_table(n2, t2.byteLength) && He("Failed to load interp_pgo table (Unknown error)"), m(n2);
}
async function Nc() {
  if (st && false === globalThis.window.isSecureContext) return We("Failed to open the cache, running on an insecure origin"), null;
  if (void 0 === globalThis.caches) return We("Failed to open the cache, probably running on an insecure origin"), null;
  const e2 = `dotnet-resources${document.baseURI.substring(document.location.origin.length)}`;
  try {
    return await globalThis.caches.open(e2) || null;
  } catch (e3) {
    return We("Failed to open cache"), null;
  }
}
async function Oc(t2) {
  if (!ct.subtle) return null;
  const n2 = Object.assign({}, ct.config);
  n2.resourcesHash = n2.resources.hash, delete n2.assets, delete n2.resources, n2.preferredIcuAsset = lt.preferredIcuAsset, delete n2.forwardConsoleLogsToWS, delete n2.diagnosticTracing, delete n2.appendElementOnExit, delete n2.interopCleanupOnExit, delete n2.dumpThreadsOnNonZeroExit, delete n2.logExitCode, delete n2.pthreadPoolInitialSize, delete n2.pthreadPoolUnusedSize, delete n2.asyncFlushOnExit, delete n2.remoteSources, delete n2.ignorePdbLoadErrors, delete n2.maxParallelDownloads, delete n2.enableDownloadRetry, delete n2.extensions, delete n2.runtimeId, delete n2.jsThreadBlockingMode, n2.GitHash = lt.gitHash, n2.ProductVersion = e;
  const r2 = JSON.stringify(n2), o2 = await ct.subtle.digest("SHA-256", new TextEncoder().encode(r2)), s2 = new Uint8Array(o2);
  return `${t2}-${Array.from(s2).map(((e2) => e2.toString(16).padStart(2, "0"))).join("")}`;
}
async function Cc(e2) {
  const t2 = lt.config.resources.lazyAssembly;
  if (!t2) throw new Error("No assemblies have been marked as lazy-loadable. Use the 'BlazorWebAssemblyLazyLoad' item group in your project file to enable lazy loading an assembly.");
  let n2 = e2;
  e2.endsWith(".dll") ? n2 = e2.substring(0, e2.length - 4) : e2.endsWith(".wasm") && (n2 = e2.substring(0, e2.length - 5));
  const r2 = n2 + ".dll", o2 = n2 + ".wasm";
  let s2 = null;
  for (let e3 = 0; e3 < t2.length; e3++) {
    const n3 = t2[e3];
    if (n3.virtualPath === r2 || n3.virtualPath === o2) {
      s2 = n3, s2.behavior = "assembly";
      break;
    }
  }
  if (null == s2) throw new Error(`${e2} must be marked with 'BlazorWebAssemblyLazyLoad' item group in your project file to allow lazy-loading.`);
  if (lt.loadedAssemblies.includes(s2.name)) return false;
  const a2 = n2 + ".pdb";
  let i2 = false, c2 = null;
  if (0 != lt.config.debugLevel && lt.isDebuggingSupported()) {
    for (let e3 = 0; e3 < t2.length; e3++) if (t2[e3].virtualPath === a2) {
      i2 = true, c2 = t2[e3], c2.behavior = "pdb";
      break;
    }
  }
  const l2 = lt.retrieve_asset_download(s2);
  let p2 = null, u2 = null;
  if (i2) {
    const e3 = null != c2 ? lt.retrieve_asset_download(c2) : Promise.resolve(null), [t3, n3] = await Promise.all([l2, e3]);
    p2 = new Uint8Array(t3), u2 = n3 ? new Uint8Array(n3) : null;
  } else {
    const e3 = await l2;
    p2 = new Uint8Array(e3), u2 = null;
  }
  return (function(e3, t3) {
    lt.assert_runtime_running();
    const n3 = Ke.stackSave();
    try {
      const n4 = $n(4), r3 = Ln(n4, 2), o3 = Ln(n4, 3);
      Wn(r3, 21), Wn(o3, 21), vo(r3, e3, 4), vo(o3, t3, 4), kn(yn.LoadLazyAssembly, n4);
    } finally {
      lt.is_runtime_running() && Ke.stackRestore(n3);
    }
  })(p2, u2), true;
}
async function Dc(e2) {
  const t2 = lt.config.resources.satelliteResources;
  t2 && await Promise.all(e2.filter(((e3) => Object.prototype.hasOwnProperty.call(t2, e3))).map(((e3) => {
    const n2 = [];
    for (let r2 = 0; r2 < t2[e3].length; r2++) {
      const o2 = t2[e3][r2];
      o2.behavior = "resource", o2.culture = e3, n2.push(lt.retrieve_asset_download(o2));
    }
    return n2;
  })).reduce(((e3, t3) => e3.concat(t3)), new Array()).map((async (e3) => {
    const t3 = await e3;
    !(function(e4) {
      lt.assert_runtime_running();
      const t4 = Ke.stackSave();
      try {
        const t5 = $n(3), n2 = Ln(t5, 2);
        Wn(n2, 21), vo(n2, e4, 4), kn(yn.LoadSatelliteAssembly, t5);
      } finally {
        lt.is_runtime_running() && Ke.stackRestore(t4);
      }
    })(new Uint8Array(t3));
  })));
}
function Fc(e2) {
  if (e2 === l) return null;
  const t2 = o.mono_wasm_read_as_bool_or_null_unsafe(e2);
  return 0 !== t2 && (1 === t2 || null);
}
function Pc(e2) {
  if (e2) try {
    (e2 = e2.toLocaleLowerCase().replace("_", "-")).startsWith("zh-") && (e2 = e2.replace("-chs", "-Hans").replace("-cht", "-Hant"));
    const t2 = Intl.getCanonicalLocales(e2);
    return t2.length > 0 ? t2[0] : void 0;
  } catch (e3) {
    return;
  }
}
const Mc = [function(e2) {
  Xo && (globalThis.clearTimeout(Xo), Xo = void 0), Xo = Ke.safeSetTimeout(mono_wasm_schedule_timer_tick, e2);
}, function(e2, t2, n2, r2, o2) {
  if (true !== ct.mono_wasm_runtime_is_ready) return;
  const s2 = K(), a2 = 0 !== e2 ? Le(e2).concat(".dll") : "", i2 = ht(new Uint8Array(s2.buffer, t2, n2));
  let c2;
  r2 && (c2 = ht(new Uint8Array(s2.buffer, r2, o2))), Lt({ eventName: "AssemblyLoaded", assembly_name: a2, assembly_b64: i2, pdb_b64: c2 });
}, function(e2, t2) {
  const n2 = Le(t2);
  et.logging && "function" == typeof et.logging.debugger && et.logging.debugger(e2, n2);
}, function(e2, t2, n2, r2) {
  const o2 = { res_ok: e2, res: { id: t2, value: ht(new Uint8Array(K().buffer, n2, r2)) } };
  bt.has(t2) && We(`Adding an id (${t2}) that already exists in commands_received`), bt.set(t2, o2);
}, function mono_wasm_fire_debugger_agent_message_with_data(e2, t2) {
  mono_wasm_fire_debugger_agent_message_with_data_to_pause(ht(new Uint8Array(K().buffer, e2, t2)));
}, mono_wasm_fire_debugger_agent_message_with_data_to_pause, function() {
  Jo || (Jo = Ke.safeSetTimeout(Ko, 0));
}, function(e2, t2, n2, r2, s2, a2, i2, c2) {
  if (n2 || mt(false, "expected instruction pointer"), ic || (ic = fa()), !ic.enableTraces) return 1;
  if (ic.wasmBytesLimit <= ua(6)) return 1;
  if (cc) return 1;
  let l2, p2 = hc[r2];
  if (p2 || (hc[r2] = p2 = new dc(n2, r2, i2)), da(0, 1), ic.estimateHeat || pc.length > 0 || p2.isVerbose) {
    const e3 = o.mono_wasm_method_get_full_name(t2);
    l2 = Le(e3), m(e3);
  }
  const u2 = Le(o.mono_wasm_method_get_name(t2));
  p2.name = l2 || u2;
  let d2 = ic.noExitBackwardBranches ? (function(e3, t3, n3) {
    const r3 = t3 + n3, o2 = [], s3 = (e3 - t3) / 2;
    for (; e3 < r3; ) {
      const n4 = (e3 - t3) / 2, r4 = O(e3), a3 = Ja(e3, r4);
      if (271 === r4) ;
      else {
        const t4 = gi(e3, r4);
        if ("number" != typeof t4) {
          e3 += 2 * a3;
          continue;
        }
        if (0 === t4) {
          Ve(`opcode @${e3} branch target is self. aborting backbranch table generation`);
          break;
        }
        if (t4 < 0) {
          const r5 = n4 + t4;
          if (r5 < 0) {
            Ve(`opcode @${e3}'s displacement of ${t4} goes before body: ${r5}. aborting backbranch table generation`);
            break;
          }
          r5 >= s3 && o2.push(r5);
        }
        switch (r4) {
          case 132:
          case 133:
            o2.push(n4 + a3);
        }
      }
      e3 += 2 * a3;
    }
    return o2.length <= 0 ? null : new Uint16Array(o2);
  })(n2, s2, a2) : null;
  if (d2 && n2 !== s2) {
    const e3 = (n2 - s2) / 2;
    let t3 = false;
    for (let n3 = 0; n3 < d2.length; n3++) if (d2[n3] >= e3) {
      t3 = true;
      break;
    }
    t3 || (d2 = null);
  }
  const f2 = (function(e3, t3, n3, r3, s3, a3, i3, c3, l3) {
    let p3 = wc;
    p3 ? p3.clear(8) : (wc = p3 = new Cs(8), (function(e4) {
      e4.defineType("trace", { frame: 127, pLocals: 127, cinfo: 127, ip: 127 }, 127, true), e4.defineType("bailout", { retval: 127, base: 127, reason: 127 }, 127, true), e4.defineType("copy_ptr", { dest: 127, src: 127 }, 64, true), e4.defineType("value_copy", { dest: 127, src: 127, klass: 127 }, 64, true), e4.defineType("entry", { imethod: 127 }, 127, true), e4.defineType("strlen", { ppString: 127, pResult: 127 }, 127, true), e4.defineType("getchr", { ppString: 127, pIndex: 127, pResult: 127 }, 127, true), e4.defineType("getspan", { destination: 127, span: 127, index: 127, element_size: 127 }, 127, true), e4.defineType("overflow_check_i4", { lhs: 127, rhs: 127, opcode: 127 }, 127, true), e4.defineType("mathop_d_d", { value: 124 }, 124, true), e4.defineType("mathop_dd_d", { lhs: 124, rhs: 124 }, 124, true), e4.defineType("mathop_f_f", { value: 125 }, 125, true), e4.defineType("mathop_ff_f", { lhs: 125, rhs: 125 }, 125, true), e4.defineType("fmaf", { x: 125, y: 125, z: 125 }, 125, true), e4.defineType("fma", { x: 124, y: 124, z: 124 }, 124, true), e4.defineType("trace_eip", { traceId: 127, eip: 127 }, 64, true), e4.defineType("newobj_i", { ppDestination: 127, vtable: 127 }, 127, true), e4.defineType("newstr", { ppDestination: 127, length: 127 }, 127, true), e4.defineType("newarr", { ppDestination: 127, vtable: 127, length: 127 }, 127, true), e4.defineType("localloc", { destination: 127, len: 127, frame: 127 }, 64, true), e4.defineType("ld_del_ptr", { ppDestination: 127, ppSource: 127 }, 64, true), e4.defineType("ldtsflda", { ppDestination: 127, offset: 127 }, 64, true), e4.defineType("gettype", { destination: 127, source: 127 }, 127, true), e4.defineType("castv2", { destination: 127, source: 127, klass: 127, opcode: 127 }, 127, true), e4.defineType("hasparent", { klass: 127, parent: 127 }, 127, true), e4.defineType("imp_iface", { vtable: 127, klass: 127 }, 127, true), e4.defineType("imp_iface_s", { obj: 127, vtable: 127, klass: 127 }, 127, true), e4.defineType("box", { vtable: 127, destination: 127, source: 127, vt: 127 }, 64, true), e4.defineType("conv", { destination: 127, source: 127, opcode: 127 }, 127, true), e4.defineType("relop_fp", { lhs: 124, rhs: 124, opcode: 127 }, 127, true), e4.defineType("safepoint", { frame: 127, ip: 127 }, 64, true), e4.defineType("prof_enter", { frame: 127, ip: 127 }, 64, true), e4.defineType("prof_samplepoint", { frame: 127, ip: 127 }, 64, true), e4.defineType("prof_leave", { frame: 127, ip: 127 }, 64, true), e4.defineType("hashcode", { ppObj: 127 }, 127, true), e4.defineType("try_hash", { ppObj: 127 }, 127, true), e4.defineType("hascsize", { ppObj: 127 }, 127, true), e4.defineType("hasflag", { klass: 127, dest: 127, sp1: 127, sp2: 127 }, 64, true), e4.defineType("array_rank", { destination: 127, source: 127 }, 127, true), e4.defineType("stfld_o", { locals: 127, fieldOffsetBytes: 127, targetLocalOffsetBytes: 127, sourceLocalOffsetBytes: 127 }, 127, true), e4.defineType("notnull", { ptr: 127, expected: 127, traceIp: 127, ip: 127 }, 64, true), e4.defineType("stelemr", { o: 127, aindex: 127, ref: 127 }, 127, true), e4.defineType("simd_p_p", { arg0: 127, arg1: 127 }, 64, true), e4.defineType("simd_p_pp", { arg0: 127, arg1: 127, arg2: 127 }, 64, true), e4.defineType("simd_p_ppp", { arg0: 127, arg1: 127, arg2: 127, arg3: 127 }, 64, true);
      const t4 = xc();
      for (let n4 = 0; n4 < t4.length; n4++) t4[n4] || mt(false, `trace #${n4} missing`), e4.defineImportedFunction("i", t4[n4][0], t4[n4][1], true, t4[n4][2]);
    })(p3)), ic = p3.options;
    const u3 = r3 + s3, d3 = `${t3}:${(n3 - r3).toString(16)}`, f3 = zs();
    let _2 = 0, m2 = true, h2 = false;
    const g2 = hc[a3], b2 = g2.isVerbose || i3 && pc.findIndex(((e4) => i3.indexOf(e4) >= 0)) >= 0;
    b2 && !i3 && mt(false, "Expected methodFullName if trace is instrumented");
    const y2 = b2 ? _c++ : 0;
    b2 && (Ve(`instrumenting: ${i3}`), fc[y2] = new uc(i3)), p3.compressImportNames = !b2;
    try {
      p3.appendU32(1836278016), p3.appendU32(1), p3.generateTypeSection();
      const t4 = { disp: 127, cknull_ptr: 127, dest_ptr: 127, src_ptr: 127, memop_dest: 127, memop_src: 127, index: 127, count: 127, math_lhs32: 127, math_rhs32: 127, math_lhs64: 126, math_rhs64: 126, temp_f32: 125, temp_f64: 124 };
      p3.options.enableSimd && (t4.v128_zero = 123, t4.math_lhs128 = 123, t4.math_rhs128 = 123);
      let s4 = true, i4 = 0;
      if (p3.defineFunction({ type: "trace", name: d3, export: true, locals: t4 }, (() => {
        switch (p3.base = n3, p3.traceIndex = a3, p3.frame = e3, O(n3)) {
          case 674:
          case 675:
          case 677:
          case 676:
            break;
          default:
            throw new Error(`Expected *ip to be a jiterpreter opcode but it was ${O(n3)}`);
        }
        return p3.cfg.initialize(r3, c3, b2 ? 1 : 0), i4 = (function(e4, t5, n4, r4, s5, a4, i5, c4) {
          let l4 = true, p4 = false, u4 = false, d4 = false, f5 = 0, _3 = 0, m3 = 0;
          Ya(), a4.backBranchTraceLevel = i5 ? 2 : 0;
          let h4 = a4.cfg.entry(n4);
          for (; n4 && n4; ) {
            if (a4.cfg.ip = n4, n4 >= s5) {
              jc(a4.traceIndex, 0, 0, "end-of-body"), i5 && Ve(`instrumented trace ${t5} exited at end of body @${n4.toString(16)}`);
              break;
            }
            const g3 = a4.options.maxModuleSize - 300 - a4.bytesGeneratedSoFar - a4.cfg.overheadBytes;
            if (a4.size >= g3) {
              jc(a4.traceIndex, 0, 0, "trace-too-big"), i5 && Ve(`instrumented trace ${t5} exited because of size limit at @${n4.toString(16)} (spaceLeft=${g3}b)`);
              break;
            }
            let b3 = O(n4);
            const y3 = o.mono_jiterp_get_opcode_info(b3, 2), w3 = o.mono_jiterp_get_opcode_info(b3, 3), k3 = Ja(n4, b3), S3 = b3 >= 657 && b3 <= 659, v3 = S3 ? b3 - 657 + 2 : 0, U2 = S3 ? Ca(n4, 1 + v3) : 0;
            b3 >= 0 && b3 < 691 || mt(false, `invalid opcode ${b3}`);
            const T2 = S3 ? ga[v3][U2] : $s(b3), E2 = n4, x2 = a4.options.noExitBackwardBranches && Wa(n4, r4, c4), I2 = a4.branchTargets.has(n4), A2 = x2 || I2 || l4 && c4, j2 = m3 + _3 + a4.branchTargets.size;
            let $2 = false, L2 = ra(b3);
            switch (x2 && (a4.backBranchTraceLevel > 1 && Ve(`${t5} recording back branch target 0x${n4.toString(16)}`), a4.backBranchOffsets.push(n4)), A2 && (u4 = false, d4 = false, ei(a4, n4, x2), p4 = true, Ya(), m3 = 0), L2 < -1 && p4 && (L2 = -2 === L2 ? 2 : 0), l4 = false, 271 === b3 || (lc.indexOf(b3) >= 0 ? (Ws(a4, n4, 23), b3 = 678) : u4 && (b3 = 678)), b3) {
              case 271:
                ji(a4, n4, j2) || (n4 = 0);
                break;
              case 678:
                u4 && (d4 || a4.appendU8(0), d4 = true);
                break;
              case 313:
              case 314:
                ai(a4, Ca(n4, 1), 0, Ca(n4, 2));
                break;
              case 312:
                si(a4, Ca(n4, 1), 0), ri(a4, Ca(n4, 2), 40), a4.local("frame"), a4.callImport("localloc");
                break;
              case 285:
                ri(a4, Ca(n4, 1), 40), a4.i32_const(0), ri(a4, Ca(n4, 2), 40), a4.appendU8(252), a4.appendU8(11), a4.appendU8(0);
                break;
              case 286:
                ri(a4, Ca(n4, 1), 40), Xs(a4, 0, Ca(n4, 2));
                break;
              case 310: {
                const e5 = Ca(n4, 3), t6 = Ca(n4, 2), r5 = Ca(n4, 1), o2 = Ga(a4, e5);
                0 !== o2 && ("number" != typeof o2 ? (ri(a4, e5, 40), a4.local("count", 34), a4.block(64, 4)) : (a4.i32_const(o2), a4.local("count", 33)), ri(a4, r5, 40), a4.local("dest_ptr", 34), a4.appendU8(69), ri(a4, t6, 40), a4.local("src_ptr", 34), a4.appendU8(69), a4.appendU8(114), a4.block(64, 4), Ws(a4, n4, 2), a4.endBlock(), "number" == typeof o2 && Qs(a4, 0, 0, o2, false, "dest_ptr", "src_ptr") || (a4.local("dest_ptr"), a4.local("src_ptr"), a4.local("count"), a4.appendU8(252), a4.appendU8(10), a4.appendU8(0), a4.appendU8(0)), "number" != typeof o2 && a4.endBlock());
                break;
              }
              case 311: {
                const e5 = Ca(n4, 3), t6 = Ca(n4, 2);
                li(a4, Ca(n4, 1), n4, true), ri(a4, t6, 40), ri(a4, e5, 40), a4.appendU8(252), a4.appendU8(11), a4.appendU8(0);
                break;
              }
              case 143:
              case 145:
              case 227:
              case 229:
              case 144:
              case 146:
              case 129:
              case 132:
              case 133:
                bi(a4, n4, e4, b3) ? p4 = true : n4 = 0;
                break;
              case 538: {
                const e5 = Ca(n4, 2), t6 = Ca(n4, 1);
                e5 !== t6 ? (a4.local("pLocals"), li(a4, e5, n4, true), oi(a4, t6, 54)) : li(a4, e5, n4, false), a4.allowNullCheckOptimization && Xa.set(t6, n4), $2 = true;
                break;
              }
              case 638:
              case 639: {
                const t6 = P(e4 + ea(4));
                a4.ptr_const(t6), a4.callImport("entry"), a4.block(64, 4), Ws(a4, n4, 1), a4.endBlock();
                break;
              }
              case 676:
                L2 = 0;
                break;
              case 138:
                break;
              case 86: {
                a4.local("pLocals");
                const e5 = Ca(n4, 2), r5 = ci(a4, e5), o2 = Ca(n4, 1), s6 = Ca(n4, 3);
                r5 || He(`${t5}: Expected local ${e5} to have address taken flag`), si(a4, e5, s6), oi(a4, o2, 54), Ha.set(o2, { type: "ldloca", offset: e5 }), $2 = true;
                break;
              }
              case 272:
              case 300:
              case 301:
              case 556: {
                a4.local("pLocals");
                let t6 = za(e4, Ca(n4, 2));
                300 === b3 && (t6 = o.mono_jiterp_imethod_to_ftnptr(t6)), a4.ptr_const(t6), oi(a4, Ca(n4, 1), 54);
                break;
              }
              case 305: {
                const t6 = za(e4, Ca(n4, 3));
                ri(a4, Ca(n4, 1), 40), ri(a4, Ca(n4, 2), 40), a4.ptr_const(t6), a4.callImport("value_copy");
                break;
              }
              case 306: {
                const e5 = Ca(n4, 3);
                ri(a4, Ca(n4, 1), 40), ri(a4, Ca(n4, 2), 40), Ys(a4, e5);
                break;
              }
              case 307: {
                const e5 = Ca(n4, 3);
                si(a4, Ca(n4, 1), e5), li(a4, Ca(n4, 2), n4, true), Ys(a4, e5);
                break;
              }
              case 308: {
                const t6 = za(e4, Ca(n4, 3));
                ri(a4, Ca(n4, 1), 40), si(a4, Ca(n4, 2), 0), a4.ptr_const(t6), a4.callImport("value_copy");
                break;
              }
              case 309: {
                const e5 = Ca(n4, 3);
                ri(a4, Ca(n4, 1), 40), si(a4, Ca(n4, 2), 0), Ys(a4, e5);
                break;
              }
              case 540:
                a4.local("pLocals"), li(a4, Ca(n4, 2), n4, true), a4.appendU8(40), a4.appendMemarg(ea(2), 2), oi(a4, Ca(n4, 1), 54);
                break;
              case 539: {
                a4.block(), ri(a4, Ca(n4, 3), 40), a4.local("index", 34);
                let e5 = "cknull_ptr";
                a4.options.zeroPageOptimization && aa() ? (da(8, 1), ri(a4, Ca(n4, 2), 40), e5 = "src_ptr", a4.local(e5, 34)) : li(a4, Ca(n4, 2), n4, true), a4.appendU8(40), a4.appendMemarg(ea(2), 2), a4.appendU8(72), a4.local("index"), a4.i32_const(0), a4.appendU8(78), a4.appendU8(113), a4.appendU8(13), a4.appendULeb(0), Ws(a4, n4, 11), a4.endBlock(), a4.local("pLocals"), a4.local("index"), a4.i32_const(2), a4.appendU8(108), a4.local(e5), a4.appendU8(106), a4.appendU8(47), a4.appendMemarg(ea(3), 1), oi(a4, Ca(n4, 1), 54);
                break;
              }
              case 342:
              case 343: {
                const e5 = Da(n4, 4);
                a4.block(), ri(a4, Ca(n4, 3), 40), a4.local("index", 34);
                let t6 = "cknull_ptr";
                342 === b3 ? li(a4, Ca(n4, 2), n4, true) : (si(a4, Ca(n4, 2), 0), t6 = "src_ptr", a4.local(t6, 34)), a4.appendU8(40), a4.appendMemarg(ea(7), 2), a4.appendU8(73), a4.local("index"), a4.i32_const(0), a4.appendU8(78), a4.appendU8(113), a4.appendU8(13), a4.appendULeb(0), Ws(a4, n4, 18), a4.endBlock(), a4.local("pLocals"), a4.local(t6), a4.appendU8(40), a4.appendMemarg(ea(8), 2), a4.local("index"), a4.i32_const(e5), a4.appendU8(108), a4.appendU8(106), oi(a4, Ca(n4, 1), 54);
                break;
              }
              case 664:
                a4.block(), ri(a4, Ca(n4, 3), 40), a4.local("count", 34), a4.i32_const(0), a4.appendU8(78), a4.appendU8(13), a4.appendULeb(0), Ws(a4, n4, 18), a4.endBlock(), si(a4, Ca(n4, 1), 16), a4.local("dest_ptr", 34), ri(a4, Ca(n4, 2), 40), a4.appendU8(54), a4.appendMemarg(0, 0), a4.local("dest_ptr"), a4.local("count"), a4.appendU8(54), a4.appendMemarg(4, 0);
                break;
              case 577:
                si(a4, Ca(n4, 1), 4), si(a4, Ca(n4, 2), 4), a4.callImport("ld_del_ptr");
                break;
              case 73:
                si(a4, Ca(n4, 1), 4), a4.ptr_const(Pa(n4, 2)), a4.callImport("ldtsflda");
                break;
              case 663:
                a4.block(), si(a4, Ca(n4, 1), 4), si(a4, Ca(n4, 2), 0), a4.callImport("gettype"), a4.appendU8(13), a4.appendULeb(0), Ws(a4, n4, 2), a4.endBlock();
                break;
              case 660: {
                const t6 = za(e4, Ca(n4, 4));
                a4.ptr_const(t6), si(a4, Ca(n4, 1), 4), si(a4, Ca(n4, 2), 0), si(a4, Ca(n4, 3), 0), a4.callImport("hasflag");
                break;
              }
              case 669: {
                const e5 = ea(1);
                a4.local("pLocals"), li(a4, Ca(n4, 2), n4, true), a4.i32_const(e5), a4.appendU8(106), oi(a4, Ca(n4, 1), 54);
                break;
              }
              case 661:
                a4.local("pLocals"), si(a4, Ca(n4, 2), 0), a4.callImport("hashcode"), oi(a4, Ca(n4, 1), 54);
                break;
              case 662:
                a4.local("pLocals"), si(a4, Ca(n4, 2), 0), a4.callImport("try_hash"), oi(a4, Ca(n4, 1), 54);
                break;
              case 665:
                a4.local("pLocals"), si(a4, Ca(n4, 2), 0), a4.callImport("hascsize"), oi(a4, Ca(n4, 1), 54);
                break;
              case 670:
                a4.local("pLocals"), ri(a4, Ca(n4, 2), 40), a4.local("math_lhs32", 34), ri(a4, Ca(n4, 3), 40), a4.appendU8(115), a4.i32_const(2), a4.appendU8(116), a4.local("math_rhs32", 33), a4.local("math_lhs32"), a4.i32_const(327685), a4.appendU8(106), a4.i32_const(10485920), a4.appendU8(114), a4.i32_const(1703962), a4.appendU8(106), a4.i32_const(-8388737), a4.appendU8(114), a4.local("math_rhs32"), a4.appendU8(113), a4.appendU8(69), oi(a4, Ca(n4, 1), 54);
                break;
              case 541:
              case 542:
                a4.block(), si(a4, Ca(n4, 1), 4), si(a4, Ca(n4, 2), 0), a4.callImport(541 === b3 ? "array_rank" : "a_elesize"), a4.appendU8(13), a4.appendULeb(0), Ws(a4, n4, 2), a4.endBlock();
                break;
              case 289:
              case 290: {
                const t6 = za(e4, Ca(n4, 3)), r5 = o.mono_jiterp_is_special_interface(t6), s6 = 289 === b3, i6 = Ca(n4, 1);
                if (!t6) {
                  jc(a4.traceIndex, 0, 0, "null-klass"), n4 = 0;
                  continue;
                }
                a4.block(), a4.options.zeroPageOptimization && aa() ? (ri(a4, Ca(n4, 2), 40), a4.local("dest_ptr", 34), da(8, 1)) : (a4.block(), ri(a4, Ca(n4, 2), 40), a4.local("dest_ptr", 34), a4.appendU8(13), a4.appendULeb(0), a4.local("pLocals"), a4.i32_const(0), oi(a4, i6, 54), a4.appendU8(12), a4.appendULeb(1), a4.endBlock(), a4.local("dest_ptr")), r5 && a4.local("dest_ptr"), a4.appendU8(40), a4.appendMemarg(ea(14), 0), a4.ptr_const(t6), a4.callImport(r5 ? "imp_iface_s" : "imp_iface"), s6 && (a4.local("dest_ptr"), a4.appendU8(69), a4.appendU8(114)), a4.block(64, 4), a4.local("pLocals"), a4.local("dest_ptr"), oi(a4, i6, 54), a4.appendU8(5), s6 ? Ws(a4, n4, 19) : (a4.local("pLocals"), a4.i32_const(0), oi(a4, i6, 54)), a4.endBlock(), a4.endBlock();
                break;
              }
              case 291:
              case 292:
              case 287:
              case 288: {
                const t6 = za(e4, Ca(n4, 3)), r5 = 291 === b3 || 292 === b3, o2 = 287 === b3 || 291 === b3, s6 = Ca(n4, 1);
                if (!t6) {
                  jc(a4.traceIndex, 0, 0, "null-klass"), n4 = 0;
                  continue;
                }
                a4.block(), a4.options.zeroPageOptimization && aa() ? (ri(a4, Ca(n4, 2), 40), a4.local("dest_ptr", 34), da(8, 1)) : (a4.block(), ri(a4, Ca(n4, 2), 40), a4.local("dest_ptr", 34), a4.appendU8(13), a4.appendULeb(0), a4.local("pLocals"), a4.i32_const(0), oi(a4, s6, 54), a4.appendU8(12), a4.appendULeb(1), a4.endBlock(), a4.local("dest_ptr")), a4.appendU8(40), a4.appendMemarg(ea(14), 0), a4.appendU8(40), a4.appendMemarg(ea(15), 0), r5 && a4.local("src_ptr", 34), a4.i32_const(t6), a4.appendU8(70), a4.block(64, 4), a4.local("pLocals"), a4.local("dest_ptr"), oi(a4, s6, 54), a4.appendU8(5), r5 ? (a4.local("src_ptr"), a4.ptr_const(t6), a4.callImport("hasparent"), o2 && (a4.local("dest_ptr"), a4.appendU8(69), a4.appendU8(114)), a4.block(64, 4), a4.local("pLocals"), a4.local("dest_ptr"), oi(a4, s6, 54), a4.appendU8(5), o2 ? Ws(a4, n4, 19) : (a4.local("pLocals"), a4.i32_const(0), oi(a4, s6, 54)), a4.endBlock()) : (si(a4, Ca(n4, 1), 4), a4.local("dest_ptr"), a4.ptr_const(t6), a4.i32_const(b3), a4.callImport("castv2"), a4.appendU8(69), a4.block(64, 4), Ws(a4, n4, 19), a4.endBlock()), a4.endBlock(), a4.endBlock();
                break;
              }
              case 295:
              case 296:
                a4.ptr_const(za(e4, Ca(n4, 3))), si(a4, Ca(n4, 1), 4), si(a4, Ca(n4, 2), 0), a4.i32_const(296 === b3 ? 1 : 0), a4.callImport("box");
                break;
              case 299: {
                const t6 = za(e4, Ca(n4, 3)), r5 = ea(17), o2 = Ca(n4, 1), s6 = P(t6 + r5);
                if (!t6 || !s6) {
                  jc(a4.traceIndex, 0, 0, "null-klass"), n4 = 0;
                  continue;
                }
                a4.options.zeroPageOptimization && aa() ? (ri(a4, Ca(n4, 2), 40), a4.local("dest_ptr", 34), da(8, 1)) : (li(a4, Ca(n4, 2), n4, true), a4.local("dest_ptr", 34)), a4.appendU8(40), a4.appendMemarg(ea(14), 0), a4.appendU8(40), a4.appendMemarg(ea(15), 0), a4.local("src_ptr", 34), a4.appendU8(40), a4.appendMemarg(r5, 0), a4.i32_const(s6), a4.appendU8(70), a4.local("src_ptr"), a4.appendU8(45), a4.appendMemarg(ea(16), 0), a4.appendU8(69), a4.appendU8(113), a4.block(64, 4), a4.local("pLocals"), a4.local("dest_ptr"), a4.i32_const(ea(18)), a4.appendU8(106), oi(a4, o2, 54), a4.appendU8(5), Ws(a4, n4, 21), a4.endBlock();
                break;
              }
              case 294:
                a4.block(), si(a4, Ca(n4, 1), 4), ri(a4, Ca(n4, 2), 40), a4.callImport("newstr"), a4.appendU8(13), a4.appendULeb(0), Ws(a4, n4, 17), a4.endBlock();
                break;
              case 293: {
                a4.block(), si(a4, Ca(n4, 1), 4);
                const t6 = za(e4, Ca(n4, 3));
                a4.i32_const(t6), ri(a4, Ca(n4, 2), 40), a4.callImport("newarr"), a4.appendU8(13), a4.appendULeb(0), Ws(a4, n4, 17), a4.endBlock();
                break;
              }
              case 283:
                a4.block(), si(a4, Ca(n4, 1), 4), a4.ptr_const(za(e4, Ca(n4, 2))), a4.callImport("newobj_i"), a4.appendU8(13), a4.appendULeb(0), Ws(a4, n4, 17), a4.endBlock();
                break;
              case 282:
              case 284:
              case 544:
              case 543:
                p4 ? (Hs(a4, n4, j2, 15), u4 = true, L2 = 0) : n4 = 0;
                break;
              case 546:
              case 547:
              case 548:
              case 549:
              case 545:
                p4 ? (Hs(a4, n4, j2, 545 == b3 ? 22 : 15), u4 = true) : n4 = 0;
                break;
              case 137:
              case 134:
                Ws(a4, n4, 16), u4 = true;
                break;
              case 130:
              case 131:
                Ws(a4, n4, 26), u4 = true;
                break;
              case 136:
                if (a4.callHandlerReturnAddresses.length > 0 && a4.callHandlerReturnAddresses.length <= 3) {
                  const t6 = Va(e4, Ca(n4, 1));
                  a4.local("pLocals"), a4.appendU8(40), a4.appendMemarg(t6, 0), a4.local("index", 33);
                  for (let e5 = 0; e5 < a4.callHandlerReturnAddresses.length; e5++) {
                    const t7 = a4.callHandlerReturnAddresses[e5];
                    a4.local("index"), a4.ptr_const(t7), a4.appendU8(70), a4.cfg.branch(t7, t7 < n4, 1);
                  }
                  Ws(a4, n4, 25);
                } else n4 = 0;
                break;
              case 135:
              case 635:
              case 636:
                n4 = 0;
                break;
              case 633:
              case 634:
                Vs(a4, n4, b3);
                break;
              case 493:
              case 498:
              case 494:
              case 496:
              case 503:
              case 495:
              case 502:
              case 497:
                a4.block(), si(a4, Ca(n4, 1), 8), si(a4, Ca(n4, 2), 0), a4.i32_const(b3), a4.callImport("conv"), a4.appendU8(13), a4.appendULeb(0), Ws(a4, n4, 13), a4.endBlock();
                break;
              case 456:
              case 457:
              case 462:
              case 463: {
                const e5 = 456 === b3 || 462 === b3, t6 = 462 === b3 || 463 === b3, r5 = t6 ? 9223372036854776e3 : 2147483648, o2 = e5 ? "temp_f32" : "temp_f64";
                a4.local("pLocals"), ri(a4, Ca(n4, 2), e5 ? 42 : 43), a4.local(o2, 34), a4.appendU8(e5 ? 139 : 153), a4.appendU8(e5 ? 67 : 68), e5 ? a4.appendF32(r5) : a4.appendF64(r5), a4.appendU8(e5 ? 93 : 99), a4.block(t6 ? 126 : 127, 4), a4.local(o2), a4.appendU8(ya[b3]), a4.appendU8(5), a4.appendU8(t6 ? 66 : 65), a4.appendBoundaryValue(t6 ? 64 : 32, -1), a4.endBlock(), oi(a4, Ca(n4, 1), t6 ? 55 : 54);
                break;
              }
              case 529:
              case 530: {
                const e5 = 529 === b3;
                a4.local("pLocals"), ri(a4, Ca(n4, 2), e5 ? 40 : 41);
                const t6 = Da(n4, 3), r5 = Da(n4, 4);
                e5 ? a4.i32_const(t6) : a4.i52_const(t6), a4.appendU8(e5 ? 106 : 124), e5 ? a4.i32_const(r5) : a4.i52_const(r5), a4.appendU8(e5 ? 108 : 126), oi(a4, Ca(n4, 1), e5 ? 54 : 55);
                break;
              }
              case 650:
              case 651: {
                const e5 = 651 === b3;
                a4.local("pLocals"), ri(a4, Ca(n4, 2), e5 ? 41 : 40), e5 ? a4.i52_const(1) : a4.i32_const(1), a4.appendU8(e5 ? 132 : 114), a4.appendU8(e5 ? 121 : 103), e5 && a4.appendU8(167), a4.i32_const(e5 ? 63 : 31), a4.appendU8(115), oi(a4, Ca(n4, 1), 54);
                break;
              }
              case 531:
              case 532: {
                const e5 = 531 === b3, t6 = e5 ? 40 : 41, r5 = e5 ? 54 : 55;
                a4.local("pLocals"), ri(a4, Ca(n4, 2), t6), ri(a4, Ca(n4, 3), t6), e5 ? a4.i32_const(31) : a4.i52_const(63), a4.appendU8(e5 ? 113 : 131), a4.appendU8(e5 ? 116 : 134), oi(a4, Ca(n4, 1), r5);
                break;
              }
              case 591:
              case 618: {
                const e5 = 618 === b3, t6 = e5 ? 42 : 43, r5 = e5 ? 56 : 57;
                a4.local("pLocals"), ri(a4, Ca(n4, 2), t6), ri(a4, Ca(n4, 3), t6), ri(a4, Ca(n4, 4), t6), a4.callImport(e5 ? "fmaf" : "fma"), oi(a4, Ca(n4, 1), r5);
                break;
              }
              default:
                b3 >= 3 && b3 <= 12 || b3 >= 509 && b3 <= 510 ? p4 || a4.options.countBailouts ? (Ws(a4, n4, 14), u4 = true) : n4 = 0 : b3 >= 13 && b3 <= 21 ? pi(a4, n4, b3) ? $2 = true : n4 = 0 : b3 >= 74 && b3 <= 85 ? ui(a4, n4, b3) || (n4 = 0) : b3 >= 344 && b3 <= 427 ? _i(a4, n4, b3) || (n4 = 0) : wa[b3] ? mi(a4, n4, b3) || (n4 = 0) : va[b3] ? yi(a4, n4, e4, b3) ? p4 = true : n4 = 0 : b3 >= 23 && b3 <= 49 ? di(a4, e4, n4, b3) || (n4 = 0) : b3 >= 50 && b3 <= 73 ? fi(a4, e4, n4, b3) || (n4 = 0) : b3 >= 87 && b3 <= 127 ? ki(a4, n4, b3) || (n4 = 0) : b3 >= 579 && b3 <= 632 ? wi(a4, n4, b3) || (n4 = 0) : b3 >= 315 && b3 <= 341 ? vi(a4, e4, n4, b3) || (n4 = 0) : b3 >= 227 && b3 <= 270 ? a4.branchTargets.size > 0 ? (Hs(a4, n4, j2, 8), u4 = true) : n4 = 0 : b3 >= 652 && b3 <= 659 ? (a4.containsSimd = true, Ti(a4, n4, b3, T2, v3, U2) ? $2 = true : n4 = 0) : b3 >= 559 && b3 <= 571 ? (a4.containsAtomics = true, Ai(a4, n4, b3) || (n4 = 0)) : 0 === L2 || (n4 = 0);
            }
            if (n4) {
              if (!$2) {
                const e5 = n4 + 2;
                for (let t6 = 0; t6 < w3; t6++) Za(O(e5 + 2 * t6));
              }
              if (ic.dumpTraces || i5) {
                let e5 = `${n4.toString(16)} ${T2} `;
                const t6 = n4 + 2, r5 = t6 + 2 * w3;
                for (let t7 = 0; t7 < y3; t7++) 0 !== t7 && (e5 += ", "), e5 += O(r5 + 2 * t7);
                w3 > 0 && (e5 += " -> ");
                for (let n5 = 0; n5 < w3; n5++) 0 !== n5 && (e5 += ", "), e5 += O(t6 + 2 * n5);
                a4.traceBuf.push(e5);
              }
              L2 > 0 && (p4 ? m3++ : _3++, f5 += L2), (n4 += 2 * k3) <= s5 && (h4 = n4);
            } else i5 && Ve(`instrumented trace ${t5} aborted for opcode ${T2} @${E2.toString(16)}`), jc(a4.traceIndex, 0, 0, b3);
          }
          for (; a4.activeBlocks > 0; ) a4.endBlock();
          return a4.cfg.exitIp = h4, a4.containsSimd && (f5 += 10240), f5;
        })(e3, d3, n3, r3, u3, p3, y2, c3), s4 = i4 >= ic.minimumTraceValue, p3.cfg.generate();
      })), p3.emitImportsAndFunctions(false), !s4) return g2 && "end-of-body" === g2.abortReason && (g2.abortReason = "trace-too-small"), 0;
      _2 = zs();
      const f4 = p3.getArrayView();
      if (da(6, f4.length), f4.length >= p3.options.maxModuleSize) return We(`Jiterpreter generated too much code (${f4.length} bytes) for trace ${d3}. Please report this issue.`), 0;
      const h3 = new WebAssembly.Module(f4), w2 = p3.getWasmImports(), k2 = new WebAssembly.Instance(h3, w2).exports[d3];
      let S2;
      m2 = false, l3 ? (qs().set(l3, k2), S2 = l3) : (S2 = Gs(0, k2), 0 === S2 && (cc = true));
      const v2 = ua(1);
      return p3.options.enableStats && v2 && v2 % 500 == 0 && $c(true), S2;
    } catch (e4) {
      h2 = true, m2 = false;
      let t4 = p3.containsSimd ? " (simd)" : "";
      return p3.containsAtomics && (t4 += " (atomics)"), He(`${i3 || d3}${t4} code generation failed: ${e4} ${e4.stack}`), Zs(), 0;
    } finally {
      const e4 = zs();
      if (_2 ? (da(11, _2 - f3), da(12, e4 - _2)) : da(11, e4 - f3), h2 || !m2 && ic.dumpTraces || b2) {
        if (h2 || ic.dumpTraces || b2) for (let e6 = 0; e6 < p3.traceBuf.length; e6++) Ve(p3.traceBuf[e6]);
        Ve(`// ${i3 || d3} generated, blob follows //`);
        let e5 = "", t4 = 0;
        try {
          for (; p3.activeBlocks > 0; ) p3.endBlock();
          p3.inSection && p3.endSection();
        } catch (e6) {
        }
        const n4 = p3.getArrayView(false, true);
        for (let r4 = 0; r4 < n4.length; r4++) {
          const o2 = n4[r4];
          o2 < 16 && (e5 += "0"), e5 += o2.toString(16), e5 += " ", e5.length % 10 == 0 && (Ve(`${t4}	${e5}`), e5 = "", t4 = r4 + 1);
        }
        Ve(`${t4}	${e5}`), Ve("// end blob //");
      }
    }
  })(e2, u2, n2, s2, a2, r2, l2, d2, c2);
  return f2 ? (da(1, 1), p2.fnPtr = f2, f2) : ic.estimateHeat ? 0 : 1;
}, function(e2) {
  const t2 = Oi[e2 &= -2];
  if (t2) {
    if (Di || (Di = fa()), t2.hitCount++, t2.hitCount === Di.interpEntryFlushThreshold) Pi();
    else if (t2.hitCount !== Di.interpEntryHitCount) return;
    o.mono_jiterp_tlqueue_add(1, e2) >= 4 ? Pi() : Ni > 0 || "function" == typeof globalThis.setTimeout && (Ni = globalThis.setTimeout((() => {
      Ni = 0, Pi();
    }), 10));
  }
}, function(e2, t2, n2, r2, o2, s2, a2, i2) {
  if (n2 > 16) return 0;
  const c2 = new Fi(e2, t2, n2, r2, o2, s2, a2, i2);
  Bi || (Bi = qs());
  const l2 = Bi.get(i2), p2 = (s2 ? a2 ? 29 : 20 : a2 ? 11 : 2) + n2;
  return c2.result = Gs(p2, l2), Oi[e2] = c2, c2.result;
}, function(e2, t2, n2, r2, s2) {
  const a2 = P(n2 + 0), i2 = Xi[a2];
  if (i2) return void (i2.result > 0 ? o.mono_jiterp_register_jit_call_thunk(n2, i2.result) : (i2.queue.push(n2), i2.queue.length > 12 && Ki()));
  const c2 = new Yi(e2, t2, n2, r2, 0 !== s2);
  Xi[a2] = c2;
  const l2 = o.mono_jiterp_tlqueue_add(0, e2);
  let p2 = Qi[e2];
  p2 || (p2 = Qi[e2] = []), p2.push(c2), l2 >= 6 && Ki();
}, function(e2, t2, n2, r2, s2) {
  const a2 = Zi(e2);
  try {
    a2(t2, n2, r2, s2);
  } catch (e3) {
    const t3 = Ke.wasmExports.__cpp_exception, n3 = t3 instanceof WebAssembly.Tag;
    if (n3 && !(e3 instanceof WebAssembly.Exception && e3.is(t3))) throw e3;
    if (i2 = s2, Ke.HEAPU32[i2 >>> 2] = 1, n3) {
      const n4 = e3.getArg(t3, 0);
      o.mono_jiterp_begin_catch(n4), o.mono_jiterp_end_catch();
    } else {
      if ("number" != typeof e3) throw e3;
      o.mono_jiterp_begin_catch(e3), o.mono_jiterp_end_catch();
    }
  }
  var i2;
}, Ki, function(e2, t2, n2) {
  ct.emscriptenBuildOptions.enableDevToolsProfiler && (function(e3) {
    Mt.delete(e3);
  })(e2), delete hc[n2], (function(e3) {
    delete Oi[e3];
  })(t2), (function(e3) {
    const t3 = Qi[e3];
    if (t3) {
      for (let e4 = 0; e4 < t3.length; e4++) delete Xi[t3[e4].addr];
      delete Qi[e3];
    }
  })(e2);
}, function() {
  return globalThis.performance.now();
}, function(e2, t2) {
  const n2 = st ? { start: t2 } : { startTime: t2 };
  let r2 = Mt.get(e2);
  if (!r2) {
    const t3 = a.mono_wasm_method_get_name_ex(e2);
    r2 = Le(t3), Mt.set(e2, r2), m(t3);
  }
  globalThis.performance.measure(r2, n2);
}, function(e2, t2, n2, r2, o2) {
  const s2 = n2 ? Le(n2) : "<no message>", a2 = !!r2, i2 = e2 ? Le(e2) : "", c2 = o2, l2 = t2 ? Le(t2) : "", p2 = `[MONO] ${s2}`;
  if (et.logging && "function" == typeof et.logging.trace) et.logging.trace(i2, l2, p2, a2, c2);
  else switch (l2) {
    case "critical":
    case "error":
      {
        const e3 = p2 + "\n" + new Error().stack;
        lt.exitReason || (lt.exitReason = e3), console.error(Qe(e3));
      }
      break;
    case "warning":
      console.warn(p2);
      break;
    case "message":
    default:
      console.log(p2);
      break;
    case "info":
      console.info(p2);
      break;
    case "debug":
      console.debug(p2);
  }
}, function(e2) {
  wt = lt.config.mainAssemblyName + ".dll", kt = e2, console.assert(true, `Adding an entrypoint breakpoint ${wt} at method token  ${kt}`);
  debugger;
}, function(e2, t2) {
  if (!globalThis.crypto || !globalThis.crypto.getRandomValues) return -1;
  const n2 = K(), r2 = n2.subarray(e2, e2 + t2), o2 = (n2.buffer, false), s2 = o2 ? new Uint8Array(t2) : r2;
  for (let e3 = 0; e3 < t2; e3 += 65536) {
    const n3 = s2.subarray(e3, e3 + Math.min(t2 - e3, 65536));
    globalThis.crypto.getRandomValues(n3);
  }
  return o2 && r2.set(s2), 0;
}, Xc, function() {
  console.clear();
}, Mr, function(e2) {
  hr(), e2 = oe(e2, 0);
  try {
    return (function(e3) {
      hr();
      const t2 = Ft(), r2 = Mn(e3);
      2 !== r2 && mt(false, `Signature version ${r2} mismatch.`);
      const o2 = (function(e4) {
        e4 || mt(false, "Null signatures");
        const t3 = V(e4 + 16);
        if (0 === t3) return null;
        const n2 = V(e4 + 20);
        return t3 || mt(false, "Null name"), Re(e4 + t3, e4 + t3 + n2);
      })(e3), s2 = (function(e4) {
        e4 || mt(false, "Null signatures");
        const t3 = V(e4 + 24);
        return 0 === t3 ? null : Re(e4 + t3, e4 + t3 + V(e4 + 28));
      })(e3), a2 = (function(e4) {
        return e4 || mt(false, "Null signatures"), V(e4 + 8);
      })(e3);
      lt.diagnosticTracing && ze(`Binding [JSImport] ${o2} from ${s2} module`);
      const i2 = (function(e4, t3) {
        e4 && "string" == typeof e4 || mt(false, "function_name must be string");
        let n2 = {};
        const r3 = e4.split(".");
        t3 ? (n2 = _r.get(t3), n2 || mt(false, `ES6 module ${t3} was not imported yet, please call JSHost.ImportAsync() first in order to invoke ${e4}.`)) : "INTERNAL" === r3[0] ? (n2 = et, r3.shift()) : "globalThis" === r3[0] && (n2 = globalThis, r3.shift());
        for (let t4 = 0; t4 < r3.length - 1; t4++) {
          const o4 = r3[t4], s3 = n2[o4];
          if (!s3) throw new Error(`${o4} not found while looking up ${e4}`);
          n2 = s3;
        }
        const o3 = n2[r3[r3.length - 1]];
        if ("function" != typeof o3) throw new Error(`${e4} must be a Function but was ${typeof o3}`);
        return o3.bind(n2);
      })(o2, s2), c2 = Pn(e3), l2 = new Array(c2), p2 = new Array(c2);
      let u2 = false;
      for (let t3 = 0; t3 < c2; t3++) {
        const n2 = Bn(e3, t3 + 2), r3 = Nn(n2), o3 = zt(n2, r3, t3 + 2);
        o3 || mt(false, "ERR42: argument marshaler must be resolved"), l2[t3] = o3, 23 === r3 && (p2[t3] = (e4) => {
          e4 && e4.dispose();
        }, u2 = true);
      }
      const d2 = Bn(e3, 1), f2 = Nn(d2), _2 = eo(d2, f2, 1), m2 = 26 == f2, h2 = 20 == f2 || 30 == f2, g2 = { fn: i2, fqn: s2 + ":" + o2, args_count: c2, arg_marshalers: l2, res_converter: _2, has_cleanup: u2, arg_cleanup: p2, is_discard_no_wait: m2, is_async: h2, isDisposed: false };
      let b2;
      b2 = h2 || m2 || u2 ? ar(g2) : 0 != c2 || _2 ? 1 != c2 || _2 ? 1 == c2 && _2 ? (function(e4) {
        const t3 = e4.fn, r3 = e4.arg_marshalers[0], o3 = e4.res_converter, s3 = e4.fqn;
        return e4 = null, function(a3) {
          const i3 = Ft();
          try {
            n && e4.isDisposed;
            const s4 = r3(a3), i4 = t3(s4);
            o3(a3, i4);
          } catch (e5) {
            wo(a3, e5);
          } finally {
            Pt(i3, "mono.callCsFunction:", s3);
          }
        };
      })(g2) : 2 == c2 && _2 ? (function(e4) {
        const t3 = e4.fn, r3 = e4.arg_marshalers[0], o3 = e4.arg_marshalers[1], s3 = e4.res_converter, a3 = e4.fqn;
        return e4 = null, function(i3) {
          const c3 = Ft();
          try {
            n && e4.isDisposed;
            const a4 = r3(i3), c4 = o3(i3), l3 = t3(a4, c4);
            s3(i3, l3);
          } catch (e5) {
            wo(i3, e5);
          } finally {
            Pt(c3, "mono.callCsFunction:", a3);
          }
        };
      })(g2) : ar(g2) : (function(e4) {
        const t3 = e4.fn, r3 = e4.arg_marshalers[0], o3 = e4.fqn;
        return e4 = null, function(s3) {
          const a3 = Ft();
          try {
            n && e4.isDisposed;
            const o4 = r3(s3);
            t3(o4);
          } catch (e5) {
            wo(s3, e5);
          } finally {
            Pt(a3, "mono.callCsFunction:", o3);
          }
        };
      })(g2) : (function(e4) {
        const t3 = e4.fn, r3 = e4.fqn;
        return e4 = null, function(o3) {
          const s3 = Ft();
          try {
            n && e4.isDisposed, t3();
          } catch (e5) {
            wo(o3, e5);
          } finally {
            Pt(s3, "mono.callCsFunction:", r3);
          }
        };
      })(g2);
      let y2 = b2;
      y2[xn] = g2, sr[a2] = y2, Pt(t2, "mono.bindJsFunction:", o2);
    })(e2), 0;
  } catch (e3) {
    return Ne((function(e4) {
      let t2 = "unknown exception";
      if (e4) {
        t2 = e4.toString();
        const n2 = e4.stack;
        n2 && (n2.startsWith(t2) ? t2 = n2 : t2 += "\n" + n2), t2 = Xe(t2);
      }
      return t2;
    })(e3));
  }
}, function(e2, t2) {
  !(function(e3, t3) {
    lt.assert_runtime_running();
    const n2 = Fr(e3);
    n2 && "function" == typeof n2 && n2[En] || mt(false, `Bound function handle expected ${e3}`), n2(t3 = oe(t3, 0));
  })(e2, t2);
}, function(e2, t2) {
  lt.assert_runtime_running(), t2 = oe(t2, 0);
  const n2 = sr[e2];
  n2 || mt(false, `Imported function handle expected ${e2}`), n2(t2);
}, function(e2) {
  gr((() => (function(e3) {
    if (!lt.is_runtime_running()) return void (lt.diagnosticTracing && ze("This promise resolution/rejection can't be propagated to managed code, mono runtime already exited."));
    const t2 = Ln(e3 = oe(e3, 0), 0), r2 = n;
    try {
      lt.assert_runtime_running();
      const n2 = Ln(e3, 1), o2 = Ln(e3, 2), s2 = Ln(e3, 3), a2 = zn(o2), i2 = Qn(o2), c2 = Fr(i2);
      c2 || mt(false, `Cannot find Promise for JSHandle ${i2}`), c2.resolve_or_reject(a2, i2, s2), r2 || (Wn(n2, 1), Wn(t2, 0));
    } catch (e4) {
      wo(t2, e4);
    }
  })(e2)));
}, function(e2) {
  gr((() => (function(e3) {
    if (!lt.is_runtime_running()) return void (lt.diagnosticTracing && ze("This promise can't be canceled, mono runtime already exited."));
    const t2 = qr(e3);
    t2 || mt(false, `Expected Promise for GCHandle ${e3}`), t2.cancel();
  })(e2)));
}, function(e2, t2, n2, r2, o2, s2, a2) {
  try {
    const i2 = Re(n2, n2 + 2 * r2), c2 = Pc(i2);
    if (!c2 && i2) return Be(o2, o2 + 2 * i2.length, i2), T(a2, i2.length), 0;
    const l2 = Pc(Re(e2, e2 + 2 * t2));
    if (!c2 || !l2) throw new Error(`Locale or culture name is null or empty. localeName=${c2}, cultureName=${l2}`);
    const p2 = c2.split("-");
    let u2, d2;
    try {
      const e3 = p2.length > 1 ? p2.pop() : void 0;
      d2 = e3 ? new Intl.DisplayNames([l2], { type: "region" }).of(e3) : void 0;
      const t3 = p2.join("-");
      u2 = new Intl.DisplayNames([l2], { type: "language" }).of(t3);
    } catch (e3) {
      if (!(e3 instanceof RangeError)) throw e3;
      try {
        u2 = new Intl.DisplayNames([l2], { type: "language" }).of(c2);
      } catch (e4) {
        if (e4 instanceof RangeError && i2) return Be(o2, o2 + 2 * i2.length, i2), T(a2, i2.length), 0;
        throw e4;
      }
    }
    const f2 = { LanguageName: u2, RegionName: d2 }, _2 = Object.values(f2).join("##");
    if (!_2) throw new Error(`Locale info for locale=${c2} is null or empty.`);
    if (_2.length > s2) throw new Error(`Locale info for locale=${c2} exceeds length of ${s2}.`);
    return Be(o2, o2 + 2 * _2.length, _2), T(a2, _2.length), 0;
  } catch (e3) {
    return T(a2, -1), Ne(e3.toString());
  }
}, function(e2) {
  return pt.ds_rt_websocket_create(e2);
}, function(e2, t2, n2) {
  return pt.ds_rt_websocket_send(e2, t2, n2);
}, function(e2) {
  return pt.ds_rt_websocket_poll(e2);
}, function(e2, t2, n2) {
  return pt.ds_rt_websocket_recv(e2, t2, n2);
}, function(e2) {
  return pt.ds_rt_websocket_close(e2);
}];
async function zc(e2, t2) {
  try {
    const n2 = await Vc(e2, t2);
    return lt.mono_exit(n2), n2;
  } catch (e3) {
    try {
      lt.mono_exit(1, e3);
    } catch (e4) {
    }
    return e3 && "number" == typeof e3.status ? e3.status : 1;
  }
}
async function Vc(e2, t2) {
  null != e2 && "" !== e2 || (e2 = lt.config.mainAssemblyName) || mt(false, "Null or empty config.mainAssemblyName"), null == t2 && (t2 = ct.config.applicationArguments), null == t2 && (t2 = tt ? (await import(
    /*! webpackIgnore: true */
    "process"
  )).argv.slice(2) : []), (function(e3, t3) {
    const n2 = t3.length + 1, r2 = _(4 * n2);
    let s2 = 0;
    Ke.setValue(r2 + 4 * s2, o.mono_wasm_strdup(e3), "i32"), s2 += 1;
    for (let e4 = 0; e4 < t3.length; ++e4) Ke.setValue(r2 + 4 * s2, o.mono_wasm_strdup(t3[e4]), "i32"), s2 += 1;
    o.mono_wasm_set_main_args(n2, r2);
  })(e2, t2), lt.config.mainAssemblyName = e2, -1 == ct.waitForDebugger && (Ve("waiting for debugger..."), await new Promise(((e3) => {
    const t3 = setInterval((() => {
      1 == ct.waitForDebugger && (clearInterval(t3), e3());
    }), 100);
  })));
  try {
    return Ke.runtimeKeepalivePush(), await new Promise(((e3) => globalThis.setTimeout(e3, 0))), await (function(e3, t3, n2) {
      lt.assert_runtime_running();
      const r2 = Ke.stackSave();
      try {
        const r3 = $n(5), o2 = Ln(r3, 1), s2 = Ln(r3, 2), a2 = Ln(r3, 3), i2 = Ln(r3, 4);
        uo(s2, $e(e3)), Uo(a2, t3 && !t3.length ? void 0 : t3, 15), no(i2, n2);
        let c2 = sn(o2, 0, Jt);
        return wn(ct.managedThreadTID, yn.CallEntrypoint, r3), c2 = an(r3, Jt, c2), null == c2 && (c2 = Promise.resolve(0)), c2[Dr] = true, c2;
      } finally {
        lt.is_runtime_running() && Ke.stackRestore(r2);
      }
    })(e2, t2, 1 == ct.waitForDebugger);
  } finally {
    Ke.runtimeKeepalivePop();
  }
}
function Wc(e2) {
  ct.runtimeReady && (ct.runtimeReady = false, o.mono_wasm_exit(e2));
}
function Hc(e2) {
  if (lt.exitReason = e2, ct.runtimeReady) {
    ct.runtimeReady = false;
    const t2 = Qe(e2);
    Ke.abort(t2);
  }
  throw e2;
}
var qc, Gc;
const Jc = 2147483647 & (null !== (Gc = null === (qc = globalThis.performance) || void 0 === qc ? void 0 : qc.timeOrigin) && void 0 !== Gc ? Gc : Date.now());
function Xc() {
  return Jc;
}
async function Qc(e2) {
  e2.out || (e2.out = console.log.bind(console)), e2.err || (e2.err = console.error.bind(console)), e2.print || (e2.print = e2.out), e2.printErr || (e2.printErr = e2.err), lt.out = e2.print, lt.err = e2.printErr, await (async function() {
    var e3;
    if (tt) {
      if (globalThis.performance === Io) {
        const { performance: e4 } = et.require("perf_hooks");
        globalThis.performance = e4;
      }
      if (et.process = await import(
        /*! webpackIgnore: true */
        "process"
      ), globalThis.crypto || (globalThis.crypto = {}), !globalThis.crypto.getRandomValues) {
        let e4;
        try {
          e4 = et.require("node:crypto");
        } catch (e5) {
        }
        e4 ? e4.webcrypto ? globalThis.crypto = e4.webcrypto : e4.randomBytes && (globalThis.crypto.getRandomValues = (t2) => {
          t2 && t2.set(e4.randomBytes(t2.length));
        }) : globalThis.crypto.getRandomValues = () => {
          throw new Error("Using node without crypto support. To enable current operation, either provide polyfill for 'globalThis.crypto.getRandomValues' or enable 'node:crypto' module.");
        };
      }
    }
    ct.subtle = null === (e3 = globalThis.crypto) || void 0 === e3 ? void 0 : e3.subtle;
  })();
}
function Yc(e2) {
  const t2 = Ft();
  e2.locateFile || (e2.locateFile = e2.__locateFile = (e3) => lt.scriptDirectory + e3), e2.mainScriptUrlOrBlob = lt.scriptUrl;
  const s2 = e2.instantiateWasm, i2 = e2.preInit ? "function" == typeof e2.preInit ? [e2.preInit] : e2.preInit : [], l2 = e2.preRun ? "function" == typeof e2.preRun ? [e2.preRun] : e2.preRun : [], p2 = e2.postRun ? "function" == typeof e2.postRun ? [e2.postRun] : e2.postRun : [], u2 = e2.onRuntimeInitialized ? e2.onRuntimeInitialized : () => {
  };
  e2.instantiateWasm = (e3, t3) => (function(e4, t4, n2) {
    const r2 = Ft();
    if (n2) {
      const o2 = n2(e4, ((e5, n3) => {
        Pt(r2, "mono.instantiateWasm"), ct.afterInstantiateWasm.promise_control.resolve(), t4(e5, n3);
      }));
      return o2;
    }
    return (async function(e5, t5) {
      try {
        await lt.afterConfigLoaded, lt.diagnosticTracing && ze("instantiate_wasm_module"), await ct.beforePreInit.promise, Ke.addRunDependency("instantiate_wasm_module"), await (async function() {
          const e6 = lt.simd(), t6 = lt.relaxedSimd(), n4 = lt.exceptions();
          ct.featureWasmSimd = await e6, ct.featureWasmRelaxedSimd = await t6, ct.featureWasmEh = await n4, ct.emscriptenBuildOptions.wasmEnableSIMD && (ct.featureWasmSimd || mt(false, "This browser/engine doesn't support WASM SIMD. Please use a modern version. See also https://aka.ms/dotnet-wasm-features")), ct.emscriptenBuildOptions.wasmEnableEH && (ct.featureWasmEh || mt(false, "This browser/engine doesn't support WASM exception handling. Please use a modern version. See also https://aka.ms/dotnet-wasm-features"));
        })(), (function(e6) {
          const t6 = e6.env || e6.a;
          if (!t6) return void We("WARNING: Neither imports.env or imports.a were present when instantiating the wasm module. This likely indicates an emscripten configuration issue.");
          const n4 = new Array(Mc.length);
          for (const e7 in t6) {
            const r3 = t6[e7];
            if ("function" == typeof r3 && -1 !== r3.toString().indexOf("runtime_idx")) try {
              const { runtime_idx: t7 } = r3();
              if (void 0 !== n4[t7]) throw new Error(`Duplicate runtime_idx ${t7}`);
              n4[t7] = e7;
            } catch (e8) {
            }
          }
          for (const [e7, r3] of Mc.entries()) {
            const o2 = n4[e7];
            if (void 0 !== o2) {
              if ("function" != typeof t6[o2]) throw new Error(`Expected ${o2} to be a function`);
              t6[o2] = r3;
            }
          }
        })(e5);
        const n3 = await lt.wasmCompilePromise.promise;
        t5(await WebAssembly.instantiate(n3, e5), n3), lt.diagnosticTracing && ze("instantiate_wasm_module done"), ct.afterInstantiateWasm.promise_control.resolve();
      } catch (e6) {
        throw He("instantiate_wasm_module() failed", e6), lt.mono_exit(1, e6), e6;
      }
      Ke.removeRunDependency("instantiate_wasm_module");
    })(e4, t4), [];
  })(e3, t3, s2), e2.preInit = [() => (function(e3) {
    Ke.addRunDependency("mono_pre_init");
    const t3 = Ft();
    try {
      Ke.addRunDependency("mono_wasm_pre_init_essential"), lt.diagnosticTracing && ze("mono_wasm_pre_init_essential"), lt.gitHash !== ct.gitHash && We(`The version of dotnet.runtime.js ${ct.gitHash} is different from the version of dotnet.js ${lt.gitHash}!`), lt.gitHash !== ct.emscriptenBuildOptions.gitHash && We(`The version of dotnet.native.js ${ct.emscriptenBuildOptions.gitHash}  is different from the version of dotnet.js ${lt.gitHash}!`), n !== ct.emscriptenBuildOptions.wasmEnableThreads && We(`The threads of dotnet.native.js ${ct.emscriptenBuildOptions.wasmEnableThreads} is different from the version of dotnet.runtime.js ${n}!`), (function() {
        const e4 = [...r];
        for (const t4 of e4) {
          const e5 = o, [n2, r2, s4, a2, i3] = t4, l3 = "function" == typeof n2;
          if (true === n2 || l3) e5[r2] = function(...t5) {
            !l3 || !n2() || mt(false, `cwrap ${r2} should not be called when binding was skipped`);
            const o2 = c(r2, s4, a2, i3);
            return e5[r2] = o2, o2(...t5);
          };
          else {
            const t5 = c(r2, s4, a2, i3);
            e5[r2] = t5;
          }
        }
      })(), s3 = et, Object.assign(s3, { mono_wasm_exit: o.mono_wasm_exit, mono_wasm_profiler_init_aot: a.mono_wasm_profiler_init_aot, mono_wasm_profiler_init_browser_devtools: a.mono_wasm_profiler_init_browser_devtools, mono_wasm_exec_regression: o.mono_wasm_exec_regression, mono_wasm_print_thread_dump: void 0 }), Ke.removeRunDependency("mono_wasm_pre_init_essential"), lt.diagnosticTracing && ze("preInit"), ct.beforePreInit.promise_control.resolve(), e3.forEach(((e4) => e4(Ke)));
    } catch (e4) {
      throw He("user preInint() failed", e4), lt.mono_exit(1, e4), e4;
    }
    var s3;
    (async () => {
      try {
        await (async function() {
          lt.diagnosticTracing && ze("mono_wasm_pre_init_essential_async"), Ke.addRunDependency("mono_wasm_pre_init_essential_async"), Ke.removeRunDependency("mono_wasm_pre_init_essential_async");
        })(), Pt(t3, "mono.preInit");
      } catch (e4) {
        throw lt.mono_exit(1, e4), e4;
      }
      ct.afterPreInit.promise_control.resolve(), Ke.removeRunDependency("mono_pre_init");
    })();
  })(i2)], e2.preRun = [() => (async function(e3) {
    Ke.addRunDependency("mono_pre_run_async");
    try {
      await ct.afterInstantiateWasm.promise, await ct.afterPreInit.promise, lt.diagnosticTracing && ze("preRunAsync");
      const t3 = Ft();
      e3.map(((e4) => e4(Ke))), Pt(t3, "mono.preRun");
    } catch (e4) {
      throw He("preRunAsync() failed", e4), lt.mono_exit(1, e4), e4;
    }
    ct.afterPreRun.promise_control.resolve(), Ke.removeRunDependency("mono_pre_run_async");
  })(l2)], e2.onRuntimeInitialized = () => (async function(e3) {
    try {
      await ct.afterPreRun.promise, lt.diagnosticTracing && ze("onRuntimeInitialized"), ct.nativeExit = Wc, ct.nativeAbort = Hc;
      const t3 = Ft();
      if (ct.beforeOnRuntimeInitialized.promise_control.resolve(), await ct.coreAssetsInMemory.promise, ct.config.virtualWorkingDirectory) {
        const e4 = Ke.FS, t4 = ct.config.virtualWorkingDirectory;
        try {
          const n2 = e4.stat(t4);
          n2 ? n2 && e4.isDir(n2.mode) || mt(false, `FS.chdir: ${t4} is not a directory`) : Ke.FS_createPath("/", t4, true, true);
        } catch (e5) {
          Ke.FS_createPath("/", t4, true, true);
        }
        e4.chdir(t4);
      }
      ct.config.interpreterPgo && setTimeout(Kc, 1e3 * (ct.config.interpreterPgoSaveDelay || 15)), Ke.runtimeKeepalivePush(), n || await (async function() {
        try {
          const t4 = Ft(), n2 = ct.config.environmentVariables || {};
          lt.diagnosticTracing && ze("Initializing mono runtime");
          for (const e5 in n2) {
            const t5 = n2[e5];
            if ("string" != typeof t5) throw new Error(`Expected environment variable '${e5}' to be a string but it was ${typeof t5}: '${t5}'`);
            Zc(e5, t5);
          }
          if (ct.config.runtimeOptions && (function(e5) {
            if (!Array.isArray(e5)) throw new Error("Expected runtimeOptions to be an array of strings");
            const t5 = _(4 * e5.length);
            let n3 = 0;
            for (let r2 = 0; r2 < e5.length; ++r2) {
              const s3 = e5[r2];
              if ("string" != typeof s3) throw new Error("Expected runtimeOptions to be an array of strings");
              Ke.setValue(t5 + 4 * n3, o.mono_wasm_strdup(s3), "i32"), n3 += 1;
            }
            o.mono_wasm_parse_runtime_options(e5.length, t5);
          })(ct.config.runtimeOptions), ct.emscriptenBuildOptions.enableEventPipe) {
            const e5 = "DOTNET_DiagnosticPorts", t5 = "js://ready";
            n2[e5] || (n2[e5] = t5, Zc(e5, t5));
          } else ct.emscriptenBuildOptions.enableAotProfiler ? (function(e5) {
            ct.emscriptenBuildOptions.enableAotProfiler || mt(false, "AOT profiler is not enabled, please use <WasmProfilers>aot;</WasmProfilers> in your project file."), null == e5 && (e5 = {}), "writeAt" in e5 || (e5.writeAt = "System.Runtime.InteropServices.JavaScript.JavaScriptExports::StopProfile"), "sendTo" in e5 || (e5.sendTo = "Interop/Runtime::DumpAotProfileData");
            const t5 = "aot:write-at-method=" + e5.writeAt + ",send-to-method=" + e5.sendTo;
            a.mono_wasm_profiler_init_aot(t5);
          })(ct.config.aotProfilerOptions || {}) : ct.emscriptenBuildOptions.enableDevToolsProfiler ? (function() {
            ct.emscriptenBuildOptions.enableDevToolsProfiler || mt(false, "DevTools profiler is not enabled, please use <WasmProfilers>browser:callspec=N:Sample</WasmProfilers> in your project file."), Dt = globalThis.performance && "function" == typeof globalThis.performance.measure;
            const e5 = `${ct.config.environmentVariables.DOTNET_WasmPerformanceInstrumentation || "callspec=all"}`;
            a.mono_wasm_profiler_init_browser_devtools(e5);
          })() : ct.emscriptenBuildOptions.enableLogProfiler && (e4 = ct.config.logProfilerOptions || {}, ct.emscriptenBuildOptions.enableLogProfiler || mt(false, "Log profiler is not enabled, please use <WasmProfilers>log;</WasmProfilers> in your project file."), e4.takeHeapshot || mt(false, "Log profiler is not enabled, the takeHeapshot method must be defined in LogProfilerOptions.takeHeapshot"), e4.configuration || (e4.configuration = "log:alloc,output=output.mlpd"), e4.takeHeapshot ? a.mono_wasm_profiler_init_log(`${e4.configuration},take-heapshot-method=${e4.takeHeapshot}`) : a.mono_wasm_profiler_init_log(e4.configuration));
          !(function() {
            var e5, t5, n3, r2;
            lt.diagnosticTracing && ze("mono_wasm_load_runtime");
            try {
              const s3 = Ft();
              let a2 = ct.config.debugLevel;
              null == a2 && (a2 = 0, ct.config.debugLevel && (a2 = 0 + a2)), lt.isDebuggingSupported() && (ct.config.resources.corePdb || ct.config.resources.pdb) || (a2 = 0);
              const i3 = /* @__PURE__ */ new Map();
              if (null === (t5 = null === (e5 = ct.config.runtimeConfig) || void 0 === e5 ? void 0 : e5.runtimeOptions) || void 0 === t5 ? void 0 : t5.configProperties) for (const [e6, t6] of Object.entries(null === (r2 = null === (n3 = ct.config.runtimeConfig) || void 0 === n3 ? void 0 : n3.runtimeOptions) || void 0 === r2 ? void 0 : r2.configProperties)) i3.set(e6, "" + t6);
              i3.set("APP_CONTEXT_BASE_DIRECTORY", "/"), i3.set("RUNTIME_IDENTIFIER", "browser-wasm");
              const c2 = i3.size, l3 = [], p3 = _(4 * i3.size), u3 = _(4 * i3.size);
              l3.push(p3), l3.push(u3);
              let d2 = 0;
              for (const [e6, t6] of i3.entries()) {
                const n4 = $e(e6), r3 = $e(t6);
                S(p3 + 4 * d2, n4), S(u3 + 4 * d2, r3), d2++, l3.push(n4), l3.push(r3);
              }
              o.mono_wasm_load_runtime(a2, c2, p3, u3);
              for (const e6 of l3) Ke._free(e6);
              Pt(s3, "mono.loadRuntime");
            } catch (e6) {
              throw He("mono_wasm_load_runtime () failed", e6), lt.mono_exit(1, e6), e6;
            }
          })(), (function() {
            if (ma) return;
            ma = true;
            const e5 = fa(), t5 = e5.tableSize, n3 = ct.emscriptenBuildOptions.runAOTCompilation ? e5.tableSize : 1, r2 = ct.emscriptenBuildOptions.runAOTCompilation ? e5.aotTableSize : 1, s3 = t5 + n3 + 36 * r2 + 1, a2 = qs();
            let i3 = a2.length;
            const c2 = performance.now();
            a2.grow(s3);
            const l3 = performance.now();
            e5.enableStats && Ve(`Allocated ${s3} function table entries for jiterpreter, bringing total table size to ${a2.length}`), i3 = _a(0, i3, t5, ta("mono_jiterp_placeholder_trace")), i3 = _a(1, i3, n3, ta("mono_jiterp_placeholder_jit_call"));
            for (let e6 = 2; e6 <= 37; e6++) i3 = _a(e6, i3, r2, a2.get(o.mono_jiterp_get_interp_entry_func(e6)));
            const p3 = performance.now();
            e5.enableStats && Ve(`Growing wasm function table took ${l3 - c2}. Filling table took ${p3 - l3}.`);
          })(), (function() {
            if (!ct.mono_wasm_bindings_is_ready) {
              lt.diagnosticTracing && ze("bindings_init"), ct.mono_wasm_bindings_is_ready = true;
              try {
                const e5 = Ft();
                we || ("undefined" != typeof TextDecoder && (Se = new TextDecoder("utf-16le"), ve = new TextDecoder("utf-8", { fatal: false }), Ue = new TextDecoder("utf-8"), Te = new TextEncoder()), we = _(12)), Ee || (Ee = (function(e6) {
                  let t5;
                  if (de.length > 0) t5 = de.pop();
                  else {
                    const e7 = (function() {
                      if (null == le || !pe) {
                        le = _e(ce, "js roots"), pe = new Int32Array(ce), ue = ce;
                        for (let e9 = 0; e9 < ce; e9++) pe[e9] = ce - e9 - 1;
                      }
                      if (ue < 1) throw new Error("Out of scratch root space");
                      const e8 = pe[ue - 1];
                      return ue--, e8;
                    })();
                    t5 = new he(le, e7);
                  }
                  if (void 0 !== e6) {
                    if ("number" != typeof e6) throw new Error("value must be an address in the managed heap");
                    t5.set(e6);
                  } else t5.set(0);
                  return t5;
                })()), (function() {
                  const e6 = "System.Runtime.InteropServices.JavaScript";
                  if (ct.runtime_interop_module = o.mono_wasm_assembly_load(e6), !ct.runtime_interop_module) throw "Can't find bindings module assembly: " + e6;
                  if (ct.runtime_interop_namespace = e6, ct.runtime_interop_exports_classname = "JavaScriptExports", ct.runtime_interop_exports_class = o.mono_wasm_assembly_find_class(ct.runtime_interop_module, ct.runtime_interop_namespace, ct.runtime_interop_exports_classname), !ct.runtime_interop_exports_class) throw "Can't find " + ct.runtime_interop_namespace + "." + ct.runtime_interop_exports_classname + " class";
                  yn.InstallMainSynchronizationContext = void 0, yn.CallEntrypoint = Sn("CallEntrypoint"), yn.BindAssemblyExports = Sn("BindAssemblyExports"), yn.ReleaseJSOwnedObjectByGCHandle = Sn("ReleaseJSOwnedObjectByGCHandle"), yn.CompleteTask = Sn("CompleteTask"), yn.CallDelegate = Sn("CallDelegate"), yn.GetManagedStackTrace = Sn("GetManagedStackTrace"), yn.LoadSatelliteAssembly = Sn("LoadSatelliteAssembly"), yn.LoadLazyAssembly = Sn("LoadLazyAssembly");
                })(), 0 == vn.size && (vn.set(21, _n), vn.set(23, hn), vn.set(22, gn), vn.set(3, Wt), vn.set(4, Ht), vn.set(5, qt), vn.set(6, Gt), vn.set(7, Jt), vn.set(8, Xt), vn.set(9, Qt), vn.set(11, Yt), vn.set(12, Kt), vn.set(10, Zt), vn.set(15, pn), vn.set(16, un), vn.set(27, un), vn.set(13, dn), vn.set(14, fn), vn.set(17, tn), vn.set(18, tn), vn.set(20, on), vn.set(29, on), vn.set(28, on), vn.set(30, sn), vn.set(24, nn), vn.set(25, nn), vn.set(0, en), vn.set(1, en), vn.set(2, en), vn.set(26, en)), 0 == Un.size && (Un.set(21, vo), Un.set(23, To), Un.set(22, Eo), Un.set(3, no), Un.set(4, ro), Un.set(5, oo), Un.set(6, so), Un.set(7, ao), Un.set(8, io), Un.set(9, co), Un.set(10, lo), Un.set(11, po), Un.set(12, uo), Un.set(17, fo), Un.set(18, _o), Un.set(15, mo), Un.set(16, wo), Un.set(27, wo), Un.set(13, ko), Un.set(14, So), Un.set(20, yo), Un.set(28, yo), Un.set(29, yo), Un.set(24, bo), Un.set(25, bo), Un.set(0, go), Un.set(2, go), Un.set(1, go), Un.set(26, go)), ct._i52_error_scratch_buffer = _(4), Pt(e5, "mono.bindingsInit");
              } catch (e5) {
                throw He("Error in bindings_init", e5), e5;
              }
            }
          })(), ct.runtimeReady = true, ct.afterMonoStarted.promise_control.resolve(), ct.config.interpreterPgo && await Bc(), Pt(t4, "mono.startRuntime");
        } catch (e5) {
          throw He("start_runtime() failed", e5), lt.mono_exit(1, e5), e5;
        }
        var e4;
      })(), await (async function() {
        await ct.allAssetsInMemory.promise, ct.config.assets && (lt.actual_downloaded_assets_count != lt.expected_downloaded_assets_count && mt(false, `Expected ${lt.expected_downloaded_assets_count} assets to be downloaded, but only finished ${lt.actual_downloaded_assets_count}`), lt.actual_instantiated_assets_count != lt.expected_instantiated_assets_count && mt(false, `Expected ${lt.expected_instantiated_assets_count} assets to be in memory, but only instantiated ${lt.actual_instantiated_assets_count}`), lt._loaded_files.forEach(((e4) => lt.loadedFiles.push(e4.url))), lt.diagnosticTracing && ze("all assets are loaded in wasm memory"));
      })(), tl.registerRuntime(it), ct.mono_wasm_runtime_is_ready || (function mono_wasm_runtime_ready() {
        if (et.mono_wasm_runtime_is_ready = ct.mono_wasm_runtime_is_ready = true, vt = 0, St = {}, Ut = -1, globalThis.dotnetDebugger) debugger;
      })();
      try {
        e3(Ke);
      } catch (e4) {
        throw He("user callback onRuntimeInitialized() failed", e4), e4;
      }
      await (async function() {
        lt.diagnosticTracing && ze("mono_wasm_after_user_runtime_initialized");
        try {
          if (Ke.onDotnetReady) try {
            await Ke.onDotnetReady();
          } catch (e4) {
            throw He("onDotnetReady () failed", e4), e4;
          }
        } catch (e4) {
          throw He("mono_wasm_after_user_runtime_initialized () failed", e4), e4;
        }
      })(), Pt(t3, "mono.onRuntimeInitialized");
    } catch (e4) {
      throw Ke.runtimeKeepalivePop(), He("onRuntimeInitializedAsync() failed", e4), lt.mono_exit(1, e4), e4;
    }
    ct.afterOnRuntimeInitialized.promise_control.resolve();
  })(u2), e2.postRun = [() => (async function(e3) {
    try {
      await ct.afterOnRuntimeInitialized.promise, lt.diagnosticTracing && ze("postRunAsync");
      const t3 = Ft();
      Ke.FS_createPath("/", "usr", true, true), Ke.FS_createPath("/", "usr/share", true, true), e3.map(((e4) => e4(Ke))), Pt(t3, "mono.postRun");
    } catch (e4) {
      throw He("postRunAsync() failed", e4), lt.mono_exit(1, e4), e4;
    }
    ct.afterPostRun.promise_control.resolve();
  })(p2)], e2.ready.then((async () => {
    await ct.afterPostRun.promise, Pt(t2, "mono.emscriptenStartup"), ct.dotnetReady.promise_control.resolve(it);
  })).catch(((e3) => {
    ct.dotnetReady.promise_control.reject(e3);
  })), e2.ready = ct.dotnetReady.promise;
}
function Zc(e2, t2) {
  o.mono_wasm_setenv(e2, t2);
}
async function Kc() {
  void 0 !== lt.exitCode && 0 !== lt.exitCode || await Rc();
}
async function el(e2) {
}
let tl;
function nl(r2) {
  const o2 = Ke, a2 = r2, i2 = globalThis;
  Object.assign(a2.internal, { mono_wasm_exit: (e2) => {
    Ke.err("early exit " + e2);
  }, forceDisposeProxies: Jr, mono_wasm_dump_threads: void 0, logging: void 0, mono_wasm_stringify_as_error_with_stack: Qe, mono_wasm_get_loaded_files: As, mono_wasm_send_dbg_command_with_parms: Et, mono_wasm_send_dbg_command: xt, mono_wasm_get_dbg_command_info: It, mono_wasm_get_details: Nt, mono_wasm_release_object: Ct, mono_wasm_call_function_on: Bt, mono_wasm_debugger_resume: At, mono_wasm_detach_debugger: jt, mono_wasm_raise_debug_event: Lt, mono_wasm_change_debugger_log_level: $t, mono_wasm_debugger_attached: Rt, mono_wasm_runtime_is_ready: ct.mono_wasm_runtime_is_ready, mono_wasm_get_func_id_to_name_mappings: Ze, get_property: lr, set_property: cr, has_property: pr, get_typeof_property: ur, get_global_this: dr, get_dotnet_instance: () => it, dynamic_import: mr, mono_wasm_bind_cs_function: wr, ws_wasm_create: bs, ws_wasm_open: ys, ws_wasm_send: ws, ws_wasm_receive: ks, ws_wasm_close: Ss, ws_wasm_abort: vs, ws_get_state: gs, http_wasm_supports_streaming_request: $o, http_wasm_supports_streaming_response: Lo, http_wasm_create_controller: Ro, http_wasm_get_response_type: Mo, http_wasm_get_response_status: zo, http_wasm_abort: No, http_wasm_transform_stream_write: Oo, http_wasm_transform_stream_close: Co, http_wasm_fetch: Po, http_wasm_fetch_stream: Do, http_wasm_fetch_bytes: Fo, http_wasm_get_response_header_names: Vo, http_wasm_get_response_header_values: Wo, http_wasm_get_response_bytes: qo, http_wasm_get_response_length: Ho, http_wasm_get_streamed_response_bytes: Go, jiterpreter_dump_stats: $c, jiterpreter_apply_options: pa, jiterpreter_get_options: fa, interp_pgo_load_data: Bc, interp_pgo_save_data: Rc, mono_wasm_gc_lock: ae, mono_wasm_gc_unlock: ie, monoObjectAsBoolOrNullUnsafe: Fc, monoStringToStringUnsafe: Pe, loadLazyAssembly: Cc, loadSatelliteAssemblies: Dc });
  const c2 = { stringify_as_error_with_stack: Qe, instantiate_symbols_asset: Is, instantiate_asset: xs, jiterpreter_dump_stats: $c, forceDisposeProxies: Jr, utf8ToString: Le, mono_wasm_process_current_pid: Xc, mono_background_exec: () => s.mono_background_exec(), mono_wasm_ds_exec: () => s.mono_wasm_ds_exec() };
  Object.assign(ct, c2);
  const l2 = { runMain: Vc, runMainAndExit: zc, exit: lt.mono_exit, setEnvironmentVariable: Zc, getAssemblyExports: vr, setModuleImports: ir, getConfig: () => ct.config, invokeLibraryInitializers: lt.invokeLibraryInitializers, setHeapB32: b, setHeapB8: y, setHeapU8: w, setHeapU16: k, setHeapU32: S, setHeapI8: v, setHeapI16: U, setHeapI32: T, setHeapI52: x, setHeapU52: I, setHeapI64Big: A, setHeapF32: j, setHeapF64: $, getHeapB32: R, getHeapB8: B, getHeapU8: N, getHeapU16: O, getHeapU32: C, getHeapI8: M, getHeapI16: z, getHeapI32: V, getHeapI52: W, getHeapU52: H, getHeapI64Big: q, getHeapF32: G, getHeapF64: J, localHeapViewU8: K, localHeapViewU16: ee, localHeapViewU32: te, localHeapViewI8: X, localHeapViewI16: Q, localHeapViewI32: Y, localHeapViewI64Big: Z, localHeapViewF32: ne, localHeapViewF64: re, collectCpuSamples: null, collectMetrics: null, collectGcDump: null, connectDSRouter: null };
  return Object.assign(it, { INTERNAL: a2.internal, Module: o2, runtimeBuildInfo: { productVersion: e, gitHash: ct.gitHash, buildConfiguration: t, wasmEnableThreads: n, wasmEnableSIMD: true, wasmEnableExceptionHandling: true }, ...l2 }), i2.getDotnetRuntime ? tl = i2.getDotnetRuntime.__list : (i2.getDotnetRuntime = (e2) => i2.getDotnetRuntime.__list.getRuntime(e2), i2.getDotnetRuntime.__list = tl = new rl()), it;
}
class rl {
  constructor() {
    this.list = {};
  }
  registerRuntime(e2) {
    return void 0 === e2.runtimeId && (e2.runtimeId = Object.keys(this.list).length), this.list[e2.runtimeId] = yr(e2), lt.config.runtimeId = e2.runtimeId, e2.runtimeId;
  }
  getRuntime(e2) {
    const t2 = this.list[e2];
    return t2 ? t2.deref() : void 0;
  }
}
