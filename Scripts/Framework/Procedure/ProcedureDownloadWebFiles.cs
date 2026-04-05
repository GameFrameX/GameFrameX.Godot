// ==========================================================================================
//   GameFrameX 组织及其衍生项目的版权、商标、专利及其他相关权利
//   GameFrameX organization and its derivative projects' copyrights, trademarks, patents, and related rights
//   均受中华人民共和国及相关国际法律法规保护。
//   are protected by the laws of the People's Republic of China and relevant international regulations.
//   使用本项目须严格遵守相应法律法规及开源许可证之规定。
//   Usage of this project must strictly comply with applicable laws, regulations, and open-source licenses.
//   本项目采用 MIT 许可证与 Apache License 2.0 双许可证分发，
//   This project is dual-licensed under the MIT License and Apache License 2.0,
//   完整许可证文本请参见源代码根目录下的 LICENSE 文件。
//   please refer to the LICENSE file in the root directory of the source code for the full license text.
//   禁止利用本项目实施任何危害国家安全、破坏社会秩序、
//   It is prohibited to use this project to engage in any activities that endanger national security, disrupt social order,
//   侵犯他人合法权益等法律法规所禁止的行为！
//   or infringe upon the legitimate rights and interests of others, as prohibited by laws and regulations!
//   因基于本项目二次开发所产生的一切法律纠纷与责任，
//   Any legal disputes and liabilities arising from secondary development based on this project
//   本项目组织与贡献者概不承担。
//   shall be borne solely by the developer; the project organization and contributors assume no responsibility.
//   GitHub 仓库：https://github.com/GameFrameX
//   GitHub Repository: https://github.com/GameFrameX
//   Gitee  仓库：https://gitee.com/GameFrameX
//   Gitee Repository:  https://gitee.com/GameFrameX
//   CNB  仓库：https://cnb.cool/GameFrameX
//   CNB Repository:  https://cnb.cool/GameFrameX
//   官方文档：https://gameframex.doc.alianblank.com/
//   Official Documentation: https://gameframex.doc.alianblank.com/
//  ==========================================================================================

using GameFrameX.Fsm.Runtime;
using GameFrameX.Procedure.Runtime;
using GameFrameX.Runtime;
using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

#if INCLUDE_ASSETSYSTEM_RUNTIME
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using YooAsset;
#endif

namespace Godot.Startup.Procedure;

/// <summary>
/// 下载资源流程。
/// </summary>
public sealed class ProcedureDownloadWebFiles : ProcedureBase
{
#if INCLUDE_ASSETSYSTEM_RUNTIME
	private enum LoadProbeStage
	{
		None,
		Initializing,
		RequestingVersion,
		UpdatingManifest,
		Downloading,
		LoadingAsset,
		ShowingPreview,
		ShowingRemoteImage,
		Completed,
		Failed
	}

	private const string ProbePackageName = "runtime_verify";
	private const string ProbePackageVersion = "v1";
	private const string ProbeAssetLocation = "verify_asset";
	private const string ProbePckLocation = "verify_content.pck";
	private const string ProbePckResourcePath = "res://probe_runtime/teamgame_external.png";
	private const string ProbePckResourcePathInPack = "probe_runtime/teamgame_external.png";
	private const string ProbeFixtureVirtualRoot = "user://assetsystem_runtime_verify/yoo";
	private const ulong LoadProbeStageTimeoutMs = 30000;
#endif
	private const string BuiltinResourcePath = "res://addons/com.gameframex.godot/Resources/gameframex_logo.png";
	private const string ExternalProbeResourcePath = "res://Assets/Probe/teamgame_external.png";
	private const string BuiltinSceneProbePath = "res://Scenes/Verification/AssetSystemRuntimeVerifier.tscn";
	private const string PreviewNodeName = "RuntimeLoadPreviewSprite";
	private const string RemotePreviewNodeName = "RuntimeLoadRemotePreviewSprite";
	private const string PckPreviewNodeName = "RuntimeLoadPckPreviewSprite";
	private const string RemoteProbeDownloadLink = "https://s1.aigei.com/prevfiles/1a0cd76a0fb64eaea9f1f0859f7faa8f.jpeg?e=2051020800&token=P7S2Xpzfz11vAkASLTkfHN7Fw-oOZBecqeJaxypL:4ffDvyEeh1Xkdvdb_S6lbTLiHck=";
	private const string LoadProbeCodeRevision = "2026-04-04-r12";
	private const ulong RemoteImageWaitLogIntervalMs = 2000;
	private const ulong RemoteImageStageTimeoutMs = 10000;
	private static readonly System.Net.Http.HttpClient RemoteImageHttpClient = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(15) };

