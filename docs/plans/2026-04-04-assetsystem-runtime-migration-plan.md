# AssetSystem Runtime Migration Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Make `addons/com.gameframex.godot.assetsystem/Runtime` compilable and runnable in Godot C# without relying on Unity runtime APIs.

**Architecture:** Keep public YooAsset-facing API stable, replace Unity-specific runtime dependencies behind Godot-capable adapters, and migrate by vertical slices (`compile -> download -> resource -> scene -> integration`). Do not attempt 1:1 Unity behavior recreation for AssetBundle internals.

**Tech Stack:** Godot 4 C#, .NET 8, existing `Godot.csproj`, xUnit (`addons/com.gameframex.godot.asset/Tests/Unit`), PowerShell validation scripts.

## Execution Progress (Updated on April 4, 2026)

- [x] M1.1 Add compile switch and baseline report
- [x] M1.2 Isolate UnityEditor leakage from runtime compile path
- [x] M1.3 Include minimal compatibility symbols for switch-on compile
- [x] M2.1 Introduce HTTP transport adapter and wire web version/hash/manifest path
- [~] M2.2 Introduce resource backend adapter (provider loader + instantiate path routed; scene/backend lifecycle alignment pending L1.x)
- [~] L1.1 Scene lifecycle behavior alignment (compatibility runtime semantics + provider async scene rebinding completed; sample-scene runtime gate pending)
- [~] L1.2 ResourcePackage + manifest chain E2E in Godot (operation-level E2E harness + retry/fallback branch complete; runtime scene-level E2E pending)
- [~] L1.2-RT Runtime scene verifier prepared (scene/script/report completed; environment run pending actual Godot executable)

---

## Baseline (Captured on April 4, 2026)

- Branch baseline: `origin/main` at `43cdb7d`.
- When `assetsystem` compile exclusion is removed, build fails with 380 errors:
  - `CS0246`: 216
  - `CS0234`: 128
  - `CS0103`: 32
  - `CS0738`: 4
- Major hotspots:
  - `Runtime/ResourceManager/Provider`
  - `Runtime/ResourceManager/Handle`
  - `Runtime/ResourcePackage`
  - `Runtime/DownloadSystem`

---

## Scope and Non-Scope

### In scope
- `addons/com.gameframex.godot.assetsystem/Runtime/**`
- Build/test wiring for continuous validation
- Required minimal adapter interfaces and implementations for Godot runtime

### Out of scope (explicit)
- 1:1 behavior compatibility of Unity `AssetBundle` runtime internals
- Unity Editor-only tooling parity in this phase
- Migration of `ui.fairygui` and `ui.ugui` packages

---

## Mid-Term Plan (April 4, 2026 to April 25, 2026)

### Task M1.1: Add an explicit compile switch for assetsystem runtime

**Files:**
- Modify: `E:/Project_godot/gfx/GameFrameX.Godot/Godot.csproj`
- Create: `E:/Project_godot/gfx/GameFrameX.Godot/docs/reports/assetsystem-compile-baseline.md`

**Step 1: Add build property switch**
- Add `IncludeAssetSystemRuntime` (default `false`) in `Godot.csproj`.
- Gate `Compile Remove="addons/com.gameframex.godot.assetsystem/**/*.cs"` by this property.

**Step 2: Verify default build behavior is unchanged**
- Run: `dotnet build Godot.sln -v minimal`
- Expected: PASS, no regression from current baseline.

**Step 3: Verify switch-on behavior exposes real errors**
- Run: `dotnet build Godot.sln -v minimal -p:IncludeAssetSystemRuntime=true`
- Expected: FAIL with current assetsystem errors.

**Step 4: Record baseline report**
- Record error count and code distribution to `docs/reports/assetsystem-compile-baseline.md`.

**Step 5: Commit**
- `git add Godot.csproj docs/reports/assetsystem-compile-baseline.md`
- `git commit -m "chore(assetsystem): add runtime compile switch and baseline report"`

---

### Task M1.2: Remove UnityEditor leakage from Runtime compile surface

**Files:**
- Modify: `addons/com.gameframex.godot.assetsystem/Runtime/DiagnosticSystem/RemoteDebuggerInRuntime.cs`
- Modify: `addons/com.gameframex.godot.assetsystem/Runtime/FileSystem/DefaultBuildinFileSystem/DefaultBuildinFileSystemBuild.cs`
- Modify: `addons/com.gameframex.godot.assetsystem/Runtime/ResourceManager/Provider/DatabaseAllAssetsProvider.cs`
- Modify: `addons/com.gameframex.godot.assetsystem/Runtime/ResourceManager/Provider/DatabaseAssetProvider.cs`
- Modify: `addons/com.gameframex.godot.assetsystem/Runtime/ResourceManager/Provider/DatabaseSubAssetsProvider.cs`

