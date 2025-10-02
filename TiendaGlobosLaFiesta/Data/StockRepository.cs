// TiendaGlobosLaFiesta\Data\StockRepository.cs
using System;
using System.Collections.Generic;
using System.Data;
using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta.Data
{
    public class StockRepository
    {
        public List<StockCriticoItem> ObtenerProductosStockCritico()
        {
            string query = "SELECT productoId, nombre, stock, unidad FROM Producto WHERE stock <= 10 AND Activo = 1 ORDER BY stock ASC";
            DataTable dt = DbHelper.ExecuteQuery(query);
            var lista = new List<StockCriticoItem>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new StockCriticoItem
                {
                    Id = row["productoId"].ToString(),
                    Nombre = row["nombre"].ToString(),
                    StockActual = Convert.ToInt32(row["stock"]),
                    Tipo = "Producto",
                    Unidad = row["unidad"] != DBNull.Value ? row["unidad"].ToString() : string.Empty,
                    Color = string.Empty
                });
            }
            return lista;
        }

        public List<StockCriticoItem> ObtenerGlobosStockCritico()
        {
            string query = "SELECT globoId, (material + ' ' + color) AS Nombre, stock, unidad, color FROM Globo WHERE stock <= 10 AND Activo = 1 ORDER BY stock ASC";
            DataTable dt = DbHelper.ExecuteQuery(query);
            var lista = new List<StockCriticoItem>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new StockCriticoItem
                {
                    Id = row["globoId"].ToString(),
                    Nombre = row["Nombre"].ToString(),
                    StockActual = Convert.ToInt32(row["stock"]),
                    Tipo = "Globo",
                    Unidad = row.Table.Columns.Contains("unidad") && row["unidad"] != DBNull.Value ? row["unidad"].ToString() : string.Empty,
                    Color = row.Table.Columns.Contains("color") && row["color"] != DBNull.Value ? row["color"].ToString() : string.Empty
                });
            }
            return lista;
        }
    }
}