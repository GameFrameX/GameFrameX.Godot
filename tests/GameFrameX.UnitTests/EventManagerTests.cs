using System;
using GameFrameX.Event.Runtime;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// EventManager.CheckUnsubscribe 行为测试（对齐 Unity com.gameframex.unity.event 的安全取消订阅语义：
    /// 已订阅时取消订阅，未订阅时不执行任何操作）。
    /// </summary>
    public sealed class EventManagerTests : IDisposable
    {
        private readonly EventManager _eventManager;

        public EventManagerTests()
        {
            _eventManager = new EventManager();
        }

        public void Dispose()
        {
            _eventManager.Shutdown();
        }

        [Fact]
        public void CheckUnsubscribe_RemovesSubscribedHandler()
        {
            const string eventId = "test_event";
            EventHandler<GameEventArgs> handler = (sender, e) => { };
            _eventManager.Subscribe(eventId, handler);
            Assert.True(_eventManager.Check(eventId, handler));

            _eventManager.CheckUnsubscribe(eventId, handler);
            Assert.False(_eventManager.Check(eventId, handler));
        }

        [Fact]
        public void CheckUnsubscribe_IgnoresAbsentHandlerWithoutThrowing()
        {
            const string eventId = "never_subscribed";
            EventHandler<GameEventArgs> handler = (sender, e) => { };

            _eventManager.CheckUnsubscribe(eventId, handler);
            Assert.False(_eventManager.Check(eventId, handler));
        }

        [Fact]
        public void CheckUnsubscribe_KeepsOtherHandlersOnSameEvent()
        {
            const string eventId = "multi_handler";
            EventHandler<GameEventArgs> first = (sender, e) => { };
            EventHandler<GameEventArgs> second = (sender, e) => { };
            _eventManager.Subscribe(eventId, first);
            _eventManager.Subscribe(eventId, second);

            _eventManager.CheckUnsubscribe(eventId, first);

            Assert.False(_eventManager.Check(eventId, first));
            Assert.True(_eventManager.Check(eventId, second));
        }
    }
}
