using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta.Data
{
    public class StockManagerRepository
    {
        #region AJUSTE DE STOCK

        public void AjustarProductoStock(string productoId, int nuevaCantidad, string motivo, int empleadoId)
        {
            var repo = new ProductoRepository();
            var producto = repo.ObtenerProductoPorId(productoId);
            if (producto == null) throw new Exception("Producto no encontrado.");

            using var conn = DbHelper.ObtenerConexion();
            using var tran = conn.BeginTransaction();
            try
            {
                int cantidadAnterior = producto.Stock;

                string queryUpdate = "UPDATE Producto SET stock=@stock WHERE productoId=@productoId";
                using (var cmd = new SqlCommand(queryUpdate, conn, tran))
                {
                    cmd.Parameters.AddWithValue("@stock", nuevaCantidad);
                    cmd.Parameters.AddWithValue("@productoId", productoId);
                    cmd.ExecuteNonQuery();
                }

                string queryHist = @"
                    INSERT INTO HistorialAjusteStock 
                    (ProductoId, CantidadAnterior, CantidadNueva, Motivo, EmpleadoId) 
                    VALUES (@productoId, @anterior, @nueva, @motivo, @empleadoId)";
                using (var cmdHist = new SqlCommand(queryHist, conn, tran))
                {
                    cmdHist.Parameters.AddWithValue("@productoId", productoId);
                    cmdHist.Parameters.AddWithValue("@anterior", cantidadAnterior);
                    cmdHist.Parameters.AddWithValue("@nueva", nuevaCantidad);
                    cmdHist.Parameters.AddWithValue("@motivo", motivo);
                    cmdHist.Parameters.AddWithValue("@empleadoId", empleadoId);
                    cmdHist.ExecuteNonQuery();
                }

                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        public void AjustarGloboStock(string globoId, int nuevaCantidad, string motivo, int empleadoId)
        {
            var repo = new GloboRepository();
            var globo = repo.ObtenerGloboPorId(globoId);
            if (globo == null) throw new Exception("Globo no encontrado.");

            using var conn = DbHelper.ObtenerConexion();
            using var tran = conn.BeginTransaction();
            try
            {
                int cantidadAnterior = globo.Stock;

                string queryUpdate = "UPDATE Globo SET stock=@stock WHERE globoId=@globoId";
                using (var cmd = new SqlCommand(queryUpdate, conn, tran))
                {
                    cmd.Parameters.AddWithValue("@stock", nuevaCantidad);
                    cmd.Parameters.AddWithValue("@globoId", globoId);
                    cmd.ExecuteNonQuery();
                }

                string queryHist = @"
                    INSERT INTO HistorialAjusteStockGlobo
                    (GloboId, CantidadAnterior, CantidadNueva, Motivo, EmpleadoId)
                    VALUES (@globoId, @anterior, @nueva, @motivo, @empleadoId)";
                using (var cmdHist = new SqlCommand(queryHist, conn, tran))
                {
                    cmdHist.Parameters.AddWithValue("@globoId", globoId);
                    cmdHist.Parameters.AddWithValue("@anterior", cantidadAnterior);
                    cmdHist.Parameters.AddWithValue("@nueva", nuevaCantidad);
                    cmdHist.Parameters.AddWithValue("@motivo", motivo);
                    cmdHist.Parameters.AddWithValue("@empleadoId", empleadoId);
                    cmdHist.ExecuteNonQuery();
                }

                tran.Commit();
            }
            catch
            {
                tran.Rollback();
                throw;
            }
        }

        #endregion

        #region STOCK CRÍTICO

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

        #endregion
    }
}