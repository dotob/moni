using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using MONI.Data;

namespace MONI.ValueConverter
{
    public class DurationBGColorConverter : IValueConverter
    {
        private readonly Brush defaultBrush;
        private readonly Brush lessHoursPerDayBrush;

        private static DurationBGColorConverter instance;

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static DurationBGColorConverter()
        {
        }

        private DurationBGColorConverter()
        {
            this.defaultBrush = Brushes.Transparent;
            this.defaultBrush.Freeze();
            this.lessHoursPerDayBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B3CD6969"));
            this.lessHoursPerDayBrush.Freeze();
        }

        public static DurationBGColorConverter Instance
        {
            get { return instance ?? (instance = new DurationBGColorConverter()); }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var duration = (double?)value;
            var b = this.defaultBrush;
            if (duration != null && duration < MoniSettings.Current.MainSettings.HoursPerDay)
            {
                b = this.lessHoursPerDayBrush;
            }
            return b;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}