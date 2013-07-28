using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace MONI.ValueConverter
{
  public class TodayColorConverter : IValueConverter
  {
    private static readonly BrushConverter bc = new BrushConverter();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      var isToday = (bool)value;

      var b = bc.ConvertFromString("#CC119EDA") as Brush;
      if (isToday) {
        b = bc.ConvertFromString("#F000638E") as Brush;
      }
      return b;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }
  }
}