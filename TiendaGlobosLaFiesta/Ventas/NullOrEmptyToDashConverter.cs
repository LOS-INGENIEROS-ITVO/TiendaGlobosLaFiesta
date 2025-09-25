using System;
using System.Globalization;
using System.Windows.Data;

namespace TiendaGlobosLaFiesta.Converters
{
    public class NullOrEmptyToDashConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Si el valor es nulo o un string vacío, devuelve un guion.
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return "---";
            }
            // De lo contrario, devuelve el valor original.
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}