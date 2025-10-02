using System.Data;
using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta.Data
{
    public class StockRepository
    {
        public List<StockCriticoItem> ObtenerProductosStockCritico()
        {
            string query = "SELECT productoId, nombre, stock FROM Producto WHERE stock <= 10 ORDER BY stock ASC";
            DataTable dt = DbHelper.ExecuteQuery(query);
            var lista = new List<StockCriticoItem>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new StockCriticoItem
                {
                    Id = row["productoId"].ToString(),
                    Nombre = row["nombre"].ToString(),
                    StockActual = Convert.ToInt32(row["stock"]),
                    Tipo = "Producto"
                });
            }
            return lista;
        }

        public List<StockCriticoItem> ObtenerGlobosStockCritico()
        {
            string query = "SELECT globoId, material + ' ' + color AS Nombre, stock FROM Globo WHERE stock <= 10 ORDER BY stock ASC";
            DataTable dt = DbHelper.ExecuteQuery(query);
            var lista = new List<StockCriticoItem>();
            foreach (DataRow row in dt.Rows)
            {
                lista.Add(new StockCriticoItem
                {
                    Id = row["globoId"].ToString(),
                    Nombre = row["Nombre"].ToString(),
                    StockActual = Convert.ToInt32(row["stock"]),
                    Tipo = "Globo"
                });
            }
            return lista;
        }
    }
}