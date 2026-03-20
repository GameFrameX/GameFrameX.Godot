using System;
using System.Text;
using GameFrameX.Runtime;

namespace GameFrameX.Network.Runtime
{
    public static class SerializerHelper
    {
        public static byte[] Serialize<T>(T value)
        {
            var json = Utility.Json.ToJson(value);
            return Encoding.UTF8.GetBytes(json);
        }

        public static object Deserialize(byte[] data, Type type)
        {
            var json = Encoding.UTF8.GetString(data);
            return Utility.Json.ToObject(type, json);
        }
    }
}
