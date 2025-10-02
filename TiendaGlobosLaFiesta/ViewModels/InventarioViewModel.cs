using System.Collections.ObjectModel;
using TiendaGlobosLaFiesta.Models;
using TiendaGlobosLaFiesta.Data;
using System.Windows.Input;

namespace TiendaGlobosLaFiesta.ViewModels
{
    public class InventarioViewModel : BaseViewModel
    {
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

        public InventarioViewModel()
        {
            ProductosView = new ObservableCollection<Producto>();
            GlobosView = new ObservableCollection<Globo>();
            StockCritico = new ObservableCollection<StockCriticoItem>();

            // Inicializar comandos
            AgregarProductoCommand = new RelayCommand(_ => AgregarProducto());
            EditarProductoCommand = new RelayCommand(_ => EditarProducto(), _ => ProductoSeleccionado != null);
            EliminarProductoCommand = new RelayCommand(_ => EliminarProducto(), _ => ProductoSeleccionado != null);

            AgregarGloboCommand = new RelayCommand(_ => AgregarGlobo());
            EditarGloboCommand = new RelayCommand(_ => EditarGlobo(), _ => GloboSeleccionado != null);
            EliminarGloboCommand = new RelayCommand(_ => EliminarGlobo(), _ => GloboSeleccionado != null);

            // Cargar datos al iniciar
            CargarDatos();
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
            var repo = new StockRepository();

            foreach (var p in repo.ObtenerProductosStockCritico())
                StockCritico.Add(p);

            foreach (var g in repo.ObtenerGlobosStockCritico())
                StockCritico.Add(g);
        }

        #endregion

        #region CRUD Productos

        private void AgregarProducto() { /* lógica con ProductoRepository y ventana de edición */ }
        private void EditarProducto() { /* lógica con ProductoRepository y ventana de edición */ }
        private void EliminarProducto() { /* lógica con ProductoRepository y ventana de confirmación */ }

        #endregion

        #region CRUD Globos

        private void AgregarGlobo() { /* lógica con GloboRepository y ventana de edición */ }
        private void EditarGlobo() { /* lógica con GloboRepository y ventana de edición */ }
        private void EliminarGlobo() { /* lógica con GloboRepository y ventana de confirmación */ }

        #endregion
    }
}