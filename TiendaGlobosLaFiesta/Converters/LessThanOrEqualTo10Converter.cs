using System.Globalization;
using System.Windows.Data;

namespace TiendaGlobosLaFiesta.Converters
{
    public class LessThanOrEqualTo10Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is int stock && stock > 0 && stock <= 10;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}