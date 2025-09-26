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
using System.Text.RegularExpressions;
using System.Windows.Input;


namespace TiendaGlobosLaFiesta.Views
{
    public partial class VentasControl : UserControl
    {
        private readonly VentaService _ventaService = new();
        public event Action VentaRealizada;

        private ModeloDeVistaVentas VM => DataContext as ModeloDeVistaVentas;
        private IEnumerable<VentaHistorial> HistorialFiltrado => VM?.HistorialView?.Cast<VentaHistorial>() ?? Enumerable.Empty<VentaHistorial>();

        public VentasControl()
        {
            InitializeComponent();
            this.DataContext = new ModeloDeVistaVentas();
            this.Loaded += VentasControl_Loaded;
        }

        private void VentasControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (VM != null)
            {
                cmbClientes.ItemsSource = VM.Clientes;
                cmbFiltroCliente.ItemsSource = VM.Clientes;
                dgProductos.ItemsSource = VM.Productos;
                dgGlobos.ItemsSource = VM.Globos;
                dgHistorial.ItemsSource = VM.HistorialView;
            }
        }

        private void Cantidad_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]+"); // Solo permite números
            e.Handled = regex.IsMatch(e.Text);
        }

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

        private void BtnRegistrarVenta_Click(object sender, RoutedEventArgs e)
        {
            if (cmbClientes.SelectedItem is not Cliente cliente)
            {
                MessageBox.Show("Selecciona un cliente.", "Validación", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var productosSeleccionados = new ObservableCollection<ProductoVenta>(VM.Productos.Where(p => p.Cantidad > 0));
            var globosSeleccionados = new ObservableCollection<GloboVenta>(VM.Globos.Where(g => g.Cantidad > 0));

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
                Productos = productosSeleccionados,
                Globos = globosSeleccionados,
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
            // 1. Llama al método del ViewModel que recarga todas las listas desde la BD.
            VM.CargarDatosIniciales();

            // 2. 🔹 CORRECCIÓN: Vuelve a enlazar las listas actualizadas a los controles de la UI 🔹
            // Esto le dice a las tablas en pantalla que muestren los nuevos datos.
            dgProductos.ItemsSource = VM.Productos;
            dgGlobos.ItemsSource = VM.Globos;
            dgHistorial.ItemsSource = VM.HistorialView;

            // 3. Limpia la selección del cliente.
            cmbClientes.SelectedIndex = -1;
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
            if (!HistorialFiltrado.Any())
            {
                MessageBox.Show("No hay datos para exportar.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var sfd = new SaveFileDialog { Filter = "Archivos Excel (*.xlsx)|*.xlsx", FileName = $"Ventas_{DateTime.Now:yyyyMMdd}.xlsx" };
            if (sfd.ShowDialog() == true)
            {
                try
                {
                    using var workbook = new XLWorkbook();
                    GeneradorDeExcel.CrearHojas(workbook, HistorialFiltrado);
                    workbook.SaveAs(sfd.FileName);
                    MessageBox.Show("Exportación a Excel completada.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error al exportar a Excel: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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
            if (!HistorialFiltrado.Any())
            {
                MessageBox.Show("No hay datos para mostrar en la gráfica.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var ventana = new VentasGraficaWindow(HistorialFiltrado);
            ventana.ShowDialog();
        }
    }
}