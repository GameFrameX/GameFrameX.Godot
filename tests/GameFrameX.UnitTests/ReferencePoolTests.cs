using GameFrameX.Runtime;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// 引用池行为测试。
    /// </summary>
    public sealed class ReferencePoolTests
    {
        private sealed class TestReference : IReference
        {
            public int Value { get; set; }

            public void Clear()
            {
                Value = 0;
            }
        }

        [Fact]
        public void Acquire_ReturnsNonNull_AndReleaseReturnsToPool()
        {
            var reference = ReferencePool.Acquire<TestReference>();
            Assert.NotNull(reference);
            reference.Value = 42;

            ReferencePool.Release(reference);

            var reused = ReferencePool.Acquire<TestReference>();
            Assert.Same(reference, reused);
            Assert.Equal(0, reused.Value); // Release 后 Clear 被调用
        }

        [Fact]
        public void Release_InvalidReference_Throws()
        {
            Assert.Throws<GameFrameworkException>(() => ReferencePool.Release(null));
        }

        [Fact]
        public void ClearAll_RemovesAllCollections()
        {
            ReferencePool.Acquire<TestReference>();
            ReferencePool.ClearAll();
            Assert.Equal(0, ReferencePool.Count);
        }
    }
}
