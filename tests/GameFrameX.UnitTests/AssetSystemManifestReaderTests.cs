using System.IO;
using System.Text;
using GameFrameX.AssetSystem;
using GameFrameX.AssetSystem.Editor;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// Phase 2.5 Reporter Runtime 二进制清单只读解析器测试：
    /// 按 SerializeRuntimeManifestBinary / DeserializeManifestOperation 的字节格式（UInt16 长度前缀 UTF8）手工构造数据，
    /// 锁定关键字段解析与错误降级行为。
    /// </summary>
    public sealed class AssetSystemManifestReaderTests
    {
        [Fact]
        public void TryReadManifestBinary_ParsesHeaderAssetsAndBundles()
        {
            var data = BuildManifestBytes();

            var success = AssetSystemManifestReader.TryReadManifestBinary(data, out var manifest, out var errorMessage);

            Assert.True(success);
            Assert.Equal(string.Empty, errorMessage);
            Assert.Equal(AssetSystemSettings.ManifestFileVersion, manifest.FileVersion);
            Assert.Equal("DefaultPackage", manifest.PackageName);
            Assert.Equal("20260828", manifest.PackageVersion);
            Assert.Equal("RawFileBuildPipeline", manifest.BuildPipeline);
            Assert.False(manifest.EnableAddressable);
            Assert.False(manifest.LocationToLower);
            Assert.False(manifest.IncludeAssetGUID);
            Assert.Single(manifest.AssetList);
            Assert.Single(manifest.BundleList);

            var asset = manifest.AssetList[0];
            Assert.Equal("assets/logo.png", asset.AssetPath);
            Assert.Equal("logo", asset.Address);
            Assert.Equal(0, asset.BundleID);
            Assert.NotNull(asset.AssetTags);
            Assert.Empty(asset.AssetTags);

            var bundle = manifest.BundleList[0];
            Assert.Equal("assets_logo.bundle", bundle.BundleName);
            Assert.Equal("hash123", bundle.FileHash);
            Assert.Equal(1024L, bundle.FileSize);
            Assert.False(bundle.Encrypted);
            Assert.Empty(bundle.DependIDs);
            // ParseBundle 生效：HashName 样式下 FileName 由 FileHash 派生
            Assert.Contains("hash123", bundle.FileName);
        }

        [Fact]
        public void TryReadManifestBinary_RejectsInvalidSignature()
        {
            var data = BuildManifestBytes();
            data[0] = 0x00;

            var success = AssetSystemManifestReader.TryReadManifestBinary(data, out var manifest, out var errorMessage);

            Assert.False(success);
            Assert.Null(manifest);
            Assert.NotEmpty(errorMessage);
        }

        [Fact]
        public void TryReadManifestBinary_RejectsTruncatedData()
        {
            var data = BuildManifestBytes();
            var truncated = new byte[data.Length / 2];
            using (var source = new MemoryStream(data, false))
            {
                source.Read(truncated, 0, truncated.Length);
            }

            var success = AssetSystemManifestReader.TryReadManifestBinary(truncated, out var manifest, out var errorMessage);

            Assert.False(success);
            Assert.Null(manifest);
            Assert.NotEmpty(errorMessage);
        }

        [Fact]
        public void TryReadManifestBinary_RejectsEmptyData()
        {
            var success = AssetSystemManifestReader.TryReadManifestBinary(new byte[0], out var manifest, out var errorMessage);

            Assert.False(success);
            Assert.Null(manifest);
            Assert.NotEmpty(errorMessage);
        }

        [Fact]
        public void TryReadManifestFile_MissingFile_DegradesGracefully()
        {
            var success = AssetSystemManifestReader.TryReadManifestFile("/nonexistent/path/package.bytes", out var manifest, out var errorMessage);

            Assert.False(success);
            Assert.Null(manifest);
            Assert.NotEmpty(errorMessage);
        }

        private static byte[] BuildManifestBytes()
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream, Encoding.UTF8))
                {
                    writer.Write(AssetSystemSettings.ManifestFileSign);
                    WriteUtf8(writer, AssetSystemSettings.ManifestFileVersion);
                    writer.Write(false);
                    writer.Write(false);
                    writer.Write(false);
                    writer.Write(0);
                    WriteUtf8(writer, "RawFileBuildPipeline");
                    WriteUtf8(writer, "DefaultPackage");
                    WriteUtf8(writer, "20260828");
                    writer.Write(1);
                    WriteUtf8(writer, "logo");
                    WriteUtf8(writer, "assets/logo.png");
                    WriteUtf8(writer, string.Empty);
                    WriteUtf8Array(writer, new string[0]);
                    writer.Write(0);
                    writer.Write(1);
                    WriteUtf8(writer, "assets_logo.bundle");
                    writer.Write(0u);
                    WriteUtf8(writer, "hash123");
                    WriteUtf8(writer, "314159");
                    writer.Write(1024L);
                    writer.Write(false);
                    WriteUtf8Array(writer, new string[0]);
                    WriteInt32Array(writer, new int[0]);
                    writer.Flush();
                }
                return stream.ToArray();
            }
        }

        private static void WriteUtf8(BinaryWriter writer, string value)
        {
            var bytes = string.IsNullOrEmpty(value) ? new byte[0] : Encoding.UTF8.GetBytes(value);
            writer.Write((ushort)bytes.Length);
            writer.Write(bytes);
        }

        private static void WriteUtf8Array(BinaryWriter writer, string[] values)
        {
            var count = values == null ? 0 : values.Length;
            writer.Write((ushort)count);
            for (var index = 0; index < count; index++)
            {
                WriteUtf8(writer, values[index]);
            }
        }

        private static void WriteInt32Array(BinaryWriter writer, int[] values)
        {
            var count = values == null ? 0 : values.Length;
            writer.Write((ushort)count);
            for (var index = 0; index < count; index++)
            {
                writer.Write(values[index]);
            }
        }
    }
}
