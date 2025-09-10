using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClosedXML.Excel;
using Microsoft.Win32;
using TiendaGlobosLaFiesta.Data;
using TiendaGlobosLaFiesta.Modelos;
using TiendaGlobosLaFiesta.Models;
using TiendaGlobosLaFiesta.Services;
using TiendaGlobosLaFiesta.ViewModels;

namespace TiendaGlobosLaFiesta
{
    public partial class VentasControl : UserControl
    {
        public ModeloDeVistaVentas VM { get; set; }
        private readonly VentaService _ventaService = new();
        private readonly VentasRepository _ventasRepo = new();
        private readonly ProductoRepository _productoRepo = new();
        private readonly GloboRepository _globoRepo = new();

        public event Action VentaRealizada;

        private IEnumerable<VentaHistorial> historialFiltrado =>
            VM.HistorialView?.Cast<VentaHistorial>() ?? Enumerable.Empty<VentaHistorial>();

        public VentasControl()
        {
            InitializeComponent();
            VM = new ModeloDeVistaVentas();
            DataContext = VM;

            cmbClientes.ItemsSource = VM.Clientes;
            cmbFiltroCliente.ItemsSource = VM.Clientes;
            dgProductos.ItemsSource = VM.Productos;
            dgGlobos.ItemsSource = VM.Globos;
            dgHistorial.ItemsSource = VM.HistorialView;

            foreach (var p in VM.Productos) p.PropertyChanged += (_, __) => ActualizarTotales();
            foreach (var g in VM.Globos) g.PropertyChanged += (_, __) => ActualizarTotales();

            ActualizarTotales();
        }

        private void ActualizarTotales()
        {
            txtTotalProductos.Text = VM.TotalProductos.ToString();
            txtTotalGlobos.Text = VM.TotalGlobos.ToString();
            txtImporteTotal.Text = VM.ImporteTotal.ToString("C");
        }

