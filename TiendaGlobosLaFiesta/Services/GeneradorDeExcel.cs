using ClosedXML.Excel;
using TiendaGlobosLaFiesta.Models;
using System.Globalization;

namespace TiendaGlobosLaFiesta.Services
{
    public static class GeneradorDeExcel
    {
        public static void CrearHojas(XLWorkbook workbook, IEnumerable<VentaHistorial> historial)
        {
            // Hoja Resumen
            CrearHoja(workbook, "Resumen", historial,
                new[] { "ID Venta", "Cliente", "Empleado", "Fecha", "Total" },
                v => new object[] { v.VentaId, v.ClienteNombre, v.NombreEmpleado, v.FechaVenta, v.Total },
                (cell, col) =>
                {
                    if (col == 4) cell.Style.DateFormat.Format = "dd/MM/yyyy";
                    if (col == 5) cell.Style.NumberFormat.Format = "$#,##0.00";
                },
                totalValue: historial.Sum(v => v.Total));

            // Hoja Productos
            var todosProductos = historial.SelectMany(v => v.Productos.Select(p => new { Venta = v, Producto = p }));
            CrearHoja(workbook, "Productos", todosProductos,
                new[] { "ID Venta", "Producto", "Unidad", "Cantidad", "Costo", "Importe" },
                item => new object[]
                {
                    item.Venta.VentaId,
                    item.Producto.Nombre,
                    item.Producto.Unidad,
                    item.Producto.Cantidad,
                    item.Producto.Costo,
                    item.Producto.Importe
                },
                (cell, col) =>
                {
                    if (col == 5 || col == 6) cell.Style.NumberFormat.Format = "$#,##0.00";
                },
                totalValue: todosProductos.Sum(p => p.Producto.Importe));

            // Hoja Globos
            var todosGlobos = historial.SelectMany(v => v.Globos.Select(g => new { Venta = v, Globo = g }));
            CrearHoja(workbook, "Globos", todosGlobos,
                new[] { "ID Venta", "Material", "Color", "Tamaño", "Forma", "Temática", "Unidad", "Cantidad", "Costo", "Importe" },
                item => new object[]
                {
                    item.Venta.VentaId,
                    item.Globo.Material,
                    item.Globo.Color,
                    FormatearVacio(item.Globo.Tamano),
                    FormatearVacio(item.Globo.Forma),
                    FormatearVacio(item.Globo.Tematica),
                    item.Globo.Unidad,
                    item.Globo.Cantidad,
                    item.Globo.Costo,
                    item.Globo.Importe
                },
                (cell, col) =>
                {
                    if (col == 9 || col == 10) cell.Style.NumberFormat.Format = "$#,##0.00";
                },
                totalValue: todosGlobos.Sum(g => g.Globo.Importe));
        }

        private static string FormatearVacio(string texto) => string.IsNullOrWhiteSpace(texto) ? "---" : texto;

        private static void CrearHoja<T>(XLWorkbook workbook, string nombreHoja, IEnumerable<T> items, string[] headers,
                                  Func<T, object[]> mapFila, Action<IXLCell, int> styleCell = null,
                                  decimal? totalValue = null)
        {
            var ws = workbook.Worksheets.Add(nombreHoja);

            // Cabecera
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Font.FontColor = XLColor.White;
                cell.Style.Fill.BackgroundColor = XLColor.CornflowerBlue;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            // Filas
            int row = 2;
            bool alternate = false;
            foreach (var item in items)
            {
                var valores = mapFila(item);
                for (int col = 0; col < valores.Length; col++)
                {
                    var cell = ws.Cell(row, col + 1);

                    if (valores[col] is decimal d)
                        cell.SetValue(d);
                    else if (valores[col] is int iVal)
                        cell.SetValue(iVal);
                    else if (valores[col] is DateTime dt)
                        cell.SetValue(dt);
                    else
                        cell.SetValue(valores[col]?.ToString() ?? "");

                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    cell.Style.Fill.BackgroundColor = alternate ? XLColor.WhiteSmoke : XLColor.White;
                    styleCell?.Invoke(cell, col + 1);
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }
                row++;
                alternate = !alternate;
            }

            // Fila Total
            if (totalValue.HasValue)
            {
                var totalRow = row;
                ws.Cell(totalRow, 1).SetValue("Total:");
                ws.Range(totalRow, 1, totalRow, headers.Length - 1).Merge();
                ws.Cell(totalRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(totalRow, 1).Style.Font.Bold = true;
                ws.Cell(totalRow, 1).Style.Fill.BackgroundColor = XLColor.CadetBlue;
                ws.Cell(totalRow, 1).Style.Font.FontColor = XLColor.White;

                ws.Cell(totalRow, headers.Length).SetValue(totalValue.Value);
                ws.Cell(totalRow, headers.Length).Style.NumberFormat.Format = "$#,##0.00";
                ws.Cell(totalRow, headers.Length).Style.Fill.BackgroundColor = XLColor.CadetBlue;
                ws.Cell(totalRow, headers.Length).Style.Font.FontColor = XLColor.White;
                ws.Cell(totalRow, headers.Length).Style.Font.Bold = true;

                ws.Range(totalRow, 1, totalRow, headers.Length).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            ws.Columns().AdjustToContents();
        }
    }
}