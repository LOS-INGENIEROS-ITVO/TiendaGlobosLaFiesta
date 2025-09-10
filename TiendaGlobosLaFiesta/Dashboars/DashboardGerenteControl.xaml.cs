using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LiveCharts;
using LiveCharts.Wpf;
using TiendaGlobosLaFiesta.Data;

namespace TiendaGlobosLaFiesta.Views
{
    public partial class DashboardGerenteControl : UserControl
    {
        private readonly VentasRepository _ventasRepo = new VentasRepository();
        private readonly StockRepository _stockRepo = new StockRepository();
        private readonly ClienteRepository _clienteRepo = new ClienteRepository();
        private readonly ProductoRepository _productoRepo = new ProductoRepository();

        public DashboardGerenteControl()
        {
            InitializeComponent();
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
                CargarKPIsVentas();
                CargarKPIStockCritico();
                CargarKPIClientesFrecuentes();
                CargarKPITopCliente();
                CargarKPIProductoMasVendido();
                CargarGraficaVentas();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar KPIs: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region KPIs Ventas
        private void CargarKPIsVentas()
        {
            try
            {
                txtVentasHoy.Text = _ventasRepo.ObtenerVentasHoy().ToString("C2");
                txtVentas7Dias.Text = _ventasRepo.ObtenerVentasUltimos7DiasTotal().ToString("C2");
                txtVentasMes.Text = _ventasRepo.ObtenerVentasMes().ToString("C2");
                txtTicketPromedioHoy.Text = _ventasRepo.ObtenerTicketPromedioHoy().ToString("C2");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en KPIs ventas: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region KPI Stock Crítico
        private void CargarKPIStockCritico()
        {
            try
            {
                var productosStockCritico = _stockRepo.ObtenerProductosStockCritico();
                txtStockCriticoNumero.Text = $"{productosStockCritico.Count} productos críticos";
                txtStockCriticoLista.Text = productosStockCritico.Any()
                    ? string.Join(", ", productosStockCritico.Take(3).Select(p => $"{p.Nombre} ({p.Stock})")) +
                      (productosStockCritico.Count > 3 ? $" +{productosStockCritico.Count - 3} más" : "")
                    : "-";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en KPI Stock: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region KPI Clientes Frecuentes
        private void CargarKPIClientesFrecuentes()
        {
            try
            {
                var clientes = _clienteRepo.ObtenerClientesFrecuentesDetalle();
                txtClientesFrecuentesNumero.Text = $"{clientes.Count} clientes";
                txtClientesFrecuentesNombres.Text = clientes.Any()
                    ? string.Join(", ", clientes.Take(3).Select(c => c.NombreCompleto())) +
                      (clientes.Count > 3 ? $" +{clientes.Count - 3} más" : "")
                    : "-";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en KPI Clientes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region KPI Top Cliente Mes
        private void CargarKPITopCliente()
        {
            try
            {
                var topCliente = _clienteRepo.ObtenerTopClienteMes();
                if (topCliente != null)
                {
                    txtTopClienteMesNombre.Text = topCliente.Nombre;
                    txtTopClienteMesTotal.Text = topCliente.Total.ToString("C2");
                    txtTopClienteMesMes.Text = $"Mes actual: {DateTime.Today:MMMM}";
                }
                else
                {
                    txtTopClienteMesNombre.Text = "-";
                    txtTopClienteMesTotal.Text = "$0.00";
                    txtTopClienteMesMes.Text = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en KPI Top Cliente: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region KPI Producto Más Vendido
        private void CargarKPIProductoMasVendido()
        {
            try
            {
                var prodDia = _productoRepo.ObtenerProductoMasVendido("DIA");
                var prodSemana = _productoRepo.ObtenerProductoMasVendido("SEMANA");
                var prodMes = _productoRepo.ObtenerProductoMasVendido("MES");

                txtProductoMasVendidoDia.Text = $"Hoy: {prodDia?.Nombre ?? "-"} ({prodDia?.VentasHoy ?? 0})";
                txtProductoMasVendidoSemana.Text = $"Semana: {prodSemana?.Nombre ?? "-"} ({prodSemana?.VentasHoy ?? 0})";
                txtProductoMasVendidoMes.Text = $"Mes: {prodMes?.Nombre ?? "-"} ({prodMes?.VentasHoy ?? 0})";

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error en KPI Producto más vendido: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        #region Gráfica Ventas
        private void CargarGraficaVentas()
        {
            try
            {
                var ventasDetalle = _ventasRepo.ObtenerVentasUltimos7DiasDetalle() ??
                                    Enumerable.Range(0, 7).Select(i => DateTime.Today.AddDays(-6 + i)).ToDictionary(d => d, d => 0m);

                var labels = ventasDetalle.Keys.Select(d => d.ToString("dd/MM")).ToArray();
                var valoresDouble = ventasDetalle.Values.Select(v => (double)v).ToList();

                // Ejes
                if (cartesianChartVentas.AxisX == null || cartesianChartVentas.AxisX.Count == 0)
                {
                    cartesianChartVentas.AxisX.Clear();
                    cartesianChartVentas.AxisX.Add(new Axis { Title = "Fecha", Labels = labels });
                }
                else
                {
                    cartesianChartVentas.AxisX[0].Labels = labels;
                }

                if (cartesianChartVentas.AxisY == null || cartesianChartVentas.AxisY.Count == 0)
                {
                    cartesianChartVentas.AxisY.Clear();
                    cartesianChartVentas.AxisY.Add(new Axis { Title = "Ventas", LabelFormatter = value => value.ToString("C2") });
                }
                else
                {
                    cartesianChartVentas.AxisY[0].LabelFormatter = value => value.ToString("C2");
                }

                // Series
                var column = new ColumnSeries
                {
                    Title = "Ventas",
                    Values = new ChartValues<double>(valoresDouble),
                    DataLabels = true,
                    LabelPoint = p => p.Y.ToString("C2"),
                    MaxColumnWidth = 48
                };
                cartesianChartVentas.Series = new SeriesCollection { column };

                cartesianChartVentas.Update(true, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar gráfica: {ex.Message}", "Error gráfica", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion

        public void RefrescarKPIs() => CargarKPIs();

        public void SuscribirEventoVenta(VentasControl ventasControl)
        {
            ventasControl.VentaRealizada += RefrescarKPIs;
        }
    }
}  