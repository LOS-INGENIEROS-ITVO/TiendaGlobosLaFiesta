using System.ComponentModel;

namespace TiendaGlobosLaFiesta.Models
{
    public class GloboVenta : INotifyPropertyChanged
    {




        public string GloboId { get; set; }
        public string Material { get; set; }
        public string Color { get; set; }
        public string Unidad { get; set; }

        // Concatenaremos los valores relacionados en una sola propiedad
        public string Tamano { get; set; }
        public string Forma { get; set; }
        public string Tematica { get; set; }


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
