using System;
using GameFrameX.Runtime;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// TimerHelper 日期（天粒度）测试（对照 Unity 基准 TimerHelperDayTests）。
    /// </summary>
    [Collection("TimerHelperState")]
    public sealed class TimerHelperDayTests : IDisposable
    {
        private static readonly TimeZoneInfo UtcPlus8 = TimeZoneInfo.CreateCustomTimeZone("Custom/UTC+8", TimeSpan.FromHours(8), "UTC+8", "UTC+8");

        public TimerHelperDayTests()
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
        public void GetTodayStartTimeWithUtc_IsUtcMidnight()
        {
            var start = TimerHelper.GetTodayStartTimeWithUtc();

            Assert.Equal(DateTime.UtcNow.Date, start);
            Assert.Equal(0, start.TimeOfDay.TotalSeconds);
        }

        [Fact]
        public void GetTodayStartTimestampWithUtc_MatchesMidnightEpoch()
        {
            var expected = new DateTimeOffset(DateTime.UtcNow.Date).ToUnixTimeSeconds();

            Assert.Equal(expected, TimerHelper.GetTodayStartTimestampWithUtc());
        }

        [Fact]
        public void GetTodayEndTimeWithUtc_IsLastSecondOfDay()
        {
            var end = TimerHelper.GetTodayEndTimeWithUtc();

            Assert.Equal(DateTime.UtcNow.Date.AddDays(1).AddSeconds(-1), end);
        }

        [Fact]
        public void GetTomorrowStartTimestampWithUtc_IsNextMidnight()
        {
            var expected = new DateTimeOffset(DateTime.UtcNow.Date.AddDays(1)).ToUnixTimeSeconds();

            Assert.Equal(expected, TimerHelper.GetTomorrowStartTimestampWithUtc());
        }

        [Fact]
        public void GetCrossDaysUtc_SameDay_ReturnsZero()
        {
            // 2024-06-15T00:00:00Z 与 2024-06-15T23:00:00Z
            Assert.Equal(0, TimerHelper.GetCrossDaysUtc(1718409600L, 1718492400L));
        }

        [Fact]
        public void GetCrossDaysUtc_NextDay_ReturnsOne()
        {
            // 2024-06-15T00:00:00Z -> 2024-06-16T23:00:00Z（1718578800）
            Assert.Equal(1, TimerHelper.GetCrossDaysUtc(1718409600L, 1718578800L));
        }

        [Fact]
        public void GetCrossDaysWithUtc_FromDateTime_SameDayZero()
        {
            var today = DateTime.UtcNow.Date;

            Assert.Equal(0, TimerHelper.GetCrossDaysWithUtc(today));
        }

        [Fact]
        public void GetTodayStartTimeWithTimeZone_ResetsAfterZoneChange()
        {
            TimerHelper.SetTimeZone(UtcPlus8);

            var start = TimerHelper.GetTodayStartTimeWithTimeZone();

            Assert.Equal(0, start.TimeOfDay.TotalSeconds);
        }

        [Fact]
        public void GetTodayStartTimestampWithTimeZone_ConvertsBackToZoneMidnight()
        {
            TimerHelper.SetTimeZone(UtcPlus8);

            var ts = TimerHelper.GetTodayStartTimestampWithTimeZone();
            var midnight = TimerHelper.TimestampSecondToDateTime(ts, true);

            // 语义：把当前时区墙钟时间视为 UTC 的时间戳；用 utc:true 解析（false 会再叠加偏移）
            Assert.Equal(0, midnight.TimeOfDay.TotalSeconds);
            Assert.Equal(TimerHelper.GetNowWithTimeZone().Date, midnight);
        }

        [Fact]
        public void CurrentDateWithDayWithTimeZone_IsEightDigitDate()
        {
            TimerHelper.SetTimeZone(UtcPlus8);

            var value = TimerHelper.CurrentDateWithDayWithTimeZone();

            Assert.InRange(value, 10000101, 99991231);
        }

        [Fact]
        public void GetCrossDaysWithTimeZone_SameInstant_ReturnsZero()
        {
            TimerHelper.SetTimeZone(UtcPlus8);

            Assert.Equal(0, TimerHelper.GetCrossDaysWithTimeZone(1718409600L, 1718409600L));
        }

        [Fact]
        public void GetTodayStartTimestampWithUtc_RoundTripsThroughTimestampSecondToDateTime()
        {
            var ts = TimerHelper.GetTodayStartTimestampWithUtc();

            Assert.Equal(DateTime.UtcNow.Date, TimerHelper.TimestampSecondToDateTime(ts, true));
        }
    }
}
