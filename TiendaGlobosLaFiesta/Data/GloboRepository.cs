using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TiendaGlobosLaFiesta.Models;
using TiendaGlobosLaFiesta.Models.Inventario;

namespace TiendaGlobosLaFiesta.Data
{
    public class GloboRepository
    {
        private readonly string _connectionString;

        public GloboRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Obtener todos los globos activos
        public List<Globo> ObtenerGlobos()
        {
            var lista = new List<Globo>();
            string query = @"SELECT globoId, material, unidad, color, stock, costo, proveedorId, Activo 
                             FROM Globo 
                             WHERE Activo = 1";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new Globo
                        {
                            GloboId = reader["globoId"].ToString(),
                            Material = reader["material"].ToString(),
                            Unidad = reader["unidad"].ToString(),
                            Color = reader["color"].ToString(),
                            Stock = Convert.ToInt32(reader["stock"]),
                            Costo = Convert.ToDecimal(reader["costo"]),
                            ProveedorId = reader["proveedorId"]?.ToString(),
                            Activo = Convert.ToBoolean(reader["Activo"])
                        });
                    }
                }
            }
            return lista;
        }

        // Obtener un globo por su ID
        public Globo ObtenerGloboPorId(string globoId)
        {
            Globo globo = null;
            string query = @"SELECT globoId, material, unidad, color, stock, costo, proveedorId, Activo 
                             FROM Globo 
                             WHERE globoId = @globoId";

            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@globoId", globoId);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        globo = new Globo
                        {
                            GloboId = reader["globoId"].ToString(),
                            Material = reader["material"].ToString(),
                            Unidad = reader["unidad"].ToString(),
                            Color = reader["color"].ToString(),
                            Stock = Convert.ToInt32(reader["stock"]),
                            Costo = Convert.ToDecimal(reader["costo"]),
                            ProveedorId = reader["proveedorId"]?.ToString(),
                            Activo = Convert.ToBoolean(reader["Activo"])
                        };
                    }
                }
            }
            return globo;
        }

        // Insertar un nuevo globo
        public bool InsertarGlobo(Globo globo)
        {
            string query = @"INSERT INTO Globo (globoId, material, unidad, color, stock, costo, proveedorId, Activo)
                             VALUES (@globoId, @material, @unidad, @color, @stock, @costo, @proveedorId, @Activo)";
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@globoId", globo.GloboId);
                cmd.Parameters.AddWithValue("@material", globo.Material);
                cmd.Parameters.AddWithValue("@unidad", globo.Unidad);
                cmd.Parameters.AddWithValue("@color", globo.Color);
                cmd.Parameters.AddWithValue("@stock", globo.Stock);
                cmd.Parameters.AddWithValue("@costo", globo.Costo);
                cmd.Parameters.AddWithValue("@proveedorId", (object)globo.ProveedorId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Activo", globo.Activo);

                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Actualizar un globo existente
        public bool ActualizarGlobo(Globo globo)
        {
            string query = @"UPDATE Globo 
                             SET material=@material, unidad=@unidad, color=@color, stock=@stock, 
                                 costo=@costo, proveedorId=@proveedorId, Activo=@Activo
                             WHERE globoId=@globoId";
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@globoId", globo.GloboId);
                cmd.Parameters.AddWithValue("@material", globo.Material);
                cmd.Parameters.AddWithValue("@unidad", globo.Unidad);
                cmd.Parameters.AddWithValue("@color", globo.Color);
                cmd.Parameters.AddWithValue("@stock", globo.Stock);
                cmd.Parameters.AddWithValue("@costo", globo.Costo);
                cmd.Parameters.AddWithValue("@proveedorId", (object)globo.ProveedorId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Activo", globo.Activo);

                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Eliminar (desactivar) un globo
        public bool EliminarGlobo(string globoId)
        {
            string query = @"UPDATE Globo SET Activo = 0 WHERE globoId = @globoId";
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@globoId", globoId);
                conn.Open();
                return cmd.ExecuteNonQuery() > 0;
            }
        }

        // Obtener tamaños asociados a un globo
        public List<string> ObtenerTamaniosGlobo(string globoId)
        {
            var lista = new List<string>();
            string query = @"SELECT tamanio FROM Globo_Tamanio WHERE globoId=@globoId";
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@globoId", globoId);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        lista.Add(reader["tamanio"].ToString());
                }
            }
            return lista;
        }

        // Obtener formas asociadas a un globo
        public List<string> ObtenerFormasGlobo(string globoId)
        {
            var lista = new List<string>();
            string query = @"SELECT forma FROM Globo_Forma WHERE globoId=@globoId";
            using (SqlConnection conn = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@globoId", globoId);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        lista.Add(reader["forma"].ToString());
                }
            }
            return lista;
        }
    }
}