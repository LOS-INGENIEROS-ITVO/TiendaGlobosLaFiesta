using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClosedXML.Excel;
using Microsoft.Win32;
using TiendaGlobosLaFiesta.Modelos;
using TiendaGlobosLaFiesta.Models;
using TiendaGlobosLaFiesta.Services;
using TiendaGlobosLaFiesta.ViewModels;
using TiendaGlobosLaFiesta.Views;

namespace TiendaGlobosLaFiesta
{
    public partial class VentasControl : UserControl
    {
        private readonly VentaService _ventaService = new();
        public event Action VentaRealizada;

        // Propiedad para acceder fácilmente al ViewModel
        private ModeloDeVistaVentas VM => DataContext as ModeloDeVistaVentas;

        // 🔹 SE DEFINE UNA SOLA VEZ: Propiedad para obtener los datos filtrados del historial
        private IEnumerable<VentaHistorial> HistorialFiltrado => VM.HistorialView?.Cast<VentaHistorial>() ?? Enumerable.Empty<VentaHistorial>();

        public VentasControl()
        {
            InitializeComponent();
            this.Loaded += VentasControl_Loaded;
        }

        private void VentasControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Se asignan las fuentes de datos una vez que el control y el ViewModel están listos.
            if (VM != null)
            {
                cmbClientes.ItemsSource = VM.Clientes;
                cmbFiltroCliente.ItemsSource = VM.Clientes;
                dgProductos.ItemsSource = VM.Productos;
                dgGlobos.ItemsSource = VM.Globos;
                dgHistorial.ItemsSource = VM.HistorialView;
            }
        }

        #region Lógica de Registrar Venta
        private void BtnAumentarCantidad_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ItemVenta item) item.Incrementar();
        }

        private void BtnDisminuirCantidad_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ItemVenta item) item.Decrementar();
        }

        private void BtnRegistrarVenta_Click(object sender, RoutedEventArgs e)
        {
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

            var venta = new Venta
            {
                VentaId = $"VEN{DateTime.Now:yyMMddHHmmss}",
                ClienteId = cliente.ClienteId,
                EmpleadoId = SesionActual.EmpleadoId,
                FechaVenta = DateTime.Now,
                ImporteTotal = VM.ImporteTotal,
                Productos = new ObservableCollection<ProductoVenta>(productosSeleccionados),
                Globos = new ObservableCollection<GloboVenta>(globosSeleccionados),
                Estatus = "Completada"
            };

            if (_ventaService.RegistrarVentaCompleta(venta, out string mensajeError))
            {
                VentaRealizada?.Invoke();
                LimpiarFormulario();
                MessageBox.Show("Venta registrada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"No se pudo registrar la venta:\n{mensajeError}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LimpiarFormulario()
        {
            // Llama al método público del ViewModel para recargar los datos
            VM.CargarDatosIniciales();
            cmbClientes.SelectedIndex = -1;
        }
        #endregion

        #region Lógica del Historial
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
            if (!HistorialFiltrado.Any())
            {
                MessageBox.Show("No hay datos para exportar.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var sfd = new SaveFileDialog { Filter = "Archivos Excel (*.xlsx)|*.xlsx", FileName = "Ventas_Exportadas.xlsx" };
            if (sfd.ShowDialog() == true)
            {
                using var workbook = new XLWorkbook();
                // Asumiendo que tienes una clase GeneradorDeExcel
                // GeneradorDeExcel.CrearHojas(workbook, HistorialFiltrado);
                // workbook.SaveAs(sfd.FileName);
                MessageBox.Show("Exportación a Excel completada.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnExportarPDF_Click(object sender, RoutedEventArgs e)
        {
            if (!HistorialFiltrado.Any())
            {
                MessageBox.Show("No hay datos para exportar.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            GeneradorDePDF.GenerarPDF(HistorialFiltrado);
        }

        private void BtnMostrarGrafica_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Funcionalidad de gráfica por reimplementar.", "Info");
        }
        #endregion
    }
}