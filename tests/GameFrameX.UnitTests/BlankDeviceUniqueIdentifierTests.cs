using System.Reflection;
using GameFrameX.SystemInfo.Runtime;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// BlankDeviceUniqueIdentifier 的 Normalize/IsValid 行为测试。
    /// 私有静态方法经反射调用；引擎耦合部分（OS/JavaScriptBridge/ConfigFile）依赖 Godot 运行时，
    /// 无引擎环境不可测，归引擎内验证；剪贴板同理归引擎测试，不强行 mock。
    /// </summary>
    public sealed class BlankDeviceUniqueIdentifierTests
    {
        private static string InvokeNormalize(string sid)
        {
            MethodInfo method = typeof(BlankDeviceUniqueIdentifier).GetMethod("Normalize", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(method);
            return (string)method.Invoke(null, new object[] { sid });
        }

        private static bool InvokeIsValid(string id)
        {
            MethodInfo method = typeof(BlankDeviceUniqueIdentifier).GetMethod("IsValid", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(method);
            return (bool)method.Invoke(null, new object[] { id });
        }

        [Fact]
        public void Normalize_RemovesAllDashes()
        {
            Assert.Equal("0123456789abcdef0123456789abcdef", InvokeNormalize("01234567-89ab-cdef-0123-456789abcdef"));
        }

        [Fact]
        public void Normalize_TruncatesTo32Characters()
        {
            Assert.Equal("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa", InvokeNormalize(new string('a', 64)));
        }

        [Fact]
        public void Normalize_ShortStringUnchanged()
        {
            Assert.Equal("abc123", InvokeNormalize("abc123"));
        }

        [Fact]
        public void Normalize_NullReturnsEmpty()
        {
            Assert.Equal(string.Empty, InvokeNormalize(null));
        }

        [Fact]
        public void Normalize_EmptyReturnsEmpty()
        {
            Assert.Equal(string.Empty, InvokeNormalize(string.Empty));
        }

        [Fact]
        public void IsValid_RejectsNullEmptyLiteralNullAndShort()
        {
            Assert.False(InvokeIsValid(null));
            Assert.False(InvokeIsValid(string.Empty));
            Assert.False(InvokeIsValid("null"));
            Assert.False(InvokeIsValid("abc"));
        }

        [Fact]
        public void IsValid_AcceptsNormalIdentifier()
        {
            Assert.True(InvokeIsValid("abcd"));
            Assert.True(InvokeIsValid("0123456789abcdef0123456789abcdef"));
        }
    }
}
