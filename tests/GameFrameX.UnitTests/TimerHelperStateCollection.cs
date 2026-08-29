using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// TimerHelper 全局状态（当前时区 / 时间偏移）相关测试的串行集合，
    /// 避免并行测试互相污染 SetTimeZone / SyncServerTime 的全局静态状态。
    /// </summary>
    [CollectionDefinition("TimerHelperState")]
    public sealed class TimerHelperStateCollection
    {
    }
}
