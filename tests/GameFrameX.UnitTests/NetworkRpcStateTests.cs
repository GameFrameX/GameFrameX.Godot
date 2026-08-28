using System;
using GameFrameX.Network.Runtime;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// RPC 状态测试（P0.3 线程安全对齐引入的 Reset/构造校验行为）。
    /// </summary>
    public sealed class NetworkRpcStateTests
    {
        private sealed class TestRequestMessage : MessageObject, IRequestMessage
        {
        }

        [Fact]
        public void Constructor_TimeoutBelowMinimum_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new NetworkManager.RpcState(2999));
        }

        [Fact]
        public void Constructor_MinimumTimeout_Succeeds()
        {
            using (var rpcState = new NetworkManager.RpcState(3000))
            {
                Assert.NotNull(rpcState);
            }
        }

        [Fact]
        public void Dispose_IsIdempotent()
        {
            var rpcState = new NetworkManager.RpcState(3000);
            rpcState.Dispose();
            rpcState.Dispose();
        }

        [Fact]
        public void Call_SameUniqueId_ReturnsSameTask()
        {
            var rpcState = new NetworkManager.RpcState(3000);
            var message = new TestRequestMessage();
            var first = rpcState.Call(message, true);
            var second = rpcState.Call(message, true);
            Assert.Same(first, second);
            rpcState.Dispose();
        }

        [Fact]
        public void Reset_AllowsReuseAfterDispose()
        {
            var rpcState = new NetworkManager.RpcState(3000);
            rpcState.Call(new TestRequestMessage(), true);
            rpcState.Dispose();
            rpcState.Reset();

            var task = rpcState.Call(new TestRequestMessage(), true);
            Assert.NotNull(task);
            rpcState.Dispose();
        }
    }
}
