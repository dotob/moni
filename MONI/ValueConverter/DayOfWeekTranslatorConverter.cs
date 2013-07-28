using System;
using System.Globalization;
using System.Windows.Data;

namespace MONI.ValueConverter
{
  public class DayOfWeekTranslatorConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      var dt = (DayOfWeek)value;
      switch (dt) {
        case DayOfWeek.Sunday:
          return "Sonntag";
        case DayOfWeek.Monday:
          return "Montag";
        case DayOfWeek.Tuesday:
          return "Dienstag";
        case DayOfWeek.Wednesday:
          return "Mittwoch";
        case DayOfWeek.Thursday:
          return "Donnerstag";
        case DayOfWeek.Friday:
          return "Freitag";
        case DayOfWeek.Saturday:
          return "Samstag";
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }
  }
}