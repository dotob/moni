using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using MONI.Data;

namespace MONI.ValueConverter
{
  public class DayTypeColorConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      var dt = (DayType)value;
      Brush b = Brushes.Transparent;
      switch (dt) {
        case DayType.Unknown:
        case DayType.Working:
          b = new SolidColorBrush(Color.FromArgb(255, 243, 237, 237));
          break;
        case DayType.Weekend:
          b = Brushes.LightGreen;
          break;
        case DayType.Holiday:
          b = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CC119EDA"));
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
      return b;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }
  }
}