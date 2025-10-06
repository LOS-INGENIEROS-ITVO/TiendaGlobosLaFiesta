using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;
using TiendaGlobosLaFiesta.Data;
using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta.ViewModels
{
    public class InventarioViewModel : BaseViewModel
    {
        // Repositorio
        private readonly StockManagerRepository _stockManager;

        // Colecciones
        public ObservableCollection<Producto> ProductosView { get; set; }
        public ObservableCollection<Globo> GlobosView { get; set; }
        public ObservableCollection<StockCriticoItem> StockCritico { get; set; }

        // Selecciones
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

        // Comandos
        public ICommand AgregarProductoCommand { get; set; }
        public ICommand EditarProductoCommand { get; set; }
        public ICommand EliminarProductoCommand { get; set; }
        public ICommand AgregarGloboCommand { get; set; }
        public ICommand EditarGloboCommand { get; set; }
        public ICommand EliminarGloboCommand { get; set; }

        // Filtros
        private string _stockCriticoFilter;
        public string StockCriticoFilter
        {
            get => _stockCriticoFilter;
            set { _stockCriticoFilter = value; OnPropertyChanged(); StockCriticoView?.Refresh(); }
        }

        private string _productosFilter;
        public string ProductosFilter
        {
            get => _productosFilter;
            set { _productosFilter = value; OnPropertyChanged(); ProductosViewFiltered?.Refresh(); }
        }

        private string _globosFilter;
        public string GlobosFilter
        {
            get => _globosFilter;
            set { _globosFilter = value; OnPropertyChanged(); GlobosViewFiltered?.Refresh(); }
        }

        // Vistas filtradas
        public ICollectionView StockCriticoView { get; set; }
        public ICollectionView ProductosViewFiltered { get; set; }
        public ICollectionView GlobosViewFiltered { get; set; }

        public InventarioViewModel()
        {
            _stockManager = new StockManagerRepository();

            ProductosView = new ObservableCollection<Producto>();
            GlobosView = new ObservableCollection<Globo>();
            StockCritico = new ObservableCollection<StockCriticoItem>();

            // Comandos
            AgregarProductoCommand = new RelayCommand(_ => AgregarProducto());
            EditarProductoCommand = new RelayCommand(_ => EditarProducto(), _ => ProductoSeleccionado != null);
            EliminarProductoCommand = new RelayCommand(_ => EliminarProducto(), _ => ProductoSeleccionado != null);

            AgregarGloboCommand = new RelayCommand(_ => AgregarGlobo());
            EditarGloboCommand = new RelayCommand(_ => EditarGlobo(), _ => GloboSeleccionado != null);
            EliminarGloboCommand = new RelayCommand(_ => EliminarGlobo(), _ => GloboSeleccionado != null);

            // Cargar datos
            CargarDatos();

            // Inicializar vistas filtradas
            StockCriticoView = CollectionViewSource.GetDefaultView(StockCritico);
            ProductosViewFiltered = CollectionViewSource.GetDefaultView(ProductosView);
            GlobosViewFiltered = CollectionViewSource.GetDefaultView(GlobosView);

            // Filtros
            StockCriticoView.Filter = o =>
            {
                if (string.IsNullOrEmpty(StockCriticoFilter)) return true;
                if (o is StockCriticoItem item)
                    return item.Nombre.Contains(StockCriticoFilter, StringComparison.OrdinalIgnoreCase);
                return false;
            };

            ProductosViewFiltered.Filter = o =>
            {
                if (string.IsNullOrEmpty(ProductosFilter)) return true;
                if (o is Producto p)
                    return !string.IsNullOrEmpty(p.Nombre) && p.Nombre.Contains(ProductosFilter, StringComparison.OrdinalIgnoreCase);
                return false;
            };

            GlobosViewFiltered.Filter = o =>
            {
                if (string.IsNullOrEmpty(GlobosFilter)) return true;
                if (o is Globo g)
                    return !string.IsNullOrEmpty(g.Nombre) && g.Nombre.Contains(GlobosFilter, StringComparison.OrdinalIgnoreCase);
                return false;
            };
        }

        #region Carga de Datos
        private void CargarDatos()
        {
            CargarProductos();
            CargarGlobos();
            CargarStockCritico();
        }

        private void CargarProductos()
        {
            ProductosView.Clear();
            var repo = new ProductoRepository();
            foreach (var p in repo.ObtenerProductos())
                ProductosView.Add(p);
        }

        private void CargarGlobos()
        {
            GlobosView.Clear();
            var repo = new GloboRepository();
            foreach (var g in repo.ObtenerGlobos())
                GlobosView.Add(g);
        }

        private void CargarStockCritico()
        {
            StockCritico.Clear();
            foreach (var p in _stockManager.ObtenerProductosStockCritico())
                StockCritico.Add(p);
            foreach (var g in _stockManager.ObtenerGlobosStockCritico())
                StockCritico.Add(g);
        }
        #endregion

        #region CRUD Placeholder
        private void AgregarProducto() { }
        private void EditarProducto() { }
        private void EliminarProducto() { }
        private void AgregarGlobo() { }
        private void EditarGlobo() { }
        private void EliminarGlobo() { }
        #endregion
    }
}