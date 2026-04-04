# AssetSystem E2E Checklist (2026-04-04)

## Target Chain
- `init -> version -> manifest -> download -> load`

## Test Coverage
- Unit harness:
  - `AssetSystemPipelineOperationTests.Pipeline_ShouldRun_InitVersionManifestDownloadLoad`
  - `AssetSystemPipelineOperationTests.RequestPackageVersion_ShouldRetryAndSucceed_OnSecondAttempt`
  - `AssetSystemPipelineOperationTests.LoadLocalManifest_ShouldFallbackToBuildinLocalManifest_WhenCacheMissing`

## Recovery Coverage
- Timeout/retry:
  - `RequestPackageVersionImplOperation` retries one additional request when first remote version request fails.
- Manifest fallback:
  - `LoadLocalManifestImplOperation` now falls back from cache local manifest load to buildin local manifest load.
  - No remote manifest request is issued in local fallback branch.

## Stability Gate (3 Consecutive Runs)
- Command:
  - `dotnet test addons/com.gameframex.godot.asset/Tests/Unit/GameFrameX.Asset.UnitTests.csproj -v minimal --filter "FullyQualifiedName~AssetSystemPipelineOperationTests.Pipeline_ShouldRun_InitVersionManifestDownloadLoad"`
- Run #1: PASS
- Run #2: PASS
- Run #3: PASS

## Current Result
- Gate status: PASS (unit E2E harness + runtime single-package verification)
- Remaining scope:
  - Runtime sample-scene E2E (multi-package scenarios) is still pending execution.
  - Prepared verifier: `docs/reports/assetsystem-runtime-scene-verification.md`

## Runtime Evidence (2026-04-04 17:36)
- Success path observed with terminal closure:
  - `[LoadProbe][Stage] Completed (remote image shown)`
- Full stage chain observed:
  - `Initializing -> RequestingVersion -> UpdatingManifest -> Downloading -> LoadingAsset -> ShowingPreview -> ShowingRemoteImage -> Completed`
- Key success markers observed:
  - `[LoadProbe][AB] PASS asset=verify_asset`
  - `[LoadProbe][GodotPck] PASS path=res://probe_runtime/teamgame_external.png type=ImageTexture bytes=99583`
  - `[LoadProbe][GodotRaw] PASS fallback ... type=Texture2D`
  - `[LoadProbe][GodotRaw] PASS fallback ... type=PackedScene`
  - `[LoadProbe][RemoteImage] PASS bytes=18168`
- Note:
  - `GodotPck` actual runtime mount/load is now confirmed with external fixture asset.
- Newer probe revision (`r12`) adds dynamic `verify_content.pck` generation, direct-path mount fallback, and dual path probing for mounted PCK resources. Expected additional verification line:
  - `[LoadProbe][GodotPck] PASS path=res://probe_runtime/teamgame_external.png type=...`
  - or `[LoadProbe][GodotPck] PASS path=probe_runtime/teamgame_external.png type=...`

## Runtime Log Contract (`[LoadProbe]`)
- Required success sequence:
  - `[LoadProbe][Stage] Initializing`
  - `[LoadProbe][Stage] RequestingVersion`
  - `[LoadProbe][Stage] UpdatingManifest`
  - `[LoadProbe][Stage] Downloading`
  - `[LoadProbe][AB] PASS ...`
  - `[LoadProbe][GodotPck] PASS ...` or `[LoadProbe][GodotPck] SKIP ...` (until pck package is configured)
  - `[LoadProbe][GodotRaw] PASS ...` (raw or fallback path)
  - `[LoadProbe][RemoteImage] PASS ...`
  - `[LoadProbe][Stage] Completed (...)`

- Required deterministic failure closure:
  - Any stage failure must emit both:
    - one fail reason line (for example `FAIL initialize/version/manifest/download/error/timeout`)
    - one terminal line: `[LoadProbe][Stage] Failed (pipeline failed)` or `[LoadProbe][Stage] Completed (...)`

## Negative Path Checklist
- Remote image URL timeout:
  - Expect: `[LoadProbe][RemoteImage] FAIL timeout=...ms`
  - Expect: terminal stage log (`Completed` for degraded continuation)
- Manifest/version remote failure:
  - Expect: `[LoadProbe][Remote] FAIL version|manifest: ...`
  - Expect: terminal stage log (`Failed`)
