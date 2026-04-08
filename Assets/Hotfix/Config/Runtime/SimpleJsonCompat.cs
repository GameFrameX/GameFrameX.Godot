using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Nodes;

namespace SimpleJSON
{
    public enum JSONNodeType
    {
        None = 0,
        NullValue = 1,
        String = 2,
        Number = 3,
        Boolean = 4,
        Array = 5,
        Object = 6
    }

    /// <summary>
    /// 最小兼容层：满足 Luban cs-simple-json 生成代码的运行时需求。
    /// </summary>
    public sealed class JSONNode
    {
        private readonly JsonNode _node;
        private readonly JSONNodeType? _overrideType;

        private JSONNode(JsonNode node)
        {
            _node = node;
        }

        private JSONNode(JSONNodeType type)
        {
            _overrideType = type;
        }

        public static JSONNode Parse(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return new JSONNode(JSONNodeType.None);
            }

            var parsed = JsonNode.Parse(json);
            return parsed == null ? new JSONNode(JSONNodeType.NullValue) : new JSONNode(parsed);
        }

        public JSONNode this[string key]
        {
            get
            {
                if (_node is not JsonObject jsonObject || string.IsNullOrEmpty(key))
                {
                    return new JSONNode(JSONNodeType.None);
                }

                return !jsonObject.TryGetPropertyValue(key, out var child)
                    ? new JSONNode(JSONNodeType.None)
                    : Wrap(child);
            }
        }

        public int Count
        {
            get
            {
                if (_node is JsonArray jsonArray)
                {
                    return jsonArray.Count;
                }

                if (_node is JsonObject jsonObject)
                {
                    return jsonObject.Count;
                }

                return 0;
            }
        }

        public IEnumerable<JSONNode> Children
        {
            get
            {
                if (_node is JsonArray jsonArray)
                {
                    return jsonArray.Select(Wrap);
                }

                if (_node is JsonObject jsonObject)
                {
                    return jsonObject.Select(pair => Wrap(pair.Value));
                }

                return Array.Empty<JSONNode>();
            }
        }

        public JSONNodeType Tag
        {
            get
            {
                if (_overrideType.HasValue)
                {
                    return _overrideType.Value;
                }

                if (_node == null)
                {
                    return JSONNodeType.NullValue;
                }

                if (_node is JsonObject)
                {
                    return JSONNodeType.Object;
                }

                if (_node is JsonArray)
                {
                    return JSONNodeType.Array;
                }

                if (_node is JsonValue value)
                {
                    if (value.TryGetValue<bool>(out _))
                    {
                        return JSONNodeType.Boolean;
                    }

                    if (value.TryGetValue<int>(out _) ||
                        value.TryGetValue<long>(out _) ||
                        value.TryGetValue<float>(out _) ||
                        value.TryGetValue<double>(out _) ||
                        value.TryGetValue<decimal>(out _))
                    {
                        return JSONNodeType.Number;
                    }

                    if (value.TryGetValue<string>(out _))
                    {
                        return JSONNodeType.String;
                    }
                }

                return JSONNodeType.NullValue;
            }
        }

        public bool IsNumber => Tag == JSONNodeType.Number;
        public bool IsString => Tag == JSONNodeType.String;
        public bool IsBoolean => Tag == JSONNodeType.Boolean;
        public bool IsArray => Tag == JSONNodeType.Array;
        public bool IsObject => Tag == JSONNodeType.Object;

        public int AsInt => TryGetInt(out var result) ? result : 0;

        public static implicit operator int(JSONNode node) => node?.AsInt ?? 0;
        public static implicit operator long(JSONNode node) => node != null && node.TryGetLong(out var value) ? value : 0L;
        public static implicit operator float(JSONNode node) => node != null && node.TryGetFloat(out var value) ? value : 0f;
        public static implicit operator double(JSONNode node) => node != null && node.TryGetDouble(out var value) ? value : 0d;
        public static implicit operator bool(JSONNode node) => node != null && node.TryGetBool(out var value) && value;
        public static implicit operator string(JSONNode node) => node?.AsString();

        public override string ToString()
        {
            if (_overrideType == JSONNodeType.None)
            {
                return string.Empty;
            }

            return _node?.ToJsonString() ?? "null";
        }

