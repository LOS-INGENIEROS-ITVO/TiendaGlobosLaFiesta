using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using TiendaGlobosLaFiesta.Core;
using TiendaGlobosLaFiesta.Data;
using TiendaGlobosLaFiesta.Models.Ventas;
using TiendaGlobosLaFiesta.Models.Clientes;
using TiendaGlobosLaFiesta.Models.Inventario;
using TiendaGlobosLaFiesta.Services;

namespace TiendaGlobosLaFiesta.Views
{
    public partial class VentasControl : UserControl, INotifyPropertyChanged
    {
        private readonly VentaService _ventaService = new();
        private readonly ProductoRepository _productoRepo = new();
        private readonly GloboRepository _globoRepo = new();

        public ObservableCollection<Cliente> Clientes { get; set; } = new();
        public ObservableCollection<ProductoVenta> Productos { get; set; } = new();
        public ObservableCollection<GloboVenta> Globos { get; set; } = new();
        public ObservableCollection<VentaHistorial> HistorialView { get; set; } = new();

        private readonly CollectionViewSource _productosView = new();
        private readonly CollectionViewSource _globosView = new();

        private int _totalProductos;
        public int TotalProductos { get => _totalProductos; set { _totalProductos = value; OnPropertyChanged(); } }

        private int _totalGlobos;
        public int TotalGlobos { get => _totalGlobos; set { _totalGlobos = value; OnPropertyChanged(); } }

        private decimal _importeTotal;
        public decimal ImporteTotal { get => _importeTotal; set { _importeTotal = value; OnPropertyChanged(); } }

        public VentasControl()
        {
            InitializeComponent();
            DataContext = this;

            // Registrar módulo
            ModuloManager.Instancia.RegistrarModulo("Ventas", this);

            Loaded += VentasControl_Loaded;

            // Suscribir a eventos de stock actualizado
            ModuloManager.Instancia.StockActualizado += async () => await Dispatcher.InvokeAsync(ActualizarStockAsync);
        }

        private void VentasControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Inicializar CollectionViewSources antes de asignar filtros
                _productosView.Source = Productos;
                _productosView.Filter += ProductosFilter;
                dgProductos.ItemsSource = _productosView.View;

                _globosView.Source = Globos;
                _globosView.Filter += GlobosFilter;
                dgGlobos.ItemsSource = _globosView.View;

                // Inicializar TextBox
                txtBuscarProducto.Text = "";
                txtBuscarGlobo.Text = "";

                // Cargar datos
                CargarClientes();
                CargarProductos();
                CargarGlobos();
                CargarHistorial();

                ActualizarTotales();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar módulo de ventas: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Carga de Datos

        private void CargarClientes()
        {
            Clientes.Clear();
            foreach (var c in _ventaService.ObtenerClientes())
                Clientes.Add(c);
        }

        private void CargarProductos()
        {
            Productos.Clear();
            foreach (var p in _productoRepo.ObtenerProductosEnStock())
            {
                Productos.Add(new ProductoVenta
                {
                    ProductoId = p.ProductoId,
                    NombreProducto = p.Nombre, // ✅ usar la propiedad subyacente
                    Unidad = p.Unidad,
                    Stock = p.Stock,
                    Costo = p.Costo,
                    Cantidad = 0
                });
            }
        }

        private void CargarGlobos()
        {
            Globos.Clear();
            foreach (var g in _globoRepo.ObtenerGlobosEnStock())
            {
                Globos.Add(new GloboVenta
                {
                    GloboId = g.GloboId,
                    Material = g.Material,
                    Color = g.Color,
                    Tamano = g.Tamano,
                    Forma = g.Forma,
                    Tematica = g.Tematica,
                    Unidad = g.Unidad,
                    Stock = g.Stock,
                    Costo = g.Costo,
                    Cantidad = 0
                });
            }
        }


        private void CargarHistorial()
        {
            HistorialView.Clear();
            foreach (var v in _ventaService.ObtenerHistorialVentas())
                HistorialView.Add(v);
        }

        #endregion

        #region Búsqueda Dinámica