#if INCLUDE_ASSETSYSTEM_RUNTIME
	private bool _stateChanged;
	private LoadProbeStage _stage;
	private string _packageVersion = string.Empty;

	private ResourcePackage _package;
	private InitializationOperation _initializeOperation;
	private RequestPackageVersionOperation _versionOperation;
	private UpdatePackageManifestOperation _manifestOperation;
	private ResourceDownloaderOperation _downloaderOperation;
	private AssetHandle _assetHandle;
	private string _probeFixturePackageRoot;
	private Task<byte[]> _remoteImageBytesTask;
	private ulong _remoteImageRequestStartTicksMs;
	private ulong _remoteImageLastWaitLogTicksMs;
	private ulong _stageEnterTicksMs;
#endif

	/// <summary>
	/// 进入流程时执行。
	/// </summary>
	/// <param name="procedureOwner">流程持有者。</param>
	protected internal override void OnEnter(IFsm<IProcedureManager> procedureOwner)
	{
		base.OnEnter(procedureOwner);
		Log.Info("进入流程：ProcedureDownloadWebFiles");
		Log.Info("[Code] rev={0}", LoadProbeCodeRevision);

#if INCLUDE_ASSETSYSTEM_RUNTIME
		_stateChanged = false;
		_packageVersion = string.Empty;
		_package = null;
		_initializeOperation = null;
		_versionOperation = null;
		_manifestOperation = null;
		_downloaderOperation = null;
		_assetHandle = null;
		_probeFixturePackageRoot = string.Empty;
		_remoteImageBytesTask = null;
		_remoteImageRequestStartTicksMs = 0;
		_remoteImageLastWaitLogTicksMs = 0;
		MoveToStage(LoadProbeStage.Initializing, "pipeline enter");
#else
		Log.Info("[Resources] SKIP runtime-disabled (build with IncludeAssetSystemRuntime=true for full probe)");
		Log.Info("[AB] SKIP runtime-disabled");
		Log.Info("[Remote] SKIP runtime-disabled");
		Log.Info("[Preview] SKIP builtin preview disabled.");
		ChangeState<ProcedurePatchDone>(procedureOwner);
#endif
	}

	protected internal override void OnUpdate(IFsm<IProcedureManager> procedureOwner, float elapseSeconds, float realElapseSeconds)
	{
		base.OnUpdate(procedureOwner, elapseSeconds, realElapseSeconds);
#if INCLUDE_ASSETSYSTEM_RUNTIME
		UpdateRuntimeLoadPipeline();
		if (_stateChanged == false && (_stage == LoadProbeStage.Completed || _stage == LoadProbeStage.Failed))
		{
			_stateChanged = true;
			ChangeState<ProcedurePatchDone>(procedureOwner);
		}
#endif
	}

