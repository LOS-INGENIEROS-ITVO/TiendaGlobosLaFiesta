using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TiendaGlobosLaFiesta.Data;
using TiendaGlobosLaFiesta.Models;
using ClosedXML.Excel;
using Microsoft.Win32;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Diagnostics;



namespace TiendaGlobosLaFiesta
{
    public partial class VentasControl : UserControl
    {
        private ObservableCollection<ProductoVenta> productos;
        private ObservableCollection<GloboVenta> globos;
        private ObservableCollection<Cliente> clientes;
        private ObservableCollection<VentaHistorial> historial;
        private ObservableCollection<VentaHistorial> historialFiltrado;

        public VentasControl()
        {
            InitializeComponent();
            CargarDatos();
        }

        private void CargarDatos()
        {
            // Cargar clientes
            clientes = new ObservableCollection<Cliente>(ConexionBD.ObtenerClientes());
            cmbClientes.ItemsSource = clientes;
            cmbFiltroCliente.ItemsSource = clientes;

            // Cargar productos
            productos = new ObservableCollection<ProductoVenta>(ConexionBD.ObtenerProductos());
            foreach (var p in productos)
                p.PropertyChanged += ItemVenta_PropertyChanged;
            dgProductos.ItemsSource = productos;

            // Cargar globos
            globos = new ObservableCollection<GloboVenta>(ConexionBD.ObtenerGlobos());
            foreach (var g in globos)
                g.PropertyChanged += ItemVenta_PropertyChanged;
            dgGlobos.ItemsSource = globos;

            // Cargar historial
            historial = new ObservableCollection<VentaHistorial>(ConexionBD.ObtenerHistorialVentas());
            historialFiltrado = new ObservableCollection<VentaHistorial>(historial);
            dgHistorial.ItemsSource = historialFiltrado;

            ActualizarTotales();
        }

