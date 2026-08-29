using System;

namespace GameFrameX.EngineTests
{
    /// <summary>
    /// 轻量断言：失败抛 EngineTestFailureException。
    /// </summary>
    public static class EngineAssert
    {
        public static void Fail(string message)
        {
            throw new EngineTestFailureException(message);
        }

        public static void True(bool condition, string message)
        {
            if (condition == false)
            {
                throw new EngineTestFailureException("期望为 true：" + message);
            }
        }

        public static void False(bool condition, string message)
        {
            if (condition)
            {
                throw new EngineTestFailureException("期望为 false：" + message);
            }
        }

        public static void NotNull(object value, string name)
        {
            if (value == null)
            {
                throw new EngineTestFailureException(name + " 不应为 null");
            }
        }

        public static void IsNull(object value, string name)
        {
            if (value != null)
            {
                throw new EngineTestFailureException(name + " 应为 null，实际：" + value);
            }
        }

        public static void Equal<T>(T expected, T actual, string name)
        {
            if (Equals(expected, actual) == false)
            {
                throw new EngineTestFailureException(name + " 期望 " + expected + "，实际 " + actual);
            }
        }

        public static void IsInstanceOf<T>(object value, string name)
        {
            if (value is T)
            {
                return;
            }

            var actualType = value == null ? "null" : value.GetType().FullName;
            throw new EngineTestFailureException(name + " 期望类型 " + typeof(T).FullName + "，实际 " + actualType);
        }
    }
}