        private void BtnAumentarCantidad_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ItemVenta item)
                item.Incrementar();
        }

        private void BtnDisminuirCantidad_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ItemVenta item)
                item.Decrementar();
        }

        private void BtnRegistrarVenta_Click(object sender, RoutedEventArgs e)
        {
            if (cmbClientes.SelectedItem is not Cliente cliente)
            {
                MessageBox.Show("Selecciona un cliente.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var productosSeleccionados = VM.Productos.Where(p => p.Cantidad > 0).ToList();
            var globosSeleccionados = VM.Globos.Where(g => g.Cantidad > 0).ToList();

            if (!productosSeleccionados.Any() && !globosSeleccionados.Any())
            {
                MessageBox.Show("Debes seleccionar al menos un producto o globo.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 🔹 Generar ID de venta seguro
            int ultimoNumero = _ventasRepo.ObtenerUltimoNumeroVenta();
            string ventaId = $"VEN{(ultimoNumero + 1):D4}";

            var venta = new Venta
            {
                VentaId = ventaId,
                ClienteId = cliente.ClienteId,
                EmpleadoId = SesionActual.EmpleadoId,
                FechaVenta = DateTime.Now,
                ImporteTotal = productosSeleccionados.Sum(p => p.Importe) + globosSeleccionados.Sum(g => g.Importe),
                Productos = new ObservableCollection<ProductoVenta>(productosSeleccionados),
                Globos = new ObservableCollection<GloboVenta>(globosSeleccionados)
            };

            if (RegistrarVentaConStock(venta))
            {
                // 🔹 Actualizar stock en la UI
                foreach (var p in venta.Productos)
                {
                    var prodVM = VM.Productos.FirstOrDefault(x => x.ProductoId == p.ProductoId);
                    if (prodVM != null) prodVM.Stock -= p.Cantidad;
                }
                foreach (var g in venta.Globos)
                {
                    var globoVM = VM.Globos.FirstOrDefault(x => x.GloboId == g.GloboId);
                    if (globoVM != null) globoVM.Stock -= g.Cantidad;
                }

                VM.CargarHistorial();
                VentaRealizada?.Invoke();
                LimpiarFormulario();
                ActualizarTotales();

                MessageBox.Show("Venta registrada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Ocurrió un error al registrar la venta.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void LimpiarFormulario()
        {
            foreach (var p in VM.Productos) p.Cantidad = 0;
            foreach (var g in VM.Globos) g.Cantidad = 0;
            cmbClientes.SelectedIndex = -1;
            ActualizarTotales();
        }

        public bool RegistrarVentaConStock(Venta venta)
        {
            using var conn = DbHelper.ObtenerConexion();
            using var tran = conn.BeginTransaction();

            try
            {
                // Insertar venta
                string queryVenta = @"INSERT INTO Venta (ventaId, clienteId, empleadoId, fechaVenta, importeTotal)
                              VALUES (@ventaId, @clienteId, @empleadoId, @fecha, @total)";
                using (var cmd = new SqlCommand(queryVenta, conn, tran))
                {
                    cmd.Parameters.AddWithValue("@ventaId", venta.VentaId);
                    cmd.Parameters.AddWithValue("@clienteId", venta.ClienteId);
                    cmd.Parameters.AddWithValue("@empleadoId", venta.EmpleadoId);
                    cmd.Parameters.AddWithValue("@fecha", venta.FechaVenta);
                    cmd.Parameters.AddWithValue("@total", venta.ImporteTotal);
                    cmd.ExecuteNonQuery();
                }

                // Insertar detalle y actualizar stock de productos
                foreach (var p in venta.Productos)
                {
                    string queryProd = @"INSERT INTO Detalle_Venta_Producto (ventaId, productoId, cantidad, costo, importe)
                                 VALUES (@ventaId, @productoId, @cantidad, @costo, @importe);
                                 UPDATE Producto SET stock = stock - @cantidad WHERE productoId = @productoId;";
                    using var cmd = new SqlCommand(queryProd, conn, tran);
                    cmd.Parameters.AddWithValue("@ventaId", venta.VentaId);
                    cmd.Parameters.AddWithValue("@productoId", p.ProductoId);
                    cmd.Parameters.AddWithValue("@cantidad", p.Cantidad);
                    cmd.Parameters.AddWithValue("@costo", p.Costo);
                    cmd.Parameters.AddWithValue("@importe", p.Importe);
                    cmd.ExecuteNonQuery();
                }

                // Insertar detalle y actualizar stock de globos
                foreach (var g in venta.Globos)
                {
                    string queryGlobo = @"INSERT INTO Detalle_Venta_Globo (ventaId, globoId, cantidad, costo, importe)
                                  VALUES (@ventaId, @globoId, @cantidad, @costo, @importe);
                                  UPDATE Globo SET stock = stock - @cantidad WHERE globoId = @globoId;";
                    using var cmd = new SqlCommand(queryGlobo, conn, tran);
                    cmd.Parameters.AddWithValue("@ventaId", venta.VentaId);
                    cmd.Parameters.AddWithValue("@globoId", g.GloboId);
                    cmd.Parameters.AddWithValue("@cantidad", g.Cantidad);
                    cmd.Parameters.AddWithValue("@costo", g.Costo);
                    cmd.Parameters.AddWithValue("@importe", g.Importe);
                    cmd.ExecuteNonQuery();
                }

                tran.Commit();
                return true;
            }
            catch
            {
                tran.Rollback();
                return false;
            }
        }


        private void BtnFiltrarHistorial_Click(object sender, RoutedEventArgs e)
        {
            VM.FiltrarHistorial(cmbFiltroCliente.SelectedItem as Cliente, dpFechaDesde.SelectedDate, dpFechaHasta.SelectedDate);
        }

        private void BtnLimpiarFiltros_Click(object sender, RoutedEventArgs e)
        {
            cmbFiltroCliente.SelectedIndex = -1;
            dpFechaDesde.SelectedDate = null;
            dpFechaHasta.SelectedDate = null;
            VM.LimpiarFiltros();
        }

        private void BtnExportarExcel_Click(object sender, RoutedEventArgs e)
        {
            if (!historialFiltrado.Any())
            {
                MessageBox.Show("No hay datos para exportar.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var sfd = new SaveFileDialog
            {
                Filter = "Archivos Excel (.xlsx)|.xlsx",
                FileName = "Ventas_Exportadas.xlsx"
            };
            if (sfd.ShowDialog() != true) return;

            using var workbook = new XLWorkbook();
            GeneradorDeExcel.CrearHojas(workbook, historialFiltrado);
            workbook.SaveAs(sfd.FileName);

            MessageBox.Show("Exportación a Excel completada.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo { FileName = sfd.FileName, UseShellExecute = true });
        }

        private void BtnExportarPDF_Click(object sender, RoutedEventArgs e)
        {
            GeneradorDePDF.GenerarPDF(historialFiltrado);
        }

        private void BtnMostrarGrafica_Click(object sender, RoutedEventArgs e)
        {
            VM.ActualizarGrafica(historialFiltrado);
            var ventana = new VentasGraficaWindow(VM);
            ventana.ShowDialog();
        }
    }
}