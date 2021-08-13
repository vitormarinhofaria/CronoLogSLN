using System;

namespace CronoLog.Utils
{
    public class DateUtils
    {
        private static void getDuration(long milli, out double minutes, out double hours, out double days)
        {
            minutes = Math.Floor((double)(milli / 60000));
            minutes = (minutes == -1) ? 0 : minutes;
            hours = Math.Floor(minutes / 60);
            days = Math.Floor(hours / 24);
            minutes %= 60;
            hours %= 24;
        }

        public static string DurationString(TimeSpan duration)
        {
            string minutes = (duration.Minutes < 10) ? $"0{duration.Minutes}" : $"{duration.Minutes}";
            string hours = (duration.Hours < 10) ? $"0{duration.Hours}" : $"{duration.Hours}";
            string days = (duration.Days < 10) ? $"0{duration.Days}" : $"{duration.Days}";

            return $"{days}:{hours}:{minutes}";
        }
        public static string HoursDuration(TimeSpan duration)
        {
            string minutes = (duration.Minutes < 10) ? $"0{duration.Minutes}" : duration.Minutes.ToString();
            int days = duration.Days * 24;
            string hours = (duration.Hours + days < 10) ? $"0{duration.Hours + days}" : $"{duration.Hours + days}";
            return $"{hours}h {minutes}m";
        }
    }
}
