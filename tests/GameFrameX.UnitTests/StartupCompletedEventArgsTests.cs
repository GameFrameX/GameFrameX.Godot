using GameFrameX.Event.Runtime;
using GameFrameX.Runtime;
using GameFrameX.Startup.Runtime;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// 启动完成事件参数测试（迁移自 Unity com.gameframex.unity.startup Tests/Runtime/StartupCompletedEventArgsTests.cs）。
    /// EventId 断言改为 Godot 侧实际命名空间（GameFrameX.Startup.Runtime）。
    /// </summary>
    public class StartupCompletedEventArgsTests
    {
        [Fact]
        public void EventId_IsStringFullName()
        {
            Assert.Equal(
                "GameFrameX.Startup.Runtime.StartupCompletedEventArgs",
                StartupCompletedEventArgs.EventId);
        }

        [Fact]
        public void Id_PropertyReturnsEventId()
        {
            var args = StartupCompletedEventArgs.Create();
            Assert.Equal(StartupCompletedEventArgs.EventId, args.Id);
        }

        [Fact]
        public void Acquire_ReturnsNonNullInstance()
        {
            var args = StartupCompletedEventArgs.Create();
            Assert.NotNull(args);
            Assert.IsType<StartupCompletedEventArgs>(args);
        }

        [Fact]
        public void InheritsGameEventArgs()
        {
            Assert.True(typeof(GameEventArgs).IsAssignableFrom(typeof(StartupCompletedEventArgs)));
        }

        [Fact]
        public void ImplementsIReference()
        {
            Assert.True(typeof(IReference).IsAssignableFrom(typeof(StartupCompletedEventArgs)));
        }

        [Fact]
        public void Clear_IsEmpty_NoOp()
        {
            var args = StartupCompletedEventArgs.Create();
            var exception = Record.Exception(() => args.Clear());
            Assert.Null(exception);
        }

        [Fact]
        public void Acquire_Release_Acquire_PoolsInstance()
        {
            var args1 = StartupCompletedEventArgs.Create();
            ReferencePool.Release(args1);
            var args2 = StartupCompletedEventArgs.Create();

            // 引用池允许返回同一实例（具体取决于池容量），这里只验证不抛异常
            Assert.NotNull(args2);
        }
    }
}
