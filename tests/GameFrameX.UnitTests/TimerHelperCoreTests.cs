using System;
using GameFrameX.Runtime;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// TimerHelper 时区核心机制测试（对照 Unity 基准 TimerHelperCoreTests；
    /// 覆盖 Godot 侧新增的 SetTimeZone / WithTimeZoneOffset / Sync 系列）。
    /// </summary>
    [Collection("TimerHelperState")]
    public sealed class TimerHelperCoreTests : IDisposable
    {
        private static readonly TimeZoneInfo UtcPlus8 = TimeZoneInfo.CreateCustomTimeZone("Custom/UTC+8", TimeSpan.FromHours(8), "UTC+8", "UTC+8");

        public TimerHelperCoreTests()
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
        public void EpochLocal_Is19700101Local()
        {
            Assert.Equal(1970, TimerHelper.EpochLocal.Year);
            Assert.Equal(1, TimerHelper.EpochLocal.Month);
            Assert.Equal(1, TimerHelper.EpochLocal.Day);
            Assert.Equal(DateTimeKind.Local, TimerHelper.EpochLocal.Kind);
        }

        [Fact]
        public void EpochUtc_Is19700101Utc()
        {
            Assert.Equal(1970, TimerHelper.EpochUtc.Year);
            Assert.Equal(1, TimerHelper.EpochUtc.Month);
            Assert.Equal(1, TimerHelper.EpochUtc.Day);
            Assert.Equal(DateTimeKind.Utc, TimerHelper.EpochUtc.Kind);
        }

        [Fact]
        public void SetTimeZone_DefaultIsUtc()
        {
            Assert.Equal(TimeZoneInfo.Utc, TimerHelper.CurrentTimeZone);
        }

        [Fact]
        public void SetTimeZone_WithValidTimeZoneInfo_ChangesTimeZone()
        {
            TimerHelper.SetTimeZone(UtcPlus8);

            Assert.Equal(UtcPlus8, TimerHelper.CurrentTimeZone);
        }

        [Fact]
        public void SetTimeZone_WithNull_FallsBackToUtc()
        {
            TimerHelper.SetTimeZone(UtcPlus8);
            TimerHelper.SetTimeZone((TimeZoneInfo)null);

            Assert.Equal(TimeZoneInfo.Utc, TimerHelper.CurrentTimeZone);
        }

        [Fact]
        public void SetTimeZone_WithValidId_ReturnsTrue()
        {
            var result = TimerHelper.SetTimeZone("UTC");

            Assert.True(result);
            Assert.Equal(TimeZoneInfo.Utc, TimerHelper.CurrentTimeZone);
        }

        [Fact]
        public void SetTimeZone_WithInvalidId_ReturnsFalseAndFallsBackToUtc()
        {
            TimerHelper.SetTimeZone(UtcPlus8);
            var result = TimerHelper.SetTimeZone("Invalid/TimeZone");

            Assert.False(result);
            Assert.Equal(TimeZoneInfo.Utc, TimerHelper.CurrentTimeZone);
        }

        [Fact]
        public void UnixTimeSecondsWithTimeZoneOffset_UtcPlus8_AddsEightHours()
        {
            TimerHelper.SetTimeZone(UtcPlus8);

            var diff = TimerHelper.UnixTimeSecondsWithTimeZoneOffset() - TimerHelper.UnixTimeSeconds();

            Assert.InRange(diff, 8 * 3600, 8 * 3600 + 2);
        }

        [Fact]
        public void UnixTimeMillisecondsWithTimeZoneOffset_UtcPlus8_AddsEightHours()
        {
            TimerHelper.SetTimeZone(UtcPlus8);

            var diff = TimerHelper.UnixTimeMillisecondsWithTimeZoneOffset() - TimerHelper.UnixTimeMilliseconds();

            Assert.InRange(diff, 8 * 3600 * 1000L, 8 * 3600 * 1000L + 2000);
        }

        [Fact]
        public void DateTimeToSecondsWithTimeZone_FixedUtcTime_AddsZoneOffset()
        {
            TimerHelper.SetTimeZone(UtcPlus8);
            var time = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            // 2024-01-01T00:00:00Z = 1704067200；UTC+8 偏移 28800 秒
            Assert.Equal(1704067200L + 28800, TimerHelper.DateTimeToSecondsWithTimeZone(time));
        }

        [Fact]
        public void TimeToMillisecondsWithTimeZone_FixedUtcTime_AddsZoneOffset()
        {
            TimerHelper.SetTimeZone(UtcPlus8);
            var time = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            Assert.Equal(1704067200000L + 28800000, TimerHelper.TimeToMillisecondsWithTimeZone(time));
        }

        [Fact]
        public void DateTimeToSecondsWithTimeZone_UnspecifiedTime_TreatedAsZoneTime()
        {
            TimerHelper.SetTimeZone(UtcPlus8);
            var time = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

            // Unspecified 按当前时区解释：2024-01-01 00:00 UTC+8 = 2023-12-31 16:00 UTC = 1704038400，再加偏移 28800 → 1704067200
            Assert.Equal(1704067200L, TimerHelper.DateTimeToSecondsWithTimeZone(time));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(-100)]
        public void DateTimeToSecond_UtcEpochBased(int deltaSeconds)
        {
            var time = TimerHelper.EpochUtc.AddSeconds(deltaSeconds);

            Assert.Equal(deltaSeconds, TimerHelper.DateTimeToSecond(time, utc: true));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-100)]
        public void DateTimeToMilliseconds_UtcEpochBased(int deltaMilliseconds)
        {
            var time = TimerHelper.EpochUtc.AddMilliseconds(deltaMilliseconds);

            Assert.Equal(deltaMilliseconds, TimerHelper.DateTimeToMilliseconds(time, utc: true));
        }

        [Fact]
        public void DateTimeToSecond_NonUtc_UsesCurrentZoneEpoch()
        {
            TimerHelper.SetTimeZone(UtcPlus8);
            var time = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);

            // 当前时区纪元墙钟 = ConvertTime(EpochUtc, UTC+8) = 1970-01-01 08:00；time(00:00) - 08:00 = -8h = -28800
            Assert.Equal(-28800, TimerHelper.DateTimeToSecond(time, utc: false));
        }

        [Fact]
        public void SyncServerTimeSeconds_EstablishesOffset()
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            TimerHelper.SyncServerTimeSeconds(now + 100);

            Assert.InRange(TimerHelper.TimeOffsetSeconds, 99, 101);
            Assert.InRange(TimerHelper.ServerNowSeconds() - now, 99, 102);
        }

        [Fact]
        public void SyncServerTimeMilliseconds_EstablishesOffset()
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            TimerHelper.SyncServerTimeMilliseconds(now + 5000);

            Assert.InRange(TimerHelper.TimeOffsetMilliseconds, 4999, 5001);
            Assert.InRange(TimerHelper.ServerNowMilliseconds() - now, 4999, 5100);
        }

        [Fact]
        public void ResetTimeOffset_ClearsOffset()
        {
            TimerHelper.SyncServerTimeSeconds(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 1000);
            TimerHelper.ResetTimeOffset();

            Assert.Equal(0, TimerHelper.TimeOffsetSeconds);
            Assert.Equal(0, TimerHelper.TimeOffsetMilliseconds);
        }

        [Fact]
        public void GetNowWithTimeZone_UtcPlus8_IsEightHoursAhead()
        {
            TimerHelper.SetTimeZone(UtcPlus8);

            var diff = TimerHelper.GetNowWithTimeZone() - DateTime.UtcNow;

            Assert.InRange(diff, TimeSpan.FromHours(8) - TimeSpan.FromSeconds(2), TimeSpan.FromHours(8) + TimeSpan.FromSeconds(2));
        }

        [Fact]
        public void CurrentTimeWithTimeZone_IsSixDigits()
        {
            TimerHelper.SetTimeZone(UtcPlus8);
            var value = TimerHelper.CurrentTimeWithTimeZone();

            Assert.InRange(value, 0, 999999);
        }
    }
}
