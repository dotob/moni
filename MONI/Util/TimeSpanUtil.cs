using System;

namespace MONI.Util
{
    public class TimeSpanUtil
    {
        private const string DefaultDayString = " d";
        private const string DefaultHourString = " h";
        private const string DefaultMinuteString = " min";

        private static string ConvertMinutes2String(double minutes)
        {
            return ConvertMinutes2String(minutes, DefaultDayString, DefaultHourString, DefaultMinuteString);
        }

        private static string ConvertMinutes2String(double minutes, string dayString, string hourString, string minuteString)
        {
            return ConvertTimeSpan2String(TimeSpan.FromMinutes(minutes), dayString, hourString, minuteString);
        }

        public static string ConvertTimeSpan2String(TimeSpan timeSpan, string dayString, string hourString, string minuteString)
        {
            if (timeSpan.TotalMinutes > 0)
            {
                var timeStr = string.Empty;
                if (timeSpan.Minutes > 0)
                {
                    timeStr = $"{timeSpan.Minutes}{minuteString}";
                }

                if (timeSpan.Hours >= 1)
                {
                    timeStr = $"{timeSpan.Hours}{hourString} {timeStr}".Trim();
                }

                if (timeSpan.Days >= 1)
                {
                    timeStr = $"{timeSpan.Days}{dayString} {timeStr}".Trim();
                }

                if (!string.IsNullOrEmpty(timeStr))
                {
                    return timeStr;
                }
            }

            return $"0{minuteString}";
        }

        public static string UptimeString(DateTime startTimeUtc)
        {
            var uptime = DateTime.UtcNow - startTimeUtc;

            return $"{ConvertMinutes2String(uptime.TotalMinutes)} (started @ {startTimeUtc.ToLocalTime():s})";
        }
    }
}