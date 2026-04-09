using GameFrameX.AssetSystem;
using Godot;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using GameFrameX.Runtime;

namespace Godot.Startup.AssetSystem
{
#if INCLUDE_ASSETSYSTEM_RUNTIME
	/// <summary>
	/// 资源更新入口（版本、清单、下载）。
	/// 说明：这里只处理更新链路，不承担业务资源实例化与PCK挂载。
	/// </summary>
	public static class AssetPackageUpdateService
	{
		/// <summary>
		/// 尝试准备本地Host模式资源包（默认对齐正式热更永久目录）。
		/// 默认目录：
		///  - 包根：user://hotfix/yoo/{packageName}
		///  - 缓存：user://hotfix/cache/{packageName}
		/// </summary>
		public static bool TryPrepareLocalHostPackage(
			string packageName,
			out ResourcePackage package,
			out string error,
			string packageRootVirtual = null,
			string cacheRootVirtual = null)
		{
			package = null;
			error = string.Empty;

			if (string.IsNullOrWhiteSpace(packageName))
			{
				error = "packageName is null or empty.";
				return false;
			}

			try
			{
				global::GameFrameX.AssetSystem.AssetSystem.Initialize();
				packageRootVirtual ??= GodotAssetPath.GetHotfixYooRootVirtual();
				cacheRootVirtual ??= GodotAssetPath.GetHotfixCacheRootVirtual();

				var packageVirtualPath = GodotAssetPath.CombineVirtualPath(packageRootVirtual, GodotAssetPath.NormalizePackageSegment(packageName));
				var packageRoot = ResolvePhysicalPath(packageVirtualPath);
				if (Directory.Exists(packageRoot) == false)
				{
					error = $"package root not found: {packageRoot}";
					return false;
				}

				packageRoot = ResolvePackageContentRoot(packageRoot);
				if (Directory.Exists(packageRoot) == false)
				{
					error = $"package content root not found: {packageRoot}";
					return false;
				}
				Log.Info("[AssetSystem] local host package root resolved. package={0} root={1}", packageName, packageRoot);

				var cacheVirtualPath = GodotAssetPath.CombineVirtualPath(cacheRootVirtual, GodotAssetPath.NormalizePackageSegment(packageName));
				var cacheRoot = ResolvePhysicalPath(cacheVirtualPath);
				Directory.CreateDirectory(cacheRoot);

				package = global::GameFrameX.AssetSystem.AssetSystem.TryGetPackage(packageName) ?? global::GameFrameX.AssetSystem.AssetSystem.CreatePackage(packageName);
				if (package.InitializeStatus != EOperationStatus.Succeed)
				{
					var remoteServices = new LocalDirectoryRemoteServices(packageRoot);
					var initializeParameters = new HostPlayModeParameters
					{
						// The Godot builder writes raw-file manifests for the version/update layer.
						// Keep this file system in raw mode; otherwise LoadRawFileSync tries to load BundleFile objects.
						CacheFileSystemParameters = FileSystemParameters.CreateDefaultCacheRawFileSystemParameters(remoteServices, rootDirectory: cacheRoot)
					};

					var initializeOperation = package.InitializeAsync(initializeParameters);
					if (WaitForAssetSystemOperation(initializeOperation, 10, out var initializeWaitError) == false)
					{
						error = $"initialize wait failed: {initializeWaitError}";
						return false;
					}

					if (initializeOperation.Status != EOperationStatus.Succeed)
					{
						error = $"initialize failed: {initializeOperation.Error}";
						return false;
					}
				}

				var versionOperation = package.RequestPackageVersionAsync(appendTimeTicks: false, timeout: 10);
				if (WaitForAssetSystemOperation(versionOperation, 10, out var versionWaitError) == false)
				{
					error = $"request version wait failed: {versionWaitError}";
					return false;
				}

				if (versionOperation.Status != EOperationStatus.Succeed)
				{
					error = $"request version failed: {versionOperation.Error}";
					return false;
				}
				Log.Info("[AssetSystem] local host package version ready. package={0} version={1}", packageName, versionOperation.PackageVersion);

				var manifestOperation = package.UpdatePackageManifestAsync(versionOperation.PackageVersion, timeout: 10);
				if (WaitForAssetSystemOperation(manifestOperation, 10, out var manifestWaitError) == false)
				{
					error = $"update manifest wait failed: {manifestWaitError}";
					return false;
				}

				if (manifestOperation.Status != EOperationStatus.Succeed)
				{
					error = $"update manifest failed: {manifestOperation.Error}";
					return false;
				}
				Log.Info("[AssetSystem] local host package manifest ready. package={0}", packageName);

				global::GameFrameX.AssetSystem.AssetSystem.SetDefaultPackage(package);
				return true;
			}
			catch (Exception exception)
			{
				error = exception.Message;
				return false;
			}
		}

