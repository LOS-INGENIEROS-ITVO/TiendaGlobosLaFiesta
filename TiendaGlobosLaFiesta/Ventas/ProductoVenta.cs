using System.ComponentModel;

namespace TiendaGlobosLaFiesta.Models
{
    public class ProductoVenta : INotifyPropertyChanged
    {
        public string ProductoId { get; set; }
        public string Nombre { get; set; }
        public string Unidad { get; set; } // <--- agregar
        public int Stock { get; set; }

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

        public decimal Costo { get; set; }
        public decimal Importe => Costo * Cantidad;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
