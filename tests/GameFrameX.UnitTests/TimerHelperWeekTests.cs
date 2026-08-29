using System;
using GameFrameX.Runtime;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// TimerHelper 周粒度测试（对照 Unity 基准 TimerHelperWeekTests）。
    /// </summary>
    [Collection("TimerHelperState")]
    public sealed class TimerHelperWeekTests : IDisposable
    {
        private static readonly TimeZoneInfo UtcPlus8 = TimeZoneInfo.CreateCustomTimeZone("Custom/UTC+8", TimeSpan.FromHours(8), "UTC+8", "UTC+8");

        public TimerHelperWeekTests()
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
        public void GetStartTimestampOfWeekWithUtc_MidweekDate_ReturnsMondayMidnight()
        {
            // 2024-01-10 是周三；所在周周一 = 2024-01-08T00:00:00Z = 1704672000
            var date = new DateTime(2024, 1, 10, 12, 30, 0, DateTimeKind.Utc);

            Assert.Equal(1704672000L, TimerHelper.GetStartTimestampOfWeekWithUtc(date));
        }

        [Fact]
        public void GetEndTimestampOfWeekWithUtc_MidweekDate_ReturnsSundayLastSecond()
        {
            // 2024-01-10 是周三；所在周周日 23:59:59Z = 2024-01-14T23:59:59Z = 1705276799
            var date = new DateTime(2024, 1, 10, 12, 30, 0, DateTimeKind.Utc);

            Assert.Equal(1705276799L, TimerHelper.GetEndTimestampOfWeekWithUtc(date));
        }

        [Fact]
        public void GetStartTimestampOfWeekWithUtc_MondayDate_ReturnsSameDay()
        {
            var monday = new DateTime(2024, 1, 8, 15, 0, 0, DateTimeKind.Utc);

            Assert.Equal(1704672000L, TimerHelper.GetStartTimestampOfWeekWithUtc(monday));
        }

        [Fact]
        public void GetStartTimestampOfWeekWithUtc_SundayDate_ReturnsSixDaysEarlier()
        {
            var sunday = new DateTime(2024, 1, 14, 8, 0, 0, DateTimeKind.Utc);

            Assert.Equal(1704672000L, TimerHelper.GetStartTimestampOfWeekWithUtc(sunday));
        }

        [Fact]
        public void GetWeekStartTimestampWithUtc_IsMondayMidnight()
        {
            var ts = TimerHelper.GetWeekStartTimestampWithUtc();
            var monday = TimerHelper.TimestampSecondToDateTime(ts, true);

            Assert.Equal(DayOfWeek.Monday, monday.DayOfWeek);
            Assert.Equal(0, monday.TimeOfDay.TotalSeconds);
        }

        [Fact]
        public void GetWeekEndTimestampWithUtc_IsSundayLastSecond()
        {
            var ts = TimerHelper.GetWeekEndTimestampWithUtc();
            var sunday = TimerHelper.TimestampSecondToDateTime(ts, true);

            Assert.Equal(DayOfWeek.Sunday, sunday.DayOfWeek);
            Assert.Equal(23, sunday.Hour);
            Assert.Equal(59, sunday.Second);
        }

        [Fact]
        public void GetNextWeekStartTimestampWithUtc_IsSevenDaysLater()
        {
            var diff = TimerHelper.GetNextWeekStartTimestampWithUtc() - TimerHelper.GetWeekStartTimestampWithUtc();

            Assert.Equal(7 * 24 * 3600L, diff);
        }

        [Fact]
        public void GetChinaDayOfWeek_MondayIsOne_SundayIsSeven()
        {
            Assert.Equal(1, TimerHelper.GetChinaDayOfWeek(DayOfWeek.Monday));
            Assert.Equal(7, TimerHelper.GetChinaDayOfWeek(DayOfWeek.Sunday));
        }

        [Fact]
        public void GetChinaDayOfWeekWithTimeZone_FixedSundayDate_ReturnsRawDayOfWeek()
        {
            TimerHelper.SetTimeZone(UtcPlus8);
            var sunday = new DateTime(2024, 1, 14, 8, 0, 0, DateTimeKind.Unspecified);

            // 与 Unity 基准逐字一致：返回原始 (int)date.DayOfWeek，周日为 0（非中国习惯 7）
            Assert.Equal(0, TimerHelper.GetChinaDayOfWeekWithTimeZone(sunday));
            Assert.Equal(1, TimerHelper.GetChinaDayOfWeekWithTimeZone(new DateTime(2024, 1, 8, 8, 0, 0, DateTimeKind.Unspecified)));
        }

        [Fact]
        public void GetChinaDayOfWeekWithTimeZone_Now_IsInRangeOneToSeven()
        {
            TimerHelper.SetTimeZone(UtcPlus8);

            Assert.InRange(TimerHelper.GetChinaDayOfWeekWithTimeZone(), 1, 7);
        }

        [Fact]
        public void GetWeekStartTimestampWithTimeZone_ConvertsBackToZoneMondayMidnight()
        {
            TimerHelper.SetTimeZone(UtcPlus8);

            var ts = TimerHelper.GetWeekStartTimestampWithTimeZone();
            var monday = TimerHelper.TimestampSecondToDateTime(ts, true);

            // 语义：墙钟视为 UTC 的时间戳；用 utc:true 解析（false 会再叠加偏移）
            Assert.Equal(DayOfWeek.Monday, monday.DayOfWeek);
            Assert.Equal(0, monday.TimeOfDay.TotalSeconds);
        }

        [Fact]
        public void GetDayOfWeekTimeWithTimeZone_ReturnsMidnightOfRequestedDay()
        {
            TimerHelper.SetTimeZone(UtcPlus8);

            var monday = TimerHelper.GetDayOfWeekTimeWithTimeZone(DayOfWeek.Monday);

            Assert.Equal(DayOfWeek.Monday, monday.DayOfWeek);
            Assert.Equal(0, monday.TimeOfDay.TotalSeconds);
        }
    }
}
