using System;
using System.Globalization;
using System.Windows.Data;
using MONI.Data;

namespace MONI.ValueConverter
{
  public class TimeItemConverter : IValueConverter
  {
    #region IValueConverter Members

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

    #endregion
  }
}