using System;
using System.Globalization;

namespace MONI.Util
{
    public static class CalendarHelper
    {
        /// <summary>Returns the week of the year that includes the date in the specified <see cref="T:System.DateTime" /> value.</summary>
        /// <param name="calendar">A calendar to work with.</param>
        /// <param name="date">A date and time value. </param>
        /// <returns>A positive integer that represents the week of the year that includes the date in the <paramref name="time" /> parameter.</returns>
        public static int GetWeekOfYear(this Calendar calendar, DateTime date)
        {
            var weekRule = CultureInfo.CurrentCulture.DateTimeFormat.CalendarWeekRule;
            var firstDayOfWeek = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
            return CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(date, weekRule, firstDayOfWeek);
        }
    }
}