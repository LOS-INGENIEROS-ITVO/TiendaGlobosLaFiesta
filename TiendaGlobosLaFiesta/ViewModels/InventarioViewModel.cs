using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TiendaGlobosLaFiesta.Models;
using System.Windows.Input;

namespace TiendaGlobosLaFiesta.ViewModels
{
    public class InventarioViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<StockCriticoItem> StockCritico { get; set; }
        public ObservableCollection<Producto> ProductosView { get; set; }
        public ObservableCollection<Globo> GlobosView { get; set; }

        private Producto _productoSeleccionado;
        public Producto ProductoSeleccionado
        {
            get => _productoSeleccionado;
            set { _productoSeleccionado = value; OnPropertyChanged(); }
        }

        private Globo _globoSeleccionado;
        public Globo GloboSeleccionado
        {
            get => _globoSeleccionado;
            set { _globoSeleccionado = value; OnPropertyChanged(); }
        }

        public string TextoBusquedaProducto { get; set; }
        public string TextoBusquedaGlobo { get; set; }

        // Comandos
        public ICommand AgregarProductoCommand { get; set; }
        public ICommand EditarProductoCommand { get; set; }
        public ICommand EliminarProductoCommand { get; set; }

        public ICommand AgregarGloboCommand { get; set; }
        public ICommand EditarGloboCommand { get; set; }
        public ICommand EliminarGloboCommand { get; set; }

        public InventarioViewModel()
        {
            StockCritico = new ObservableCollection<StockCriticoItem>();
            ProductosView = new ObservableCollection<Producto>();
            GlobosView = new ObservableCollection<Globo>();

            AgregarProductoCommand = new RelayCommand(_ => AgregarProducto());
            EditarProductoCommand = new RelayCommand(_ => EditarProducto());
            EliminarProductoCommand = new RelayCommand(_ => EliminarProducto());

            AgregarGloboCommand = new RelayCommand(_ => AgregarGlobo());
            EditarGloboCommand = new RelayCommand(_ => EditarGlobo());
            EliminarGloboCommand = new RelayCommand(_ => EliminarGlobo());
        }

        private void AgregarProducto() { /* lógica */ }
        private void EditarProducto() { /* lógica */ }
        private void EliminarProducto() { /* lógica */ }

        private void AgregarGlobo() { /* lógica */ }
        private void EditarGlobo() { /* lógica */ }
        private void EliminarGlobo() { /* lógica */ }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}