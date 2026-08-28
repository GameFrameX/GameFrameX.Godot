using System;
using GameFrameX.Runtime;
using GameFrameX.Setting.Runtime;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// SettingStorageBackendFactory 行为测试（对齐 Unity setting 包的存储后端注册语义：
    /// 未注册平台后端时回落到默认 Godot ConfigFile 后端，注册后由工厂提供）。
    /// 默认后端惰性创建 ConfigFile，构造不触碰 Godot 原生运行时，可在纯 .NET 测试宿主下断言类型。
    /// </summary>
    public sealed class SettingStorageBackendFactoryTests : IDisposable
    {
        public void Dispose()
        {
            SettingStorageBackendFactory.ResetBackendForTests();
        }

        [Fact]
        public void Create_ReturnsDefaultGodotConfigFileBackend()
        {
            SettingStorageBackendFactory.ResetBackendForTests();

            ISettingStorageBackend backend = SettingStorageBackendFactory.Create();

            Assert.IsType<GodotConfigFileSettingStorage>(backend);
        }

        [Fact]
        public void RegisterBackend_OverridesCreate()
        {
            FakeSettingStorageBackend backend = new FakeSettingStorageBackend();
            SettingStorageBackendFactory.RegisterBackend(() => backend);

            ISettingStorageBackend created = SettingStorageBackendFactory.Create();

            Assert.Same(backend, created);
        }

        [Fact]
        public void RegisterBackend_WithNull_Throws()
        {
            Assert.Throws<GameFrameworkException>(() => SettingStorageBackendFactory.RegisterBackend(null));
        }

        private sealed class FakeSettingStorageBackend : ISettingStorageBackend
        {
            public bool Load()
            {
                return true;
            }

            public bool Save()
            {
                return true;
            }

            public bool HasKey(string key)
            {
                return false;
            }

            public bool DeleteKey(string key)
            {
                return false;
            }

            public void DeleteAll()
            {
            }

            public int GetInt(string key, int defaultValue)
            {
                return defaultValue;
            }

            public void SetInt(string key, int value)
            {
            }

            public float GetFloat(string key, float defaultValue)
            {
                return defaultValue;
            }

            public void SetFloat(string key, float value)
            {
            }

            public string GetString(string key, string defaultValue)
            {
                return defaultValue;
            }

            public void SetString(string key, string value)
            {
            }
        }
    }
}
