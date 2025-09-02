using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ClosedXML.Excel;
using Microsoft.Win32;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TiendaGlobosLaFiesta.Data;
using TiendaGlobosLaFiesta.Models;
using TiendaGlobosLaFiesta.Services;
using TiendaGlobosLaFiesta.ViewModels;

namespace TiendaGlobosLaFiesta
{
    public partial class VentasControl : UserControl
    {
        public VentasViewModel VM { get; set; }

        // Datos filtrados del historial
        private IEnumerable<VentaHistorial> historialFiltrado =>
            VM.HistorialView?.Cast<VentaHistorial>() ?? Enumerable.Empty<VentaHistorial>();

        public VentasControl()
        {
            InitializeComponent();
            VM = new VentasViewModel();
            DataContext = VM;

            cmbClientes.ItemsSource = VM.Clientes;
            cmbFiltroCliente.ItemsSource = VM.Clientes;

            dgProductos.ItemsSource = VM.Productos;
            dgGlobos.ItemsSource = VM.Globos;
            dgHistorial.ItemsSource = VM.HistorialView;

            foreach (var p in VM.Productos) p.PropertyChanged += ItemVenta_PropertyChanged;
            foreach (var g in VM.Globos) g.PropertyChanged += ItemVenta_PropertyChanged;

            ActualizarTotales();
        }

