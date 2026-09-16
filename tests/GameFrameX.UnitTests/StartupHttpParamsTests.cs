using System.Collections.Generic;
using GameFrameX.Runtime;
using GameFrameX.Startup.Runtime;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// 启动HTTP参数测试（迁移自 Unity com.gameframex.unity.startup Tests/Runtime/StartupHttpParamsTests.cs）。
    /// StartupOptions 由 ScriptableObject 改为 Godot Resource，实例化需要 Godot native 运行时，
    /// 无引擎测试宿主会崩溃，因此 FromOptions 拷贝字段与 CustomProvider 两类用例归引擎内验证（项目惯例同 BlankDeviceUniqueIdentifierTests）；
    /// Unity 版中依赖 Application.dataPath 的源文件断言（Source_DoesNotReferenceChannelSdk）不迁移。
    /// </summary>
    public sealed class StartupHttpParamsTests
    {
        public StartupHttpParamsTests()
        {
            // 测试宿主无引擎组件装配，手动注册工程默认 JSON helper（BaseComponent._Ready 中完成同一注册）。
            Utility.Json.SetJsonHelper(new SystemTextJsonHelper());
        }

        [Fact]
        public void FromOptions_NullOptions_ThrowsArgumentNullException()
        {
            Assert.Throws<System.ArgumentNullException>(() => StartupHttpParams.FromOptions(null));
        }

        [Fact]
        public void ToJson_OutputsAllEightFields()
        {
            var parameters = new StartupHttpParams
            {
                Language = "ChineseSimplified",
                UserLanguage = "zh-CN",
                AppVersion = "1.2.3",
                DeviceUniqueIdentifier = "device-1",
                Platform = "Android",
                PackageName = "com.company.game",
                Channel = "official",
                SubChannel = "qa",
            };

            var json = parameters.ToJson();

            Assert.Contains("\"Language\":\"ChineseSimplified\"", json);
            Assert.Contains("\"UserLanguage\":\"zh-CN\"", json);
            Assert.Contains("\"AppVersion\":\"1.2.3\"", json);
            Assert.Contains("\"DeviceUniqueIdentifier\":\"device-1\"", json);
            Assert.Contains("\"Platform\":\"Android\"", json);
            Assert.Contains("\"PackageName\":\"com.company.game\"", json);
            Assert.Contains("\"Channel\":\"official\"", json);
            Assert.Contains("\"SubChannel\":\"qa\"", json);
        }

        [Fact]
        public void StartupHttpParams_ImplementsInterface()
        {
            Assert.IsAssignableFrom<IStartupHttpParams>(new StartupHttpParams());
        }

        [Fact]
        public void StartupHttpParams_CanBeSubclassed()
        {
            var parameters = new CustomStartupHttpParams();

            var dictionary = parameters.ToDictionary();

            Assert.Equal("custom", dictionary["Custom"]);
        }

        private sealed class CustomStartupHttpParams : StartupHttpParams
        {
            public override Dictionary<string, object> ToDictionary()
            {
                var dictionary = base.ToDictionary();
                dictionary["Custom"] = "custom";
                return dictionary;
            }
        }

        private sealed class CustomStartupHttpParamsProvider : IStartupHttpParamsProvider
        {
            public IStartupHttpParams Create(StartupOptions options)
            {
                return new CustomStartupHttpParams();
            }
        }
    }
}
