using System.ComponentModel;

namespace TiendaGlobosLaFiesta.Inventario
{
    public class GloboInventario : INotifyPropertyChanged
    {
        public string GloboId { get; set; }
        public string Material { get; set; }
        public string Unidad { get; set; }
        public string Color { get; set; }
        public int Stock { get; set; }
        public decimal Costo { get; set; }

        // Relacionados
        public string Tamanios { get; set; }
        public string Formas { get; set; }
        public string Tematicas { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}