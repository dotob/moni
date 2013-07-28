using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MONI.ValueConverter
{
  public class LessThanColorConverter : IMultiValueConverter
  {
    private static readonly Brush okBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CC7DBEDA"));
    private static readonly Brush notOkBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B3CD6969"));

    public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture) {
      try {
        var need = (double)value[0];
        var have = (double)value[1];
        var b = okBrush;
        if (have < need) {
          b = notOkBrush;
        }
        return b;
      } catch (Exception exception) {
        return okBrush;
      }
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }
  }
}