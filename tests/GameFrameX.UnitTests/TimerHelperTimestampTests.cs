using System;
using GameFrameX.Runtime;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// TimerHelper 时间戳转换测试（对照 Unity 基准 TimerHelperTimestampTests）。
    /// </summary>
    [Collection("TimerHelperState")]
    public sealed class TimerHelperTimestampTests : IDisposable
    {
        private static readonly TimeZoneInfo UtcPlus8 = TimeZoneInfo.CreateCustomTimeZone("Custom/UTC+8", TimeSpan.FromHours(8), "UTC+8", "UTC+8");

        public TimerHelperTimestampTests()
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
        public void TimestampSecondToDateTime_ZeroUtc_IsEpochUtc()
        {
            Assert.Equal(TimerHelper.EpochUtc, TimerHelper.TimestampSecondToDateTime(0, true));
        }

        [Fact]
        public void TimestampSecondToDateTime_ZeroZone_UtcPlus8IsEightOClock()
        {
            TimerHelper.SetTimeZone(UtcPlus8);

            Assert.Equal(new DateTime(1970, 1, 1, 8, 0, 0), TimerHelper.TimestampSecondToDateTime(0, false));
        }

        [Fact]
        public void TimeStampMillisecondToDateTime_ThousandUtc_IsOneSecondAfterEpoch()
        {
            Assert.Equal(TimerHelper.EpochUtc.AddSeconds(1), TimerHelper.TimeStampMillisecondToDateTime(1000, true));
        }

        [Fact]
        public void TimeStampMillisecondToDateTime_ZeroZone_UtcPlus8IsEightOClock()
        {
            TimerHelper.SetTimeZone(UtcPlus8);

            Assert.Equal(new DateTime(1970, 1, 1, 8, 0, 0), TimerHelper.TimeStampMillisecondToDateTime(0, false));
        }

        [Fact]
        public void TimestampToTicks_Zero_IsEpochUtcTicks()
        {
            // 与 Unity 基准一致：timestamp * TicksPerSecond + EpochUtc.Ticks
            Assert.Equal(621355968000000000L, TimerHelper.TimestampToTicks(0));
        }

        [Fact]
        public void TimestampMillisToTicks_Zero_IsEpochUtcTicks()
        {
            Assert.Equal(621355968000000000L, TimerHelper.TimestampMillisToTicks(0));
        }

        [Fact]
        public void TimeSpanWithTimestampUtc_OneHour_IsOneHour()
        {
            Assert.Equal(TimeSpan.FromHours(1), TimerHelper.TimeSpanWithTimestampUtc(3600));
        }

        [Theory]
        [InlineData(-62135596801L)]
        [InlineData(253402300800L)]
        public void TimeSpanWithTimestampUtc_OutOfRange_Throws(long timestamp)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => TimerHelper.TimeSpanWithTimestampUtc(timestamp));
        }

        [Fact]
        public void TimeSpanWithTimestampUtcMs_Thousand_IsOneSecond()
        {
            Assert.Equal(TimeSpan.FromSeconds(1), TimerHelper.TimeSpanWithTimestampUtcMs(1000));
        }

        [Theory]
        [InlineData(-62135596800001L)]
        [InlineData(253402300800000L)]
        public void TimeSpanWithTimestampUtcMs_OutOfRange_Throws(long timestampMs)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => TimerHelper.TimeSpanWithTimestampUtcMs(timestampMs));
        }

        [Fact]
        public void TimeSpanWithTimestampWithTimeZone_Sixty_IsOneMinute()
        {
            Assert.Equal(TimeSpan.FromMinutes(1), TimerHelper.TimeSpanWithTimestampWithTimeZone(60));
        }

        [Fact]
        public void TimeSpanWithTimestampWithTimeZoneMs_SixtyThousand_IsOneMinute()
        {
            Assert.Equal(TimeSpan.FromMinutes(1), TimerHelper.TimeSpanWithTimestampWithTimeZoneMs(60000));
        }
    }
}
