using System;
using System.Collections.Generic;
using System.Data;
using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta.Data
{
    public class StockRepository
    {
        public class ProductoStockCritico
        {
            public string ProductoId { get; set; } // ⬅️ AÑADE ESTA LÍNEA
            public string Nombre { get; set; }
            public int Stock { get; set; }
        }

        public List<ProductoStockCritico> ObtenerProductosStockCritico()
        {
            string query = "SELECT productoId, nombre, stock FROM Producto WHERE stock <= 10 ORDER BY stock ASC";
            DataTable dt = DbHelper.ExecuteQuery(query);

            var lista = new List<ProductoStockCritico>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new ProductoStockCritico
                {
                    ProductoId = row["productoId"].ToString(), // ⬅️ AÑADE ESTA LÍNEA
                    Nombre = row["nombre"].ToString(),
                    Stock = Convert.ToInt32(row["stock"])
                });
            }
            return lista;
        }

        public List<ProductoStockCritico> ObtenerGlobosStockCritico()
        {
            string query = "SELECT material + ' ' + unidad AS Nombre, stock FROM Globo WHERE stock <= 5 ORDER BY stock ASC";
            DataTable dt = DbHelper.ExecuteQuery(query);

            var lista = new List<ProductoStockCritico>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new ProductoStockCritico
                {
                    Nombre = row["Nombre"].ToString(),
                    Stock = Convert.ToInt32(row["stock"])
                });
            }

            return lista;
        }
    }
}