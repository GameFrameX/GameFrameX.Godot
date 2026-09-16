# LeanCLR

Language: [中文](./README.md) | [English](./README_EN.md)

[![GitHub](https://img.shields.io/badge/GitHub-Repository-181717?logo=github)](https://github.com/focus-creative-games/leanclr) [![Gitee](https://img.shields.io/badge/Gitee-Repository-C71D23?logo=gitee)](https://gitee.com/focus-creative-games/leanclr)

[![license](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/focus-creative-games/leanclr/blob/main/LICENSE) [![DeepWiki](https://deepwiki.com/badge.svg)](https://deepwiki.com/focus-creative-games/leanclr) [![Discord](https://img.shields.io/badge/Discord-Join-7289DA?logo=discord&logoColor=white)](https://discord.gg/esAYcM6RDQ)

LeanCLR is a lean, cross-platform implementation of the CLR (Common Language Runtime). It targets strong alignment with **ECMA-335** while staying **compact**, **easy to embed**, and **low on memory**, for resource-constrained environments such as mobile clients, HTML5 runtimes, and mini-game platforms.

## Why LeanCLR?

Mature runtimes already exist (CoreCLR, Mono, IL2CPP). LeanCLR addresses a different set of constraints:

1. **Shipping size** — After publishing, CoreCLR, Mono, and IL2CPP often produce binaries on the order of several MB to tens of MB, which is a poor fit for resource-constrained targets such as mobile apps, H5, or mini-game runtimes.
2. **IL2CPP trade-offs** — IL2CPP is closed-source, effectively AOT-only, and offers comparatively limited ECMA-335 coverage.
3. **Maintainability and porting** — CoreCLR is very large; Mono’s architecture is dated. Both are hard to customize and hard to port to new platforms.

LeanCLR is designed from scratch around these goals:

- **Minimal footprint** — Single-threaded builds are under **600 KB** on x64/WebAssembly; aggressive trimming can reach **~300 KB**.
- **Strong portability** — Implemented in **standard C++11** with **no platform-specific dependencies**. An **AOT + interpreter** hybrid (no JIT) yields excellent portability: the same codebase runs on any platform with a conforming C++11 toolchain, without porting patches.
- **Straightforward integration** — Simple build flow: reference LeanCLR’s **CMakeLists.txt** module and you are most of the way there. The embedding API surface is intentionally smaller than CoreCLR or Mono.
- **Maintainable codebase** — Clear structure for reading, customizing, and extending.

Typical use cases:

- Mobile games and applications (iOS / Android)
- H5 games and mini-game platforms (e.g. WeChat, Douyin mini games)
- Game clients that need hot-update–friendly deployment
- Embedded systems and IoT devices

## Features and advantages

### Standards compatibility

- **High ECMA-335 coverage** — Near-complete spec compliance; overall completeness is **higher** than `IL2CPP + HybridCLR` and **lower** than Mono.
- **Modern C# surface** — Generics, exception handling, reflection, delegates, LINQ-related patterns, and other core features are supported.
- **CoreCLR extensions** (planned) — e.g. static abstract members in interfaces (.NET 7+).
- **Deliberately trimmed** — Only removes deprecated legacy features (e.g. `arglist`, `jmp`).

### Minimal footprint

- **Small binaries** — ~**600 KB** single-threaded; trimming the IR interpreter and non-essential icalls can shrink toward **300 KB** or lower.
- **Low memory use** — Optimized metadata representations; method-body metadata can be reclaimed on demand.
- **Fine-grained alignment** — Separate allocation pools sized to real metadata / managed-object alignment needs, avoiding a blanket 8-byte alignment tax.
- **Compact object headers** — Single-threaded builds use a one-pointer object header.

### Efficient, modern execution model

- **AOT + interpreter hybrid** — Balances startup, steady-state performance, and portability.
- **IL → C++ AOT** — IL is translated to generated C++.
- **Dual interpreters** — IL interpreter for cold paths; IR interpreter for hot code, trading compile work for runtime speed.
- **Per-method AOT** — Each managed method can be toggled independently for AOT. AOTing only performance-sensitive paths preserves behavior while shrinking binaries.
- **Interpreter-backed exception paths** — Exception handling is centralized in the interpreter, greatly reducing generated AOT bulk.

### Portability

- **Portable C++11** — Zero hard dependency on OS-specific APIs in the core story.
- **No mandatory C++ exceptions** — Can run in environments where C++ exceptions are disabled.
- **Low porting cost** — Targets any platform with a C++11 toolchain (Windows, Linux, macOS, iOS, Android, WebAssembly, and others).

## Documentation

The [docs](./docs) directory contains deeper material:

- [Documentation index](./docs/README.md) — structure and navigation
- [Build docs overview](./docs/build/README.md)
- [Building the runtime](./docs/build/build_runtime.md)
- [Embedding LeanCLR](./docs/build/embed_leanclr.md)
- [AOT](./docs/aot.md) — AOT capabilities and usage
- [Tests](./src/tests/README.md) — test harness and how to author cases

## Engine and platform integrations

The **LeanCLR runtime itself** is fully cross-platform. For convenience, we ship or track integrations on selected stacks:

| Platform | Status | Notes |
|----------|--------|-------|
| **Unity & Tuanjie — WebGL and mini games** | Done | [leanclr4unity](https://github.com/focus-creative-games/leanclr4unity) is a Unity package: when publishing games (not limited to WebGL / mini-game targets), replace IL2CPP with LeanCLR. |
| WeChat mini games / mini apps | Partial | [leanclr-sdk](https://github.com/focus-creative-games/leanclr-sdk) wraps a subset of APIs; you can build mini games and apps primarily in C#. |
| **Cocos Engine — native C# on all platforms** | In development | Release timeline TBD. |
| **Unreal Engine — native C# on all platforms** | In development | Release timeline TBD. |
| **Godot — native C# on all platforms** | In development | Release timeline TBD. |

## Project status

### Current progress

| Module | Status | Notes |
|--------|--------|-------|
| **Metadata loading** | Done | Full PE/COFF and CLI metadata tables |
| **Type system** | Done | Classes, interfaces, generics, arrays, value types, … |
| **IR interpreter** | Done | Hot-path–oriented execution |
| **Exception handling** | Done | try/catch/finally, nested cases, … |
| **Reflection** | Done | Type, MethodInfo, FieldInfo, … |
| **Delegates** | Done | Unicast / multicast, generic delegates |
| **Internal calls (icall)** | Done | |
| **P/Invoke** | Done | Manual registration; LeanAOT can auto-generate P/Invoke wrappers |
| **LeanAOT toolchain** | Done | IL → C++ translation supported |
| **Garbage collection** | In progress | Core framework in place |
| **Multithreading** | Planned | Threads, synchronization primitives, … |

### Stability

The current release has reached a **very high** stability bar for the scenarios we exercise:

- **Unity** — BCL matches Unity **2019.4.x – 6000.3.x LTS** IL2CPP expectations; **thousands** of tests pass end-to-end.
- **Mono** — **99.95%** BCL compatibility on Mono **4.8**; **one** known failing test.

## Editions

LeanCLR is offered in two editions — **Core** and **Standard** — for different deployment profiles.

### Core edition

Status: **Done**.

**Design goals:** minimize binary size and memory; maximize portability.

- **Single-threaded execution** — Simpler memory model; no cross-thread synchronization in the runtime core.
- **AOT + interpreter hybrid** — Flexible execution strategy.
- **Accurate cooperative GC** — Precise tracking of managed references; efficient reclamation.
- **Zero platform coupling** — No dependency on OS- or vendor-specific entry points in the core profile.
- **Standard C++11** — Builds on any toolchain that implements C++11.
- **Partial platform icalls** — Covers `System.IO`, `System.Net`, and other platform-facing areas, but **excludes** threading-related surface.

**Best for:** mobile apps, mobile games, WebAssembly, embedded / IoT, or any project that prioritizes footprint and cross-platform consistency over full BCL breadth.

### Standard edition

Status: **Planned**.

**Design goals:** a full-featured, production-grade runtime.

- **Multithreading** — Complete thread model and synchronization primitives.
- **AOT + interpreter hybrid** — Same architectural idea as Core.
- **Conservative GC** — Broader compatibility for complex workloads.
- **Full platform icalls** — `System.IO`, `System.Net`, and related stacks implemented against the host OS.
- **Standard C++11** — Porting to a new OS may require a small, explicit shim layer for platform APIs.

**Best for:** desktop applications, large mobile titles, or any project that needs closer parity with “full” .NET base libraries.

## Demos

### leanclr-demo

[leanclr-demo](https://github.com/focus-creative-games/leanclr-demo) provides two quickstarts:

| Sample | Description |
|--------|-------------|
| **win64** | Windows x64 — run `run.bat`. |
| **h5** | Browser WASM — serve `index.html` over HTTP. |

### leanclr4unity-demo

[leanclr4unity-demo](https://github.com/focus-creative-games/leanclr4unity_demo) shows how to use the **leanclr4unity** plugin so that when you publish WebGL, mini-game, Win64, or other targets, IL2CPP is replaced by the LeanCLR runtime.

## Related repositories

| Repository | Description |
|------------|-------------|
| [leanclr4unity](https://github.com/focus-creative-games/leanclr4unity) | LeanCLR runtime and toolchain (**C++11**, zero external dependencies). |
| [hybridclr](https://github.com/focus-creative-games/hybridclr) | **HybridCLR** — a feature-complete, **zero-intrinsic-cost**, high-performance, low-memory hot-update solution for native C# on all Unity platforms. |

## Contact

- Email: `leanclr#code-philosophy.com` (replace `#` with `@`)
- Discord: <https://discord.gg/esAYcM6RDQ>
- QQ group: **1047250380**
