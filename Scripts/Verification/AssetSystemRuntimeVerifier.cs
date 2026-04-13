using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using GameFrameX.AssetSystem;

namespace Godot.Startup.Verification
{
	/// <summary>
	/// Runtime verifier for: init -> version -> manifest -> download -> load(scene)
	/// </summary>
	public partial class AssetSystemRuntimeVerifier : Node
	{
		[Export] public string PackageName { get; set; } = "runtime_verify";
		[Export] public string PackageVersion { get; set; } = "v1";
		[Export] public string SceneLocation { get; set; } = "verify_scene";
		[Export] public string AssetLocation { get; set; } = "verify_asset";
		[Export] public string BuiltinResourcePath { get; set; } = "res://addons/com.gameframex.godot/Resources/gameframex_logo.png";
		[Export] public string FixtureVirtualRoot { get; set; } = "user://asset_runtime_verify/asset";
		[Export] public bool AutoRunOnReady { get; set; } = true;
		[Export] public bool AutoQuitOnFinish { get; set; } = true;

		public override async void _Ready()
		{
			if (!AutoRunOnReady)
			{
				GD.Print("[AssetSystemRuntimeVerifier] AutoRunOnReady=false, skip.");
				return;
			}

			await RunVerificationAsync();
		}

		private async Task RunVerificationAsync()
		{
			var stopwatch = Stopwatch.StartNew();
			var failed = false;
			try
			{
				var fixtureRoot = PrepareFixtureFiles();
				var fixturePackageRoot = Path.Combine(fixtureRoot, PackageName).Replace('\\', '/');
				var fixtureCacheRoot = Path.Combine(fixtureRoot, "_probe_cache").Replace('\\', '/');
				ResetDirectory(fixtureCacheRoot);
				GD.Print($"[AssetSystemRuntimeVerifier] Fixture root: {fixtureRoot}");

			global::GameFrameX.AssetSystem.AssetSystem.Initialize();
			global::GameFrameX.AssetSystem.AssetSystem.SetDownloadSystemHttpTransport(new LocalFileHttpTransport());

			var package = global::GameFrameX.AssetSystem.AssetSystem.TryGetPackage(PackageName) ?? global::GameFrameX.AssetSystem.AssetSystem.CreatePackage(PackageName);
			global::GameFrameX.AssetSystem.AssetSystem.SetDefaultPackage(package);

				InitializeParameters initializeParameters;
				if (OS.HasFeature("web"))
				{
					initializeParameters = new WebPlayModeParameters
					{
						WebFileSystemParameters = new FileSystemParameters(typeof(DefaultWebFileSystem).FullName, fixtureRoot)
					};
					GD.Print($"[AssetSystemRuntimeVerifier] init mode=WebPlayMode root={fixtureRoot}");
				}
				else
				{
					var remoteServices = new LocalDirectoryRemoteServices(fixturePackageRoot);
					initializeParameters = new HostPlayModeParameters
					{
						CacheFileSystemParameters = FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices, rootDirectory: fixtureCacheRoot)
					};
					GD.Print($"[AssetSystemRuntimeVerifier] init mode=HostPlayMode root={fixturePackageRoot} cache={fixtureCacheRoot}");
				}

				var initializeOperation = package.InitializeAsync(initializeParameters);
				await WaitOperationAsync(initializeOperation, "Initialize");

				var versionOperation = package.RequestPackageVersionAsync(appendTimeTicks: false, timeout: 10);
				await WaitOperationAsync(versionOperation, "RequestPackageVersion");
				var remoteVersion = versionOperation.PackageVersion;
				GD.Print($"[AssetSystemRuntimeVerifier] version={remoteVersion}");

				var manifestOperation = package.UpdatePackageManifestAsync(remoteVersion, timeout: 10);
				await WaitOperationAsync(manifestOperation, "UpdatePackageManifest");

				var downloader = package.CreateResourceDownloader(downloadingMaxNumber: 4, failedTryAgain: 1, timeout: 10);
				downloader.BeginDownload();
				await WaitOperationAsync(downloader, "DownloadRemoteContent");
				GD.Print("[AssetSystemRuntimeVerifier] Remote load pipeline ready.");

				VerifyBuiltinResourceLoad();
				await VerifyAssetBundleLoadAsync(package, AssetLocation);

				var sceneHandle = package.LoadSceneAsync(SceneLocation, suspendLoad: false, priority: 100);
				await WaitHandleAsync(sceneHandle, "LoadScene");
				GD.Print($"[AssetSystemRuntimeVerifier] loaded scene={sceneHandle.SceneName}");

				var unloadOperation = sceneHandle.UnloadAsync();
				await WaitOperationAsync(unloadOperation, "UnloadScene");

				GD.Print("[AssetSystemRuntimeVerifier] PASS");
			}
			catch (Exception exception)
			{
				failed = true;
				GD.PrintErr($"[AssetSystemRuntimeVerifier] FAIL: {exception.Message}");
			}
			finally
			{
				stopwatch.Stop();
				GD.Print($"[AssetSystemRuntimeVerifier] elapsed={stopwatch.ElapsedMilliseconds}ms");
				if (AutoQuitOnFinish)
				{
					GetTree().Quit(failed ? 1 : 0);
				}
			}
		}

