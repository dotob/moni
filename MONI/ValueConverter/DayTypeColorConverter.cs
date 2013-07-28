using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using MONI.Data;

namespace MONI.ValueConverter
{
  public class DayTypeColorConverter : IValueConverter
  {
    private readonly Brush workingBrush;
    private readonly Brush weekendBrush;
    private readonly Brush holidayBrush;
    private readonly Brush defaultBrush;

    private static DayTypeColorConverter instance;

    // Explicit static constructor to tell C# compiler
    // not to mark type as beforefieldinit
    static DayTypeColorConverter() {
    }

    private DayTypeColorConverter() {
      this.workingBrush = new SolidColorBrush(Color.FromArgb(255, 243, 237, 237));
      this.workingBrush.Freeze();
      this.weekendBrush = Brushes.LightGreen;
      this.weekendBrush.Freeze();
      this.holidayBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CC119EDA"));
      this.holidayBrush.Freeze();
      this.defaultBrush = Brushes.Transparent;
      this.defaultBrush.Freeze();
    }

    public static DayTypeColorConverter Instance {
      get { return instance ?? (instance = new DayTypeColorConverter()); }
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      var dt = (DayType)value;
      var b = this.defaultBrush;
      switch (dt) {
        case DayType.Unknown:
        case DayType.Working:
          b = this.workingBrush;
          break;
        case DayType.Weekend:
          b = this.weekendBrush;
          break;
        case DayType.Holiday:
          b = this.holidayBrush;
          break;
      }
      return b;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      return DependencyProperty.UnsetValue;
    }
  }
}