using System;

namespace GameFrameX.EngineTests
{
    /// <summary>
    /// 引擎测试跳过异常：当前运行环境不满足用例前提（如非 TOOLS 构建）时使用，
    /// runner 记为 SKIP，不计失败也不计通过。
    /// </summary>
    public sealed class EngineTestSkippedException : Exception
    {
        public EngineTestSkippedException(string reason) : base(reason)
        {
        }
    }
}
