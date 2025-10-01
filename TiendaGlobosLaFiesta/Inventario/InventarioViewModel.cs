using System.Collections.ObjectModel;
using System.ComponentModel;
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

        public ObservableCollection<Producto> Productos { get; set; }
        public ObservableCollection<Globo> Globos { get; set; }
        public ICollectionView ProductosView { get; private set; }
        public ICollectionView GlobosView { get; private set; }
        public ObservableCollection<StockCriticoItem> StockCritico { get; set; }

        public string HeaderProductos => $"🎁 Productos ({Productos?.Count ?? 0})";
        public string HeaderGlobos => $"🎈 Globos ({Globos?.Count ?? 0})";
        public string HeaderStockCritico => $"⚠️ Stock Crítico ({StockCritico?.Count ?? 0})";

        private string _textoBusquedaProducto;
        public string TextoBusquedaProducto { get => _textoBusquedaProducto; set { _textoBusquedaProducto = value; OnPropertyChanged(); ProductosView.Refresh(); } }

        private string _textoBusquedaGlobo;
        public string TextoBusquedaGlobo { get => _textoBusquedaGlobo; set { _textoBusquedaGlobo = value; OnPropertyChanged(); GlobosView.Refresh(); } }

        private Producto _productoSeleccionado;
        public Producto ProductoSeleccionado { get => _productoSeleccionado; set { _productoSeleccionado = value; OnPropertyChanged(); ((RelayCommand)EditarProductoCommand).RaiseCanExecuteChanged(); ((RelayCommand)EliminarProductoCommand).RaiseCanExecuteChanged(); ((RelayCommand)AjustarStockProductoCommand).RaiseCanExecuteChanged(); } }

        private Globo _globoSeleccionado;
        public Globo GloboSeleccionado { get => _globoSeleccionado; set { _globoSeleccionado = value; OnPropertyChanged(); ((RelayCommand)EditarGloboCommand).RaiseCanExecuteChanged(); ((RelayCommand)EliminarGloboCommand).RaiseCanExecuteChanged(); ((RelayCommand)AjustarStockGloboCommand).RaiseCanExecuteChanged(); } }

        public ICommand AgregarProductoCommand { get; }
        public ICommand EditarProductoCommand { get; }
        public ICommand EliminarProductoCommand { get; }
        public ICommand AjustarStockProductoCommand { get; }
        public ICommand AgregarGloboCommand { get; }
        public ICommand EditarGloboCommand { get; }
        public ICommand EliminarGloboCommand { get; }
        public ICommand AjustarStockGloboCommand { get; }

        public InventarioViewModel()
        {
            CargarDatos();

            AgregarProductoCommand = new RelayCommand(ExecuteAgregarProducto);
            EditarProductoCommand = new RelayCommand(ExecuteEditarProducto, CanExecuteAccionProducto);
            EliminarProductoCommand = new RelayCommand(ExecuteEliminarProducto, CanExecuteAccionProducto);
            AjustarStockProductoCommand = new RelayCommand(ExecuteAjustarStockProducto, CanExecuteAccionProducto);

            AgregarGloboCommand = new RelayCommand(ExecuteAgregarGlobo);
            EditarGloboCommand = new RelayCommand(ExecuteEditarGlobo, CanExecuteAccionGlobo);
            EliminarGloboCommand = new RelayCommand(ExecuteEliminarGlobo, CanExecuteAccionGlobo);
            AjustarStockGloboCommand = new RelayCommand(ExecuteAjustarStockGlobo, CanExecuteAccionGlobo);
        }

        private void CargarDatos()
        {
            Productos = new ObservableCollection<Producto>(_productoRepo.ObtenerProductos());
            Globos = new ObservableCollection<Globo>(_globoRepo.ObtenerGlobos());
            ProductosView = CollectionViewSource.GetDefaultView(Productos);
            GlobosView = CollectionViewSource.GetDefaultView(Globos);

            ProductosView.Filter = item => string.IsNullOrEmpty(TextoBusquedaProducto) || (item as Producto).Nombre.Contains(TextoBusquedaProducto, StringComparison.OrdinalIgnoreCase);
            GlobosView.Filter = item => string.IsNullOrEmpty(TextoBusquedaGlobo) || (item as Globo).Nombre.Contains(TextoBusquedaGlobo, StringComparison.OrdinalIgnoreCase);

            var criticoProductos = _stockRepo.ObtenerProductosStockCritico().Select(p => new StockCriticoItem { Id = p.Id, Nombre = p.Nombre, StockActual = p.StockActual, Tipo = "Producto" });
            var criticoGlobos = _stockRepo.ObtenerGlobosStockCritico().Select(g => new StockCriticoItem { Id = g.Id, Nombre = g.Nombre, StockActual = g.StockActual, Tipo = "Globo" });
            StockCritico = new ObservableCollection<StockCriticoItem>(criticoProductos.Concat(criticoGlobos));

            OnPropertyChanged(string.Empty);
        }

        private bool CanExecuteAccionProducto(object obj) => ProductoSeleccionado != null;
        private bool CanExecuteAccionGlobo(object obj) => GloboSeleccionado != null;

        #region Comandos de Productos
        private void ExecuteAgregarProducto(object obj)
        {
            var nuevoProducto = new Producto { ProductoId = "PROD" + DateTime.Now.ToString("yyMMddHHmmss") };
            var ventana = new ProductoEditWindow(nuevoProducto);
            if (ventana.ShowDialog() == true)
            {
                _productoRepo.AgregarProducto(ventana.Producto);
                CargarDatos();
            }
        }

        private void ExecuteEditarProducto(object obj)
        {
            var productoAEditar = ProductoSeleccionado.Clone();
            var ventana = new ProductoEditWindow(productoAEditar);
            if (ventana.ShowDialog() == true)
            {
                _productoRepo.ActualizarProducto(ventana.Producto);
                CargarDatos();
            }
        }

        private void ExecuteEliminarProducto(object obj)
        {
            if (MessageBox.Show($"¿Desactivar '{ProductoSeleccionado.Nombre}'? No se borrará, solo se ocultará de las listas.", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                _productoRepo.EliminarProducto(ProductoSeleccionado.ProductoId);
                CargarDatos();
            }
        }

        private void ExecuteAjustarStockProducto(object obj)
        {
            var ventana = new AjustarStockWindow(ProductoSeleccionado.Nombre);
            if (ventana.ShowDialog() == true)
            {
                int cantidadAnterior = ProductoSeleccionado.Stock;
                ProductoSeleccionado.Stock = ventana.NuevaCantidad;
                _productoRepo.ActualizarProducto(ProductoSeleccionado);
                _productoRepo.RegistrarAjusteStock(ProductoSeleccionado.ProductoId, cantidadAnterior, ProductoSeleccionado.Stock, ventana.Motivo);
                CargarDatos();
            }
        }
        #endregion

        #region Comandos de Globos
        private void ExecuteAgregarGlobo(object obj)
        {
            var nuevoGlobo = new Globo { GloboId = "GLOBO" + DateTime.Now.ToString("yyMMddHHmmss") };
            var ventana = new GloboEditWindow(nuevoGlobo);
            if (ventana.ShowDialog() == true)
            {
                _globoRepo.AgregarGlobo(ventana.Globo);
                CargarDatos();
            }
        }

        private void ExecuteEditarGlobo(object obj)
        {
            var globoAEditar = GloboSeleccionado.Clone();
            var ventana = new GloboEditWindow(globoAEditar);
            if (ventana.ShowDialog() == true)
            {
                _globoRepo.ActualizarGlobo(ventana.Globo);
                CargarDatos();
            }
        }

        private void ExecuteEliminarGlobo(object obj)
        {
            if (MessageBox.Show($"¿Desactivar el globo '{GloboSeleccionado.Nombre}'?", "Confirmar", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                _globoRepo.EliminarGlobo(GloboSeleccionado.GloboId);
                CargarDatos();
            }
        }

        private void ExecuteAjustarStockGlobo(object obj)
        {
            // Similar a productos, pero llamando a GloboRepository
            // (Esta lógica se añadiría si necesitas ajustar stock de globos)
            MessageBox.Show("Funcionalidad de ajuste de stock para globos por implementar.");
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}