using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                    System.Windows.MessageBox.Show("No hay datos para exportar.", "Atención", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
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
                                .FontColor(Colors.Blue.Medium);

                            row.RelativeColumn().AlignRight()
                                .Text(DateTime.Now.ToString("dd/MM/yyyy"))
                                .FontSize(10)
                                .FontColor(Colors.Grey.Medium);
                        });

                        // ======= CONTENT =======
                        page.Content().Column(col =>
                        {
                            // ======= Resumen de Ventas =======
                            col.Item().PaddingVertical(10).AlignCenter()
                               .Text("📄 Resumen de Ventas")
                               .FontSize(16)
                               .Bold()
                               .FontColor(Colors.Blue.Darken1);

                            col.Item().PaddingBottom(10).Table(table =>
                            {
                                table.ColumnsDefinition(c =>
                                {
                                    c.RelativeColumn(1);
                                    c.RelativeColumn(2);
                                    c.RelativeColumn(2);
                                    c.RelativeColumn(2);
                                    c.RelativeColumn(2);
                                });

                                table.Header(header =>
                                {
                                    foreach (var text in new[] { "ID Venta", "Cliente", "Empleado", "Fecha", "Total" })
                                    {
                                        header.Cell()
                                            .Background(Colors.Grey.Lighten2)
                                            .Border(0.8f)
                                            .Padding(4)
                                            .AlignCenter()
                                            .Text(text)
                                            .Bold()
                                            .FontColor(Colors.Black);
                                    }
                                });

                                foreach (var venta in historial)
                                {
                                    table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(venta.VentaId);
                                    table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(venta.ClienteNombre);
                                    table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(venta.NombreEmpleado);
                                    table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(venta.FechaVenta.ToShortDateString());
                                    table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(venta.Total.ToString("C"));
                                }
                            });

                            col.Item().PageBreak();

                            // ======= Productos =======
                            col.Item().PaddingVertical(10).AlignCenter()
                               .Text("🛒 Productos")
                               .FontSize(16)
                               .Bold()
                               .FontColor(Colors.Green.Darken1);

                            col.Item().PaddingBottom(10).Table(table =>
                            {
                                table.ColumnsDefinition(c => { for (int i = 0; i < 6; i++) c.RelativeColumn(); });
                                table.Header(header =>
                                {
                                    foreach (var text in new[] { "ID Venta", "Producto", "Unidad", "Cantidad", "Costo", "Importe" })
                                    {
                                        header.Cell().Background(Colors.Green.Lighten3).Border(0.8f).Padding(4).AlignCenter().Text(text).Bold();
                                    }
                                });

                                foreach (var venta in historial)
                                {
                                    foreach (var prod in venta.Productos)
                                    {
                                        table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(venta.VentaId);
                                        table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(prod.Nombre);
                                        table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(prod.Unidad);
                                        table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(prod.Cantidad.ToString());
                                        table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(prod.Costo.ToString("C"));
                                        table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(prod.Importe.ToString("C"));
                                    }
                                }
                            });

                            col.Item().PageBreak();

                            // ======= Globos =======
                            col.Item().PaddingVertical(10).AlignCenter()
                               .Text("🎈 Globos")
                               .FontSize(16)
                               .Bold()
                               .FontColor(Colors.Red.Darken1);

                            col.Item().PaddingBottom(10).Table(table =>
                            {
                                table.ColumnsDefinition(c => { for (int i = 0; i < 10; i++) c.RelativeColumn(); });
                                table.Header(header =>
                                {
                                    foreach (var text in new[]
                                             { "ID Venta", "Material", "Color", "Tamaño", "Forma", "Temática", "Unidad", "Cantidad", "Costo", "Importe" })
                                    {
                                        header.Cell().Background(Colors.Red.Lighten3).Border(0.8f).Padding(4).AlignCenter().Text(text).Bold();
                                    }
                                });

                                foreach (var venta in historial)
                                {
                                    foreach (var globo in venta.Globos)
                                    {
                                        table.Cell().Border(0.5f).Padding(3).AlignCenter().Text(venta.VentaId);
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
                            txt.Span(" | Tienda Globos La Fiesta").FontSize(9);
                        });
                    });
                });

                document.GeneratePdf(sfd.FileName);

                System.Windows.MessageBox.Show("Exportación a PDF completada.", "Éxito", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                Process.Start(new ProcessStartInfo { FileName = sfd.FileName, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error generando PDF: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}
