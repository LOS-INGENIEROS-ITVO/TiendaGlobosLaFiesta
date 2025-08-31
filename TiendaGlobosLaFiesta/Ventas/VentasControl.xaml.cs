using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TiendaGlobosLaFiesta.Data;
using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta
{
    public partial class VentasControl : UserControl
    {
        private ObservableCollection<ProductoVenta> productos;
        private ObservableCollection<GloboVenta> globos;
        private ObservableCollection<Cliente> clientes;
        private ObservableCollection<VentaHistorial> historial;

        public VentasControl()
        {
            InitializeComponent();
            CargarDatos();
        }

        private void CargarDatos()
        {
            // Clientes
            clientes = new ObservableCollection<Cliente>(VentaDAO.ObtenerClientes());
            cmbClientes.ItemsSource = clientes;
            cmbFiltroCliente.ItemsSource = clientes;

            // Productos
            productos = new ObservableCollection<ProductoVenta>(VentaDAO.ObtenerProductos());
            dgProductos.ItemsSource = productos;

            // Globos
            globos = new ObservableCollection<GloboVenta>(VentaDAO.ObtenerGlobos());
            dgGlobos.ItemsSource = globos;

            // Historial
            historial = new ObservableCollection<VentaHistorial>(VentaDAO.ObtenerHistorial());
            dgHistorial.ItemsSource = historial;

            ActualizarTotales();
        }

        // ==========================
        // Botones Cantidad
        // ==========================
        private void BtnAumentarCantidad_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                switch (btn.Tag)
                {
                    case ProductoVenta p:
                        if (p.Cantidad < p.Stock) p.Cantidad++;
                        break;
                    case GloboVenta g:
                        if (g.Cantidad < g.Stock) g.Cantidad++;
                        break;
                }
                ActualizarTotales();
            }
        }

        private void BtnDisminuirCantidad_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                switch (btn.Tag)
                {
                    case ProductoVenta p:
                        if (p.Cantidad > 0) p.Cantidad--;
                        break;
                    case GloboVenta g:
                        if (g.Cantidad > 0) g.Cantidad--;
                        break;
                }
                ActualizarTotales();
            }
        }

        // ==========================
        // Totales
        // ==========================
        private void ActualizarTotales()
        {
            txtTotalProductos.Text = productos.Sum(p => p.Cantidad).ToString();
            txtTotalGlobos.Text = globos.Sum(g => g.Cantidad).ToString();
            txtImporteTotal.Text = (productos.Sum(p => p.Importe) + globos.Sum(g => g.Importe)).ToString("C");
        }

        // ==========================
        // Registrar Venta
        // ==========================
        private void BtnRegistrarVenta_Click(object sender, RoutedEventArgs e)
        {
            if (cmbClientes.SelectedItem == null)
            {
                MessageBox.Show("Seleccione un cliente.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var productosVenta = productos.Where(p => p.Cantidad > 0).ToList();
            var globosVenta = globos.Where(g => g.Cantidad > 0).ToList();

            if (productosVenta.Count == 0 && globosVenta.Count == 0)
            {
                MessageBox.Show("Seleccione al menos un producto o globo.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validar stock
            foreach (var p in productosVenta)
            {
                if (p.Cantidad > p.Stock)
                {
                    MessageBox.Show($"No hay suficiente stock para el producto {p.Nombre}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            foreach (var g in globosVenta)
            {
                if (g.Cantidad > g.Stock)
                {
                    MessageBox.Show($"No hay suficiente stock para el globo {g.Material} - {g.Color}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            var venta = new Venta
            {
                VentaId = ConexionBD.ObtenerSiguienteVentaId(),
                ClienteId = ((Cliente)cmbClientes.SelectedItem).ClienteId,
                EmpleadoId = SesionActual.EmpleadoId,
                FechaVenta = DateTime.Now,
                Productos = new ObservableCollection<ProductoVenta>(productosVenta),
                Globos = new ObservableCollection<GloboVenta>(globosVenta)
            };
            venta.ImporteTotal = venta.Productos.Sum(p => p.Importe) + venta.Globos.Sum(g => g.Importe);

            if (ConexionBD.RegistrarVenta(venta))
            {
                MessageBox.Show("Venta registrada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                // Actualizar stock localmente
                foreach (var p in productosVenta)
                {
                    var prod = productos.First(x => x.ProductoId == p.ProductoId);
                    prod.Stock -= p.Cantidad;
                    prod.Cantidad = 0;
                }

                foreach (var g in globosVenta)
                {
                    var glob = globos.First(x => x.GloboId == g.GloboId);
                    glob.Stock -= g.Cantidad;
                    glob.Cantidad = 0;
                }

                ActualizarTotales();

                // Refrescar historial
                historial = new ObservableCollection<VentaHistorial>(VentaDAO.ObtenerHistorial());
                dgHistorial.ItemsSource = historial;
            }
        }

        // ==========================
        // Filtrar Historial
        // ==========================
        private void BtnFiltrarHistorial_Click(object sender, RoutedEventArgs e)
        {
            var lista = VentaDAO.ObtenerHistorial();
            if (cmbFiltroCliente.SelectedItem is Cliente cliente)
                lista = lista.FindAll(v => v.Cliente.Contains(cliente.Nombre));
            if (dpFechaDesde.SelectedDate.HasValue)
                lista = lista.FindAll(v => v.Fecha >= dpFechaDesde.SelectedDate.Value);
            if (dpFechaHasta.SelectedDate.HasValue)
                lista = lista.FindAll(v => v.Fecha <= dpFechaHasta.SelectedDate.Value);

            historial = new ObservableCollection<VentaHistorial>(lista);
            dgHistorial.ItemsSource = historial;
        }

        // ==========================
        // Limpiar filtros
        // ==========================
        private void BtnLimpiarFiltros_Click(object sender, RoutedEventArgs e)
        {
            cmbFiltroCliente.SelectedIndex = -1;
            dpFechaDesde.SelectedDate = null;
            dpFechaHasta.SelectedDate = null;

            historial = new ObservableCollection<VentaHistorial>(VentaDAO.ObtenerHistorial());
            dgHistorial.ItemsSource = historial;
        }
    }
}
