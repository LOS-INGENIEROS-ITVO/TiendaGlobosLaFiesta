using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using TiendaGlobosLaFiesta.Core;
using TiendaGlobosLaFiesta.Data;
using TiendaGlobosLaFiesta.Models.Clientes;
using TiendaGlobosLaFiesta.Models.Ventas;
using TiendaGlobosLaFiesta.Models.Inventario;
using TiendaGlobosLaFiesta.Services;
using System.Diagnostics;

namespace TiendaGlobosLaFiesta.ViewModels
{
    public class VentasViewModel : INotifyPropertyChanged
    {
        private readonly VentaService _ventaService;
        private readonly ProductoRepository _productoRepo;
        private readonly GloboRepository _globoRepo;

        public VentasViewModel()
        {
            _ventaService = new VentaService();
            _productoRepo = new ProductoRepository();
            _globoRepo = new GloboRepository();

            Clientes = new ObservableCollection<Cliente>();
            Productos = new ObservableCollection<ProductoVenta>();
            Globos = new ObservableCollection<GloboVenta>();
            Historial = new ObservableCollection<VentaHistorial>();

            // Comandos
            RegistrarVentaCommand = new RelayCommand(async _ => await RegistrarVentaAsync(), _ => CanRegistrarVenta());
            AumentarCantidadProductoCommand = new RelayCommand(AumentarCantidadProducto);
            DisminuirCantidadProductoCommand = new RelayCommand(DisminuirCantidadProducto);
            AumentarCantidadGloboCommand = new RelayCommand(AumentarCantidadGlobo);
            DisminuirCantidadGloboCommand = new RelayCommand(DisminuirCantidadGlobo);

            ModuloManager.Instancia.StockActualizado += OnStockActualizado;

            Task.Run(() => CargarDatosInicialesAsync());
        }

        #region Propiedades

        private Cliente _clienteSeleccionado;
        public Cliente ClienteSeleccionado
        {
            get => _clienteSeleccionado;
            set
            {
                _clienteSeleccionado = value;
                OnPropertyChanged();
                ((RelayCommand)RegistrarVentaCommand).RaiseCanExecuteChanged();
            }
        }

        public ObservableCollection<ProductoVenta> Productos { get; }
        public ObservableCollection<GloboVenta> Globos { get; }
        public ObservableCollection<Cliente> Clientes { get; }
        public ObservableCollection<VentaHistorial> Historial { get; private set; }
        public ICollectionView HistorialView { get; private set; }

        public int TotalProductos => Productos.Sum(p => p.Cantidad);
        public int TotalGlobos => Globos.Sum(g => g.Cantidad);
        public decimal ImporteTotal => Productos.Sum(p => p.Importe) + Globos.Sum(g => g.Importe);

        #endregion

        #region Comandos

        public ICommand RegistrarVentaCommand { get; }
        public ICommand AumentarCantidadProductoCommand { get; }
        public ICommand DisminuirCantidadProductoCommand { get; }
        public ICommand AumentarCantidadGloboCommand { get; }
        public ICommand DisminuirCantidadGloboCommand { get; }

        private bool CanRegistrarVenta() =>
            ClienteSeleccionado != null &&
            (Productos.Any(p => p.Cantidad > 0) || Globos.Any(g => g.Cantidad > 0));

        #endregion

        #region Métodos Async

        public async Task CargarDatosInicialesAsync()
        {
            await Task.Delay(10); // Simular async DB call

            // Cargar clientes
            Clientes.Clear();
            foreach (var c in _ventaService.ObtenerClientes())
                Clientes.Add(c);

            // Cargar productos y globos
            await ActualizarStockAsync();

            // Cargar historial
            Historial = new ObservableCollection<VentaHistorial>(_ventaService.ObtenerHistorialVentas());
            HistorialView = CollectionViewSource.GetDefaultView(Historial);

            OnPropertyChanged(nameof(Historial));
            OnPropertyChanged(nameof(HistorialView));
        }

