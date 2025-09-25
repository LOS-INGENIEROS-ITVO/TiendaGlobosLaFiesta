using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TiendaGlobosLaFiesta.Models
{
    public abstract class ItemVenta : INotifyPropertyChanged
    {
        // 🔹 PROPIEDAD AÑADIDA: Un identificador genérico para productos o globos
        public string Id { get; set; }
        public string Unidad { get; set; }
        public string Nombre { get; set; }


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
        public decimal Importe => Cantidad * Costo;

        public void Incrementar() => Cantidad = (Cantidad < Stock) ? Cantidad + 1 : Stock;
        public void Decrementar() => Cantidad = (Cantidad > 0) ? Cantidad - 1 : 0;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}