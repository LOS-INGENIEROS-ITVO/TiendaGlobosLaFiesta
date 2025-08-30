using System;
using System.ComponentModel;

namespace TiendaGlobosLaFiesta.Models
{
    // Clase para productos en venta
    public class ProductoVenta : INotifyPropertyChanged
    {
        public string ProductoId { get; set; }
        public string Nombre { get; set; }
        public string Unidad { get; set; }
        public int Stock { get; set; }
        public decimal Costo { get; set; }

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

        public decimal Importe => Costo * Cantidad;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
