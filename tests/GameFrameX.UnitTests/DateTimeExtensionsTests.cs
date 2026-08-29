using System;
using Xunit;

namespace GameFrameX.UnitTests
{
    /// <summary>
    /// DateTime 扩展测试（对照 Unity 基准 DateTimeExtensionsTests）。
    /// </summary>
    public sealed class DateTimeExtensionsTests
    {
        [Fact]
        public void GetDaysFrom_OneDayEarlier_ReturnsMinusOne()
        {
            var later = new DateTime(2024, 6, 16);
            var earlier = new DateTime(2024, 6, 15);

            Assert.Equal(-1, earlier.GetDaysFrom(later));
        }

        [Fact]
        public void GetDaysFrom_CrossesMonthBoundary_CalculatesCorrectly()
        {
            var juneEnd = new DateTime(2024, 6, 30);
            var julyStart = new DateTime(2024, 7, 1);

            Assert.Equal(1, julyStart.GetDaysFrom(juneEnd));
        }

        [Fact]
        public void GetDaysFrom_IgnoresTimeComponent_UsesDateOnly()
        {
            var dt1 = new DateTime(2024, 6, 15, 23, 59, 59);
            var dt2 = new DateTime(2024, 6, 16, 0, 0, 0);

            Assert.Equal(1, dt2.GetDaysFrom(dt1));
        }

        [Fact]
        public void GetDaysFrom_SameDayDifferentTime_ReturnsZero()
        {
            var morning = new DateTime(2024, 6, 15, 6, 0, 0);
            var evening = new DateTime(2024, 6, 15, 22, 0, 0);

            Assert.Equal(0, evening.GetDaysFrom(morning));
        }

        [Fact]
        public void GetDaysFrom_LargeSpan_CalculatesCorrectly()
        {
            var start = new DateTime(2000, 1, 1);
            var end = new DateTime(2000, 12, 31);

            Assert.Equal(365, end.GetDaysFrom(start));
        }

        [Fact]
        public void GetDaysFrom_LeapYear_February()
        {
            var start = new DateTime(2024, 2, 28);
            var end = new DateTime(2024, 3, 1);

            Assert.Equal(2, end.GetDaysFrom(start));
        }

        [Fact]
        public void GetDaysFromDefault_UnixEpoch_ReturnsZero()
        {
            var epoch = new DateTime(1970, 1, 1);

            Assert.Equal(0, epoch.GetDaysFromDefault());
        }

        [Fact]
        public void GetDaysFromDefault_DayAfterEpoch_ReturnsOne()
        {
            var dayAfter = new DateTime(1970, 1, 2);

            Assert.Equal(1, dayAfter.GetDaysFromDefault());
        }

        [Fact]
        public void GetDaysFromDefault_BeforeEpoch_ReturnsNegative()
        {
            var before = new DateTime(1969, 12, 31);

            Assert.Equal(-1, before.GetDaysFromDefault());
        }

        [Fact]
        public void GetDaysFromDefault_KnownDate_CalculatesCorrectly()
        {
            var known = new DateTime(2000, 1, 1);
            var epoch = new DateTime(1970, 1, 1);
            int expected = (int)(known.Date - epoch).TotalDays;

            Assert.Equal(expected, known.GetDaysFromDefault());
        }
    }
}
