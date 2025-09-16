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
using TiendaGlobosLaFiesta.Views;

namespace TiendaGlobosLaFiesta.ViewModels
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

        private string _textoBusquedaGlobo;
        public string TextoBusquedaGlobo { get => _textoBusquedaGlobo; set { _textoBusquedaGlobo = value; OnPropertyChanged(); GlobosView.Refresh(); } }

        private Producto _productoSeleccionado;
        public Producto ProductoSeleccionado { get => _productoSeleccionado; set { _productoSeleccionado = value; OnPropertyChanged(); (EditarProductoCommand as RelayCommand)?.RaiseCanExecuteChanged(); (EliminarProductoCommand as RelayCommand)?.RaiseCanExecuteChanged(); } }

        private Globo _globoSeleccionado;
        public Globo GloboSeleccionado { get => _globoSeleccionado; set { _globoSeleccionado = value; OnPropertyChanged(); (EditarGloboCommand as RelayCommand)?.RaiseCanExecuteChanged(); (EliminarGloboCommand as RelayCommand)?.RaiseCanExecuteChanged(); } }

        public ICommand AgregarProductoCommand { get; }
        public ICommand EditarProductoCommand { get; }
        public ICommand EliminarProductoCommand { get; }
        public ICommand AgregarGloboCommand { get; }
        public ICommand EditarGloboCommand { get; }
        public ICommand EliminarGloboCommand { get; }

        public InventarioViewModel()
        {
            CargarDatos();

            AgregarProductoCommand = new RelayCommand(ExecuteAgregarProducto);
            EditarProductoCommand = new RelayCommand(ExecuteEditarProducto, CanExecuteEditarOEliminarProducto);
            EliminarProductoCommand = new RelayCommand(ExecuteEliminarProducto, CanExecuteEditarOEliminarProducto);

            AgregarGloboCommand = new RelayCommand(ExecuteAgregarGlobo);
            EditarGloboCommand = new RelayCommand(ExecuteEditarGlobo, CanExecuteEditarOEliminarGlobo);
            EliminarGloboCommand = new RelayCommand(ExecuteEliminarGlobo, CanExecuteEditarOEliminarGlobo);
        }

        private void CargarDatos()
        {
            var productos = new ObservableCollection<Producto>(_productoRepo.ObtenerProductos());
            ProductosView = CollectionViewSource.GetDefaultView(productos);
            ProductosView.Filter = item => string.IsNullOrEmpty(TextoBusquedaProducto) ||
                                           (item as Producto).Nombre.Contains(TextoBusquedaProducto, StringComparison.OrdinalIgnoreCase);

            var globos = new ObservableCollection<Globo>(_globoRepo.ObtenerGlobos());
            GlobosView = CollectionViewSource.GetDefaultView(globos);
            GlobosView.Filter = item => string.IsNullOrEmpty(TextoBusquedaGlobo) ||
                                        (item as Globo).Nombre.Contains(TextoBusquedaGlobo, StringComparison.OrdinalIgnoreCase);

            var criticoProductos = _stockRepo.ObtenerProductosStockCritico()
                .Select(p => new StockCriticoItem { Id = p.Id, Nombre = p.Nombre, StockActual = p.StockActual, Tipo = "Producto" });
            var criticoGlobos = _stockRepo.ObtenerGlobosStockCritico()
                .Select(g => new StockCriticoItem { Id = g.Id, Nombre = g.Nombre, StockActual = g.StockActual, Tipo = "Globo" });
            StockCritico = new ObservableCollection<StockCriticoItem>(criticoProductos.Concat(criticoGlobos));

            OnPropertyChanged(nameof(ProductosView));
            OnPropertyChanged(nameof(GlobosView));
            OnPropertyChanged(nameof(StockCritico));
        }

        #region Lógica de Comandos para Productos
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

        private bool CanExecuteEditarOEliminarProducto(object obj) => ProductoSeleccionado != null;
        #endregion

        #region Lógica de Comandos para Globos
        private void ExecuteAgregarGlobo(object obj)
        {
            var nuevoGlobo = new Globo { GloboId = "GLOBO" + DateTime.Now.ToString("yyMMddHHmmss") };
            var ventana = new GloboEditWindow(nuevoGlobo);
            if (ventana.ShowDialog() == true) { _globoRepo.AgregarGlobo(ventana.Globo); CargarDatos(); }
        }

        private void ExecuteEditarGlobo(object obj)
        {
            var globoAEditar = GloboSeleccionado.Clone();
            var ventana = new GloboEditWindow(globoAEditar);
            if (ventana.ShowDialog() == true) { _globoRepo.ActualizarGlobo(ventana.Globo); CargarDatos(); }
        }

        private void ExecuteEliminarGlobo(object obj)
        {
            if (MessageBox.Show($"¿Estás seguro de eliminar el globo '{GloboSeleccionado.Nombre}'?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                _globoRepo.EliminarGlobo(GloboSeleccionado.GloboId);
                CargarDatos();
            }
        }

        private bool CanExecuteEditarOEliminarGlobo(object obj) => GloboSeleccionado != null;
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}