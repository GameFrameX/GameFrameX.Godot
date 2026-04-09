using System;
using System.Collections;
using System.Globalization;
using System.Linq;

namespace LuBan.Runtime
{
    /// <summary>
    /// Luban JSON 代码生成产物在运行时的最小基类占位。
    /// </summary>
    public abstract class BeanBase
    {
        public abstract int GetTypeId();
    }

    /// <summary>
    /// Luban 反序列化异常类型。
    /// </summary>
    public sealed class SerializationException : Exception
    {
        public SerializationException()
        {
        }

        public SerializationException(string message) : base(message)
        {
        }

        public SerializationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    public static class StringUtil
    {
        public static string CollectionToString(IEnumerable collection)
        {
            if (collection == null)
            {
                return "[]";
            }

            var items = collection.Cast<object>()
                .Select(FormatValue);
            return "[" + string.Join(",", items) + "]";
        }

        private static string FormatValue(object value)
        {
            if (value == null)
            {
                return "null";
            }

            return value switch
            {
                string s => "\"" + s + "\"",
                IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
                _ => value.ToString()
            };
        }
    }
}
