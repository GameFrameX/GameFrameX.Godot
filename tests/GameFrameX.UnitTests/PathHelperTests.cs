using GameFrameX.Runtime;
using Xunit;

namespace GameFrameX.UnitTests
{
    // Combine 内部使用静态 StringBuilder（非线程安全共享状态），挂 ReferencePool 串行集合避免并行用例互踩
    [Collection("ReferencePool")]
    public class PathHelperTests
    {
        [Theory]
        [InlineData("/Users/blank/cache", "img.png", "/Users/blank/cache/img.png")]
        [InlineData("user://data", "save.json", "user://data/save.json")]
        public void Combine_AbsoluteSegment_AppendsSeparator(string first, string second, string expected)
        {
            // 修复回归：以 "/" 开头且不以分隔符结尾的绝对路径段曾被误吞分隔符，产生 "...cacheimg.png" 式粘连
            Assert.Equal(expected, PathHelper.Combine(first, second));
        }

        [Theory]
        [InlineData("dir/", "file.png", "dir/file.png")]
        [InlineData("dir", "file.png", "dir/file.png")]
        [InlineData("/", "file.png", "/file.png")]
        public void Combine_SeparatorSemantics_Unchanged(string first, string second, string expected)
        {
            // 尾斜杠段不重复追加、纯 "/" 段行为保持（EndsWith 分支已覆盖，删除 StartsWith 分支不影响）
            Assert.Equal(expected, PathHelper.Combine(first, second));
        }

        [Fact]
        public void Combine_ThreeSegments_Chains()
        {
            Assert.Equal("a/b/c", PathHelper.Combine("a", "b", "c"));
        }
    }
}
