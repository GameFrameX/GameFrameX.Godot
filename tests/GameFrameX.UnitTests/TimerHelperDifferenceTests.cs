using System;
using GameFrameX.Runtime;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// TimerHelper 时间差计算测试（对照 Unity 基准 TimerHelperDifferenceTests）。
    /// </summary>
    [Collection("TimerHelperState")]
    public sealed class TimerHelperDifferenceTests : IDisposable
    {
        private static readonly TimeZoneInfo UtcPlus8 = TimeZoneInfo.CreateCustomTimeZone("Custom/UTC+8", TimeSpan.FromHours(8), "UTC+8", "UTC+8");

        public TimerHelperDifferenceTests()
        {
            TimerHelper.ResetTimeOffset();
            TimerHelper.SetTimeZone(TimeZoneInfo.Utc);
        }

        public void Dispose()
        {
            TimerHelper.ResetTimeOffset();
            TimerHelper.SetTimeZone(TimeZoneInfo.Utc);
        }

        [Fact]
        public void GetElapsedSecondsWithUtc_Epoch_ReturnsCurrentUnixSeconds()
        {
            var elapsed = TimerHelper.GetElapsedSecondsWithUtc(0);

            Assert.InRange(elapsed, TimerHelper.UnixTimeSeconds() - 2, TimerHelper.UnixTimeSeconds());
        }

        [Fact]
        public void GetElapsedMillisecondsWithUtc_Epoch_ReturnsCurrentUnixMilliseconds()
        {
            var elapsed = TimerHelper.GetElapsedMillisecondsWithUtc(0);

            Assert.InRange(elapsed, TimerHelper.UnixTimeMilliseconds() - 2000, TimerHelper.UnixTimeMilliseconds());
        }

        [Fact]
        public void GetTimeDifferenceWithTimeZone_OneHourApart_IsOneHour()
        {
            var start = TimerHelper.UnixTimeSeconds();
            var end = start + 3600;

            Assert.Equal(TimeSpan.FromHours(1), TimerHelper.GetTimeDifferenceWithTimeZone(start, end));
        }

        [Fact]
        public void GetTimeDifferenceMillisecondWithTimeZone_OneMinuteApart_IsOneMinute()
        {
            var start = TimerHelper.UnixTimeMilliseconds();
            var end = start + 60000;

            Assert.Equal(TimeSpan.FromMinutes(1), TimerHelper.GetTimeDifferenceMillisecondWithTimeZone(start, end));
        }

        [Fact]
        public void GetTimeDifferenceMillisecond_UtcMode_IgnoresLocalZone()
        {
            TimerHelper.SetTimeZone(UtcPlus8);
            var start = 1704067200000L; // 2024-01-01T00:00:00Z
            var end = 1704067260000L;   // +60s

            Assert.Equal(TimeSpan.FromSeconds(60), TimerHelper.GetTimeDifferenceMillisecond(start, end, true));
        }

        [Fact]
        public void GetTimeDifferenceFromNowWithTimeZone_FutureTime_IsNegative()
        {
            var future = TimerHelper.GetNowWithTimeZone().AddHours(2);

            var diff = TimerHelper.GetTimeDifferenceFromNowWithTimeZone(future);

            Assert.InRange(diff, TimeSpan.FromHours(-2) - TimeSpan.FromSeconds(2), TimeSpan.FromHours(-2) + TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void GetTimeDifferenceFromNowWithTimeZone_FutureTimestamp_IsNegative()
        {
            var futureTs = TimerHelper.UnixTimeSeconds() + 7200;

            var diff = TimerHelper.GetTimeDifferenceFromNowWithTimeZone(futureTs);

            Assert.InRange(diff, TimeSpan.FromHours(-2) - TimeSpan.FromSeconds(2), TimeSpan.FromHours(-2) + TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void GetTimeDifferenceFromNowMsWithTimeZone_FutureTimestamp_IsNegative()
        {
            var futureTs = TimerHelper.UnixTimeMilliseconds() + 7200000;

            var diff = TimerHelper.GetTimeDifferenceFromNowMsWithTimeZone(futureTs);

            Assert.InRange(diff, TimeSpan.FromHours(-2) - TimeSpan.FromSeconds(2), TimeSpan.FromHours(-2) + TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void GetElapsedSecondsWithTimeZone_UtcEpochTime_MatchesNow()
        {
            var elapsed = TimerHelper.GetElapsedSecondsWithTimeZone(DateTime.UnixEpoch);

            Assert.InRange(elapsed, TimerHelper.UnixTimeSeconds() - 2, TimerHelper.UnixTimeSeconds());
        }
    }
}
