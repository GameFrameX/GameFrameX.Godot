using GameFrameX.Startup.Runtime;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// 启动结果测试（迁移自 Unity com.gameframex.unity.startup Tests/Runtime/StartupResultTests.cs）。
    /// </summary>
    public class StartupResultTests
    {
        [Fact]
        public void Succeed_ReturnsSuccessWithEmptyFields()
        {
            var result = StartupResult.Succeed();

            Assert.True(result.Success);
            Assert.Equal(string.Empty, result.FailedProcedureName);
            Assert.Equal(string.Empty, result.FailedUrl);
            Assert.Equal(string.Empty, result.ErrorMessage);
        }

        [Fact]
        public void Fail_ReturnsFailureWithPopulatedFields()
        {
            var result = StartupResult.Fail("ProcedureX", "http://example.com/api", "boom");

            Assert.False(result.Success);
            Assert.Equal("ProcedureX", result.FailedProcedureName);
            Assert.Equal("http://example.com/api", result.FailedUrl);
            Assert.Equal("boom", result.ErrorMessage);
        }

        [Fact]
        public void Fail_WithNullArgs_TreatsAsEmptyStrings()
        {
            var result = StartupResult.Fail(null, null, null);

            Assert.False(result.Success);
            Assert.Equal(string.Empty, result.FailedProcedureName);
            Assert.Equal(string.Empty, result.FailedUrl);
            Assert.Equal(string.Empty, result.ErrorMessage);
        }

        [Fact]
        public void IsClass()
        {
            Assert.False(typeof(StartupResult).IsValueType, "StartupResult should be a class");
        }
    }
}
