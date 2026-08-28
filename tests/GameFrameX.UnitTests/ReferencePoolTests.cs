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
            // 不断言全局 Count == 0：其他测试类（Startup*/AssetPatch EventArgs 等）并行执行时会随时
            // 注册新的引用集合，全局计数存在竞态；此处只断言本测试注册的 TestReference 集合确被移除。
            Assert.DoesNotContain(ReferencePool.GetAllReferencePoolInfos(), info => info.Type == typeof(TestReference));
        }
    }
}
