using System;
using System.Collections.Generic;
using System.Text.Json;
using GameFrameX.AssetSystem.Editor;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// Phase 2.4 assetsystem 构建器补充能力的纯托管测试:
    /// Collector 数据模型子集、Pack/Address 枚举策略、PCK 内路径映射、BuildinCatalog JSON 格式。
    /// </summary>
    public sealed class AssetSystemBuilderTests
    {
        [Fact]
        public void CollectorSetting_BuildHierarchy_And_GetPackage()
        {
            var setting = new AssetBundleCollectorSetting();
            var package = new AssetBundleCollectorPackage { PackageName = "DefaultPackage", PackageDesc = "desc" };
            var group = new AssetBundleCollectorGroup { GroupName = "UI", ActiveRule = EActiveRule.EnableGroup };
            var collector = new AssetBundleCollector
            {
                CollectPath = "res://Assets/UI",
                CollectorType = ECollectorType.MainAssetCollector,
                PackRule = EPackRule.PackDirectory
            };
            group.Collectors.Add(collector);
            package.Groups.Add(group);
            setting.Packages.Add(package);

            var found = setting.GetPackage("DefaultPackage");
            Assert.Same(package, found);
            Assert.True(group.IsActive());
            Assert.True(collector.IsValid());
            Assert.Equal(ECollectorType.MainAssetCollector, collector.CollectorType);
        }

        [Fact]
        public void CollectorSetting_GetPackage_Missing_Throws()
        {
            var setting = new AssetBundleCollectorSetting();
            Assert.ThrowsAny<Exception>(() => setting.GetPackage("nope"));
        }

        [Fact]
        public void Collector_Invalid_When_TypeNone_Or_EmptyPath()
        {
            var collector = new AssetBundleCollector { CollectPath = string.Empty };
            Assert.False(collector.IsValid());

            collector.CollectPath = "res://Assets/UI";
            collector.CollectorType = ECollectorType.None;
            Assert.False(collector.IsValid());

            collector.CollectorType = ECollectorType.DependAssetCollector;
            Assert.True(collector.IsValid());
        }

        [Fact]
        public void CollectorSetting_GetPackageAllTags_Collects_Group_And_Collector_Tags()
        {
            var setting = new AssetBundleCollectorSetting();
            var package = new AssetBundleCollectorPackage { PackageName = "DefaultPackage" };
            var group = new AssetBundleCollectorGroup { GroupName = "UI", AssetTags = "ui;widget" };
            group.Collectors.Add(new AssetBundleCollector { CollectPath = "res://Assets/UI", AssetTags = "texture" });
            package.Groups.Add(group);
            setting.Packages.Add(package);

            var tags = setting.GetPackageAllTags("DefaultPackage");
            Assert.Contains("ui", tags);
            Assert.Contains("widget", tags);
            Assert.Contains("texture", tags);
            Assert.Equal(3, tags.Count);
        }

        [Fact]
        public void PackRule_Separately_Directory_RawFile()
        {
            var assetPath = "res://Assets/UI/main.tscn";
            Assert.Equal("res://Assets/UI/main", CollectorRuleStrategy.GetPackBundleName(EPackRule.PackSeparately, assetPath));
            Assert.Equal("res://Assets/UI", CollectorRuleStrategy.GetPackBundleName(EPackRule.PackDirectory, assetPath));
            Assert.Equal("res://Assets/UI/main", CollectorRuleStrategy.GetPackBundleName(EPackRule.PackRawFile, assetPath));
        }

        [Fact]
        public void PackRule_Unsupported_Throws()
        {
            Assert.Throws<NotSupportedException>(() => CollectorRuleStrategy.GetPackBundleName(EPackRule.PackTopDirectory, "res://Assets/UI/main.tscn"));
        }

        [Fact]
        public void AddressRule_Disable_ByFileName_ByGroupAndFileName_ByAssetPath()
        {
            var assetPath = "res://Assets/UI/main.tscn";
            Assert.Equal(string.Empty, CollectorRuleStrategy.GetAssetAddress(EAddressRule.AddressDisable, assetPath, "UI"));
            Assert.Equal("main", CollectorRuleStrategy.GetAssetAddress(EAddressRule.AddressByFileName, assetPath, "UI"));
            Assert.Equal("UI_main", CollectorRuleStrategy.GetAssetAddress(EAddressRule.AddressByGroupAndFileName, assetPath, "UI"));
            Assert.Equal("res://Assets/UI/main.tscn", CollectorRuleStrategy.GetAssetAddress(EAddressRule.AddressByAssetPath, assetPath, "UI"));
        }

        [Fact]
        public void FilterRule_CollectScene_Matches_Tscn_Only()
        {
            Assert.True(CollectorRuleStrategy.IsMatchFilter(EFilterRule.CollectAll, "res://Assets/a.txt"));
            Assert.True(CollectorRuleStrategy.IsMatchFilter(EFilterRule.CollectScene, "res://Assets/main.TSCN"));
            Assert.False(CollectorRuleStrategy.IsMatchFilter(EFilterRule.CollectScene, "res://Assets/main.png"));
            Assert.True(CollectorRuleStrategy.IsMatchFilter(EFilterRule.CollectTexture, "res://Assets/tex.webp"));
            Assert.False(CollectorRuleStrategy.IsMatchFilter(EFilterRule.CollectTexture, "res://Assets/tex.tscn"));
            Assert.True(CollectorRuleStrategy.IsMatchFilter(EFilterRule.CollectShader, "res://Assets/shader.gdshader"));
            Assert.False(CollectorRuleStrategy.IsIgnored(EIgnoreRule.NormalIgnore, "res://Assets/a.png"));
            Assert.True(CollectorRuleStrategy.IsIgnored(EIgnoreRule.IgnoreAll, "res://Assets/a.png"));
        }

        [Fact]
        public void PckPath_Maps_To_Relative_SlashPath()
        {
            var root = "/tmp/GameFrameXBuild/Bundles";
            var innerPath = AssetSystemPckPathUtility.GetPckInnerPath(root + "/DefaultPackage/1.0.0/abc123.bundle", root);
            Assert.Equal("DefaultPackage/1.0.0/abc123.bundle", innerPath);

            var catalogPath = AssetSystemPckPathUtility.GetPckInnerPath(root + "/DefaultPackage/BuildinCatalog", root);
            Assert.Equal("DefaultPackage/BuildinCatalog", catalogPath);
        }

        [Fact]
        public void PckPath_OutsideRoot_Throws()
        {
            Assert.Throws<ArgumentException>(() => AssetSystemPckPathUtility.GetPckInnerPath("/other/abc.bundle", "/tmp/GameFrameXBuild/Bundles"));
            Assert.Throws<ArgumentException>(() => AssetSystemPckPathUtility.GetPckInnerPath(string.Empty, "/tmp/out"));
            Assert.Throws<ArgumentException>(() => AssetSystemPckPathUtility.GetPckInnerPath("/tmp/out/a.bundle", string.Empty));
        }

        [Fact]
        public void PckSourceInnerPath_Strips_ResPrefix()
        {
            // Phase 2.1 方向 a：源资源在 PCK 内按 res:// 相对路径存放，挂载后 ResourceLoader.Load(AssetPath) 直接命中
            Assert.Equal("assets/ui/logo.png", AssetSystemPckPathUtility.GetPckSourceInnerPath("res://assets/ui/logo.png"));
            Assert.Equal("scenes/main.tscn", AssetSystemPckPathUtility.GetPckSourceInnerPath("res://scenes/main.tscn"));
            Assert.Equal("a/b.bin", AssetSystemPckPathUtility.GetPckSourceInnerPath("res://a/b.bin"));
        }

        [Fact]
        public void PckSourceInnerPath_NonResPath_Throws()
        {
            Assert.Throws<ArgumentException>(() => AssetSystemPckPathUtility.GetPckSourceInnerPath("/abs/path/a.png"));
            Assert.Throws<ArgumentException>(() => AssetSystemPckPathUtility.GetPckSourceInnerPath(string.Empty));
            Assert.Throws<ArgumentException>(() => AssetSystemPckPathUtility.GetPckSourceInnerPath("user://a.png"));
        }

        [Fact]
        public void BuildinCatalog_Json_FieldNames_And_RoundTrip()
        {
            var entries = new List<BuildinCatalogFileEntry>
            {
                new BuildinCatalogFileEntry("hash0001", "hash0001.bundle"),
                new BuildinCatalogFileEntry("hash0002", "hash0002.bundle")
            };
            var json = BuildinCatalogUtility.BuildJson("DefaultPackage", "1.0.0", entries);

            // 字段名与运行时 DefaultBuildinFileCatalog 对齐:PackageName/PackageVersion/Wrappers[].BundleGUID/FileName
            using (var document = JsonDocument.Parse(json))
            {
                var root = document.RootElement;
                Assert.Equal("DefaultPackage", root.GetProperty("PackageName").GetString());
                Assert.Equal("1.0.0", root.GetProperty("PackageVersion").GetString());
                var wrappers = root.GetProperty("Wrappers");
                Assert.Equal(2, wrappers.GetArrayLength());
                Assert.Equal("hash0001", wrappers[0].GetProperty("BundleGUID").GetString());
                Assert.Equal("hash0001.bundle", wrappers[0].GetProperty("FileName").GetString());
                Assert.Equal("hash0002", wrappers[1].GetProperty("BundleGUID").GetString());
            }
        }

        [Fact]
        public void BuildinCatalog_Json_NullEntries_Produces_EmptyWrappers()
        {
            var json = BuildinCatalogUtility.BuildJson("DefaultPackage", "2.0.0", null);
            using (var document = JsonDocument.Parse(json))
            {
                Assert.Equal(0, document.RootElement.GetProperty("Wrappers").GetArrayLength());
            }
        }

        [Fact]
        public void CollectorSettingStore_Text_RoundTrip()
        {
            var setting = new AssetBundleCollectorSetting { UniqueBundleName = true };
            var package = new AssetBundleCollectorPackage { PackageName = "DefaultPackage", EnableAddressable = true };
            var group = new AssetBundleCollectorGroup { GroupName = "UI" };
            group.Collectors.Add(new AssetBundleCollector { CollectPath = "res://Assets/UI", PackRule = EPackRule.PackSeparately });
            package.Groups.Add(group);
            setting.Packages.Add(package);

            var text = AssetBundleCollectorSettingStore.SerializeToText(setting);
            var restored = AssetBundleCollectorSettingStore.DeserializeFromText(text);
            Assert.NotNull(restored);
            Assert.True(restored.UniqueBundleName);
            var restoredPackage = restored.GetPackage("DefaultPackage");
            Assert.True(restoredPackage.EnableAddressable);
            Assert.Equal(EPackRule.PackSeparately, restoredPackage.Groups[0].Collectors[0].PackRule);
        }

        [Fact]
        public void CollectorSettingStore_LoadOrCreate_MissingFile_Returns_Default()
        {
            var missingPath = "/tmp/GameFrameXUnitTests/missing_collector_setting_" + Guid.NewGuid().ToString("N") + ".json";
            var setting = AssetBundleCollectorSettingStore.LoadOrCreate(missingPath);
            Assert.NotNull(setting);
            Assert.Empty(setting.Packages);
        }
    }
}
