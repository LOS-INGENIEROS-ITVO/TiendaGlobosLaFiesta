using System;
using System.Collections.Generic;
using System.Data;

namespace TiendaGlobosLaFiesta.Data
{
    public class StockRepository
    {
        public class ProductoStockCritico
        {
            public string Nombre { get; set; }
            public int Stock { get; set; }
        }

        // Productos con stock crítico (ejemplo: stock <= 5)
        public List<ProductoStockCritico> ObtenerProductosStockCritico()
        {
            string query = "SELECT nombre, stock FROM Globo WHERE stock <= 5 ORDER BY stock ASC";
            DataTable dt = DbHelper.ExecuteQuery(query);

            var lista = new List<ProductoStockCritico>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new ProductoStockCritico
                {
                    Nombre = row["nombre"].ToString(),
                    Stock = Convert.ToInt32(row["stock"])
                });
            }

            return lista;
        }
    }
}