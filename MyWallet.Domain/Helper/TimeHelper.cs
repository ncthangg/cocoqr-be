namespace MyWallet.Domain.Helper
{
    public static class TimeHelper
    {
        // Always get current time in UTC+7
        public static DateTime GetCurrentTimeInUtcPlus7()
        {
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); // UTC+7
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
        }

        // Convert any DateTime to UTC+7
        public static DateTime ConvertToUtcPlus7(DateTime dateTime)
        {
            TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            if (dateTime.Kind == DateTimeKind.Utc)
            {
                return TimeZoneInfo.ConvertTimeFromUtc(dateTime, tz);
            }
            else if (dateTime.Kind == DateTimeKind.Local)
            {
                DateTime utc = dateTime.ToUniversalTime();
                return TimeZoneInfo.ConvertTimeFromUtc(utc, tz);
            }
            else // Unspecified
            {
                DateTime utc = DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
                return TimeZoneInfo.ConvertTimeFromUtc(utc, tz);
            }
        }
    }
}
