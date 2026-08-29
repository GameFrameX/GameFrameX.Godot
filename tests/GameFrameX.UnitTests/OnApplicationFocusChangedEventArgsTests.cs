using GameFrameX.Event.Runtime;
using GameFrameX.Runtime;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// OnApplicationFocusChangedEventArgs 契约测试。
    /// 迁移自 Unity com.gameframex.unity.mono；Godot 侧转发点为 BaseComponent。全程无引擎调用。
    /// </summary>
    [Collection("ReferencePool")]
    public class OnApplicationFocusChangedEventArgsTests
    {
        [Fact]
        public void EventId_IsTypeFullName()
        {
            Assert.Equal("GameFrameX.Runtime.OnApplicationFocusChangedEventArgs", OnApplicationFocusChangedEventArgs.EventId);
        }

        [Fact]
        public void Id_ReturnsEventId()
        {
            OnApplicationFocusChangedEventArgs eventArgs = OnApplicationFocusChangedEventArgs.Create(true);
            Assert.Equal(OnApplicationFocusChangedEventArgs.EventId, eventArgs.Id);
        }

        [Fact]
        public void Create_FillsIsFocus()
        {
            OnApplicationFocusChangedEventArgs gained = OnApplicationFocusChangedEventArgs.Create(true);
            Assert.True(gained.IsFocus);

            OnApplicationFocusChangedEventArgs lost = OnApplicationFocusChangedEventArgs.Create(false);
            Assert.False(lost.IsFocus);
        }

        [Fact]
        public void Clear_ResetsIsFocus()
        {
            OnApplicationFocusChangedEventArgs eventArgs = OnApplicationFocusChangedEventArgs.Create(true);
            ReferencePool.Release(eventArgs);

            OnApplicationFocusChangedEventArgs reused = ReferencePool.Acquire<OnApplicationFocusChangedEventArgs>();
            Assert.Same(eventArgs, reused);
            Assert.False(reused.IsFocus);
        }

        [Fact]
        public void TypeHierarchy_IsGameEventArgsAndIReference()
        {
            Assert.True(typeof(GameEventArgs).IsAssignableFrom(typeof(OnApplicationFocusChangedEventArgs)));
            Assert.True(typeof(IReference).IsAssignableFrom(typeof(OnApplicationFocusChangedEventArgs)));
        }
    }
}
