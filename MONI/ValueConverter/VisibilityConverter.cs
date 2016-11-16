using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace MONI.ValueConverter
{
    public class VisibilityConverter : IValueConverter
    {
        public VisibilityConverter()
        {
            this.SupportIsNullOrEmpty = true;
            this.NotVisibleValue = Visibility.Collapsed;
        }

        public bool Inverse { get; set; }

        public bool SupportIsNullOrEmpty { get; set; }

        public Visibility NotVisibleValue { get; set; }

        public static object GetDefaultValue(Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool visible;

            if (value is string && this.SupportIsNullOrEmpty)
            {
                visible = !String.IsNullOrEmpty(value.ToString());
            }
            else
            {
                var defaultValue = value != null ? GetDefaultValue(value.GetType()) : null;

                visible = !Equals(value, defaultValue);
            }

            if (this.Inverse)
            {
                visible = !visible;
            }

            return visible ? Visibility.Visible : this.NotVisibleValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}