using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using TiendaGlobosLaFiesta.Models.Inventario;

namespace TiendaGlobosLaFiesta.Data
{
    public class StockManagerRepository
    {
        private readonly string _connectionString;

        public StockManagerRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // ===========================
        // Ajustar stock producto
        // ===========================
        public bool AjustarStockProducto(string productoId, int cantidad, string empleadoId, string motivo = null)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var tran = conn.BeginTransaction();

            try
            {
                // Obtener stock actual
                int stockAnterior;
                using (var cmd = new SqlCommand("SELECT stock FROM Producto WHERE productoId=@Id", conn, tran))
                {
                    cmd.Parameters.AddWithValue("@Id", productoId);
                    stockAnterior = (int)cmd.ExecuteScalar();
                }

                // Actualizar stock
                var query = "UPDATE Producto SET stock = stock + @Cantidad WHERE productoId = @Id";
                using (var cmd = new SqlCommand(query, conn, tran))
                {
                    cmd.Parameters.AddWithValue("@Cantidad", cantidad);
                    cmd.Parameters.AddWithValue("@Id", productoId);
                    cmd.ExecuteNonQuery();
                }

                // Insertar historial
                query = @"INSERT INTO HistorialAjusteStock (ProductoId, CantidadAnterior, CantidadNueva, Motivo, EmpleadoId)
                          VALUES (@Id, @Anterior, @Nueva, @Motivo, @EmpleadoId)";
                using (var cmd = new SqlCommand(query, conn, tran))
                {
                    cmd.Parameters.AddWithValue("@Id", productoId);
                    cmd.Parameters.AddWithValue("@Anterior", stockAnterior);
                    cmd.Parameters.AddWithValue("@Nueva", stockAnterior + cantidad);
                    cmd.Parameters.AddWithValue("@Motivo", (object)motivo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@EmpleadoId", empleadoId);
                    cmd.ExecuteNonQuery();
                }

                tran.Commit();
                return true;
            }
            catch
            {
                tran.Rollback();
                return false;
            }
        }

        // ===========================
        // Ajustar stock globo
        // ===========================
        public bool AjustarStockGlobo(string globoId, int cantidad, string empleadoId, string motivo = null)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();
            using var tran = conn.BeginTransaction();

            try
            {
                // Obtener stock actual
                int stockAnterior;
                using (var cmd = new SqlCommand("SELECT stock FROM Globo WHERE globoId=@Id", conn, tran))
                {
                    cmd.Parameters.AddWithValue("@Id", globoId);
                    stockAnterior = (int)cmd.ExecuteScalar();
                }

                // Actualizar stock
                var query = "UPDATE Globo SET stock = stock + @Cantidad WHERE globoId = @Id";
                using (var cmd = new SqlCommand(query, conn, tran))
                {
                    cmd.Parameters.AddWithValue("@Cantidad", cantidad);
                    cmd.Parameters.AddWithValue("@Id", globoId);
                    cmd.ExecuteNonQuery();
                }

                // Insertar historial
                query = @"INSERT INTO HistorialAjusteStockGlobo (GloboId, CantidadAnterior, CantidadNueva, Motivo, EmpleadoId)
                          VALUES (@Id, @Anterior, @Nueva, @Motivo, @EmpleadoId)";
                using (var cmd = new SqlCommand(query, conn, tran))
                {
                    cmd.Parameters.AddWithValue("@Id", globoId);
                    cmd.Parameters.AddWithValue("@Anterior", stockAnterior);
                    cmd.Parameters.AddWithValue("@Nueva", stockAnterior + cantidad);
                    cmd.Parameters.AddWithValue("@Motivo", (object)motivo ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@EmpleadoId", empleadoId);
                    cmd.ExecuteNonQuery();
                }

                tran.Commit();
                return true;
            }
            catch
            {
                tran.Rollback();
                return false;
            }
        }



