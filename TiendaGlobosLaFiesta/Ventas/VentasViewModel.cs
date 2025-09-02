using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using TiendaGlobosLaFiesta.Models;
using TiendaGlobosLaFiesta.Data;

namespace TiendaGlobosLaFiesta.ViewModels
{
    public class VentasViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ProductoVenta> Productos { get; set; }
        public ObservableCollection<GloboVenta> Globos { get; set; }
        public ObservableCollection<Cliente> Clientes { get; set; }
        public ObservableCollection<VentaHistorial> Historial { get; set; }

        public ICollectionView HistorialView { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public VentasViewModel()
        {
            CargarDatos();
        }

        private void CargarDatos()
        {
            Clientes = new ObservableCollection<Cliente>(ConexionBD.ObtenerClientes());

            Productos = new ObservableCollection<ProductoVenta>(ConexionBD.ObtenerProductos());
            foreach (var p in Productos) p.PropertyChanged += ItemVenta_PropertyChanged;

            Globos = new ObservableCollection<GloboVenta>(ConexionBD.ObtenerGlobos());
            foreach (var g in Globos) g.PropertyChanged += ItemVenta_PropertyChanged;

            Historial = new ObservableCollection<VentaHistorial>(ConexionBD.ObtenerHistorialVentas());
            HistorialView = CollectionViewSource.GetDefaultView(Historial);

            ActualizarTotales();
        }

        private void ItemVenta_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ItemVenta.Cantidad))
                ActualizarTotales();
        }

        public int TotalProductos => Productos.Sum(p => p.Cantidad);
        public int TotalGlobos => Globos.Sum(g => g.Cantidad);
        public decimal ImporteTotal => Productos.Sum(p => p.Importe) + Globos.Sum(g => g.Importe);

        private void ActualizarTotales()
        {
            OnPropertyChanged(nameof(TotalProductos));
            OnPropertyChanged(nameof(TotalGlobos));
            OnPropertyChanged(nameof(ImporteTotal));
        }

        public void Incrementar(ItemVenta item)
        {
            if (item.Cantidad < item.Stock) item.Cantidad++;
        }

        public void Decrementar(ItemVenta item)
        {
            if (item.Cantidad > 0) item.Cantidad--;
        }

        public void FiltrarHistorial(Cliente cliente = null, DateTime? desde = null, DateTime? hasta = null)
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
    }
}
