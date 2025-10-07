using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TiendaGlobosLaFiesta.Models
{
    public abstract class ItemVenta : INotifyPropertyChanged
    {
        public string Id { get; set; } = string.Empty;
        public string Unidad { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;

        private int cantidad;
        public int Cantidad
        {
            get => cantidad;
            set
            {
                if (value > Stock) value = Stock;
                if (value < 0) value = 0;

                if (cantidad != value)
                {
                    cantidad = value;
                    OnPropertyChanged(nameof(Cantidad));
                    OnPropertyChanged(nameof(Importe));
                }
            }
        }

        public int Stock { get; set; }
        public decimal Costo { get; set; }
        public decimal Importe => Cantidad * Costo;

        public void Incrementar() => Cantidad = (Cantidad < Stock) ? Cantidad + 1 : Stock;
        public void Decrementar() => Cantidad = (Cantidad > 0) ? Cantidad - 1 : 0;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}