		private string PrepareFixtureFiles()
		{
			var rootDirectory = ProjectSettings.GlobalizePath(FixtureVirtualRoot).Replace('\\', '/');
			Directory.CreateDirectory(rootDirectory);

			var packageDirectory = Path.Combine(rootDirectory, PackageName);
			Directory.CreateDirectory(packageDirectory);

			var bundleBytes = new byte[] { 0x00 };
			var bundleName = "verify_scene_bundle.ab";
			var bundleHash = ToLowerMd5(bundleBytes);
			var bundleCrc = HashUtility.BytesCRC32(bundleBytes);
			var bundleSize = (long)bundleBytes.Length;
			var bundleFilePath = Path.Combine(packageDirectory, bundleName);
			if (!File.Exists(bundleFilePath))
			{
				File.WriteAllBytes(bundleFilePath, bundleBytes);
			}

			var manifestBytes = BuildManifestBinary(PackageName, PackageVersion, SceneLocation, AssetLocation, bundleName, bundleHash, bundleCrc, bundleSize);
			var packageHash = ToLowerMd5(manifestBytes);

			var versionFileName = AssetSystemSettingsData.GetPackageVersionFileName(PackageName);
			var hashFileName = AssetSystemSettingsData.GetPackageHashFileName(PackageName, PackageVersion);
			var manifestFileName = AssetSystemSettingsData.GetManifestBinaryFileName(PackageName, PackageVersion);

			File.WriteAllText(Path.Combine(packageDirectory, versionFileName), PackageVersion, Encoding.UTF8);
			File.WriteAllText(Path.Combine(packageDirectory, hashFileName), packageHash, Encoding.UTF8);
			File.WriteAllBytes(Path.Combine(packageDirectory, manifestFileName), manifestBytes);

			return rootDirectory;
		}

		private static byte[] BuildManifestBinary(string packageName, string packageVersion, string sceneLocation, string assetLocation, string bundleName, string bundleHash, string bundleCrc, long bundleSize)
		{
			using var stream = new MemoryStream();
			using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);

			writer.Write(AssetSystemSettings.ManifestFileSign);
			WriteUtf8(writer, AssetSystemSettings.ManifestFileVersion);
			writer.Write(true); // EnableAddressable
			writer.Write(false); // LocationToLower
			writer.Write(false); // IncludeAssetGUID
			writer.Write(1); // OutputNameStyle : BundleName
			WriteUtf8(writer, "BuiltinBuildPipeline");
			WriteUtf8(writer, packageName);
			WriteUtf8(writer, packageVersion);