        private void ItemVenta_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ItemVenta.Cantidad) || e.PropertyName == nameof(ItemVenta.Importe))
            {
                ActualizarTotales();
            }
        }

        private void ActualizarTotales()
        {
            int totalProductos = VM.Productos.Sum(p => p.Cantidad);
            int totalGlobos = VM.Globos.Sum(g => g.Cantidad);
            decimal importeTotal = VM.Productos.Sum(p => p.Importe) + VM.Globos.Sum(g => g.Importe);

            txtTotalProductos.Text = totalProductos.ToString();
            txtTotalGlobos.Text = totalGlobos.ToString();
            txtImporteTotal.Text = importeTotal.ToString("C");
        }





        // ==========================
        // Incrementar / Disminuir
        // ==========================
        private void BtnAumentarCantidad_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ItemVenta item)
            {
                VM.Incrementar(item);
                // Forzar actualización de DataGrid
                dgProductos.Items.Refresh();
                dgGlobos.Items.Refresh();
            }
        }

        private void BtnDisminuirCantidad_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ItemVenta item)
            {
                VM.Decrementar(item);
                // Forzar actualización de DataGrid
                dgProductos.Items.Refresh();
                dgGlobos.Items.Refresh();
            }
        }

        // ==========================
        // Registrar Venta
        // ==========================
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

            if (!VentasService.ValidarStock(itemsVenta, out string mensaje))
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
                Productos = new ObservableCollection<ProductoVenta>(productosVenta),
                Globos = new ObservableCollection<GloboVenta>(globosVenta),
                ImporteTotal = productosVenta.Sum(p => p.Importe) + globosVenta.Sum(g => g.Importe)
            };

            if (ConexionBD.RegistrarVenta(venta))
            {
                VentasService.ActualizarStock(itemsVenta);
                var vh = VentasService.CrearHistorial(venta, cliente);

                VM.Historial.Insert(0, vh);
                VM.HistorialView.Refresh();

                // Refrescar DataGrids para reflejar stock y cantidad
                dgProductos.Items.Refresh();
                dgGlobos.Items.Refresh();

                ActualizarTotales();

                MessageBox.Show("Venta registrada correctamente.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        // ==========================
        // Filtrar / Limpiar Historial
        // ==========================
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

        // ==========================
        // Exportar Excel
        // ==========================
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

            // Resumen
            CrearHoja(workbook, "Resumen", historialFiltrado,
                new[] { "ID Venta", "Cliente", "Empleado", "Fecha", "Total" },
                v => new object[] { v.VentaId, v.ClienteNombre, v.Empleado, v.FechaVenta, v.Total },
                (cell, col) =>
                {
                    if (col == 4) cell.Style.DateFormat.Format = "dd/MM/yyyy";
                    if (col == 5) cell.Style.NumberFormat.Format = "$#,##0.00";
                });

            // Productos
            CrearHoja(workbook, "Productos", historialFiltrado.SelectMany(v => v.Productos.Select(p => new { Venta = v, Producto = p })),
                new[] { "ID Venta", "Producto", "Unidad", "Cantidad", "Costo", "Importe" },
                item => new object[] { item.Venta.VentaId, item.Producto.Nombre, item.Producto.Unidad, item.Producto.Cantidad, item.Producto.Costo, item.Producto.Importe },
                (cell, col) =>
                {
                    if (col == 5 || col == 6) cell.Style.NumberFormat.Format = "$#,##0.00";
                });

            // Globos
            CrearHoja(workbook, "Globos", historialFiltrado.SelectMany(v => v.Globos.Select(g => new { Venta = v, Globo = g })),
                new[] { "ID Venta", "Material", "Color", "Tamaño", "Forma", "Temática", "Unidad", "Cantidad", "Costo", "Importe" },
                item => new object[] { item.Venta.VentaId, item.Globo.Material, item.Globo.Color, item.Globo.Tamano, item.Globo.Forma, item.Globo.Tematica, item.Globo.Unidad, item.Globo.Cantidad, item.Globo.Costo, item.Globo.Importe },
                (cell, col) =>
                {
                    if (col == 9 || col == 10) cell.Style.NumberFormat.Format = "$#,##0.00";
                });

            workbook.SaveAs(sfd.FileName);
            MessageBox.Show("Exportación a Excel completada.", "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
            Process.Start(new ProcessStartInfo { FileName = sfd.FileName, UseShellExecute = true });
        }

        private void CrearHoja<T>(XLWorkbook workbook, string nombreHoja, IEnumerable<T> items, string[] headers,
                                   Func<T, object[]> mapFila, Action<IXLCell, int> styleCell = null)
        {
            var ws = workbook.Worksheets.Add(nombreHoja);

            // Header
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            // Filas
            int row = 2;
            foreach (var item in items)
            {
                var valores = mapFila(item);
                for (int col = 0; col < valores.Length; col++)
                {
                    var cell = ws.Cell(row, col + 1);
                    cell.Value = valores[col]?.ToString() ?? "";
                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    styleCell?.Invoke(cell, col + 1);
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }
                row++;
            }

            ws.Columns().AdjustToContents();
        }

        // ==========================
        // Exportar PDF
        // ==========================
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
                    Filter = "Archivos PDF (.pdf)|.pdf",
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
                        page.DefaultTextStyle(x => x.FontSize(9));

                        // ======= HEADER =======
                        page.Header().Row(row =>
                        {
                            row.RelativeColumn().AlignLeft()
                                .Text("Reporte de Ventas")
                                .FontSize(18)
                                .Bold()
                                .FontColor(QuestPDF.Helpers.Colors.Blue.Medium);

                            row.RelativeColumn().AlignRight()
                                .Text(DateTime.Now.ToString("dd/MM/yyyy"))
                                .FontSize(10)
                                .FontColor(QuestPDF.Helpers.Colors.Grey.Medium);
                        });

                        // ======= CONTENT =======
                        page.Content().Column(col =>
                        {
                            // =========================
                            // TABLA RESUMEN
                            // =========================
                            col.Item().PaddingVertical(5).AlignCenter()
                                .Text("Resumen de Ventas")
                                .FontSize(13)
                                .Bold()
                                .FontColor(QuestPDF.Helpers.Colors.Blue.Darken1);

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(c =>
                                {
                                    c.RelativeColumn(1);
                                    c.RelativeColumn(2);
                                    c.RelativeColumn(2);
                                    c.RelativeColumn(2);
                                    c.RelativeColumn(2);
                                });

                                // Encabezado
                                table.Header(header =>
                                {
                                    foreach (var text in new[] { "ID Venta", "Cliente", "Empleado", "Fecha", "Total" })
                                    {
                                        header.Cell()
                                            .Background(QuestPDF.Helpers.Colors.Grey.Lighten2)
                                            .Border(0.8f)
                                            .Padding(4)
                                            .AlignCenter()
                                            .Text(text)
                                            .Bold()
                                            .FontColor(QuestPDF.Helpers.Colors.Black);
                                    }
                                });

                                // Datos
                                foreach (var venta in historialFiltrado)
                                {
                                    table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(venta.VentaId.ToString());
                                    table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(venta.ClienteNombre);
                                    table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(venta.Empleado);
                                    table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(venta.FechaVenta.ToShortDateString());
                                    table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(venta.Total.ToString("C"));
                                }
                            });

                            col.Item().PageBreak();

                            // =========================
                            // TABLA PRODUCTOS
                            // =========================
                            col.Item().PaddingVertical(5).AlignCenter()
                                .Text("Productos")
                                .FontSize(13)
                                .Bold()
                                .FontColor(QuestPDF.Helpers.Colors.Green.Darken1);

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(c =>
                                {
                                    for (int i = 0; i < 6; i++) c.RelativeColumn();
                                });

                                table.Header(header =>
                                {
                                    foreach (var text in new[] { "ID Venta", "Producto", "Unidad", "Cantidad", "Costo", "Importe" })
                                    {
                                        header.Cell()
                                            .Background(QuestPDF.Helpers.Colors.Green.Lighten3)
                                            .Border(0.8f)
                                            .Padding(4)
                                            .AlignCenter()
                                            .Text(text)
                                            .Bold();
                                    }
                                });

                                foreach (var venta in historialFiltrado)
                                {
                                    foreach (var prod in venta.Productos)
                                    {
                                        table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(venta.VentaId.ToString());
                                        table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(prod.Nombre);
                                        table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(prod.Unidad);
                                        table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(prod.Cantidad.ToString());
                                        table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(prod.Costo.ToString("C"));
                                        table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(prod.Importe.ToString("C"));
                                    }
                                }
                            });

                            col.Item().PageBreak();

                            // =========================
                            // TABLA GLOBOS
                            // =========================
                            col.Item().PaddingVertical(5).AlignCenter()
                                .Text("Globos")
                                .FontSize(13)
                                .Bold()
                                .FontColor(QuestPDF.Helpers.Colors.Red.Darken1);

                            col.Item().Table(table =>
                            {
                                table.ColumnsDefinition(c =>
                                {
                                    for (int i = 0; i < 10; i++) c.RelativeColumn();
                                });

                                table.Header(header =>
                                {
                                    foreach (var text in new[]
                                             { "ID Venta", "Material", "Color", "Tamaño", "Forma", "Temática", "Unidad", "Cantidad", "Costo", "Importe" })
                                    {
                                        header.Cell()
                                            .Background(QuestPDF.Helpers.Colors.Red.Lighten3)
                                            .Border(0.8f)
                                            .Padding(4)
                                            .AlignCenter()
                                            .Text(text)
                                            .Bold();
                                    }
                                });

                                foreach (var venta in historialFiltrado)
                                {
                                    foreach (var globo in venta.Globos)
                                    {
                                        table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(venta.VentaId.ToString());
                                        table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(globo.Material);
                                        table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(globo.Color);
                                        table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(globo.Tamano);
                                        table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(globo.Forma);
                                        table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(globo.Tematica);
                                        table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(globo.Unidad);
                                        table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(globo.Cantidad.ToString());
                                        table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(globo.Costo.ToString("C"));
                                        table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(globo.Importe.ToString("C"));
                                    }
                                }
                            });
                        });

                        // ======= FOOTER =======
                        page.Footer().AlignCenter().Text(txt =>
                        {
                            txt.Span("Página ").FontSize(9);
                            txt.CurrentPageNumber().FontSize(9);
                            txt.Span(" de ").FontSize(9);
                            txt.TotalPages().FontSize(9);
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





        // ==========================
        // Mostrar gráfica
        // ==========================
        private void BtnMostrarGrafica_Click(object sender, RoutedEventArgs e)
        {
            // Aquí se puede implementar la lógica para mostrar gráficas basadas en historialFiltrado
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