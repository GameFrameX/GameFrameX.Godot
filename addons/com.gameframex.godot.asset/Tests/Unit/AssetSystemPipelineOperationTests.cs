using Xunit;

namespace GameFrameX.Asset.Tests.Unit
{
    public sealed class AssetSystemPipelineOperationTests
    {
        [Fact]
        public void RequestPackageVersion_ShouldRetryAndSucceed_OnSecondAttempt()
        {
            var fileSystem = new YooAsset.TestFileSystem();
            fileSystem.RemoteVersionOperations.Enqueue(new YooAsset.TestRequestPackageVersionOperation(YooAsset.EOperationStatus.Failed, string.Empty, "timeout"));
            fileSystem.RemoteVersionOperations.Enqueue(new YooAsset.TestRequestPackageVersionOperation(YooAsset.EOperationStatus.Succeed, "v2", string.Empty));

            var operation = new YooAsset.RequestPackageVersionImplOperation(fileSystem, appendTimeTicks: true, timeout: 10);
            RunToDone(operation);

            Assert.Equal(YooAsset.EOperationStatus.Succeed, operation.Status);
            Assert.Equal("v2", operation.PackageVersion);
            Assert.Equal(2, fileSystem.RequestRemoteVersionCount);
        }

        [Fact]
        public void LoadLocalManifest_ShouldFallbackToBuildinLocalManifest_WhenCacheMissing()
        {
            var playMode = new YooAsset.TestPlayMode(new YooAsset.PackageManifest
            {
                PackageName = "TestPackage",
                PackageVersion = "v0"
            });

            var fileSystem = new YooAsset.TestFileSystem
            {
                PackageName = "TestPackage",
                FileRoot = "tmp/cache"
            };
            fileSystem.LocalManifestOperations.Enqueue(new YooAsset.TestLoadPackageManifestOperation(YooAsset.EOperationStatus.Failed, null, "cache missing"));
            fileSystem.LocalManifestOperations.Enqueue(new YooAsset.TestLoadPackageManifestOperation(YooAsset.EOperationStatus.Succeed, new YooAsset.PackageManifest
            {
                PackageName = "TestPackage",
                PackageVersion = "v2"
            }, string.Empty));

            var operation = new YooAsset.LoadLocalManifestImplOperation(playMode, fileSystem, fileSystem, packageVersion: "v2", timeout: 10);
            RunToDone(operation);

            Assert.Equal(YooAsset.EOperationStatus.Succeed, operation.Status);
            Assert.Equal("v2", playMode.ActiveManifest.PackageVersion);
            Assert.Equal(2, fileSystem.LoadLocalManifestCount);
            Assert.Equal(0, fileSystem.RequestRemoteManifestCount);
        }

        [Fact]
        public void Pipeline_ShouldRun_InitVersionManifestDownloadLoad()
        {
            var initOp = new TestStepOperation(YooAsset.EOperationStatus.Succeed, string.Empty);
            RunToDone(initOp);
            Assert.Equal(YooAsset.EOperationStatus.Succeed, initOp.Status);

            var fileSystem = new YooAsset.TestFileSystem
            {
                PackageName = "TestPackage",
                FileRoot = "tmp/cache",
                RemoteManifestOperation = new YooAsset.TestLoadPackageManifestOperation(YooAsset.EOperationStatus.Succeed, new YooAsset.PackageManifest
                {
                    PackageName = "TestPackage",
                    PackageVersion = "v2"
                }, string.Empty)
            };
            fileSystem.RemoteVersionOperations.Enqueue(new YooAsset.TestRequestPackageVersionOperation(YooAsset.EOperationStatus.Succeed, "v2", string.Empty));

            var playMode = new YooAsset.TestPlayMode(new YooAsset.PackageManifest
            {
                PackageName = "TestPackage",
                PackageVersion = "v1"
            });

            var requestVersionOp = new YooAsset.RequestPackageVersionImplOperation(fileSystem, appendTimeTicks: false, timeout: 10);
            RunToDone(requestVersionOp);
            Assert.Equal("v2", requestVersionOp.PackageVersion);

            var updateManifestOp = new YooAsset.UpdatePackageManifestImplOperation(playMode, fileSystem, requestVersionOp.PackageVersion, 10);
            RunToDone(updateManifestOp);
            Assert.Equal(YooAsset.EOperationStatus.Succeed, updateManifestOp.Status);
            Assert.Equal("v2", playMode.ActiveManifest.PackageVersion);

            var downloadOp = new TestStepOperation(YooAsset.EOperationStatus.Succeed, string.Empty);
            RunToDone(downloadOp);
            Assert.Equal(YooAsset.EOperationStatus.Succeed, downloadOp.Status);

            var loadOp = new TestLoadOperation(playMode.ActiveManifest);
            RunToDone(loadOp);
            Assert.Equal(YooAsset.EOperationStatus.Succeed, loadOp.Status);
            Assert.Equal("Asset@TestPackage@v2", loadOp.LoadedAssetToken);
        }

        private static void RunToDone(YooAsset.AsyncOperationBase operation)
        {
            operation.SetStart();
            for (var i = 0; i < 16 && operation.IsDone == false; i++)
            {
                operation.InternalOnUpdate();
            }
        }

        private sealed class TestStepOperation : YooAsset.AsyncOperationBase
        {
            private readonly YooAsset.EOperationStatus _status;
            private readonly string _error;

            public TestStepOperation(YooAsset.EOperationStatus status, string error)
            {
                _status = status;
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

        private sealed class TestLoadOperation : YooAsset.AsyncOperationBase
        {
            private readonly YooAsset.PackageManifest _manifest;

            public TestLoadOperation(YooAsset.PackageManifest manifest)
            {
                _manifest = manifest;
            }

            public string LoadedAssetToken { get; private set; } = string.Empty;

            public override void InternalOnStart()
            {
            }

            public override void InternalOnUpdate()
            {
                if (_manifest == null || string.IsNullOrEmpty(_manifest.PackageVersion))
                {
                    Status = YooAsset.EOperationStatus.Failed;
                    Error = "manifest is invalid";
                    return;
                }

                LoadedAssetToken = $"Asset@{_manifest.PackageName}@{_manifest.PackageVersion}";
                Status = YooAsset.EOperationStatus.Succeed;
            }
        }
    }
}
