using System;
using System.IO;
using Xunit;

namespace GameFrameX.Asset.Tests.Unit
{
    public sealed class UnityResourceLoadingCompatibilityTests
    {
        [Fact]
        public void ResourcesLoad_ShouldReturnPlaceholderObject_WhenFileExists()
        {
            var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.txt");
            File.WriteAllText(tempFile, "placeholder");

            try
            {
                var loaded = UnityEngine.Resources.Load<UnityEngine.Object>(tempFile);
                Assert.NotNull(loaded);
                Assert.Equal(Path.GetFileNameWithoutExtension(tempFile), loaded.name);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        [Fact]
        public void AssetBundleLoadAssetAsync_ShouldReturnPlaceholderObject()
        {
            var bundle = UnityEngine.AssetBundle.LoadFromFile("dummy_bundle.ab");
            var request = bundle.LoadAssetAsync("Assets/verify_asset.prefab", typeof(UnityEngine.Object));

            Assert.NotNull(request);
            Assert.True(request.isDone);
            Assert.NotNull(request.asset);
        }
    }
}
