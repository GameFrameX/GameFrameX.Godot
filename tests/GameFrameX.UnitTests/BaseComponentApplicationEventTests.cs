using System;
using GameFrameX.Event.Runtime;
using GameFrameX.Runtime;
using Godot;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// BaseComponent 焦点/暂停通知 → 全局事件转发测试。
    /// 说明：_Notification 依赖 Godot 原生通知（xunit 宿主禁用引擎调用），此处直接驱动抽出的内部入口
    /// ResolveApplicationEventArgs / FireApplicationEvent；通知常量为编译期常量，读取无引擎调用。
    /// </summary>
    [Collection("ReferencePool")]
    public sealed class BaseComponentApplicationEventTests : IDisposable
    {
        private readonly EventManager m_EventManager;

        public BaseComponentApplicationEventTests()
        {
            m_EventManager = new EventManager();
        }

        public void Dispose()
        {
            m_EventManager.Shutdown();
        }

        [Fact]
        public void Resolve_FocusIn_ReturnsFocusGained()
        {
            GameEventArgs eventArgs = BaseComponent.ResolveApplicationEventArgs((int)Node.NotificationApplicationFocusIn);
            OnApplicationFocusChangedEventArgs focusEventArgs = Assert.IsType<OnApplicationFocusChangedEventArgs>(eventArgs);
            Assert.True(focusEventArgs.IsFocus);
        }

        [Fact]
        public void Resolve_FocusOut_ReturnsFocusLost()
        {
            GameEventArgs eventArgs = BaseComponent.ResolveApplicationEventArgs((int)Node.NotificationApplicationFocusOut);
            OnApplicationFocusChangedEventArgs focusEventArgs = Assert.IsType<OnApplicationFocusChangedEventArgs>(eventArgs);
            Assert.False(focusEventArgs.IsFocus);
        }

        [Fact]
        public void Resolve_Paused_ReturnsPausedTrue()
        {
            GameEventArgs eventArgs = BaseComponent.ResolveApplicationEventArgs((int)Node.NotificationApplicationPaused);
            OnApplicationPauseChangedEventArgs pauseEventArgs = Assert.IsType<OnApplicationPauseChangedEventArgs>(eventArgs);
            Assert.True(pauseEventArgs.IsPause);
        }

        [Fact]
        public void Resolve_Resumed_ReturnsPausedFalse()
        {
            GameEventArgs eventArgs = BaseComponent.ResolveApplicationEventArgs((int)Node.NotificationApplicationResumed);
            OnApplicationPauseChangedEventArgs pauseEventArgs = Assert.IsType<OnApplicationPauseChangedEventArgs>(eventArgs);
            Assert.False(pauseEventArgs.IsPause);
        }

        [Fact]
        public void Resolve_UnrelatedNotification_ReturnsNull()
        {
            Assert.Null(BaseComponent.ResolveApplicationEventArgs((int)Node.NotificationWMCloseRequest));
        }

        [Fact]
        public void Resolve_AndFireNow_DeliversEventToSubscriber()
        {
            object receivedSender = null;
            GameEventArgs receivedArgs = null;
            m_EventManager.Subscribe(OnApplicationFocusChangedEventArgs.EventId, (sender, e) =>
            {
                receivedSender = sender;
                receivedArgs = e;
            });

            object sender = new object();
            m_EventManager.FireNow(sender, BaseComponent.ResolveApplicationEventArgs((int)Node.NotificationApplicationFocusOut));

            Assert.Same(sender, receivedSender);
            OnApplicationFocusChangedEventArgs focusEventArgs = Assert.IsType<OnApplicationFocusChangedEventArgs>(receivedArgs);
            Assert.False(focusEventArgs.IsFocus);
        }

        [Fact]
        public void FireApplicationEvent_WithoutRegisteredEventComponent_DoesNotThrow()
        {
            Assert.Null(Record.Exception(() =>
            {
                BaseComponent.FireApplicationEvent(new object(), OnApplicationFocusChangedEventArgs.Create(true));
            }));
        }
    }
}
