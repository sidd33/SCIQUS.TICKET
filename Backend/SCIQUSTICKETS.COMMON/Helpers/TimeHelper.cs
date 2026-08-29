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

        public static DateTime CalculateSlaBusinessHours(DateTime clockStart, int slaInHours, string supportHours, bool includesWeekend)
        {
            if (supportHours == "24x7" && includesWeekend)
            {
                return clockStart.AddHours(slaInHours);
            }

            int startHour = 9;
            int endHour = 18; // 6 PM
            if (supportHours == "ExtendedBusinessHours")
            {
                startHour = 8;
                endHour = 20; // 8 PM
            }
            else if (supportHours == "24x7")
            {
                startHour = 0;
                endHour = 24;
            }

            DateTime current = clockStart;
            double remainingHours = slaInHours;

            while (remainingHours > 0)
            {
                // Skip weekends if not included
                if (!includesWeekend && (current.DayOfWeek == DayOfWeek.Saturday || current.DayOfWeek == DayOfWeek.Sunday))
                {
                    current = current.Date.AddDays(1).AddHours(startHour);
                    continue;
                }

                // If current time is before business hours, move to start
                if (current.TimeOfDay.TotalHours < startHour)
                {
                    current = current.Date.AddHours(startHour);
                }

                // If current time is after or at business end, move to next day
                if (current.TimeOfDay.TotalHours >= endHour)
                {
                    current = current.Date.AddDays(1).AddHours(startHour);
                    continue;
                }

                // Calculate time remaining in today's business hours
                double hoursLeftToday = endHour - current.TimeOfDay.TotalHours;

                if (remainingHours <= hoursLeftToday)
                {
                    current = current.AddHours(remainingHours);
                    remainingHours = 0;
                }
                else
                {
                    remainingHours -= hoursLeftToday;
                    current = current.Date.AddDays(1).AddHours(startHour);
                }
            }

            return current;
        }
    }
}
