using System;
using System.Globalization;
using System.Windows.Data;

namespace MONI.ValueConverter
{
  public class Null2FalseConverter : IValueConverter
  {
    private static Null2FalseConverter instance;

    // Explicit static constructor to tell C# compiler
    // not to mark type as beforefieldinit
    static Null2FalseConverter() {
    }

    private Null2FalseConverter() {
    }

    public static Null2FalseConverter Instance {
      get { return instance ?? (instance = new Null2FalseConverter()); }
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      return value != null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      return null;
    }
  }
}