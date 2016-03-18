using System;
using System.Linq;

namespace MONI.Util {
    public class TimeSpanUtil {
        private const string defaultMinuteString = "min";
        private const string defaultHourString = "h";
        private const string defaultDayString = "d";

        public static string ConvertMinutes2String(double minutes) {
            return ConvertMinutes2String(minutes, defaultDayString, defaultHourString, defaultMinuteString);
        }

        public static string ConvertMinutes2String(double minutes, string dayString, string hourString, string minuteString) {
            return ConvertTimeSpan2String(TimeSpan.FromMinutes(minutes), dayString, hourString, minuteString);
        }

        public static string ConvertHours2String(double hours) {
            return ConvertHours2String(hours, defaultDayString, defaultHourString, defaultMinuteString);
        }

        public static string ConvertHours2String(double hours, string dayString, string hourString, string minuteString) {
            return ConvertTimeSpan2String(TimeSpan.FromHours(hours), dayString, hourString, minuteString);
        }

        public static string ConvertTimeSpan2String(TimeSpan timeSpan) {
            return ConvertTimeSpan2String(timeSpan, defaultDayString, defaultHourString, defaultMinuteString);
        }

        public static string ConvertTimeSpan2String(TimeSpan timeSpan, string dayString, string hourString, string minuteString) {
            if (timeSpan.TotalMinutes > 0) {
                var timeStr = string.Empty;
                if (timeSpan.Minutes > 0) {
                    timeStr = String.Format("{0:0}{1}", timeSpan.Minutes, minuteString);
                }
                if (timeSpan.Hours >= 1) {
                    timeStr = String.Format("{0:0}{1} {2}", timeSpan.Hours, hourString, timeStr).Trim();
                }
                if (timeSpan.Days >= 1) {
                    timeStr = String.Format("{0:0}{1} {2}", timeSpan.Days, dayString, timeStr).Trim();
                }
                return timeStr;
            }
            return String.Empty;
        }

        public static double ConvertTimeSpanString2Double(string timespanstring) {
            return ConvertTimeSpanString2Double(timespanstring, defaultDayString, defaultHourString, defaultMinuteString);
        }

        public static double ConvertTimeSpanString2Double(string timespanstring, string dayString, string hourString, string minuteString) {
            double ret = 0;
            if (!String.IsNullOrEmpty(timespanstring)) {
                var splitted = timespanstring.Split(' ');
                if (splitted.Length == 3) {
                    return GetMinutes(splitted[0], dayString, hourString, minuteString) + GetMinutes(splitted[1], dayString, hourString, minuteString) + GetMinutes(splitted[2], dayString, hourString, minuteString);
                }
                if (splitted.Length == 2) {
                    return GetMinutes(splitted[0], dayString, hourString, minuteString) + GetMinutes(splitted[1], dayString, hourString, minuteString);
                }
                if (splitted.Length == 1) {
                    return GetMinutes(splitted[0], dayString, hourString, minuteString);
                }
            }
            return ret;
        }

        public static double GetMinutes(string s) {
            return GetMinutes(s, defaultDayString, defaultHourString, defaultMinuteString);
        }

        public static double GetMinutes(string s, string dayString, string hourString, string minuteString) {
            var number = Double.Parse(NumberOnly(s));
            if (IdentifierOnly(s) == dayString) {
                return 1440 * number;
            }
            if (IdentifierOnly(s) == hourString) {
                return 60 * number;
            }
            if (IdentifierOnly(s) == minuteString) {
                return number;
            }
            return 0;
        }

        public static string NumberOnly(string s) {
            return new string(s.Where(Char.IsDigit).ToArray());
        }

        public static string IdentifierOnly(string s) {
            return new string(s.Where(Char.IsLetter).ToArray());
        }

        public static string UptimeString(DateTime startTime) {
            var uptime = DateTime.UtcNow - startTime;
            var uptimeString = String.Format("{0} (started @ {1:s}; {2}minutes)", ConvertMinutes2String(uptime.TotalMinutes), startTime, ((int) uptime.TotalMinutes));
            return uptimeString;
        }
    }
}