using System;
using GameFrameX.Network.Runtime;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// 消息序列化器全局注册表测试（P0.2 恢复的注册体系）。
    /// </summary>
    public sealed class MessageSerializerRegistryTests : IDisposable
    {
        private sealed class StubSerializer : IMessageSerializer
        {
            public byte[] Serialize<T>(T message) where T : MessageObject
            {
                return new byte[0];
            }

            public object Deserialize(byte[] data, Type targetType)
            {
                return null;
            }
        }

        public MessageSerializerRegistryTests()
        {
            MessageSerializerRegistry.Reset();
        }

        public void Dispose()
        {
            MessageSerializerRegistry.Reset();
        }

        [Fact]
        public void Global_DefaultsToNull()
        {
            Assert.Null(MessageSerializerRegistry.Global);
        }

        [Fact]
        public void RegisterGlobal_MakesSerializerAvailable()
        {
            var serializer = new StubSerializer();
            MessageSerializerRegistry.RegisterGlobal(serializer);
            Assert.Same(serializer, MessageSerializerRegistry.Global);
        }

        [Fact]
        public void RegisterGlobal_Null_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => MessageSerializerRegistry.RegisterGlobal(null));
        }

        [Fact]
        public void RegisterGlobal_SecondRegistration_ReplacesFirst()
        {
            var first = new StubSerializer();
            var second = new StubSerializer();
            MessageSerializerRegistry.RegisterGlobal(first);
            MessageSerializerRegistry.RegisterGlobal(second);
            Assert.Same(second, MessageSerializerRegistry.Global);
        }

        [Fact]
        public void Reset_ClearsRegistration()
        {
            MessageSerializerRegistry.RegisterGlobal(new StubSerializer());
            MessageSerializerRegistry.Reset();
            Assert.Null(MessageSerializerRegistry.Global);
        }
    }
}
