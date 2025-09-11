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
            // ===== Hoja Resumen de Ventas =====
            CrearHoja(workbook, "Resumen", historial,
                new[] { "ID Venta", "Cliente", "Empleado", "Fecha", "Total" },
                v => new object[] { v.VentaId, v.ClienteNombre, v.NombreEmpleado, v.FechaVenta, v.Total },
                (cell, col) =>
                {
                    if (col == 4) cell.Style.DateFormat.Format = "dd/MM/yyyy";
                    if (col == 5) cell.Style.NumberFormat.Format = "$#,##0.00";
                },
                new double[] { 15, 30, 30, 20, 15 },
                includeTotal: true,
                totalValue: historial.Sum(v => v.Total)
            );

            // ===== Hoja Productos =====
            CrearHoja(workbook, "Productos", historial.SelectMany(v => v.Productos.Select(p => new { Venta = v, Producto = p })),
                new[] { "ID Venta", "Producto", "Unidad", "Cantidad", "Costo", "Importe" },
                item => new object[] { item.Venta.VentaId, item.Producto.Nombre, item.Producto.Unidad, item.Producto.Cantidad, item.Producto.Costo, item.Producto.Importe },
                (cell, col) =>
                {
                    if (col == 5 || col == 6) cell.Style.NumberFormat.Format = "$#,##0.00";
                },
                new double[] { 15, 35, 15, 10, 12, 13 },
                includeTotal: true,
                totalValue: historial.SelectMany(v => v.Productos).Sum(p => p.Importe)
            );

            // ===== Hoja Globos =====
            CrearHoja(workbook, "Globos", historial.SelectMany(v => v.Globos.Select(g => new { Venta = v, Globo = g })),
                new[] { "ID Venta", "Material", "Color", "Tamaño", "Forma", "Temática", "Unidad", "Cantidad", "Costo", "Importe" },
                item => new object[] { item.Venta.VentaId, item.Globo.Material, item.Globo.Color, item.Globo.Tamano, item.Globo.Forma, item.Globo.Tematica, item.Globo.Unidad, item.Globo.Cantidad, item.Globo.Costo, item.Globo.Importe },
                (cell, col) =>
                {
                    if (col == 9 || col == 10) cell.Style.NumberFormat.Format = "$#,##0.00";
                },
                new double[] { 10, 15, 10, 10, 10, 15, 10, 10, 10, 10 },
                includeTotal: true,
                totalValue: historial.SelectMany(v => v.Globos).Sum(g => g.Importe)
            );
        }

        private static void CrearHoja<T>(XLWorkbook workbook, string nombreHoja, IEnumerable<T> items, string[] headers,
                                 Func<T, object[]> mapFila, Action<IXLCell, int> styleCell = null,
                                 double[] anchosRelativos = null, bool includeTotal = false, decimal totalValue = 0)
        {
            var ws = workbook.Worksheets.Add(nombreHoja);

            // ===== Cabecera =====
            for (int i = 0; i < headers.Length; i++)
            {
                var cell = ws.Cell(1, i + 1);
                cell.Value = headers[i];
                cell.Style.Font.Bold = true;
                cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;

                if (anchosRelativos != null && i < anchosRelativos.Length)
                    ws.Column(i + 1).Width = anchosRelativos[i] * 1.5;
            }

            // ===== Filas =====
            int row = 2;
            bool alternate = false;
            foreach (var item in items)
            {
                var valores = mapFila(item);
                for (int col = 0; col < valores.Length; col++)
                {
                    var cell = ws.Cell(row, col + 1);
                    var valor = valores[col] ?? "";
                    if (valor is DateTime dt)
                        cell.SetValue(dt);
                    else if (valor is decimal dec)
                        cell.SetValue(dec);
                    else if (valor is double dbl)
                        cell.SetValue(dbl);
                    else
                        cell.SetValue(valor.ToString());

                    cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    styleCell?.Invoke(cell, col + 1);

                    if (alternate)
                        cell.Style.Fill.BackgroundColor = XLColor.LightGray;
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                }
                row++;
                alternate = !alternate;
            }

            // ===== Fila Total =====
            if (includeTotal)
            {
                int totalRow = row;
                ws.Cell(totalRow, 1).Value = "Total:";
                ws.Cell(totalRow, 1).Style.Font.Bold = true;
                ws.Range(totalRow, 1, totalRow, headers.Length - 1).Merge();
                ws.Cell(totalRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                ws.Cell(totalRow, headers.Length).SetValue(totalValue);
                ws.Cell(totalRow, headers.Length).Style.NumberFormat.Format = "$#,##0.00";
                ws.Cell(totalRow, headers.Length).Style.Font.Bold = true;
                ws.Range(totalRow, 1, totalRow, headers.Length).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }

            ws.Columns().AdjustToContents();
        }
    }
}