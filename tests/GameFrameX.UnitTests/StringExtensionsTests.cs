using System;
using System.Linq;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// 字符串扩展测试（对照 Unity 基准 StringExtensionsTests；类名沿用 Godot 侧 StringExtension）。
    /// </summary>
    public sealed class StringExtensionsTests
    {
        [Fact]
        public void EqualsFast_BothNull_ReturnsTrue()
        {
            string a = null;
            string b = null;

            Assert.True(a.EqualsFast(b));
        }

        [Fact]
        public void EqualsFast_SelfNull_TargetNonNull_ReturnsFalse()
        {
            string a = null;

            Assert.False(a.EqualsFast("hello"));
        }

        [Fact]
        public void EqualsFast_TargetNull_ReturnsFalse()
        {
            Assert.False("hello".EqualsFast(null));
        }

        [Fact]
        public void EqualsFast_EmptyStrings_ReturnsTrue()
        {
            Assert.True(string.Empty.EqualsFast(string.Empty));
        }

        [Theory]
        [InlineData("hello", "hello", true)]
        [InlineData("hello", "world", false)]
        [InlineData("hello world", "hello", false)]
        public void EqualsFast_General(string a, string b, bool expected)
        {
            Assert.Equal(expected, a.EqualsFast(b));
        }

        [Theory]
        [InlineData("hello world", "world", true)]
        [InlineData("hello world", "hello", false)]
        [InlineData("hi", "hello", false)]
        [InlineData("hello", "", true)]
        [InlineData("hello", "hello", true)]
        public void EndsWithFast_General(string self, string target, bool expected)
        {
            Assert.Equal(expected, self.EndsWithFast(target));
        }

        [Fact]
        public void EndsWithFast_NullSelf_ReturnsFalse()
        {
            string self = null;

            Assert.False(self.EndsWithFast("test"));
        }

        [Fact]
        public void EndsWithFast_NullTarget_ReturnsFalse()
        {
            Assert.False("test".EndsWithFast(null));
        }

        [Theory]
        [InlineData("hello world", "hello", true)]
        [InlineData("hello world", "world", false)]
        [InlineData("hi", "hello", false)]
        [InlineData("hello", "", true)]
        [InlineData("hello", "hello", true)]
        public void StartsWithFast_General(string self, string target, bool expected)
        {
            Assert.Equal(expected, self.StartsWithFast(target));
        }

        [Fact]
        public void StartsWithFast_NullSelf_ReturnsFalse()
        {
            string self = null;

            Assert.False(self.StartsWithFast("test"));
        }

        [Fact]
        public void StartsWithFast_NullTarget_ReturnsFalse()
        {
            Assert.False("test".StartsWithFast(null));
        }

        [Fact]
        public void ToBytes_ReturnsByteArray()
        {
            Assert.Equal(new byte[] { 104, 101, 108, 108, 111 }, "hello".ToBytes().ToArray());
        }

        [Fact]
        public void ToByteArray_ReturnsByteArray()
        {
            Assert.Equal(new byte[] { 104, 101, 108, 108, 111 }, "hello".ToByteArray());
        }

        [Fact]
        public void HexToBytes_ValidHex_ReturnsCorrectBytes()
        {
            Assert.Equal(new byte[] { 0xDE, 0xAD, 0xBE, 0xEF }, "DEADBEEF".HexToBytes());
        }

        [Fact]
        public void HexToBytes_LowercaseHex_Works()
        {
            Assert.Equal(new byte[] { 0xDE, 0xAD }, "dead".HexToBytes());
        }

        [Fact]
        public void HexToBytes_AllZeros_ReturnsZeroBytes()
        {
            Assert.Equal(new byte[] { 0, 0 }, "0000".HexToBytes());
        }

        [Fact]
        public void HexToBytes_OddLength_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => "ABC".HexToBytes());
        }

        [Fact]
        public void HexToBytes_EmptyString_ReturnsEmptyArray()
        {
            Assert.Empty("".HexToBytes());
        }

        [Theory]
        [InlineData(null, true)]
        [InlineData("", true)]
        [InlineData("   ", true)]
        [InlineData("abc", false)]
        public void IsNullOrWhiteSpace_General(string value, bool expected)
        {
            Assert.Equal(expected, value.IsNullOrWhiteSpace());
        }

        [Theory]
        [InlineData(null, true)]
        [InlineData("", true)]
        [InlineData("   ", false)]
        [InlineData("abc", false)]
        public void IsNullOrEmpty_General(string value, bool expected)
        {
            Assert.Equal(expected, value.IsNullOrEmpty());
        }

        [Theory]
        [InlineData("abc", true)]
        [InlineData(null, false)]
        [InlineData("", false)]
        public void IsNotNullOrWhiteSpace_General(string value, bool expected)
        {
            Assert.Equal(expected, value.IsNotNullOrWhiteSpace());
        }

        [Theory]
        [InlineData("abc", true)]
        [InlineData(null, false)]
        [InlineData("", false)]
        public void IsNotNullOrEmpty_General(string value, bool expected)
        {
            Assert.Equal(expected, value.IsNotNullOrEmpty());
        }

        [Fact]
        public void Format_WithArgs_FormatsCorrectly()
        {
            Assert.Equal("a=1, b=two", "a={0}, b={1}".Format(1, "two"));
        }

        [Fact]
        public void Format_NoArgs_ReturnsOriginal()
        {
            Assert.Equal("original", "original".Format());
        }

        [Fact]
        public void TrimEmpty_RemovesWhitespaceChars()
        {
            Assert.Equal("helloworld", " hello world ".TrimEmpty());
        }

        [Fact]
        public void TrimEmpty_NoWhitespace_ReturnsOriginal()
        {
            Assert.Equal("hello", "hello".TrimEmpty());
        }

        [Fact]
        public void TrimEmpty_AllWhitespace_ReturnsEmpty()
        {
            Assert.Equal(string.Empty, "   ".TrimEmpty());
        }
    }
}