		public static ResourcePackage PreparePackage(string packageName, IHttpTransport transport = null)
		{
			global::GameFrameX.AssetSystem.AssetSystem.Initialize();
			if (transport != null)
			{
				global::GameFrameX.AssetSystem.AssetSystem.SetDownloadSystemHttpTransport(transport);
			}

			var package = global::GameFrameX.AssetSystem.AssetSystem.TryGetPackage(packageName) ?? global::GameFrameX.AssetSystem.AssetSystem.CreatePackage(packageName);
			global::GameFrameX.AssetSystem.AssetSystem.SetDefaultPackage(package);
			return package;
		}

		public static InitializationOperation BeginInitializeWeb(ResourcePackage package, string webRootDirectory)
		{
			var initializeParameters = new WebPlayModeParameters
			{
				WebFileSystemParameters = new FileSystemParameters(typeof(DefaultWebFileSystem).FullName, webRootDirectory)
			};
			return package.InitializeAsync(initializeParameters);
		}

		public static InitializationOperation BeginInitializeHost(ResourcePackage package, IRemoteServices remoteServices, string cacheRootDirectory)
		{
			var initializeParameters = new HostPlayModeParameters
			{
				CacheFileSystemParameters = FileSystemParameters.CreateDefaultCacheFileSystemParameters(remoteServices, rootDirectory: cacheRootDirectory)
			};
			return package.InitializeAsync(initializeParameters);
		}

		public static RequestPackageVersionOperation BeginRequestPackageVersion(ResourcePackage package, bool appendTimeTicks = false, int timeout = 10)
		{
			return package.RequestPackageVersionAsync(appendTimeTicks, timeout);
		}

		public static UpdatePackageManifestOperation BeginUpdatePackageManifest(ResourcePackage package, string packageVersion, int timeout = 10)
		{
			return package.UpdatePackageManifestAsync(packageVersion, timeout);
		}

		public static ResourceDownloaderOperation BeginCreateDownloader(ResourcePackage package, int downloadingMaxNumber = 1, int failedTryAgain = 0, int timeout = 10)
		{
			var downloader = package.CreateResourceDownloader(downloadingMaxNumber, failedTryAgain, timeout);
			downloader.BeginDownload();
			return downloader;
		}

		private static string ResolvePhysicalPath(string path)
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				return string.Empty;
			}

			if (path.StartsWith("res://", StringComparison.OrdinalIgnoreCase) ||
				path.StartsWith("user://", StringComparison.OrdinalIgnoreCase))
			{
				return ProjectSettings.GlobalizePath(path).Replace('\\', '/');
			}

			return path.Replace('\\', '/');
		}

		private static bool WaitForAssetSystemOperation(AsyncOperationBase operation, double timeoutSeconds, out string error)
		{
			error = string.Empty;
			if (operation == null)
			{
				error = "operation is null.";
				return false;
			}

			var stopwatch = Stopwatch.StartNew();
			while (operation.IsDone == false)
			{
				global::GameFrameX.AssetSystem.AssetSystem.Tick();
				if (stopwatch.Elapsed.TotalSeconds >= timeoutSeconds)
				{
					error = $"operation timeout. type={operation.GetType().Name} timeout={timeoutSeconds:0.##}s status={operation.Status} error={operation.Error}";
					return false;
				}

				Thread.Sleep(1);
			}

			return true;
		}

		private static string ResolvePackageContentRoot(string packageRoot)
		{
			if (string.IsNullOrWhiteSpace(packageRoot) || Directory.Exists(packageRoot) == false)
			{
				return packageRoot;
			}

			var directManifest = Directory.GetFiles(packageRoot, "PackageManifest_*.bytes", SearchOption.TopDirectoryOnly);
			if (directManifest.Length > 0)
			{
				return packageRoot;
			}

			var packageNameDirectory = Directory.GetDirectories(packageRoot);
			if (packageNameDirectory.Length != 1)
			{
				return packageRoot;
			}

			var versionRoot = packageNameDirectory[0];
			var versionDirectories = Directory.GetDirectories(versionRoot);
			if (versionDirectories.Length == 0)
			{
				return versionRoot;
			}

			Array.Sort(versionDirectories, StringComparer.OrdinalIgnoreCase);
			for (var i = versionDirectories.Length - 1; i >= 0; i--)
			{
				var candidate = versionDirectories[i];
				var manifestFiles = Directory.GetFiles(candidate, "PackageManifest_*.bytes", SearchOption.TopDirectoryOnly);
				if (manifestFiles.Length > 0)
				{
					return candidate.Replace('\\', '/');
				}
			}

			return versionRoot.Replace('\\', '/');
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
#endif
}
