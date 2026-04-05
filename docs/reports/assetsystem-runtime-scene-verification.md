# AssetSystem Runtime Scene Verification

## Scene
- `res://Scenes/Verification/AssetSystemRuntimeVerifier.tscn`

## What This Verifier Executes
- `InitializeAsync(WebPlayMode)`
- `RequestPackageVersionAsync`
- `UpdatePackageManifestAsync`
- `CreateResourceDownloader(...).BeginDownload()`
- `Resources.Load` (builtin resource probe)
- `LoadAssetAsync` (asset-bundle style load)
- `LoadSceneAsync(Additive)` and `UnloadAsync()`

## Fixture Strategy
- At runtime, the verifier auto-creates a minimal package fixture under:
  - `user://assetsystem_runtime_verify/yoo/<PackageName>/`
- Files created:
  - `PackageManifest_<PackageName>.version`
  - `PackageManifest_<PackageName>_<Version>.hash`
  - `PackageManifest_<PackageName>_<Version>.bytes`
  - a placeholder bundle file
- Hash is computed as lowercase MD5 of manifest bytes.

## Required Build Mode
- `IncludeAssetSystemRuntime=true`
- The script has a guarded fallback:
  - if assetsystem runtime is not compiled, it logs a clear error and exits.

## Run Commands
1. Build with runtime enabled:
   - `dotnet build Godot.sln -v minimal -p:IncludeAssetSystemRuntime=true`
2. Run scene in editor:
   - open and run `res://Scenes/Verification/AssetSystemRuntimeVerifier.tscn`
3. Run scene headless (if `godot4` is available):
   - `godot4 --headless --path . --scene res://Scenes/Verification/AssetSystemRuntimeVerifier.tscn`

## PASS / FAIL Criteria
- PASS log:
  - `[AssetSystemRuntimeVerifier] PASS`
- FAIL log:
  - `[AssetSystemRuntimeVerifier] FAIL: ...`
- End summary always prints elapsed milliseconds.
- Additional checkpoints:
  - builtin probe: `[AssetSystemRuntimeVerifier] builtin loaded: ...`
  - AB load probe: `[AssetSystemRuntimeVerifier] assetbundle loaded: ...`
  - remote chain ready: `[AssetSystemRuntimeVerifier] Remote load pipeline ready.`

## Current Execution Note (2026-04-04)
- In this environment, `godot4/godot` executable was not found, so scene run could not be executed here.
- Build and unit matrix remain green.
