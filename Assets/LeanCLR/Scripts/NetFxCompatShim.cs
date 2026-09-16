// netfx 兼容 shim：System.Text.Json API 面桥接到 Newtonsoft.Json（特性与序列化器），
// 使 net8 源码零改动编译进 leanclr（netfx 4.7.2）目标。
using System;
using Newtonsoft.Json;

namespace System.Text.Json
{
    public static class JsonSerializer
    {
        public static string Serialize(object value) { return JsonConvert.SerializeObject(value); }
        public static T Deserialize<T>(string json) { return JsonConvert.DeserializeObject<T>(json); }
        public static object Deserialize(string json, Type returnType) { return JsonConvert.DeserializeObject(json, returnType); }
    }
}

namespace System.Text.Json.Serialization
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class JsonIncludeAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class JsonPropertyNameAttribute : Attribute
    {
        public string Name { get; }

        public JsonPropertyNameAttribute(string name)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class JsonIgnoreAttribute : Attribute
    {
    }
}

namespace Godot
{
    /// <summary>
    /// 全局类注册特性 shim（leanclr 下注册为编辑器可挂脚本由运行时扫描处理，此处保证编译面完整）。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class GlobalClassAttribute : Attribute
    {
    }
}

namespace System.Text.Json
{
    /// <summary>
    /// JsonSerializerOptions shim：leanclr/netfx 目标下仅作占位透传（Newtonsoft 无对应概念）。
    /// </summary>
    public sealed class JsonSerializerOptions
    {
        public bool IncludeFields { get; set; }
        public bool WriteIndented { get; set; }
        public System.Text.Json.Serialization.JsonNamingPolicy PropertyNamingPolicy { get; set; }
    }
}

namespace System.Text.Json.Serialization
{
    public abstract class JsonNamingPolicy
    {
        public abstract string ConvertName(string name);

        public static readonly JsonNamingPolicy CamelCase = new CamelCasePolicy();

        private sealed class CamelCasePolicy : JsonNamingPolicy
        {
            public override string ConvertName(string name)
            {
                if (string.IsNullOrEmpty(name) || char.IsLower(name[0]))
                {
                    return name;
                }

                return char.ToLowerInvariant(name[0]) + name.Substring(1);
            }
        }
    }
}
