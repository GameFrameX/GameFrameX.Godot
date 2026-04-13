using System;
using System.Collections.Generic;
using System.IO;
using GameFrameX.AssetSystem;
using GameFrameX.Runtime;
using Godot;

namespace Godot.Hotfix.AssetSystem
{
	internal static class GodotPckResourceLoader
	{
		private const string DefaultBuilderRoot = "user://hotfix";
		private static readonly Dictionary<string, string> MountedPackagePathMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

		internal static bool EnsurePackageMounted(string packageName, out string mountedPhysicalPath)
		{
			mountedPhysicalPath = string.Empty;
			if (string.IsNullOrWhiteSpace(packageName))
			{
				return false;
			}

			if (MountedPackagePathMap.TryGetValue(packageName, out var cachedPath) && File.Exists(cachedPath))
			{
				mountedPhysicalPath = cachedPath;
				return true;
			}

			var candidates = BuildPckCandidates(packageName);
			for (var i = 0; i < candidates.Count; i++)
			{
				var physicalPath = ResolvePhysicalPath(candidates[i]);
				if (string.IsNullOrWhiteSpace(physicalPath) || !File.Exists(physicalPath))
				{
					continue;
				}

				if (ProjectSettings.LoadResourcePack(physicalPath, false, 0))
				{
					MountedPackagePathMap[packageName] = physicalPath;
					mountedPhysicalPath = physicalPath;
					Log.Info("[HotfixPCK] package mounted: {0} -> {1}", packageName, physicalPath);
					return true;
				}

				Log.Warning("[HotfixPCK] package mount failed: {0}", physicalPath);
			}

			Log.Warning("[HotfixPCK] package not found or mount failed: {0}", packageName);
			return false;
		}

		internal static string TryResolveResourcePathFromPackage(string packageName, params string[] resourcePathCandidates)
		{
			if (!EnsurePackageMounted(packageName, out _))
			{
				return string.Empty;
			}

			var dedupe = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			for (var i = 0; i < resourcePathCandidates.Length; i++)
			{
				foreach (var resourcePath in ExpandResourcePathCandidates(resourcePathCandidates[i]))
				{
					if (!dedupe.Add(resourcePath))
					{
						continue;
					}

					if (AssetSystemResources.Load<Resource>(resourcePath) == null)
					{
						continue;
					}

					return resourcePath;
				}
			}

			return string.Empty;
		}

		internal static TResource LoadResourceFromPackage<TResource>(string packageName, string[] resourcePathCandidates, out string resolvedResourcePath)
			where TResource : Resource
		{
			resolvedResourcePath = TryResolveResourcePathFromPackage(packageName, resourcePathCandidates);
			if (string.IsNullOrWhiteSpace(resolvedResourcePath))
			{
				return null;
			}

			return AssetSystemResources.Load<TResource>(resolvedResourcePath);
		}

		private static List<string> BuildPckCandidates(string packageName)
		{
			var result = new List<string>(8);
			var dedupe = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			var safePackageName = MakeSafeFileName(packageName);
			var packagesRoot = GodotAssetPath.GetHotfixPackagesRootVirtual();
			AddCandidate(result, dedupe, $"{packagesRoot}/{safePackageName}.pck");
			if (!string.Equals(safePackageName, packageName, StringComparison.Ordinal))
			{
				AddCandidate(result, dedupe, $"{packagesRoot}/{packageName}.pck");
			}

			var searchRoots = new[]
			{
				DefaultBuilderRoot
			};
			for (var rootIndex = 0; rootIndex < searchRoots.Length; rootIndex++)
			{
				var builderRootPath = ResolvePhysicalPath(searchRoots[rootIndex]);
				if (string.IsNullOrWhiteSpace(builderRootPath) || Directory.Exists(builderRootPath) == false)
				{
					continue;
				}

				var allPckFiles = Directory.GetFiles(builderRootPath, "*.pck", SearchOption.AllDirectories);
				Array.Sort(allPckFiles, static (left, right) =>
					File.GetLastWriteTimeUtc(right).CompareTo(File.GetLastWriteTimeUtc(left)));
				for (var i = 0; i < allPckFiles.Length; i++)
				{
					var fileName = Path.GetFileNameWithoutExtension(allPckFiles[i]);
					if (!string.Equals(fileName, packageName, StringComparison.OrdinalIgnoreCase) &&
						!string.Equals(fileName, safePackageName, StringComparison.OrdinalIgnoreCase))
					{
						continue;
					}

					AddCandidate(result, dedupe, allPckFiles[i]);
				}
			}

			return result;
		}

		private static void AddCandidate(List<string> result, HashSet<string> dedupe, string path)
		{
			if (string.IsNullOrWhiteSpace(path))
			{
				return;
			}

			var normalized = path.Replace('\\', '/').Trim();
			if (!dedupe.Add(normalized))
			{
				return;
			}

			result.Add(normalized);
		}

		private static IEnumerable<string> ExpandResourcePathCandidates(string rawCandidate)
		{
			if (string.IsNullOrWhiteSpace(rawCandidate))
			{
				yield break;
			}

			var normalized = rawCandidate.Trim().Replace('\\', '/').TrimStart('/');
			foreach (var basePath in ExpandBasePathCandidates(normalized))
			{
				if (Path.HasExtension(basePath))
				{
					yield return basePath;
					continue;
				}

				yield return basePath;
				yield return basePath + ".png";
				yield return basePath + ".jpg";
				yield return basePath + ".jpeg";
				yield return basePath + ".webp";
				yield return basePath + ".bmp";
				yield return basePath + ".tga";
			}
		}

		private static IEnumerable<string> ExpandBasePathCandidates(string normalizedCandidate)
		{
			if (normalizedCandidate.StartsWith("res://", StringComparison.OrdinalIgnoreCase))
			{
				yield return normalizedCandidate;
				yield break;
			}

			if (normalizedCandidate.StartsWith("user://", StringComparison.OrdinalIgnoreCase))
			{
				yield return normalizedCandidate;
				yield break;
			}

			yield return $"res://{normalizedCandidate}";
			yield return $"res://Assets/{normalizedCandidate}";
			yield return $"res://Assets/Probe/{normalizedCandidate}";
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

		private static string MakeSafeFileName(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				return "package";
			}

			var invalidChars = Path.GetInvalidFileNameChars();
			var chars = value.ToCharArray();
			for (var i = 0; i < chars.Length; i++)
			{
				for (var j = 0; j < invalidChars.Length; j++)
				{
					if (chars[i] != invalidChars[j])
					{
						continue;
					}

					chars[i] = '_';
					break;
				}
			}

			return new string(chars);
		}
	}
}