**Step 1: Write failing compile check (switch on)**
- Run: `dotnet build Godot.sln -v minimal -p:IncludeAssetSystemRuntime=true`
- Expected: FAIL, includes Runtime references to `UnityEditor`.

**Step 2: Move Editor-only code behind strict compile guards or split files**
- Keep Runtime code free from direct `UnityEditor` symbol use in active compile path.

**Step 3: Re-run compile check**
- Run: `dotnet build Godot.sln -v minimal -p:IncludeAssetSystemRuntime=true`
- Expected: `CS0234` (UnityEditor-related) reduced to zero.

**Step 4: Commit**
- `git add <modified files>`
- `git commit -m "refactor(assetsystem): isolate UnityEditor-only runtime paths"`

---

### Task M1.3: Stabilize minimal Unity compatibility surface needed for compile

**Files:**
- Modify: `addons/com.gameframex.godot.assetsystem/Runtime/Compatibility/UnityCompatibilityPlaceholders.cs`
- Modify: `E:/Project_godot/gfx/GameFrameX.Godot/Godot.csproj`

**Step 1: Ensure compatibility placeholders are included for switch-on builds**
- Remove/adjust exclusion for `Runtime/Compatibility/**/*.cs` when `IncludeAssetSystemRuntime=true`.

**Step 2: Add only required missing symbols**
- Prioritize symbols that unblock compile only: `LoadSceneMode`, `LocalPhysicsMode`, `LoadSceneParameters`, networking placeholders.

**Step 3: Compile gate**
- Run: `dotnet build Godot.sln -v minimal -p:IncludeAssetSystemRuntime=true`
- Expected: total errors reduced by at least 40% from baseline.

**Step 4: Commit**
- `git add Godot.csproj addons/com.gameframex.godot.assetsystem/Runtime/Compatibility/UnityCompatibilityPlaceholders.cs`
- `git commit -m "feat(assetsystem): extend minimal compatibility symbols for compile"`

---

### Task M2.1: Introduce HTTP adapter to replace UnityWebRequest usage path

**Files:**
- Create: `addons/com.gameframex.godot.assetsystem/Runtime/Services/IHttpTransport.cs`
- Create: `addons/com.gameframex.godot.assetsystem/Runtime/Services/HttpResponse.cs`
- Create: `addons/com.gameframex.godot.assetsystem/Runtime/DownloadSystem/GodotHttpTransport.cs`
- Modify: `addons/com.gameframex.godot.assetsystem/Runtime/DownloadSystem/DownloadSystemHelper.cs`
- Modify: `addons/com.gameframex.godot.assetsystem/Runtime/DownloadSystem/*.cs` (incremental callers)

**Step 1: Add adapter interfaces and null implementation tests**
- Create tests in `addons/com.gameframex.godot.asset/Tests/Unit` for adapter contract behavior.

**Step 2: Route one download operation through adapter first**
- Use smallest vertical path (text/version request first).

**Step 3: Expand to file download paths**
- Replace direct `UnityWebRequest` construction points.

**Step 4: Verification**
- Run:
  - `dotnet build Godot.sln -v minimal -p:IncludeAssetSystemRuntime=true`
  - `dotnet test addons/com.gameframex.godot.asset/Tests/Unit/GameFrameX.Asset.UnitTests.csproj -v minimal`
- Expected: `CS0246` for `UnityWebRequest*` reaches zero.

**Step 5: Commit**
- `git commit -m "feat(assetsystem): introduce Godot HTTP transport adapter"`

---

### Task M2.2: Introduce resource backend adapter for load/instantiate paths

**Files:**
- Create: `addons/com.gameframex.godot.assetsystem/Runtime/Services/IResourceBackend.cs`
- Create: `addons/com.gameframex.godot.assetsystem/Runtime/ResourceManager/Backend/GodotResourceBackend.cs`
- Modify: `addons/com.gameframex.godot.assetsystem/Runtime/ResourceManager/Provider/ProviderOperation.cs`
- Modify: `addons/com.gameframex.godot.assetsystem/Runtime/ResourceManager/Provider/Bundled*.cs`
- Modify: `addons/com.gameframex.godot.assetsystem/Runtime/ResourceManager/Handle/*.cs`

**Step 1: Add backend interface and fake backend tests**
- Test contract: load success/failure/cancel semantics.

**Step 2: Move provider calls from `AssetBundle` APIs to backend abstraction**
- Keep public handle APIs unchanged.

