using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using GameFrameX.AssetSystem;

namespace GameFrameX.EngineTests
{
    /// <summary>
    /// Runtime 二进制清单 fixture 写入器。
    /// 字节格式与 Plugin 侧 SerializeRuntimeManifestBinary / Runtime 侧 DeserializeManifestOperation 一致：
    /// UInt32 签名 + UTF8 版本 + 三个 Bool + Int32 命名样式 + UTF8 管线/包名/版本 + 资产列表 + 资源包列表；
    /// UTF8 字符串/字符串数组/Int32 数组使用 UInt16 长度前缀。
    /// </summary>
    public static class ManifestFixtureWriter
    {
        public sealed class FixtureAsset
        {
            public string Address;
            public string AssetPath;
            public int BundleId;

            public FixtureAsset(string address, string assetPath, int bundleId)
            {
                Address = address;
                AssetPath = assetPath;
                BundleId = bundleId;
            }
        }

        public sealed class FixtureBundle
        {
            public string BundleName;
            public string FileHash;
            public long FileSize;
            public string SourceFilePath;

            public FixtureBundle(string bundleName, string sourceFilePath)
            {
                BundleName = bundleName;
                SourceFilePath = sourceFilePath;
            }
        }

        public static byte[] WriteManifestBinary(string buildPipeline, string packageName, string packageVersion, List<FixtureAsset> assets, List<FixtureBundle> bundles)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
                {
                    writer.Write(AssetSystemSettings.ManifestFileSign);
                    WriteUtf8(writer, AssetSystemSettings.ManifestFileVersion);
                    writer.Write(true);
                    writer.Write(false);
                    writer.Write(false);
                    writer.Write(1);
                    WriteUtf8(writer, buildPipeline);
                    WriteUtf8(writer, packageName);
                    WriteUtf8(writer, packageVersion);
                    writer.Write(assets.Count);
                    foreach (var asset in assets)
                    {
                        WriteUtf8(writer, asset.Address);
                        WriteUtf8(writer, asset.AssetPath);
                        WriteUtf8(writer, string.Empty);
                        WriteUtf8Array(writer, null);
                        writer.Write(asset.BundleId);
                    }

                    writer.Write(bundles.Count);
                    foreach (var bundle in bundles)
                    {
                        var bundleBytes = ReadSourceBytes(bundle.SourceFilePath);
                        WriteUtf8(writer, bundle.BundleName);
                        writer.Write((uint)0);
                        WriteUtf8(writer, ToLowerMd5(bundleBytes));
                        WriteUtf8(writer, HashUtility.BytesCRC32(bundleBytes));
                        writer.Write(bundleBytes == null ? 0L : bundleBytes.Length);
                        writer.Write(false);
                        WriteUtf8Array(writer, null);
                        WriteInt32Array(writer, null);
                    }

                    writer.Flush();
                }

                return stream.ToArray();
            }
        }

        public static string ToLowerMd5(byte[] data)
        {
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(data == null ? new byte[0] : data);
                var builder = new StringBuilder(hash.Length * 2);
                foreach (var item in hash)
                {
                    builder.Append(item.ToString("x2"));
                }

                return builder.ToString();
            }
        }

        private static byte[] ReadSourceBytes(string sourceFilePath)
        {
            if (string.IsNullOrEmpty(sourceFilePath) || File.Exists(sourceFilePath) == false)
            {
                return new byte[0];
            }

            return File.ReadAllBytes(sourceFilePath);
        }

        private static void WriteUtf8(BinaryWriter writer, string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value ?? string.Empty);
            if (bytes.Length > ushort.MaxValue)
            {
                throw new InvalidOperationException("fixture 字符串超长");
            }

            writer.Write((ushort)bytes.Length);
            writer.Write(bytes);
        }

        private static void WriteUtf8Array(BinaryWriter writer, string[] values)
        {
            if (values == null || values.Length == 0)
            {
                writer.Write((ushort)0);
                return;
            }

            if (values.Length > ushort.MaxValue)
            {
                throw new InvalidOperationException("fixture 字符串数组超长");
            }

            writer.Write((ushort)values.Length);
            foreach (var value in values)
            {
                WriteUtf8(writer, value);
            }
        }

        private static void WriteInt32Array(BinaryWriter writer, int[] values)
        {
            if (values == null || values.Length == 0)
            {
                writer.Write((ushort)0);
                return;
            }

            writer.Write((ushort)values.Length);
            foreach (var value in values)
            {
                writer.Write(value);
            }
        }
    }
}
