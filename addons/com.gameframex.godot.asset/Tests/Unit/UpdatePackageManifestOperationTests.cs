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
        FSRequestPackageVersionOperation LoadLocalPackageVersionAsync(bool appendTimeTicks, int timeout);
        FSRequestPackageVersionOperation RequestRemotePackageVersionAsync(bool appendTimeTicks, int timeout);
        FSLoadPackageManifestOperation LoadLocalPackageManifestAsync(string packageVersion, int timeout);
        FSLoadPackageManifestOperation RequestRemotePackageManifestAsync(string packageVersion, int timeout);
    }

    internal abstract class FSRequestPackageVersionOperation : AsyncOperationBase
    {
        public string PackageVersion { get; protected set; } = string.Empty;
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
        public int LoadLocalVersionCount { get; private set; }
        public int RequestRemoteVersionCount { get; private set; }
        public int LoadLocalManifestCount { get; private set; }
        public int RequestRemoteManifestCount { get; private set; }
        public Queue<FSRequestPackageVersionOperation> LocalVersionOperations { get; } = new();
        public Queue<FSRequestPackageVersionOperation> RemoteVersionOperations { get; } = new();
        public Queue<FSLoadPackageManifestOperation> LocalManifestOperations { get; } = new();
        public FSLoadPackageManifestOperation? RemoteManifestOperation { get; set; }

        public FSRequestPackageVersionOperation LoadLocalPackageVersionAsync(bool appendTimeTicks, int timeout)
        {
            LoadLocalVersionCount++;
            if (LocalVersionOperations.Count == 0)
            {
                throw new System.InvalidOperationException("LocalVersionOperations is empty.");
            }

            var operation = LocalVersionOperations.Dequeue();
            operation.SetStart();
            return operation;
        }

        public FSRequestPackageVersionOperation RequestRemotePackageVersionAsync(bool appendTimeTicks, int timeout)
        {
            RequestRemoteVersionCount++;
            if (RemoteVersionOperations.Count == 0)
            {
                throw new System.InvalidOperationException("RemoteVersionOperations is empty.");
            }

            var operation = RemoteVersionOperations.Dequeue();
            operation.SetStart();
            return operation;
        }

        public FSLoadPackageManifestOperation LoadLocalPackageManifestAsync(string packageVersion, int timeout)
        {
            LoadLocalManifestCount++;
            if (LocalManifestOperations.Count == 0)
            {
                throw new System.InvalidOperationException("LocalManifestOperations is empty.");
            }

            var operation = LocalManifestOperations.Dequeue();
            operation.SetStart();
            return operation;
        }

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

    internal sealed class TestRequestPackageVersionOperation : FSRequestPackageVersionOperation
    {
        private readonly EOperationStatus _status;
        private readonly string _error;

        public TestRequestPackageVersionOperation(EOperationStatus status, string packageVersion, string error)
        {
            _status = status;
            PackageVersion = packageVersion;
            _error = error;
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
