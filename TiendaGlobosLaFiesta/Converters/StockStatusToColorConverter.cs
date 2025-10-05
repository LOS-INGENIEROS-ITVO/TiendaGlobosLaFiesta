using System;
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
                if (stock <= 5) return Brushes.Red;
                if (stock <= 10) return Brushes.Orange;
                return Brushes.Green;
            }
            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}