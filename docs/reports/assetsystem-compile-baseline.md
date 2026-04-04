# AssetSystem Compile Baseline (2026-04-04)

## Build Commands
- `dotnet build Godot.sln -v minimal`
- `dotnet build Godot.sln -v minimal -p:IncludeAssetSystemRuntime=true`

## Result Summary
- Default build exit code: 0
- Switch-on build exit code: 1
- Switch-on compile error count: 380

## Error Code Distribution (switch-on)
- CS0246: 216
- CS0234: 128
- CS0103: 32
- CS0738: 4

## Top Error Directories (switch-on)
- E:\Project_godot\gfx\GameFrameX.Godot\addons\com.gameframex.godot.assetsystem\Runtime\ResourceManager\Provider: 76
- E:\Project_godot\gfx\GameFrameX.Godot\addons\com.gameframex.godot.assetsystem\Runtime\ResourceManager\Handle: 68
- E:\Project_godot\gfx\GameFrameX.Godot\addons\com.gameframex.godot.assetsystem\Runtime: 52
- E:\Project_godot\gfx\GameFrameX.Godot\addons\com.gameframex.godot.assetsystem\Runtime\ResourcePackage: 50
- E:\Project_godot\gfx\GameFrameX.Godot\addons\com.gameframex.godot.assetsystem\Runtime\ResourceManager\Operation: 28
- E:\Project_godot\gfx\GameFrameX.Godot\addons\com.gameframex.godot.assetsystem\Runtime\DownloadSystem: 22
- E:\Project_godot\gfx\GameFrameX.Godot\addons\com.gameframex.godot.assetsystem\Runtime\DownloadSystem\Operation\Internal: 16
- E:\Project_godot\gfx\GameFrameX.Godot\addons\com.gameframex.godot.assetsystem\Runtime\FileSystem\DefaultCacheFileSystem\Operation\internal: 8
- E:\Project_godot\gfx\GameFrameX.Godot\addons\com.gameframex.godot.assetsystem\Runtime\FileSystem\DefaultWebFileSystem\Operation\internal: 8
- E:\Project_godot\gfx\GameFrameX.Godot\addons\com.gameframex.godot.assetsystem\Runtime\FileSystem\DefaultBuildinFileSystem: 6

## Notes
- This baseline is used for M1/M2 migration progress tracking.
- Detailed build log: `assetsystem_build_switch_on.log` (temporary local file).

## Update (M1.1 + M1.3 Applied on 2026-04-04)

### Changes
- Added project switch in `Godot.csproj`:
  - `<IncludeAssetSystemRuntime>false</IncludeAssetSystemRuntime>`
- Gated assetsystem runtime exclusion:
  - `<Compile Remove="addons/com.gameframex.godot.assetsystem/**/*.cs" Condition="'$(IncludeAssetSystemRuntime)' != 'true'" />`
- Gated assetsystem compatibility exclusion:
  - `<Compile Remove="addons/com.gameframex.godot.assetsystem/Runtime/Compatibility/**/*.cs" Condition="'$(IncludeAssetSystemRuntime)' != 'true'" />`

### Re-check Results
- `dotnet build Godot.sln -v minimal`: PASS (0 errors)
- `dotnet build Godot.sln -v minimal -p:IncludeAssetSystemRuntime=true`: PASS (0 errors)
- `dotnet test addons/com.gameframex.godot.asset/Tests/Unit/GameFrameX.Asset.UnitTests.csproj -v minimal`: PASS (24 passed)

## Update (M2.1 Applied on 2026-04-04)

### Changes
- Added HTTP transport abstraction:
  - `Runtime/Services/IHttpTransport.cs`
  - `Runtime/Services/HttpResponse.cs`
- Added Godot-based transport implementation:
  - `Runtime/DownloadSystem/GodotHttpTransport.cs`
- Added one vertical migration request operation:
  - `Runtime/DownloadSystem/Operation/Internal/HttpTextRequestOperation.cs`
