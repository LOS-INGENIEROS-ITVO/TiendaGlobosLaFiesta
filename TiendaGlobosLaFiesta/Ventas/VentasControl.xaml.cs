using System;
using System.Collections.Generic;
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
                MessageBox.Show("Seleccione un cliente.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var productosVenta = VM.Productos.Where(p => p.Cantidad > 0).ToList();
            var globosVenta = VM.Globos.Where(g => g.Cantidad > 0).ToList();
            var itemsVenta = productosVenta.Cast<ItemVenta>().Concat(globosVenta);

            if (!itemsVenta.Any())
            {
                MessageBox.Show("Seleccione al menos un producto o globo.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!_ventaService.ValidarStock(itemsVenta, out string mensaje))
            {
                MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var venta = new Venta
            {
                VentaId = Guid.NewGuid().ToString(),
                ClienteId = cliente.ClienteId,
                EmpleadoId = SesionActual.EmpleadoId,
                FechaVenta = DateTime.Now,
                Productos = new System.Collections.ObjectModel.ObservableCollection<ProductoVenta>(productosVenta),
                Globos = new System.Collections.ObjectModel.ObservableCollection<GloboVenta>(globosVenta),
                ImporteTotal = VM.ImporteTotal
            };

            if (_ventasRepo.RegistrarVenta(venta))
            {
                _ventaService.ActualizarStock(itemsVenta);
                var vh = _ventaService.CrearHistorial(venta, cliente, SesionActual.NombreEmpleadoCompleto);

                VM.Historial.Insert(0, vh);
                VM.HistorialView.Refresh();

                ActualizarTotales();
                MessageBox.Show("Venta registrada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                VentaRealizada?.Invoke();
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