        private string AsString()
        {
            if (_node is not JsonValue value)
            {
                return null;
            }

            if (value.TryGetValue<string>(out var stringValue))
            {
                return stringValue;
            }

            if (value.TryGetValue<bool>(out var boolValue))
            {
                return boolValue ? "true" : "false";
            }

            if (value.TryGetValue<int>(out var intValue))
            {
                return intValue.ToString(CultureInfo.InvariantCulture);
            }

            if (value.TryGetValue<long>(out var longValue))
            {
                return longValue.ToString(CultureInfo.InvariantCulture);
            }

            if (value.TryGetValue<float>(out var floatValue))
            {
                return floatValue.ToString(CultureInfo.InvariantCulture);
            }

            if (value.TryGetValue<double>(out var doubleValue))
            {
                return doubleValue.ToString(CultureInfo.InvariantCulture);
            }

            if (value.TryGetValue<decimal>(out var decimalValue))
            {
                return decimalValue.ToString(CultureInfo.InvariantCulture);
            }

            return value.ToJsonString();
        }

        private bool TryGetInt(out int value)
        {
            if (_node is JsonValue jsonValue)
            {
                if (jsonValue.TryGetValue<int>(out value))
                {
                    return true;
                }

                if (jsonValue.TryGetValue<long>(out var longValue))
                {
                    value = (int)longValue;
                    return true;
                }

                if (jsonValue.TryGetValue<double>(out var doubleValue))
                {
                    value = (int)doubleValue;
                    return true;
                }

                if (jsonValue.TryGetValue<string>(out var stringValue) && int.TryParse(stringValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                {
                    return true;
                }
            }

            value = 0;
            return false;
        }

        private bool TryGetLong(out long value)
        {
            if (_node is JsonValue jsonValue)
            {
                if (jsonValue.TryGetValue<long>(out value))
                {
                    return true;
                }

                if (jsonValue.TryGetValue<int>(out var intValue))
                {
                    value = intValue;
                    return true;
                }

                if (jsonValue.TryGetValue<double>(out var doubleValue))
                {
                    value = (long)doubleValue;
                    return true;
                }

                if (jsonValue.TryGetValue<string>(out var stringValue) && long.TryParse(stringValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                {
                    return true;
                }
            }

            value = 0;
            return false;
        }

        private bool TryGetFloat(out float value)
        {
            if (_node is JsonValue jsonValue)
            {
                if (jsonValue.TryGetValue<float>(out value))
                {
                    return true;
                }

                if (jsonValue.TryGetValue<double>(out var doubleValue))
                {
                    value = (float)doubleValue;
                    return true;
                }

                if (jsonValue.TryGetValue<int>(out var intValue))
                {
                    value = intValue;
                    return true;
                }

                if (jsonValue.TryGetValue<string>(out var stringValue) && float.TryParse(stringValue, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out value))
                {
                    return true;
                }
            }

            value = 0;
            return false;
        }

        private bool TryGetDouble(out double value)
        {
            if (_node is JsonValue jsonValue)
            {
                if (jsonValue.TryGetValue<double>(out value))
                {
                    return true;
                }

                if (jsonValue.TryGetValue<float>(out var floatValue))
                {
                    value = floatValue;
                    return true;
                }

                if (jsonValue.TryGetValue<long>(out var longValue))
                {
                    value = longValue;
                    return true;
                }

                if (jsonValue.TryGetValue<string>(out var stringValue) && double.TryParse(stringValue, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out value))
                {
                    return true;
                }
            }

            value = 0;
            return false;
        }

        private bool TryGetBool(out bool value)
        {
            if (_node is JsonValue jsonValue)
            {
                if (jsonValue.TryGetValue<bool>(out value))
                {
                    return true;
                }

                if (jsonValue.TryGetValue<string>(out var stringValue))
                {
                    if (bool.TryParse(stringValue, out value))
                    {
                        return true;
                    }

                    if (int.TryParse(stringValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intValue))
                    {
                        value = intValue != 0;
                        return true;
                    }
                }

                if (jsonValue.TryGetValue<int>(out var numberValue))
                {
                    value = numberValue != 0;
                    return true;
                }
            }

            value = false;
            return false;
        }

        private static JSONNode Wrap(JsonNode node)
        {
            return node == null ? new JSONNode(JSONNodeType.NullValue) : new JSONNode(node);
        }
    }
}
