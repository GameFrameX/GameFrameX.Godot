using GameFrameX.Network.Runtime;
using ProtoBuf;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// protobuf 消息序列化器回环测试（P0.2 恢复 protobuf↔network 粘合链路）。
    /// </summary>
    public sealed class ProtobufMessageSerializerTests
    {
        [ProtoContract]
        private sealed class ProtoRoundTripMessage : MessageObject
        {
            [ProtoMember(1)]
            public string Text { get; set; }

            [ProtoMember(2)]
            public int Number { get; set; }
        }

        [Fact]
        public void SerializeDeserialize_RoundTripsMessageFields()
        {
            var serializer = new ProtobufMessageSerializer();
            var message = new ProtoRoundTripMessage { Text = "GameFrameX", Number = 42 };

            byte[] data = serializer.Serialize(message);
            Assert.NotNull(data);
            Assert.NotEmpty(data);

            var restored = (ProtoRoundTripMessage)serializer.Deserialize(data, typeof(ProtoRoundTripMessage));
            Assert.NotNull(restored);
            Assert.Equal("GameFrameX", restored.Text);
            Assert.Equal(42, restored.Number);
        }

        [Fact]
        public void Initializer_Register_InstallsProtobufAsGlobalSerializer()
        {
            try
            {
                ProtobufSerializerInitializer.Register();
                Assert.IsType<ProtobufMessageSerializer>(MessageSerializerRegistry.Global);
            }
            finally
            {
                MessageSerializerRegistry.Reset();
            }
        }
    }
}
