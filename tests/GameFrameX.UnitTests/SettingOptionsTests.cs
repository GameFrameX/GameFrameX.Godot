using GameFrameX.Setting.Runtime;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// SettingOptions 默认值测试。默认辅助器工厂返回 Node 实例，需 Godot 运行时，不在纯 .NET 宿主调用，仅断言可空与引用非空。
    /// </summary>
    public sealed class SettingOptionsTests
    {
        [Fact]
        public void Defaults_MatchUnityBaseline()
        {
            SettingOptions options = new SettingOptions();

            Assert.NotNull(options.SettingHelperFactory);
            Assert.Null(options.SaveSlotFileStorageRoot);
        }
    }
}
