using System.ComponentModel;

namespace TiendaGlobosLaFiesta.Models
{
    // Cliente
    public class Cliente : INotifyPropertyChanged
    {
        private string _primerNombre;
        private string _segundoNombre;
        private string _apellidoP;
        private string _apellidoM;
        private string _telefono;

        public string clienteId { get; set; }

        public string primerNombre
        {
            get => _primerNombre;
            set { _primerNombre = value; OnPropertyChanged(nameof(primerNombre)); }
        }

        public string segundoNombre
        {
            get => _segundoNombre;
            set { _segundoNombre = value; OnPropertyChanged(nameof(segundoNombre)); }
        }

        public string apellidoP
        {
            get => _apellidoP;
            set { _apellidoP = value; OnPropertyChanged(nameof(apellidoP)); }
        }

        public string apellidoM
        {
            get => _apellidoM;
            set { _apellidoM = value; OnPropertyChanged(nameof(apellidoM)); }
        }

        public string telefono
        {
            get => _telefono;
            set { _telefono = value; OnPropertyChanged(nameof(telefono)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // Producto general
    public class Producto : INotifyPropertyChanged
    {
        public string productoId { get; set; }
        public string nombre { get; set; }

        private int _stock;
        public int stock
        {
            get => _stock;
            set { _stock = value; OnPropertyChanged(nameof(stock)); }
        }

        public double costo { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // Producto para venta (cantidad editable)
    public class ProductoVenta : INotifyPropertyChanged
    {
        public string productoId { get; set; }
        public string nombre { get; set; }
        public int stock { get; set; }

        private int _cantidad;
        public int cantidad
        {
            get => _cantidad;
            set { _cantidad = value; OnPropertyChanged(nameof(cantidad)); OnPropertyChanged(nameof(Importe)); }
        }

        public double costo { get; set; }
        public double Importe => cantidad * costo;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // Globo general
    public class Globo : INotifyPropertyChanged
    {
        public string globoId { get; set; }
        public string material { get; set; }
        public string color { get; set; }

        private int _stock;
        public int stock
        {
            get => _stock;
            set { _stock = value; OnPropertyChanged(nameof(stock)); }
        }

        public double costo { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // Globo para venta (cantidad editable)
    public class GloboVenta : INotifyPropertyChanged
    {
        public string globoId { get; set; }
        public string material { get; set; }
        public string color { get; set; }

        private int _cantidad;
        public int cantidad
        {
            get => _cantidad;
            set { _cantidad = value; OnPropertyChanged(nameof(cantidad)); OnPropertyChanged(nameof(Importe)); }
        }

        public double costo { get; set; }
        public double Importe => cantidad * costo;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