        // Agrega estas sobrecargas en StockManagerRepository
        public bool AjustarStockProducto(string productoId, int cantidad, SqlConnection conn, SqlTransaction tran, string empleadoId, string motivo = null)
        {
            // Obtener stock actual
            int stockAnterior;
            using (var cmd = new SqlCommand("SELECT stock FROM Producto WHERE productoId=@Id", conn, tran))
            {
                cmd.Parameters.AddWithValue("@Id", productoId);
                stockAnterior = (int)cmd.ExecuteScalar();
            }

            // Actualizar stock
            using (var cmd = new SqlCommand("UPDATE Producto SET stock = stock + @Cantidad WHERE productoId = @Id", conn, tran))
            {
                cmd.Parameters.AddWithValue("@Cantidad", cantidad);
                cmd.Parameters.AddWithValue("@Id", productoId);
                cmd.ExecuteNonQuery();
            }

            // Insertar historial
            using (var cmd = new SqlCommand(
                @"INSERT INTO HistorialAjusteStock (ProductoId, CantidadAnterior, CantidadNueva, Motivo, EmpleadoId)
          VALUES (@Id, @Anterior, @Nueva, @Motivo, @EmpleadoId)", conn, tran))
            {
                cmd.Parameters.AddWithValue("@Id", productoId);
                cmd.Parameters.AddWithValue("@Anterior", stockAnterior);
                cmd.Parameters.AddWithValue("@Nueva", stockAnterior + cantidad);
                cmd.Parameters.AddWithValue("@Motivo", (object)motivo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@EmpleadoId", empleadoId);
                cmd.ExecuteNonQuery();
            }

            return true;
        }

        public bool AjustarStockGlobo(string globoId, int cantidad, SqlConnection conn, SqlTransaction tran, string empleadoId, string motivo = null)
        {
            int stockAnterior;
            using (var cmd = new SqlCommand("SELECT stock FROM Globo WHERE globoId=@Id", conn, tran))
            {
                cmd.Parameters.AddWithValue("@Id", globoId);
                stockAnterior = (int)cmd.ExecuteScalar();
            }

            using (var cmd = new SqlCommand("UPDATE Globo SET stock = stock + @Cantidad WHERE globoId = @Id", conn, tran))
            {
                cmd.Parameters.AddWithValue("@Cantidad", cantidad);
                cmd.Parameters.AddWithValue("@Id", globoId);
                cmd.ExecuteNonQuery();
            }

            using (var cmd = new SqlCommand(
                @"INSERT INTO HistorialAjusteStockGlobo (GloboId, CantidadAnterior, CantidadNueva, Motivo, EmpleadoId)
          VALUES (@Id, @Anterior, @Nueva, @Motivo, @EmpleadoId)", conn, tran))
            {
                cmd.Parameters.AddWithValue("@Id", globoId);
                cmd.Parameters.AddWithValue("@Anterior", stockAnterior);
                cmd.Parameters.AddWithValue("@Nueva", stockAnterior + cantidad);
                cmd.Parameters.AddWithValue("@Motivo", (object)motivo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@EmpleadoId", empleadoId);
                cmd.ExecuteNonQuery();
            }

            return true;
        }


        // ===========================
        // Obtener stock crítico
        // ===========================
        public List<StockCriticoItem> ObtenerStockCritico(int nivelCritico)
        {
            var lista = new List<StockCriticoItem>();

            // Productos
            using (var conn = new SqlConnection(_connectionString))
            {
                var query = "SELECT productoId, nombre, stock FROM Producto WHERE stock <= @Nivel";
                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Nivel", nivelCritico);
                conn.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lista.Add(new StockCriticoItem
                    {
                        Id = reader.GetString(0),
                        Nombre = reader.GetString(1),
                        StockActual = reader.GetInt32(2),
                        Tipo = "Producto"
                    });
                }
            }

            // Globos
            using (var conn = new SqlConnection(_connectionString))
            {
                var query = "SELECT globoId, material + ' ' + color AS nombre, stock FROM Globo WHERE stock <= @Nivel";
                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Nivel", nivelCritico);
                conn.Open();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    lista.Add(new StockCriticoItem
                    {
                        Id = reader.GetString(0),
                        Nombre = reader.GetString(1),
                        StockActual = reader.GetInt32(2),
                        Tipo = "Globo"
                    });
                }
            }

            return lista;
        }
    }
}
