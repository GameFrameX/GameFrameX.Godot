using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GameFrameX.AssetSystem;

namespace GameFrameX.AssetSystem.Editor
{
    /// <summary>
    /// Runtime 二进制 PackageManifest 只读解析器（编辑器 Reporter 展示用）。
    /// 字节格式与 Runtime 侧 DeserializeManifestOperation 以及 Plugin 侧 SerializeRuntimeManifestBinary 严格一致：
    /// UInt32 文件签名 + UTF8 文件版本 + 三个 Bool 配置 + Int32 命名样式 + UTF8 管线/包名/版本 + 资产列表 + 资源包列表；
    /// 其中 UTF8 字符串、UTF8 字符串数组、Int32 数组均使用 UInt16 长度前缀。
    /// ponytail: 只填充 AssetList/BundleList 线性数据，不构建 Runtime 用的 AssetDic/BundleDic/AssetPathMapping 索引；
    /// 若后续需要完整索引，升级路径是放开 Runtime 程序集 InternalsVisibleTo 后直接复用 DeserializeManifestOperation。
    /// TODO: IndependAssets / 重复资源检测依赖依赖分析，按 Phase 2.5 决策明确推迟，不在此实现。
    /// </summary>
    public static class AssetSystemManifestReader
    {
        /// <summary>
        /// 从磁盘读取并解析 Runtime 二进制清单
        /// </summary>
        public static bool TryReadManifestFile(string manifestPath, out PackageManifest manifest, out string errorMessage)
        {
            manifest = null;
            errorMessage = string.Empty;
            if (string.IsNullOrEmpty(manifestPath) || !File.Exists(manifestPath))
            {
                errorMessage = "Runtime 清单文件不存在。";
                return false;
            }
            byte[] binaryData;
            try
            {
                binaryData = File.ReadAllBytes(manifestPath);
            }
            catch (Exception exception)
            {
                errorMessage = exception.Message;
                return false;
            }
            return TryReadManifestBinary(binaryData, out manifest, out errorMessage);
        }

        /// <summary>
        /// 解析 Runtime 二进制清单数据
        /// </summary>
        public static bool TryReadManifestBinary(byte[] binaryData, out PackageManifest manifest, out string errorMessage)
        {
            manifest = null;
            errorMessage = string.Empty;
            if (binaryData == null || binaryData.Length == 0)
            {
                errorMessage = "Runtime 清单数据为空。";
                return false;
            }
            try
            {
                using (var stream = new MemoryStream(binaryData, false))
                {
                    using (var reader = new BinaryReader(stream, Encoding.UTF8))
                    {
                        var fileSign = reader.ReadUInt32();
                        if (fileSign != AssetSystemSettings.ManifestFileSign)
                        {
                            errorMessage = "Runtime 清单文件签名无效。";
                            return false;
                        }
                        manifest = new PackageManifest();
                        manifest.FileVersion = ReadUtf8(reader);
                        if (manifest.FileVersion != AssetSystemSettings.ManifestFileVersion)
                        {
                            errorMessage = $"Runtime 清单版本不兼容：{manifest.FileVersion}";
                            manifest = null;
                            return false;
                        }
                        manifest.EnableAddressable = reader.ReadBoolean();
                        manifest.LocationToLower = reader.ReadBoolean();
                        manifest.IncludeAssetGUID = reader.ReadBoolean();
                        manifest.OutputNameStyle = reader.ReadInt32();
                        manifest.BuildPipeline = ReadUtf8(reader);
                        manifest.PackageName = ReadUtf8(reader);
                        manifest.PackageVersion = ReadUtf8(reader);
                        var assetCount = reader.ReadInt32();
                        manifest.AssetList = new List<PackageAsset>(assetCount);
                        for (var index = 0; index < assetCount; index++)
                        {
                            var packageAsset = new PackageAsset();
                            packageAsset.Address = ReadUtf8(reader);
                            packageAsset.AssetPath = ReadUtf8(reader);
                            packageAsset.AssetGUID = ReadUtf8(reader);
                            packageAsset.AssetTags = ReadUtf8Array(reader);
                            packageAsset.BundleID = reader.ReadInt32();
                            manifest.AssetList.Add(packageAsset);
                        }
                        var bundleCount = reader.ReadInt32();
                        manifest.BundleList = new List<PackageBundle>(bundleCount);
                        for (var index = 0; index < bundleCount; index++)
                        {
                            var packageBundle = new PackageBundle();
                            packageBundle.BundleName = ReadUtf8(reader);
                            packageBundle.UnityCRC = reader.ReadUInt32();
                            packageBundle.FileHash = ReadUtf8(reader);
                            packageBundle.FileCRC = ReadUtf8(reader);
                            packageBundle.FileSize = reader.ReadInt64();
                            packageBundle.Encrypted = reader.ReadBoolean();
                            packageBundle.Tags = ReadUtf8Array(reader);
                            packageBundle.DependIDs = ReadInt32Array(reader);
                            packageBundle.ParseBundle(manifest);
                            manifest.BundleList.Add(packageBundle);
                        }
                        return true;
                    }
                }
            }
            catch (Exception exception)
            {
                manifest = null;
                errorMessage = $"Runtime 清单解析异常：{exception.Message}";
                return false;
            }
        }

        private static string ReadUtf8(BinaryReader reader)
        {
            var count = reader.ReadUInt16();
            if (count == 0)
            {
                return string.Empty;
            }
            return Encoding.UTF8.GetString(reader.ReadBytes(count));
        }

        private static string[] ReadUtf8Array(BinaryReader reader)
        {
            var count = reader.ReadUInt16();
            var values = new string[count];
            for (var index = 0; index < count; index++)
            {
                values[index] = ReadUtf8(reader);
            }
            return values;
        }

        private static int[] ReadInt32Array(BinaryReader reader)
        {
            var count = reader.ReadUInt16();
            var values = new int[count];
            for (var index = 0; index < count; index++)
            {
                values[index] = reader.ReadInt32();
            }
            return values;
        }
    }
}
