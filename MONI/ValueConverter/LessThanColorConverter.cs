using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using NLog;

namespace MONI.ValueConverter
{
    public class LessThanColorConverter : IMultiValueConverter
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly Brush okBrush;
        private readonly Brush notOkBrush;

        private static LessThanColorConverter instance;

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static LessThanColorConverter()
        {
        }

        private LessThanColorConverter()
        {
            this.okBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CC7DBEDA"));
            this.okBrush.Freeze();
            this.notOkBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B3CD6969"));
            this.notOkBrush.Freeze();
        }

        public static LessThanColorConverter Instance
        {
            get { return instance ?? (instance = new LessThanColorConverter()); }
        }

        public object Convert(object[] value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var need = (double)value[0];
                var have = (double)value[1];
                var b = this.okBrush;
                if (have < need)
                {
                    b = this.notOkBrush;
                }
                return b;
            }
            catch (Exception exception)
            {
                logger.Error(exception, "Could not convert value to brush!");
                return this.okBrush;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return targetTypes.Select(t => DependencyProperty.UnsetValue).ToArray();
        }
    }
}