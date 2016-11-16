using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MONI.ValueConverter
{
    public class ScaleDoubleConverter : IValueConverter
    {
        private static ScaleDoubleConverter instance;

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static ScaleDoubleConverter()
        {
        }

        private ScaleDoubleConverter()
        {
        }

        public static ScaleDoubleConverter Instance
        {
            get { return instance ?? (instance = new ScaleDoubleConverter()); }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double scale;
            if (value is double && parameter != null && double.TryParse(parameter.ToString(), out scale))
            {
                return Math.Max(1, scale * (double)value);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}