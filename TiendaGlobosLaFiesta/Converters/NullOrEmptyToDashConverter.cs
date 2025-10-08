using System;
using System.Globalization;
using System.Windows.Data;

namespace TiendaGlobosLaFiesta.Converters
{
    public class NullOrEmptyToDashConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var texto = value as string;
            return string.IsNullOrWhiteSpace(texto) ? "---" : texto;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}