using GameFrameX.Runtime;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// Utility.Hash 哈希计算测试（与 Unity 版共同行为基线：小写十六进制输出）。
    /// </summary>
    public sealed class UtilityHashTests
    {
        [Theory]
        [InlineData("", "d41d8cd98f00b204e9800998ecf8427e")]
        [InlineData("abc", "900150983cd24fb0d6963f7d28e17f72")]
        [InlineData("The quick brown fox jumps over the lazy dog", "9e107d9d372bb6826bd81d3542a419d6")]
        public void Md5Hash_ReturnsKnownVectors(string input, string expected)
        {
            Assert.Equal(expected, Utility.Hash.MD5.Hash(input));
        }

        [Fact]
        public void Md5Hash_IsDeterministic()
        {
            var first = Utility.Hash.MD5.Hash("deterministic");
            var second = Utility.Hash.MD5.Hash("deterministic");
            Assert.Equal(first, second);
        }

        [Fact]
        public void Md5Hash_StreamOverload_MatchesStringOverload()
        {
            using (var stream = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes("stream input")))
            {
                Assert.Equal(Utility.Hash.MD5.Hash("stream input"), Utility.Hash.MD5.Hash(stream));
            }
        }
    }
}
