namespace SCIQUSTICKETS.COMMON.Helpers
{
    /// <summary>
    /// All business timestamps in the ticketing system use this helper (Indian Standard Time).
    /// Email/WhatsApp provider fields that arrive in UTC stay UTC and are converted only for display.
    /// </summary>
    public static class TimeHelper
    {
        private static readonly TimeZoneInfo IndianTimeZone = GetIndianTimeZone();

        public static DateTime GetIndianTime()
        {
            var utcNow = DateTime.UtcNow;
            return TimeZoneInfo.ConvertTimeFromUtc(utcNow, IndianTimeZone);
        }

        public static DateTime ConvertUtcToIndianTime(DateTime utcDateTime)
        {
            if (utcDateTime.Kind == DateTimeKind.Unspecified)
                utcDateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

            return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, IndianTimeZone);
        }

        private static TimeZoneInfo GetIndianTimeZone()
        {
            // "India Standard Time" on Windows, "Asia/Kolkata" on Linux/macOS (IANA).
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");
            }
            catch (TimeZoneNotFoundException)
            {
                return TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
            }
        }
    }
}
