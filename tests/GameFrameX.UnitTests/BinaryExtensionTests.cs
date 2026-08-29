using System.IO;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// 二进制读写扩展测试（对照 Unity 基准 BinaryExtensionTests，7Bit 编解码 RoundTrip）。
    /// </summary>
    public sealed class BinaryExtensionTests
    {
        private static (int Result, int ByteCount) RoundTripInt32(int value)
        {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                writer.Write7BitEncodedInt32(value);
                writer.Flush();
                var length = (int)ms.Length;
                ms.Position = 0;
                using (var reader = new BinaryReader(ms))
                {
                    return (reader.Read7BitEncodedInt32(), length);
                }
            }
        }

        private static (uint Result, int ByteCount) RoundTripUInt32(uint value)
        {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                writer.Write7BitEncodedUInt32(value);
                writer.Flush();
                var length = (int)ms.Length;
                ms.Position = 0;
                using (var reader = new BinaryReader(ms))
                {
                    return (reader.Read7BitEncodedUInt32(), length);
                }
            }
        }

        private static long RoundTripInt64(long value)
        {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                writer.Write7BitEncodedInt64(value);
                writer.Flush();
                ms.Position = 0;
                using (var reader = new BinaryReader(ms))
                {
                    return reader.Read7BitEncodedInt64();
                }
            }
        }

        private static ulong RoundTripUInt64(ulong value)
        {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                writer.Write7BitEncodedUInt64(value);
                writer.Flush();
                ms.Position = 0;
                using (var reader = new BinaryReader(ms))
                {
                    return reader.Read7BitEncodedUInt64();
                }
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(127)]
        [InlineData(12345678)]
        [InlineData(-1)]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void WriteAndRead7BitEncodedInt32_RoundTrip(int value)
        {
            Assert.Equal(value, RoundTripInt32(value).Result);
        }

        [Theory]
        [InlineData(0u)]
        [InlineData(uint.MaxValue)]
        [InlineData(3000000000u)]
        public void WriteAndRead7BitEncodedUInt32_RoundTrip(uint value)
        {
            Assert.Equal(value, RoundTripUInt32(value).Result);
        }

        [Theory]
        [InlineData(0L)]
        [InlineData(long.MaxValue)]
        [InlineData(long.MinValue)]
        [InlineData(4611686018427387904L)]
        public void WriteAndRead7BitEncodedInt64_RoundTrip(long value)
        {
            Assert.Equal(value, RoundTripInt64(value));
        }

        [Theory]
        [InlineData(0UL)]
        [InlineData(ulong.MaxValue)]
        public void WriteAndRead7BitEncodedUInt64_RoundTrip(ulong value)
        {
            Assert.Equal(value, RoundTripUInt64(value));
        }

        [Fact]
        public void Write7BitEncodedInt32_SmallValue_UsesSingleByte()
        {
            Assert.Equal(1, RoundTripInt32(127).ByteCount);
        }

        [Fact]
        public void Write7BitEncodedInt32_LargeValue_UsesMultipleBytes()
        {
            Assert.Equal(4, RoundTripInt32(12345678).ByteCount); // 2^21 < 12345678 < 2^28 → 4 字节
        }
    }
}
