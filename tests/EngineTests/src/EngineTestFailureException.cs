using System;

namespace GameFrameX.EngineTests
{
    /// <summary>
    /// 引擎测试断言失败异常：runner 捕获后标记用例 FAIL。
    /// </summary>
    public sealed class EngineTestFailureException : Exception
    {
        public EngineTestFailureException(string message) : base(message)
        {
        }
    }
}
