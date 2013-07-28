using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using MONI.Data;

namespace MONI.ValueConverter
{
  public class DurationFGColorConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      var duration = (double)value;
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