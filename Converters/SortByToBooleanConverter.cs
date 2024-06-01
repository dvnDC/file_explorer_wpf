using System;
using System.Globalization;
using System.Windows.Data;
using WpfApp1.Enums;

namespace WpfApp1.Converters
{
    public class SortByToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SortBy sortBy && parameter is string parameterString)
            {
                return sortBy.ToString().StartsWith(parameterString);
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue && parameter is string parameterString)
            {
                if (Enum.TryParse<SortBy>(parameterString, out var sortBy))
                {
                    return sortBy;
                }
            }
            return null;
        }
    }
}
