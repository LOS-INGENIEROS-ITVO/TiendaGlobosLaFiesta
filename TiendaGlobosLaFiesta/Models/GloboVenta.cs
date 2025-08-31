using System.ComponentModel;

namespace TiendaGlobosLaFiesta.Models
{
    public class GloboVenta : INotifyPropertyChanged
    {
        public string GloboId { get; set; }
        public string Material { get; set; }
        public string Color { get; set; }
        public string Unidad { get; set; }

        private int stock;
        public int Stock
        {
            get => stock;
            set
            {
                if (stock != value)
                {
                    stock = value;
                    OnPropertyChanged(nameof(Stock));
                }
            }
        }

        public decimal Costo { get; set; }
        public string Tamano { get; set; }
        public string Forma { get; set; }
        public string Tematica { get; set; }

        private int cantidad;
        public int Cantidad
        {
            get => cantidad;
            set
            {
                if (cantidad != value)
                {
                    cantidad = value;
                    OnPropertyChanged(nameof(Cantidad));
                    OnPropertyChanged(nameof(Importe));
                }
            }
        }

        public decimal Importe => Cantidad * Costo;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}