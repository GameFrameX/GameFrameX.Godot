using Xunit;

namespace GameFrameX.Asset.Tests.Unit
{
    public sealed class UpdatePackageManifestOperationTests
    {
    [Fact]
    public void Update_ShouldShortCircuit_WhenActiveManifestVersionMatches()
    {
        var activeManifest = new YooAsset.PackageManifest
        {
            PackageName = "TestPackage",
            PackageVersion = "v1"
        };
        var playMode = new YooAsset.TestPlayMode(activeManifest);
        var fileSystem = new YooAsset.TestFileSystem
        {
            PackageName = "TestPackage",
            FileRoot = "/tmp/test-assetsystem",
            RemoteManifestOperation = new YooAsset.TestLoadPackageManifestOperation(
                YooAsset.EOperationStatus.Succeed,
                new YooAsset.PackageManifest
                {
                    PackageName = "TestPackage",
                    PackageVersion = "v1"
                },
                string.Empty)
        };

        var operation = new YooAsset.UpdatePackageManifestImplOperation(playMode, fileSystem, "v1", 60);
        operation.SetStart();
        operation.InternalOnUpdate();

        Assert.Equal(YooAsset.EOperationStatus.Succeed, operation.Status);
        Assert.Equal(0, fileSystem.RequestRemoteManifestCount);
        Assert.Same(activeManifest, playMode.ActiveManifest);
    }

    [Fact]
    public void Update_ShouldFallbackToPreviousManifest_WhenRemoteManifestLoadFailed()
    {
        var activeManifest = new YooAsset.PackageManifest
        {
            PackageName = "TestPackage",
            PackageVersion = "v1"
        };
        var playMode = new YooAsset.TestPlayMode(activeManifest);
        var fileSystem = new YooAsset.TestFileSystem
        {
            PackageName = "TestPackage",
            FileRoot = "/tmp/test-assetsystem",
            RemoteManifestOperation = new YooAsset.TestLoadPackageManifestOperation(
                YooAsset.EOperationStatus.Failed,
                null,
                "manifest damaged")
        };

        var operation = new YooAsset.UpdatePackageManifestImplOperation(playMode, fileSystem, "v2", 60);
        operation.SetStart();
        operation.InternalOnUpdate();
        operation.InternalOnUpdate();

        Assert.Equal(YooAsset.EOperationStatus.Failed, operation.Status);
        Assert.Equal("manifest damaged", operation.Error);
        Assert.Equal(1, fileSystem.RequestRemoteManifestCount);
        Assert.Same(activeManifest, playMode.ActiveManifest);
    }

    [Fact]
    public void Update_ShouldFailAndFallback_WhenRemoteManifestVersionMismatch()
    {
        var activeManifest = new YooAsset.PackageManifest
        {
            PackageName = "TestPackage",
            PackageVersion = "v1"
        };
        var playMode = new YooAsset.TestPlayMode(activeManifest);
        var fileSystem = new YooAsset.TestFileSystem
        {
            PackageName = "TestPackage",
            FileRoot = "/tmp/test-assetsystem",
            RemoteManifestOperation = new YooAsset.TestLoadPackageManifestOperation(
                YooAsset.EOperationStatus.Succeed,
                new YooAsset.PackageManifest
                {
                    PackageName = "TestPackage",
                    PackageVersion = "v3"
                },
                string.Empty)
        };

        var operation = new YooAsset.UpdatePackageManifestImplOperation(playMode, fileSystem, "v2", 60);
        operation.SetStart();
        operation.InternalOnUpdate();
        operation.InternalOnUpdate();

        Assert.Equal(YooAsset.EOperationStatus.Failed, operation.Status);
        Assert.Contains("version mismatch", operation.Error);
        Assert.Same(activeManifest, playMode.ActiveManifest);
    }
    }
}

namespace YooAsset
{
    internal static class DefaultCacheFileSystemDefine
    {
        public const string ManifestFilesFolderName = "ManifestFiles";
        public static string PackageVersion { get; set; } = string.Empty;
    }

    internal static class YooAssetSettingsData
    {
        public static string GetPackageVersionFileName(string packageName)
        {
            return $"{packageName}.version";
        }
    }

    internal interface IPlayMode
    {
        PackageManifest ActiveManifest { get; set; }
    }

    internal interface IFileSystem
    {
        string PackageName { get; }
        string FileRoot { get; }
        FSLoadPackageManifestOperation RequestRemotePackageManifestAsync(string packageVersion, int timeout);
    }

    internal abstract class FSLoadPackageManifestOperation : AsyncOperationBase
    {
        internal PackageManifest? Manifest { get; set; }
    }

    public class PackageManifest
    {
        public string PackageName { get; set; } = string.Empty;
        public string PackageVersion { get; set; } = string.Empty;
    }

    internal sealed class TestPlayMode : IPlayMode
    {
        public TestPlayMode(PackageManifest activeManifest)
        {
            ActiveManifest = activeManifest;
        }

        public PackageManifest ActiveManifest { get; set; }
    }

    internal sealed class TestFileSystem : IFileSystem
    {
        public string PackageName { get; set; } = string.Empty;
        public string FileRoot { get; set; } = string.Empty;
        public int RequestRemoteManifestCount { get; private set; }
        public FSLoadPackageManifestOperation? RemoteManifestOperation { get; set; }

        public FSLoadPackageManifestOperation RequestRemotePackageManifestAsync(string packageVersion, int timeout)
        {
            RequestRemoteManifestCount++;
            if (RemoteManifestOperation == null)
            {
                throw new System.InvalidOperationException("RemoteManifestOperation is null.");
            }

            RemoteManifestOperation.SetStart();
            return RemoteManifestOperation;
        }
    }

    internal sealed class TestLoadPackageManifestOperation : FSLoadPackageManifestOperation
    {
        private readonly EOperationStatus _status;
        private readonly string _error;

        public TestLoadPackageManifestOperation(EOperationStatus status, PackageManifest? manifest, string error)
        {
            _status = status;
            _error = error;
            Manifest = manifest;
        }

        public override void InternalOnStart()
        {
            Status = _status;
            Error = _error;
        }

        public override void InternalOnUpdate()
        {
            Status = _status;
            Error = _error;
        }
    }
}
