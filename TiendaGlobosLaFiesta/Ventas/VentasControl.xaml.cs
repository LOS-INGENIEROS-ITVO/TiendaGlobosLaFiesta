using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TiendaGlobosLaFiesta.Data;
using TiendaGlobosLaFiesta.Core;
using TiendaGlobosLaFiesta.Models.Ventas;
using TiendaGlobosLaFiesta.Models.Clientes;

namespace TiendaGlobosLaFiesta.Views
{
    public partial class VentasControl : UserControl, INotifyPropertyChanged
    {
        private readonly VentasRepository _ventasRepo = new VentasRepository();
        private readonly ProductoRepository _productoRepo = new ProductoRepository();
        private readonly GloboRepository _globoRepo = new GloboRepository();

        // --- Colecciones ---
        public ObservableCollection<Cliente> Clientes { get; set; } = new();
        public ObservableCollection<ProductoVenta> Productos { get; set; } = new();
        public ObservableCollection<GloboVenta> Globos { get; set; } = new();
        public ObservableCollection<VentaView> HistorialView { get; set; } = new();

        // --- Totales ---
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

            Loaded += VentasControl_Loaded;
        }

        private void VentasControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                CargarClientes();
                CargarProductos();
                CargarGlobos();
                CargarHistorial();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al cargar datos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #region Carga de Datos

        private void CargarClientes()
        {
            Clientes.Clear();
            var lista = _ventasRepo.ObtenerClientes();
            foreach (var c in lista) Clientes.Add(c);
        }

        private void CargarProductos()
        {
            Productos.Clear();
            var lista = _productoRepo.ObtenerProductosEnStock();
            foreach (var p in lista)
            {
                p.Cantidad = 0;
                Productos.Add(p);
            }
        }

        private void CargarGlobos()
        {
            Globos.Clear();
            var lista = _globoRepo.ObtenerGlobosEnStock();
            foreach (var g in lista)
            {
                g.Cantidad = 0;
                Globos.Add(g);
            }
        }

        private void CargarHistorial()
        {
            HistorialView.Clear();
            var ventas = _ventasRepo.ObtenerHistorialVentas();
            foreach (var v in ventas) HistorialView.Add(v);
        }

        #endregion

        #region Botones Cantidad

        private void BtnAumentarCantidad_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                dynamic item = btn.Tag;
                if (item.Cantidad < item.Stock)
                {
                    item.Cantidad++;
                    ActualizarTotales();
                }
                else
                {
                    MessageBox.Show("No hay suficiente stock", "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

        private void BtnDisminuirCantidad_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                dynamic item = btn.Tag;
                if (item.Cantidad > 0)
                {
                    item.Cantidad--;
                    ActualizarTotales();
                }
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

        private void BtnRegistrarVenta_Click(object sender, RoutedEventArgs e)
        {
            RegistrarVenta();
        }

        public void RegistrarVenta()
        {
            try
            {
                if (cmbClientes.SelectedItem == null)
                {
                    MessageBox.Show("Seleccione un cliente antes de registrar la venta.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var cliente = (Cliente)cmbClientes.SelectedItem;
                var productosSeleccionados = Productos.Where(p => p.Cantidad > 0).ToList();
                var globosSeleccionados = Globos.Where(g => g.Cantidad > 0).ToList();

                if (!productosSeleccionados.Any() && !globosSeleccionados.Any())
                {
                    MessageBox.Show("Debe seleccionar al menos un producto o globo para registrar la venta.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Verificación de stock
                foreach (var p in productosSeleccionados)
                    if (p.Cantidad > p.Stock)
                        throw new InvalidOperationException($"No hay suficiente stock del producto '{p.Nombre}'.");

                foreach (var g in globosSeleccionados)
                    if (g.Cantidad > g.Stock)
                        throw new InvalidOperationException($"No hay suficiente stock del globo '{g.Material} {g.Color}'.");

                // Registro de venta
                _ventasRepo.RegistrarVenta(cliente, productosSeleccionados, globosSeleccionados);

                MessageBox.Show("Venta registrada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                // Actualizar UI
                CargarProductos();
                CargarGlobos();
                CargarHistorial();
                ActualizarTotales();

                // Notificar al ModuloManager
                ModuloManager.Instancia.NotificarVentaRegistrada();
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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al refrescar lista de clientes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region Filtros Historial (Opcional)

        private void BtnFiltrarHistorial_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var cliente = cmbFiltroCliente.SelectedItem as Cliente;
                var desde = dpFechaDesde.SelectedDate;
                var hasta = dpFechaHasta.SelectedDate;

                var historial = _ventasRepo.FiltrarHistorial(cliente?.ClienteId, desde, hasta);
                HistorialView.Clear();
                foreach (var v in historial) HistorialView.Add(v);
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

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
}