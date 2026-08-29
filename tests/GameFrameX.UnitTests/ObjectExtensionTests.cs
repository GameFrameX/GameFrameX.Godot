using System;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// 对象扩展测试（对照 Unity 基准 ObjectExtensionTests）。
    /// </summary>
    public sealed class ObjectExtensionTests
    {
        [Fact]
        public void IsNull_NullObject_ReturnsTrue()
        {
            object obj = null;

            Assert.True(obj.IsNull());
        }

        [Fact]
        public void IsNull_NonNullObject_ReturnsFalse()
        {
            object obj = new object();

            Assert.False(obj.IsNull());
        }

        [Fact]
        public void IsNull_BoxedValue_ReturnsFalse()
        {
            object obj = 0;

            Assert.False(obj.IsNull());
        }

        [Fact]
        public void IsNull_EmptyString_ReturnsFalse()
        {
            object obj = string.Empty;

            Assert.False(obj.IsNull());
        }

        [Fact]
        public void IsNotNull_NullObject_ReturnsFalse()
        {
            object obj = null;

            Assert.False(obj.IsNotNull());
        }

        [Fact]
        public void IsNotNull_NonNullObject_ReturnsTrue()
        {
            object obj = new object();

            Assert.True(obj.IsNotNull());
        }

        [Fact]
        public void IsNotNull_BoxedValue_ReturnsTrue()
        {
            object obj = 0;

            Assert.True(obj.IsNotNull());
        }

        [Fact]
        public void CheckNull_NullObject_ThrowsArgumentNullException()
        {
            object obj = null;

            var ex = Assert.Throws<ArgumentNullException>(() =>
            {
                obj.CheckNull("paramName");
            });

            Assert.Equal("paramName", ex.ParamName);
        }

        [Fact]
        public void CheckNull_NonNullObject_DoesNotThrow()
        {
            object obj = new object();

            obj.CheckNull("paramName");
        }

        [Fact]
        public void CheckNull_BoxedInt_DoesNotThrow()
        {
            object obj = 0;

            obj.CheckNull("test");
        }
    }
}
