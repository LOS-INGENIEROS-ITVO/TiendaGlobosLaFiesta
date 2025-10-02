using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Data;
using TiendaGlobosLaFiesta.Data;
using TiendaGlobosLaFiesta.Modelos;
using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta.ViewModels
{
    public class ModeloDeVistaVentas : INotifyPropertyChanged
    {
        private readonly ProductoRepository _productoRepo = new();
        private readonly GloboRepository _globoRepo = new();
        private readonly ClienteRepository _clienteRepo = new();
        private readonly VentasRepository _ventasRepo = new();

        public ObservableCollection<ProductoVenta> Productos { get; set; }
        public ObservableCollection<GloboVenta> Globos { get; set; }
        public ObservableCollection<Cliente> Clientes { get; set; }
        public ObservableCollection<VentaHistorial> Historial { get; set; }
        public ICollectionView HistorialView { get; private set; }

        public ModeloDeVistaVentas()
        {
            CargarDatosIniciales();
        }

        public void CargarDatosIniciales()
        {
            Clientes = new ObservableCollection<Cliente>(_clienteRepo.ObtenerClientes().Where(c => c.Activo));

            Productos = new ObservableCollection<ProductoVenta>(
                _productoRepo.ObtenerProductos().Select(p => new ProductoVenta
                {
                    Id = p.ProductoId,
                    ProductoId = p.ProductoId,
                    Nombre = p.Nombre,
                    Costo = p.Costo,
                    Stock = p.Stock,
                    Unidad = p.Unidad
                })
            );

            Globos = new ObservableCollection<GloboVenta>(
                _globoRepo.ObtenerGlobos().Select(g => new GloboVenta
                {
                    Id = g.GloboId,
                    GloboId = g.GloboId,
                    Material = g.Material,
                    Unidad = g.Unidad,
                    Color = g.Color,
                    Stock = g.Stock,
                    Costo = g.Costo,
                    Tamano = g.Tamano,
                    Forma = g.Forma,
                    Tematica = g.Tematica
                })
            );

            foreach (var p in Productos) p.PropertyChanged += ItemVenta_PropertyChanged;
            foreach (var g in Globos) g.PropertyChanged += ItemVenta_PropertyChanged;

            CargarHistorial();
            OnPropertyChanged(string.Empty);
        }

        public void CargarHistorial()
        {
            Historial = new ObservableCollection<VentaHistorial>(_ventasRepo.ObtenerHistorialVentas());
            HistorialView = CollectionViewSource.GetDefaultView(Historial);
            OnPropertyChanged(nameof(Historial));
            OnPropertyChanged(nameof(HistorialView));
        }

        private void ItemVenta_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ItemVenta.Cantidad))
            {
                OnPropertyChanged(nameof(TotalProductos));
                OnPropertyChanged(nameof(TotalGlobos));
                OnPropertyChanged(nameof(ImporteTotal));
            }
        }

        public int TotalProductos => Productos.Sum(p => p.Cantidad);
        public int TotalGlobos => Globos.Sum(g => g.Cantidad);
        public decimal ImporteTotal => Productos.Sum(p => p.Importe) + Globos.Sum(g => g.Importe);

        public void FiltrarHistorial(Cliente cliente, DateTime? desde, DateTime? hasta)
        {
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
            HistorialView.Filter = null;
            HistorialView.Refresh();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}