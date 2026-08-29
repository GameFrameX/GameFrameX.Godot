using GameFrameX.Event.Runtime;
using GameFrameX.Runtime;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// OnApplicationPauseChangedEventArgs 契约测试。
    /// 迁移自 Unity com.gameframex.unity.mono；Godot 侧转发点为 BaseComponent。全程无引擎调用。
    /// </summary>
    [Collection("ReferencePool")]
    public class OnApplicationPauseChangedEventArgsTests
    {
        [Fact]
        public void EventId_IsTypeFullName()
        {
            Assert.Equal("GameFrameX.Runtime.OnApplicationPauseChangedEventArgs", OnApplicationPauseChangedEventArgs.EventId);
        }

        [Fact]
        public void Id_ReturnsEventId()
        {
            OnApplicationPauseChangedEventArgs eventArgs = OnApplicationPauseChangedEventArgs.Create(true);
            Assert.Equal(OnApplicationPauseChangedEventArgs.EventId, eventArgs.Id);
        }

        [Fact]
        public void Create_FillsIsPause()
        {
            OnApplicationPauseChangedEventArgs paused = OnApplicationPauseChangedEventArgs.Create(true);
            Assert.True(paused.IsPause);

            OnApplicationPauseChangedEventArgs resumed = OnApplicationPauseChangedEventArgs.Create(false);
            Assert.False(resumed.IsPause);
        }

        [Fact]
        public void Clear_ResetsIsPause()
        {
            OnApplicationPauseChangedEventArgs eventArgs = OnApplicationPauseChangedEventArgs.Create(true);
            ReferencePool.Release(eventArgs);

            OnApplicationPauseChangedEventArgs reused = ReferencePool.Acquire<OnApplicationPauseChangedEventArgs>();
            Assert.Same(eventArgs, reused);
            Assert.False(reused.IsPause);
        }

        [Fact]
        public void TypeHierarchy_IsGameEventArgsAndIReference()
        {
            Assert.True(typeof(GameEventArgs).IsAssignableFrom(typeof(OnApplicationPauseChangedEventArgs)));
            Assert.True(typeof(IReference).IsAssignableFrom(typeof(OnApplicationPauseChangedEventArgs)));
        }
    }
}
