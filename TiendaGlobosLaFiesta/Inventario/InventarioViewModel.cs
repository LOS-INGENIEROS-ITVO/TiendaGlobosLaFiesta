using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using TiendaGlobosLaFiesta.Data;
using TiendaGlobosLaFiesta.Modelos;
using TiendaGlobosLaFiesta.Models;
using TiendaGlobosLaFiesta.ViewModels;

namespace TiendaGlobosLaFiesta.Inventario
{
    public class InventarioViewModel : INotifyPropertyChanged
    {
        private readonly ProductoRepository _productoRepo = new();
        private readonly GloboRepository _globoRepo = new();
        private readonly StockRepository _stockRepo = new();

        public ICollectionView ProductosView { get; private set; }
        public ICollectionView GlobosView { get; private set; }
        public ObservableCollection<StockCriticoItem> StockCritico { get; set; }
        private string _textoBusquedaProducto;
        public string TextoBusquedaProducto { get => _textoBusquedaProducto; set { _textoBusquedaProducto = value; OnPropertyChanged(); ProductosView.Refresh(); } }
        private Producto _productoSeleccionado;
        public Producto ProductoSeleccionado { get => _productoSeleccionado; set { _productoSeleccionado = value; OnPropertyChanged(); (EditarProductoCommand as RelayCommand)?.RaiseCanExecuteChanged(); (EliminarProductoCommand as RelayCommand)?.RaiseCanExecuteChanged(); } }
        public ICommand AgregarProductoCommand { get; }
        public ICommand EditarProductoCommand { get; }
        public ICommand EliminarProductoCommand { get; }

        public InventarioViewModel()
        {
            CargarDatos();
            AgregarProductoCommand = new RelayCommand(ExecuteAgregarProducto);
            EditarProductoCommand = new RelayCommand(ExecuteEditarProducto, CanExecuteEditarOEliminar);
            EliminarProductoCommand = new RelayCommand(ExecuteEliminarProducto, CanExecuteEditarOEliminar);
        }
        private void CargarDatos()
        {
            var productos = new ObservableCollection<Producto>(_productoRepo.ObtenerProductos());
            ProductosView = CollectionViewSource.GetDefaultView(productos);
            ProductosView.Filter = item => string.IsNullOrEmpty(TextoBusquedaProducto) || (item as Producto).Nombre.Contains(TextoBusquedaProducto, StringComparison.OrdinalIgnoreCase);

            var globos = new ObservableCollection<Globo>(_globoRepo.ObtenerGlobos());
            GlobosView = CollectionViewSource.GetDefaultView(globos);

            var criticoProductos = _stockRepo.ObtenerProductosStockCritico().Select(p => new StockCriticoItem { Id = p.ProductoId, Nombre = p.Nombre, StockActual = p.Stock, Tipo = "Producto" });
            var criticoGlobos = _stockRepo.ObtenerGlobosStockCritico().Select(g => new StockCriticoItem { Nombre = g.Nombre, StockActual = g.Stock, Tipo = "Globo" });
            StockCritico = new ObservableCollection<StockCriticoItem>(criticoProductos.Concat(criticoGlobos));

            OnPropertyChanged(nameof(ProductosView));
            OnPropertyChanged(nameof(GlobosView));
            OnPropertyChanged(nameof(StockCritico));
        }
        private void ExecuteAgregarProducto(object obj)
        {
            var nuevoProducto = new Producto { ProductoId = "PROD" + DateTime.Now.ToString("yyMMddHHmmss") };
            var ventana = new ProductoEditWindow(nuevoProducto);
            if (ventana.ShowDialog() == true) { _productoRepo.AgregarProducto(ventana.Producto); CargarDatos(); }
        }
        private void ExecuteEditarProducto(object obj)
        {
            var productoAEditar = ProductoSeleccionado.Clone();
            var ventana = new ProductoEditWindow(productoAEditar);
            if (ventana.ShowDialog() == true) { _productoRepo.ActualizarProducto(ventana.Producto); CargarDatos(); }
        }
        private void ExecuteEliminarProducto(object obj)
        {
            if (MessageBox.Show($"¿Estás seguro de eliminar '{ProductoSeleccionado.Nombre}'?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                _productoRepo.EliminarProducto(ProductoSeleccionado.ProductoId);
                CargarDatos();
            }
        }
        private bool CanExecuteEditarOEliminar(object obj) => ProductoSeleccionado != null;
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}