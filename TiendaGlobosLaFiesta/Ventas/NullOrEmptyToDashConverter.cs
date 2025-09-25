using System;
using System.Globalization;
using System.Windows.Data;

namespace TiendaGlobosLaFiesta.Ventas
{
    public class NullOrEmptyToDashConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value == null || string.IsNullOrWhiteSpace(value.ToString())) ? "-" : value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}