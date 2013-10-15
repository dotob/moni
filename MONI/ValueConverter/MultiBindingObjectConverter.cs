using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace MONI.ValueConverter
{
  public class MultiBindingObjectConverter : IMultiValueConverter
  {
    private static MultiBindingObjectConverter instance;

    // Explicit static constructor to tell C# compiler
    // not to mark type as beforefieldinit
    static MultiBindingObjectConverter()
    {
    }

    private MultiBindingObjectConverter()
    {
    }

    public static MultiBindingObjectConverter Instance
    {
      get { return instance ?? (instance = new MultiBindingObjectConverter()); }
    }

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
      return values.ToArray();
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
      return targetTypes.Select(t => DependencyProperty.UnsetValue).ToArray();
    }
  }
}