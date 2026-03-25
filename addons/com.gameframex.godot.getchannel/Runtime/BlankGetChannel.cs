using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Godot;

namespace GameFrameX.GetChannel.Runtime
{
    public sealed class BlankGetChannel
    {
        [DllImport("__Internal")]
        private static extern string getChannelName(string channelKey);

        private static readonly Dictionary<string, string> ChannelCache = new Dictionary<string, string>();

        public static string GetChannelName(string channelKey = "channel", string defaultValue = "default")
        {
            if (string.IsNullOrWhiteSpace(channelKey))
            {
                return defaultValue;
            }

            if (ChannelCache.TryGetValue(channelKey, out var value))
            {
                return value;
            }

            string channelName = defaultValue;
            try
            {
                if (OS.HasFeature("ios"))
                {
                    channelName = ReadFromIos(channelKey, defaultValue);
                }
                else
                {
                    channelName = ReadFromStreamingAssets(channelKey, defaultValue);
                }
            }
            catch (Exception exception)
            {
                GD.PushWarning($"GetChannelName failed, key: {channelKey}, error: {exception.Message}");
                channelName = defaultValue;
            }

            if (string.IsNullOrWhiteSpace(channelName))
            {
                channelName = defaultValue;
            }

            ChannelCache[channelKey] = channelName;
            return channelName;
        }

        private static string ReadFromStreamingAssets(string channelKey, string defaultValue)
        {
            string path = ProjectSettings.GlobalizePath("res://streaming_assets/channel.txt");
            if (!File.Exists(path))
            {
                return defaultValue;
            }

            string[] lines = File.ReadAllLines(path);
            foreach (string line in lines)
            {
                string[] split = line.Split(new[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries);
                if (split.Length < 2)
                {
                    continue;
                }

                if (!string.Equals(split[0].Trim(), channelKey, StringComparison.Ordinal))
                {
                    continue;
                }

                return split[1].Trim();
            }

            return defaultValue;
        }

        private static string ReadFromIos(string channelKey, string defaultValue)
        {
            try
            {
                string value = getChannelName(channelKey);
                return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
            }
            catch (Exception exception)
            {
                GD.PushWarning($"Read iOS channel failed, key: {channelKey}, error: {exception.Message}");
                return defaultValue;
            }
        }
    }
}
