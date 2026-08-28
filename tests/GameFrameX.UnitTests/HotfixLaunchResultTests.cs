using GameFrameX.Startup.Runtime;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// 热更加载结果测试（迁移自 Unity com.gameframex.unity.startup Tests/Runtime/HotfixLaunchResultTests.cs）。
    /// </summary>
    public class HotfixLaunchResultTests
    {
        [Fact]
        public void Succeed_ReturnsSuccessWithEmptyError()
        {
            var result = HotfixLaunchResult.Succeed();

            Assert.True(result.Success);
            Assert.Equal(string.Empty, result.ErrorMessage);
        }

        [Fact]
        public void Fail_ReturnsFailureWithErrorMessage()
        {
            var result = HotfixLaunchResult.Fail("DLL not found");

            Assert.False(result.Success);
            Assert.Equal("DLL not found", result.ErrorMessage);
        }

        [Fact]
        public void Fail_WithNullError_TreatsAsEmptyString()
        {
            var result = HotfixLaunchResult.Fail(null);

            Assert.False(result.Success);
            Assert.Equal(string.Empty, result.ErrorMessage);
        }

        [Fact]
        public void IsClass()
        {
            Assert.False(typeof(HotfixLaunchResult).IsValueType, "HotfixLaunchResult should be a class");
        }
    }
}
