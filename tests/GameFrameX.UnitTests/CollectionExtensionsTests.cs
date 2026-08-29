using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// 集合扩展测试（对照 Unity 基准 CollectionExtensionsTests）。
    /// </summary>
    public sealed class CollectionExtensionsTests
    {
        [Fact]
        public void Merge_ExistingKey_ReplaceStrategy()
        {
            var dict = new Dictionary<string, string> { { "key", "old" } };

            dict.Merge("key", "new", (existing, incoming) => incoming);

            Assert.Equal("new", dict["key"]);
        }

        [Fact]
        public void GetOrAdd_ExistingKey_ReturnsExistingValue()
        {
            var dict = new Dictionary<string, int> { { "key", 42 } };

            var result = dict.GetOrAdd("key", k => 100);

            Assert.Equal(42, result);
        }

        [Fact]
        public void GetOrAdd_MissingKey_CreatesAndReturnsValue()
        {
            var dict = new Dictionary<string, int>();

            var result = dict.GetOrAdd("key", k => 100);

            Assert.Equal(100, result);
            Assert.Equal(100, dict["key"]);
        }

        [Fact]
        public void GetOrAdd_FactoryReceivesKey()
        {
            var dict = new Dictionary<string, string>();

            var result = dict.GetOrAdd("myKey", k => "value_" + k);

            Assert.Equal("value_myKey", result);
        }

        [Fact]
        public void GetOrAdd_DefaultCtor_ExistingKey_ReturnsExistingValue()
        {
            var dict = new Dictionary<string, List<int>> { { "key", new List<int> { 1, 2 } } };

            var result = dict.GetOrAdd("key");

            Assert.Equal(2, result.Count);
        }

        [Fact]
        public void GetOrAdd_DefaultCtor_MissingKey_CreatesNewInstance()
        {
            var dict = new Dictionary<string, List<int>>();

            var result = dict.GetOrAdd("key");

            Assert.NotNull(result);
            Assert.Equal(1, dict.Count);
        }

        [Fact]
        public void RemoveIf_MatchingCondition_RemovesAndReturnsCount()
        {
            var dict = new Dictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } };

            var removed = dict.RemoveIf((k, v) => v > 1);

            Assert.Equal(2, removed);
            Assert.Equal(1, dict.Count);
            Assert.True(dict.ContainsKey("a"));
        }

        [Fact]
        public void RemoveIf_NoMatch_ReturnsZero()
        {
            var dict = new Dictionary<string, int> { { "a", 1 }, { "b", 2 } };

            var removed = dict.RemoveIf((k, v) => v > 10);

            Assert.Equal(0, removed);
            Assert.Equal(2, dict.Count);
        }

        [Fact]
        public void RemoveIf_AllMatch_ClearsDictionary()
        {
            var dict = new Dictionary<string, int> { { "a", 1 }, { "b", 2 } };

            var removed = dict.RemoveIf((k, v) => true);

            Assert.Equal(2, removed);
            Assert.Equal(0, dict.Count);
        }

        [Fact]
        public void RemoveIf_EmptyDictionary_ReturnsZero()
        {
            var dict = new Dictionary<string, int>();

            var removed = dict.RemoveIf((k, v) => true);

            Assert.Equal(0, removed);
        }

        [Fact]
        public void IsNullOrEmpty_NullCollection_ReturnsTrue()
        {
            List<int> list = null;

            Assert.True(list.IsNullOrEmpty());
        }

        [Fact]
        public void IsNullOrEmpty_EmptyCollection_ReturnsTrue()
        {
            Assert.True(new List<int>().IsNullOrEmpty());
        }

        [Fact]
        public void IsNullOrEmpty_NonEmptyCollection_ReturnsFalse()
        {
            Assert.False(new List<int> { 1 }.IsNullOrEmpty());
        }

        [Fact]
        public void IsNullOrEmpty_NullArray_ReturnsTrue()
        {
            int[] arr = null;

            Assert.True(arr.IsNullOrEmpty());
        }

        [Fact]
        public void IsNullOrEmpty_EmptyArray_ReturnsTrue()
        {
            Assert.True(new int[0].IsNullOrEmpty());
        }

        [Fact]
        public void Shuffer_PreservesElements()
        {
            var list = Enumerable.Range(1, 100).ToList();

            list.Shuffer();

            Assert.Equal(100, list.Count);
            Assert.Equal(Enumerable.Range(1, 100).Sum(), list.Sum());
            Assert.All(list, x => Assert.InRange(x, 1, 100));
        }

        [Fact]
        public void Shuffer_SingleElement_NoChange()
        {
            var list = new List<int> { 42 };

            list.Shuffer();

            Assert.Equal(1, list.Count);
            Assert.Equal(42, list[0]);
        }

        [Fact]
        public void Shuffer_EmptyList_DoesNotThrow()
        {
            var list = new List<int>();

            list.Shuffer();

            Assert.Empty(list);
        }

        [Fact]
        public void ListRemoveIf_MatchingElements_RemovesThem()
        {
            var list = new List<int> { 1, 2, 3, 4, 5 };

            list.RemoveIf(x => x % 2 == 0);

            Assert.Equal(new[] { 1, 3, 5 }, list);
        }

        [Fact]
        public void ListRemoveIf_NoMatch_DoesNotRemove()
        {
            var list = new List<int> { 1, 3, 5 };

            list.RemoveIf(x => x % 2 == 0);

            Assert.Equal(new[] { 1, 3, 5 }, list);
        }

        [Fact]
        public void ListToString_JoinsWithSeparator()
        {
            var list = new List<int> { 1, 2, 3 };

            Assert.Equal("1,2,3", list.ListToString());
            Assert.Equal("1-2-3", list.ListToString("-"));
        }

        [Fact]
        public void AddRange_AddsAllToHashSet()
        {
            var set = new HashSet<int> { 1 };

            set.AddRange(new[] { 2, 3, 3 });

            Assert.Equal(new[] { 1, 2, 3 }, set.OrderBy(x => x));
        }
    }
}
