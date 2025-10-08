using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using TiendaGlobosLaFiesta.Data;
using TiendaGlobosLaFiesta.Models.Utilities; // Asegúrate que SesionActual esté aquí

namespace TiendaGlobosLaFiesta.Views
{
    public partial class DashboardEmpleadoControl : UserControl, INotifyPropertyChanged
    {
        private readonly DashboardRepository _dashboardRepo = new DashboardRepository();

        public SeriesCollection SeriesCollection { get; set; }
        public string[] LabelsGrafica { get; set; }
        public Func<double, string> Formatter { get; set; }

        public DashboardEmpleadoControl()
        {
            InitializeComponent();
            SeriesCollection = new SeriesCollection();
            LabelsGrafica = Array.Empty<string>();
            Formatter = value => value.ToString("C0");
            DataContext = this;
            Loaded += DashboardEmpleadoControl_Loaded;
        }

        private void DashboardEmpleadoControl_Loaded(object sender, RoutedEventArgs e)
        {
            CargarKPIs();
        }

        public void CargarKPIs()
        {
            try
            {
                // 1. Verificamos que el EmpleadoId de la sesión exista (no sea nulo).
                if (!SesionActual.EmpleadoId.HasValue)
                {
                    MessageBox.Show("No se pudo identificar al empleado actual.", "Error de Sesión", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return; // Salimos del método si no hay un empleado válido.
                }

                // 2. CORRECCIÓN: Usamos el nombre correcto "EmpleadoId" y accedemos a su valor.
                int idEmpleadoLogueado = SesionActual.EmpleadoId.Value;
                var kpis = _dashboardRepo.ObtenerDatosDashboardEmpleado(idEmpleadoLogueado);

                txtMisVentasHoy.Text = kpis.MisVentasHoy.ToString("C2");
                txtTicketsHoy.Text = kpis.TicketsHoy.ToString();
                txtTicketPromedio.Text = kpis.TicketPromedioHoy.ToString("C2");
                txtClientesAtendidos.Text = kpis.ClientesAtendidosHoy.ToString();

                CargarGraficaVentas(kpis.VentasDiarias7Dias);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar el dashboard: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarGraficaVentas(Dictionary<DateTime, decimal> ventas)
        {
            var ultimos7dias = Enumerable.Range(0, 7).Select(i => DateTime.Today.AddDays(-i)).OrderBy(d => d.Date).ToList();
            var valores = ultimos7dias.Select(d => ventas.ContainsKey(d.Date) ? ventas[d.Date] : 0).ToList();

            SeriesCollection.Clear();
            SeriesCollection.Add(new ColumnSeries
            {
                Title = "Ventas",
                Values = new ChartValues<decimal>(valores),
                DataLabels = true,
                Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#28a745"))
            });

            LabelsGrafica = ultimos7dias.Select(d => d.ToString("ddd d")).ToArray();

            OnPropertyChanged(nameof(SeriesCollection));
            OnPropertyChanged(nameof(LabelsGrafica));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}