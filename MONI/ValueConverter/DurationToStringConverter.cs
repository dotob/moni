using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using MONI.Data;

namespace MONI.ValueConverter
{
    public class DurationToStringConverter : IValueConverter
    {
        private static DurationToStringConverter instance;

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static DurationToStringConverter()
        {
        }

        private DurationToStringConverter()
        {
        }

        public static DurationToStringConverter Instance
        {
            get { return instance ?? (instance = new DurationToStringConverter()); }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var duration = (double?)value;
            var durationString = duration != null ? string.Format("{0:f} h", duration) : string.Empty;
            return durationString;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}