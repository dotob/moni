using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using MONI.Data;

namespace MONI.Util
{
  public class DurationFGColorConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      double duration = (double)value;
      Brush b = Brushes.DarkSlateGray;
      if (duration < MoniSettings.Current.MainSettings.HoursPerDay) {
        b = Brushes.WhiteSmoke;
      }
      return b;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }
  }
}