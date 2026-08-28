using System;
using System.Collections.Generic;
using GameFrameX.AssetSystem;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// Database 系列 Provider 的纯逻辑行为测试。
    /// EditorSimulate 模式下 DatabaseSubAssetsProvider 将“子资产”语义映射为
    /// “加载同路径主资源 + BundleAssetLoadUtility.FilterByType 类型过滤”，
    /// 这里锁定该映射依赖的类型过滤语义（纯托管逻辑，不触碰 Godot 原生运行时）；
    /// Provider 状态机本身依赖 ResourceLoader / ResourceManager（Godot 原生），留待编辑器内的引擎测试覆盖。
    /// </summary>
    public sealed class DatabaseProviderTypeFilterTests
    {
        [Fact]
        public void FilterByType_WildcardType_ReturnsInputUnchanged()
        {
            var assets = new object[] { "a", 1, new object() };

            Assert.Same(assets, BundleAssetLoadUtility.FilterByType(assets, null));
            Assert.Same(assets, BundleAssetLoadUtility.FilterByType(assets, typeof(object)));
        }

        [Fact]
        public void FilterByType_TypeMismatch_YieldsEmptyArrayNotNull()
        {
            // SubAssets 语义：主资源类型与请求类型不匹配时结果是空集合而非 null（空集合按 YooAsset 语义算成功）
            var filtered = BundleAssetLoadUtility.FilterByType(new object[] { "hello" }, typeof(int));

            Assert.NotNull(filtered);
            Assert.Empty(filtered);
        }

        [Fact]
        public void FilterByType_KeepsMatchesAndSkipsNullEntries()
        {
            var filtered = BundleAssetLoadUtility.FilterByType(new object[] { "a", "b", null, 42 }, typeof(string));

            Assert.Equal(new object[] { "a", "b" }, filtered);
        }

        [Fact]
        public void FilterByType_NullOrEmptyInput_ReturnsAsIs()
        {
            Assert.Null(BundleAssetLoadUtility.FilterByType(null, typeof(string)));

            var empty = new object[0];
            Assert.Same(empty, BundleAssetLoadUtility.FilterByType(empty, typeof(string)));
        }

        [Fact]
        public void IsTypeMatch_FollowsAssignableFromSemantics()
        {
            // DatabaseAssetProvider 用 IsTypeMatch 校验 ResourceLoader.Load 的结果：
            // null 资源对象视为失败；通配类型（null / object）非空即成功；基类或接口请求可以命中派生实例。
            Assert.False(BundleAssetLoadUtility.IsTypeMatch(null, null));
            Assert.False(BundleAssetLoadUtility.IsTypeMatch(null, typeof(string)));

            Assert.True(BundleAssetLoadUtility.IsTypeMatch("x", null));
            Assert.True(BundleAssetLoadUtility.IsTypeMatch("x", typeof(object)));
            Assert.True(BundleAssetLoadUtility.IsTypeMatch(new List<int>(), typeof(IEnumerable<int>)));

            Assert.False(BundleAssetLoadUtility.IsTypeMatch("x", typeof(int)));
        }
    }
}
