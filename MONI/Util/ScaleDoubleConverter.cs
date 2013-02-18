using System;
using System.Globalization;
using System.Windows.Data;

namespace MONI.Util {
  public class ScaleDoubleConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      double scale;
      if (value is double && parameter!=null && double.TryParse(parameter.ToString(), out scale)) {
        return scale * (double)value;
      }
      return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }
  }
}