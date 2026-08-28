using GameFrameX.Event.Runtime;
using GameFrameX.Runtime;
using GameFrameX.Startup.Runtime;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// 启动失败事件参数测试（迁移自 Unity com.gameframex.unity.startup Tests/Runtime/StartupFailedEventArgsTests.cs）。
    /// EventId 断言改为 Godot 侧实际命名空间（GameFrameX.Startup.Runtime）。
    /// </summary>
    public class StartupFailedEventArgsTests
    {
        [Fact]
        public void EventId_IsStringFullName()
        {
            Assert.Equal(
                "GameFrameX.Startup.Runtime.StartupFailedEventArgs",
                StartupFailedEventArgs.EventId);
        }

        [Fact]
        public void Id_PropertyReturnsEventId()
        {
            var args = StartupFailedEventArgs.Create("ProcedureX", "http://example.com", "boom");
            Assert.Equal(StartupFailedEventArgs.EventId, args.Id);
        }

        [Fact]
        public void Create_PopulatesFields()
        {
            var args = StartupFailedEventArgs.Create("ProcedureLauncher", "http://x/api", "timeout");

            Assert.Equal("ProcedureLauncher", args.FailedProcedureName);
            Assert.Equal("http://x/api", args.FailedUrl);
            Assert.Equal("timeout", args.ErrorMessage);
        }

        [Fact]
        public void Create_WithNullArgs_TreatsAsEmptyStrings()
        {
            var args = StartupFailedEventArgs.Create(null, null, null);

            Assert.Equal(string.Empty, args.FailedProcedureName);
            Assert.Equal(string.Empty, args.FailedUrl);
            Assert.Equal(string.Empty, args.ErrorMessage);
        }

        [Fact]
        public void Clear_ResetsAllFields()
        {
            var args = StartupFailedEventArgs.Create("ProcedureX", "http://x/api", "boom");

            args.Clear();

            Assert.Equal(string.Empty, args.FailedProcedureName);
            Assert.Equal(string.Empty, args.FailedUrl);
            Assert.Equal(string.Empty, args.ErrorMessage);
        }

        [Fact]
        public void InheritsGameEventArgs()
        {
            Assert.True(typeof(GameEventArgs).IsAssignableFrom(typeof(StartupFailedEventArgs)));
        }

        [Fact]
        public void ImplementsIReference()
        {
            Assert.True(typeof(IReference).IsAssignableFrom(typeof(StartupFailedEventArgs)));
        }
    }
}
