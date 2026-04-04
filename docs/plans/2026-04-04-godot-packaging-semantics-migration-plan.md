# Godot Packaging Semantics Migration Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Migrate package/build semantics from Unity-style AssetBundle assumptions to Godot-oriented package semantics while keeping the existing YooAsset runtime pipeline usable.

**Architecture:** Keep YooAsset's version/manifest/download/cache pipeline, but switch the build side to Godot-friendly outputs (RawFile-first), and switch runtime asset consumption to `ResourceLoader`-based loading for Godot resources. Keep Unity compatibility members only as a temporary shim layer, then shrink them in P1.

**Tech Stack:** Godot 4.x C#, YooAsset runtime/editor modules, local file HTTP transport, existing Procedure pipeline logs (`[LoadProbe]`).

---

## Baseline (2026-04-04)

- Runtime verification pipeline is green up to remote image stage (`rev=2026-04-04-r6`):
  - `Initializing -> RequestingVersion -> UpdatingManifest -> Downloading -> LoadingAsset -> ShowingPreview -> ShowingRemoteImage -> Completed`
- Current "AB success" is based on runtime fixture package (`runtime_verify`) and compatibility placeholders.
- Build side is still Unity-semantics-heavy (`UnityEditor.BuildTarget`, `BuildPipeline.BuildAssetBundles`, `StreamingAssets` conventions).

## Scope

- In scope:
  - Godot-oriented package output layout.
  - Default build pipeline to RawFile/Godot-file semantics.
  - Runtime loading path for Godot resources from downloaded/local files.
  - End-to-end logs and verification checklist.
- Out of scope (this plan):
  - Full removal of every Unity compatibility symbol in one pass.
  - Replacing all legacy editor tooling outside asset packaging path.

## Execution Rules (for interruption safety)

- Always continue from the first unchecked task.
- Do not reorder tasks unless the dependency says so.
- After each task, update the checkbox and append one-line result under "Execution Log".
- If interrupted, reopen this file and resume from "Current Resume Point".

## Current Resume Point

- `Task 4 / Negative-path evidence collection (and GodotPck runtime evidence)`

## Task Checklist

### Task 1: Lock default packaging semantics to Godot-file pipeline (P0)

**Files:**
- Modify: `addons/com.gameframex.godot.assetsystem/Editor/AssetBundleBuilder/EBuildPipeline.cs`
- Modify: `addons/com.gameframex.godot.assetsystem/Editor/AssetBundleBuilder/AssetBundleBuilderSetting.cs`
- Modify: `addons/com.gameframex.godot.assetsystem/Editor/AssetBundleBuilder/AssetBundleBuilderWindow.cs`
- Modify: `addons/com.gameframex.godot.assetsystem/Editor/AssetBundleBuilder/VisualViewers/RawfileBuildpipelineViewer.cs`

**Steps:**
1. [x] Add/alias a Godot-first pipeline enum name (keep old enum for compatibility).
2. [x] Set default build pipeline selection to Godot-file pipeline.
3. [x] Update builder UI labels/menu to present Godot-first naming.
4. [x] Keep compatibility fallback so old saved prefs still map correctly.

**Verification:**
- Build window opens and default pipeline is Godot-file semantics.
- No compile errors in editor/runtime assemblies.

---

### Task 2: Replace Unity-style output directory semantics with Godot layout (P0)

**Files:**
- Modify: `addons/com.gameframex.godot.assetsystem/Editor/AssetBundleBuilder/AssetBundleBuilderHelper.cs`
- Modify: `addons/com.gameframex.godot.assetsystem/Editor/AssetBundleBuilder/BuildParameters.cs`
- Modify: `addons/com.gameframex.godot.assetsystem/Editor/AssetBundleBuilder/BuildParametersContext.cs` (if needed for helper access)

**Target layout (example):**
- Build root: `<project>/Builds/Godot/<platform>/<package>/<version>/`
- Builtin copy root: `<project>/Builds/GodotBuiltin/<package>/`

**Steps:**
1. [x] Change default build output root from `Bundles` to `Builds/Godot`.
2. [x] Change builtin root from Unity `StreamingAssets` style to Godot-friendly project path.
3. [x] Ensure path join logic is platform-safe and uses normalized separators.
4. [x] Keep old path constants only as optional fallback, not defaults.

**Verification:**
- One build run outputs to `Builds/Godot/...` instead of Unity-style defaults.
- Manifest/hash/version files still generated correctly.

---

### Task 3: Make runtime consume Godot resource files as first-class path (P0)

**Files:**
- Modify: `addons/com.gameframex.godot.assetsystem/Runtime/YooAssetsExtension.cs`
- Modify: `addons/com.gameframex.godot.assetsystem/Runtime/ResourceManager/Handle/RawFileHandle.cs` (if API extension needed)
- Add or Modify: `addons/com.gameframex.godot.assetsystem/Runtime/ResourceManager/Backend/GodotResourceBackend.cs`
- Modify: `Scripts/Framework/Procedure/ProcedureDownloadWebFiles.cs` (verification branch)

