using System;
using GameFrameX.Runtime;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// TimerHelper 月 / 年粒度测试（对照 Unity 基准 TimerHelperMonthYearTests）。
    /// </summary>
    [Collection("TimerHelperState")]
    public sealed class TimerHelperMonthYearTests : IDisposable
    {
        private static readonly TimeZoneInfo UtcPlus8 = TimeZoneInfo.CreateCustomTimeZone("Custom/UTC+8", TimeSpan.FromHours(8), "UTC+8", "UTC+8");

        public TimerHelperMonthYearTests()
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
        public void GetMonthStartTimeWithUtc_IsFirstDayMidnight()
        {
            var start = TimerHelper.GetMonthStartTimeWithUtc();
            var now = DateTime.UtcNow;

            Assert.Equal(now.Year, start.Year);
            Assert.Equal(now.Month, start.Month);
            Assert.Equal(1, start.Day);
            Assert.Equal(0, start.TimeOfDay.TotalSeconds);
        }

        [Fact]
        public void GetMonthEndTimestampWithUtc_MatchesLastSecondOfDay23()
        {
            var end = TimerHelper.TimestampSecondToDateTime(TimerHelper.GetMonthEndTimestampWithUtc(), true);
            var now = DateTime.UtcNow;
            var lastDay = DateTime.DaysInMonth(now.Year, now.Month);

            Assert.Equal(lastDay, end.Day);
            Assert.Equal(23, end.Hour);
            Assert.Equal(59, end.Second);
        }

        [Fact]
        public void GetNextMonthStartTimeWithUtc_IsOneMonthAhead()
        {
            var next = TimerHelper.GetNextMonthStartTimeWithUtc();
            var now = DateTime.UtcNow;
            var expectedMonth = now.Month == 12 ? 1 : now.Month + 1;
            var expectedYear = now.Month == 12 ? now.Year + 1 : now.Year;

            Assert.Equal(expectedYear, next.Year);
            Assert.Equal(expectedMonth, next.Month);
            Assert.Equal(1, next.Day);
            Assert.Equal(0, next.TimeOfDay.TotalSeconds);
        }

        [Fact]
        public void GetMonthStartTimestampWithTimeZone_ConvertsBackToZoneMonthStart()
        {
            TimerHelper.SetTimeZone(UtcPlus8);

            var ts = TimerHelper.GetMonthStartTimestampWithTimeZone();
            var monthStart = TimerHelper.TimestampSecondToDateTime(ts, true);
            var now = TimerHelper.GetNowWithTimeZone();

            // 语义：墙钟视为 UTC 的时间戳；用 utc:true 解析（false 会再叠加偏移）
            Assert.Equal(now.Year, monthStart.Year);
            Assert.Equal(now.Month, monthStart.Month);
            Assert.Equal(1, monthStart.Day);
            Assert.Equal(0, monthStart.TimeOfDay.TotalSeconds);
        }

        [Fact]
        public void GetNextMonthEndTimeWithTimeZone_IsLastSecondOfNextMonth()
        {
            TimerHelper.SetTimeZone(UtcPlus8);

            var end = TimerHelper.GetNextMonthEndTimeWithTimeZone();
            var now = TimerHelper.GetNowWithTimeZone();
            var expectedMonth = now.Month == 12 ? 1 : now.Month + 1;
            var expectedYear = now.Month == 12 ? now.Year + 1 : now.Year;

            Assert.Equal(expectedYear, end.Year);
            Assert.Equal(expectedMonth, end.Month);
            Assert.Equal(DateTime.DaysInMonth(expectedYear, expectedMonth), end.Day);
            Assert.Equal(23, end.Hour);
        }

        [Fact]
        public void GetYearStartTimeWithUtc_IsJanuaryFirst()
        {
            var start = TimerHelper.GetYearStartTimeWithUtc();
            var now = DateTime.UtcNow;

            Assert.Equal(now.Year, start.Year);
            Assert.Equal(1, start.Month);
            Assert.Equal(1, start.Day);
            Assert.Equal(0, start.TimeOfDay.TotalSeconds);
        }

        [Fact]
        public void GetYearEndTimestampWithUtc_MatchesLastSecondOfYear()
        {
            var end = TimerHelper.TimestampSecondToDateTime(TimerHelper.GetYearEndTimestampWithUtc(), true);
            var now = DateTime.UtcNow;

            Assert.Equal(now.Year, end.Year);
            Assert.Equal(12, end.Month);
            Assert.Equal(31, end.Day);
            Assert.Equal(23, end.Hour);
            Assert.Equal(59, end.Second);
        }

        [Fact]
        public void GetStartTimestampOfYearWithUtc_FixedDate_ReturnsYearStart()
        {
            // 2024-01-01T00:00:00Z = 1704067200
            var date = new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc);

            Assert.Equal(1704067200L, TimerHelper.GetStartTimestampOfYearWithUtc(date));
        }

        [Fact]
        public void GetEndTimestampOfYearWithUtc_FixedDate_ReturnsYearEnd()
        {
            // 2024-12-31T23:59:59Z = 1735689599
            var date = new DateTime(2024, 6, 15, 12, 0, 0, DateTimeKind.Utc);

            Assert.Equal(1735689599L, TimerHelper.GetEndTimestampOfYearWithUtc(date));
        }

        [Fact]
        public void GetNextYearStartTimestamp_IsNextJanuaryFirst()
        {
            var ts = TimerHelper.GetNextYearStartTimestamp();
            var next = TimerHelper.TimestampSecondToDateTime(ts, true);

            Assert.Equal(DateTime.UtcNow.Year + 1, next.Year);
            Assert.Equal(1, next.Month);
            Assert.Equal(1, next.Day);
        }

        [Fact]
        public void GetNextYearStartTimestampWithTimeZone_ConvertsBackToZoneNextYearStart()
        {
            TimerHelper.SetTimeZone(UtcPlus8);

            var ts = TimerHelper.GetNextYearStartTimestampWithTimeZone();
            var nextYearStart = TimerHelper.TimestampSecondToDateTime(ts, true);
            var now = TimerHelper.GetNowWithTimeZone();

            // 语义：墙钟视为 UTC 的时间戳；用 utc:true 解析（false 会再叠加偏移）
            Assert.Equal(now.Year + 1, nextYearStart.Year);
            Assert.Equal(1, nextYearStart.Month);
            Assert.Equal(1, nextYearStart.Day);
            Assert.Equal(0, nextYearStart.TimeOfDay.TotalSeconds);
        }
    }
}
