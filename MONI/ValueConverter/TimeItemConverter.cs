using System;
using System.Globalization;
using System.Windows.Data;
using MONI.Data;

namespace MONI.ValueConverter
{
  public class TimeItemConverter : IValueConverter
  {
    private static TimeItemConverter instance;

    // Explicit static constructor to tell C# compiler
    // not to mark type as beforefieldinit
    static TimeItemConverter() {
    }

    private TimeItemConverter() {
    }

    public static TimeItemConverter Instance {
      get { return instance ?? (instance = new TimeItemConverter()); }
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      var ti = value as TimeItem;
      if (ti != null) {
        return ti.ToString();
      }
      return Binding.DoNothing;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      var tiAsString = value as string;
      if (tiAsString != null) {
        TimeItem ti;
        if (TimeItem.TryParse(tiAsString, out ti)) {
          return ti;
        }
      }
      return Binding.DoNothing;
    }
  }
}