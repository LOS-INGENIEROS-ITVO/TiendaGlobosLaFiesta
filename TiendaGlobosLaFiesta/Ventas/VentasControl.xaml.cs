using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private readonly VentaService _ventaService = new(); // Se usa para la lógica de negocio.

        public event Action VentaRealizada;

        private IEnumerable<VentaHistorial> historialFiltrado =>
            VM.HistorialView?.Cast<VentaHistorial>() ?? Enumerable.Empty<VentaHistorial>();

        public VentasControl()
        {
            InitializeComponent();
            VM = new ModeloDeVistaVentas();
            DataContext = VM;

            // Enlazado de datos inicial
            cmbClientes.ItemsSource = VM.Clientes;
            cmbFiltroCliente.ItemsSource = VM.Clientes;
            dgProductos.ItemsSource = VM.Productos;
            dgGlobos.ItemsSource = VM.Globos;
            dgHistorial.ItemsSource = VM.HistorialView;

            // Suscripción a eventos para actualizar totales
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


        // En el archivo: VentasControl.cs

        private void BtnRegistrarVenta_Click(object sender, RoutedEventArgs e)
        {
            // --- 1. Validaciones de la UI ---
            if (cmbClientes.SelectedItem is not Cliente cliente)
            {
                MessageBox.Show("Selecciona un cliente.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var productosSeleccionados = VM.Productos.Where(p => p.Cantidad > 0).ToList();
            var globosSeleccionados = VM.Globos.Where(g => g.Cantidad > 0).ToList();

            if (!productosSeleccionados.Any() && !globosSeleccionados.Any())
            {
                MessageBox.Show("Debes seleccionar al menos un producto o globo.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // --- 2. Crear el objeto Venta ---
            var venta = new Venta
            {
                VentaId = $"VEN{DateTime.Now:yyMMddHHmmss}",
                ClienteId = cliente.ClienteId,
                EmpleadoId = SesionActual.EmpleadoId,
                FechaVenta = DateTime.Now,
                ImporteTotal = VM.ImporteTotal,
                Productos = new ObservableCollection<ProductoVenta>(productosSeleccionados),
                Globos = new ObservableCollection<GloboVenta>(globosSeleccionados)
            };

            // --- 3. Delegar toda la lógica al VentaService ---
            if (_ventaService.RegistrarVentaCompleta(venta, out string mensajeError))
            {
                // --- 4. Actualizar la UI si la operación fue exitosa ---

                // 🔹 CÓDIGO RESTAURADO PARA ACTUALIZAR EL STOCK VISUAL 🔹
                // Se actualiza el stock en la colección del ViewModel para que la UI lo refleje inmediatamente.
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

                VM.CargarHistorial();    // Recargar el historial de ventas
                VentaRealizada?.Invoke(); // Notificar al dashboard
                LimpiarFormulario();      // Limpiar el carrito de compras

                MessageBox.Show("Venta registrada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Mostrar el mensaje de error específico que devolvió el servicio
                MessageBox.Show($"No se pudo registrar la venta:\n{mensajeError}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void LimpiarFormulario()
        {
            foreach (var p in VM.Productos) p.Cantidad = 0;
            foreach (var g in VM.Globos) g.Cantidad = 0;
            cmbClientes.SelectedIndex = -1;
            ActualizarTotales();
        }

        // --- El método "RegistrarVentaConStock" se ha eliminado completamente de esta clase. ---

        // ... (Aquí continúan los métodos para filtrar y exportar el historial, que no se modifican) ...
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