using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using TiendaGlobosLaFiesta.Data;
using TiendaGlobosLaFiesta.Models.Inventario;
using TiendaGlobosLaFiesta.Models.Ventas; 

namespace TiendaGlobosLaFiesta.ViewModels
{
    public class InventarioViewModel : BaseViewModel
    {
        private readonly StockManagerRepository _stockManager;
        private readonly ProductoRepository _productoRepo;
        private readonly GloboRepository _globoRepo;

        // Colecciones en memoria
        public ObservableCollection<Producto> ProductosView { get; set; }
        public ObservableCollection<Globo> GlobosView { get; set; }
        public ObservableCollection<StockCriticoItem> StockCritico { get; set; }

        // Vistas filtradas
        public ICollectionView ProductosViewFiltered { get; set; }
        public ICollectionView GlobosViewFiltered { get; set; }
        public ICollectionView StockCriticoView { get; set; }

        // Selecciones
        private Producto? _productoSeleccionado;
        public Producto? ProductoSeleccionado
        {
            get => _productoSeleccionado;
            set { _productoSeleccionado = value; OnPropertyChanged(); }
        }

        private Globo? _globoSeleccionado;
        public Globo? GloboSeleccionado
        {
            get => _globoSeleccionado;
            set { _globoSeleccionado = value; OnPropertyChanged(); }
        }

        // Filtros
        private string _productosFilter = string.Empty;
        public string ProductosFilter
        {
            get => _productosFilter;
            set { _productosFilter = value; OnPropertyChanged(); ProductosViewFiltered?.Refresh(); }
        }

        private string _globosFilter = string.Empty;
        public string GlobosFilter
        {
            get => _globosFilter;
            set { _globosFilter = value; OnPropertyChanged(); GlobosViewFiltered?.Refresh(); }
        }

        private string _stockCriticoFilter = string.Empty;
        public string StockCriticoFilter
        {
            get => _stockCriticoFilter;
            set { _stockCriticoFilter = value; OnPropertyChanged(); StockCriticoView?.Refresh(); }
        }

        // Constructor
        public InventarioViewModel()
        {
            _productoRepo = new ProductoRepository();
            _globoRepo = new GloboRepository();
            _stockManager = new StockManagerRepository(_productoRepo, _globoRepo);

            ProductosView = new ObservableCollection<Producto>();
            GlobosView = new ObservableCollection<Globo>();
            StockCritico = new ObservableCollection<StockCriticoItem>();

            ProductosViewFiltered = CollectionViewSource.GetDefaultView(ProductosView);
            ProductosViewFiltered.Filter = FiltroProductos;

            GlobosViewFiltered = CollectionViewSource.GetDefaultView(GlobosView);
            GlobosViewFiltered.Filter = FiltroGlobos;

            StockCriticoView = CollectionViewSource.GetDefaultView(StockCritico);
            StockCriticoView.Filter = FiltroStockCritico;

            CargarDatos();
        }


        // --- Filtros ---
        private bool FiltroProductos(object o) =>
            string.IsNullOrEmpty(ProductosFilter) || (o is Producto p && p.Nombre.Contains(ProductosFilter, StringComparison.OrdinalIgnoreCase));

        private bool FiltroGlobos(object o) =>
            string.IsNullOrEmpty(GlobosFilter) || (o is Globo g && g.Material.Contains(GlobosFilter, StringComparison.OrdinalIgnoreCase));

        private bool FiltroStockCritico(object o) =>
            string.IsNullOrEmpty(StockCriticoFilter) || (o is StockCriticoItem s && s.Nombre.Contains(StockCriticoFilter, StringComparison.OrdinalIgnoreCase));

        // --- Carga inicial ---
        private void CargarDatos()
        {
            try
            {
                ProductosView.Clear();
                foreach (var p in _productoRepo.ObtenerProductos())
                    ProductosView.Add(p);

                GlobosView.Clear();
                foreach (var g in _globoRepo.ObtenerGlobos())
                    GlobosView.Add(g);

                RefrescarStockCritico();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar datos: {ex.Message}");
            }
        }

        private void RefrescarStockCritico()
        {
            StockCritico.Clear();
            try
            {
                foreach (var item in _stockManager.ObtenerProductosStockCritico())
                    StockCritico.Add(item);

                foreach (var item in _stockManager.ObtenerGlobosStockCritico())
                    StockCritico.Add(item);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al refrescar stock crítico: {ex.Message}");
            }
        }

        // --- CRUD Productos ---
        public void AgregarProducto(Producto p)
        {
            try
            {
                _productoRepo.AgregarProducto(p);
                ProductosView.Add(p);
                ProductosViewFiltered.Refresh();
                RefrescarStockCritico();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al agregar producto: {ex.Message}");
            }
        }

        public void EditarProducto(Producto p)
        {
            try
            {
                if (_productoRepo.ActualizarProducto(p))
                {
                    var original = ProductosView.FirstOrDefault(x => x.ProductoId == p.ProductoId);
                    if (original != null)
                    {
                        int idx = ProductosView.IndexOf(original);
                        ProductosView[idx] = p;
                        ProductosViewFiltered.Refresh();
                    }
                    RefrescarStockCritico();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al editar producto: {ex.Message}");
            }
        }

        public void EliminarProducto(Producto p)
        {
            try
            {
                if (_productoRepo.EliminarProducto(p.ProductoId))
                {
                    ProductosView.Remove(p);
                    ProductosViewFiltered.Refresh();
                    RefrescarStockCritico();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar producto: {ex.Message}");
            }
        }

        // --- CRUD Globos ---
        public void AgregarGlobo(Globo g)
        {
            try
            {
                _globoRepo.AgregarGlobo(g);
                GlobosView.Add(g);
                GlobosViewFiltered.Refresh();
                RefrescarStockCritico();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al agregar globo: {ex.Message}");
            }
        }

        public void EditarGlobo(Globo g)
        {
            try
            {
                if (_globoRepo.ActualizarGlobo(g))
                {
                    var original = GlobosView.FirstOrDefault(x => x.GloboId == g.GloboId);
                    if (original != null)
                    {
                        int idx = GlobosView.IndexOf(original);
                        GlobosView[idx] = g;
                        GlobosViewFiltered.Refresh();
                    }
                    RefrescarStockCritico();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al editar globo: {ex.Message}");
            }
        }

        public void EliminarGlobo(Globo g)
        {
            try
            {
                if (_globoRepo.EliminarGlobo(g.GloboId))
                {
                    GlobosView.Remove(g);
                    GlobosViewFiltered.Refresh();
                    RefrescarStockCritico();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al eliminar globo: {ex.Message}");
            }
        }
    }
}