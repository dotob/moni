using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MONI.Util
{
  public class LessThanColorConverter : IMultiValueConverter
  {
    static Brush okBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CC7DBEDA"));
    static Brush notOkBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B3CD6969"));

    public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture) {
      try {
        double need = (double)value[0];
        double have = (double)value[1];
        Brush b = okBrush;
        if (have < need) {
          b = notOkBrush;
        }
        return b;
      }
      catch (Exception exception) {
        return okBrush;
      }
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }
  }
}