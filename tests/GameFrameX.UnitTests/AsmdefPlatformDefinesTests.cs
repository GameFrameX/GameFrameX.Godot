using System.Collections.Generic;
using GameFrameX.Editor.Asmdef;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// platformDefines 多行文本格式互转测试（G4：属性编辑器的平台宏编辑输入解析）。
    /// </summary>
    public sealed class AsmdefPlatformDefinesTests
    {
        [Fact]
        public void Parse_And_Format_RoundTrip()
        {
            string text = "windows: GF_WIN, GF_DEBUG\nandroid: GF_ANDROID";

            Dictionary<string, List<string>> parsed = AsmdefPlatformDefines.Parse(text);
            string formatted = AsmdefPlatformDefines.Format(parsed);

            Assert.Equal(2, parsed.Count);
            Assert.Equal(new[] { "GF_WIN", "GF_DEBUG" }, parsed["windows"]);
            Assert.Equal(new[] { "GF_ANDROID" }, parsed["android"]);
            // Format 按平台键稳定排序（G3 去重稳定排序语义），roundtrip 后顺序归一
            Assert.Equal("android: GF_ANDROID\nwindows: GF_WIN, GF_DEBUG", formatted);
        }

        [Fact]
        public void Parse_SkipsMalformedLines()
        {
            // 冒号前缺平台名 / 整行无冒号 / 空行 —— 均忽略，不抛异常
            string text = ": GF_NO_PLATFORM\nno colon line\n\nwindows: GF_WIN";

            Dictionary<string, List<string>> parsed = AsmdefPlatformDefines.Parse(text);

            Assert.Single(parsed);
            Assert.Equal(new[] { "GF_WIN" }, parsed["windows"]);
        }

        [Fact]
        public void Parse_TrimsAndDeduplicatesDefines()
        {
            string text = "windows:  GF_A , GF_A , ,GF_B ";

            Dictionary<string, List<string>> parsed = AsmdefPlatformDefines.Parse(text);

            Assert.Equal(new[] { "GF_A", "GF_B" }, parsed["windows"]);
        }

        [Fact]
        public void Parse_EmptyOrNull_ReturnsEmptyMap()
        {
            Assert.Empty(AsmdefPlatformDefines.Parse(null));
            Assert.Empty(AsmdefPlatformDefines.Parse(string.Empty));
        }

        [Fact]
        public void Format_EmptyOrNull_ReturnsEmptyText()
        {
            Assert.Equal(string.Empty, AsmdefPlatformDefines.Format(null));
            Assert.Equal(string.Empty, AsmdefPlatformDefines.Format(new Dictionary<string, List<string>>()));
        }

        [Fact]
        public void Format_SkipsPlatformWithOnlyBlankDefines()
        {
            var map = new Dictionary<string, List<string>>
            {
                { "windows", new List<string> { "  ", string.Empty } },
                { "android", new List<string> { "GF_ANDROID" } }
            };

            string text = AsmdefPlatformDefines.Format(map);

            Assert.Equal("android: GF_ANDROID", text);
        }

        [Fact]
        public void Format_SortsPlatformsStably()
        {
            var map = new Dictionary<string, List<string>>
            {
                { "windows", new List<string> { "GF_WIN" } },
                { "android", new List<string> { "GF_ANDROID" } }
            };

            Assert.Equal("android: GF_ANDROID\nwindows: GF_WIN", AsmdefPlatformDefines.Format(map));
        }
    }
}
