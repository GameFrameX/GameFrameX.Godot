namespace System
{
    public static class DateTimeExtensions
    {
        public static int GetDaysFrom(this DateTime now, DateTime dt)
        {
            // 迁移备注：dt 取 .Date 与 Unity 基准对齐（忽略时间分量；2026-08 补测试时发现的语义修复）。
            return (int)(now.Date - dt.Date).TotalDays;
        }

        public static int GetDaysFromDefault(this DateTime now)
        {
            return now.GetDaysFrom(new DateTime(1970, 1, 1).Date);
        }
    }
}