**Steps:**
1. [x] Add extension method for "load raw file then `ResourceLoader.Load`".
2. [x] Support common Godot resource types in verification (`Texture2D`, `PackedScene` minimal path).
3. [x] Add `[LoadProbe][GodotRaw]` logs for request, path, result type.
4. [x] Keep existing compatibility path working while introducing Godot-first path.

**Verification:**
- Runtime log includes `GodotRaw PASS`.
- A downloaded/local resource is displayed/instantiated through Godot APIs.

---

### Task 4: Stabilize E2E verification artifacts and log contract (P0)

**Files:**
- Modify: `Scripts/Framework/Procedure/ProcedureDownloadWebFiles.cs`
- Modify: `Scripts/Verification/AssetSystemRuntimeVerifier.cs`
- Modify: `docs/reports/assetsystem-e2e-checklist.md`

**Required log contract:**
- `[LoadProbe][Stage] Initializing`
- `[LoadProbe][Stage] RequestingVersion`
- `[LoadProbe][Stage] UpdatingManifest`
- `[LoadProbe][Stage] Downloading`
- `[LoadProbe][AB] PASS ...` or equivalent compatibility marker
- `[LoadProbe][GodotRaw] PASS ...`
- `[LoadProbe][RemoteImage] PASS ...`
- `[LoadProbe][Stage] Completed (...)`

**Steps:**
1. [x] Ensure each stage has deterministic pass/fail/timeout exit logs.
2. [x] Keep `rev=` marker updated each patch revision.
3. [x] Update checklist doc with exact expected log lines.
4. [x] Add one negative-path checklist (missing file / HTTP fail).

**Verification:**
- Two full runs: one success, one intentional failure, both with deterministic stage closure.

---

### Task 5: Reduce Unity compatibility surface in runtime loading path (P1)

**Files:**
- Modify: `addons/com.gameframex.godot.assetsystem/Runtime/Services/IResourceBackend.cs`
- Modify: `addons/com.gameframex.godot.assetsystem/Runtime/ResourceManager/Provider/ProviderOperation.cs`
- Modify: `addons/com.gameframex.godot.assetsystem/Runtime/Compatibility/UnityCompatibilityPlaceholders.cs`
- Modify: `addons/com.gameframex.godot.assetsystem/Runtime/ResourceManager/Operation/InstantiateOperation.cs`

**Steps:**
1. [ ] Introduce Godot-oriented backend interface path (do not break existing callers in one patch).
2. [ ] Replace high-frequency runtime load/instantiate path with Godot-native object handling.
3. [ ] Keep placeholder compatibility only for legacy code paths not yet migrated.
4. [ ] Add guard comments/todo links for remaining Unity-only APIs.

**Verification:**
- No regression in current runtime verification flow.
- Reduced direct dependency on `UnityEngine.AssetBundle` placeholder path.

---

### Task 6: Documentation sync and migration status updates (P0/P1 gate)

**Files:**
- Modify: `YooAsset_Godot_迁移实施计划.md`
- Modify: `Unity2Godot_Package_Migration_Plan.md` (if present in current branch)
- Modify: `docs/reports/assetsystem-runtime-scene-verification.md`
- Modify: `docs/reports/assetsystem-compile-baseline.md`

**Steps:**
1. [ ] Mark completed tasks and attach exact evidence (command + key log line).
2. [ ] Record remaining blockers and dependency owners.
3. [ ] Add "can resume from task X" checkpoint in each planning doc.
4. [ ] Keep terminology consistent: "Godot-file pipeline", "compatibility shim", "runtime fixture".

**Verification:**
- All plan/report docs reflect latest task status and next action.

---

## Dependency Map

- Task 1 -> Task 2 -> Task 3 -> Task 4
- Task 5 depends on Task 3 stable.
- Task 6 runs after each task completion.

## Minimal Command Set Per Task

- Build:
  - `dotnet build Godot.sln -v minimal`
  - `dotnet build Godot.sln -v minimal -p:IncludeAssetSystemRuntime=false`
- Unit tests:
  - `dotnet test addons/com.gameframex.godot.asset/Tests/Unit/GameFrameX.Asset.UnitTests.csproj -v minimal`
- Runtime verification:
  - Run Godot project and collect `[LoadProbe]` logs.

## Done Definition

- P0 done when:
  - Default packaging semantics are Godot-file oriented.
  - Output directories are Godot layout.
  - Runtime can load at least one real Godot resource via raw-file path.
  - E2E logs are deterministic and checklist updated.
- P1 done when:
  - Runtime loading path no longer depends on Unity AssetBundle placeholder as primary route.
  - Compatibility surface is documented and minimized.

## Execution Log

