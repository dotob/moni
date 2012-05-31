using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MonlistClone.Util
{
  public class DurationBGColorConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      double duration = (double)value;
      Brush b = Brushes.Transparent;
      if (duration < 8) {
        b = Brushes.IndianRed;
      }
      return b;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }
  }
}