        // ==========================
        // Eventos para actualizar totales
        // ==========================
        private void ItemVenta_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ItemVenta.Cantidad))
            {
                ActualizarTotales();
            }
        }

        // ==========================
        // Botones Cantidad
        // ==========================
        private void BtnAumentarCantidad_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ItemVenta item)
            {
                if (item.Cantidad < item.Stock) item.Cantidad++;
            }
        }

        private void BtnDisminuirCantidad_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ItemVenta item)
            {
                if (item.Cantidad > 0) item.Cantidad--;
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

            if (!productosVenta.Any() && !globosVenta.Any())
            {
                MessageBox.Show("Seleccione al menos un producto o globo.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validar stock
            foreach (var p in productosVenta)
                if (p.Cantidad > p.Stock)
                {
                    MessageBox.Show($"No hay suficiente stock para el producto {p.Nombre}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

            foreach (var g in globosVenta)
                if (g.Cantidad > g.Stock)
                {
                    MessageBox.Show($"No hay suficiente stock para el globo {g.Material} - {g.Color}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

            var venta = new Venta
            {
                VentaId = ConexionBD.ObtenerSiguienteVentaId(),
                ClienteId = ((Cliente)cmbClientes.SelectedItem).ClienteId,
                EmpleadoId = SesionActual.EmpleadoId,
                FechaVenta = DateTime.Now,
                Productos = new ObservableCollection<ProductoVenta>(productosVenta),
                Globos = new ObservableCollection<GloboVenta>(globosVenta),
                ImporteTotal = productosVenta.Sum(p => p.Importe) + globosVenta.Sum(g => g.Importe)
            };

            if (ConexionBD.RegistrarVenta(venta))
            {
                MessageBox.Show("Venta registrada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                // Actualizar stock local
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

                // Agregar al historial
                var vh = new VentaHistorial
                {
                    VentaId = venta.VentaId,
                    ClienteId = venta.ClienteId,
                    ClienteNombre = ((Cliente)cmbClientes.SelectedItem).NombreCompleto(),
                    Empleado = SesionActual.NombreEmpleadoCompleto,
                    FechaVenta = venta.FechaVenta,
                    Total = venta.ImporteTotal,
                    Productos = venta.Productos,
                    Globos = venta.Globos
                };

                historial.Insert(0, vh);
                historialFiltrado.Insert(0, vh);
            }
        }

        // ==========================
        // Filtrar Historial
        // ==========================
        private void BtnFiltrarHistorial_Click(object sender, RoutedEventArgs e)
        {
            var listaFiltrada = historial.AsEnumerable();

            if (cmbFiltroCliente.SelectedItem is Cliente cliente)
                listaFiltrada = listaFiltrada.Where(v => v.ClienteId == cliente.ClienteId);

            if (dpFechaDesde.SelectedDate.HasValue)
                listaFiltrada = listaFiltrada.Where(v => v.FechaVenta.Date >= dpFechaDesde.SelectedDate.Value.Date);

            if (dpFechaHasta.SelectedDate.HasValue)
                listaFiltrada = listaFiltrada.Where(v => v.FechaVenta.Date <= dpFechaHasta.SelectedDate.Value.Date);

            historialFiltrado = new ObservableCollection<VentaHistorial>(listaFiltrada);
            dgHistorial.ItemsSource = historialFiltrado;
        }

        private void BtnLimpiarFiltros_Click(object sender, RoutedEventArgs e)
        {
            cmbFiltroCliente.SelectedIndex = -1;
            dpFechaDesde.SelectedDate = null;
            dpFechaHasta.SelectedDate = null;

            historialFiltrado = new ObservableCollection<VentaHistorial>(historial);
            dgHistorial.ItemsSource = historialFiltrado;
        }




        private void BtnExportarExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (historialFiltrado == null || !historialFiltrado.Any())
                {
                    MessageBox.Show("No hay datos para exportar.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                SaveFileDialog sfd = new SaveFileDialog
                {
                    Filter = "Archivos Excel (*.xlsx)|*.xlsx",
                    FileName = "Ventas_Exportadas.xlsx"
                };

                if (sfd.ShowDialog() != true) return;

                using (var workbook = new XLWorkbook())
                {
                    // =====================
                    // Hoja Resumen
                    // =====================
                    var wsResumen = workbook.Worksheets.Add("Resumen");
                    string[] headersResumen = { "ID Venta", "Cliente", "Empleado", "Fecha", "Total" };

                    for (int i = 0; i < headersResumen.Length; i++)
                    {
                        var cell = wsResumen.Cell(1, i + 1);
                        cell.Value = headersResumen[i];
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }

                    int rowResumen = 2;
                    foreach (var venta in historialFiltrado)
                    {
                        wsResumen.Cell(rowResumen, 1).Value = venta.VentaId;
                        wsResumen.Cell(rowResumen, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        wsResumen.Cell(rowResumen, 2).Value = venta.ClienteNombre;
                        wsResumen.Cell(rowResumen, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                        wsResumen.Cell(rowResumen, 3).Value = venta.Empleado;
                        wsResumen.Cell(rowResumen, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                        wsResumen.Cell(rowResumen, 4).Value = venta.FechaVenta;
                        wsResumen.Cell(rowResumen, 4).Style.DateFormat.Format = "dd/MM/yyyy";
                        wsResumen.Cell(rowResumen, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        wsResumen.Cell(rowResumen, 5).Value = venta.Total;
                        wsResumen.Cell(rowResumen, 5).Style.NumberFormat.Format = "$#,##0.00";
                        wsResumen.Cell(rowResumen, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                        // Bordes de la fila
                        for (int col = 1; col <= headersResumen.Length; col++)
                        {
                            wsResumen.Cell(rowResumen, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        }

                        rowResumen++;
                    }
                    wsResumen.Columns().AdjustToContents();

                    // =====================
                    // Hoja Productos
                    // =====================
                    var wsProductos = workbook.Worksheets.Add("Productos");
                    string[] headersProd = { "ID Venta", "Producto", "Unidad", "Cantidad", "Costo", "Importe" };
                    for (int i = 0; i < headersProd.Length; i++)
                    {
                        var cell = wsProductos.Cell(1, i + 1);
                        cell.Value = headersProd[i];
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }

                    int rowProd = 2;
                    foreach (var venta in historialFiltrado)
                    {
                        foreach (var prod in venta.Productos)
                        {
                            wsProductos.Cell(rowProd, 1).Value = venta.VentaId;
                            wsProductos.Cell(rowProd, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            wsProductos.Cell(rowProd, 2).Value = prod.Nombre;
                            wsProductos.Cell(rowProd, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                            wsProductos.Cell(rowProd, 3).Value = prod.Unidad;
                            wsProductos.Cell(rowProd, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            wsProductos.Cell(rowProd, 4).Value = prod.Cantidad;
                            wsProductos.Cell(rowProd, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            wsProductos.Cell(rowProd, 5).Value = prod.Costo;
                            wsProductos.Cell(rowProd, 5).Style.NumberFormat.Format = "$#,##0.00";
                            wsProductos.Cell(rowProd, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                            wsProductos.Cell(rowProd, 6).Value = prod.Importe;
                            wsProductos.Cell(rowProd, 6).Style.NumberFormat.Format = "$#,##0.00";
                            wsProductos.Cell(rowProd, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                            for (int col = 1; col <= headersProd.Length; col++)
                                wsProductos.Cell(rowProd, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            rowProd++;
                        }
                    }
                    wsProductos.Columns().AdjustToContents();

                    // =====================
                    // Hoja Globos
                    // =====================
                    var wsGlobos = workbook.Worksheets.Add("Globos");
                    string[] headersGlobos = { "ID Venta", "Material", "Color", "Tamaño", "Forma", "Temática", "Unidad", "Cantidad", "Costo", "Importe" };
                    for (int i = 0; i < headersGlobos.Length; i++)
                    {
                        var cell = wsGlobos.Cell(1, i + 1);
                        cell.Value = headersGlobos[i];
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }

                    int rowGlobos = 2;
                    foreach (var venta in historialFiltrado)
                    {
                        foreach (var globo in venta.Globos)
                        {
                            wsGlobos.Cell(rowGlobos, 1).Value = venta.VentaId;
                            wsGlobos.Cell(rowGlobos, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            wsGlobos.Cell(rowGlobos, 2).Value = globo.Material;
                            wsGlobos.Cell(rowGlobos, 2).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                            wsGlobos.Cell(rowGlobos, 3).Value = globo.Color;
                            wsGlobos.Cell(rowGlobos, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            wsGlobos.Cell(rowGlobos, 4).Value = globo.Tamano;
                            wsGlobos.Cell(rowGlobos, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            wsGlobos.Cell(rowGlobos, 5).Value = globo.Forma;
                            wsGlobos.Cell(rowGlobos, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            wsGlobos.Cell(rowGlobos, 6).Value = globo.Tematica;
                            wsGlobos.Cell(rowGlobos, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;

                            wsGlobos.Cell(rowGlobos, 7).Value = globo.Unidad;
                            wsGlobos.Cell(rowGlobos, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            wsGlobos.Cell(rowGlobos, 8).Value = globo.Cantidad;
                            wsGlobos.Cell(rowGlobos, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                            wsGlobos.Cell(rowGlobos, 9).Value = globo.Costo;
                            wsGlobos.Cell(rowGlobos, 9).Style.NumberFormat.Format = "$#,##0.00";
                            wsGlobos.Cell(rowGlobos, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                            wsGlobos.Cell(rowGlobos, 10).Value = globo.Importe;
                            wsGlobos.Cell(rowGlobos, 10).Style.NumberFormat.Format = "$#,##0.00";
                            wsGlobos.Cell(rowGlobos, 10).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                            for (int col = 1; col <= headersGlobos.Length; col++)
                                wsGlobos.Cell(rowGlobos, col).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            rowGlobos++;
                        }
                    }
                    wsGlobos.Columns().AdjustToContents();

                    // Guardar y abrir
                    workbook.SaveAs(sfd.FileName);
                    MessageBox.Show("Exportación a Excel completada.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = sfd.FileName,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al exportar: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }














        private void BtnExportarPDF_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (historialFiltrado == null || !historialFiltrado.Any())
                {
                    MessageBox.Show("No hay datos para exportar.", "Atención", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var sfd = new SaveFileDialog
                {
                    Filter = "Archivos PDF (*.pdf)|*.pdf",
                    FileName = "Ventas_Exportadas.pdf"
                };
                if (sfd.ShowDialog() != true) return;

                QuestPDF.Settings.License = LicenseType.Community;

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(20);
                        page.Content().Column(col =>
                        {
                            // Título principal
                            col.Item().Element(c =>
                                c.PaddingBottom(10).AlignCenter()
                                 .Text("Reporte de Ventas").Bold().FontSize(22)
                            );

                            // Subtítulo Resumen
                            col.Item().Element(c =>
                                c.PaddingBottom(5).AlignCenter()
                                 .Text("Resumen de Ventas").Bold().FontSize(16)
                            );

                            // TABLA DE RESUMEN
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(c => {
                                    c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn(); c.RelativeColumn();
                                });

                                table.Header(header =>
                                {
                                    foreach (var text in new[] { "ID Venta", "Cliente", "Empleado", "Fecha", "Total" })
                                    {
                                        header.Cell().Element(cell =>
                                            cell.Border(1).Padding(5)
                                                .Text(text).Bold().AlignCenter()
                                        );
                                    }
                                });

                                foreach (var venta in historialFiltrado)
                                {
                                    table.Cell().Element(c => c.Border(1).Padding(5).Text(venta.VentaId.ToString()).AlignCenter());
                                    table.Cell().Element(c => c.Border(1).Padding(5).Text(venta.ClienteNombre).AlignCenter());
                                    table.Cell().Element(c => c.Border(1).Padding(5).Text(venta.Empleado).AlignCenter());
                                    table.Cell().Element(c => c.Border(1).Padding(5).Text(venta.FechaVenta.ToShortDateString()).AlignCenter());
                                    table.Cell().Element(c => c.Border(1).Padding(5).Text(venta.Total.ToString("C")).AlignCenter());
                                }
                            });

                            col.Item().PageBreak();

                            // Subtítulo Productos
                            col.Item().Element(c =>
                                c.PaddingBottom(5).AlignCenter()
                                 .Text("Productos").Bold().FontSize(16)
                            );

                            // TABLA DE PRODUCTOS
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(c => {
                                    for (int i = 0; i < 6; i++) c.RelativeColumn();
                                });

                                table.Header(header =>
                                {
                                    foreach (var text in new[] { "ID Venta", "Producto", "Unidad", "Cantidad", "Costo", "Importe" })
                                    {
                                        header.Cell().Element(cell =>
                                            cell.Border(1).Padding(5)
                                                .Text(text).Bold().AlignCenter()
                                        );
                                    }
                                });

                                foreach (var venta in historialFiltrado)
                                {
                                    foreach (var prod in venta.Productos)
                                    {
                                        table.Cell().Element(c => c.Border(1).Padding(5).Text(venta.VentaId.ToString()).AlignCenter());
                                        table.Cell().Element(c => c.Border(1).Padding(5).Text(prod.Nombre).AlignCenter());
                                        table.Cell().Element(c => c.Border(1).Padding(5).Text(prod.Unidad).AlignCenter());
                                        table.Cell().Element(c => c.Border(1).Padding(5).Text(prod.Cantidad.ToString()).AlignCenter());
                                        table.Cell().Element(c => c.Border(1).Padding(5).Text(prod.Costo.ToString("C")).AlignCenter());
                                        table.Cell().Element(c => c.Border(1).Padding(5).Text(prod.Importe.ToString("C")).AlignCenter());
                                    }
                                }
                            });

                            col.Item().PageBreak();

                            // Subtítulo Globos
                            col.Item().Element(c =>
                                c.PaddingBottom(5).AlignCenter()
                                 .Text("Globos").Bold().FontSize(16)
                            );

                            // TABLA DE GLOBOS
                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(c => {
                                    for (int i = 0; i < 10; i++) c.RelativeColumn();
                                });

                                table.Header(header =>
                                {
                                    foreach (var text in new[] { "ID Venta", "Material", "Color", "Tamaño", "Forma", "Temática", "Unidad", "Cantidad", "Costo", "Importe" })
                                    {
                                        header.Cell().Element(cell =>
                                            cell.Border(1).Padding(5)
                                                .Text(text).Bold().AlignCenter()
                                        );
                                    }
                                });

                                foreach (var venta in historialFiltrado)
                                {
                                    foreach (var globo in venta.Globos)
                                    {
                                        table.Cell().Element(c => c.Border(1).Padding(5).Text(venta.VentaId.ToString()).AlignCenter());
                                        table.Cell().Element(c => c.Border(1).Padding(5).Text(globo.Material).AlignCenter());
                                        table.Cell().Element(c => c.Border(1).Padding(5).Text(globo.Color).AlignCenter());
                                        table.Cell().Element(c => c.Border(1).Padding(5).Text(globo.Tamano).AlignCenter());
                                        table.Cell().Element(c => c.Border(1).Padding(5).Text(globo.Forma).AlignCenter());
                                        table.Cell().Element(c => c.Border(1).Padding(5).Text(globo.Tematica).AlignCenter());
                                        table.Cell().Element(c => c.Border(1).Padding(5).Text(globo.Unidad).AlignCenter());
                                        table.Cell().Element(c => c.Border(1).Padding(5).Text(globo.Cantidad.ToString()).AlignCenter());
                                        table.Cell().Element(c => c.Border(1).Padding(5).Text(globo.Costo.ToString("C")).AlignCenter());
                                        table.Cell().Element(c => c.Border(1).Padding(5).Text(globo.Importe.ToString("C")).AlignCenter());
                                    }
                                }
                            });
                        });
                    });
                });

                document.GeneratePdf(sfd.FileName);

                MessageBox.Show("Exportación a PDF completada.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);

                Process.Start(new ProcessStartInfo { FileName = sfd.FileName, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generando PDF: {ex.Message}\nStackTrace: {ex.StackTrace}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }






















        private void BtnMostrarGrafica_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Funcionalidad de mostrar gráfica aún no implementada.");
        }
    }

    // ==========================
    // Extensión para NombreCompleto
    // ==========================
    public static class ClienteExtensions
    {
        public static string NombreCompleto(this Cliente c)
        {
            return $"{c.PrimerNombre} {c.SegundoNombre} {c.ApellidoP} {c.ApellidoM}".Trim();
        }
    }
}