        private void TxtBuscarProducto_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_productosView.View != null)
                _productosView.View.Refresh();
        }

        private void TxtBuscarGlobo_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_globosView.View != null)
                _globosView.View.Refresh();
        }

        private void ProductosFilter(object sender, FilterEventArgs e)
        {
            if (e.Item is ProductoVenta p)
            {
                string texto = txtBuscarProducto?.Text?.Trim().ToLower() ?? "";
                e.Accepted = string.IsNullOrEmpty(texto) || p.Nombre.ToLower().Contains(texto);
            }
        }

        private void GlobosFilter(object sender, FilterEventArgs e)
        {
            if (e.Item is GloboVenta g)
            {
                string texto = txtBuscarGlobo?.Text?.Trim().ToLower() ?? "";
                e.Accepted = string.IsNullOrEmpty(texto) ||
                             g.Material.ToLower().Contains(texto) ||
                             g.Color.ToLower().Contains(texto) ||
                             (g.Tamano?.ToLower().Contains(texto) ?? false) ||
                             (g.Forma?.ToLower().Contains(texto) ?? false) ||
                             (g.Tematica?.ToLower().Contains(texto) ?? false);
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
                tb.Text = "";
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb && string.IsNullOrWhiteSpace(tb.Text))
                tb.Text = "";
        }

        #endregion

        #region Botones Cantidad

        private void BtnAumentarCantidad_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                if (btn.Tag is ProductoVenta producto && producto.Cantidad < producto.Stock)
                    producto.Cantidad++;
                else if (btn.Tag is GloboVenta globo && globo.Cantidad < globo.Stock)
                    globo.Cantidad++;

                ActualizarTotales();
            }
        }

        private void BtnDisminuirCantidad_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                if (btn.Tag is ProductoVenta producto && producto.Cantidad > 0)
                    producto.Cantidad--;
                else if (btn.Tag is GloboVenta globo && globo.Cantidad > 0)
                    globo.Cantidad--;

                ActualizarTotales();
            }
        }

        private void Cantidad_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !int.TryParse(e.Text, out _);
        }

        private void ActualizarTotales()
        {
            TotalProductos = Productos.Sum(p => p.Cantidad);
            TotalGlobos = Globos.Sum(g => g.Cantidad);
            ImporteTotal = Productos.Sum(p => p.Importe) + Globos.Sum(g => g.Importe);
        }

        #endregion

        #region Registro de Venta

        private void BtnRegistrarVenta_Click(object sender, RoutedEventArgs e) => RegistrarVenta();

        public void RegistrarVenta()
        {
            try
            {
                if (cmbClientes.SelectedItem is not Cliente cliente)
                {
                    MessageBox.Show("Seleccione un cliente antes de registrar la venta.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var productosSeleccionados = Productos.Where(p => p.Cantidad > 0).ToList();
                var globosSeleccionados = Globos.Where(g => g.Cantidad > 0).ToList();

                if (!productosSeleccionados.Any() && !globosSeleccionados.Any())
                {
                    MessageBox.Show("Debe seleccionar al menos un producto o globo para registrar la venta.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var venta = new Venta
                {
                    ClienteId = cliente.ClienteId,
                    Productos = new ObservableCollection<ProductoVenta>(productosSeleccionados),
                    Globos = new ObservableCollection<GloboVenta>(globosSeleccionados),
                    VentaId = Guid.NewGuid().ToString(),
                    FechaVenta = DateTime.Now,
                    Estatus = "Completada",
                    ImporteTotal = productosSeleccionados.Sum(p => p.Importe) + globosSeleccionados.Sum(g => g.Importe)
                };

                if (_ventaService.RegistrarVentaCompleta(venta, out string mensajeError))
                {
                    MessageBox.Show("Venta registrada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                    foreach (var p in productosSeleccionados)
                    {
                        var original = Productos.FirstOrDefault(x => x.ProductoId == p.ProductoId);
                        if (original != null)
                        {
                            original.Stock -= p.Cantidad;
                            original.Cantidad = 0;
                        }
                    }

                    foreach (var g in globosSeleccionados)
                    {
                        var original = Globos.FirstOrDefault(x => x.GloboId == g.GloboId);
                        if (original != null)
                        {
                            original.Stock -= g.Cantidad;
                            original.Cantidad = 0;
                        }
                    }

                    ActualizarTotales();
                    CargarHistorial();
                    ModuloManager.Instancia.NotificarVentaRegistrada();
                }
                else
                {
                    MessageBox.Show($"No se pudo registrar la venta: {mensajeError}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al registrar la venta: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Refrescar Cliente

        public void RefrescarListaClientes()
        {
            try
            {
                CargarClientes();
                ModuloManager.Instancia.NotificarClienteAgregado();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al refrescar lista de clientes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Filtros Historial

        private void BtnFiltrarHistorial_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var clienteId = cmbFiltroCliente.SelectedValue as string;
                var desde = dpFechaDesde.SelectedDate;
                var hasta = dpFechaHasta.SelectedDate;

                var historial = _ventaService.FiltrarHistorial(clienteId, desde, hasta);

                HistorialView.Clear();
                foreach (var v in historial)
                    HistorialView.Add(v);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al filtrar historial: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnLimpiarFiltros_Click(object sender, RoutedEventArgs e)
        {
            cmbFiltroCliente.SelectedIndex = -1;
            dpFechaDesde.SelectedDate = null;
            dpFechaHasta.SelectedDate = null;
            CargarHistorial();
        }

        #endregion

        #region Exportar y Gráficas

        private void BtnExportarPDF_Click(object sender, RoutedEventArgs e)
        {
            GeneradorDePDF.GenerarPDF(HistorialView);
        }

        private void BtnExportarExcel_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Archivos Excel (*.xlsx)|*.xlsx",
                FileName = "Ventas_Exportadas.xlsx"
            };

            if (sfd.ShowDialog() != true) return;

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            GeneradorDeExcel.CrearHojas(workbook, HistorialView);
            workbook.SaveAs(sfd.FileName);

            MessageBox.Show("Exportación a Excel completada.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnMostrarGrafica_Click(object sender, RoutedEventArgs e)
        {
            var graficaWindow = new VentasGraficaWindow(HistorialView);
            graficaWindow.ShowDialog();
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        #endregion

        #region Stock Actualizado

        private async System.Threading.Tasks.Task ActualizarStockAsync()
        {
            CargarProductos();
            CargarGlobos();
            ActualizarTotales();

            // Refrescar CollectionViews
            _productosView.View?.Refresh();
            _globosView.View?.Refresh();
        }

        #endregion
    }
}