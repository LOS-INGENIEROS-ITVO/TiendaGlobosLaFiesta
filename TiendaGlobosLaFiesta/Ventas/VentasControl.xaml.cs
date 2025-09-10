using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClosedXML.Excel;
using Microsoft.Win32;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TiendaGlobosLaFiesta.Clientes;
using TiendaGlobosLaFiesta.Data;
using TiendaGlobosLaFiesta.Models;
using TiendaGlobosLaFiesta.Services;
using TiendaGlobosLaFiesta.ViewModels;

namespace TiendaGlobosLaFiesta
{
    public partial class VentasControl : UserControl
    {
        public ModeloDeVistaVentas VM { get; set; }

        // Evento para notificar que se realizó una venta
        public event Action VentaRealizada;

        // Datos filtrados del historial
        private IEnumerable<VentaHistorial> historialFiltrado =>
            VM.HistorialView?.Cast<VentaHistorial>() ?? Enumerable.Empty<VentaHistorial>();

        public VentasControl()
        {
            InitializeComponent();
            VM = new ModeloDeVistaVentas();
            DataContext = VM;

            // Bindings de combos y datagrids
            cmbClientes.ItemsSource = VM.Clientes;
            cmbFiltroCliente.ItemsSource = VM.Clientes;
            dgProductos.ItemsSource = VM.Productos;
            dgGlobos.ItemsSource = VM.Globos;
            dgHistorial.ItemsSource = VM.HistorialView;

            // Suscripción a cambios de cantidad para actualizar totales
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

        // =========================
        // Incrementar / Decrementar usando botones
        // =========================
        private void BtnAumentarCantidad_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ItemVenta item)
            {
                item.Incrementar();
            }
        }

        private void BtnDisminuirCantidad_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ItemVenta item)
            {
                item.Decrementar();
            }
        }

        // =========================
        // Registrar Venta
        // =========================
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

            if (!GestorDeVentas.ValidarStock(itemsVenta, out string mensaje))
            {
                MessageBox.Show(mensaje, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var venta = new Venta
            {
                VentaId = ConexionBD.ObtenerSiguienteVentaId(),
                ClienteId = cliente.ClienteId,
                EmpleadoId = SesionActual.EmpleadoId,
                FechaVenta = DateTime.Now,
                Productos = new System.Collections.ObjectModel.ObservableCollection<ProductoVenta>(productosVenta),
                Globos = new System.Collections.ObjectModel.ObservableCollection<GloboVenta>(globosVenta),
                ImporteTotal = VM.ImporteTotal
            };

            if (ConexionBD.RegistrarVenta(venta))
            {
                GestorDeVentas.ActualizarStock(itemsVenta);
                var vh = GestorDeVentas.CrearHistorial(venta, cliente, SesionActual.NombreEmpleadoCompleto);

                VM.Historial.Insert(0, vh);
                VM.HistorialView.Refresh();

                ActualizarTotales();
                MessageBox.Show("Venta registrada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                // ⚡ Notificar al Dashboard que se realizó una venta
                VentaRealizada?.Invoke();
            }
        }

        // =========================
        // Filtrar / Limpiar Historial
        // =========================
        private void BtnFiltrarHistorial_Click(object sender, RoutedEventArgs e)
        {
            VM.FiltrarHistorial(
                cmbFiltroCliente.SelectedItem as Cliente,
                dpFechaDesde.SelectedDate,
                dpFechaHasta.SelectedDate
            );
        }

        private void BtnLimpiarFiltros_Click(object sender, RoutedEventArgs e)
        {
            cmbFiltroCliente.SelectedIndex = -1;
            dpFechaDesde.SelectedDate = null;
            dpFechaHasta.SelectedDate = null;
            VM.LimpiarFiltros();
        }

        // =========================
        // Exportar Excel
        // =========================
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

        // =========================
        // Exportar PDF
        // =========================
        private void BtnExportarPDF_Click(object sender, RoutedEventArgs e)
        {
            GeneradorDePDF.GenerarPDF(historialFiltrado);
        }

        private void BtnMostrarGrafica_Click(object sender, RoutedEventArgs e)
        {
            VM.ActualizarGrafica(historialFiltrado);

            // Abrir ventana con la gráfica
            var ventana = new VentasGraficaWindow(VM);
            ventana.ShowDialog();
        }
    }

    // =========================
    // Extensión para NombreCompleto
    // =========================
    public static class ClienteExtensions
    {
        public static string NombreCompleto(this Cliente c)
        {
            return $"{c.PrimerNombre} {c.SegundoNombre} {c.ApellidoP} {c.ApellidoM}".Trim();
        }
    }
}