- 2026-04-04: Document created. Next action: Task 1 / Step 1.
- 2026-04-04: Task 1 completed. Added `GodotFileBuildPipeline` alias, switched default selection to Godot-first mapping, updated window menu labels, and kept legacy value compatibility via normalization.
- 2026-04-04: Task 2 completed. Build output default switched to `Builds/Godot`, builtin root switched to `Builds/GodotBuiltin`, path joins normalized with `Path.Combine`, and optional legacy Unity path layout fallback added.
- 2026-04-04: Task 3 completed. Added `LoadGodotResourceFromRawFile*` extension methods (including typed `Texture2D`/`PackedScene` helpers) and hooked `[LoadProbe][GodotRaw]` verification logs with compatibility fallback.
- 2026-04-04: Task 4 code/doc updates completed (`[LoadProbe]` deterministic stage logs, `rev` updates, E2E checklist + negative-path checklist). Pending: runtime evidence collection from actual Godot run logs.
- 2026-04-04: Task 4 success-path evidence collected from runtime logs (`Completed`, `AB PASS`, `GodotRaw PASS fallback`, `RemoteImage PASS`). Remaining for Task 4 closure: one negative-path runtime evidence sample.
- 2026-04-04: Added Godot `PCK` runtime path in YooAsset extension (`MountGodotResourcePackFromRawFile*`) and wired probe log branch `[LoadProbe][GodotPck]` (PASS/FAIL/SKIP). Current fixture has no `verify_content.pck` manifest entry, so expected default behavior is `SKIP missing location`.
- 2026-04-04: Upgraded probe fixture to `r8`: runtime now attempts to generate `verify_content.pck` via `PckPacker`, injects it into manifest when available, and enables end-to-end `GodotPck` mount/load verification in the same pipeline.
- 2026-04-04: Copied external image `E:/Project_godot/Godot_TeamGame/Assets 2(Scale x2)-No-BG.png` into project (`Assets/Probe/teamgame_external.png`) and upgraded probe to `r9` so PCK packaging prefers this asset as source.
- 2026-04-04: Fixed `GodotPck` mount failure in probe `r10` by adding direct-path fallback mount (`MountGodotResourcePackByPath`) when raw-file mount path is unavailable under AB pipeline; added on-screen preview node for mounted PCK texture.
- 2026-04-04: Upgraded probe to `r11`: PCK build now writes internal path as `probe_runtime/teamgame_external.png` and `GodotPck` adds `ResourceLoader -> FileAccess+Image` fallback decode path with explicit fail-stage logs.
- 2026-04-04: Upgraded probe to `r12`: `GodotPck` now probes both path semantics (`res://probe_runtime/...` and `probe_runtime/...`) when mounted PCK cannot be loaded by a single canonical path.
- 2026-04-04: Added Godot editor-side asset builder entry (`GameFrameX` toolbar menu -> `Asset Builder`) and implemented `AssetSystemBuilderDialog` (source/mount/output inputs + one-click `PckPacker` build + output folder open + build logs). Verification: `dotnet build Godot.sln -v minimal` PASS.
- 2026-04-04: Single-package build + runtime verification closed as PASS. Evidence:
  - Builder output: `PASS source=.../Assets/Probe/teamgame_external.png`, `PASS output=.../verify_content.pck bytes=99792`, `PASS mount=probe_runtime/teamgame_external.png`.
  - Runtime output (`r12`): `[LoadProbe][GodotPck] PASS path=res://probe_runtime/teamgame_external.png type=ImageTexture bytes=99583`, terminal `[LoadProbe][Stage] Completed (remote image shown)`.
  - Builder default output path aligned to runtime fixture path: `user://assetsystem_runtime_verify/yoo/runtime_verify/verify_content.pck`.
- 2026-04-04: Added package-list mode to `AssetSystemBuilderDialog`:
  - New `Package List Mode (JSON)` area with template + `Build Batch` button.
  - Supports multi-package / multi-file PCK build in one run (`packages[].files[]`).
  - Added batch summary result (`success=N fail=M`) and per-package pass/fail logs.
  - Verification: `dotnet build Godot.sln -v minimal` PASS.
- 2026-04-04: Added config persistence for package-list mode:
  - Added `Save` button to write current package config to local disk.
  - `Build All` now auto-saves config before build.
  - Save outputs:
    - root config: `user://assetsystem_builder/package_list_config.json`
    - per-package configs: `user://assetsystem_builder/packages/<packageName>.json`
  - Verification: `dotnet build Godot.sln -v minimal` PASS.
- 2026-04-04: Updated `AssetSystemBuilderDialog` layout to editor-friendly split mode:
  - Left column now lists packable project assets (with refresh and count).
  - Added `Use Selected As Source` action and double-click activation to fill `Source`/`Mount` inputs.
  - Right column keeps build controls, package-list JSON area, and logs.
  - Verification: `dotnet build Godot.sln -v minimal` PASS.