			// AssetList
			writer.Write(2);
			WriteUtf8(writer, sceneLocation); // Address
			WriteUtf8(writer, $"Assets/{sceneLocation}.scene"); // AssetPath
			WriteUtf8(writer, string.Empty); // AssetGUID
			WriteUtf8Array(writer, Array.Empty<string>()); // Tags
			writer.Write(0); // BundleID

			WriteUtf8(writer, assetLocation); // Address
			WriteUtf8(writer, $"Assets/{assetLocation}.prefab"); // AssetPath
			WriteUtf8(writer, string.Empty); // AssetGUID
			WriteUtf8Array(writer, Array.Empty<string>()); // Tags
			writer.Write(0); // BundleID

			// BundleList
			writer.Write(1);
			WriteUtf8(writer, bundleName);
			writer.Write((uint)0); // BundleCRC
			WriteUtf8(writer, bundleHash); // FileHash
			WriteUtf8(writer, bundleCrc); // FileCRC
			writer.Write(bundleSize); // FileSize
			writer.Write(false); // Encrypted
			WriteUtf8Array(writer, Array.Empty<string>()); // Tags
			WriteInt32Array(writer, Array.Empty<int>()); // DependIDs

			writer.Flush();
			return stream.ToArray();
		}

		private void VerifyBuiltinResourceLoad()
		{
			var physicalPath = ProjectSettings.GlobalizePath(BuiltinResourcePath).Replace('\\', '/');
			if (!File.Exists(physicalPath))
			{
				throw new InvalidOperationException($"Builtin resource is missing: {BuiltinResourcePath} -> {physicalPath}");
			}

			var resource = AssetSystemResources.Load<Resource>(BuiltinResourcePath);
			if (resource == null)
			{
				throw new InvalidOperationException($"Builtin resource load failed via Resources.Load: {BuiltinResourcePath}");
			}

			var resourceLabel = string.IsNullOrEmpty(resource.ResourceName) ? resource.GetType().Name : resource.ResourceName;
			GD.Print($"[AssetSystemRuntimeVerifier] builtin loaded: {resourceLabel}");
		}

		private static async Task VerifyAssetBundleLoadAsync(ResourcePackage package, string assetLocation)
		{
			var handle = package.LoadAssetAsync(assetLocation, typeof(object), priority: 100);
			await WaitHandleAsync(handle, "LoadAsset(AssetBundle)");
			if (handle.AssetObject == null)
			{
				throw new InvalidOperationException("LoadAsset(AssetBundle) returned null AssetObject.");
			}

			var assetName = handle.AssetObject.GetType().GetProperty("name")?.GetValue(handle.AssetObject)?.ToString()
							?? handle.AssetObject.GetType().GetProperty("Name")?.GetValue(handle.AssetObject)?.ToString()
							?? handle.AssetObject.GetType().Name;
			GD.Print($"[AssetSystemRuntimeVerifier] assetbundle loaded: {assetName}");
			handle.Release();
		}

		private static async Task WaitOperationAsync(AsyncOperationBase operation, string step)
		{
			while (!operation.IsDone)
			{
				await Task.Delay(1);
			}

			if (operation.Status != EOperationStatus.Succeed)
			{
				throw new InvalidOperationException($"{step} failed: {operation.Error}");
			}
		}

		private static async Task WaitHandleAsync(HandleBase handle, string step)
		{
			while (!handle.IsDone)
			{
				await Task.Delay(1);
			}

			if (handle.Status != EOperationStatus.Succeed)
			{
				throw new InvalidOperationException($"{step} failed: {handle.LastError}");
			}
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

		private sealed class LocalFileHttpTransport : IHttpTransport
		{
			private readonly GodotHttpTransport _fallback = new GodotHttpTransport();

			public Task<HttpResponse> GetTextAsync(string requestURL, int timeout, bool appendTimeTicks, CancellationToken cancellationToken)
			{
				if (TryResolveLocalFile(requestURL, out var localPath))
				{
					if (!File.Exists(localPath))
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
					if (!File.Exists(localPath))
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
	}
}
