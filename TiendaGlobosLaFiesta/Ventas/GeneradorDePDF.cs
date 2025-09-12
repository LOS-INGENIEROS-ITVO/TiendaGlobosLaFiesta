using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.Win32;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta.Services
{
    public static class GeneradorDePDF
    {
        public static void GenerarPDF(IEnumerable<VentaHistorial> historial)
        {
            try
            {
                if (historial == null || !historial.Any())
                {
                    System.Windows.MessageBox.Show("No hay datos para exportar.", "Atención",
                        System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
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

                        // ===== HEADER =====
                        page.Header().Row(row =>
                        {
                            row.RelativeColumn().AlignLeft()
                                .Text("Reporte de Ventas")
                                .FontSize(18)
                                .Bold()
                                .FontColor(Colors.Blue.Medium);

                            row.RelativeColumn().AlignRight()
                                .Text(DateTime.Now.ToString("dd/MM/yyyy"))
                                .FontSize(10)
                                .FontColor(Colors.Grey.Medium);
                        });

                        // ===== CONTENT =====
                        page.Content().Column(col =>
                        {
                            // ----- Resumen de Ventas -----
                            col.Item().PaddingVertical(10).AlignCenter()
                                .Text("📄 Resumen de Ventas")
                                .FontSize(20)
                                .Bold()
                                .FontColor(Colors.Blue.Darken1);

                            col.Item().PaddingBottom(10).Table(table =>
                            {
                                table.ColumnsDefinition(c =>
                                {
                                    c.ConstantColumn(120);  // ID Venta → modificar ancho aquí
                                    c.RelativeColumn(1);   // Cliente → ajustable automáticamente
                                    c.RelativeColumn(1);   // Empleado → ajustable automáticamente
                                    c.ConstantColumn(60);  // Fecha
                                    c.ConstantColumn(60);  // Total
                                });

                                // Header
                                table.Header(header =>
                                {
                                    foreach (var text in new[] { "ID Venta", "Cliente", "Empleado", "Fecha", "Total" })
                                    {
                                        header.Cell().Background(Colors.Grey.Lighten2).Border(0.8f).Padding(4)
                                            .AlignCenter().Text(text).SemiBold();
                                    }
                                });

                                // Filas de ventas
                                foreach (var venta in historial)
                                {
                                    table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(venta.VentaId);
                                    table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(venta.ClienteNombre);
                                    table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(venta.NombreEmpleado);
                                    table.Cell().Border(0.5f).Padding(3).AlignCenter()
                                        .Text(venta.FechaVenta.ToString("dd/MM/yyyy"));
                                    table.Cell().Border(0.5f).Padding(3).AlignCenter()
                                        .Text(venta.Total.ToString("C", CultureInfo.CurrentCulture));
                                }

                                // Total general
                                table.Cell().ColumnSpan(4).Border(0.5f).Padding(3).AlignRight().Text("Total Ventas:").SemiBold();
                                table.Cell().Border(0.5f).Padding(3).AlignCenter()
                                    .Text(historial.Sum(v => v.Total).ToString("C", CultureInfo.CurrentCulture)).SemiBold();
                            });

                            col.Item().PageBreak();

                            // ----- Productos -----
                            col.Item().PaddingVertical(10).AlignCenter()
                                .Text("🛒 Productos")
                                .FontSize(20)
                                .Bold()
                                .FontColor(Colors.Green.Darken1);

                            var todosProductos = historial.SelectMany(v => v.Productos.Select(p => new { Venta = v, Producto = p }));
                            col.Item().PaddingBottom(10).Table(table =>
                            {
                                table.ColumnsDefinition(c =>
                                {
                                    c.ConstantColumn(120);  // ID Venta
                                    c.RelativeColumn(1);   // Producto → ajustable
                                    c.ConstantColumn(50);  // Unidad
                                    c.ConstantColumn(50);  // Cantidad
                                    c.ConstantColumn(60);  // Costo
                                    c.ConstantColumn(60);  // Importe
                                });

                                table.Header(header =>
                                {
                                    foreach (var text in new[] { "ID Venta", "Producto", "Unidad", "Cantidad", "Costo", "Importe" })
                                        header.Cell().Background(Colors.Green.Lighten3).Border(0.8f).Padding(4)
                                            .AlignCenter().Text(text).SemiBold();
                                });

                                foreach (var item in todosProductos)
                                {
                                    table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(item.Venta.VentaId);
                                    table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(item.Producto.Nombre);
                                    table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(item.Producto.Unidad);
                                    table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(item.Producto.Cantidad.ToString());
                                    table.Cell().Border(0.5f).Padding(3).AlignCenter()
                                        .Text(item.Producto.Costo.ToString("C", CultureInfo.CurrentCulture));
                                    table.Cell().Border(0.5f).Padding(3).AlignCenter()
                                        .Text(item.Producto.Importe.ToString("C", CultureInfo.CurrentCulture));
                                }

                                // Total productos
                                table.Cell().ColumnSpan(5).Border(0.5f).Padding(3).AlignRight().Text("Total Productos:").SemiBold();
                                table.Cell().Border(0.5f).Padding(3).AlignCenter()
                                    .Text(todosProductos.Sum(p => p.Producto.Importe).ToString("C", CultureInfo.CurrentCulture)).SemiBold();
                            });

                            col.Item().PageBreak();

                            // ----- Globos -----
                            col.Item().PaddingVertical(10).AlignCenter()
                                .Text("🎈 Globos")
                                .FontSize(20)
                                .Bold()
                                .FontColor(Colors.Red.Darken1);

                            var todosGlobos = historial.SelectMany(v => v.Globos.Select(g => new { Venta = v, Globo = g }));
                            col.Item().PaddingBottom(10).Table(table =>
                            {
                                table.ColumnsDefinition(c =>
                                {
                                    c.ConstantColumn(100);  // ID Venta
                                    c.RelativeColumn(1);   // Material → ajustable
                                    c.ConstantColumn(50);  // Color
                                    c.ConstantColumn(50);  // Tamaño
                                    c.ConstantColumn(50);  // Forma
                                    c.RelativeColumn(1);   // Temática → ajustable
                                    c.ConstantColumn(50);  // Unidad
                                    c.ConstantColumn(50);  // Cantidad
                                    c.ConstantColumn(40);  // Costo
                                    c.ConstantColumn(40);  // Importe
                                });

                                table.Header(header =>
                                {
                                    foreach (var text in new[]
                                        { "ID Venta", "Material", "Color", "Tamaño", "Forma", "Temática", "Unidad", "Cantidad", "Costo", "Importe" })
                                        header.Cell().Background(Colors.Red.Lighten3).Border(0.8f).Padding(4)
                                            .AlignCenter().Text(text).SemiBold();
                                });

                                foreach (var item in todosGlobos)
                                {
                                    table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(item.Venta.VentaId);
                                    table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(item.Globo.Material);
                                    table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(item.Globo.Color);
                                    table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(item.Globo.Tamano);
                                    table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(item.Globo.Forma);
                                    table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(item.Globo.Tematica);
                                    table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(item.Globo.Unidad);
                                    table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(item.Globo.Cantidad.ToString());
                                    table.Cell().Border(0.5f).Padding(3).AlignCenter()
                                        .Text(item.Globo.Costo.ToString("C", CultureInfo.CurrentCulture));
                                    table.Cell().Border(0.5f).Padding(3).AlignCenter()
                                        .Text(item.Globo.Importe.ToString("C", CultureInfo.CurrentCulture));
                                }

                                // Total globos
                                table.Cell().ColumnSpan(9).Border(0.5f).Padding(3).AlignRight().Text("Total Globos:").SemiBold();
                                table.Cell().Border(0.5f).Padding(3).AlignCenter()
                                    .Text(todosGlobos.Sum(g => g.Globo.Importe).ToString("C", CultureInfo.CurrentCulture)).SemiBold();
                            });
                        });

                        // ===== FOOTER =====
                        page.Footer().AlignCenter().Text(txt =>
                        {
                            txt.Span("Página ").FontSize(9);
                            txt.CurrentPageNumber().FontSize(9);
                            txt.Span(" de ").FontSize(9);
                            txt.TotalPages().FontSize(9);
                            txt.Span(" | Tienda Globos La Fiesta").FontSize(9);
                        });
                    });
                });

                // Generar PDF
                document.GeneratePdf(sfd.FileName);
                System.Windows.MessageBox.Show("Exportación a PDF completada.", "Éxito",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                Process.Start(new ProcessStartInfo { FileName = sfd.FileName, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error generando PDF: {ex.Message}", "Error",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}