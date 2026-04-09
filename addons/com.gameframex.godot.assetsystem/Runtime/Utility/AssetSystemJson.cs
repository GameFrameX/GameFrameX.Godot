using System.Text.Json;

namespace GameFrameX.AssetSystem
{
    internal static class AssetSystemJson
    {
        private static JsonSerializerOptions CreateOptions(bool prettyPrint)
        {
            return new JsonSerializerOptions
            {
                IncludeFields = true,
                WriteIndented = prettyPrint
            };
        }

        public static string ToJson<T>(T value, bool prettyPrint = false)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return JsonSerializer.Serialize(value, CreateOptions(prettyPrint));
        }

        public static T FromJson<T>(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(json, CreateOptions(false));
        }
    }
}
