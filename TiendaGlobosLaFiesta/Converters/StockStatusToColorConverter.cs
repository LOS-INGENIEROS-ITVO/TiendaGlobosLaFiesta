using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TiendaGlobosLaFiesta.Converters
{
    public class StockStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int stock)
            {
                if (stock == 0) return Brushes.Red;
                if (stock <= 5) return Brushes.OrangeRed;
                if (stock <= 10) return Brushes.Gold;
            }
            return Brushes.Transparent;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}