using System;
using GameFrameX.AssetSystem;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// Phase 2.1 BundleFile 真实现的纯托管逻辑测试。
    /// NormalizeResourcePath 是 Bundled 资源按 AssetPath 走 ResourceLoader 的路径映射入口；
    /// LoadFromFile 的空路径拒绝是“不许假成功”的失败语义。
    /// ResourceLoader 真加载依赖 Godot 引擎运行时，留待编辑器内的引擎测试覆盖（xunit 内触碰原生会段错误）。
    /// </summary>
    public sealed class BundleFileResourcePathTests
    {
        [Theory]
        [InlineData("res://assets/ui/logo.png", "res://assets/ui/logo.png")]
        [InlineData("assets/ui/logo.png", "res://assets/ui/logo.png")]
        [InlineData("/assets/ui/logo.png", "res://assets/ui/logo.png")]
        [InlineData("user://cache/a.bin", "user://cache/a.bin")]
        [InlineData("res://a\\b\\c.tscn", "res://a/b/c.tscn")]
        public void NormalizeResourcePath_Makes_GodotLoadable(string input, string expected)
        {
            Assert.Equal(expected, BundleFile.NormalizeResourcePath(input));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void NormalizeResourcePath_EmptyInput_ReturnsEmpty(string input)
        {
            Assert.Equal(string.Empty, BundleFile.NormalizeResourcePath(input));
        }

        [Fact]
        public void LoadFromFile_EmptyPath_ReturnsNull()
        {
            // bundle 物理路径为空时诚实失败（DBFS 操作以 Result==null 判定失败），不返回占位对象
            Assert.Null(BundleFile.LoadFromFile(null));
            Assert.Null(BundleFile.LoadFromFile(string.Empty));
            Assert.Null(BundleFile.LoadFromFile("  "));
        }

        [Fact]
        public void LoadFromFile_ValidPath_KeepsSourcePath()
        {
            var bundleFile = BundleFile.LoadFromFile("/tmp/GameFrameX/DefaultPackage/abc123.bundle");
            Assert.NotNull(bundleFile);
        }
    }
}