        public async Task RegistrarVentaAsync()
        {
            try
            {
                var venta = new Venta
                {
                    ClienteId = ClienteSeleccionado?.ClienteId,
                    Productos = new ObservableCollection<ProductoVenta>(Productos.Where(p => p.Cantidad > 0)),
                    Globos = new ObservableCollection<GloboVenta>(Globos.Where(g => g.Cantidad > 0)),
                    VentaId = Guid.NewGuid().ToString()
                };

                string error = null;

                bool resultado = await Task.Run(() => _ventaService.RegistrarVentaCompleta(venta, out error));

                if (resultado)
                {
                    await ActualizarStockAsync();

                    Historial.Insert(0, _ventaService.ObtenerUltimaVenta());
                    ModuloManager.Instancia.NotificarVentaRegistrada();

                    OnPropertyChanged(nameof(TotalProductos));
                    OnPropertyChanged(nameof(TotalGlobos));
                    OnPropertyChanged(nameof(ImporteTotal));
                }
                else
                {
                    Debug.WriteLine($"❌ Error al registrar venta: {error}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error en RegistrarVentaAsync: {ex.Message}");
            }
        }

        #endregion

        #region Métodos de Cantidad (ICommand)

        private void AumentarCantidadProducto(object parameter)
        {
            if (parameter is ProductoVenta p && p.Cantidad < p.Stock)
                p.Cantidad++;
        }

        private void DisminuirCantidadProducto(object parameter)
        {
            if (parameter is ProductoVenta p && p.Cantidad > 0)
                p.Cantidad--;
        }

        private void AumentarCantidadGlobo(object parameter)
        {
            if (parameter is GloboVenta g && g.Cantidad < g.Stock)
                g.Cantidad++;
        }

        private void DisminuirCantidadGlobo(object parameter)
        {
            if (parameter is GloboVenta g && g.Cantidad > 0)
                g.Cantidad--;
        }

        #endregion

        #region Manejo de Stock

        private async void OnStockActualizado()
        {
            await ActualizarStockAsync();
        }

        private async Task ActualizarStockAsync()
        {
            var productosActuales = _productoRepo.ObtenerProductos().Where(p => p.Stock > 0).ToList();
            var globosActuales = _globoRepo.ObtenerGlobos().Where(g => g.Stock > 0).ToList();

            ActualizarColeccion(
                Productos,
                productosActuales,
                (pv, p) => pv.ProductoId == p.ProductoId,
                (pv, p) =>
                {
                    pv.NombreProducto = p.Nombre; // ✅ usar propiedad subyacente
                    pv.Costo = p.Costo;
                    pv.Stock = p.Stock;
                    pv.Cantidad = Math.Min(pv.Cantidad, p.Stock);
                },
                p => new ProductoVenta
                {
                    ProductoId = p.ProductoId,
                    NombreProducto = p.Nombre, // ✅ usar propiedad subyacente
                    Costo = p.Costo,
                    Stock = p.Stock,
                    Cantidad = 0
                }
            );

            ActualizarColeccion(
                Globos,
                globosActuales,
                (gv, g) => gv.GloboId == g.GloboId,
                (gv, g) =>
                {
                    gv.Material = g.Material;
                    gv.Color = g.Color;
                    gv.Tamano = g.Tamano;
                    gv.Forma = g.Forma;
                    gv.Tematica = g.Tematica;
                    gv.Costo = g.Costo;
                    gv.Stock = g.Stock;
                    gv.Cantidad = Math.Min(gv.Cantidad, g.Stock);
                },
                g => new GloboVenta
                {
                    GloboId = g.GloboId,
                    Material = g.Material,
                    Color = g.Color,
                    Tamano = g.Tamano,
                    Forma = g.Forma,
                    Tematica = g.Tematica,
                    Costo = g.Costo,
                    Stock = g.Stock,
                    Cantidad = 0
                }
            );

            OnPropertyChanged(nameof(TotalProductos));
            OnPropertyChanged(nameof(TotalGlobos));
            OnPropertyChanged(nameof(ImporteTotal));
        }

        private void ActualizarColeccion<T, U>(
            ObservableCollection<T> coleccion,
            List<U> nuevosItems,
            Func<T, U, bool> comparar,
            Action<T, U> actualizar,
            Func<U, T> crearNuevo)
            where T : INotifyPropertyChanged
        {
            foreach (var item in nuevosItems)
            {
                var existing = coleccion.FirstOrDefault(c => comparar(c, item));
                if (existing != null)
                    actualizar(existing, item);
                else
                    coleccion.Add(crearNuevo(item));
            }

            for (int i = coleccion.Count - 1; i >= 0; i--)
            {
                if (!nuevosItems.Any(x => comparar(coleccion[i], x)))
                    coleccion.RemoveAt(i);
            }
        }

        #endregion

        #region Filtrado Historial

        public void FiltrarHistorial(Cliente cliente, DateTime? desde, DateTime? hasta)
        {
            if (HistorialView == null) return;

            HistorialView.Filter = obj =>
            {
                if (obj is VentaHistorial vh)
                {
                    bool clienteOk = cliente == null || vh.ClienteId == cliente.ClienteId;
                    bool desdeOk = !desde.HasValue || vh.FechaVenta.Date >= desde.Value.Date;
                    bool hastaOk = !hasta.HasValue || vh.FechaVenta.Date <= hasta.Value.Date;
                    return clienteOk && desdeOk && hastaOk;
                }
                return false;
            };
            HistorialView.Refresh();
        }

        public void LimpiarFiltros()
        {
            if (HistorialView == null) return;
            HistorialView.Filter = null;
            HistorialView.Refresh();
        }

        public void AgregarCliente(Cliente cliente)
        {
            if (cliente == null) return;
            Clientes.Add(cliente);
            ModuloManager.Instancia.NotificarClienteAgregado();
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        #endregion
    }
}