#if INCLUDE_ASSETSYSTEM_RUNTIME
	private void UpdateRuntimeLoadPipeline()
	{
		try
		{
			if (_stage != LoadProbeStage.None && _stage != LoadProbeStage.Completed && _stage != LoadProbeStage.Failed)
			{
				var elapsed = Time.GetTicksMsec() - _stageEnterTicksMs;
				if (elapsed > LoadProbeStageTimeoutMs)
				{
					FailPipeline($"[Stage] TIMEOUT stage={_stage} elapsedMs={elapsed}");
					return;
				}
			}

			if (_stage == LoadProbeStage.Initializing)
			{
				if (_initializeOperation == null)
				{
					var fixtureRoot = PrepareFixtureFiles();
					var fixturePackageRoot = Path.Combine(fixtureRoot, ProbePackageName).Replace('\\', '/');
					_probeFixturePackageRoot = fixturePackageRoot;
					var fixtureCacheRoot = Path.Combine(fixtureRoot, "_probe_cache").Replace('\\', '/');
					ResetDirectory(fixtureCacheRoot);
					YooAssets.Initialize();
					YooAssets.SetDownloadSystemHttpTransport(new LocalFileHttpTransport());
					_package = YooAssets.TryGetPackage(ProbePackageName) ?? YooAssets.CreatePackage(ProbePackageName);
					YooAssets.SetDefaultPackage(_package);
					if (OS.HasFeature("web"))
					{
						var initParams = new WebPlayModeParameters
						{
							WebFileSystemParameters = new FileSystemParameters(typeof(DefaultWebFileSystem).FullName, fixtureRoot)
						};
						Log.Info("[Remote] initialize mode=WebPlayMode root={0}", fixtureRoot);
						_initializeOperation = _package.InitializeAsync(initParams);
					}
					else
					{
						var remoteServices = new LocalDirectoryRemoteServices(fixturePackageRoot);
						var initParams = new HostPlayModeParameters
						{
							CacheFileSystemParameters = FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices, rootDirectory: fixtureCacheRoot)
						};
						Log.Info("[Remote] initialize mode=HostPlayMode root={0} cache={1}", fixturePackageRoot, fixtureCacheRoot);
						_initializeOperation = _package.InitializeAsync(initParams);
					}
					return;
				}

				if (_initializeOperation.IsDone == false)
				{
					return;
				}

				if (_initializeOperation.Status != EOperationStatus.Succeed)
				{
					FailPipeline($"[Remote] FAIL initialize: {_initializeOperation.Error}");
					return;
				}

				MoveToStage(LoadProbeStage.RequestingVersion, "initialize succeeded");
				return;
			}

			if (_stage == LoadProbeStage.RequestingVersion)
			{
				if (_versionOperation == null)
				{
					_versionOperation = _package.RequestPackageVersionAsync(appendTimeTicks: false, timeout: 10);
					return;
				}

				if (_versionOperation.IsDone == false)
				{
					return;
				}

				if (_versionOperation.Status != EOperationStatus.Succeed)
				{
					FailPipeline($"[Remote] FAIL version: {_versionOperation.Error}");
					return;
				}

				_packageVersion = _versionOperation.PackageVersion;
				MoveToStage(LoadProbeStage.UpdatingManifest, "version succeeded");
				return;
			}

			if (_stage == LoadProbeStage.UpdatingManifest)
			{
				if (_manifestOperation == null)
				{
					_manifestOperation = _package.UpdatePackageManifestAsync(_packageVersion, timeout: 10);
					return;
				}

				if (_manifestOperation.IsDone == false)
				{
					return;
				}

				if (_manifestOperation.Status != EOperationStatus.Succeed)
				{
					FailPipeline($"[Remote] FAIL manifest: {_manifestOperation.Error}");
					return;
				}

				MoveToStage(LoadProbeStage.Downloading, "manifest succeeded");
				return;
			}

			if (_stage == LoadProbeStage.Downloading)
			{
				if (_downloaderOperation == null)
				{
					_downloaderOperation = _package.CreateResourceDownloader(downloadingMaxNumber: 1, failedTryAgain: 0, timeout: 10);
					_downloaderOperation.OnStartDownloadFileCallback = data =>
					{
						Log.Info("[Remote] downloading file={0} size={1}", data.FileName, data.FileSize);
					};
					_downloaderOperation.OnDownloadErrorCallback = data =>
					{
						Log.Warning("[Remote] download error file={0} info={1}", data.FileName, data.ErrorInfo);
						Log.Info("[Remote] download error file={0} info={1}", data.FileName, data.ErrorInfo);
					};
					_downloaderOperation.BeginDownload();
					return;
				}

				if (_downloaderOperation.IsDone == false)
				{
					return;
				}

				if (_downloaderOperation.Status != EOperationStatus.Succeed)
				{
					FailPipeline($"[Remote] FAIL download: {_downloaderOperation.Error}");
					return;
				}

				Log.Info("[Remote] READY package={0} totalCount={1} totalBytes={2}",
					_package.PackageName, _downloaderOperation.TotalDownloadCount, _downloaderOperation.TotalDownloadBytes);
				MoveToStage(LoadProbeStage.LoadingAsset, "download succeeded");
				return;
			}

			if (_stage == LoadProbeStage.LoadingAsset)
			{
				if (_assetHandle == null)
				{
					_assetHandle = _package.LoadAssetAsync(ProbeAssetLocation, typeof(UnityEngine.Object), priority: 100);
					return;
				}

				if (_assetHandle.IsDone == false)
				{
					return;
				}

				if (_assetHandle.Status != EOperationStatus.Succeed || _assetHandle.AssetObject == null)
				{
					FailPipeline($"[AB] FAIL handle: {_assetHandle.LastError}");
					return;
				}

				Log.Info("[AB] PASS asset={0}", _assetHandle.AssetObject.name);
				_assetHandle.Release();
				_assetHandle = null;
				MoveToStage(LoadProbeStage.ShowingPreview, "asset loaded");
				return;
			}

			if (_stage == LoadProbeStage.ShowingPreview)
			{
				Log.Info("[Preview] SKIP pck/raw/builtin preview disabled.");
				// MoveToStage(LoadProbeStage.ShowingRemoteImage, "preview shown");
				// 按调试需求临时关闭远程图片加载入口，预览完成后直接结束探针流程。
				MoveToStage(LoadProbeStage.Completed, "preview shown (remote image entry disabled)");
				return;
			}

			if (_stage == LoadProbeStage.ShowingRemoteImage)
			{
				if (_remoteImageBytesTask == null)
				{
					var requestUrl = ResolveRemoteImageRequestUrl(RemoteProbeDownloadLink);
					Log.Info("[RemoteImage] request={0}", requestUrl);
					_remoteImageBytesTask = DownloadRemoteImageBytesAsync(requestUrl);
					_remoteImageRequestStartTicksMs = Time.GetTicksMsec();
					_remoteImageLastWaitLogTicksMs = _remoteImageRequestStartTicksMs;
					return;
				}

				if (_remoteImageBytesTask.IsCompleted == false)
				{
					var nowTicks = Time.GetTicksMsec();
					var waitMs = nowTicks - _remoteImageRequestStartTicksMs;
					if (waitMs >= RemoteImageStageTimeoutMs)
					{
						Log.Warning("[RemoteImage] FAIL timeout={0}ms", waitMs);
						Log.Info("[RemoteImage] FAIL timeout={0}ms", waitMs);
						MoveToStage(LoadProbeStage.Completed, "remote image timeout");
						return;
					}

					if (nowTicks - _remoteImageLastWaitLogTicksMs >= RemoteImageWaitLogIntervalMs)
					{
						_remoteImageLastWaitLogTicksMs = nowTicks;
						Log.Info("[RemoteImage] waiting elapsedMs={0}", waitMs);
					}

					return;
				}

				if (_remoteImageBytesTask.IsFaulted || _remoteImageBytesTask.IsCanceled)
				{
					var error = _remoteImageBytesTask.Exception?.GetBaseException().Message ?? "request failed.";
					Log.Warning("[RemoteImage] FAIL error={0}", error);
					Log.Info("[RemoteImage] FAIL error={0}", error);
					MoveToStage(LoadProbeStage.Completed, "remote image failed");
					return;
				}

				Log.Info("[RemoteImage] response bytes={0}", _remoteImageBytesTask.Result.Length);
				if (TryShowRemoteImagePreview(_remoteImageBytesTask.Result))
				{
					MoveToStage(LoadProbeStage.Completed, "remote image shown");
				}
				else
				{
					Log.Warning("[RemoteImage] FAIL decode/display.");
					Log.Info("[RemoteImage] FAIL decode/display.");
					MoveToStage(LoadProbeStage.Completed, "remote image decode failed");
				}

				return;
			}
		}
		catch (Exception exception)
		{
			FailPipeline($"[Remote] FAIL exception={exception.Message}");
		}
	}

	private void FailPipeline(string message)
	{
		Log.Warning(message);
		Log.Info(message);
		Log.Info("[Preview] SKIP builtin preview disabled in fail path.");
		MoveToStage(LoadProbeStage.Failed, "pipeline failed");
	}

	private void MoveToStage(LoadProbeStage nextStage, string reason)
	{
		_stage = nextStage;
		_stageEnterTicksMs = Time.GetTicksMsec();
		Log.Info("[Stage] {0} ({1})", nextStage, reason);
	}

	private static void ProbeGodotRawResourceLoading()
	{
		try
		{
			var rawTexture = YooAssets.LoadGodotResourceFromRawFileSync<Texture2D>(ProbeAssetLocation);
			if (rawTexture != null)
			{
				Log.Info("[GodotRaw] PASS location={0} type=Texture2D", ProbeAssetLocation);
			}
			else
			{
				var fallbackTexture = ResourceLoader.Load<Texture2D>(BuiltinResourcePath);
				if (fallbackTexture != null)
				{
					Log.Info("[GodotRaw] PASS fallback path={0} type=Texture2D", BuiltinResourcePath);
				}
				else
				{
					Log.Warning("[GodotRaw] FAIL location={0} type=Texture2D", ProbeAssetLocation);
					Log.Info("[GodotRaw] FAIL location={0} type=Texture2D", ProbeAssetLocation);
				}
			}

			var rawScene = YooAssets.LoadGodotResourceFromRawFileSync<PackedScene>(ProbeAssetLocation);
			if (rawScene != null)
			{
				Log.Info("[GodotRaw] PASS location={0} type=PackedScene", ProbeAssetLocation);
			}
			else
			{
				// var fallbackScene = ResourceLoader.Load<PackedScene>(BuiltinSceneProbePath);
				// AssetSystemRuntimeVerifier 场景探针入口已按需求禁用，不再触发其脚本加载链路。
				Log.Info("[GodotRaw] SKIP fallback scene probe path={0}", BuiltinSceneProbePath);
			}
		}
		catch (Exception exception)
		{
			Log.Warning("[GodotRaw] FAIL exception={0}", exception.Message);
			Log.Info("[GodotRaw] FAIL exception={0}", exception.Message);
		}
	}

	private void ProbeGodotPckMounting()
	{
		try
		{
			if (_package == null || _package.CheckLocationValid(ProbePckLocation) == false)
			{
				Log.Info("[GodotPck] SKIP missing location={0}", ProbePckLocation);
				return;
			}

			var mounted = YooAssets.MountGodotResourcePackFromRawFileSync(ProbePckLocation, replaceFiles: false);
			if (mounted == false)
			{
				Log.Warning("[GodotPck] raw mount failed location={0}, try direct-path fallback", ProbePckLocation);
				Log.Info("[GodotPck] raw mount failed location={0}, try direct-path fallback", ProbePckLocation);
				if (string.IsNullOrWhiteSpace(_probeFixturePackageRoot) == false)
				{
					var pckPath = Path.Combine(_probeFixturePackageRoot, ProbePckLocation).Replace('\\', '/');
					mounted = YooAssets.MountGodotResourcePackByPath(pckPath, replaceFiles: false);
					if (mounted)
					{
						Log.Info("[GodotPck] mounted via direct path={0}", pckPath);
					}
				}

				if (mounted == false)
				{
					Log.Warning("[GodotPck] FAIL mount location={0}", ProbePckLocation);
					Log.Info("[GodotPck] FAIL mount location={0}", ProbePckLocation);
					return;
				}
			}

			var loaded = ResourceLoader.Load<Resource>(ProbePckResourcePath) ?? ResourceLoader.Load<Resource>(ProbePckResourcePathInPack);
			if (loaded is Texture2D loadedTexture)
			{
				ShowTexturePreview(loadedTexture, PckPreviewNodeName, new Vector2(650f, 120f));
				Log.Info("[GodotPck] PASS path={0} type={1}", ProbePckResourcePath, loaded.GetType().Name);
				return;
			}

			var filePath = ProbePckResourcePath;
			var existsInPack = FileAccess.FileExists(filePath);
			if (existsInPack == false && FileAccess.FileExists(ProbePckResourcePathInPack))
			{
				filePath = ProbePckResourcePathInPack;
				existsInPack = true;
			}

			Log.Info("[GodotPck] resourceLoader miss, fileExists={0} path={1}", existsInPack, filePath);
			if (existsInPack == false)
			{
				Log.Warning("[GodotPck] FAIL load path={0}", ProbePckResourcePath);
				Log.Info("[GodotPck] FAIL load path={0}", ProbePckResourcePath);
				return;
			}

			using var fileAccess = FileAccess.Open(filePath, FileAccess.ModeFlags.Read);
			if (fileAccess == null)
			{
				Log.Warning("[GodotPck] FAIL file open path={0}", filePath);
				Log.Info("[GodotPck] FAIL file open path={0}", filePath);
				return;
			}

			var bytes = fileAccess.GetBuffer((long)fileAccess.GetLength());
			if (bytes == null || bytes.Length == 0)
			{
				Log.Warning("[GodotPck] FAIL file empty path={0}", filePath);
				Log.Info("[GodotPck] FAIL file empty path={0}", filePath);
				return;
			}

			var image = new Image();
			var loadResult = image.LoadPngFromBuffer(bytes);
			if (loadResult != Error.Ok)
			{
				loadResult = image.LoadJpgFromBuffer(bytes);
			}

			if (loadResult != Error.Ok)
			{
				Log.Warning("[GodotPck] FAIL decode path={0} error={1}", filePath, loadResult);
				Log.Info("[GodotPck] FAIL decode path={0} error={1}", filePath, loadResult);
				return;
			}

			var previewTexture = ImageTexture.CreateFromImage(image);
			if (previewTexture == null || ShowTexturePreview(previewTexture, PckPreviewNodeName, new Vector2(650f, 120f)) == false)
			{
				Log.Warning("[GodotPck] FAIL display path={0}", filePath);
				Log.Info("[GodotPck] FAIL display path={0}", filePath);
				return;
			}

			Log.Info("[GodotPck] PASS path={0} type=ImageTexture bytes={1}", filePath, bytes.Length);
		}
		catch (Exception exception)
		{
			Log.Warning("[GodotPck] FAIL exception={0}", exception.Message);
			Log.Info("[GodotPck] FAIL exception={0}", exception.Message);
		}
	}

	private static string PrepareFixtureFiles()
	{
		var rootDirectory = ProjectSettings.GlobalizePath(ProbeFixtureVirtualRoot).Replace('\\', '/');
		Directory.CreateDirectory(rootDirectory);

		var packageDirectory = Path.Combine(rootDirectory, ProbePackageName);
		Directory.CreateDirectory(packageDirectory);

		var sceneBundleBytes = new byte[] { 0x00 };
		const string sceneBundleName = "verify_scene_bundle.ab";
		var sceneBundleHash = ToLowerMd5(sceneBundleBytes);
		var sceneBundleCrc = HashUtility.BytesCRC32(sceneBundleBytes);
		var sceneBundleSize = (long)sceneBundleBytes.Length;
		var sceneBundleFilePath = Path.Combine(packageDirectory, sceneBundleName);
		if (File.Exists(sceneBundleFilePath) == false)
		{
			File.WriteAllBytes(sceneBundleFilePath, sceneBundleBytes);
		}

		var bundleNames = new List<string>(2) { sceneBundleName };
		var bundleHashes = new List<string>(2) { sceneBundleHash };
		var bundleCrcs = new List<string>(2) { sceneBundleCrc };
		var bundleSizes = new List<long>(2) { sceneBundleSize };
		var assetLocations = new List<string>(2) { ProbeAssetLocation };
		var assetPaths = new List<string>(2) { $"Assets/{ProbeAssetLocation}.prefab" };
		var assetBundleIds = new List<int>(2) { 0 };

		var pckFilePath = Path.Combine(packageDirectory, ProbePckLocation);
		if (TryBuildProbePckFile(pckFilePath))
		{
			var pckBytes = File.ReadAllBytes(pckFilePath);
			bundleNames.Add(ProbePckLocation);
			bundleHashes.Add(ToLowerMd5(pckBytes));
			bundleCrcs.Add(HashUtility.BytesCRC32(pckBytes));
			bundleSizes.Add(pckBytes.LongLength);
			assetLocations.Add(ProbePckLocation);
			assetPaths.Add($"Assets/{ProbePckLocation}");
			assetBundleIds.Add(1);
			Log.Info("[GodotPck] fixture prepared path={0} bytes={1}", pckFilePath.Replace('\\', '/'), pckBytes.LongLength);
		}
		else
		{
			Log.Info("[GodotPck] fixture skipped (packer unavailable or source missing)");
		}

		var manifestBytes = BuildManifestBinary(
			ProbePackageName,
			ProbePackageVersion,
			assetLocations.ToArray(),
			assetPaths.ToArray(),
			assetBundleIds.ToArray(),
			bundleNames.ToArray(),
			bundleHashes.ToArray(),
			bundleCrcs.ToArray(),
			bundleSizes.ToArray());
		var packageHash = ToLowerMd5(manifestBytes);

		var versionFileName = YooAssetSettingsData.GetPackageVersionFileName(ProbePackageName);
		var hashFileName = YooAssetSettingsData.GetPackageHashFileName(ProbePackageName, ProbePackageVersion);
		var manifestFileName = YooAssetSettingsData.GetManifestBinaryFileName(ProbePackageName, ProbePackageVersion);

		File.WriteAllText(Path.Combine(packageDirectory, versionFileName), ProbePackageVersion, Encoding.UTF8);
		File.WriteAllText(Path.Combine(packageDirectory, hashFileName), packageHash, Encoding.UTF8);
		File.WriteAllBytes(Path.Combine(packageDirectory, manifestFileName), manifestBytes);

		return rootDirectory;
	}

	private static bool TryBuildProbePckFile(string pckFilePath)
	{
		var preferredSourceFilePath = ProjectSettings.GlobalizePath(ExternalProbeResourcePath).Replace('\\', '/');
		var fallbackSourceFilePath = ProjectSettings.GlobalizePath(BuiltinResourcePath).Replace('\\', '/');
		var sourceFilePath = File.Exists(preferredSourceFilePath) ? preferredSourceFilePath : fallbackSourceFilePath;
		if (File.Exists(sourceFilePath) == false)
		{
			return false;
		}

		Log.Info("[GodotPck] fixture source={0}", sourceFilePath);

		var packer = new PckPacker();
		var startResult = packer.PckStart(pckFilePath);
		if (startResult != Error.Ok)
		{
			return false;
		}

		var addResult = packer.AddFile(ProbePckResourcePathInPack, sourceFilePath);
		if (addResult != Error.Ok)
		{
			return false;
		}

		var flushResult = packer.Flush();
		return flushResult == Error.Ok;
	}

	private static byte[] BuildManifestBinary(
		string packageName,
		string packageVersion,
		string[] assetLocations,
		string[] assetPaths,
		int[] assetBundleIds,
		string[] bundleNames,
		string[] bundleHashes,
		string[] bundleCrcs,
		long[] bundleSizes)
	{
		using var stream = new MemoryStream();
		using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);

		writer.Write(YooAssetSettings.ManifestFileSign);
		WriteUtf8(writer, YooAssetSettings.ManifestFileVersion);
		writer.Write(true);
		writer.Write(false);
		writer.Write(false);
		writer.Write(1);
		WriteUtf8(writer, "BuiltinBuildPipeline");
		WriteUtf8(writer, packageName);
		WriteUtf8(writer, packageVersion);

		writer.Write(assetLocations.Length);
		for (var i = 0; i < assetLocations.Length; i++)
		{
			WriteUtf8(writer, assetLocations[i]);
			WriteUtf8(writer, assetPaths[i]);
			WriteUtf8(writer, string.Empty);
			WriteUtf8Array(writer, Array.Empty<string>());
			writer.Write(assetBundleIds[i]);
		}

		writer.Write(bundleNames.Length);
		for (var i = 0; i < bundleNames.Length; i++)
		{
			WriteUtf8(writer, bundleNames[i]);
			writer.Write((uint)0);
			WriteUtf8(writer, bundleHashes[i]);
			WriteUtf8(writer, bundleCrcs[i]);
			writer.Write(bundleSizes[i]);
			writer.Write(false);
			WriteUtf8Array(writer, Array.Empty<string>());
			WriteInt32Array(writer, Array.Empty<int>());
		}

		writer.Flush();
		return stream.ToArray();
	}

	private static string ToLowerMd5(byte[] data)
	{
		using var md5 = MD5.Create();
		var hash = md5.ComputeHash(data);
		var sb = new StringBuilder(hash.Length * 2);
		foreach (var item in hash)
		{
			sb.Append(item.ToString("x2"));
		}

		return sb.ToString();
	}

	private static void WriteUtf8(BinaryWriter writer, string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			writer.Write((ushort)0);
			return;
		}

		var bytes = Encoding.UTF8.GetBytes(value);
		if (bytes.Length > ushort.MaxValue)
		{
			throw new InvalidOperationException("UTF8 string is too long.");
		}

		writer.Write((ushort)bytes.Length);
		writer.Write(bytes);
	}

	private static void WriteUtf8Array(BinaryWriter writer, string[] values)
	{
		if (values == null)
		{
			writer.Write((ushort)0);
			return;
		}

		if (values.Length > ushort.MaxValue)
		{
			throw new InvalidOperationException("String array is too large.");
		}

		writer.Write((ushort)values.Length);
		for (var i = 0; i < values.Length; i++)
		{
			WriteUtf8(writer, values[i]);
		}
	}

	private static void WriteInt32Array(BinaryWriter writer, int[] values)
	{
		if (values == null)
		{
			writer.Write((ushort)0);
			return;
		}

		if (values.Length > ushort.MaxValue)
		{
			throw new InvalidOperationException("Int32 array is too large.");
		}

		writer.Write((ushort)values.Length);
		for (var i = 0; i < values.Length; i++)
		{
			writer.Write(values[i]);
		}
	}

	private static void ResetDirectory(string directoryPath)
	{
		if (string.IsNullOrWhiteSpace(directoryPath))
		{
			return;
		}

		if (Directory.Exists(directoryPath))
		{
			Directory.Delete(directoryPath, recursive: true);
		}

		Directory.CreateDirectory(directoryPath);
	}

	private static async Task<byte[]> DownloadRemoteImageBytesAsync(string requestUrl)
	{
		if (string.IsNullOrWhiteSpace(requestUrl))
		{
			throw new InvalidOperationException("Remote image request url is empty.");
		}

		using var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, requestUrl);
		request.Headers.UserAgent.ParseAdd("GameFrameX.Godot.LoadProbe/1.0");
		using var response = await RemoteImageHttpClient.SendAsync(request, System.Net.Http.HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
		if (response.IsSuccessStatusCode == false)
		{
			throw new InvalidOperationException($"HTTP {(int)response.StatusCode}");
		}

		return await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
	}

	private static string ResolveRemoteImageRequestUrl(string sourceUrl)
	{
		if (string.IsNullOrWhiteSpace(sourceUrl))
		{
			return string.Empty;
		}

		if (Uri.TryCreate(sourceUrl, UriKind.Absolute, out var uri) == false)
		{
			return sourceUrl;
		}

		var query = ParseQueryString(uri.Query);
		if (query.TryGetValue("url", out var directUrl) && string.IsNullOrWhiteSpace(directUrl) == false)
		{
			return directUrl;
		}

		if (query.TryGetValue("thumburl", out var thumbUrl) && string.IsNullOrWhiteSpace(thumbUrl) == false)
		{
			return thumbUrl;
		}

		return sourceUrl;
	}

	private static Dictionary<string, string> ParseQueryString(string query)
	{
		var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		if (string.IsNullOrEmpty(query))
		{
			return result;
		}

		var parts = query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries);
		for (var i = 0; i < parts.Length; i++)
		{
			var segment = parts[i];
			var index = segment.IndexOf('=');
			var key = index >= 0 ? segment[..index] : segment;
			var value = index >= 0 ? segment[(index + 1)..] : string.Empty;
			if (string.IsNullOrWhiteSpace(key))
			{
				continue;
			}

			key = Uri.UnescapeDataString(key.Replace('+', ' '));
			value = Uri.UnescapeDataString(value.Replace('+', ' '));
			result[key] = value;
		}

		return result;
	}

	private static bool TryShowRemoteImagePreview(byte[] bytes)
	{
		if (bytes == null || bytes.Length == 0)
		{
			return false;
		}

		var image = new Image();
		var loadResult = image.LoadJpgFromBuffer(bytes);
		if (loadResult != Error.Ok)
		{
			loadResult = image.LoadPngFromBuffer(bytes);
		}

		if (loadResult != Error.Ok)
		{
			return false;
		}

		var texture = ImageTexture.CreateFromImage(image);
		if (texture == null)
		{
			return false;
		}

		var displayed = ShowTexturePreview(texture, RemotePreviewNodeName, new Vector2(420f, 120f));
		if (displayed)
		{
			Log.Info("[RemoteImage] PASS bytes={0}", bytes.Length);
		}

		return displayed;
	}

	private sealed class LocalFileHttpTransport : IHttpTransport
	{
		private readonly GodotHttpTransport _fallback = new GodotHttpTransport();

		public Task<HttpResponse> GetTextAsync(string requestURL, int timeout, bool appendTimeTicks, CancellationToken cancellationToken)
		{
			if (TryResolveLocalFile(requestURL, out var localPath))
			{
				if (File.Exists(localPath) == false)
				{
					return Task.FromResult(new HttpResponse
					{
						Success = false,
						StatusCode = 404,
						Error = $"Local file not found: {localPath}"
					});
				}

				return Task.FromResult(new HttpResponse
				{
					Success = true,
					StatusCode = 200,
					Text = File.ReadAllText(localPath, Encoding.UTF8)
				});
			}

			return _fallback.GetTextAsync(requestURL, timeout, appendTimeTicks, cancellationToken);
		}

		public Task<HttpResponse> GetDataAsync(string requestURL, int timeout, bool appendTimeTicks, CancellationToken cancellationToken)
		{
			if (TryResolveLocalFile(requestURL, out var localPath))
			{
				if (File.Exists(localPath) == false)
				{
					return Task.FromResult(new HttpResponse
					{
						Success = false,
						StatusCode = 404,
						Error = $"Local file not found: {localPath}"
					});
				}

				return Task.FromResult(new HttpResponse
				{
					Success = true,
					StatusCode = 200,
					Data = File.ReadAllBytes(localPath)
				});
			}

			return _fallback.GetDataAsync(requestURL, timeout, appendTimeTicks, cancellationToken);
		}

		private static bool TryResolveLocalFile(string requestURL, out string localPath)
		{
			localPath = string.Empty;
			if (string.IsNullOrWhiteSpace(requestURL))
			{
				return false;
			}

			if (requestURL.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
			{
				var uri = new Uri(requestURL);
				localPath = uri.LocalPath;
				return true;
			}

			if (Path.IsPathRooted(requestURL))
			{
				localPath = requestURL;
				return true;
			}

			return false;
		}
	}

	private sealed class LocalDirectoryRemoteServices : IRemoteServices
	{
		private readonly string _mainDirectory;
		private readonly string _fallbackDirectory;

		public LocalDirectoryRemoteServices(string mainDirectory, string fallbackDirectory = null)
		{
			_mainDirectory = mainDirectory ?? string.Empty;
			_fallbackDirectory = string.IsNullOrWhiteSpace(fallbackDirectory) ? _mainDirectory : fallbackDirectory;
		}

		public string GetRemoteMainURL(string fileName, string packageVersion)
		{
			return CombinePath(_mainDirectory, fileName);
		}

		public string GetRemoteFallbackURL(string fileName, string packageVersion)
		{
			return CombinePath(_fallbackDirectory, fileName);
		}

		private static string CombinePath(string baseDirectory, string fileName)
		{
			if (string.IsNullOrWhiteSpace(baseDirectory))
			{
				return fileName ?? string.Empty;
			}

			if (string.IsNullOrWhiteSpace(fileName))
			{
				return baseDirectory.Replace('\\', '/');
			}

			return Path.Combine(baseDirectory, fileName).Replace('\\', '/');
		}
	}
#endif

	private static void ShowBuiltinResourcePreview()
	{
		var texture = ResourceLoader.Load<Texture2D>(BuiltinResourcePath);
		if (texture == null)
		{
			Log.Warning("[Resources] FAIL path={0}", BuiltinResourcePath);
			return;
		}

		if (ShowTexturePreview(texture, PreviewNodeName, new Vector2(180f, 120f)))
		{
			Log.Info("[Resources] PASS displayed preview path={0}", BuiltinResourcePath);
			return;
		}

		Log.Warning("[Resources] FAIL no scene root.");
	}

	private static Node GetSceneRootNode()
	{
		var sceneTree = Engine.GetMainLoop() as SceneTree;
		return sceneTree?.CurrentScene ?? sceneTree?.Root;
	}

	private static bool ShowTexturePreview(Texture2D texture, string nodeName, Vector2 position)
	{
		if (texture == null)
		{
			return false;
		}

		var rootNode = GetSceneRootNode();
		if (rootNode == null)
		{
			return false;
		}

		var existing = rootNode.GetNodeOrNull<Sprite2D>(nodeName);
		if (existing != null)
		{
			existing.Texture = texture;
			existing.Position = position;
			existing.Visible = true;
			return true;
		}

		var preview = new Sprite2D
		{
			Name = nodeName,
			Texture = texture,
			Position = position,
			ZIndex = 999
		};
		rootNode.AddChild(preview);
		return true;
	}
}
