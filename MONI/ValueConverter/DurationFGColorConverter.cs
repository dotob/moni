using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using MONI.Data;

namespace MONI.ValueConverter
{
    public class DurationFGColorConverter : IValueConverter
    {
        private readonly Brush defaultBrush;
        private readonly Brush lessHoursPerDayBrush;

        private static DurationFGColorConverter instance;

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static DurationFGColorConverter()
        {
        }

        private DurationFGColorConverter()
        {
            this.defaultBrush = Brushes.DarkSlateGray;
            this.defaultBrush.Freeze();
            this.lessHoursPerDayBrush = Brushes.WhiteSmoke;
            this.lessHoursPerDayBrush.Freeze();
        }

        public static DurationFGColorConverter Instance
        {
            get { return instance ?? (instance = new DurationFGColorConverter()); }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var duration = (double)value;
            var b = this.defaultBrush;
            if (duration < MoniSettings.Current.MainSettings.HoursPerDay)
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