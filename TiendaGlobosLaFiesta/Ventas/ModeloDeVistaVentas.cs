using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using TiendaGlobosLaFiesta.Models;
using TiendaGlobosLaFiesta.Data;
using TiendaGlobosLaFiesta.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;

namespace TiendaGlobosLaFiesta.ViewModels
{
    public class ModeloDeVistaVentas : INotifyPropertyChanged
    {
        public ObservableCollection<ProductoVenta> Productos { get; set; } = new();
        public ObservableCollection<GloboVenta> Globos { get; set; } = new();
        public ObservableCollection<Cliente> Clientes { get; set; } = new();
        public ObservableCollection<VentaHistorial> Historial { get; set; } = new();

        public ICollectionView HistorialView { get; set; }

        // =========================
        // Para gráficas
        // =========================
        public ObservableCollection<ISeries> Series { get; set; } = new();
        public string[] Labels { get; set; } = Array.Empty<string>();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public ModeloDeVistaVentas()
        {
            CargarDatos();

            Productos.CollectionChanged += (_, _) => OnPropertyChanged(nameof(TotalProductos));
            Globos.CollectionChanged += (_, _) => OnPropertyChanged(nameof(TotalGlobos));
        }

        private void CargarDatos()
        {
            Clientes = new ObservableCollection<Cliente>(ConexionBD.ObtenerClientes());

            Productos = new ObservableCollection<ProductoVenta>(ConexionBD.ObtenerProductos());
            Globos = new ObservableCollection<GloboVenta>(ConexionBD.ObtenerGlobos());

            foreach (var p in Productos) p.PropertyChanged += (_, __) => OnPropertyChanged(nameof(ImporteTotal));
            foreach (var g in Globos) g.PropertyChanged += (_, __) => OnPropertyChanged(nameof(ImporteTotal));

            Historial = new ObservableCollection<VentaHistorial>(ConexionBD.ObtenerHistorialVentas());
            HistorialView = CollectionViewSource.GetDefaultView(Historial);
        }

        public int TotalProductos => Productos.Sum(p => p.Cantidad);
        public int TotalGlobos => Globos.Sum(g => g.Cantidad);
        public decimal ImporteTotal => Productos.Sum(p => p.Importe) + Globos.Sum(g => g.Importe);

        // =========================
        // Comandos para UI
        // =========================
        public ICommand IncrementarCommand => new RelayCommand<ItemVenta>(item => item.Incrementar());
        public ICommand DecrementarCommand => new RelayCommand<ItemVenta>(item => item.Decrementar());

        // =========================
        // Filtrar historial
        // =========================
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

        // =========================
        // Actualizar gráfica
        // =========================
        public void ActualizarGrafica(IEnumerable<VentaHistorial> historialFiltrado)
        {
            var datos = historialFiltrado
                .GroupBy(v => v.ClienteNombre)
                .Select(g => new
                {
                    Cliente = g.Key,
                    Total = g.Sum(x => (double)x.Total) // convertir a double
                })
                .OrderByDescending(x => x.Total)
                .ToList();

            Labels = datos.Select(d => d.Cliente).ToArray();

            Series.Clear();
            Series.Add(new ColumnSeries<double>
            {
                Values = datos.Select(d => d.Total).ToArray(),
                Name = "Ventas",
                DataLabelsPaint = new LiveChartsCore.SkiaSharpView.Painting.SolidColorPaint(SkiaSharp.SKColors.Black),
                DataLabelsSize = 14,
                // Ahora usamos directamente el valor
                DataLabelsFormatter = value => $"{value:C}"
            });

            OnPropertyChanged(nameof(Series));
            OnPropertyChanged(nameof(Labels));
        }
    }
}