- Wired `RequestWebPackageVersionOperation` to prefer `HttpTextRequestOperation` when `DownloadSystemHelper.HttpTransport` is available, with legacy `UnityWebTextRequestOperation` fallback.
- Added runtime configuration entry:
  - `YooAssets.SetDownloadSystemHttpTransport(IHttpTransport transport)`

### Re-check Results
- `dotnet build Godot.sln -v minimal`: PASS (0 errors)
- `dotnet build Godot.sln -v minimal -p:IncludeAssetSystemRuntime=true`: PASS (0 errors)
- `dotnet test addons/com.gameframex.godot.asset/Tests/Unit/GameFrameX.Asset.UnitTests.csproj -v minimal`: PASS (24 passed)

## Update (M2.2 Partial Applied on 2026-04-04)

### Changes
- Added resource backend abstraction:
  - `Runtime/Services/IResourceBackend.cs`
  - `Runtime/ResourceManager/Backend/GodotResourceBackend.cs`
- Routed provider-side bundle loader creation through backend:
  - `Runtime/ResourceManager/Provider/ProviderOperation.cs` (`BundleAssetLoaderFactory.Backend`)
- Routed instantiate/destroy path through backend:
  - `Runtime/ResourceManager/Operation/InstantiateOperation.cs`
- Added runtime internal backend configuration entry:
  - `Runtime/YooAssets.cs` (`SetResourceBackend` internal)
- Added trimming-keep references for new backend types:
  - `Runtime/YooAssetCroppingHelper.cs`

### Re-check Results
- `dotnet build Godot.sln -v minimal`: PASS (0 errors)
- `dotnet build Godot.sln -v minimal -p:IncludeAssetSystemRuntime=true`: PASS (0 errors)
- `dotnet test addons/com.gameframex.godot.asset/Tests/Unit/GameFrameX.Asset.UnitTests.csproj -v minimal`: PASS (24 passed)

## Update (L1.1 Partial Applied on 2026-04-04)

### Changes
- Added scene lifecycle compatibility tests (RED -> GREEN):
  - `addons/com.gameframex.godot.asset/Tests/Unit/UnitySceneManagementCompatibilityTests.cs`
  - Covered additive load count, unload count rollback, and active-scene failure after unload.
- Improved `UnityCompatibilityPlaceholders.SceneManager` scene lifecycle behavior:
  - Tracks loaded scenes and active scene state.
  - Supports additive/single load semantics and unload state changes.
  - Added `GetSceneByName` for async scene completion resolution.
- Updated scene providers to rebind scene object by name after async completion:
  - `Runtime/ResourceManager/Provider/BundledSceneProvider.cs`
  - `Runtime/ResourceManager/Provider/DatabaseSceneProvider.cs`
- Hardened unload flow null handling:
  - `Runtime/ResourceManager/Operation/UnloadSceneOperation.cs`

### Re-check Results
- `dotnet test addons/com.gameframex.godot.asset/Tests/Unit/GameFrameX.Asset.UnitTests.csproj -v minimal --filter "FullyQualifiedName~UnitySceneManagementCompatibilityTests"`: PASS (3 passed)
- `dotnet test addons/com.gameframex.godot.asset/Tests/Unit/GameFrameX.Asset.UnitTests.csproj -v minimal`: PASS (27 passed)
- `dotnet build Godot.sln -v minimal`: PASS (0 errors)
- `dotnet build Godot.sln -v minimal -p:IncludeAssetSystemRuntime=true`: PASS (0 errors)

## Update (L1.2 Partial Applied on 2026-04-04)

### Changes
- Added operation-level E2E harness tests:
  - `addons/com.gameframex.godot.asset/Tests/Unit/AssetSystemPipelineOperationTests.cs`
  - Covered chain: `init -> version -> manifest -> download -> load`
- Added retry behavior for remote version request:
  - `Runtime/ResourcePackage/Operation/RequestPackageVersionOperation.cs`
  - First failure retries once before final fail.
