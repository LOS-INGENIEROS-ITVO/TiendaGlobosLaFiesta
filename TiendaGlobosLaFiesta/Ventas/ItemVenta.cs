using System.ComponentModel;

namespace TiendaGlobosLaFiesta.Models
{
    public abstract class ItemVenta : INotifyPropertyChanged
    {
        private int cantidad;
        private int stock;

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

        // Precio base
        public decimal Costo { get; set; }

        // Alias para compatibilidad con VentasControl
        public decimal Precio
        {
            get => Costo;
            set => Costo = value;
        }

        // Importe total según cantidad
        public decimal Importe => Cantidad * Costo;

        public void Incrementar() => Cantidad = Cantidad < Stock ? Cantidad + 1 : Cantidad;
        public void Decrementar() => Cantidad = Cantidad > 0 ? Cantidad - 1 : 0;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}