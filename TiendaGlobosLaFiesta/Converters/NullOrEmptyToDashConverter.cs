using System;
using System.Globalization;
using System.Windows.Data;

namespace TiendaGlobosLaFiesta.Converters
{
    public class NullOrEmptyToDashConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "---";
            if (value is string s && string.IsNullOrWhiteSpace(s)) return "---";
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}