- Fixed local manifest fallback path:
  - `Runtime/ResourcePackage/Operation/LoadLocalManifestOperation.cs`
  - Buildin fallback now uses `LoadLocalPackageManifestAsync` (local) instead of remote manifest request.
- Added E2E checklist report:
  - `docs/reports/assetsystem-e2e-checklist.md`

### Re-check Results
- `dotnet test addons/com.gameframex.godot.asset/Tests/Unit/GameFrameX.Asset.UnitTests.csproj -v minimal --filter "FullyQualifiedName~AssetSystemPipelineOperationTests"`: PASS (3 passed)
- `dotnet test addons/com.gameframex.godot.asset/Tests/Unit/GameFrameX.Asset.UnitTests.csproj -v minimal`: PASS (30 passed)
- `dotnet build Godot.sln -v minimal`: PASS (0 errors)
- `dotnet build Godot.sln -v minimal -p:IncludeAssetSystemRuntime=true`: PASS (0 errors)
- E2E harness stability run (same pipeline test, 3 consecutive runs): PASS / PASS / PASS

## Update (L1.2-RT Partial Applied on 2026-04-04)

### Changes
- Added runtime scene verifier script:
  - `Scripts/Verification/AssetSystemRuntimeVerifier.cs`
  - Executes chain: `init -> version -> manifest -> download -> load(scene) -> unload(scene)`.
  - Auto-generates local fixture files (version/hash/manifest bytes) for a deterministic run.
- Added verifier scene:
  - `Scenes/Verification/AssetSystemRuntimeVerifier.tscn`
- Added compile symbol guard:
  - `Godot.csproj` adds `INCLUDE_ASSETSYSTEM_RUNTIME` only when `IncludeAssetSystemRuntime=true`.
  - Default build remains unaffected.
- Added runtime verification report:
  - `docs/reports/assetsystem-runtime-scene-verification.md`

### Re-check Results
- `dotnet build Godot.sln -v minimal`: PASS (0 errors)
- `dotnet build Godot.sln -v minimal -p:IncludeAssetSystemRuntime=true`: PASS (0 errors)
- `dotnet test addons/com.gameframex.godot.asset/Tests/Unit/GameFrameX.Asset.UnitTests.csproj -v minimal`: PASS (30 passed)
- Scene execution in this environment: NOT RUN (no `godot4/godot` executable available).

## Update (L1.2-RT Load-Modes Extension on 2026-04-04)

### Changes
- Added compatibility unit tests for resource loading placeholders:
  - `addons/com.gameframex.godot.asset/Tests/Unit/UnityResourceLoadingCompatibilityTests.cs`
  - Covers `Resources.Load` file-presence path and `AssetBundle.LoadAssetAsync` placeholder object path.
- Enhanced compatibility placeholders for testable load behavior:
  - `Runtime/Compatibility/UnityCompatibilityPlaceholders.cs`
  - `Resources.Load<T>` now resolves existing file paths and returns placeholder objects for Unity-style types.
  - `AssetBundle.LoadAsset*` now returns placeholder objects instead of always-null.
- Extended runtime scene verifier:
  - `Scripts/Verification/AssetSystemRuntimeVerifier.cs`
  - After normal remote chain, now verifies:
    - builtin-style load via `Resources.Load`
    - AB-style load via `package.LoadAssetAsync`
    - existing remote chain (`version/manifest/download/loadscene`)
- Updated runtime verifier report:
  - `docs/reports/assetsystem-runtime-scene-verification.md`

### Re-check Results
- `dotnet test addons/com.gameframex.godot.asset/Tests/Unit/GameFrameX.Asset.UnitTests.csproj --filter UnityResourceLoadingCompatibilityTests -v minimal`: PASS (2 passed)
- `dotnet test addons/com.gameframex.godot.asset/Tests/Unit/GameFrameX.Asset.UnitTests.csproj -v minimal`: PASS (32 passed)
- `dotnet build Godot.sln -v minimal`: PASS (0 errors)
- `dotnet build Godot.sln -v minimal -p:IncludeAssetSystemRuntime=true`: PASS (0 errors)
