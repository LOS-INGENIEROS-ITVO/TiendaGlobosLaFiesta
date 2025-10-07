using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using TiendaGlobosLaFiesta.Data;
using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta.ViewModels
{
    public class InventarioViewModel : BaseViewModel
    {
        private readonly StockManagerRepository _stockManager;
        private readonly ProductoRepository _productoRepo;
        private readonly GloboRepository _globoRepo;

        public ObservableCollection<Producto> ProductosView { get; set; }
        public ObservableCollection<Globo> GlobosView { get; set; }
        public ObservableCollection<StockCriticoItem> StockCritico { get; set; }

        public ICollectionView ProductosViewFiltered { get; set; }
        public ICollectionView GlobosViewFiltered { get; set; }
        public ICollectionView StockCriticoView { get; set; }

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

        private string _stockCriticoFilter;
        public string StockCriticoFilter
        {
            get => _stockCriticoFilter;
            set { _stockCriticoFilter = value; OnPropertyChanged(); StockCriticoView?.Refresh(); }
        }

        public InventarioViewModel()
        {
            // Instancia los repositorios individuales
            _productoRepo = new ProductoRepository();
            _globoRepo = new GloboRepository();

            // Inyecta los repositorios en StockManagerRepository
            _stockManager = new StockManagerRepository(_productoRepo, _globoRepo);

            ProductosView = new ObservableCollection<Producto>();
            GlobosView = new ObservableCollection<Globo>();
            StockCritico = new ObservableCollection<StockCriticoItem>();

            ProductosViewFiltered = CollectionViewSource.GetDefaultView(ProductosView);
            GlobosViewFiltered = CollectionViewSource.GetDefaultView(GlobosView);
            StockCriticoView = CollectionViewSource.GetDefaultView(StockCritico);

            // Filtros
            ProductosViewFiltered.Filter = o =>
                string.IsNullOrEmpty(ProductosFilter) || (o is Producto p && p.Nombre.Contains(ProductosFilter, StringComparison.OrdinalIgnoreCase));

            GlobosViewFiltered.Filter = o =>
                string.IsNullOrEmpty(GlobosFilter) || (o is Globo g && g.Material.Contains(GlobosFilter, StringComparison.OrdinalIgnoreCase));

            StockCriticoView.Filter = o =>
                string.IsNullOrEmpty(StockCriticoFilter) || (o is StockCriticoItem s && s.Nombre.Contains(StockCriticoFilter, StringComparison.OrdinalIgnoreCase));

            CargarDatos();
        }

        private void CargarDatos()
        {
            ProductosView.Clear();
            foreach (var p in _productoRepo.ObtenerProductos())
                ProductosView.Add(p);

            GlobosView.Clear();
            foreach (var g in _globoRepo.ObtenerGlobos())
                GlobosView.Add(g);

            RefrescarStockCritico();
        }

        private void RefrescarStockCritico()
        {
            StockCritico.Clear();
            foreach (var s in _stockManager.ObtenerProductosStockCritico())
                StockCritico.Add(s);

            foreach (var s in _stockManager.ObtenerGlobosStockCritico())
                StockCritico.Add(s);
        }

        // --- Métodos CRUD Productos ---
        public void AgregarProducto(Producto p)
        {
            _productoRepo.AgregarProducto(p);
            ProductosView.Add(p);
            RefrescarStockCritico();
        }

        public void EditarProducto(Producto p)
        {
            _productoRepo.ActualizarProducto(p);
            RefrescarStockCritico();
        }

        public void EliminarProducto(Producto p)
        {
            _productoRepo.EliminarProducto(p.ProductoId);
            ProductosView.Remove(p);
            RefrescarStockCritico();
        }

        // --- Métodos CRUD Globos ---
        public void AgregarGlobo(Globo g)
        {
            _globoRepo.AgregarGlobo(g);
            GlobosView.Add(g);
            RefrescarStockCritico();
        }

        public void EditarGlobo(Globo g)
        {
            _globoRepo.ActualizarGlobo(g);
            RefrescarStockCritico();
        }

        public void EliminarGlobo(Globo g)
        {
            _globoRepo.EliminarGlobo(g.GloboId);
            GlobosView.Remove(g);
            RefrescarStockCritico();
        }
    }
}