**Step 3: Verify compile + tests**
- Run same two commands as M2.1.
- Expected: `CS0246` for `AssetBundle*` and most `GameObject/Transform` references reduced to backend boundary only.

**Step 4: Commit**
- `git commit -m "refactor(assetsystem): route providers through resource backend adapter"`

---

### Task M3.1: Mid-term integration gate

**Files:**
- Create: `docs/reports/assetsystem-midterm-gate-2026-04-25.md`
- Modify: `YooAsset_Godot_迁移实施计划.md`

**Step 1: Enable runtime compile switch in CI/local command profile**
- Build command: `dotnet build Godot.sln -v minimal -p:IncludeAssetSystemRuntime=true`

**Step 2: Validate zero compile errors for runtime target slice**
- If not zero, block phase exit and list remaining errors by code and file.

**Step 3: Record gate result**
- Write pass/fail and residual risks.

**Step 4: Commit**
- `git commit -m "docs(assetsystem): record mid-term migration gate"`

---

## Late-Term Plan (April 26, 2026 to May 23, 2026)

### Task L1.1: Scene and lifecycle behavior alignment in Godot runtime

**Files:**
- Modify: `addons/com.gameframex.godot.assetsystem/Runtime/ResourceManager/Provider/BundledSceneProvider.cs`
- Modify: `addons/com.gameframex.godot.assetsystem/Runtime/ResourceManager/Handle/SceneHandle.cs`
- Modify: `addons/com.gameframex.godot.assetsystem/Runtime/ResourceManager/Operation/UnloadSceneOperation.cs`

**Steps:**
1. Add failing tests for load/unload/cancel scene semantics.
2. Implement minimal Godot-consistent scene transition behavior.
3. Run build + tests.
4. Commit.

**Gate:** scene load/unload path works in a sample Godot scene.

---

### Task L1.2: ResourcePackage + manifest chain E2E in Godot

**Files:**
- Modify: `addons/com.gameframex.godot.assetsystem/Runtime/ResourcePackage/*.cs`
- Modify: `addons/com.gameframex.godot.assetsystem/Runtime/FileSystem/**/*.cs`
- Create: `docs/reports/assetsystem-e2e-checklist.md`

**Steps:**
1. Add E2E test harness (`init -> version -> manifest -> download -> load`).
2. Implement missing error recovery branches (timeout/retry/manifest fallback).
3. Run E2E test + build.
4. Commit.

**Gate:** E2E path stable for 3 consecutive runs without manual intervention.

---

### Task L2.1: Remove permanent compile exclusion and finalize

**Files:**
- Modify: `E:/Project_godot/gfx/GameFrameX.Godot/Godot.csproj`
- Modify: `Unity2Godot_迁移标准模板.md`
- Modify: `YooAsset_Godot_迁移实施计划.md`

**Steps:**
1. Remove hard exclusion of `addons/com.gameframex.godot.assetsystem/**/*.cs`.
2. Run full build and test matrix:
   - `dotnet build Godot.sln -v minimal`
   - `dotnet build Godot.sln -v minimal -p:IncludeAssetSystemRuntime=true`
   - `dotnet test addons/com.gameframex.godot.asset/Tests/Unit/GameFrameX.Asset.UnitTests.csproj -v minimal`
3. Update migration docs with final status and unresolved items.
4. Commit.

**Gate:** no compile exclusion needed for assetsystem runtime, all required gates pass.

---

## Dependency Map (Must-have for completion)

- **Component dependency A:** Godot HTTP capability abstraction (`IHttpTransport`) must be accepted by download pipeline.
- **Component dependency B:** Resource backend abstraction (`IResourceBackend`) must be accepted by Provider/Handle layers.
- **Component dependency C:** Runtime test harness must include assetsystem migration checks (not only asset package tests).

If any dependency is missing, migration cannot be marked complete.

---

## Completion Definition (Executable DoD)

Migration is complete only when all are true:

- [ ] `assetsystem` runtime compiles with no package-wide exclusion.
- [ ] `dotnet build Godot.sln -v minimal` passes.
- [ ] `dotnet build Godot.sln -v minimal -p:IncludeAssetSystemRuntime=true` passes.
- [ ] Unit tests for migration adapters and E2E path pass.
- [ ] Migration docs updated with final evidence and residual risk list.

---

## Risks and Decisions to Escalate Early

- If compile errors after M1.3 remain > 150, stop and re-slice by submodule before M2.
- If backend adapter introduces API break in public handles, stop and design compatibility facade first.
- If E2E test cannot run due to missing runtime sample assets, create a minimal reproducible asset fixture before further code changes.
