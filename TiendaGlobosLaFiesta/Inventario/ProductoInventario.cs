using System.ComponentModel;

namespace TiendaGlobosLaFiesta.Models
{
    public class ProductoInventario : INotifyPropertyChanged
    {
        public string ProductoId { get; set; }
        public string Nombre { get; set; }
        public int Unidad { get; set; }

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

        private decimal costo;
        public decimal Costo
        {
            get => costo;
            set
            {
                if (costo != value)
                {
                    costo = value;
                    OnPropertyChanged(nameof(Costo));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
    }
}
