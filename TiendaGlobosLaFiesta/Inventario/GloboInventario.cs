using System.ComponentModel;

namespace TiendaGlobosLaFiesta.Models
{
    public class GloboInventario : INotifyPropertyChanged
    {
        public string GloboId { get; set; }

        private string material;
        public string Material
        {
            get => material;
            set
            {
                if (material != value)
                {
                    material = value;
                    OnPropertyChanged(nameof(Material));
                }
            }
        }

        private string color;
        public string Color
        {
            get => color;
            set
            {
                if (color != value)
                {
                    color = value;
                    OnPropertyChanged(nameof(Color));
                }
            }
        }

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
