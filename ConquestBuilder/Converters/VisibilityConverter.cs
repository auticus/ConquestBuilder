using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ConquestBuilder.Converters
{
    [ValueConversion(typeof(bool), typeof(System.Windows.Visibility))]
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visibilityBool = (bool) value;
            return visibilityBool ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var visibilityValue = (Visibility) value;
            return (visibilityValue == Visibility.Visible) ? true : false;
        }
    }
}
