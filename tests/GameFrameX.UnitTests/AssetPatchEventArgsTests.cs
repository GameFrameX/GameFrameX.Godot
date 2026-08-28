using GameFrameX.Asset.Runtime;
using GameFrameX.Runtime;
using Xunit;

namespace GameFrameX.UnitTests
{
    [Collection("ReferencePool")]
    public class AssetPatchEventArgsTests
    {
        [Fact]
        public void Create_FillsFields()
        {
            var args = AssetDownloadProgressUpdateEventArgs.Create("DefaultPackage", 10, 3, 1024L, 256L);

            Assert.Equal("DefaultPackage", args.PackageName);
            Assert.Equal(10, args.TotalDownloadCount);
            Assert.Equal(3, args.CurrentDownloadCount);
            Assert.Equal(1024L, args.TotalDownloadSizeBytes);
            Assert.Equal(256L, args.CurrentDownloadSizeBytes);
        }

        [Fact]
        public void EventId_EqualsTypeFullName()
        {
            Assert.Equal("GameFrameX.Asset.Runtime.AssetDownloadProgressUpdateEventArgs", AssetDownloadProgressUpdateEventArgs.EventId);
            Assert.Equal("GameFrameX.Asset.Runtime.AssetPatchStatesChangeEventArgs", AssetPatchStatesChangeEventArgs.EventId);
        }

        [Fact]
        public void Release_ClearsAndReturnsToReferencePool()
        {
            var args = AssetPatchStatesChangeEventArgs.Create("Pkg", EPatchStates.DownloadWebFiles);

            ReferencePool.Release(args);
            var reused = ReferencePool.Acquire<AssetPatchStatesChangeEventArgs>();

            Assert.Same(args, reused);
            Assert.Null(reused.PackageName);
            Assert.Equal(EPatchStates.CreateDownloader, reused.CurrentStates);
        }

        [Fact]
        public void PatchStates_EnumOrderIsStable()
        {
            Assert.Equal(0, (int)EPatchStates.UpdateStaticVersion);
            Assert.Equal(1, (int)EPatchStates.UpdateManifest);
            Assert.Equal(2, (int)EPatchStates.CreateDownloader);
            Assert.Equal(3, (int)EPatchStates.DownloadWebFiles);
            Assert.Equal(4, (int)EPatchStates.PatchDone);
        }
    }
}
