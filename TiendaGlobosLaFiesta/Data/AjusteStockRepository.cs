using System;
using System.Data.SqlClient;
using TiendaGlobosLaFiesta.Data;
using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta.Repositories
{
    public static class AjusteStockRepository
    {
        public static void AjustarProductoStock(string productoId, int nuevaCantidad, string motivo, int empleadoId)
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

        public static void AjustarGloboStock(string globoId, int nuevaCantidad, string motivo, int empleadoId)
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
    }
}