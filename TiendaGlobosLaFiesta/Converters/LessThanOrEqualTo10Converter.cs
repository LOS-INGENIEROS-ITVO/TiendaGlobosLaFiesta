using System;
using System.Globalization;
using System.Windows.Data;

namespace TiendaGlobosLaFiesta.Converters
{
    public class LessThanOrEqualTo10Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int stock)
                return stock <= 10 && stock > 0;
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}