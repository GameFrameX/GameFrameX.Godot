using System;
using System.Threading;
using System.Threading.Tasks;
using GameFrameX.Config.Runtime;
using GameFrameX.Runtime;
using Godot;
using Hotfix.Config;
using Hotfix.Config.Tables;
using SimpleJSON;

namespace Godot.Hotfix.Config
{
    /// <summary>
    /// Hotfix 配置加载统一入口，供 UI/业务层调用。
    /// </summary>
    public static class ConfigRuntimeDispatcher
    {
        private const string ConfigRootPath = "res://Assets/Bundles/Config";

        private static readonly SemaphoreSlim LoadLock = new(1, 1);
        private static readonly TablesComponent Tables = new();
        private static bool s_IsLoaded;

        public static async Task EnsureLoadedAndLogDemoAsync(string callerTag)
        {
            try
            {
                await EnsureLoadedAsync();

                var configComponent = GameEntry.GetComponent<ConfigComponent>();
                var soundsConfig = configComponent?.GetConfig<TbSoundsConfig>();
                var count = soundsConfig?.Count ?? 0;
                var first = soundsConfig?.FirstOrDefault;
                var firstId = first != null ? first.Id.ToString() : "null";
                GD.Print($"[ConfigDemo:{callerTag}] TbSoundsConfig count={count}, firstId={firstId}");
            }
            catch (Exception exception)
            {
                GD.PushWarning($"[ConfigDemo:{callerTag}] load failed: {exception.Message}");
            }
        }

        public static async Task EnsureLoadedAsync()
        {
            if (s_IsLoaded)
            {
                return;
            }

            await LoadLock.WaitAsync();
            try
            {
                if (s_IsLoaded)
                {
                    return;
                }

                var configComponent = GameEntry.GetComponent<ConfigComponent>();
                if (configComponent == null)
                {
                    throw new InvalidOperationException("ConfigComponent not found.");
                }

                Tables.Init(configComponent);
                await Tables.LoadAsync(LoadTableJsonAsync);
                s_IsLoaded = true;
                GD.Print("[Config] tables loaded via ConfigRuntimeDispatcher.");
            }
            finally
            {
                LoadLock.Release();
            }
        }

        private static Task<JSONNode> LoadTableJsonAsync(string tableName)
        {
            var tablePath = $"{ConfigRootPath}/{tableName}.json";
            if (!FileAccess.FileExists(tablePath))
            {
                throw new InvalidOperationException($"Config file not found: {tablePath}");
            }

            using var fileAccess = FileAccess.Open(tablePath, FileAccess.ModeFlags.Read);
            if (fileAccess == null)
            {
                throw new InvalidOperationException($"Config file open failed: {tablePath}");
            }

            var content = fileAccess.GetAsText();
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new InvalidOperationException($"Config file is empty: {tablePath}");
            }

            return Task.FromResult(JSONNode.Parse(content));
        }
    }
}
