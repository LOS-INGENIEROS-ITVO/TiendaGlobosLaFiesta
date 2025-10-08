using System.ComponentModel;
using System.Linq;

namespace TiendaGlobosLaFiesta.Models.Ventas
{
    public abstract class ItemVenta : INotifyPropertyChanged
    {
        public abstract string Id { get; }
        public abstract string Nombre { get; }

        public string Unidad { get; set; } = "pieza";

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
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(Importe));
                }
            }
        }

        public int Stock { get; set; }
        public decimal Costo { get; set; }
        public decimal Importe => Cantidad * Costo;

        public void Incrementar() => Cantidad = Cantidad < Stock ? Cantidad + 1 : Stock;
        public void Decrementar() => Cantidad = Cantidad > 0 ? Cantidad - 1 : 0;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
