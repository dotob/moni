using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace MONI.ValueConverter
{
  public class TodayColorConverter : IValueConverter
  {
    private readonly Brush todayBrush;
    private readonly Brush notTodayBrush;

    private static TodayColorConverter instance;

    // Explicit static constructor to tell C# compiler
    // not to mark type as beforefieldinit
    static TodayColorConverter() {
    }

    private TodayColorConverter() {
      this.todayBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F000638E"));
      this.todayBrush.Freeze();
      this.notTodayBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CC119EDA"));
      this.notTodayBrush.Freeze();
    }

    public static TodayColorConverter Instance {
      get { return instance ?? (instance = new TodayColorConverter()); }
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      var isToday = (bool)value;
      var b = this.notTodayBrush;
      if (isToday) {
        b = this.todayBrush;
      }
      return b;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      return DependencyProperty.UnsetValue;
    }
  }
}