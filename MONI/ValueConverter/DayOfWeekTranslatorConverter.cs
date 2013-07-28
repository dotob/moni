using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MONI.ValueConverter
{
  public sealed class DayOfWeekTranslatorConverter : IValueConverter
  {
    private static DayOfWeekTranslatorConverter instance;

    // Explicit static constructor to tell C# compiler
    // not to mark type as beforefieldinit
    static DayOfWeekTranslatorConverter() {
    }

    private DayOfWeekTranslatorConverter() {
    }

    public static DayOfWeekTranslatorConverter Instance {
      get { return instance ?? (instance = new DayOfWeekTranslatorConverter()); }
    }

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
      return DependencyProperty.UnsetValue;
    }
  }
}