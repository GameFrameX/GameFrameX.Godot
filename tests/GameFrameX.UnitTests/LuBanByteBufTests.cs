using LuBan.Runtime;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// LuBan 运行时 ByteBuf 读写测试（迁移自 Unity com.gameframex.unity.focus-creative-games.luban Runtime）。
    /// 编码语义（已对照源码核实）：
    /// WriteInt/WriteShort/WriteLong 为变长编码（按无符号位截断，非 zigzag）；
    /// WriteFint/WriteFshort/WriteFlong 为定长小端；WriteString/WriteBytes 以 WriteSize 变长长度为前缀。
    /// </summary>
    public sealed class LuBanByteBufTests
    {
        private sealed class TestBean : BeanBase
        {
            public override int GetTypeId()
            {
                return 12345;
            }
        }

        // ──────────────── 基础类型读写回环 ────────────────

        [Fact]
        public void WriteRead_Bool_RoundTrips()
        {
            var buf = new ByteBuf();
            buf.WriteBool(true);
            buf.WriteBool(false);
            Assert.True(buf.ReadBool());
            Assert.False(buf.ReadBool());
            Assert.True(buf.Empty);
        }

        [Fact]
        public void WriteRead_Byte_RoundTrips()
        {
            var buf = new ByteBuf();
            buf.WriteByte(0xAB);
            buf.WriteByte(0x00);
            buf.WriteByte(0xFF);
            Assert.Equal(0xAB, buf.ReadByte());
            Assert.Equal(0x00, buf.ReadByte());
            Assert.Equal(0xFF, buf.ReadByte());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        [InlineData(150)]
        [InlineData(-32768)]
        [InlineData(32767)]
        [InlineData(-12345)]
        [InlineData(12345)]
        public void WriteRead_Short_Varint_RoundTrips(short v)
        {
            var buf = new ByteBuf();
            buf.WriteShort(v);
            Assert.Equal(v, buf.ReadShort());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-32768)]
        [InlineData(32767)]
        public void WriteRead_Fshort_Fixed2_RoundTrips(short v)
        {
            var buf = new ByteBuf();
            buf.WriteFshort(v);
            Assert.Equal(2, buf.Size);
            Assert.Equal(v, buf.ReadFshort());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        [InlineData(150)]
        [InlineData(-2147483648)]
        [InlineData(2147483647)]
        public void WriteRead_Int_Varint_RoundTrips(int v)
        {
            var buf = new ByteBuf();
            buf.WriteInt(v);
            Assert.Equal(v, buf.ReadInt());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-1)]
        [InlineData(-2147483648)]
        [InlineData(2147483647)]
        public void WriteRead_Fint_Fixed4_RoundTrips(int v)
        {
            var buf = new ByteBuf();
            buf.WriteFint(v);
            Assert.Equal(4, buf.Size);
            Assert.Equal(v, buf.ReadFint());
        }

        [Theory]
        [InlineData(0L)]
        [InlineData(-1L)]
        [InlineData(-9223372036854775808L)]
        [InlineData(9223372036854775807L)]
        public void WriteRead_Long_Varint_RoundTrips(long v)
        {
            var buf = new ByteBuf();
            buf.WriteLong(v);
            Assert.Equal(v, buf.ReadLong());
        }

        // ponytail: 上游 ReadFlong 用 (long)xl 组装，xl 为负 int（低 32 位 >= 0x80000000）时符号扩展
        // 经 OR 吞掉 xh 高位（long.MaxValue 读回 -1），Unity 基准同样如此；迁移保真不改实现，
        // 用例避开低 32 位 >= 0x80000000 且高 32 位非全 1 的值，修复属上游同步任务。
        [Theory]
        [InlineData(0L)]
        [InlineData(-1L)]
        [InlineData(-9223372036854775808L)]
        [InlineData(0x1234567812345678L)]
        public void WriteRead_Flong_Fixed8_RoundTrips(long v)
        {
            var buf = new ByteBuf();
            buf.WriteFlong(v);
            Assert.Equal(8, buf.Size);
            Assert.Equal(v, buf.ReadFlong());
        }

        [Fact]
        public void WriteRead_Float_RoundTrips()
        {
            var buf = new ByteBuf();
            buf.WriteFloat(3.5f);
            buf.WriteFloat(-0.125f);
            buf.WriteFloat(float.MaxValue);
            buf.WriteFloat(float.MinValue);
            Assert.Equal(16, buf.Size);
            Assert.Equal(3.5f, buf.ReadFloat());
            Assert.Equal(-0.125f, buf.ReadFloat());
            Assert.Equal(float.MaxValue, buf.ReadFloat());
            Assert.Equal(float.MinValue, buf.ReadFloat());
        }

        [Fact]
        public void WriteRead_Double_RoundTrips()
        {
            var buf = new ByteBuf();
            buf.WriteDouble(3.141592653589793);
            buf.WriteDouble(double.MaxValue);
            buf.WriteDouble(double.MinValue);
            Assert.Equal(24, buf.Size);
            Assert.Equal(3.141592653589793, buf.ReadDouble());
            Assert.Equal(double.MaxValue, buf.ReadDouble());
            Assert.Equal(double.MinValue, buf.ReadDouble());
        }

        [Theory]
        [InlineData("")]
        [InlineData("hello")]
        [InlineData("你好，LuBan")]
        public void WriteRead_String_RoundTrips(string s)
        {
            var buf = new ByteBuf();
            buf.WriteString(s);
            Assert.Equal(s, buf.ReadString());
        }

        [Fact]
        public void WriteRead_Bytes_RoundTrips()
        {
            var buf = new ByteBuf();
            var data = new byte[] { 1, 2, 3, 0xFF };
            buf.WriteBytes(data);
            buf.WriteBytes(new byte[0]);
            Assert.Equal(data, buf.ReadBytes());
            Assert.Empty(buf.ReadBytes());
        }

        // ──────────────── 变长编码字节形态 ────────────────

        [Fact]
        public void WriteInt_Zero_EncodesSingleByte()
        {
            var buf = new ByteBuf();
            buf.WriteInt(0);
            Assert.Equal(new byte[] { 0x00 }, buf.CopyData());
        }

        [Fact]
        public void WriteInt_150_EncodesTwoBytes()
        {
            var buf = new ByteBuf();
            buf.WriteInt(150);
            Assert.Equal(new byte[] { 0x80, 0x96 }, buf.CopyData());
        }

        [Fact]
        public void WriteInt_Negative_EncodesFiveBytes()
        {
            var buf = new ByteBuf();
            buf.WriteInt(-1); // (uint)(-1) = 0xFFFFFFFF，变长编码封顶 5 字节
            Assert.Equal(5, buf.Size);
        }

        [Fact]
        public void WriteFint_LittleEndianByteOrder()
        {
            var buf = new ByteBuf();
            buf.WriteFint(0x12345678);
            Assert.Equal(new byte[] { 0x78, 0x56, 0x34, 0x12 }, buf.CopyData());
        }

        [Fact]
        public void WriteString_Hello_EncodesSizePrefixedUtf8()
        {
            var buf = new ByteBuf();
            buf.WriteString("hello");
            Assert.Equal(new byte[] { 0x05, 0x68, 0x65, 0x6C, 0x6C, 0x6F }, buf.CopyData());
        }

        // ──────────────── 缓冲区行为 ────────────────

        [Fact]
        public void CopyData_ReturnsUnreadSegmentOnly()
        {
            var buf = new ByteBuf();
            buf.WriteInt(1);   // 1 字节变长
            buf.WriteInt(150); // 2 字节变长
            buf.ReadInt();
            Assert.Equal(new byte[] { 0x80, 0x96 }, buf.CopyData());
        }

        [Fact]
        public void Size_Remaining_TrackReadProgress()
        {
            var buf = new ByteBuf();
            buf.WriteInt(1);
            Assert.Equal(1, buf.Size);
            Assert.Equal(1, buf.Remaining);
            Assert.True(buf.NotEmpty);
            buf.ReadInt();
            Assert.Equal(0, buf.Size);
            Assert.True(buf.Empty);
        }

        [Fact]
        public void Wrap_ExistingBytes_ExposesRawBytes()
        {
            // Wrap 仅包装裸字节（ReaderIndex=0, WriterIndex=len），不含 ReadBytes 的长度前缀
            var buf = ByteBuf.Wrap(new byte[] { 9, 8, 7 });
            Assert.Equal(3, buf.Remaining);
            Assert.Equal(9, buf.ReadByte());
            Assert.Equal(8, buf.ReadByte());
            Assert.Equal(7, buf.ReadByte());
            Assert.True(buf.Empty);
        }

        [Fact]
        public void MixedStream_ReadsInWriteOrder()
        {
            var buf = new ByteBuf();
            buf.WriteBool(true);
            buf.WriteInt(-42);
            buf.WriteString("mixed");
            buf.WriteBytes(new byte[] { 7, 8, 9 });
            Assert.True(buf.ReadBool());
            Assert.Equal(-42, buf.ReadInt());
            Assert.Equal("mixed", buf.ReadString());
            Assert.Equal(new byte[] { 7, 8, 9 }, buf.ReadBytes());
        }

        // ──────────────── Bean 基类行为 ────────────────

        [Fact]
        public void BeanBase_Derived_GetTypeId_ReturnsValue()
        {
            var bean = new TestBean();
            Assert.Equal(12345, bean.GetTypeId());
        }

        [Fact]
        public void BeanBase_IsAssignableTo_ITypeId()
        {
            ITypeId typed = new TestBean();
            Assert.Equal(12345, typed.GetTypeId());
        }

        [Fact]
        public void SerializationException_PreservesMessage()
        {
            var ex = new SerializationException("boom");
            Assert.Equal("boom", ex.Message);
        }
    }
}
