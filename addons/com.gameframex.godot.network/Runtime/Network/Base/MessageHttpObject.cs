using GameFrameX.Runtime;
using System.Text.Json.Serialization;

namespace GameFrameX.Network.Runtime
{
    /// <summary>
    /// HTTP消息包装基类
    /// </summary>
    /// <remarks>
    /// Protobuf 契约（字段 1=Id、2=UniqueId、3=Body）由 com.gameframex.godot.google.protobuf 包的
    /// ProtobufSerializerInitializer 在运行时注册到 RuntimeTypeModel，本类不直接依赖 ProtoBuf 注解，
    /// 以保持 network 包对 protobuf 包零依赖。
    /// </remarks>
    public class MessageHttpObject
    {
        /// <summary>
        /// 消息ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// 消息序列号
        /// </summary>
        public int UniqueId { get; set; }

        [JsonIgnore]
        public byte[] Body { get; set; }

        public override string ToString()
        {
            return Utility.Json.ToJson(this);
        }
    }
}
