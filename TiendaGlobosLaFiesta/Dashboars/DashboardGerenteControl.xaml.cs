using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Wpf;
using TiendaGlobosLaFiesta.Data;
using TiendaGlobosLaFiesta.Modelos;

namespace TiendaGlobosLaFiesta.Views
{
    public partial class DashboardGerenteControl : UserControl, INotifyPropertyChanged
    {
        private readonly DashboardRepository _dashboardRepo = new DashboardRepository();

        private SeriesCollection _seriesCollection;
        public SeriesCollection SeriesCollection
        {
            get => _seriesCollection;
            set { _seriesCollection = value; OnPropertyChanged(); }
        }

        private string[] _labelsGrafica;
        public string[] LabelsGrafica
        {
            get => _labelsGrafica;
            set { _labelsGrafica = value; OnPropertyChanged(); }
        }

        public Func<double, string> Formatter { get; set; }

        public DashboardGerenteControl()
        {
            InitializeComponent();

            SeriesCollection = new SeriesCollection();
            LabelsGrafica = Array.Empty<string>();
            Formatter = value => value.ToString("C0");

            DataContext = this;
            Loaded += DashboardGerenteControl_Loaded;
        }

        private void DashboardGerenteControl_Loaded(object sender, RoutedEventArgs e)
        {
            CargarKPIs();
        }

        public void CargarKPIs()
        {
            try
            {
                var kpis = _dashboardRepo.ObtenerDatosDashboard();

                txtVentasHoy.Text = kpis.VentasHoy.ToString("C2");
                txtVentas7Dias.Text = kpis.Ventas7Dias.ToString("C2");
                txtVentasMes.Text = kpis.VentasMes.ToString("C2");
                txtTicketPromedioHoy.Text = kpis.TicketPromedioHoy.ToString("C2");
                txtStockCriticoNumero.Text = $"{kpis.TotalStockCritico} productos críticos";
                txtStockCriticoLista.Text = kpis.NombresStockCritico.Any() ? string.Join(", ", kpis.NombresStockCritico) : "Ninguno";
                txtClientesFrecuentesNumero.Text = $"{kpis.TotalClientesFrecuentes} clientes frecuentes";
                txtClientesFrecuentesNombres.Text = kpis.NombresClientesFrecuentes.Any() ? string.Join(", ", kpis.NombresClientesFrecuentes) : "Ninguno";

                if (!string.IsNullOrEmpty(kpis.NombreTopCliente))
                {
                    txtTopClienteMesNombre.Text = kpis.NombreTopCliente;
                    txtTopClienteMesTotal.Text = kpis.TotalTopCliente.ToString("C2");
                }
                else
                {
                    txtTopClienteMesNombre.Text = "N/A";
                    txtTopClienteMesTotal.Text = "$0.00";
                }

                txtProductoMasVendidoDia.Text = $"Hoy: {kpis.ProductoMasVendidoDia ?? "N/A"}";
                txtProductoMasVendidoSemana.Text = $"Semana: {kpis.ProductoMasVendidoSemana ?? "N/A"}";
                txtProductoMasVendidoMes.Text = $"Mes: {kpis.ProductoMasVendidoMes ?? "N/A"}";

                CargarGraficaVentas(kpis.VentasDiarias7Dias);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar KPIs: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarGraficaVentas(Dictionary<DateTime, decimal> ventas)
        {
            var ultimos7dias = Enumerable.Range(0, 7)
                                        .Select(i => DateTime.Today.AddDays(-i))
                                        .OrderBy(d => d.Date)
                                        .ToList();

            var valores = ultimos7dias.Select(d => ventas.ContainsKey(d.Date) ? ventas[d.Date] : 0).ToList();

            SeriesCollection = new SeriesCollection
{
    new ColumnSeries
    {
        Title = "Ventas",
        Values = new ChartValues<decimal>(valores),
        DataLabels = true,
        Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#17a2b8"))
    }
};

            LabelsGrafica = ultimos7dias.Select(d => d.ToString("ddd d")).ToArray();
        }

        public void RefrescarKPIs() => CargarKPIs();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}