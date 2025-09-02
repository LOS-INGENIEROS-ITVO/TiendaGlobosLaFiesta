using System;
using System.Collections.Generic;
using System.Linq;
using ClosedXML.Excel;
using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta.Services
{
    public static class GeneradorDeExcel
    {
        public static void CrearHojas(XLWorkbook workbook, IEnumerable<VentaHistorial> historial)
        {
            // Hoja Resumen
            CrearHoja(workbook, "Resumen", historial,
                new[] { "ID Venta", "Cliente", "Empleado", "Fecha", "Total" },
                v => new object[] { v.VentaId, v.ClienteNombre, v.Empleado, v.FechaVenta, v.Total },
                (cell, col) =>
                {
                    if (col == 4) cell.Style.DateFormat.Format = "dd/MM/yyyy";
                    if (col == 5) cell.Style.NumberFormat.Format = "$#,##0.00";
                });

            // Hoja Productos
            CrearHoja(workbook, "Productos", historial.SelectMany(v => v.Productos.Select(p => new { Venta = v, Producto = p })),
                new[] { "ID Venta", "Producto", "Unidad", "Cantidad", "Costo", "Importe" },
                item => new object[] { item.Venta.VentaId, item.Producto.Nombre, item.Producto.Unidad, item.Producto.Cantidad, item.Producto.Costo, item.Producto.Importe },
                (cell, col) =>
                {
                    if (col == 5 || col == 6) cell.Style.NumberFormat.Format = "$#,##0.00";
                });

            // Hoja Globos
            CrearHoja(workbook, "Globos", historial.SelectMany(v => v.Globos.Select(g => new { Venta = v, Globo = g })),
                new[] { "ID Venta", "Material", "Color", "Tamaño", "Forma", "Temática", "Unidad", "Cantidad", "Costo", "Importe" },
                item => new object[] { item.Venta.VentaId, item.Globo.Material, item.Globo.Color, item.Globo.Tamano, item.Globo.Forma, item.Globo.Tematica, item.Globo.Unidad, item.Globo.Cantidad, item.Globo.Costo, item.Globo.Importe },
                (cell, col) =>
                {
                    if (col == 9 || col == 10) cell.Style.NumberFormat.Format = "$#,##0.00";
                });
        }

        private static void CrearHoja<T>(XLWorkbook workbook, string nombreHoja, IEnumerable<T> items, string[] headers,
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
    }
}
