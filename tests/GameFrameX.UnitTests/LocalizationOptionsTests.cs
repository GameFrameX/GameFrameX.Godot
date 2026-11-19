using GameFrameX.Localization.Runtime;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// LocalizationOptions 默认值测试。默认辅助器工厂返回 Node 实例，需 Godot 运行时，不在纯 .NET 宿主调用，仅断言引用非空。
    /// </summary>
    public sealed class LocalizationOptionsTests
    {
        [Fact]
        public void Defaults_MatchUnityBaseline()
        {
            LocalizationOptions options = new LocalizationOptions();

            Assert.Equal("zh_CN", options.DefaultLanguage);
            Assert.Equal("zh_CN", options.EditorLanguage);
            Assert.False(options.EnableLoadDictionaryUpdateEvent);
            Assert.False(options.IsEnableEditorMode);
            Assert.NotNull(options.LocalizationHelperFactory);
        }
    }
}
