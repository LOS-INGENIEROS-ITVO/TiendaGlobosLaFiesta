using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using TiendaGlobosLaFiesta.Models;

namespace TiendaGlobosLaFiesta.Data
{
    public class StockManagerRepository
    {
        private readonly string _connectionString;

        public StockManagerRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        #region Ajuste de Stock

        // Versión sin conexión externa
        public bool AjustarStockProducto(string productoId, int nuevaCantidad, int empleadoId, string motivo)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var tran = conn.BeginTransaction();
            return AjustarStockProducto(productoId, nuevaCantidad, empleadoId, motivo, conn, tran);
        }

        public bool AjustarStockGlobo(string globoId, int nuevaCantidad, int empleadoId, string motivo)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var tran = conn.BeginTransaction();
            return AjustarStockGlobo(globoId, nuevaCantidad, empleadoId, motivo, conn, tran);
        }

        // Versión con conexión y transacción externa
        public bool AjustarStockProducto(string productoId, int nuevaCantidad, int empleadoId, string motivo, SqlConnection conn, SqlTransaction tran)
        {
            try
            {
                var cmdSelect = new SqlCommand("SELECT stock FROM Producto WHERE productoId = @ProductoId", conn, tran);
                cmdSelect.Parameters.AddWithValue("@ProductoId", productoId);
                int stockActual = Convert.ToInt32(cmdSelect.ExecuteScalar());

                var cmdUpdate = new SqlCommand(
                    "UPDATE Producto SET stock = @NuevaCantidad WHERE productoId = @ProductoId", conn, tran);
                cmdUpdate.Parameters.AddWithValue("@NuevaCantidad", nuevaCantidad);
                cmdUpdate.Parameters.AddWithValue("@ProductoId", productoId);
                cmdUpdate.ExecuteNonQuery();

                var cmdHist = new SqlCommand(
                    "INSERT INTO HistorialAjusteStock (ProductoId, CantidadAnterior, CantidadNueva, Motivo, EmpleadoId) " +
                    "VALUES (@ProductoId, @CantidadAnterior, @CantidadNueva, @Motivo, @EmpleadoId)", conn, tran);
                cmdHist.Parameters.AddWithValue("@ProductoId", productoId);
                cmdHist.Parameters.AddWithValue("@CantidadAnterior", stockActual);
                cmdHist.Parameters.AddWithValue("@CantidadNueva", nuevaCantidad);
                cmdHist.Parameters.AddWithValue("@Motivo", motivo);
                cmdHist.Parameters.AddWithValue("@EmpleadoId", empleadoId);
                cmdHist.ExecuteNonQuery();

                return true;
            }
            catch
            {
                tran.Rollback();
                return false;
            }
        }

        public bool AjustarStockGlobo(string globoId, int nuevaCantidad, int empleadoId, string motivo, SqlConnection conn, SqlTransaction tran)
        {
            try
            {
                var cmdSelect = new SqlCommand("SELECT stock FROM Globo WHERE globoId = @GloboId", conn, tran);
                cmdSelect.Parameters.AddWithValue("@GloboId", globoId);
                int stockActual = Convert.ToInt32(cmdSelect.ExecuteScalar());

                var cmdUpdate = new SqlCommand(
                    "UPDATE Globo SET stock = @NuevaCantidad WHERE globoId = @GloboId", conn, tran);
                cmdUpdate.Parameters.AddWithValue("@NuevaCantidad", nuevaCantidad);
                cmdUpdate.Parameters.AddWithValue("@GloboId", globoId);
                cmdUpdate.ExecuteNonQuery();

                var cmdHist = new SqlCommand(
                    "INSERT INTO HistorialAjusteStockGlobo (GloboId, CantidadAnterior, CantidadNueva, Motivo, EmpleadoId) " +
                    "VALUES (@GloboId, @CantidadAnterior, @CantidadNueva, @Motivo, @EmpleadoId)", conn, tran);
                cmdHist.Parameters.AddWithValue("@GloboId", globoId);
                cmdHist.Parameters.AddWithValue("@CantidadAnterior", stockActual);
                cmdHist.Parameters.AddWithValue("@CantidadNueva", nuevaCantidad);
                cmdHist.Parameters.AddWithValue("@Motivo", motivo);
                cmdHist.Parameters.AddWithValue("@EmpleadoId", empleadoId);
                cmdHist.ExecuteNonQuery();

                return true;
            }
            catch
            {
                tran.Rollback();
                return false;
            }
        }

        #endregion

        #region Consultas de Stock Crítico

        public List<StockCriticoItem> ObtenerProductosStockCritico(int limite = 10)
        {
            var lista = new List<StockCriticoItem>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var cmd = new SqlCommand("SELECT productoId, nombre, stock, costo, unidad FROM Producto WHERE stock <= @Limite AND Activo = 1", conn);
            cmd.Parameters.AddWithValue("@Limite", limite);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new StockCriticoItem
                {
                    Id = reader.GetString(0),
                    Nombre = reader.GetString(1),
                    StockActual = reader.GetInt32(2),
                    Precio = reader.GetDecimal(3),
                    Tipo = "Producto",
                    Unidad = reader.GetString(4)
                });
            }

            return lista;
        }

        public List<StockCriticoItem> ObtenerGlobosStockCritico(int limite = 10)
        {
            var lista = new List<StockCriticoItem>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var cmd = new SqlCommand("SELECT globoId, material, color, stock, costo, unidad FROM Globo WHERE stock <= @Limite AND Activo = 1", conn);
            cmd.Parameters.AddWithValue("@Limite", limite);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new StockCriticoItem
                {
                    Id = reader.GetString(0),
                    Nombre = $"{reader.GetString(1)} {reader.GetString(2)}",
                    StockActual = reader.GetInt32(3),
                    Precio = reader.GetDecimal(4),
                    Tipo = "Globo",
                    Unidad = reader.GetString(5)
                });
            }

            return lista;
        }

        #endregion
    }
}