using System;
using System.Globalization;
using System.Windows.Data;
using WpfApp1.Enums;

namespace WpfApp1.Converters
{
    public class DirectionToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Direction direction && parameter is string parameterString)
            {
                return direction.ToString().StartsWith(parameterString);
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue && parameter is string parameterString)
            {
                if (Enum.TryParse<Direction>(parameterString, out var direction))
                {
                    return direction;
                }
            }
            return null;
        }
    }
}
