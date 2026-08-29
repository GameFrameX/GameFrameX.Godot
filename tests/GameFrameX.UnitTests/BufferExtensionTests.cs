using System;
using System.Linq;
using Xunit;
using static BufferExtension; // BufferExtension 位于全局命名空间

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// 字节数组缓冲区扩展测试（对照 Unity 基准 BufferExtensionTests 的 RoundTrip 部分）。
    /// 迁移备注：Unity 基准中的越界抛异常用例不迁移——Godot 版 WriteInt 越界为静默跳过、
    /// ReadInt 仅在 offset 远超长度时抛出，行为与 Unity 基准不同（已在迁移报告中备案）。
    /// </summary>
    public sealed class BufferExtensionTests
    {
        [Fact]
        public void WriteAndReadInt_RoundTrip()
        {
            var buffer = new byte[IntSize];
            int offset = 0;
            buffer.WriteInt(-12345, ref offset);
            Assert.Equal(IntSize, offset);

            offset = 0;
            Assert.Equal(-12345, buffer.ReadInt(ref offset));
        }

        [Fact]
        public void WriteAndReadInt_MaxValue_RoundTrip()
        {
            var buffer = new byte[IntSize];
            int offset = 0;
            buffer.WriteInt(int.MaxValue, ref offset);

            offset = 0;
            Assert.Equal(int.MaxValue, buffer.ReadInt(ref offset));
        }

        [Fact]
        public void WriteAndReadUInt_RoundTrip()
        {
            var buffer = new byte[UIntSize];
            int offset = 0;
            buffer.WriteUInt(uint.MaxValue, ref offset);

            offset = 0;
            Assert.Equal(uint.MaxValue, buffer.ReadUInt(ref offset));
        }

        [Fact]
        public void WriteAndReadShort_RoundTrip()
        {
            var buffer = new byte[ShortSize];
            int offset = 0;
            buffer.WriteShort(-300, ref offset);

            offset = 0;
            Assert.Equal((short)-300, buffer.ReadShort(ref offset));
        }

        [Fact]
        public void WriteAndReadUShort_RoundTrip()
        {
            var buffer = new byte[UShortSize];
            int offset = 0;
            buffer.WriteUShort(ushort.MaxValue, ref offset);

            offset = 0;
            Assert.Equal(ushort.MaxValue, buffer.ReadUShort(ref offset));
        }

        [Fact]
        public void WriteAndReadLong_RoundTrip()
        {
            var buffer = new byte[LongSize];
            int offset = 0;
            buffer.WriteLong(long.MaxValue, ref offset);

            offset = 0;
            Assert.Equal(long.MaxValue, buffer.ReadLong(ref offset));
        }

        [Theory]
        [InlineData(3.14f)]
        [InlineData(0f)]
        [InlineData(-99.5f)]
        public void WriteAndReadFloat_RoundTrip(float value)
        {
            var buffer = new byte[FloatSize];
            int offset = 0;
            buffer.WriteFloat(value, ref offset);

            offset = 0;
            Assert.Equal(value, buffer.ReadFloat(ref offset));
        }

        [Fact]
        public void WriteAndReadDouble_RoundTrip()
        {
            var buffer = new byte[DoubleSize];
            int offset = 0;
            buffer.WriteDouble(2.718281828459045, ref offset);

            offset = 0;
            Assert.Equal(2.718281828459045, buffer.ReadDouble(ref offset));
        }

        [Fact]
        public void WriteAndReadByte_RoundTrip()
        {
            var buffer = new byte[1];
            int offset = 0;
            buffer.WriteByte(0xAB, ref offset);

            offset = 0;
            Assert.Equal((byte)0xAB, buffer.ReadByte(ref offset));
        }

        [Fact]
        public void WriteAndReadSByte_RoundTrip()
        {
            var buffer = new byte[1];
            int offset = 0;
            buffer.WriteSByte(-5, ref offset);

            offset = 0;
            Assert.Equal((sbyte)-5, buffer.ReadSByte(ref offset));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void WriteAndReadBool_RoundTrip(bool value)
        {
            var buffer = new byte[1];
            int offset = 0;
            buffer.WriteBool(value, ref offset);

            offset = 0;
            Assert.Equal(value, buffer.ReadBool(ref offset));
        }

        [Fact]
        public void WriteAndReadBytes_RoundTrip()
        {
            var payload = new byte[] { 1, 2, 3, 4, 5 };
            var buffer = new byte[IntSize + payload.Length];
            int offset = 0;
            buffer.WriteBytes(payload, ref offset);

            offset = 0;
            Assert.Equal(payload, buffer.ReadBytes(ref offset));
        }

        [Fact]
        public void WriteBytes_Null_WritesZeroLength()
        {
            var buffer = new byte[IntSize];
            int offset = 0;
            buffer.WriteBytes(null, ref offset);
            Assert.Equal(IntSize, offset);

            offset = 0;
            Assert.Empty(buffer.ReadBytes(ref offset));
        }

        [Fact]
        public void ReadBytes_WithExplicitOffsetAndLen_ReturnsSlice()
        {
            var buffer = new byte[] { 9, 1, 2, 3, 9 };

            Assert.Equal(new byte[] { 1, 2, 3 }, buffer.ReadBytes(1, 3));
        }

        [Fact]
        public void ReadBytes_ZeroLen_ReturnsEmpty()
        {
            var buffer = new byte[] { 1, 2, 3 };

            Assert.Empty(buffer.ReadBytes(0, 0));
        }

        [Fact]
        public void ReadBytes_WithRefOffset_AdvancesOffset()
        {
            var buffer = new byte[] { 1, 2, 3, 4 };
            int offset = 1;

            Assert.Equal(new byte[] { 2, 3 }, buffer.ReadBytes(ref offset, 2));
            Assert.Equal(3, offset);
        }

        [Fact]
        public void WriteBytesWithoutLength_WritesRawBytes()
        {
            var payload = new byte[] { 0xAA, 0xBB };
            var buffer = new byte[payload.Length + 4]; // 实现预检统一预留 IntSize
            int offset = 0;
            buffer.WriteBytesWithoutLength(payload, ref offset);

            Assert.Equal(payload, buffer[0..payload.Length]);
            Assert.Equal(payload.Length, offset);
        }

        [Fact]
        public void WriteBytesWithoutLength_Null_WritesZeroInt()
        {
            var buffer = new byte[4];
            int offset = 0;
            buffer.WriteBytesWithoutLength(null, ref offset);

            Assert.All(buffer, b => Assert.Equal(0, b));
            Assert.Equal(4, offset); // null 走 WriteInt(0)，offset 推进 IntSize
        }

        [Fact]
        public void WriteAndReadString_RoundTrip()
        {
            var buffer = new byte[128];
            int offset = 0;
            buffer.WriteString("hello 你好", ref offset);

            offset = 0;
            Assert.Equal("hello 你好", buffer.ReadString(ref offset));
        }

        [Fact]
        public void ToHex_Default_ReturnsContinuousHexString()
        {
            Assert.Equal("010203", new byte[] { 1, 2, 3 }.ToHex());
        }
    }
}
